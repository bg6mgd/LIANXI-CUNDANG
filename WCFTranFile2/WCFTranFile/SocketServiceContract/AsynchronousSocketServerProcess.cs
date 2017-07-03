using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace SocketServiceContract
{
    public class AsynchronousSocketServerProcess
    {
        Socket client = null;
        public AsynchronousSocketServerProcess(Socket client)
        {
            this.client = client;
            StateObject so = new StateObject();
            so.Client = client;
            client.BeginReceive(so.RevaceBuf, 0, StateObject.BufferSize, SocketFlags.None, out so.Error, new AsyncCallback(DoReadBuff), so);

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
            }
            catch (SocketException)
            {

                Client.Close();

            }
        }
        void ProcessMess(ReviceBuffer buff)
        {
            byte[] byt = buff.ReviceByte;
            string Mess = Encoding.ASCII.GetString(byt).Trim().Trim((char)'\0');
            if (Mess.StartsWith("SynchroGetFile ", StringComparison.InvariantCultureIgnoreCase))
            {
                string FileName = Mess.Replace("SynchroGetFile ", "").Trim().Trim((char)0);
                SynchroSendFile(FileName);
            }
        }
        void SynchroSendFile(string FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);

            SocketFileInfo socketSendFile = new SocketFileInfo(client, fs, FileName);
            socketSendFile.FileLength = fs.Length;
            int currentLen = fs.Read(socketSendFile.CurrentByte, 0, socketSendFile.MaxLength);
            byte[] MessByte = new byte[1024];

            while (currentLen > 0)
            {
                socketSendFile.CurrentLen = currentLen;
                socketSendFile.CurrentOffset = socketSendFile.FileStream.Position - currentLen;
                byte[] Buff = SocketFileInfo.GetSendByte(socketSendFile);
                while (true)
                {
                    client.Send(Buff, 0, Buff.Length, SocketFlags.None);
                    client.Receive(MessByte, SocketFlags.None);
                    string Mess = Encoding.ASCII.GetString(MessByte).Trim().Trim((char)'\0');
                    if (Mess.Equals("Success!", StringComparison.InvariantCultureIgnoreCase))
                    {
                        break;
                    }
                }
                currentLen = fs.Read(socketSendFile.CurrentByte, 0, socketSendFile.MaxLength);
            }
            fs.Close();
            fs.Dispose();
            client.Close();
        }
        public void CloseClient(Socket Client)
        {
            Client.Close();
            Client = null;
        }
    }
}
