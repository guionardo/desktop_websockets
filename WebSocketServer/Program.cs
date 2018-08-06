using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebSocketServer
{
    class Program
    {
        const string x = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        public static byte[] SetEncodedData(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return new byte[0];
            byte[] btexto = Encoding.UTF8.GetBytes(texto);
            if (btexto.Length > 125)
                return new byte[0];
            byte[] ret = new byte[btexto.Length+2];
            
            ret[0] = 129;   // String com menos de 126 caracteres, sem mapa
            ret[1] = (byte)btexto.Length;
            for (int i = 0; i < btexto.Length; i++)
                ret[i + 2] = btexto[i];

            return ret;
        }

        public static string GetDecodedData(byte[] buffer, int length)
        {
            byte b = buffer[1];
            int dataLength = 0;
            int totalLength = 0;
            int keyIndex = 0;

            if (b - 128 <= 125)
            {
                dataLength = b - 128;
                keyIndex = 2;
                totalLength = dataLength + 6;
            }

            if (b - 128 == 126)
            {
                dataLength = BitConverter.ToInt16(new byte[] { buffer[3], buffer[2] }, 0);
                keyIndex = 4;
                totalLength = dataLength + 8;
            }

            if (b - 128 == 127)
            {
                dataLength = (int)BitConverter.ToInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                keyIndex = 10;
                totalLength = dataLength + 14;
            }

            if (totalLength > length)
                throw new Exception("The buffer length is small than the data length");

            byte[] key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };

            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }

            return Encoding.UTF8.GetString(buffer, dataIndex, dataLength);
        }

        static void Main(string[] args)
        {
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 80);

            server.Start();
            Console.WriteLine("Server has started on 127.0.0.1:80.{0}Waiting for a connection...", Environment.NewLine);
            bool continuar = true;
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("A client connected.");
            string enviar = "";
            while (continuar)
            {



                NetworkStream stream = client.GetStream();

                Byte[] bytes = new Byte[client.Available];

                stream.Read(bytes, 0, bytes.Length);

                //translate bytes of request to string
                String data = Encoding.UTF8.GetString(bytes);
                Console.WriteLine("data======\n" + data + "\n=====\n");

                if (new Regex("^GET").IsMatch(data))
                {

                    /*
                     * Este código tem um defeito. 
                     * Digamos que a propriedade client.Available retorna o valor 2 porque somente a 
                     * requisição GET está disponível até agora. 
                     * a expressão regular iria falhar mesmo que os dados recebidos sejam perfeitamente válidos.
                     */

                    Byte[] response = Encoding.UTF8.GetBytes("HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                        + "Connection: Upgrade" + Environment.NewLine
                        + "Upgrade: websocket" + Environment.NewLine
                        + "Sec-WebSocket-Accept: " + Convert.ToBase64String(
                            SHA1.Create().ComputeHash(
                                Encoding.UTF8.GetBytes(
                                    new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11"
                                )
                            )
                        ) + Environment.NewLine
                        + Environment.NewLine);

                    stream.Write(response, 0, response.Length);
                    Console.WriteLine(Encoding.UTF8.GetString(response));
                }
                else
                {
                    Console.WriteLine("=== BYTES ===");
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        Console.Write($"#{i.ToString("000")} {bytes[i].ToString("x2")}  " + (((i + 1) % 8 == 0) ? "\n" : ""));
                    }
                    Console.WriteLine("\n=== BYTES ===");


                    if (bytes.Length > 6)
                    {
                        string deco = GetDecodedData(bytes, 256);

                        // Se o segundo byte menos 128 estiver entre 0 e 125, este é o tamanho da mensagem. 
                        // Se for 126, os 2 bytes seguintes (16-bit inteiro sem sinal) 
                        // e se 127, os 8 bytes seguintes (64-bit inteiro sem sinal) são o comprimento.
                        /*                        int tamanho, prox;
                                                if (bytes[1] - 128 <= 125)
                                                {
                                                    tamanho = bytes[1]-125;
                                                    prox = 2;
                                                }
                                                else
                                                if (bytes[1] - 128 == 126)
                                                {
                                                    tamanho = bytes[2] * 256 + bytes[3];
                                                    prox = 4;
                                                }
                                                else
                                                {
                                                    tamanho = bytes[2] * 256 * 256 * 256 + bytes[3] * 256 * 256 + bytes[4] * 256 + bytes[5];
                                                    prox = 6;
                                                }

                                                byte[] key = new byte[4]{ bytes[prox], bytes[prox + 1], bytes[prox + 2], bytes[prox + 3]};
                                                prox += 4;
                                                byte[] decoded = new byte[tamanho];
                                                for(int i=0;i<tamanho;i++)
                                                    decoded[i]= (Byte)(bytes[i+prox] ^ key[i % 4]);
                                                    */
                        Console.WriteLine("Decoded=" + deco);
                        switch (deco)
                        {
                            case "quit":
                                continuar = false;
                                enviar = "FECHANDO";

                                break;
                            case "ola":
                                enviar = "Olá, Guionardo";
                                break;

                        }

                    }


                }
                if (!string.IsNullOrEmpty(enviar))
                {
                    byte[] be = SetEncodedData(enviar);
                    stream.Write(be, 0, be.Length);
                    enviar = "";
                }
            }

            Console.ReadLine();
        }
    }
}
