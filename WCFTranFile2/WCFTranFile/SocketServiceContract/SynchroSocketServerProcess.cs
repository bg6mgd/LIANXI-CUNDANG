using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace SocketServiceContract
{
    class SynchroSocketServerProcess
    {
        Socket client = null;
        public SynchroSocketServerProcess(Socket client)
        {
            this.client = client;
            StateObject so = new StateObject();
            so.Client = client;
            resultIAsync = client.BeginReceive(so.RevaceBuf, 0, StateObject.BufferSize, SocketFlags.None, out so.Error, new AsyncCallback(DoReadBuff), so);

        }
        IAsyncResult resultIAsync = null;
        public void SendFile(string FileName, Socket client)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            SocketFileInfo socketFile = new SocketFileInfo(client, fs, FileName);
            socketFile.FileLength = fs.Length;
            fs.BeginRead(socketFile.CurrentByte, 0, socketFile.MaxLength, (AsyncCallback)EndReadStream, socketFile);
        }
        SocketFileInfo socketSendFile = null;
        void EndReadStream(IAsyncResult ra)
        {
            socketSendFile = (SocketFileInfo)ra.AsyncState;
            int currentLen = socketSendFile.FileStream.EndRead(ra);
            if (currentLen != 0)
            {
                socketSendFile.CurrentLen = currentLen;
                socketSendFile.CurrentOffset = socketSendFile.FileStream.Position - currentLen;

                byte[] sendBuff = SocketFileInfo.GetSendByte(socketSendFile);
                ars.Reset();
                socketSendFile.Client.BeginSend(
                     sendBuff,
                     0,
                     sendBuff.Length,
                     SocketFlags.None, null, null);
                ars.WaitOne();
                socketSendFile.FileStream.BeginRead(socketSendFile.CurrentByte, 0, socketSendFile.MaxLength, (AsyncCallback)EndReadStream, socketSendFile);


            }
            else
            {
                socketSendFile.FileStream.Close();
                socketSendFile.FileStream.Dispose();
                socketSendFile.Client.EndReceive(resultIAsync);
                socketSendFile.Client.Close();
            }
        }
        private void DoReadBuff(IAsyncResult ar)
        {
            StateObject so = (StateObject)ar.AsyncState;
            Socket Client = so.Client;
            try
            {
                int ReadSize = Client.EndReceive(ar);
                if (ReadSize > 0)
                {
                    ReviceBuffer Buffer = new ReviceBuffer(so.RevaceBuf, Client, so.Error);
                    ProcessMess(Buffer);
                }
                so.RevaceBuf = null;
                so.RevaceBuf = new byte[StateObject.BufferSize];
                resultIAsync = Client.BeginReceive(so.RevaceBuf, 0, StateObject.BufferSize, SocketFlags.None, out so.Error, new AsyncCallback(DoReadBuff), so);
            }
            catch (SocketException err)
            {
                if (err.ErrorCode == 10054) //客户端退出
                {
                    Client.Close();
                }
                else if (err.ErrorCode == 10053)     //服务器主动断开连接
                {
                    Client.Close();
                }
                else if (err.ErrorCode == 10060)     //服务器主动断开连接
                {
                    Client.Close();
                }
                else
                {
                    Client.Close();
                }
            }
            catch (Exception err)
            {
            }
        }
        AutoResetEvent ars = new AutoResetEvent(true);
        void ProcessMess(ReviceBuffer buff)
        {
            byte[] byt = buff.ReviceByte;
            string Mess = Encoding.ASCII.GetString(byt).Trim().Trim((char)'\0');
            if (Mess.StartsWith("GetFile ", StringComparison.InvariantCultureIgnoreCase))
            {
                string FileName = Mess.Replace("GetFile ", "").Trim().Trim((char)0);

                SendFile(FileName, buff.Client);
            }
            else if (Mess.Equals("Success", StringComparison.InvariantCultureIgnoreCase))
            {
                ars.Set();
            }
            else if (Mess.Equals("Error", StringComparison.InvariantCultureIgnoreCase))
            {
                byte[] sendBuff = SocketFileInfo.GetSendByte(socketSendFile);
                ars.Reset();
                socketSendFile.Client.BeginSend(
                     sendBuff,
                     0,
                     sendBuff.Length,
                     SocketFlags.None, null, null);
            }
            else if (Mess.Equals("SynchroGetFile ", StringComparison.InvariantCultureIgnoreCase))
            {
                string FileName = Mess.Replace("SynchroGetFile ", "").Trim().Trim((char)0);
            }
        }
        void SynchroSendFile(string FileName, Socket Client)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            byte[] sendBuff = new byte[10240];
            int currentLen = fs.Read(sendBuff, 0, sendBuff.Length);
            SocketFileInfo socketSendFile = new SocketFileInfo(client, fs, FileName);
            while (currentLen > 0)
            {
                socketSendFile.CurrentLen = currentLen;
                socketSendFile.CurrentOffset = socketSendFile.FileStream.Position - currentLen;
                byte[] Buff = SocketFileInfo.GetSendByte(socketSendFile);
                Client.Send(Buff, 0, Buff.Length, SocketFlags.None);
                currentLen = fs.Read(sendBuff, 0, sendBuff.Length);
            }
            fs.Close();
            fs.Dispose();


        }
        public void CloseClient(Socket Client)
        {
            Client.Close();
            Client = null;
        }
    }
}
