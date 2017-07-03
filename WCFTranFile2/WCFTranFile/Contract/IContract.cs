using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.IO;

namespace Contract
{
    // 注意: 如果更改此处的接口名称 "IContract"，也必须更新 App.config 中对 "IContract" 的引用。
    [ServiceContract(CallbackContract = typeof(ICallBack))]
    public interface IContract
    {
        [OperationContract]
        void Connection();
        [OperationContract]
        void SayHello(string Mess);
        [OperationContract(IsOneWay = false)]
        void DownFile(string FileName);
        [OperationContract(IsOneWay = false, AsyncPattern = true)]
        IAsyncResult BeginDownFile(string FileName, AsyncCallback ra, object state);
        void EndDownFile(IAsyncResult ra);
        [OperationContract]
        long GetFileLenth(string FileName);
        [OperationContract]
        byte[] ReadFileBtyes(string fileName, long offset, out   int length);
        [OperationContract(IsOneWay = false)]
        void UploadFile(CustomFileInfo currentUploadFile);
        [OperationContract(IsOneWay = false, AsyncPattern = true)]
        IAsyncResult BeginUploadFile(CustomFileInfo currentUploadFile, AsyncCallback ra, object state);
        void EndUploadFile(IAsyncResult ra);
    }
    public interface ICallBack
    {
        [OperationContract]
        bool DownFile(CustomFileInfo currentDownFile);
    }
}
