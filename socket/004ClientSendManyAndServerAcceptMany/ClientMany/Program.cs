using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
namespace ClientMany
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client Running...");
            TcpClient client;
            //启动多个客户端
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect("localhost", 8500);//与服务器连接
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }

                //打印连接到的服务器信息
                Console.WriteLine("Server Connected! {0}-->{1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);

                //定义要传送的信息
                string msg = "\"欢迎连接到服务器！\"";
                NetworkStream streamToServer = client.GetStream();

                //发送信息
                byte[] buffer = Encoding.Unicode.GetBytes(msg);//获得缓存
                streamToServer.Write(buffer, 0, buffer.Length);//发往服务器
                Console.WriteLine("Sent:{0}", msg);
            }
            //按Q退出
            Console.WriteLine("\n\n 输入 \"Q\"键退出。");
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            } while (key != ConsoleKey.Q);
        }
    }
}
