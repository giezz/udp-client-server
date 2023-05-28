using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace client
{
    internal class Client
    {
        private const int port = 8080;
        public static void Main(string[] args)
        {
            UdpClient udpClient = new UdpClient();
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
            Console.WriteLine("UDP-клиент запущен. Ожидание сообщений от сервера...");

            try
            {
                while (true)
                {
                    IPEndPoint serverEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);;
                    
                    // receive file name
                    byte[] data = udpClient.Receive(ref serverEP);
                    string fileName = Encoding.UTF8.GetString(data);
                    
                    // receive file size
                    byte[] fileSizeBytes = udpClient.Receive(ref serverEP);
                    long fileSize = BitConverter.ToInt64(fileSizeBytes, 0);
                    
                    // receive file itself
                    using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                    {
                        int bytesRead;
                        long bytesReceived = 0;

                        while (bytesReceived < fileSize)
                        {
                            IPEndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);
                            byte[] receivedData = udpClient.Receive(ref senderEndPoint);
                            bytesRead = receivedData.Length;
                            
                            fileStream.Write(receivedData, 0, bytesRead);

                            bytesReceived += bytesRead;
                        }
                        
                        Console.WriteLine("Файл успешно принят и сохранен.");
                        Process.Start(fileStream.Name);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }
    }
}