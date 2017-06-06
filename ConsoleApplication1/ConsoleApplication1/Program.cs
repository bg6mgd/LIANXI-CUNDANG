using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Queue myQ = new Queue();
            myQ.Enqueue("The");//入队   
            myQ.Enqueue("quick");
            myQ.Enqueue("brown");
            myQ.Enqueue("fox");
            myQ.Enqueue(null);//添加null   
            myQ.Enqueue("fox");//添加重复的元素   

            // 打印队列的数量和值   
            Console.WriteLine("myQ");
            Console.WriteLine("\tCount:    {0}", myQ.Count);

            // 打印队列中的所有值   
            Console.Write("Queue values:");
            PrintValues(myQ);

            // 打印队列中的第一个元素，并移除   
            Console.WriteLine("(Dequeue)\t{0}", myQ.Dequeue());

            // 打印队列中的所有值   
            Console.Write("Queue values:");
            PrintValues(myQ);

            // 打印队列中的第一个元素，并移除   
            Console.WriteLine("(Dequeue)\t{0}", myQ.Dequeue());

            // 打印队列中的所有值   
            Console.Write("Queue values:");
            PrintValues(myQ);

            // 打印队列中的第一个元素   
            Console.WriteLine("(Peek)   \t{0}", myQ.Peek());

            // 打印队列中的所有值   
            Console.Write("Queue values:");
            PrintValues(myQ);

            Console.ReadLine();

        }
        public static void PrintValues(IEnumerable myCollection)
        {
            foreach (Object obj in myCollection)
                Console.Write("    {0}", obj);
            Console.WriteLine();
        }
    }
    

}
