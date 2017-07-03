using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace SocketServiceContract
{
    [Serializable]
    public class SocketFileInfo
    {
        long fileLength;

        public long FileLength
        {
            get { return fileLength; }
            set { fileLength = value; }
        }
        long currentOffset;

        public long CurrentOffset
        {
            get { return currentOffset; }
            set { currentOffset = value; }
        }
        int currentLen;

        public int CurrentLen
        {
            get { return currentLen; }
            set { currentLen = value; }
        }
        public const int DownFileLength = 1024 * 10;
        [NonSerialized]
        FileStream fileStream;
        public int MaxLength
        {
            get
            {
                return DownFileLength;
            }
        }
        public FileStream FileStream
        {
            get { return fileStream; }
            set { fileStream = value; }
        }
        byte[] currentByte;

        public byte[] CurrentByte
        {
            get
            {
                if (currentByte == null)
                {
                    currentByte = new byte[DownFileLength];
                }
                return currentByte;
            }
            set { currentByte = value; }
        }
        string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
        [NonSerialized]
        Socket client;
        public Socket Client
        {
            get { return client; }
            set { client = value; }
        }
        public SocketFileInfo(Socket client, FileStream fs, string fileName)
        {
            this.client = client;
            this.fileStream = fs;
            this.fileName = fileName;

        }
        public static byte[] GetSendByte(SocketFileInfo socketInfo)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, socketInfo);
            byte[] byt = ms.ToArray();
            ms.Close();
            ms.Dispose();
            byte[] MessLength = BitConverter.GetBytes((uint)byt.Length);
            byte[] SendByte = new byte[MessLength.Length + byt.Length];
            Array.Copy(MessLength, 0, SendByte, 0, 4);
            Array.Copy(byt, 0, SendByte, 4, byt.Length);

            return SendByte;

        }
        public static SocketFileInfo DeSerialize(byte[] Revicebyt)
        {
            byte[] MessLength = new byte[4];
            Array.Copy(Revicebyt, 0, MessLength, 0, 4);
            byte[] infoByt = new byte[(int)BitConverter.ToUInt32(MessLength, 0)];
            Array.Copy(Revicebyt, 4, infoByt, 0, infoByt.Length);
            SocketFileInfo socketInfo;
            MemoryStream ms = new MemoryStream(infoByt);

            BinaryFormatter formatter = new BinaryFormatter();
            socketInfo = (SocketFileInfo)formatter.Deserialize(ms);
            ms.Close();
            ms.Dispose();
            return socketInfo;
        }
    }
}
