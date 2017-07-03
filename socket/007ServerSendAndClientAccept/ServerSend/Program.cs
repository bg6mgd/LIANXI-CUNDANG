using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
namespace ServerSend
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

            //转换成大写并发送
            msg = msg.ToUpper();
            buffer = Encoding.Unicode.GetBytes(msg);
            lock (streamToClient) {
                streamToClient.Write(buffer, 0, buffer.Length);
            }
            Console.WriteLine("Send:{0}", msg);

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
