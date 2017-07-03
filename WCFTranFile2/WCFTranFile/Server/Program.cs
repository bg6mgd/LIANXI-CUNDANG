using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.ServiceModel;
using System.ServiceModel.Description;
using Contract;
using SocketServiceContract;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
namespace Server
{
    static class Program
    {

        static void Main()
        {
            string IP = getIP();
            Listening(IP);

            string httpurl = "http://" + IP + ":8888/HelloWCFService";
            string tcpUrl = "net.tcp://" + IP + ":9999/HelloWCFService";

            ServiceHost currentHost = GetServiceHost(httpurl, tcpUrl, IP);
            currentHost.Open();
            Console.ReadLine();
            currentHost.Close();
        }
        static ServerListen listen = null;
        private static void Listening(string ip)
        {
            listen = new ServerListen();

            listen.Start(ip, 7777);
        }
        public static string getIP() //获取IP
        {
            IPAddress[] arrIPAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in arrIPAddresses)
            {
                if (ip.AddressFamily.Equals(AddressFamily.InterNetwork))
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        private static ServiceHost GetServiceHost(string httpurl, string tcpUrl, string ip)
        {

            ServiceHost currentHost = null;
            Uri baseHttpUri = new Uri(httpurl);
            Uri baseTcpUri = new Uri(tcpUrl);
            currentHost = new ServiceHost(typeof(ServerContract), baseTcpUri);
            NetTcpBinding tcpBinding = new NetTcpBinding();
            tcpBinding.Security.Mode = SecurityMode.None;
            tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            tcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
            tcpBinding.ReliableSession.Enabled = true;
            currentHost.AddServiceEndpoint(typeof(IContract), tcpBinding, baseTcpUri);
            ServiceThrottlingBehavior ServicebeHavior = currentHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (ServicebeHavior == null)
            {
                ServicebeHavior = new ServiceThrottlingBehavior();
                currentHost.Description.Behaviors.Add(ServicebeHavior);
            }

            ServicebeHavior.MaxConcurrentSessions = 1000;

            ServiceMetadataBehavior behavior = currentHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (behavior == null)
            {
                behavior = new ServiceMetadataBehavior();
                currentHost.Description.Behaviors.Add(behavior);
            }

            behavior.HttpGetUrl = baseHttpUri;
            behavior.HttpGetEnabled = true;
            ServiceDebugBehavior debugBehavior = currentHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            debugBehavior.IncludeExceptionDetailInFaults = true;
            debugBehavior.HttpHelpPageEnabled = true;
            debugBehavior.HttpHelpPageUrl = new Uri("http://" + ip + ":8880/Help");

            currentHost.Opened += delegate
            {
                foreach (Uri url in currentHost.BaseAddresses)
                {
                    Console.WriteLine("WCF监听地址:{0}", url.ToString());
                }

            };
            return currentHost;
        }
    }
}
