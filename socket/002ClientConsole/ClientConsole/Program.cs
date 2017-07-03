using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
namespace ClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client Running...");
            TcpClient client = new TcpClient();
            try
            {
                client.Connect("localhost", 8500);//与服务器连接
            }
            catch (Exception ex){
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                return;
            }
            //打印连接到的服务器信息
            Console.WriteLine("Server Connected! {0}-->{1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);

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
