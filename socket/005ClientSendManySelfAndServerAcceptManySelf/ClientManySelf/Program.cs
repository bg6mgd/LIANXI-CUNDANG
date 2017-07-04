﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
namespace ClientManySelf
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client Running...");
            TcpClient client;
            //启动多个服务器
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

                NetworkStream streamToServer = client.GetStream();

                ConsoleKey key;
                Console.WriteLine("Menu:S-Send,X-Exit");
                do
                {
                    key = Console.ReadKey(true).Key;
                    if (key == ConsoleKey.S)
                    {
                        //获取输入的字符串
                        Console.Write("输入信息：");
                        string msg = Console.ReadLine();

                        //发送信息
                        byte[] buffer = Encoding.Unicode.GetBytes(msg);//获得缓存
                        streamToServer.Write(buffer, 0, buffer.Length);//发往服务器
                        Console.WriteLine("Sent:{0}", msg);
                    }
                } while (key != ConsoleKey.X);
            }

        }
    }
}