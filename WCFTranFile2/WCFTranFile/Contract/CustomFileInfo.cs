using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Contract
{
    public struct CustomFileInfo
    {
        string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }
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
        byte[] currentData;

        public byte[] CurrentData
        {
            get { return currentData; }
            set { currentData = value; }
        }
        int currentLen;

        public int CurrentLen
        {
            get { return currentLen; }
            set { currentLen = value; }
        }
        string serverPath;

        public string ServerPath
        {
            get { return serverPath; }
            set { serverPath = value; }
        }

    }
    public enum FileType
    {
        Dir,
        File
    }
    class CustomFile
    {
        public const int DownFileLength = 1024 * 10;
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
        ICallBack clientCallBack;

        public ICallBack ClientCallBack
        {
            get { return clientCallBack; }
            set { clientCallBack = value; }
        }
        string fileName;

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        public CustomFile(ICallBack clientCallBack, FileStream fs, string fileName)
        {
            this.clientCallBack = clientCallBack;
            this.fileStream = fs;
            this.fileName = fileName;

        }
    }
}
