using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace SocketServiceContract
{
    public class SynchroSocketClient : IDisposable
    {
        private Socket ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Stop()
        {
            if (ClientSocket != null)
            {

                ClientSocket.Disconnect(false);
            }
        }


        public void Connect(string ConnectIP, int Port)
        {

            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSocket.Connect(ConnectIP, Port);
            SendComand("Synchro");
            StateObject so = new StateObject();
            so.Client = ClientSocket;

            ClientSocket.BeginReceive(so.RevaceBuf, 0, StateObject.BufferSize, SocketFlags.None, out so.Error, new AsyncCallback(DoReadBuff), so);

        }
        static object obj = new object();
        private void DoReadBuff(IAsyncResult ar)
        {
            lock (obj)
            {
                try
                {
                    StateObject so = (StateObject)ar.AsyncState;
                    int ReadSize = ClientSocket.EndReceive(ar);
                    if (ReadSize > 0)
                    {
                        ReviceBuffer Buffer = new ReviceBuffer(so.RevaceBuf, ClientSocket, so.Error);
                        try
                        {
                            OnRevice(Buffer);
                            SendComand("Success");
                        }
                        catch
                        {
                            SendComand("Error");
                        }
                    }

                    StateObject reviceBuff = new StateObject();
                    ClientSocket.BeginReceive(reviceBuff.RevaceBuf, 0, StateObject.BufferSize, SocketFlags.None, out reviceBuff.Error, new AsyncCallback(DoReadBuff), reviceBuff);

                }
                catch (SocketException)
                {
                    ClientSocket.Close();
                }
            }

        }

        public void SendComand(string cmd)
        {
            byte[] byt = Encoding.ASCII.GetBytes(cmd);
            SendMess(byt);
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="Client">客户端IP</param>
        /// <param name="SendBuffer">发送数据</param>
        public bool SendMess(byte[] SendBuffer)
        {

            SendObject SendBuf = new SendObject(SendBuffer, ClientSocket);
            ClientSocket.BeginSend(SendBuf.SendBuf, 0, SendBuf.SendBuf.Length, SocketFlags.None, new AsyncCallback(SendCallback), SendBuf);
            return true;

        }
        public void SendMess(string Mess)
        {
            byte[] byt = System.Text.Encoding.UTF8.GetBytes(Mess);
            SendObject SendBuf = new SendObject(byt, ClientSocket);
            ClientSocket.Send(SendBuf.SendBuf, 0, SendBuf.SendBuf.Length, SocketFlags.None);

        }
        private void SendCallback(IAsyncResult ar)
        {
            SendObject SendState = (SendObject)ar.AsyncState;
            int SendCount = ClientSocket.EndSendTo(ar);
            MessBuffer SendBuffer = new MessBuffer(SendState.SendBuf, ClientSocket);
            OnSend(SendBuffer);
        }




        /// <summary>
        /// 数据接收完成
        /// </summary>
        public ReviceBufferHandle Revice;
        protected void OnRevice(ReviceBuffer ReviceBuffer)
        {
            if (Revice != null)
            {
                Revice.Invoke(this, ReviceBuffer);
            }
        }
        /// <summary>
        /// 数据下发完成
        /// </summary>
        public MessBufferHandle Send;
        protected void OnSend(MessBuffer ReviceBuffer)
        {
            if (Send != null)
            {
                Send.Invoke(this, ReviceBuffer);
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            Stop();
        }

        #endregion
    }
}
