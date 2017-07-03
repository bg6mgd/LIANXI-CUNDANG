using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
namespace ServerSQL
{
    class Program
    {
        static void Main(string[] args)
        {
            const int BufferSize = 8192;//缓存大小，8192字节，可以保存4096个汉字和英文字符

            Console.WriteLine("Server is running ...");
            IPAddress ip = IPAddress.Parse("127.0.0.1");//获取ip地址
            TcpListener listener = new TcpListener(ip, 8500);

            listener.Start();//开始监听
            Console.WriteLine("Start Listening...");

            //获取一个连接，中断方法
            TcpClient remoteClient = listener.AcceptTcpClient();
            //打印连接到的客户端信息
            Console.WriteLine("Client Connected! {0}<--{1}", remoteClient.Client.LocalEndPoint, remoteClient.Client.RemoteEndPoint);

            //获取流，并写入buffer中
            NetworkStream streamToClient = remoteClient.GetStream();
            byte[] buffer = new byte[BufferSize];
            int bytesRead = streamToClient.Read(buffer, 0, BufferSize);//一直等待客户端传信息
            Console.WriteLine("Reading data,{0} bytes...", bytesRead);

            //获得请求的字符串
            string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Received:{0}", msg);

            //发送信息
            ConsoleKey key;
            Console.WriteLine("Menu:S-Send,X-Exit");
            do
            {
                key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.S)
                {
                    //获取输入的字符串
                    Console.Write("输入信息：");
                    msg = Console.ReadLine();

                    //发送信息
                    buffer = Encoding.Unicode.GetBytes(msg);//获得缓存
                    streamToClient.Write(buffer, 0, buffer.Length);//发往服务器
                    Console.WriteLine("Sent:{0}", msg);

                    //获取流，并写入buffer中
                    streamToClient = remoteClient.GetStream();
                    buffer = new byte[BufferSize];
                    bytesRead = streamToClient.Read(buffer, 0, BufferSize);//一直等待客户端传信息
                    Console.WriteLine("Reading data,{0} bytes...", bytesRead);

                    //获得请求的字符串
                    msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Received:{0}", msg);
                }
            } while (key != ConsoleKey.X);

            //按Q退出
            Console.WriteLine("\n\n 输入 \"Q\"键退出。");
            do
            {
                key = Console.ReadKey(true).Key;
            } while (key != ConsoleKey.Q);
        }
    }
}
