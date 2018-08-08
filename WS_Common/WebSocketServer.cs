using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WS_Common
{
    public class WebSocketServer
    {
        const string webSocketKey = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
        /// <summary>
        /// Porta padrão
        /// </summary>
        public const int DefaultPort = 65500;
        public const int DefaultMaxPackageSize = 65535;

        private TcpListener listener = null;
        /// <summary>
        ///  Porta de conexão
        /// </summary>
        public int Port { get; private set; } = DefaultPort;
        public int MaxPackageSize { get; private set; } = DefaultMaxPackageSize;
        private bool ouvindo = false;

        public bool Start(int port = DefaultPort)
        {
            if (port < 1 || port > 65535)
                throw new WebSocketServerException("Port must be between 1 and 65535");

            Stop();

            Port = port;
            listener = new TcpListener(IPAddress.Parse("127.0.0.0"), Port);
            bool ok = false;
            try
            {
                listener.Start();
                ok = true;
            }
            catch (Exception e)
            {
                DoLog("listener.Start exception: " + e.Message, MessageType.Exception);
            }

            if (!ok)
            {
                listener = null;
                return false;
            }
            ouvindo = true;        
            while (ouvindo)
            {
                if (!listener.Pending())
                {
                    Thread.Sleep(100);
                    continue;
                }
                TcpClient client = listener.AcceptTcpClient();
                DoLog("client connected", MessageType.ClientConnected);

                while (client.Connected)
                {
                    if (client.Available == 0)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    NetworkStream stream = client.GetStream();

                    Byte[] bytes = new Byte[client.Available];

                    stream.Read(bytes, 0, bytes.Length);

                    //translate bytes of request to string
                    String data = Encoding.UTF8.GetString(bytes);

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
                                        new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + webSocketKey
                                    )
                                )
                            ) + Environment.NewLine
                            + Environment.NewLine);

                        stream.Write(response, 0, response.Length);
                        DoLog("Handshake", MessageType.HandShake);
                    }
                    else
                    {
                        if (bytes.Length > 6)
                        {
                            string deco = WSHelpers.DecodedData(bytes, MaxPackageSize);
                            if (!DoDataReceived(deco))
                            {
                                client.Close();
                                DoLog("client.close", MessageType.ClientClose);
                            }
                        }
                    }

                }
            }


        }

        /// <summary>
        /// Evento disparado quando um dado é recebido do cliente
        /// </summary>
        /// <param name="data"></param>
        /// <returns>false, se deve desconectar o cliente</returns>
        private bool DoDataReceived(string data)
        {
            throw new NotImplementedException();
        }

        private void DoLog(string message, MessageType exception)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            if (listener == null)
                return;

            listener.Stop();
            listener = null;
        }

    }

    public enum MessageType
    {
        Exception,
        ClientConnected,
        HandShake,
        ClientClose
    }
}
