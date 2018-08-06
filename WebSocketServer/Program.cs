using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using WS_Common;

namespace WebSocketServer
{
    class Program
    {
        const string x = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

    

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
                if (client.Available == 0)
                {
                    Thread.Sleep(1000);
                    continue;
                }
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
                        string deco = WSHelpers.DecodedData(bytes, 256);

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
                            case "grande":
                                enviar = "";
                                for (int i = 0; i < 64; i++)
                                    enviar += "0123456789ABCDEF";
                                break;
                        }

                    }


                }
                if (!string.IsNullOrEmpty(enviar))
                {
                    byte[] be = WSHelpers.EncodedData(enviar);
                    stream.Write(be, 0, be.Length);
                    enviar = "";
                }
            }

            Console.ReadLine();
        }
    }
}
