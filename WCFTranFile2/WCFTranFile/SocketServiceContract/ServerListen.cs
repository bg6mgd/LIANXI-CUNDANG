using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace SocketServiceContract
{
    public class ServerListen : IDisposable
    {

        private Socket ServerSocket = null;
        /// <summary>
        /// 启动监听
        /// </summary>
        /// <param name="IP">监听IP</param>
        /// <param name="Port">监听端口</param>
        public void Start(string IP, int Port)
        {
            Thread th = new Thread((ThreadStart)delegate()
                {
                    ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint local = new IPEndPoint(IPAddress.Parse(IP), Port);
                    ServerSocket.Bind(local);
                    ServerSocket.Listen(100);
                    while (true)
                    {
                        Socket Client = ServerSocket.Accept();
                        ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), Client);
                    }
                    //ServerSocket.BeginAccept(new AsyncCallback(DoAcceptSocketCallback), ServerSocket);
                });
            th.Start();

        }
        /// <summary>
        /// 停止监听程序
        /// </summary>
        public void Stop()
        {
            if (ServerSocket != null)
            {
                ServerSocket.Close();
                ServerSocket = null;
            }
        }

        private void DoAcceptSocketCallback(IAsyncResult ar)
        {
            Socket Listen = (Socket)ar.AsyncState;
            Socket Client = Listen.EndAccept(ar);
            ThreadPool.QueueUserWorkItem(new WaitCallback(ProcessClient), Client);
            SynchroSocketServerProcess process = new SynchroSocketServerProcess(Client);
            Listen.BeginAccept(new AsyncCallback(DoAcceptSocketCallback), Listen);

        }
        void ProcessClient(object Client)
        {
            Socket ConnectionClient = Client as Socket;
            byte[] byt = new byte[1024];
            ConnectionClient.Receive(byt);
            string Mess = Encoding.ASCII.GetString(byt).Trim().Trim((char)'\0');
            if (Mess.Equals("Asynchronous", StringComparison.InvariantCultureIgnoreCase))
            {
                AsynchronousSocketServerProcess process = new AsynchronousSocketServerProcess(ConnectionClient);
            }
            else if (Mess.Equals("Synchro", StringComparison.InvariantCultureIgnoreCase))
            {
                SynchroSocketServerProcess process = new SynchroSocketServerProcess(ConnectionClient);
            }
            else
            {
                ConnectionClient.Close();
            }

        }

        private void SendCallback(IAsyncResult ar)
        {

            SendObject SendState = (SendObject)ar.AsyncState;
            Socket Client = SendState.Client;
            int SendCount = Client.EndSendTo(ar);
            MessBuffer SendBuffer = new MessBuffer(SendState.SendBuf, Client);
            OnSend(SendBuffer);

        }

        /// <summary>
        /// 数据下发完成
        /// </summary>
        public event MessBufferHandle Send;
        protected void OnSend(MessBuffer ReviceBuffer)
        {
            if (Send != null)
            {
                Send(this, ReviceBuffer);
            }
        }



        #region IDisposable 成员

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
    public class StateObject
    {
        public static int BufferSize = 20480;
        public byte[] RevaceBuf;
        public System.Net.Sockets.Socket Client;
        public SocketError Error;
        public StateObject()
        {
            RevaceBuf = new byte[StateObject.BufferSize];
        }
    }
    public class SendObject
    {
        public byte[] SendBuf;
        public System.Net.Sockets.Socket Client;
        public SendObject(byte[] SendBuff, System.Net.Sockets.Socket ConnSocket)
        {
            SendBuf = SendBuff;
            Client = ConnSocket;
        }
    }

}

