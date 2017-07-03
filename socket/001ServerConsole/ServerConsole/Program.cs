using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;

namespace ServerConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server is running...");
            IPAddress ip = IPAddress.Parse("127.0.0.1");//获取ip地址
            TcpListener listener = new TcpListener(ip, 8500);

            listener.Start();//开始监听
            Console.WriteLine("Start Listening...");

            Console.WriteLine("\n\n 输入 \"Q\"键退出。");
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            } while (key != ConsoleKey.Q);
        }
    }
}
