using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;

namespace Contract
{
    // 注意: 如果更改此处的类名 "Contract"，也必须更新 App.config 中对 "Contract" 的引用。
    public class ServerContract : IContract
    {
        #region IContract 成员

        public void DownFile(string FileName)
        {
            ICallBack callback = OperationContext.Current.GetCallbackChannel<ICallBack>();
            FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
            CustomFile customFile = new CustomFile(callback, fs, FileName);

            fs.BeginRead(customFile.CurrentByte, 0, customFile.MaxLength, (AsyncCallback)DownFileCallBack, customFile);
        }
        private void DownFileCallBack(IAsyncResult ra)
        {
            CustomFile customFile = (CustomFile)ra.AsyncState;
            int currentLen = customFile.FileStream.EndRead(ra);

            if (currentLen != 0)
            {
                CustomFileInfo customFileInfo = new CustomFileInfo();
                customFileInfo.CurrentData = customFile.CurrentByte;
                customFileInfo.CurrentOffset = customFile.FileStream.Position - currentLen;
                customFileInfo.FileLength = customFile.FileStream.Length;
                customFileInfo.CurrentLen = currentLen;
                customFileInfo.FileName = customFile.FileName;
                if (customFile.ClientCallBack.DownFile(customFileInfo))
                {
                    customFile.FileStream.BeginRead(customFile.CurrentByte, 0, customFile.MaxLength, (AsyncCallback)DownFileCallBack, customFile);
                }
                else
                {
                    customFile.FileStream.Close();
                    customFile.FileStream.Dispose();
                }
            }
            else
            {
                customFile.FileStream.Close();
                customFile.FileStream.Dispose();
            }
        }
        public IAsyncResult BeginDownFile(string FileName, AsyncCallback ra, object state)
        {
            throw new NotImplementedException();
        }

        public void EndDownFile(IAsyncResult ra)
        {
            throw new NotImplementedException();
        }

        public void UploadFile(CustomFileInfo currentUploadFile)
        {
            FileStream fs = new FileStream(currentUploadFile.ServerPath + "\\" + currentUploadFile.FileName, FileMode.OpenOrCreate, FileAccess.Write);
            fs.Seek(currentUploadFile.CurrentOffset, SeekOrigin.Begin);
            fs.BeginWrite(
                currentUploadFile.CurrentData, 0,
                currentUploadFile.CurrentLen,
                new AsyncCallback(delegate(IAsyncResult ra)
                    {
                        fs.EndWrite(ra);
                        fs.Close();
                        fs.Dispose();
                    }), null);
        }

        public IAsyncResult BeginUploadFile(CustomFileInfo currentUploadFile, AsyncCallback ra, object state)
        {
            throw new NotImplementedException();
        }

        public void EndUploadFile(IAsyncResult ra)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region IContract 成员

        public void SayHello(string Mess)
        {
            Console.Write("WCF:" + Mess);
        }

        #endregion

        #region IContract 成员

        public void Connection()
        {
            
        }

        #endregion
        ///   <summary>   
        ///   下载文件   
        ///   </summary>   
        ///   <param   name="fileName">文件的名字,不含路径</param>   
        ///   <param   name="offset">偏移量</param>   
        ///   <param   name="length">读取字节的长度</param>   
        ///   <returns>如果正确则返回字节数组</returns>   
        public byte[] ReadFileBtyes(string fileName, long offset, out   int length)
        {
            length = 0;
            if (!System.IO.File.Exists(fileName))
            {
                return null;
            }

            byte[] bts = new byte[1024 * 10];
            FileStream fs = null;
            try
            {
                fs = File.OpenRead(fileName);
                fs.Seek(offset, SeekOrigin.Begin);
                length = fs.Read(bts, 0, bts.Length);
                fs.Close();
                return bts;
            }
            catch (Exception err)
            {
                Console.WriteLine("发生错误" + err.ToString());
                if (fs != null)
                {
                    fs.Close();
                }
            }
            return null;
        }

        /// <summary>
        /// 获取文件的长度
        /// </summary>
        /// <param name="FileName">文件名</param>
        /// <returns></returns>
        public long GetFileLenth(string FileName)
        {
            try
            {
                if (File.Exists(FileName))
                {
                    FileInfo f = new FileInfo(FileName);
                    long Len = f.Length;
                    return Len;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }

    }
}
