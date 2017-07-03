using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace 创建线程
{
    class Program
    {
        static void Main(string[] args)
        {
            var thread = new Thread(PrintNumbers);
            thread.Start();
            Console.WriteLine("thread 正在运行      ");
            Console.ReadKey();
        }
        static void PrintNumbers()
        {
            Console.WriteLine("start... ");
            for (int i = 0; i < 10; i++)
            {
               
                Console.WriteLine(i);
            }
        }
    }
}
