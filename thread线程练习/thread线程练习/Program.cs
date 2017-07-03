using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace thread线程练习
{
    class Program
    {
        static void Main(string[] args)
        {
            var a = new Data { message = "abcdefj " };
            var b = new Thread(Threadwithpara);
            b.Start(a);
            Console.Read();
            
        }

       

        static void Threadwithpara(object o)
        {
            Data d = (Data)o;
            Console.WriteLine("running in thread,reived{0}", d.message);
        }

        public struct Data
        {
            public string message;
        }
        
    }
}
