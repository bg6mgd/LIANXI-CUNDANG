using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCFclient
{
    class Program
    {
        static void Main(string[] args)
        {
            myservice1.HelloWCFServiceClient client = new myservice1.HelloWCFServiceClient();
            string srtRet = client.HelloWCF();
            Console.WriteLine(srtRet);
            Console.ReadLine();
            client.Close();
        }
    }
}
