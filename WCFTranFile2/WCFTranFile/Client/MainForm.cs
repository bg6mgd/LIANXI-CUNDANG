using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ServiceModel.Channels;
using System.ServiceModel;
using Contract;
using System.IO;
using System.Threading;
using SocketServiceContract;

namespace Client
{
    public partial class MainForm : Form
    {
        public string ServiceIP = "192.168.0.96";
        public MainForm()
        {
            InitializeComponent();
            btnStart.Enabled = false;
            btnSendMess.Enabled = false;
        }
        DuplexChannelFactory<IContract> duplexChannelFactory = null;
        public DuplexChannelFactory<IContract> DuplexChannelFactory
        {
            get
            {
                if (duplexChannelFactory == null)
                {
                    AddressHeaderCollection headers = new System.ServiceModel.Channels.AddressHeaderCollection();
                    NetTcpBinding tcpBinding = new NetTcpBinding();
                    tcpBinding.Security.Mode = SecurityMode.None;
                    tcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
                    tcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
                    tcpBinding.ReliableSession.Enabled = true;
                    EndpointAddress tcpAddress = new EndpointAddress("net.tcp://" + ServiceIP + ":9999/HelloWCFService");
                    duplexChannelFactory = new DuplexChannelFactory<IContract>(
                        new InstanceContext(
                            new CallBack((DownFile)delegate(CustomFileInfo currentDownFile)
                                {
                                    using (FileStream fs = new FileStream("WCF" + currentDownFile.FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                                    {
                                        fs.Position = currentDownFile.CurrentOffset;
                                        fs.Write(currentDownFile.CurrentData, 0, currentDownFile.CurrentLen);
                                        fs.Flush();
                                        fs.Close();
                                        fs.Dispose();
                                    }

                                    long downSize = currentDownFile.CurrentOffset + currentDownFile.CurrentLen;
                                    int offset = (int)(downSize * 100 / currentDownFile.FileLength);
                                    if (this.IsDisposed) return;
                                    this.Invoke((MethodInvoker)delegate()
                                    {
                                        this.pbWCFDown.Value = offset;
                                        this.lbWCFDownPos.Text = offset + "%";
                                        if (downSize > 1024 * 1024 * 1024)
                                        {
                                            float posLen = downSize / (1024f * 1024f * 1024f);
                                            this.lbWCFDown.Text = posLen.ToString("f1") + "G";
                                        }
                                        else if (downSize > 1024 * 1024)
                                        {
                                            float posLen = downSize / (1024f * 1024f);
                                            this.lbWCFDown.Text = posLen.ToString("f1") + "M";
                                        }
                                        else if (downSize > 1024)
                                        {
                                            float posLen = downSize / 1024f;
                                            this.lbWCFDown.Text = posLen.ToString("f1") + "K";
                                        }
                                        else
                                        { this.lbWCFDown.Text = downSize.ToString("f1") + "B"; }

                                    });

                                }

                            )), tcpBinding, tcpAddress);
                    duplexChannelFactory.Closing += delegate
                    {
                        helloWCF = null;
                        duplexChannelFactory = null;
                    };
                }
                return duplexChannelFactory;
            }
        }
        IContract helloWCF = null;

        public IContract HelloWCF
        {
            get
            {
                if (helloWCF == null)
                {

                    helloWCF = DuplexChannelFactory.CreateChannel();

                }
                return helloWCF;
            }
        }
        IContract helloAsyWCF = null;
        public IContract HelloAsyWCF
        {
            get
            {
                if (helloAsyWCF == null)
                {

                    helloAsyWCF = DuplexChannelFactory.CreateChannel();

                }
                return helloAsyWCF;
            }
        }
        SynchroSocketClient synchroClient = new SynchroSocketClient();
        AsynchronousSocketClient asynchronousClient = null;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            synchroClient.Revice += new ReviceBufferHandle(client_Revice);
        }

        void client_Revice(object sender, ReviceBuffer e)
        {
            SocketFileInfo fileInfo = SocketFileInfo.DeSerialize(e.ReviceByte);
            using (FileStream fs = new FileStream("Socket" + fileInfo.FileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                fs.Position = fileInfo.CurrentOffset;
                fs.Write(fileInfo.CurrentByte, 0, fileInfo.CurrentLen);
                fs.Flush();
                fs.Close();
                fs.Dispose();
            }

            long downSize = fileInfo.CurrentOffset + fileInfo.CurrentLen;
            int offset = (int)(downSize * 100 / fileInfo.FileLength);
            if (this.IsDisposed) return;
            this.Invoke((MethodInvoker)delegate()
            {
                this.pbSocketDown.Value = offset;
                this.lbSocketDownPos.Text = offset + "%";
                if (downSize > 1024 * 1024 * 1024)
                {
                    float posLen = downSize / (1024f * 1024f * 1024f);
                    this.lbSocketDown.Text = posLen.ToString("f1") + "G";
                }
                else if (downSize > 1024 * 1024)
                {
                    float posLen = downSize / (1024f * 1024f);
                    this.lbSocketDown.Text = posLen.ToString("f1") + "M";
                }
                else if (downSize > 1024)
                {
                    float posLen = downSize / 1024f;
                    this.lbSocketDown.Text = posLen.ToString("f1") + "K";
                }
                else
                { this.lbSocketDown.Text = downSize.ToString("f1") + "B"; }

            });
        }
        private void btnConnection_Click(object sender, EventArgs e)
        {
            asynchronousClient = new AsynchronousSocketClient(AsynchronousData);
            synchroClient.Connect(ServiceIP, 7777);
            asynchronousClient.Connect(ServiceIP, 7777);




            HelloWCF.Connection();
            HelloAsyWCF.Connection();
            btnStart.Enabled = true;
            btnSendMess.Enabled = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {    
            Thread downFile = new Thread((ThreadStart)delegate()
                {
                    asynchronousClient.DownFile(txtServerFileName.Text);
                });

            Thread th = new Thread((ThreadStart)delegate()
                {
                    DownFile("WCF同步" + txtServerFileName.Text, txtServerFileName.Text);
                });
            string cmd = "GetFile " + txtServerFileName.Text;
            byte[] byt = Encoding.ASCII.GetBytes(cmd);
            synchroClient.SendMess(byt);

            downFile.Start();
            HelloWCF.BeginDownFile(txtServerFileName.Text, null, null);
            th.Start();
        }

        private void btnSendMess_Click(object sender, EventArgs e)
        {
            synchroClient.SendMess("消息抓包测试");
            HelloWCF.SayHello("消息抓包测试");
        }

        public void AsynchronousData(long downSize, int offset)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                this.pbSocketAsyDown.Value = offset;
                this.lbSocketAsyDownPos.Text = offset + "%";
                if (downSize > 1024 * 1024 * 1024)
                {
                    float posLen = downSize / (1024f * 1024f * 1024f);
                    this.lbSocketDownAsy.Text = posLen.ToString("f1") + "G";
                }
                else if (downSize > 1024 * 1024)
                {
                    float posLen = downSize / (1024f * 1024f);
                    this.lbSocketDownAsy.Text = posLen.ToString("f1") + "M";
                }
                else if (downSize > 1024)
                {
                    float posLen = downSize / 1024f;
                    this.lbSocketDownAsy.Text = posLen.ToString("f1") + "K";
                }
                else
                { this.lbSocketDownAsy.Text = downSize.ToString("f1") + "B"; }

            });
        }
        private void DownFile(string LocalFileName, string ServerPath)
        {


            long filelenth = HelloAsyWCF.GetFileLenth(ServerPath);
            if (filelenth == 0)
            {
                throw new Exception("服务器请求文件失败");
            }
            using (FileStream fs = new FileStream(LocalFileName, FileMode.Create))
            {
                int length = 0;
                long CurrentLength = 0;
                byte[] bts = HelloAsyWCF.ReadFileBtyes(ServerPath, 0, out   length);
                while (bts != null && length > 0)
                {
                    CurrentLength += length;
                    fs.Write(bts, 0, length);
                    bts = HelloAsyWCF.ReadFileBtyes(ServerPath, CurrentLength, out   length);
                    int Pos = Convert.ToInt32(CurrentLength * 100 / filelenth);//当前进度
                    long downSize = fs.Length;
                    int offset = (int)(downSize * 100 / filelenth);
                    if (this.IsDisposed) return;
                    this.Invoke((MethodInvoker)delegate()
                    {
                        this.pbDown.Value = offset;
                        this.lbDownPos.Text = offset + "%";
                        if (downSize > 1024 * 1024 * 1024)
                        {
                            float posLen = downSize / (1024f * 1024f * 1024f);
                            this.lbDown.Text = posLen.ToString("f1") + "G";
                        }
                        else if (downSize > 1024 * 1024)
                        {
                            float posLen = downSize / (1024f * 1024f);
                            this.lbDown.Text = posLen.ToString("f1") + "M";
                        }
                        else if (downSize > 1024)
                        {
                            float posLen = downSize / 1024f;
                            this.lbDown.Text = posLen.ToString("f1") + "K";
                        }
                        else
                        { this.lbDown.Text = downSize.ToString("f1") + "B"; }

                    });

                }
                fs.Close();
                fs.Dispose();
            }

        }
    }
}
