using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server;

class Server
{
    private static string[] files = new[] {"file_doc.txt", "file_img.jpg", "file_web.html"};
    private const int port = 8080;

    public static void Main(string[] args)
    {
        UdpClient udpServer = new UdpClient();
        udpServer.EnableBroadcast = true;

        try
        {
            while (true)
            {
                string command = Console.ReadLine();

                if (command == "/sr")
                {
                    SendRandomFile(udpServer);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Ошибка: " + e.Message);
        }
        finally
        {
            udpServer.Close();
        }
    }

    public static void SendRandomFile(UdpClient server)
    {
        Random random = new Random();
        string fileName = files[random.Next(3)];
        
        // send file name
        byte[] data = Encoding.UTF8.GetBytes(fileName);
        server.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, port));

        // send file size
        FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        long fileSize = fileStream.Length;
        byte[] fileSizeBytes = BitConverter.GetBytes(fileSize);
        server.Send(fileSizeBytes, fileSizeBytes.Length, new IPEndPoint(IPAddress.Broadcast, port));

        // send file itself
        int bufferSize = 1024;
        byte[] buffer = new byte[bufferSize];
        int bytesRead;
        while ((bytesRead = fileStream.Read(buffer, 0, bufferSize)) > 0)
        {
            server.Send(buffer, bytesRead, new IPEndPoint(IPAddress.Broadcast, port));
        }
        
        fileStream.Close();
        Console.WriteLine("Файл отправлено клиентам.");
    }
}