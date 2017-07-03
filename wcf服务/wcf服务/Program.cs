using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Description;
namespace wcf服务
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri baseAddress = new Uri("http://localhost:8000/MyService");
            ServiceHost host = new ServiceHost(typeof(HelloWCFService), baseAddress);
            host.AddServiceEndpoint(typeof(IHelloWCFService),new WSHttpBinding(),
                "HelloWCFService");
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            smb.HttpGetEnabled = true;
            host.Description.Behaviors.Add(smb);
            host.Open();
            Console.WriteLine("Service is Ready     ");
            Console.WriteLine("Press Any Key to terminated ..." );
            Console.ReadLine();
            host.Close();
        }
    }
    [ServiceContract]
    interface IHelloWCFService
    {
        [OperationContract]
        string HelloWCF();

    }
    public class HelloWCFService: IHelloWCFService
    {
        public string HelloWCF()
        {
            return "hello WCF!";
        }
    }

}
