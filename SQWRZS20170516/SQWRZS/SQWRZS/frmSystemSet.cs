using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SQWRZS
{
    public partial class frmSystemSet : Form
    {
        public frmSystemSet()
        {
            InitializeComponent();
        }

        private SystemSet.SysSet sysSet = new SystemSet.SysSet();

        public frmSystemSet(SystemSet.SysSet sysSet)
        {
            InitializeComponent();
            this.sysSet = sysSet;
        }

        private void frmSystemSet_Load(object sender, EventArgs e)
        {

            foreach (var item in System.IO.Ports.SerialPort.GetPortNames())
            {
                cmbCZYPort.Items.Add(item);
            }
            ShowSet();

        }

        /// <summary>
        /// 显示参数
        /// </summary>
        private void ShowSet()
        {
            txtDBName.Text = sysSet.DBName;
            txtDBIP.Text = sysSet.DBIP;
            txtDBUser.Text = sysSet.DBUser;
            txtDBPwd.Text = sysSet.DBPwd;

            txtFtpIP.Text = sysSet.FtpIP;
            txtFtpPath.Text = sysSet.FtpPath;
            txtFtpUser.Text = sysSet.FtpUser;
            txtFtpPwd.Text = sysSet.FtpPwd;
            txtFtpPort.Text = sysSet.FtpPort.ToString();

            txtSystemTitle.Text = sysSet.SystemTitle;
            txtDataSavePath.Text = sysSet.DataSavePath;
            txtDataSaveDays.Text = sysSet.DataSaveDays.ToString();
            txtZDName.Text = sysSet.ZDMC;
            txtZDIP.Text = sysSet.ZDIP;
            txtCXBZ.Text = sysSet.MinCXBZ.ToString() + "--" + sysSet.MaxCXBZ.ToString();
            txtMaxZS.Text = sysSet.MaxZS.ToString();
            txtMinZS.Text = sysSet.MinZS.ToString();
            txtMaxZL.Text = sysSet.MaxZL.ToString();
            txtMinZL.Text = sysSet.MinZL.ToString();


            cmbCZYPort.Text = sysSet.CZYPort;
            cmbCZYBaudRate.Text = sysSet.CZYBaudRate.ToString();

            switch (sysSet.CZYParity)
            {
                case System.IO.Ports.Parity.Even:
                    cmbCZYParity.Text = "偶检验";
                    break;
                case System.IO.Ports.Parity.None:
                    cmbCZYParity.Text = "无";
                    break;
                case System.IO.Ports.Parity.Odd:
                    cmbCZYParity.Text = "奇校验";
                    break;
                default:
                    cmbCZYParity.Text = "无";
                    break;
            }
            cmbCZYDataBit.Text = sysSet.CZYDataBit.ToString();

            switch (sysSet.CZYStopBits)
            {
                case System.IO.Ports.StopBits.None:
                    break;
                case System.IO.Ports.StopBits.One:
                    cmbCZYStopBit.Text = "1";
                    break;
                case System.IO.Ports.StopBits.OnePointFive:
                    cmbCZYStopBit.Text = "1.5";
                    break;
                case System.IO.Ports.StopBits.Two:
                    cmbCZYStopBit.Text = "2";
                    break;
                default:
                    cmbCZYStopBit.Text = "1";
                    break;
            }

            cmbSetPSNo.Items.Clear();
            for (int i = 0; i < sysSet.CpsbList.Count; i++)
            {
                cmbSetPSNo.Items.Add(i + 1);
            }

            if (cmbSetPSNo.Items.Count > 0)
            {
                cmbSetPSNo.SelectedIndex = 0;
            }

            cmbPSFX.Text = sysSet.CpsbList[0].FangXiang;
            txtPSUserName.Text = sysSet.CpsbList[0].User;
            txtPSPwd.Text = sysSet.CpsbList[0].Pwd;
            txtPSNetPort.Text = sysSet.CpsbList[0].NetPort.ToString();
            txtPSIP.Text = sysSet.CpsbList[0].IP;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmbSetPSNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSetPSNo.Text == null || cmbSetPSNo.Text == "")
            {
                return;
            }
            int num = Convert.ToInt32(cmbSetPSNo.Text) - 1;

            cmbPSFX.Text = sysSet.CpsbList[num].FangXiang;
            txtPSUserName.Text = sysSet.CpsbList[num].User;
            txtPSPwd.Text = sysSet.CpsbList[num].Pwd;
            txtPSNetPort.Text = sysSet.CpsbList[num].NetPort.ToString();
            txtPSIP.Text = sysSet.CpsbList[num].IP;
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "选择保存路径";
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtDataSavePath.Text = fbd.SelectedPath;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            switch (tabControlSet.SelectedIndex)
            {
                case 0:
                    EditSoftSet(); 
                    break;
                case 1:
                    EditDeviceSet();
                    break;
                default:
                    break;
            }


        }
        /// <summary>
        /// 修改硬件配置
        /// </summary>
        private void EditDeviceSet()
        {
            sysSet.CZYPort = cmbCZYPort.Text;
            sysSet.CZYBaudRate = Convert.ToInt32(cmbCZYBaudRate.Text);
            sysSet.CZYDataBit = Convert.ToInt32(cmbCZYDataBit.Text);

            switch (cmbCZYParity.Text)
            {
                case "偶检验":
                    sysSet.CZYParity = System.IO.Ports.Parity.Even;
                    break;
                case "奇检验":
                    sysSet.CZYParity = System.IO.Ports.Parity.Odd;
                    break;
                case "无":
                    sysSet.CZYParity = System.IO.Ports.Parity.None;
                    break;
                default:
                    sysSet.CZYParity = System.IO.Ports.Parity.None;
                    break;
            }

            switch (cmbCZYStopBit.Text)
            {
                case "1":
                    sysSet.CZYStopBits = System.IO.Ports.StopBits.One;
                    break;
                case "1.5":
                    sysSet.CZYStopBits = System.IO.Ports.StopBits.OnePointFive;
                    break;
                case "2":
                    sysSet.CZYStopBits = System.IO.Ports.StopBits.Two;
                    break;
                default:
                    sysSet.CZYStopBits = System.IO.Ports.StopBits.One;
                    break;
            }

            #region 修改仪表

            XElement xe = XElement.Load("SystemSet.xml");

            IEnumerable<XElement> elements = from ele in xe.Elements("DeviceSet").Elements("CZY")
                                             select ele;

            XElement xelCZY = elements.ElementAt(0);
            xelCZY.Element("Port").Value = sysSet.CZYPort;
            xelCZY.Element("BaudRate").Value = sysSet.CZYBaudRate.ToString();
            xelCZY.Element("Parit").Value = cmbCZYParity.Text;
            xelCZY.Element("DataBit").Value = sysSet.CZYDataBit.ToString();
            xelCZY.Element("StopBit").Value = cmbCZYStopBit.Text;


            #endregion

            #region 修改牌识

            int editPSNo = Convert.ToInt32(cmbSetPSNo.Text) - 1;
            sysSet.CpsbList[editPSNo].FangXiang = cmbPSFX.Text;
            sysSet.CpsbList[editPSNo].User = txtPSUserName.Text;
            sysSet.CpsbList[editPSNo].Pwd = txtPSPwd.Text;
            sysSet.CpsbList[editPSNo].NetPort = Convert.ToInt32(txtPSNetPort.Text);
            sysSet.CpsbList[editPSNo].IP = txtPSIP.Text;

            // xe = XElement.Load("SystemSet.xml");

            elements = from ele in xe.Elements("DeviceSet").Elements("CPSB").Elements("Device")
                       select ele;


            XElement xel = elements.ElementAt(editPSNo);

            xel.Element("FangXiang").Value = sysSet.CpsbList[editPSNo].FangXiang;
            xel.Element("User").Value = sysSet.CpsbList[editPSNo].User;
            xel.Element("PassWord").Value = sysSet.CpsbList[editPSNo].Pwd;
            xel.Element("Port").Value = sysSet.CpsbList[editPSNo].NetPort.ToString();
            xel.Element("IP").Value = sysSet.CpsbList[editPSNo].IP;

            #endregion



            xe.Save("SystemSet.xml");
            MessageBox.Show("配置文件修改完成。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// 修改软件部分配置
        /// </summary>
        private void EditSoftSet()
        {
            sysSet.DBName = txtDBName.Text;
            sysSet.DBIP = txtDBIP.Text;
            sysSet.DBUser = txtDBUser.Text;
            sysSet.DBPwd = txtDBPwd.Text;

            sysSet.FtpIP = txtFtpIP.Text;
            sysSet.FtpPath = txtFtpPath.Text;
            sysSet.FtpUser = txtFtpUser.Text;
            sysSet.FtpPwd = txtFtpPwd.Text;
            sysSet.FtpPort = Convert.ToInt32(txtFtpPort.Text);

            sysSet.SystemTitle = txtSystemTitle.Text;
            sysSet.DataSavePath = txtDataSavePath.Text;
            sysSet.DataSaveDays = Convert.ToInt32(txtDataSaveDays.Text);
            sysSet.ZDMC = txtZDName.Text;
            sysSet.ZDIP = txtZDIP.Text;
            sysSet.MaxCXBZ = Convert.ToInt32(txtCXBZ.Text.Split(new string[] { "--" }, StringSplitOptions.None)[1]);
            sysSet.MinCXBZ = Convert.ToInt32(txtCXBZ.Text.Split(new string[] { "--" }, StringSplitOptions.None)[0]);
            sysSet.MaxZS = Convert.ToInt32(txtMaxZS.Text);
            sysSet.MinZS = Convert.ToInt32(txtMinZS.Text);
            sysSet.MaxZL = Convert.ToInt32(txtMaxZL.Text);
            sysSet.MinZL = Convert.ToInt32(txtMinZL.Text);

            XElement xe = XElement.Load("SystemSet.xml");

            IEnumerable<XElement> elements = from ele in xe.Elements("SoftSet")
                                             select ele;

            foreach (var item in elements)
            {
                item.Element("EchoOrData").Element("SystemTitle").Value = sysSet.SystemTitle;
                item.Element("EchoOrData").Element("DataSavePath").Value = sysSet.DataSavePath;
                item.Element("EchoOrData").Element("DataSaveDays").Value = sysSet.DataSaveDays.ToString();
                item.Element("EchoOrData").Element("ZDIP").Value = sysSet.ZDIP;
                item.Element("EchoOrData").Element("ZDMC").Value = sysSet.ZDMC;
                item.Element("EchoOrData").Element("MaxZS").Value = sysSet.MaxZS.ToString();
                item.Element("EchoOrData").Element("MinZS").Value = sysSet.MinZS.ToString();
                item.Element("EchoOrData").Element("MaxZL").Value = sysSet.MaxZL.ToString();
                item.Element("EchoOrData").Element("MinZL").Value = sysSet.MinZL.ToString();
                item.Element("EchoOrData").Element("MaxCXBZ").Value = sysSet.MaxCXBZ.ToString();
                item.Element("EchoOrData").Element("MinCXBZ").Value = sysSet.MinCXBZ.ToString();

                item.Element("DataBase").Element("IP").Value = sysSet.DBIP;
                item.Element("DataBase").Element("Name").Value = sysSet.DBName;
                item.Element("DataBase").Element("User").Value = sysSet.DBUser;
                item.Element("DataBase").Element("PassWord").Value = sysSet.DBPwd;

                item.Element("Ftp").Element("IP").Value = sysSet.FtpIP;
                item.Element("Ftp").Element("Path").Value = sysSet.FtpPath;
                item.Element("Ftp").Element("User").Value = sysSet.FtpUser;
                item.Element("Ftp").Element("PassWord").Value = sysSet.FtpPwd;
                item.Element("Ftp").Element("Port").Value = sysSet.FtpPort.ToString();
            }

            xe.Save("SystemSet.xml");
            MessageBox.Show("配置文件修改完成。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


    }
}
