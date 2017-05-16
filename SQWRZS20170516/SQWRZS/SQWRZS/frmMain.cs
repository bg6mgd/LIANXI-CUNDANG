using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO.Ports;
using NLog;
using System.Runtime.InteropServices;
using System.IO;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using SQWRZS.DataBase;
using System.Configuration;
using System.Diagnostics;
using SQWRZS.Devices;

namespace SQWRZS
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        /*
       * NLog支持如下几种记录等级：

          Trace - 最常见的记录信息，一般用于普通输出
          Debug - 同样是记录信息，不过出现的频率要比Trace少一些，一般用来调试程序
          Info - 信息类型的消息
          Warn - 警告信息，一般用于比较重要的场合
          Error - 错误信息
          Fatal - 致命异常信息。一般来讲，发生致命异常之后程序将无法继续执行。
       */

        public static string dirTxt;         //记录上传TXT文件存放路径
        public static string dirPhoto;       //记录车牌照片存放路径
        public static string dirUpPhoto;     //记录需上传车牌照片存放路径，上传完毕删除文件

        public SystemSet.SysSet SysSet = new SystemSet.SysSet();

        private static Logger logger = LogManager.GetCurrentClassLogger();

        private Int32[] m_lUserID;
        private Int32[] m_lFortifyHandle;
        //private HCNetSDK.MSGCallBack[] m_falarmData;

        private HCNetSDK.NET_DVR_DEVICEINFO_V30[] m_struDeviceInfo;


        /// <summary>
        /// 标定系数
        /// </summary>
        private static double K = 615;// 0.9 * 1000;//3750;

        /// <summary>
        /// 两个称板之间的时刻差不大于总时刻的一半，及FFFFFF/2=0x7FFFFF，即8388607,大于该值的话认为前一个时刻被清零
        /// </summary>
        private int timeInterval = 8388607;

        /// <summary>
        /// 数据接收队列
        /// </summary>
        private static ConcurrentQueue<byte> ConQueue = new ConcurrentQueue<byte>();


        private int RecvCount = 0;


        /// <summary>
        /// 存储车头车牌识别结果，与称重数据进行匹配
        /// </summary>
        CpsbResult[] cpsbResHead;

        /// <summary>
        /// 存贮车尾结果
        /// </summary>
        CpsbResult[] cpsbResTail;

        /// <summary>
        /// 存贮侧抓结果
        /// </summary>
        CpsbResult[] cpsbResSide;


        /// <summary>
        /// 牌识结构列表
        /// </summary>
        List<CpsbResult> listCpsb = new List<CpsbResult>();
        /// <summary>
        /// 称板数据列表
        /// </summary>
        public List<CBData> CBList = new List<CBData>();

        /// <summary>
        /// 线圈数组，总共8个线圈，根据实际线圈数量获取需要使用的
        /// </summary>
        Devices.XianQuan[] XianQuan = null;

        /// <summary>
        /// 接收到线圈信号后的状态
        /// </summary>
        int[] RecvXQZT = new int[8];


        /// <summary>
        /// 用于锁定列表
        /// </summary>
        private static object lockObj = new object();

        //frmDataShow formShow;


        Bitmap bmapCD1 = null, bmapCD2 = null, bmapCD3 = null, bmapCD4 = null;
        /// <summary>
        /// 线圈12同时触发。1收尾时判断2线圈是否收尾，如果2收尾则进入计算，2没收尾等待2收尾
        /// </summary>
        private bool TS12;
        /// <summary>
        /// 线圈23同时触发
        /// </summary>
        private bool TS23;
        /// <summary>
        /// 线圈34同时触发
        /// </summary>
        private bool TS34;

        private int DBInSuccessCount = 0; //入库的数量
        private int DBInFailedCount = 0;//入库的数量
        private int CBNotEnoughCount = 0;//称板不够计算的
        private int SuperRangeCount = 0;//超过设定范围的
        private int CSErrorCount = 0;//速度错误的

        private bool smallLedInited = false;
        private bool BigLEdInited = false;
        private Devices.LedSend ledSend = new LedSend();
        private bool ledDataShow = false;//是否有数据需要显示
        /// <summary>
        /// 显示缓存的个数
        /// </summary>
        private int DisFlag = 0;
        private string[] DisInfo = new string[3];

        /// <summary>
        /// 逆行数据
        /// </summary>
        private NXTable NXtable = new NXTable();

        frmDataShow formShow = null;

        #region 新版报警变量

        private uint iLastErr = 0;
        private Int32[] m_lRealHandle = new int[4];

        private Int32[] m_lAlarmHandle = new Int32[200];
        private HCNetSDK.MSGCallBack m_falarmData = null;
        HCNetSDK.NET_VCA_TRAVERSE_PLANE m_struTraversePlane = new HCNetSDK.NET_VCA_TRAVERSE_PLANE();
        HCNetSDK.NET_VCA_AREA m_struVcaArea = new HCNetSDK.NET_VCA_AREA();
        HCNetSDK.NET_VCA_INTRUSION m_struIntrusion = new HCNetSDK.NET_VCA_INTRUSION();

        private List<Int32> m_lUserIDList = null;
        private List<Int32> m_lUserIDListPreview = null;

        private int iDeviceNumber = 0; //添加设备个数

        string str_temp = "";
        string str_cpys = "";
        string str_cph = "";
        string str_photo = "";
        string str_upPhoto = "";
        string str_plate = "";
        short cd = 0;
        /// <summary>
        /// 四个视频预览窗体
        /// </summary>
        private PictureBox[] picBoxPreview = new PictureBox[4];
        #endregion


        /// <summary>
        /// 显示运行相关信息
        /// </summary>
        /// <param name="Msg">需要显示的信息</param>
        /// <param name="color">显示的颜色</param>
        private void ShowStateInfo(string Msg, Color color)
        {
            this.Invoke(new MethodInvoker(delegate
            {
                if (tabPageInfo.Parent != null)
                {
                    rtbStateInfo.SelectionColor = color;
                    rtbStateInfo.AppendText(DateTime.Now.ToString() + " " + Msg + "\r\n");
                    rtbStateInfo.ScrollToCaret();
                }
            }));
        }

        private void btnSysConfig_Click(object sender, EventArgs e)
        {
            frmSystemSet formSet = new frmSystemSet(SysSet);
            formSet.ShowDialog();
        }

        #region CBList添加修改

        private void CBListAdd(CBData CbData)
        {
            lock (lockObj)
            {
                CBList.Add(CbData);
            }
        }

        private void CBListRemove(List<CBData> except)
        {
            lock (lockObj)
            {
                CBList = CBList.Except(except).ToList();
            }
        }
        #endregion

        private void Test()
        {
            #region Linq测试

            List<CBData> listOdd = new List<CBData>();
            List<CBData> listEven = new List<CBData>();

            CBData cb = new CBData();
            //cb.TDH = 1;
            //listOdd.Add(cb);
            cb = new CBData();
            cb.TDH = 3;
            listOdd.Add(cb);
            //cb = new CBData();
            //cb.TDH = 3;
            //listOdd.Add(cb);
            //cb = new CBData();
            //cb.TDH = 1;
            //listOdd.Add(cb);

            cb = new CBData();
            cb.TDH = 2;
            listEven.Add(cb);

            cb = new CBData();
            cb.TDH = 4;
            listEven.Add(cb);
            //cb = new CBData();
            //cb.TDH = 4;
            //listEven.Add(cb);
            //cb = new CBData();
            //cb.TDH = 2;
            //listEven.Add(cb);

            var jj = from p in listEven where (from pp in listOdd select pp.TDH).Contains(p.TDH - 1) select p;

            foreach (var item in jj)
            {
                MessageBox.Show(item.TDH.ToString());
            }

            #endregion
            #region 根据速度修正重量
            ZH_ST_ZDCZB zh_st_zdczb = new ZH_ST_ZDCZB();
            zh_st_zdczb.CS = 58;
            zh_st_zdczb.ZZ = 17560;

            int ZZXZPos = (int)(zh_st_zdczb.CS % SysSet.SpeedXZInterver);
            if (ZZXZPos > 0)
            {//取模大于0，说明有余数，修正位置段应该为除数的商+1
                ZZXZPos = (int)(Convert.ToUInt32(zh_st_zdczb.CS) / SysSet.SpeedXZInterver) + 1;
                if (SysSet.SpeedXZPara.Length >= ZZXZPos)
                {
                    Console.WriteLine("速度=" + zh_st_zdczb.CS + ",修正参数=" + SysSet.SpeedXZPara[ZZXZPos - 1] + ",检测重量=" + zh_st_zdczb.ZZ);
                    logger.Info("速度=" + zh_st_zdczb.CS + ",修正参数=" + SysSet.SpeedXZPara[ZZXZPos - 1] + ",检测重量=" + zh_st_zdczb.ZZ);

                    zh_st_zdczb.ZZ = (long)(zh_st_zdczb.ZZ * SysSet.SpeedXZPara[ZZXZPos - 1]);
                    Console.WriteLine("修正后总重=" + zh_st_zdczb.ZZ);
                }
            }
            else
            {
                ZZXZPos = (int)(Convert.ToUInt32(zh_st_zdczb.CS) / SysSet.SpeedXZInterver);
                if (SysSet.SpeedXZPara.Length >= ZZXZPos)
                {
                    Console.WriteLine("速度=" + zh_st_zdczb.CS + ",修正参数=" + SysSet.SpeedXZPara[ZZXZPos - 1] + ",检测重量=" + zh_st_zdczb.ZZ);
                    logger.Info("速度=" + zh_st_zdczb.CS + ",修正参数=" + SysSet.SpeedXZPara[ZZXZPos - 1] + ",检测重量=" + zh_st_zdczb.ZZ);

                    zh_st_zdczb.ZZ = (long)(zh_st_zdczb.ZZ * SysSet.SpeedXZPara[ZZXZPos - 1]);
                    Console.WriteLine("修正后总重=" + zh_st_zdczb.ZZ);
                }
            }
            #endregion
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            //Test();

            InitDB();
            ReadSystemSet();
            K = SysSet.K;

            GetDataSavePath();

            if (SysSet.CZYName == "四方")
            {
                Thread threadData = new Thread(new ThreadStart(CombinationFrame));
                threadData.IsBackground = true;
                threadData.Start();
            }
            else if (SysSet.CZYName == "华驰")
            {
                Thread threadData = new Thread(new ThreadStart(CombinationFrameHuaChi));
                threadData.IsBackground = true;
                threadData.Start();
            }


            m_lUserID = new Int32[SysSet.CpsbList.Count];
            m_lFortifyHandle = new Int32[SysSet.CpsbList.Count];
            //m_falarmData = new HCNetSDK.MSGCallBack(;
            m_lUserIDList = new List<int>();
            m_lUserIDListPreview = new List<int>();

            m_struDeviceInfo = new HCNetSDK.NET_DVR_DEVICEINFO_V30[SysSet.CpsbList.Count];

            for (int i = 0; i < m_lUserID.Count(); i++)
            {
                m_lUserID[i] = -1;
                m_lFortifyHandle[i] = -1;

            }

            for (int i = 0; i < 4; i++)
            {
                picBoxPreview[i] = new PictureBox();
            }

            cpsbResHead = new CpsbResult[SysSet.CpsbList.Count * 2];
            for (int i = 0; i < cpsbResHead.Count(); i++)
            {
                cpsbResHead[i] = new CpsbResult();
            }
            cpsbResTail = new CpsbResult[SysSet.CpsbList.Count * 2];
            for (int i = 0; i < cpsbResTail.Count(); i++)
            {
                cpsbResTail[i] = new CpsbResult();
            }

            cpsbResSide = new CpsbResult[SysSet.CpsbList.Count * 2];
            for (int i = 0; i < cpsbResSide.Count(); i++)
            {
                cpsbResSide[i] = new CpsbResult();
            }

            /*
             * 初始化8个线圈，现在内部线圈是4个，使用序号为1,2,3,4的，
             * 其他序号保留，这样便于程序内部的快速分辨是哪个线圈
             */
            XianQuan = new Devices.XianQuan[9];
            for (int i = 0; i < 9; i++)
            {//初始化线圈
                XianQuan[i] = new Devices.XianQuan();
                XianQuan[i].XQBH = i;
                XianQuan[i].ChufaTime = XianQuan[i].ShouweiTime = DateTime.Now;
                XianQuan[i].XQZTChufa = XianQuan[i].XQZTShouwei = -1;
            }

            InitCZY();
            InitCPSB();
            InitSmallLED();

            //Parallel.Invoke(() => InitCPSB(), () => InitCZY());

            //ZH_ST_ZDCZB zdczb = new ZH_ST_ZDCZB();
            //DBInsert(zdczb);



            //timerFtp.Interval = 1000 * 60;
            //timerFtp.Tick += new EventHandler(timerFtp_Tick);
            //timerFtp.Start();

            //formShow = new frmDataShow();
            //formShow.Show();

            //LoadTodayData();
        }


        #region 上传进程判断 
        void timerFtp_Tick(object sender, EventArgs e)
        {
            timerFtp.Enabled = false;
            if (!IsFtpRun(false))
            {
                //Process.Start("JZFTP.exe");
            }
            timerFtp.Enabled = true;
        }

        /// <summary>
        /// FTP传送进程是否在运行
        /// </summary>
        /// <param name="isKill">是否杀死该进程</param>
        /// <returns></returns>
        private static bool IsFtpRun(bool isKill)
        {
            bool isFtpServerOn = false;
            Process[] pro = Process.GetProcesses();
            foreach (Process pp in pro)
            {
                if (pp.ProcessName.Equals("JZFTP"))
                {
                    if (isKill)
                    {
                        pp.Kill();
                    }
                    else
                    {
                        isFtpServerOn = true;
                    }
                    break;
                }
            }
            return isFtpServerOn;
        }
        #endregion

        #region 安装数据库
        /// <summary>
        /// 建立数据库
        /// </summary>
        private void InitDB()
        {
            try
            {
                if (!Directory.Exists("d:\\HWZC\\ZDDatabase"))
                {
                    Directory.CreateDirectory("d:\\HWZC\\ZDDatabase");
                }
                string DBName = ConfigurationManager.ConnectionStrings["ConnStringUser"].ConnectionString;
                DBName = DBName.Split(';')[1];
                DBName = DBName.Split('=')[1];

                DbHelperSQLP dbhelper1 = new DbHelperSQLP(ConfigurationManager.ConnectionStrings["ConnStringMaster"].ToString());

                string strSql = "select count(*) from sysdatabases where name='" + DBName + "'";//'JZWRZS'";

                object n = dbhelper1.GetSingle(strSql);

                if ((int)n > 0)
                {
                    return;
                }

                //StringBuilder sb = new StringBuilder();
                //sb.Append("Create database JZWRZS ");
                //sb.Append("ON(Name='JZWRZS_dat', FileName='D:\\HWZC\\ZDDatabase\\JZWRZS.mdf')");
                //sb.Append("LOG ON(Name='JZWRZS_log', FileName='D:\\HWZC\\ZDDatabase\\JZWRZS_Log.ldf')");

                StringBuilder sb = new StringBuilder();
                sb.Append("Create database  " + DBName);
                sb.Append(" ON(Name='" + DBName + "_dat', FileName='D:\\HWZC\\ZDDatabase\\" + DBName + ".mdf')");
                sb.Append("LOG ON(Name='" + DBName + "_log', FileName='D:\\HWZC\\ZDDatabase\\" + DBName + "_Log.ldf')");

                dbhelper1.ExecuteSql(sb.ToString());

                DbHelperSQLP DbHelper2 = new DbHelperSQLP(ConfigurationManager.ConnectionStrings["ConnStringUser"].ToString());

                //创建数据库成功,开始建立相关数据库表
                sb.Clear();
                sb.Append("CREATE TABLE zh_st_zdczb");
                sb.Append("(ID int IDENTITY (1, 1) NOT NULL,CPH nvarchar(15) NOT NULL,CPYS nvarchar(10),");
                sb.Append("ZZ int NOT NULL,ZS int NOT NULL,CS decimal(5,2) NOT NULL,XZ int NOT NULL,");
                sb.Append("CXL int NOT NULL,CPTX nvarchar(200) NOT NULL,PLATE nvarchar(200) NOT NULL,");
                sb.Append("CWTX nvarchar(200) NOT NULL,CMTX nvarchar(200) NOT NULL,");
                sb.Append("Video nvarchar(200) NOT NULL,");

                sb.Append("JCSJ datetime NOT NULL,ZX int DEFAULT NULL,CD int DEFAULT NULL,");
                sb.Append("CZY nvarchar(50) NOT NULL,ZDBZ nvarchar(50) NOT NULL,SFCX int DEFAULT NULL,");
                sb.Append("SFXZ int DEFAULT NULL,FJBZ int DEFAULT NULL,XHZL int DEFAULT NULL,");
                sb.Append("JCZT int DEFAULT NULL,SJDJ int DEFAULT NULL,FX nvarchar(50),SFSC int DEFAULT 0, SFNX int DEFAULT 0, PRIMARY KEY (ID))");

                DbHelper2.ExecuteSql(sb.ToString());

                sb.Clear();
                sb.Append("CREATE TABLE NXTable");
                sb.Append("(ID int IDENTITY (1, 1) NOT NULL,CPH nvarchar(15) NOT NULL,CPYS nvarchar(10),");
                sb.Append("CPTX nvarchar(200) NOT NULL,PLATE nvarchar(200) NOT NULL,");
                sb.Append("CWTX nvarchar(200) NOT NULL,CMTX nvarchar(200) NOT NULL,");
                sb.Append("Video nvarchar(200) NOT NULL,");
                sb.Append("JCSJ datetime NOT NULL, PRIMARY KEY (ID))");

                //sb.Clear();
                //sb.Append("CREATE TABLE CBData");
                //sb.Append("(ID int IDENTITY (1, 1) NOT NULL,");
                //sb.Append("TDH nvarchar(100) NOT NULL,");
                //sb.Append("JLZ nvarchar(500) NOT NULL,");
                //sb.Append("SK nvarchar(500) NOT NULL,");
                //sb.Append("SKC nvarchar(500) NOT NULL,");
                //sb.Append("CKZ nvarchar(500) NOT NULL,");
                //sb.Append("JSSJ nvarchar(1000) NOT NULL,");
                //sb.Append("YSSJ nvarchar(4000) NOT NULL,PRIMARY KEY (ID))");

                DbHelper2.ExecuteSql(sb.ToString());

            }
            catch (Exception ex)
            {
                logger.Debug("创建数据库或数据库表出现异常" + ex.ToString());
                MessageBox.Show("创建数据库或数据库表出现异常\r\n请联系相关厂家技术", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

        }
        #endregion

        #region 读取系统配置
        /// <summary>
        /// 读取系统配置
        /// </summary>
        private void ReadSystemSet()
        {
            SystemSet.DeviceCPSB deviceCpsb;

            XElement xe = XElement.Load("SystemSet.xml");

            IEnumerable<XElement> elements = from ele in xe.Elements("SoftSet")
                                             select ele;
            foreach (var item in elements)
            {
                SysSet.SystemTitle = item.Element("EchoOrData").Element("SystemTitle").Value;
                SysSet.DataSavePath = item.Element("EchoOrData").Element("DataSavePath").Value;
                SysSet.DataSaveDays = Convert.ToInt32(item.Element("EchoOrData").Element("DataSaveDays").Value);
                SysSet.ZDIP = item.Element("EchoOrData").Element("ZDIP").Value;
                SysSet.ZDMC = item.Element("EchoOrData").Element("ZDMC").Value;
                SysSet.MaxZS = Convert.ToInt32(item.Element("EchoOrData").Element("MaxZS").Value);
                SysSet.MinZS = Convert.ToInt32(item.Element("EchoOrData").Element("MinZS").Value);
                SysSet.MaxZL = Convert.ToInt32(item.Element("EchoOrData").Element("MaxZL").Value);
                SysSet.MinZL = Convert.ToInt32(item.Element("EchoOrData").Element("MinZL").Value);
                SysSet.MaxCXBZ = Convert.ToInt32(item.Element("EchoOrData").Element("MaxCXBZ").Value);
                SysSet.MinCXBZ = Convert.ToInt32(item.Element("EchoOrData").Element("MinCXBZ").Value);
                SysSet.K = Convert.ToInt32(item.Element("EchoOrData").Element("K").Value);

                SysSet.K13 = Convert.ToInt32(item.Element("EchoOrData").Element("K13").Value);
                SysSet.K35 = Convert.ToInt32(item.Element("EchoOrData").Element("K35").Value);
                SysSet.K57 = Convert.ToInt32(item.Element("EchoOrData").Element("K57").Value);
                SysSet.K79 = Convert.ToInt32(item.Element("EchoOrData").Element("K79").Value);
                SysSet.K911 = Convert.ToInt32(item.Element("EchoOrData").Element("K911").Value);
                SysSet.K24 = Convert.ToInt32(item.Element("EchoOrData").Element("K24").Value);
                SysSet.K46 = Convert.ToInt32(item.Element("EchoOrData").Element("K46").Value);
                SysSet.K68 = Convert.ToInt32(item.Element("EchoOrData").Element("K68").Value);
                SysSet.K810 = Convert.ToInt32(item.Element("EchoOrData").Element("K810").Value);

                SysSet.PreTime = Convert.ToInt32(item.Element("EchoOrData").Element("PreTime").Value);
                SysSet.NextTime = Convert.ToInt32(item.Element("EchoOrData").Element("NextTime").Value);
                SysSet.PsInterverTime = Convert.ToInt32(item.Element("EchoOrData").Element("PsInterverTime").Value);
                SysSet.XqInterverTime = Convert.ToInt32(item.Element("EchoOrData").Element("XqInterverTime").Value);

                SysSet.Axle2WeightMax = Convert.ToInt32(item.Element("EchoOrData").Element("Axle2WeightMax").Value);
                SysSet.Axle3WeightMax = Convert.ToInt32(item.Element("EchoOrData").Element("Axle3WeightMax").Value);
                SysSet.Axle4WeightMax = Convert.ToInt32(item.Element("EchoOrData").Element("Axle4WeightMax").Value);
                SysSet.Axle5WeightMax = Convert.ToInt32(item.Element("EchoOrData").Element("Axle5WeightMax").Value);
                SysSet.Axle6WeightMax = Convert.ToInt32(item.Element("EchoOrData").Element("Axle6WeightMax").Value);

                SysSet.SpeedXZInterver = Convert.ToInt32(item.Element("SpeedXZ").Element("Interver").Value);

                string[] s = item.Element("SpeedXZ").Element("XZPara").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SysSet.SpeedXZPara = new double[s.Length];
                for (int i = 0; i < s.Count(); i++)
                {
                    SysSet.SpeedXZPara[i] = Convert.ToDouble(s[i]);
                }

                s = item.Element("SpeedXZ").Element("K13").Element("XZPara").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SysSet.SpeedXZPara13 = new double[s.Length];
                for (int i = 0; i < s.Count(); i++)
                {
                    SysSet.SpeedXZPara13[i] = Convert.ToDouble(s[i]);
                }
                s = item.Element("SpeedXZ").Element("K35").Element("XZPara").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SysSet.SpeedXZPara35 = new double[s.Length];
                for (int i = 0; i < s.Count(); i++)
                {
                    SysSet.SpeedXZPara35[i] = Convert.ToDouble(s[i]);
                }
                s = item.Element("SpeedXZ").Element("K57").Element("XZPara").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SysSet.SpeedXZPara57 = new double[s.Length];
                for (int i = 0; i < s.Count(); i++)
                {
                    SysSet.SpeedXZPara57[i] = Convert.ToDouble(s[i]);
                }
                s = item.Element("SpeedXZ").Element("K79").Element("XZPara").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SysSet.SpeedXZPara79 = new double[s.Length];
                for (int i = 0; i < s.Count(); i++)
                {
                    SysSet.SpeedXZPara79[i] = Convert.ToDouble(s[i]);
                }
                s = item.Element("SpeedXZ").Element("K911").Element("XZPara").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SysSet.SpeedXZPara911 = new double[s.Length];
                for (int i = 0; i < s.Count(); i++)
                {
                    SysSet.SpeedXZPara911[i] = Convert.ToDouble(s[i]);
                }

                s = item.Element("SpeedXZ").Element("K24").Element("XZPara").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SysSet.SpeedXZPara24 = new double[s.Length];
                for (int i = 0; i < s.Count(); i++)
                {
                    SysSet.SpeedXZPara24[i] = Convert.ToDouble(s[i]);
                }
                s = item.Element("SpeedXZ").Element("K46").Element("XZPara").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SysSet.SpeedXZPara46 = new double[s.Length];
                for (int i = 0; i < s.Count(); i++)
                {
                    SysSet.SpeedXZPara46[i] = Convert.ToDouble(s[i]);
                }
                s = item.Element("SpeedXZ").Element("K68").Element("XZPara").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SysSet.SpeedXZPara68 = new double[s.Length];
                for (int i = 0; i < s.Count(); i++)
                {
                    SysSet.SpeedXZPara68[i] = Convert.ToDouble(s[i]);
                }
                s = item.Element("SpeedXZ").Element("K810").Element("XZPara").Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                SysSet.SpeedXZPara810 = new double[s.Length];
                for (int i = 0; i < s.Count(); i++)
                {
                    SysSet.SpeedXZPara810[i] = Convert.ToDouble(s[i]);
                }



                SysSet.DBIP = item.Element("DataBase").Element("IP").Value;
                SysSet.DBName = item.Element("DataBase").Element("Name").Value;
                SysSet.DBUser = item.Element("DataBase").Element("User").Value;
                SysSet.DBPwd = item.Element("DataBase").Element("PassWord").Value;

                SysSet.FtpIP = item.Element("Ftp").Element("IP").Value;
                SysSet.FtpPath = item.Element("Ftp").Element("Path").Value;
                SysSet.FtpUser = item.Element("Ftp").Element("User").Value;
                SysSet.FtpPwd = item.Element("Ftp").Element("PassWord").Value;
                SysSet.FtpPort = Convert.ToInt32(item.Element("Ftp").Element("Port").Value);
            }

            elements = from ele in xe.Elements("DeviceSet").Elements("CPSB").Elements("Device")
                       select ele;

            foreach (var item in elements)
            {
                deviceCpsb = new SystemSet.DeviceCPSB();
                deviceCpsb.IP = item.Element("IP").Value;
                deviceCpsb.User = item.Element("User").Value;
                deviceCpsb.Pwd = item.Element("PassWord").Value;
                deviceCpsb.NetPort = Convert.ToInt32(item.Element("Port").Value);
                deviceCpsb.FangXiang = item.Element("FangXiang").Value;

                SysSet.CpsbList.Add(deviceCpsb);
            }

            elements = from ele in xe.Elements("DeviceSet").Elements("CZY")
                       select ele;
            string temp;
            foreach (var item in elements)
            {
                SysSet.CZYName = item.Element("CZYName").Value;
                SysSet.CZYPort = item.Element("Port").Value;
                SysSet.CZYBaudRate = Convert.ToInt32(item.Element("BaudRate").Value);
                temp = item.Element("Parit").Value;
                if (temp == "无")
                {
                    SysSet.CZYParity = Parity.None;
                }
                else if (temp == "偶校验")
                {
                    SysSet.CZYParity = Parity.Even;
                }
                else if (temp == "奇校验")
                {
                    SysSet.CZYParity = Parity.Odd;
                }

                SysSet.CZYDataBit = Convert.ToInt32(item.Element("DataBit").Value);

                temp = item.Element("StopBit").Value;
                if (temp == "1")
                {
                    SysSet.CZYStopBits = StopBits.One;
                }
                else if (temp == "1.5")
                {
                    SysSet.CZYStopBits = StopBits.OnePointFive;
                }
                else if (temp == "2")
                {
                    SysSet.CZYStopBits = StopBits.Two;
                }
                else
                {
                    SysSet.CZYStopBits = StopBits.None;
                }
            }

        }
        #endregion

        #region 获取文件存贮位置
        /// <summary>
        /// 获取文件存贮位置
        /// </summary>
        private void GetDataSavePath()
        {
            dirTxt = SysSet.DataSavePath + "\\file";
            dirPhoto = SysSet.DataSavePath + "\\photo\\";
            dirUpPhoto = SysSet.DataSavePath + "\\upPhoto";
        }
        #endregion

        #region 初始化显示屏
        private void InitSmallLED()
        {
            if (EQ2008_DLL.User_OpenScreen(EQ2008_DLL.g_iCardNum_small) == false)
            {
                logger.Info("诣阔LED显示屏（小）,连接失败！");
                ShowStateInfo("诣阔LED显示屏（小）,连接失败！", Color.Red);
            }
            else
            {
                smallLedInited = true;
                SendLedRealtimeRight();
                logger.Trace("诣阔LED显示屏（小）,连接成功！");
                ShowStateInfo("诣阔LED显示屏（小）,连接成功！", Color.Black);
            }
        }

        /// <summary>
        /// 发送一次右侧显示的 超载提示
        /// </summary>
        private void SendLedRealtimeRight()
        {
            try
            {
                if (!EQ2008_DLL.User_RealtimeConnect(EQ2008_DLL.g_iCardNum_small))
                {
                    Console.WriteLine("连接实时通信失败！");
                }
                else
                {
                    Console.WriteLine("连接实时通信成功！");
                    #region 开始发送内容 
                    int iX = 241;
                    int iY = 0;
                    int iW = 79;
                    int iH = 208;
                    string strText = "超 \n载";
                    //string strText = "测 \n试";
                    User_FontSet FontInfo = new User_FontSet();

                    FontInfo.bFontBold = false;
                    FontInfo.bFontItaic = false;
                    FontInfo.bFontUnderline = false;
                    FontInfo.colorFont = 0xFFFF;
                    FontInfo.iFontSize = 52;
                    FontInfo.strFontName = "黑体";
                    FontInfo.iAlignStyle = 0;
                    FontInfo.iVAlignerStyle = 0;
                    FontInfo.iRowSpace = 0;

                    Console.WriteLine("Led发送=" + strText);

                    if (!EQ2008_DLL.User_RealtimeSendText(EQ2008_DLL.g_iCardNum_small, iX, iY, iW, iH, strText, ref FontInfo))
                    {
                        Console.WriteLine("发送实时文本失败！");
                    }
                    else
                    {
                        Console.WriteLine("发送实时文本成功！");
                    }
                    #endregion

                    #region 测试左侧发送
                    //string[] sss = new string[3];
                    //sss[0] = "豫A7GF13";
                    //sss[1] = "豫ADY222";
                    //sss[2] = "豫AA3333";
                    //SendLedRealtime(sss);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                logger.Debug("发送LED异常" + ex.ToString(), ex);
                Console.WriteLine("发送LED异常" + ex.ToString());
            }
            finally
            {
                if (!EQ2008_DLL.User_RealtimeDisConnect(EQ2008_DLL.g_iCardNum_small))
                {
                    Console.WriteLine("关闭实时通信失败！");
                }
                else
                {
                    Console.WriteLine("关闭实时通信成功！");
                }
            }
        }
        /// <summary>
        /// 实时发送模式发送数据
        /// </summary>
        private void SendLedRealtime(string[] Content)
        {
            try
            {
                Console.WriteLine("LED发送数据.");

                if (!EQ2008_DLL.User_RealtimeConnect(EQ2008_DLL.g_iCardNum_small))
                {
                    Console.WriteLine("连接实时通信失败！");
                }
                else
                {
                    Console.WriteLine("连接实时通信成功！");
                    #region 开始发送内容 
                    int iX = 0;
                    int iY = 0;
                    int iW = 240;
                    int iH = 208;
                    string strText = Content[0] + "\n" + Content[1] + "\n" + Content[2];
                    User_FontSet FontInfo = new User_FontSet();

                    FontInfo.bFontBold = false;
                    FontInfo.bFontItaic = false;
                    FontInfo.bFontUnderline = false;
                    FontInfo.colorFont = 0xFFFF;
                    FontInfo.iFontSize = 40;
                    FontInfo.strFontName = "宋体";
                    FontInfo.iAlignStyle = 1;
                    FontInfo.iVAlignerStyle = 1;
                    FontInfo.iRowSpace = 14;

                    Console.WriteLine("Led发送=" + strText);

                    if (!EQ2008_DLL.User_RealtimeSendText(EQ2008_DLL.g_iCardNum_small, iX, iY, iW, iH, strText, ref FontInfo))
                    {
                        Console.WriteLine("发送实时文本失败！");
                    }
                    else
                    {
                        Console.WriteLine("发送实时文本成功！");
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                logger.Debug("发送LED异常" + ex.ToString(), ex);
                Console.WriteLine("发送LED异常" + ex.ToString());
            }
            finally
            {
                if (!EQ2008_DLL.User_RealtimeDisConnect(EQ2008_DLL.g_iCardNum_small))
                {
                    Console.WriteLine("关闭实时通信失败！");
                }
                else
                {
                    Console.WriteLine("关闭实时通信成功！");
                }
            }
        }

        private void LedContentFrame(ZH_ST_ZDCZB zdczb)
        {
            try
            {
                //VehicleInfo.DisplayTime = DateTime.Now;
                //VehicleInfo.DisplayFlag = true;

                if (DisFlag == 3)
                {
                    DisInfo[0] = DisInfo[1];
                    DisInfo[1] = DisInfo[2];
                    DisFlag = 2;
                }
                DisInfo[DisFlag] = zdczb.CPH;
                DisFlag++;

                SendLedRealtime(DisInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine("组合Led发送数据异常" + ex.ToString());
            }
        }

        #endregion

        #region 初始化仪表
        /// <summary>
        /// 初始化仪表
        /// </summary>
        private void InitCZY()
        {
            serialPortCZY.PortName = SysSet.CZYPort;
            serialPortCZY.BaudRate = SysSet.CZYBaudRate;
            serialPortCZY.Parity = SysSet.CZYParity;
            serialPortCZY.DataBits = SysSet.CZYDataBit;
            serialPortCZY.StopBits = SysSet.CZYStopBits;

            try
            {
                if (serialPortCZY.IsOpen)
                {
                    serialPortCZY.Close();
                }
                serialPortCZY.Open();
                ShowStateInfo("称重仪 " + serialPortCZY.PortName + " 串口打开成功", Color.Black);
            }
            catch (Exception ex)
            {
                logger.Debug(ex.ToString());
                ShowStateInfo("称重仪 " + serialPortCZY.PortName + " 串口打开失败", Color.Red);
            }
        }
        #endregion

        #region 初始化牌识相机

        /*
        private bool Init_NET_DVR()
        {
            bool m_bInitSDK = HCNetSDK.NET_DVR_Init();
            return m_bInitSDK;
        }

        /// <summary>
        /// 初始化牌识相机
        /// </summary>
        private void InitCPSB()
        {
            if (Init_NET_DVR())
            {
                logger.Info("海康车牌自动识别系统初始化成功");
            }
            else
            {
                logger.Info("海康车牌自动识别系统初始化失败");
                ShowStateInfo("车牌自动识别系统初始化失败", Color.Red);
                return;
            }

            for (int i = 0; i < SysSet.CpsbList.Count; i++)
            {
                m_lUserID[i] = HCNetSDK.NET_DVR_Login_V30(SysSet.CpsbList[i].IP, SysSet.CpsbList[i].NetPort,
                                                        SysSet.CpsbList[i].User, SysSet.CpsbList[i].Pwd,
                                                        ref m_struDeviceInfo[i]);
                if (m_lUserID[i] == -2)
                {
                    logger.Info("第" + (i + 1) + "车道 车牌自动识别系统配置信息为空");
                    ShowStateInfo("第" + (i + 1) + "车道 车牌自动识别系统配置信息为空！", Color.Red);
                }
                else if (m_lUserID[i] == -1)
                {
                    logger.Info("第" + (i + 1) + "车道 车牌自动识别系统登录失败。");
                    ShowStateInfo("第" + (i + 1) + "车道 车牌自动识别系统登录失败！", Color.Red);
                }
                else
                {
                    logger.Info("第" + (i + 1) + "车道 车牌自动识别系统登录成功。ID=" + m_lUserID[i]);
                    ShowStateInfo("第" + (i + 1) + "车道 车牌自动识别系统登录成功！ID=" + m_lUserID[i], Color.Black);
                }
            }

            AlarMSSGCallback_double();
        }

        private void AlarMSSGCallback_double()
        {
            for (int i = 0; i < SysSet.CpsbList.Count; i++)
            {
                m_lFortifyHandle[i] = HCNetSDK.NET_DVR_SetupAlarmChan_V30(m_lUserID[i]);
                if (m_lFortifyHandle[i] != -1)
                {
                    logger.Info("第" + (i + 1) + "车道 车牌自动识别系统布防成功。");
                    ShowStateInfo("第" + (i + 1) + "车道 车牌自动识别系统布防成功！", Color.Black);
                }
                else
                {
                    logger.Info("第" + (i + 1) + "车道车牌自动识别系统布防失败。");
                    ShowStateInfo("第" + (i + 1) + "车道 车牌自动识别系统布防失败！", Color.Red);
                }

                m_falarmData[i] = new HCNetSDK.MSGCallBack(MsgCallback);
                if (HCNetSDK.NET_DVR_SetDVRMessageCallBack_V30(m_falarmData[i], IntPtr.Zero))
                {
                    logger.Info("第" + (i + 1) + "车道 设置接收报警信息成功。");
                    ShowStateInfo("第" + (i + 1) + "车道 设置接收报警信息成功！", Color.Black);
                }
                else
                {
                    uint errorNo = HCNetSDK.NET_DVR_GetLastError();
                    logger.Info("第" + (i + 1) + "车道 设置接收报警信息失败！错误码为：" + i.ToString());
                    ShowStateInfo("第" + (i + 1) + "车道 设置接收报警信息失败！", Color.Red);
                }
            }
        }

        private void MsgCallback(int lCommand, ref HCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //MyDebugInfo AlarmInfo = new MyDebugInfo(DebugInfo);
            switch (lCommand)
            {
                case HCNetSDK.COMM_ALARM:
                    ProcessCommAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case HCNetSDK.COMM_ALARM_V30:
                    ProcessCommAlarm_V30(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case HCNetSDK.COMM_ALARM_RULE:
                    logger.Info("COMM_ALARM_RULE ");
                    break;
                case HCNetSDK.COMM_TRADEINFO:
                    logger.Info("COMM_TRADEINFO ");
                    break;
                case HCNetSDK.COMM_IPCCFG:
                    logger.Info("COMM_IPCCFG ");
                    break;
                case HCNetSDK.COMM_IPCCFG_V31:
                    logger.Info("COMM_IPCCFG_V31 ");
                    break;
                case HCNetSDK.COMM_UPLOAD_PLATE_RESULT:

                    //initVehicleInfo();
                    //HCNetSDK.NET_DVR_ALARMER alarmer = pAlarmer;

                    //ThreadPool.QueueUserWorkItem(x => SaveCpsb(alarmer, pAlarmInfo));

                    pAlarmer = SaveCpsb(pAlarmer, pAlarmInfo);
                    //this.BeginInvoke(AlarmInfo);
                    break;
                default:
                    break;
            }
        }

        private HCNetSDK.NET_DVR_ALARMER SaveCpsb(HCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo)
        {
            try
            {

                HCNetSDK.NET_VCA_PLATE_RESULT struPlateResult = new HCNetSDK.NET_VCA_PLATE_RESULT();
                struPlateResult = (HCNetSDK.NET_VCA_PLATE_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(HCNetSDK.NET_VCA_PLATE_RESULT));
                string str_temp = "";
                string str_cpys = "";
                string str_cph = "";
                string str_photo = "";
                string str_upPhoto = "";
                string str_plate = "";
                //string str_upPlate = "";
                short cd = 0;

                switch (struPlateResult.struPlateInfo.byColor)
                {
                    case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_BLACK_PLATE:
                        str_cpys = "黑";
                        break;
                    case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_BLUE_PLATE:
                        str_cpys = "蓝";
                        break;
                    case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_WHITE_PLATE:
                        str_cpys = "白";
                        break;
                    case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_YELLOW_PLATE:
                        str_cpys = "黄";
                        break;
                    default:
                        str_cpys = "无";
                        break;
                }
                //if (str_cpys == "蓝")//屏蔽蓝牌车辆
                //{
                //    return;
                //}

                for (int i = 0; i < struPlateResult.struPlateInfo.sLicense.Length; i++)
                    str_temp += struPlateResult.struPlateInfo.sLicense[i].ToString();
                if (str_temp.IndexOf("车牌") > 0)
                    str_cph = "未识别";
                else
                    str_cph = str_temp.Substring(1, 7);

                cd = struPlateResult.byDriveChan;
                DateTime recvTime = DateTime.Now;

                string DeviceIP = pAlarmer.sDeviceIP;

                Console.WriteLine("接收车牌号:" + str_temp + ",车道为：" + cd + ",IP=" + DeviceIP);
                logger.Info("接收车牌号:" + str_temp + ",车道为：" + cd + ",IP=" + DeviceIP);

                string str_id = DateTime.Now.ToString("yyyyMMddHHmmss");
                string path_li = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "\\");
                string directorySave = "";

                directorySave = dirPhoto + path_li;
                str_photo = directorySave + "\\" + DeviceIP + "-" + cd + str_id + "-Car.jpg";
                str_upPhoto = dirUpPhoto + "\\" + DeviceIP + "-" + cd + str_id + ".jpg";
                str_plate = directorySave + "\\" + DeviceIP + "-" + cd + str_id + "-P.jpg";


                if (DeviceIP == SysSet.CpsbList[0].IP)
                {//如果为正向车头抓拍，则启动正向车尾抓拍和侧面抓拍，此处移植到线圈收尾后进行抓拍
                    path_li = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "\\") + "\\Head";
                    directorySave = dirPhoto + path_li;
                    str_photo = directorySave + "\\" + DeviceIP + "-" + cd + str_id + "-Head.jpg";
                    str_upPhoto = dirUpPhoto + "\\" + DeviceIP + "-" + cd + str_id + ".jpg";
                    str_plate = directorySave + "\\" + DeviceIP + "-" + cd + str_id + "-Head_P.jpg";

                    //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(1, struPlateResult.byPicNum, true)));
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, struPlateResult.byPicNum, false)));

                    #region 车牌数据放入缓存 
                    for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
                    {
                        if (!cpsbResHead[i].CPSB_Flag)
                        {
                            double totalSeconds = (DateTime.Now - cpsbResHead[i].CPSB_Time).TotalSeconds;
                            if (totalSeconds > 8)
                            {
                                //Console.WriteLine("i=" + i + ",过时8s,原车牌" + cpsbRes[i].CPH + "新车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                InitCpsbRes(i, 0);

                                cpsbResHead[i].CPH = str_cph;
                                cpsbResHead[i].CPYS = str_cpys;
                                cpsbResHead[i].CD_GDW = cd;
                                cpsbResHead[i].photoPath = str_photo;
                                cpsbResHead[i].CPSB_Time = recvTime;
                                cpsbResHead[i].Plate = str_plate;
                                cpsbResHead[i].CPSB_Flag = false;
                                //num = i;
                                cpsbResHead[i].DeviceIP = DeviceIP;

                                break;
                            }
                        }
                        else if (cpsbResHead[i].CPSB_Flag)
                        {
                            cpsbResHead[i].CPH = str_cph;
                            cpsbResHead[i].CPYS = str_cpys;
                            cpsbResHead[i].CD_GDW = cd;
                            cpsbResHead[i].photoPath = str_photo;
                            cpsbResHead[i].CPSB_Time = recvTime;
                            cpsbResHead[i].Plate = str_plate;
                            cpsbResHead[i].CPSB_Flag = false;
                            cpsbResHead[i].DeviceIP = DeviceIP;

                            //Console.WriteLine("i=" + i + ", 车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                            break;
                        }
                    }
                    #endregion
                }
                else if (DeviceIP == SysSet.CpsbList[3].IP)
                {//如果为逆向车头抓拍，则启动逆向车尾抓拍
                    path_li += "\\NX";
                    directorySave = dirPhoto + path_li;
                    str_photo = directorySave + "\\" + DeviceIP + "-" + cd + str_id + "-Car.jpg";
                    str_upPhoto = dirUpPhoto + "\\" + DeviceIP + "-" + cd + str_id + ".jpg";
                    str_plate = directorySave + "\\" + DeviceIP + "-" + cd + str_id + "-P.jpg";

                    //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(4, struPlateResult.byPicNum, true)));
                    //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(5, struPlateResult.byPicNum, false)));

                    NXtable.CPH = str_cph;
                    NXtable.CPTX = str_photo;
                    NXtable.JCSJ = recvTime;

                    Console.WriteLine("逆行车辆=" + str_cph);
                    StartManuanSnap(4, struPlateResult.byPicNum, true);
                    StartManuanSnap(5, struPlateResult.byPicNum, false);


                    this.Invoke(new MethodInvoker(delegate
                    {
                        ShowDataNX(NXtable);
                    }));

                    DBInsertNX(NXtable);

                }

                #region 存贮图片


                if (!Directory.Exists(directorySave))
                    Directory.CreateDirectory(directorySave);

                //var query = from p in cpsbRes
                //            where p.CPSB_Flag == false && p.DeviceIP == DeviceIP
                //                && (DateTime.Now - p.CPSB_Time).TotalMilliseconds < SysSet.PsInterverTime
                //            select p;
                //if (query.ToList().Count() > 0)
                //{//如果存在同一个牌识相机返回的数据间隔小于指定时间间隔的，本次数据丢弃不缓存 
                //    Console.WriteLine("接收车牌号:" + str_temp + ",车道为：" + cd + ",已抓拍，本次抛弃");
                //    return;
                //}  

                if (struPlateResult.byResultType == 1 && struPlateResult.dwPicLen != 0)
                {
                    uint dwJpegSize = struPlateResult.dwPicLen;

                    // FileStream fs = new FileStream(str_photo, FileMode.Create);
                    FileStream fs = new FileStream(str_photo, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true);

                    byte[] by = new byte[dwJpegSize];
                    Marshal.Copy(struPlateResult.pBuffer1, by, 0, (int)dwJpegSize);
                    //fs.Write(by, 0, (int)dwJpegSize);
                    //fs.Close();
                    fs.BeginWrite(by, 0, (int)dwJpegSize, new AsyncCallback(AsyncCallbackWrite), fs);

                    //if (struPlateResult.dwPicPlateLen != 0)
                    //{
                    //    fs = new FileStream(str_plate, FileMode.Create);
                    //    byte[] plate = new byte[struPlateResult.dwPicPlateLen];
                    //    Marshal.Copy(struPlateResult.pBuffer2, plate, 0, (int)struPlateResult.dwPicPlateLen);
                    //    //fs.Write(plate, 0, (int)struPlateResult.dwPicPlateLen);
                    //    fs.BeginWrite(plate, 0, (int)struPlateResult.dwPicPlateLen, new AsyncCallback(AsyncCallbackWrite), fs);
                    //    fs.Close();
                    //    //Marshal.FreeHGlobal(struPlateResult.pBuffer2);
                    //}


                }
                else if (struPlateResult.byResultType == 0 && struPlateResult.dwVideoLen != 0 && struPlateResult.dwVideoLen != 0xffffffff)
                {
                    // FileStream fs = new FileStream(str_photo, FileMode.Create);
                    FileStream fs = new FileStream(str_photo, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true);

                    byte[] by = new byte[struPlateResult.dwVideoLen];
                    Marshal.Copy(struPlateResult.pBuffer1, by, 0, (int)struPlateResult.dwVideoLen);
                    //fs.Write(by, 0, (int)struPlateResult.dwVideoLen);
                    //fs.Dispose();
                    //fs.Close();
                    fs.BeginWrite(by, 0, (int)struPlateResult.dwVideoLen, new AsyncCallback(AsyncCallbackWrite), fs);

                    //Marshal.FreeHGlobal(struPlateResult.pBuffer1);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger.Debug(ex.ToString());
            }
            return pAlarmer;
        }

        private static void AsyncCallbackWrite(IAsyncResult iar)
        {
            using (FileStream fs = (FileStream)iar.AsyncState)
            {
                fs.EndWrite(iar);
                //  Console.WriteLine("异步写入结束");
                //if (fs.Name.Contains("-Car"))
                //{
                //    File.Copy(fs.Name, dirUpPhoto + "\\" + fs.Name.Substring(fs.Name.LastIndexOf("\\")), true);
                //}
            }
        }

        private void ProcessCommAlarm(ref HCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            HCNetSDK.NET_DVR_ALARMINFO struAlarmInfo = new HCNetSDK.NET_DVR_ALARMINFO();

            struAlarmInfo = (HCNetSDK.NET_DVR_ALARMINFO)Marshal.PtrToStructure(pAlarmInfo, typeof(HCNetSDK.NET_DVR_ALARMINFO));

            string str;
            switch (struAlarmInfo.dwAlarmType)
            {
                case 0:
                    logger.Info("sensor alarm");
                    break;
                case 1:
                    logger.Info("hard disk full");
                    break;
                case 2:
                    logger.Info("video lost");
                    break;
                case 3:
                    str = "";
                    str += pAlarmer.sDeviceIP;
                    str += " motion detection";
                    logger.Info(str);
                    //m_bJpegCapture = true;
                    break;
                case 4:
                    logger.Info("hard disk unformatted");
                    break;
                case 5:
                    logger.Info("hard disk error");
                    break;
                case 6:
                    logger.Info("tampering detection");
                    break;
                case 7:
                    logger.Info("unmatched video output standard");
                    break;
                case 8:
                    logger.Info("illegal operation");
                    break;
                default:
                    logger.Info("Unknow alarm");
                    break;
            }
        }

        private static void ProcessCommAlarm_V30(ref HCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            HCNetSDK.NET_DVR_ALARMINFO_V30 struAlarmInfoV30 = new HCNetSDK.NET_DVR_ALARMINFO_V30();

            struAlarmInfoV30 = (HCNetSDK.NET_DVR_ALARMINFO_V30)Marshal.PtrToStructure(pAlarmInfo, typeof(HCNetSDK.NET_DVR_ALARMINFO_V30));

            string str;
            switch (struAlarmInfoV30.dwAlarmType)
            {
                case 0:
                    logger.Info("sensor alarm");
                    break;
                case 1:
                    logger.Info("hard disk full");
                    break;
                case 2:
                    logger.Info("video lost");
                    break;
                case 3:
                    str = "";
                    str += pAlarmer.sDeviceIP;
                    str += " motion detection";
                    logger.Info(str);
                    break;
                case 4:
                    logger.Info("hard disk unformatted");
                    break;
                case 5:
                    logger.Info("hard disk error");
                    break;
                case 6:
                    logger.Info("tampering detection");
                    break;
                case 7:
                    logger.Info("unmatched video output standard");
                    break;
                case 8:
                    logger.Info("illegal operation");
                    break;
                case 9:
                    logger.Info("videl Signal abnormal");
                    break;
                case 10:
                    logger.Info("record abnormal");
                    break;
                default:
                    logger.Info("Unknow alarm");
                    break;
            }

        }
        */

        /// <summary>
        /// 清除指定索引位置上的牌识结果
        /// </summary>
        /// <param name="index">索引</param>
        /// <param name="which">清除车头0，车尾1，,侧面2识别结果</param>
        private void InitCpsbRes(int index, int which)
        {
            switch (which)
            {
                case 0:
                    cpsbResHead[index].CPH = "无车牌";
                    cpsbResHead[index].CPYS = "无";
                    cpsbResHead[index].CD_GDW = 0;
                    cpsbResHead[index].photoPath = "";
                    cpsbResHead[index].CPSB_Time = DateTime.Now;
                    cpsbResHead[index].Plate = "";
                    cpsbResHead[index].DeviceIP = "";

                    cpsbResHead[index].CPSB_Flag = true;
                    break;
                case 1:
                    cpsbResTail[index].CPH = "无车牌";
                    cpsbResTail[index].CPYS = "无";
                    cpsbResTail[index].CD_GDW = 0;
                    cpsbResTail[index].photoPath = "";
                    cpsbResTail[index].CPSB_Time = DateTime.Now;
                    cpsbResTail[index].Plate = "";
                    cpsbResTail[index].DeviceIP = "";

                    cpsbResTail[index].CPSB_Flag = true;
                    break;
                case 2:
                    cpsbResSide[index].CPH = "无车牌";
                    cpsbResSide[index].CPYS = "无";
                    cpsbResSide[index].CD_GDW = 0;
                    cpsbResSide[index].photoPath = "";
                    cpsbResSide[index].CPSB_Time = DateTime.Now;
                    cpsbResSide[index].Plate = "";
                    cpsbResSide[index].DeviceIP = "";

                    cpsbResSide[index].CPSB_Flag = true;
                    break;
            }
        }

        /// <summary>
        /// 按照牌识数组中的时间字段初始化数组
        /// </summary>
        /// <param name="dt"></param>
        private void InitCpsbRes(DateTime dt)
        {
            for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
            {
                if (cpsbResHead[i].CPSB_Time.CompareTo(dt) == 0)
                {
                    InitCpsbRes(i, 0);
                }
            }
        }



        #endregion

        #region 新版报警布防

        private void InitCPSB()
        {
            InitNetDVR();
            for (int i = 0; i < 4; i++)
            {
                m_lRealHandle[i] = -1;
            }

            string DVRIPAddress, DVRUserName, DVRPassword;
            Int16 DVRPortNumber;
            int userID = -1;
            //bool m_bInitSDK = HCNetSDK.NET_DVR_Init();
            //if (m_bInitSDK == false)
            //{
            //    MessageBox.Show("NET_DVR_Init error!");
            //    return;
            //}

            for (int i = 0; i < SysSet.CpsbList.Count; i++)
            {
                DVRIPAddress = SysSet.CpsbList[i].IP;
                DVRPortNumber = (Int16)SysSet.CpsbList[i].NetPort;
                DVRUserName = SysSet.CpsbList[i].User;
                DVRPassword = SysSet.CpsbList[i].Pwd;
                HCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new HCNetSDK.NET_DVR_DEVICEINFO_V30();

                //登录设备 Login the device
                userID = HCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (userID < 0)
                {
                    iLastErr = HCNetSDK.NET_DVR_GetLastError();

                    ShowStateInfo("车牌识别相机 【" + DVRIPAddress + "】 登录失败，错误号=" + iLastErr, Color.Red);
                    logger.Info("车牌识别相机 【" + DVRIPAddress + "】 登录失败，错误号=" + iLastErr);
                    continue;
                }
                else
                {
                    //登录成功  
                    m_lUserIDList.Add(userID);
                    iDeviceNumber++;

                    if (i == 1 || i == 2 || i == 4 || i == 5)
                    {//预览使用
                        m_lUserIDListPreview.Add(userID);
                    }

                    ShowStateInfo("车牌识别相机 【" + DVRIPAddress + "】 登录成功,ID=" + userID, Color.Black);
                    logger.Info("车牌识别相机 【" + DVRIPAddress + "】 登录成功,ID=" + userID);
                }
            }

            //开始布防
            SetAlarm();
            //打开车尾和侧面预览，准备抓图
            //OpenPreview();
        }

        private void SetAlarm()
        {
            HCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam = new HCNetSDK.NET_DVR_SETUPALARM_PARAM();
            struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
            struAlarmParam.byAlarmInfoType = 1;//智能交通设备有效

            for (int i = 0; i < iDeviceNumber; i++)
            {
                m_lAlarmHandle[m_lUserIDList[i]] = HCNetSDK.NET_DVR_SetupAlarmChan_V41(m_lUserIDList[i], ref struAlarmParam);
                if (m_lAlarmHandle[m_lUserIDList[i]] < 0)
                {
                    iLastErr = HCNetSDK.NET_DVR_GetLastError();

                    ShowStateInfo("第" + (i + 1) + "车道抓拍相机布防失败，错误号：" + iLastErr, Color.Red);
                    logger.Info("第" + (i + 1) + "车道抓拍相机布防失败，错误号：" + iLastErr);
                }
                else
                {
                    ShowStateInfo("第" + (i + 1) + "车道抓拍相机布防成功", Color.Black);
                    logger.Info("第" + (i + 1) + "车道抓拍相机布防成功");
                }
            }
        }

        private void CloseAlarm()
        {
            for (int i = 0; i < iDeviceNumber; i++)
            {
                if (m_lAlarmHandle[m_lUserIDList[i]] >= 0)
                {
                    if (!HCNetSDK.NET_DVR_CloseAlarmChan_V30(m_lAlarmHandle[m_lUserIDList[i]]))
                    {
                        iLastErr = HCNetSDK.NET_DVR_GetLastError();

                        ShowStateInfo("第" + (i + 1) + "车道抓拍相机撤防失败，错误号：" + iLastErr, Color.Red);
                        logger.Info("第" + (i + 1) + "车道抓拍相机撤防失败，错误号：" + iLastErr);
                    }
                    else
                    {
                        ShowStateInfo("第" + (i + 1) + "车道抓拍相机未布防", Color.Black);
                        logger.Info("第" + (i + 1) + "车道抓拍相机未布防");

                        m_lAlarmHandle[m_lUserIDList[i]] = -1;
                    }
                }
                else
                {
                    ShowStateInfo("第" + (i + 1) + "车道抓拍相机未布防", Color.Black);
                    logger.Info("第" + (i + 1) + "车道抓拍相机未布防");
                }
            }
        }

        private void InitNetDVR()
        {
            bool m_bInitSDK = HCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                HCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
                for (int i = 0; i < 200; i++)
                {
                    m_lAlarmHandle[i] = -1;
                }

                //设置报警回调函数
                m_falarmData = new HCNetSDK.MSGCallBack(MsgCallback);
                HCNetSDK.NET_DVR_SetDVRMessageCallBack_V30(m_falarmData, IntPtr.Zero);
            }
        }

        public void MsgCallback(int lCommand, ref HCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            switch (lCommand)
            {
                case HCNetSDK.COMM_ALARM: //(DS-8000老设备)移动侦测、视频丢失、遮挡、IO信号量等报警信息
                    //ProcessCommAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case HCNetSDK.COMM_ALARM_V30://移动侦测、视频丢失、遮挡、IO信号量等报警信息
                    // ProcessCommAlarm_V30(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case HCNetSDK.COMM_ALARM_RULE://进出区域、入侵、徘徊、人员聚集等行为分析报警信息
                    // ProcessCommAlarm_RULE(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case HCNetSDK.COMM_UPLOAD_PLATE_RESULT://交通抓拍结果上传(老报警信息类型)
                    ProcessCommAlarm_Plate(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                case HCNetSDK.COMM_ITS_PLATE_RESULT://交通抓拍结果上传(新报警信息类型)
                    ProcessCommAlarm_ITSPlate(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                default:
                    break;
            }
        }

        public void ProcessCommAlarm(ref HCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            HCNetSDK.NET_DVR_ALARMINFO struAlarmInfo = new HCNetSDK.NET_DVR_ALARMINFO();

            struAlarmInfo = (HCNetSDK.NET_DVR_ALARMINFO)Marshal.PtrToStructure(pAlarmInfo, typeof(HCNetSDK.NET_DVR_ALARMINFO));

            string strIP = pAlarmer.sDeviceIP;
            string stringAlarm = "";
            int i = 0;

            switch (struAlarmInfo.dwAlarmType)
            {
                case 0:
                    stringAlarm = "信号量报警，报警报警输入口：" + struAlarmInfo.dwAlarmInputNumber + "，触发录像通道：";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM; i++)
                    {
                        if (struAlarmInfo.dwAlarmRelateChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 1:
                    stringAlarm = "硬盘满，报警硬盘号：";
                    for (i = 0; i < HCNetSDK.MAX_DISKNUM; i++)
                    {
                        if (struAlarmInfo.dwDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 2:
                    stringAlarm = "信号丢失，报警通道：";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM; i++)
                    {
                        if (struAlarmInfo.dwChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 3:
                    stringAlarm = "移动侦测，报警通道：";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM; i++)
                    {
                        if (struAlarmInfo.dwChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 4:
                    stringAlarm = "硬盘未格式化，报警硬盘号：";
                    for (i = 0; i < HCNetSDK.MAX_DISKNUM; i++)
                    {
                        if (struAlarmInfo.dwDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 5:
                    stringAlarm = "读写硬盘出错，报警硬盘号：";
                    for (i = 0; i < HCNetSDK.MAX_DISKNUM; i++)
                    {
                        if (struAlarmInfo.dwDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 6:
                    stringAlarm = "遮挡报警，报警通道：";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM; i++)
                    {
                        if (struAlarmInfo.dwChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 7:
                    stringAlarm = "制式不匹配，报警通道";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM; i++)
                    {
                        if (struAlarmInfo.dwChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 8:
                    stringAlarm = "非法访问";
                    break;
                default:
                    stringAlarm = "其他未知报警信息";
                    break;
            }
        }

        private void ProcessCommAlarm_V30(ref HCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {

            HCNetSDK.NET_DVR_ALARMINFO_V30 struAlarmInfoV30 = new HCNetSDK.NET_DVR_ALARMINFO_V30();

            struAlarmInfoV30 = (HCNetSDK.NET_DVR_ALARMINFO_V30)Marshal.PtrToStructure(pAlarmInfo, typeof(HCNetSDK.NET_DVR_ALARMINFO_V30));

            string strIP = pAlarmer.sDeviceIP;
            string stringAlarm = "";
            int i;

            switch (struAlarmInfoV30.dwAlarmType)
            {
                case 0:
                    stringAlarm = "信号量报警，报警报警输入口：" + struAlarmInfoV30.dwAlarmInputNumber + "，触发录像通道：";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byAlarmRelateChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + "\\";
                        }
                    }
                    break;
                case 1:
                    stringAlarm = "硬盘满，报警硬盘号：";
                    for (i = 0; i < HCNetSDK.MAX_DISKNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " ";
                        }
                    }
                    break;
                case 2:
                    stringAlarm = "信号丢失，报警通道：";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 3:
                    stringAlarm = "移动侦测，报警通道：";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 4:
                    stringAlarm = "硬盘未格式化，报警硬盘号：";
                    for (i = 0; i < HCNetSDK.MAX_DISKNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 5:
                    stringAlarm = "读写硬盘出错，报警硬盘号：";
                    for (i = 0; i < HCNetSDK.MAX_DISKNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byDiskNumber[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 6:
                    stringAlarm = "遮挡报警，报警通道：";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 7:
                    stringAlarm = "制式不匹配，报警通道";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 8:
                    stringAlarm = "非法访问";
                    break;
                case 9:
                    stringAlarm = "视频信号异常，报警通道";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 10:
                    stringAlarm = "录像/抓图异常，报警通道";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 11:
                    stringAlarm = "智能场景变化，报警通道";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 12:
                    stringAlarm = "阵列异常";
                    break;
                case 13:
                    stringAlarm = "前端/录像分辨率不匹配，报警通道";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                case 15:
                    stringAlarm = "智能侦测，报警通道";
                    for (i = 0; i < HCNetSDK.MAX_CHANNUM_V30; i++)
                    {
                        if (struAlarmInfoV30.byChannel[i] == 1)
                        {
                            stringAlarm += (i + 1) + " \\ ";
                        }
                    }
                    break;
                default:
                    stringAlarm = "其他未知报警信息";
                    break;
            }
        }

        private void ProcessCommAlarm_RULE(ref HCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            HCNetSDK.NET_VCA_RULE_ALARM struRuleAlarmInfo = new HCNetSDK.NET_VCA_RULE_ALARM();
            struRuleAlarmInfo = (HCNetSDK.NET_VCA_RULE_ALARM)Marshal.PtrToStructure(pAlarmInfo, typeof(HCNetSDK.NET_VCA_RULE_ALARM));

            //报警信息
            string stringAlarm = "";
            uint dwSize = (uint)Marshal.SizeOf(struRuleAlarmInfo.struRuleInfo.uEventParam);

            switch (struRuleAlarmInfo.struRuleInfo.wEventTypeEx)
            {
                case (ushort)HCNetSDK.VCA_RULE_EVENT_TYPE_EX.ENUM_VCA_EVENT_TRAVERSE_PLANE:
                    IntPtr ptrTraverseInfo = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struRuleAlarmInfo.struRuleInfo.uEventParam, ptrTraverseInfo, false);
                    m_struTraversePlane = (HCNetSDK.NET_VCA_TRAVERSE_PLANE)Marshal.PtrToStructure(ptrTraverseInfo, typeof(HCNetSDK.NET_VCA_TRAVERSE_PLANE));
                    stringAlarm = "穿越警戒面，目标ID：" + struRuleAlarmInfo.struTargetInfo.dwID;
                    //警戒面边线起点坐标: (m_struTraversePlane.struPlaneBottom.struStart.fX, m_struTraversePlane.struPlaneBottom.struStart.fY)
                    //警戒面边线终点坐标: (m_struTraversePlane.struPlaneBottom.struEnd.fX, m_struTraversePlane.struPlaneBottom.struEnd.fY)
                    break;
                case (ushort)HCNetSDK.VCA_RULE_EVENT_TYPE_EX.ENUM_VCA_EVENT_ENTER_AREA:
                    IntPtr ptrEnterInfo = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struRuleAlarmInfo.struRuleInfo.uEventParam, ptrEnterInfo, false);
                    m_struVcaArea = (HCNetSDK.NET_VCA_AREA)Marshal.PtrToStructure(ptrEnterInfo, typeof(HCNetSDK.NET_VCA_AREA));
                    stringAlarm = "目标进入区域，目标ID：" + struRuleAlarmInfo.struTargetInfo.dwID;
                    //m_struVcaArea.struRegion 多边形区域坐标
                    break;
                case (ushort)HCNetSDK.VCA_RULE_EVENT_TYPE_EX.ENUM_VCA_EVENT_EXIT_AREA:
                    IntPtr ptrExitInfo = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struRuleAlarmInfo.struRuleInfo.uEventParam, ptrExitInfo, false);
                    m_struVcaArea = (HCNetSDK.NET_VCA_AREA)Marshal.PtrToStructure(ptrExitInfo, typeof(HCNetSDK.NET_VCA_AREA));
                    stringAlarm = "目标离开区域，目标ID：" + struRuleAlarmInfo.struTargetInfo.dwID;
                    //m_struVcaArea.struRegion 多边形区域坐标
                    break;
                case (ushort)HCNetSDK.VCA_RULE_EVENT_TYPE_EX.ENUM_VCA_EVENT_INTRUSION:
                    IntPtr ptrIntrusionInfo = Marshal.AllocHGlobal((Int32)dwSize);
                    Marshal.StructureToPtr(struRuleAlarmInfo.struRuleInfo.uEventParam, ptrIntrusionInfo, false);
                    m_struIntrusion = (HCNetSDK.NET_VCA_INTRUSION)Marshal.PtrToStructure(ptrIntrusionInfo, typeof(HCNetSDK.NET_VCA_INTRUSION));
                    stringAlarm = "周界入侵，目标ID：" + struRuleAlarmInfo.struTargetInfo.dwID;
                    //m_struIntrusion.struRegion 多边形区域坐标
                    break;
                default:
                    stringAlarm = "其他行为分析报警，目标ID：" + struRuleAlarmInfo.struTargetInfo.dwID;
                    break;
            }


            //报警图片保存
            if (struRuleAlarmInfo.dwPicDataLen > 0)
            {
                FileStream fs = new FileStream("行为分析报警抓图.jpg", FileMode.Create);
                int iLen = (int)struRuleAlarmInfo.dwPicDataLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struRuleAlarmInfo.pImage, by, 0, iLen);
                fs.Write(by, 0, iLen);
                fs.Close();
            }

            //报警时间：年月日时分秒
            string strTimeYear = ((struRuleAlarmInfo.dwAbsTime >> 26) + 2000).ToString();
            string strTimeMonth = ((struRuleAlarmInfo.dwAbsTime >> 22) & 15).ToString("d2");
            string strTimeDay = ((struRuleAlarmInfo.dwAbsTime >> 17) & 31).ToString("d2");
            string strTimeHour = ((struRuleAlarmInfo.dwAbsTime >> 12) & 31).ToString("d2");
            string strTimeMinute = ((struRuleAlarmInfo.dwAbsTime >> 6) & 63).ToString("d2");
            string strTimeSecond = ((struRuleAlarmInfo.dwAbsTime >> 0) & 63).ToString("d2");
            string strTime = strTimeYear + "-" + strTimeMonth + "-" + strTimeDay + " " + strTimeHour + ":" + strTimeMinute + ":" + strTimeSecond;

            //报警设备IP地址
            string strIP = struRuleAlarmInfo.struDevInfo.struDevIP.sIpV4;

        }

        private void ProcessCommAlarm_Plate(ref HCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            HCNetSDK.NET_DVR_PLATE_RESULT struPlateResultInfo = new HCNetSDK.NET_DVR_PLATE_RESULT();
            uint dwSize = (uint)Marshal.SizeOf(struPlateResultInfo);

            struPlateResultInfo = (HCNetSDK.NET_DVR_PLATE_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(HCNetSDK.NET_DVR_PLATE_RESULT));

            switch (struPlateResultInfo.struPlateInfo.byColor)
            {
                case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_BLACK_PLATE:
                    str_cpys = "黑";
                    break;
                case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_BLUE_PLATE:
                    str_cpys = "蓝";
                    break;
                case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_WHITE_PLATE:
                    str_cpys = "白";
                    break;
                case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_YELLOW_PLATE:
                    str_cpys = "黄";
                    break;
                default:
                    str_cpys = "无";
                    break;
            }

            str_cph = struPlateResultInfo.struPlateInfo.sLicense;
            cd = struPlateResultInfo.byDriveChan;

            DateTime recvTime = DateTime.Now;

            Console.WriteLine("接收车牌号:" + str_temp + ",车道为：" + cd + ",IP=" + pAlarmer.sDeviceIP);
            logger.Info("接收车牌号:" + str_temp + ",车道为：" + cd + ",IP=" + pAlarmer.sDeviceIP);

            string str_id = DateTime.Now.ToString("yyyyMMddHHmmss");
            string path_li = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "\\");
            string directorySave = "";

            directorySave = dirPhoto + path_li;
            str_photo = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-Car.jpg";
            str_upPhoto = dirUpPhoto + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + ".jpg";
            str_plate = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-P.jpg";


            if (pAlarmer.sDeviceIP == SysSet.CpsbList[0].IP)
            {//如果为正向车头抓拍，则启动正向车尾抓拍和侧面抓拍，此处移植到线圈收尾后进行抓拍
                path_li = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "\\") + "\\Head";
                directorySave = dirPhoto + path_li;
                str_photo = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-Head.jpg";
                str_upPhoto = dirUpPhoto + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + ".jpg";
                str_plate = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-Head_P.jpg";

                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(1, struPlateResult.byPicNum, true)));
                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, struPlateResult.byPicNum, false)));  

                #region 车牌数据放入缓存 
                for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
                {
                    if (!cpsbResHead[i].CPSB_Flag)
                    {
                        double totalSeconds = (DateTime.Now - cpsbResHead[i].CPSB_Time).TotalSeconds;
                        if (totalSeconds > 8)
                        {
                            //Console.WriteLine("i=" + i + ",过时8s,原车牌" + cpsbRes[i].CPH + "新车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                            InitCpsbRes(i, 0);

                            cpsbResHead[i].CPH = str_cph;
                            cpsbResHead[i].CPYS = str_cpys;
                            cpsbResHead[i].CD_GDW = cd;
                            cpsbResHead[i].photoPath = str_photo;
                            cpsbResHead[i].CPSB_Time = recvTime;
                            cpsbResHead[i].Plate = str_plate;
                            cpsbResHead[i].CPSB_Flag = false;
                            //num = i;
                            cpsbResHead[i].DeviceIP = pAlarmer.sDeviceIP;

                            break;
                        }
                    }
                    else if (cpsbResHead[i].CPSB_Flag)
                    {
                        cpsbResHead[i].CPH = str_cph;
                        cpsbResHead[i].CPYS = str_cpys;
                        cpsbResHead[i].CD_GDW = cd;
                        cpsbResHead[i].photoPath = str_photo;
                        cpsbResHead[i].CPSB_Time = recvTime;
                        cpsbResHead[i].Plate = str_plate;
                        cpsbResHead[i].CPSB_Flag = false;
                        cpsbResHead[i].DeviceIP = pAlarmer.sDeviceIP;

                        //Console.WriteLine("i=" + i + ", 车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                        break;
                    }
                }
                #endregion
            }
            else if (pAlarmer.sDeviceIP == SysSet.CpsbList[3].IP)
            {//如果为逆向车头抓拍，则启动逆向车尾抓拍
                path_li += "\\NX";
                directorySave = dirPhoto + path_li;
                str_photo = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-Car.jpg";
                str_upPhoto = dirUpPhoto + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + ".jpg";
                str_plate = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-P.jpg";

                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(4, struPlateResult.byPicNum, true)));
                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(5, struPlateResult.byPicNum, false)));

                NXtable.CPH = str_cph;
                NXtable.CPTX = str_photo;
                NXtable.JCSJ = recvTime;

                Console.WriteLine("逆行车辆=" + str_cph);
                //StartManuanSnap(4, struPlateResultInfo.byPicNum, true);
                //StartManuanSnap(5, struPlateResultInfo.byPicNum, false);


                this.Invoke(new MethodInvoker(delegate
                {
                    ShowDataNX(NXtable);
                }));

                DBInsertNX(NXtable);

            }

            if (!Directory.Exists(directorySave))
                Directory.CreateDirectory(directorySave);

            //保存抓拍图片
            if (struPlateResultInfo.byResultType == 1 && struPlateResultInfo.dwPicLen != 0)
            {
                FileStream fs = new FileStream(str_photo, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true);

                int iLen = (int)struPlateResultInfo.dwPicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struPlateResultInfo.pBuffer1, by, 0, iLen);
                fs.BeginWrite(by, 0, iLen, new AsyncCallback(AsyncCallbackWrite), fs);

                //fs.Write(by, 0, iLen);
                //fs.Close();
            }
            if (struPlateResultInfo.dwPicPlateLen != 0)
            {
                FileStream fs = new FileStream(str_plate, FileMode.Create);

                int iLen = (int)struPlateResultInfo.dwPicPlateLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struPlateResultInfo.pBuffer2, by, 0, iLen);

                fs.BeginWrite(by, 0, iLen, new AsyncCallback(AsyncCallbackWrite), fs);

                //fs.Write(by, 0, iLen);
                //fs.Close();
            }
            if (struPlateResultInfo.dwFarCarPicLen != 0)
            {
                FileStream fs = new FileStream(str_photo, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true);

                int iLen = (int)struPlateResultInfo.dwFarCarPicLen;
                byte[] by = new byte[iLen];
                Marshal.Copy(struPlateResultInfo.pBuffer5, by, 0, iLen);

                fs.BeginWrite(by, 0, iLen, new AsyncCallback(AsyncCallbackWrite), fs);
                //fs.Write(by, 0, iLen);
                //fs.Close();
            }

            //抓拍时间：年月日时分秒
            //string strTimeYear = System.Text.Encoding.UTF8.GetString(struPlateResultInfo.byAbsTime);

            //上传结果
            //string stringAlarm = "抓拍上传，" + "车牌：" + struPlateResultInfo.struPlateInfo.sLicense + "，车辆序号：" + struPlateResultInfo.struVehicleInfo.dwIndex; ;


        }

        private static void AsyncCallbackWrite(IAsyncResult iar)
        {
            using (FileStream fs = (FileStream)iar.AsyncState)
            {
                fs.EndWrite(iar);
                //Console.WriteLine("异步写入结束");
            }
        }

        private void ProcessCommAlarm_ITSPlate(ref HCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 1, true, false)));
            //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 1, false, false)));


            HCNetSDK.NET_ITS_PLATE_RESULT struITSPlateResult = new HCNetSDK.NET_ITS_PLATE_RESULT();
            uint dwSize = (uint)Marshal.SizeOf(struITSPlateResult);

            struITSPlateResult = (HCNetSDK.NET_ITS_PLATE_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(HCNetSDK.NET_ITS_PLATE_RESULT));

            switch (struITSPlateResult.struPlateInfo.byColor)
            {
                case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_BLACK_PLATE:
                    str_cpys = "黑";
                    break;
                case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_BLUE_PLATE:
                    str_cpys = "蓝";
                    break;
                case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_WHITE_PLATE:
                    str_cpys = "白";
                    break;
                case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_YELLOW_PLATE:
                    str_cpys = "黄";
                    break;
                default:
                    str_cpys = "无";
                    break;
            }

            str_cph = struITSPlateResult.struPlateInfo.sLicense;
            Console.WriteLine("牌识返回车牌号：" + str_cph);

            if (str_cph != "无车牌")
            {
                str_cph = str_cph.Substring(1, str_cph.Length - 1);
            }
            cd = struITSPlateResult.byDriveChan;

            DateTime recvTime = DateTime.Now;

            Console.WriteLine("接收车牌号:" + str_temp + ",车道为：" + cd + ",IP=" + pAlarmer.sDeviceIP);
            logger.Info("接收车牌号:" + str_temp + ",车道为：" + cd + ",IP=" + pAlarmer.sDeviceIP);

            string str_id = DateTime.Now.ToString("yyyyMMddHHmmss");
            string path_li = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "\\");
            string directorySave = "";

            directorySave = dirPhoto + path_li;
            str_photo = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-Car.jpg";
            str_upPhoto = dirUpPhoto + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + ".jpg";
            str_plate = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-P.jpg";


            if (pAlarmer.sDeviceIP == SysSet.CpsbList[0].IP)
            {//如果为正向车头抓拍，则启动正向车尾抓拍和侧面抓拍，此处移植到线圈收尾后进行抓拍
                path_li = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "\\") + "\\Head";
                directorySave = dirPhoto + path_li;
                str_photo = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-Head.jpg";
                str_upPhoto = dirUpPhoto + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + ".jpg";
                str_plate = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-Head_P.jpg";

                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(1, struPlateResult.byPicNum, true)));
                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, struPlateResult.byPicNum, false)));

                #region 车牌数据放入缓存 
                for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
                {
                    if (!cpsbResHead[i].CPSB_Flag)
                    {
                        double totalSeconds = (DateTime.Now - cpsbResHead[i].CPSB_Time).TotalSeconds;
                        if (totalSeconds > 8)
                        {
                            //Console.WriteLine("i=" + i + ",过时8s,原车牌" + cpsbRes[i].CPH + "新车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                            InitCpsbRes(i, 0);

                            cpsbResHead[i].CPH = str_cph;
                            cpsbResHead[i].CPYS = str_cpys;
                            cpsbResHead[i].CD_GDW = cd;
                            cpsbResHead[i].photoPath = str_photo;
                            cpsbResHead[i].CPSB_Time = recvTime;
                            cpsbResHead[i].Plate = str_plate;
                            cpsbResHead[i].CPSB_Flag = false;
                            //num = i;
                            cpsbResHead[i].DeviceIP = pAlarmer.sDeviceIP;

                            break;
                        }
                    }
                    else if (cpsbResHead[i].CPSB_Flag)
                    {
                        cpsbResHead[i].CPH = str_cph;
                        cpsbResHead[i].CPYS = str_cpys;
                        cpsbResHead[i].CD_GDW = cd;
                        cpsbResHead[i].photoPath = str_photo;
                        cpsbResHead[i].CPSB_Time = recvTime;
                        cpsbResHead[i].Plate = str_plate;
                        cpsbResHead[i].CPSB_Flag = false;
                        cpsbResHead[i].DeviceIP = pAlarmer.sDeviceIP;

                        //Console.WriteLine("i=" + i + ", 车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                        break;
                    }
                }
                #endregion
            }
            else if (pAlarmer.sDeviceIP == SysSet.CpsbList[3].IP)
            {//如果为逆向车头抓拍，则启动逆向车尾抓拍
                path_li += "\\NX";
                directorySave = dirPhoto + path_li;
                str_photo = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-Car.jpg";
                str_upPhoto = dirUpPhoto + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + ".jpg";
                str_plate = directorySave + "\\" + pAlarmer.sDeviceIP + "-" + cd + str_id + "-P.jpg";

                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(4, struPlateResult.byPicNum, true)));
                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(5, struPlateResult.byPicNum, false)));

                NXtable.CPH = str_cph;
                NXtable.CPTX = str_photo;
                NXtable.JCSJ = recvTime;
                NXtable.CMTX = "";
                NXtable.CPYS = "";
                NXtable.CWTX = "";
                NXtable.PLATE = "";
                NXtable.Video = "";

                Console.WriteLine("逆行车辆=" + str_cph);
                //StartManuanSnap(4, struITSPlateResult.byPicNo, true);
                //StartManuanSnap(5, struITSPlateResult.byPicNo, false);
                StartManualSnapNX(2, true, ref NXtable);
                StartManualSnapNX(3, false, ref NXtable);

                this.Invoke(new MethodInvoker(delegate
                {
                    ShowDataNX(NXtable);
                }));

                DBInsertNX(NXtable);

            }

            try
            {
                if (!Directory.Exists(directorySave))
                    Directory.CreateDirectory(directorySave);


                //保存抓拍图片
                for (int i = 0; i < struITSPlateResult.dwPicNum; i++)
                {
                    if (struITSPlateResult.struPicInfo[i].dwDataLen != 0)
                    {
                        string str = "D:/pic_type_" + struITSPlateResult.struPicInfo[i].byType + "_Num" + (i + 1) + ".jpg";

                        switch (struITSPlateResult.struPicInfo[i].byType)
                        {
                            case 0:
                                //车牌图片
                                str = str_plate;
                                break;
                            case 1:
                                //车头图片
                                str = str_photo;
                                break;
                            default:
                                break;
                        }

                        FileStream fs = new FileStream(str, FileMode.Create);
                        int iLen = (int)struITSPlateResult.struPicInfo[i].dwDataLen;
                        byte[] by = new byte[iLen];
                        Marshal.Copy(struITSPlateResult.struPicInfo[i].pBuffer, by, 0, iLen);
                        fs.BeginWrite(by, 0, iLen, new AsyncCallback(AsyncCallbackWrite), fs);
                        //fs.Write(by, 0, iLen);
                        //fs.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("保存图片异常" + ex.ToString());
                logger.Debug("保存图片异常" + ex.ToString());
            }
            ////报警设备IP地址
            //string strIP = pAlarmer.sDeviceIP;

            ////抓拍时间：年月日时分秒
            //string strTimeYear = string.Format("{0:D4}", struITSPlateResult.struSnapFirstPicTime.wYear) +
            //    string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byMonth) +
            //    string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byDay) + " "
            //    + string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byHour) + ":"
            //    + string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byMinute) + ":"
            //    + string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.bySecond) + ":"
            //    + string.Format("{0:D3}", struITSPlateResult.struSnapFirstPicTime.wMilliSec);

            ////上传结果
            //string stringAlarm = "抓拍上传，" + "车牌：" + struITSPlateResult.struPlateInfo.sLicense + "，车辆序号：" + struITSPlateResult.struVehicleInfo.dwIndex;


        }

        private void OpenPreview()
        {
            try
            {
                for (int i = 0; i < m_lUserIDListPreview.Count; i++)
                {

                    HCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new HCNetSDK.NET_DVR_PREVIEWINFO();
                    lpPreviewInfo.hPlayWnd = picBoxPreview[i].Handle;//预览窗口 live view window
                    lpPreviewInfo.lChannel = 1;//预览的设备通道 the device channel number
                    lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                    lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                    lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
                    lpPreviewInfo.dwDisplayBufNum = 15; //播放库显示缓冲区最大帧数

                    IntPtr pUser = IntPtr.Zero;//用户数据 user data  

                    //打开预览 Start live view 
                    m_lRealHandle[i] = HCNetSDK.NET_DVR_RealPlay_V40(m_lUserIDListPreview[i], ref lpPreviewInfo, null/*RealData*/, pUser);

                    if (m_lRealHandle[i] < 0)
                    {
                        iLastErr = HCNetSDK.NET_DVR_GetLastError();
                        string str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号 failed to start live view, and output the error code.
                        Console.WriteLine(str);
                        logger.Info(str);
                        return;
                    }
                    else
                    {
                        //预览成功
                        Console.WriteLine("预览成功 " + i);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("打开预览失败!" + ex.ToString());
                logger.Debug("打开预览失败!" + ex.ToString(), ex);
            }
        }

        #endregion 

        #region 手动抓拍

        /// <summary>
        /// 手动触发抓拍
        /// </summary>
        /// <param name="m_lUserID">登录设备返回的句柄</param>
        /// <param name="deviceIP">抓拍的设备IP地址</param>
        /// <param name="picNum">和车牌识别结果等匹配用的唯一值，同一辆车的三张图片此值一致</param>
        /// <param name="TailOrSide">true为车尾抓拍，false为侧面抓拍</param>
        private void ManualSnap(int m_lUserID, string deviceIP, int picNum, bool TailOrSide)
        {
            HCNetSDK.NET_DVR_MANUALSNAP m_struManualSnap = new HCNetSDK.NET_DVR_MANUALSNAP();
            HCNetSDK.NET_DVR_PLATE_RESULT m_struResult = new HCNetSDK.NET_DVR_PLATE_RESULT();

            IntPtr ptrCar = new IntPtr();
            IntPtr ptrPlate = new IntPtr();
            string str_cpys = "";
            string str_cph = "";
            string str_photo = "";
            string str_upPhoto = "";
            string str_plate = "";
            short cd = 0;

            try
            {
                uint nBuffersize = 2 * 1024 * 1024;
                ptrCar = Marshal.AllocHGlobal((int)nBuffersize);
                ptrPlate = Marshal.AllocHGlobal((int)nBuffersize);

                m_struResult.pBuffer1 = ptrCar;
                m_struResult.pBuffer2 = ptrPlate;

                Console.WriteLine("手动抓相机登录ID=" + m_lUserID);

                if (HCNetSDK.NET_DVR_ManualSnap(m_lUserID, ref m_struManualSnap, ref m_struResult))
                {
                    if (HCNetSDK.NET_DVR_GetLastError() == HCNetSDK.NET_DVR_NOSUPPORT)
                    {
                        logger.Info("该设备不支持手动抓拍");
                        Console.WriteLine("该设备不支持手动抓拍");
                        return;
                    }
                    //switch (m_struResult.struPlateInfo.byColor)
                    //{
                    //    case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_BLACK_PLATE:
                    //        str_cpys = "黑";
                    //        break;
                    //    case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_BLUE_PLATE:
                    //        str_cpys = "蓝";
                    //        break;
                    //    case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_WHITE_PLATE:
                    //        str_cpys = "白";
                    //        break;
                    //    case (byte)HCNetSDK.VCA_PLATE_COLOR.VCA_YELLOW_PLATE:
                    //        str_cpys = "黄";
                    //        break;
                    //    default:
                    //        str_cpys = "无";
                    //        break;
                    //}

                    StringBuilder sb = new StringBuilder();
                    //foreach (var item in m_struResult.struPlateInfo.sLicense)
                    //{
                    //    sb.Append(item);
                    //}
                    //str_cph = sb.ToString();
                    //str_cph = new string(m_struResult.struPlateInfo.sLicense);
                    //str_cph = str_cph.Replace("\0", string.Empty).Trim();

                    cd = m_struResult.byDriveChan;
                    //手动抓拍车尾和侧面时给的对应车头的车道号，用于匹配
                    cd = (short)picNum;

                    Console.WriteLine("手动抓拍，返回车牌号:" + str_cph + ",车道为：" + cd + ",IP=" + deviceIP + ",时间=" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));

                    //if (str_cph != "无车牌")
                    //{
                    //    str_cph = str_cph.Substring(1, str_cph.Length - 1);
                    //}

                    string str_id = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string path_li = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "\\");
                    string directorySave = "";

                    directorySave = dirPhoto + path_li;

                    if (TailOrSide == true)
                    {
                        directorySave += "\\Tail";

                        str_photo = directorySave + "\\" + deviceIP + "-" + cd + "-Tail.jpg";
                        str_upPhoto = dirUpPhoto + "\\" + deviceIP + "-" + cd + ".jpg";
                        str_plate = directorySave + "\\" + deviceIP + "-" + cd + "-Tail_P.jpg";
                    }
                    else
                    {
                        directorySave += "\\Side";

                        str_photo = directorySave + "\\" + deviceIP + "-" + cd + "-Side.jpg";
                        str_upPhoto = dirUpPhoto + "\\" + deviceIP + "-" + cd + ".jpg";
                        str_plate = directorySave + "\\" + deviceIP + "-" + cd + "-Side_P.jpg";
                    }




                    if (!Directory.Exists(directorySave))
                        Directory.CreateDirectory(directorySave);

                    if (deviceIP == SysSet.CpsbList[3].IP)
                    {//如果是逆行触发的抓拍，则直接赋值
                        if (TailOrSide == true)
                        {
                            NXtable.CWTX = str_photo;
                        }
                        else
                        {
                            NXtable.CMTX = str_photo;
                        }
                    }
                    else
                    {
                        if (TailOrSide == true)
                        {
                            #region 放置到车尾识别结果缓存中
                            for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
                            {
                                if (!cpsbResTail[i].CPSB_Flag)
                                {
                                    double totalSeconds = (DateTime.Now - cpsbResTail[i].CPSB_Time).TotalSeconds;
                                    if (totalSeconds > 8)
                                    {
                                        //Console.WriteLine("i=" + i + ",过时8s,原车牌" + cpsbRes[i].CPH + "新车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                        InitCpsbRes(i, 1);

                                        cpsbResTail[i].CPH = str_cph;
                                        cpsbResTail[i].CPYS = str_cpys;
                                        cpsbResTail[i].CD_GDW = cd;
                                        cpsbResTail[i].photoPath = str_photo;
                                        cpsbResTail[i].CPSB_Time = DateTime.Now;
                                        cpsbResTail[i].Plate = str_plate;
                                        cpsbResTail[i].CPSB_Flag = false;
                                        //num = i;
                                        cpsbResTail[i].DeviceIP = deviceIP;
                                        cpsbResTail[i].picNum = picNum;
                                        break;
                                    }
                                }
                                else if (cpsbResTail[i].CPSB_Flag)
                                {
                                    cpsbResTail[i].CPH = str_cph;
                                    cpsbResTail[i].CPYS = str_cpys;
                                    cpsbResTail[i].CD_GDW = cd;
                                    cpsbResTail[i].photoPath = str_photo;
                                    cpsbResTail[i].CPSB_Time = DateTime.Now;
                                    cpsbResTail[i].Plate = str_plate;
                                    cpsbResTail[i].CPSB_Flag = false;
                                    cpsbResTail[i].DeviceIP = deviceIP;
                                    cpsbResTail[i].picNum = picNum;
                                    //Console.WriteLine("i=" + i + ", 车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                    break;
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region 放置到侧面识别结果中
                            for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
                            {
                                if (!cpsbResSide[i].CPSB_Flag)
                                {
                                    double totalSeconds = (DateTime.Now - cpsbResSide[i].CPSB_Time).TotalSeconds;
                                    if (totalSeconds > 8)
                                    {
                                        //Console.WriteLine("i=" + i + ",过时8s,原车牌" + cpsbRes[i].CPH + "新车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                        InitCpsbRes(i, 2);

                                        cpsbResSide[i].CPH = str_cph;
                                        cpsbResSide[i].CPYS = str_cpys;
                                        cpsbResSide[i].CD_GDW = cd;
                                        cpsbResSide[i].photoPath = str_photo;
                                        cpsbResSide[i].CPSB_Time = DateTime.Now;
                                        cpsbResSide[i].Plate = str_plate;
                                        cpsbResSide[i].CPSB_Flag = false;
                                        //num = i;
                                        cpsbResSide[i].DeviceIP = deviceIP;
                                        cpsbResSide[i].picNum = picNum;
                                        break;
                                    }
                                }
                                else if (cpsbResTail[i].CPSB_Flag)
                                {
                                    cpsbResSide[i].CPH = str_cph;
                                    cpsbResSide[i].CPYS = str_cpys;
                                    cpsbResSide[i].CD_GDW = cd;
                                    cpsbResSide[i].photoPath = str_photo;
                                    cpsbResSide[i].CPSB_Time = DateTime.Now;
                                    cpsbResSide[i].Plate = str_plate;
                                    cpsbResSide[i].CPSB_Flag = false;
                                    cpsbResSide[i].DeviceIP = deviceIP;
                                    cpsbResSide[i].picNum = picNum;
                                    //Console.WriteLine("i=" + i + ", 车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                    break;
                                }
                            }
                            #endregion
                        }
                    }
                    if (m_struResult.byResultType == 1 && m_struResult.dwPicLen != 0)
                    {
                        uint dwJpegSize = m_struResult.dwPicLen;

                        // FileStream fs = new FileStream(str_photo, FileMode.Create);
                        FileStream fs = new FileStream(str_photo, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true);

                        byte[] by = new byte[dwJpegSize];
                        Marshal.Copy(m_struResult.pBuffer1, by, 0, (int)dwJpegSize);
                        //fs.Write(by, 0, (int)dwJpegSize);
                        //fs.Close();
                        fs.BeginWrite(by, 0, (int)dwJpegSize, new AsyncCallback(AsyncCallbackWrite), fs);

                        if (m_struResult.dwPicPlateLen != 0)
                        {
                            fs = new FileStream(str_plate, FileMode.Create);
                            byte[] plate = new byte[m_struResult.dwPicPlateLen];
                            Marshal.Copy(m_struResult.pBuffer2, plate, 0, (int)m_struResult.dwPicPlateLen);
                            fs.Write(plate, 0, (int)m_struResult.dwPicPlateLen);
                            fs.Close();
                            //Marshal.FreeHGlobal(struPlateResult.pBuffer2);
                        }
                    }
                    else if (m_struResult.byResultType == 0 && m_struResult.dwVideoLen != 0 && m_struResult.dwVideoLen != 0xffffffff)
                    {
                        // FileStream fs = new FileStream(str_photo, FileMode.Create);
                        FileStream fs = new FileStream(str_photo, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, true);

                        byte[] by = new byte[m_struResult.dwVideoLen];
                        Marshal.Copy(m_struResult.pBuffer1, by, 0, (int)m_struResult.dwVideoLen);
                        //fs.Write(by, 0, (int)struPlateResult.dwVideoLen);
                        //fs.Dispose();
                        //fs.Close();
                        fs.BeginWrite(by, 0, (int)m_struResult.dwVideoLen, new AsyncCallback(AsyncCallbackWrite), fs);

                        //Marshal.FreeHGlobal(struPlateResult.pBuffer1);
                    }
                }
                else
                {
                    int iErrorNum = (int)HCNetSDK.NET_DVR_GetLastError();
                    string s = " 网络触发抓拍失败!错误号: " + iErrorNum;//+ " 错误消息" + HCNetSDK.NET_DVR_GetErrorMsg(ref iErrorNum);
                    Console.WriteLine(s);
                    logger.Info(s);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("手动抓拍异常" + ex.ToString());
                logger.Debug("手动抓拍异常" + ex.ToString(), ex);
            }
            finally
            {
                Marshal.FreeHGlobal(ptrCar);
                Marshal.FreeHGlobal(ptrPlate);
            }
        }


        #endregion

        #region 网络触发抓拍
        private void NetSnap(int m_lUserID)
        {
            HCNetSDK.NET_DVR_SNAPCFG struSnapCfg = new HCNetSDK.NET_DVR_SNAPCFG();
            struSnapCfg.wIntervalTime = new ushort[4];
            struSnapCfg.dwSize = (uint)Marshal.SizeOf(struSnapCfg);
            struSnapCfg.byRelatedDriveWay = 1;
            struSnapCfg.bySnapTimes = 1;
            struSnapCfg.wSnapWaitTime = 100;
            struSnapCfg.wIntervalTime[0] = 100;

            bool bManualSnap = HCNetSDK.NET_DVR_ContinuousShoot(m_lUserID, ref struSnapCfg);

            if (!bManualSnap)
            {
                uint iLastErr = HCNetSDK.NET_DVR_GetLastError();
                string str = "NET_DVR_ContinuousShoot failed, error code= " + iLastErr;
                Console.WriteLine(str);
                logger.Info(str);
                return;
            }
        }

        #endregion

        #region 视频流抓图
        private void CaptureJpeg(int m_lUserID, bool TailOrSide, bool NX, int picNum)
        {

            try
            {


                string strphoto = "";
                short cd = 0;

                string str = "";
                HCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new HCNetSDK.NET_DVR_JPEGPARA();
                lpJpegPara.wPicQuality = 0; //图像质量 Image quality
                lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 0xff-Auto(使用当前码流分辨率) 
                                            //抓图分辨率需要设备支持，更多取值请参考SDK文档

                //JPEG抓图保存成文件 Capture a JPEG picture
                string sJpegPicFileName;
                sJpegPicFileName = "filetest.jpg";//图片保存路径和文件名 the path and file name to save

                string str_id = DateTime.Now.ToString("yyyyMMddHHmmss");
                string path_li = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "\\");
                string directorySave = "";

                directorySave = dirPhoto + path_li;

                if (TailOrSide == true)
                {
                    directorySave += "\\Tail";
                    strphoto = directorySave + "\\" + cd + str_id + "-Tail.jpg";

                    if (NX == true)
                    {
                        NXtable.CWTX = str_photo;
                    }
                }
                else
                {
                    directorySave += "\\Side";
                    strphoto = directorySave + "\\" + cd + str_id + "-Side.jpg";

                    if (NX == true)
                    {
                        NXtable.CMTX = str_photo;
                    }
                }
                if (NX == false)
                {//正向车头或者车尾
                    if (TailOrSide == true)
                    {
                        #region 放置到车尾识别结果缓存中
                        for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
                        {
                            if (!cpsbResTail[i].CPSB_Flag)
                            {
                                double totalSeconds = (DateTime.Now - cpsbResTail[i].CPSB_Time).TotalSeconds;
                                if (totalSeconds > 8)
                                {
                                    //Console.WriteLine("i=" + i + ",过时8s,原车牌" + cpsbRes[i].CPH + "新车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                    InitCpsbRes(i, 1);

                                    cpsbResTail[i].CPH = str_cph;
                                    cpsbResTail[i].CPYS = str_cpys;
                                    cpsbResTail[i].CD_GDW = cd;
                                    cpsbResTail[i].photoPath = str_photo;
                                    cpsbResTail[i].CPSB_Time = DateTime.Now;
                                    cpsbResTail[i].Plate = str_plate;
                                    cpsbResSide[i].picNum = picNum;
                                    cpsbResTail[i].CPSB_Flag = false;
                                    break;
                                }
                            }
                            else if (cpsbResTail[i].CPSB_Flag)
                            {
                                cpsbResTail[i].CPH = str_cph;
                                cpsbResTail[i].CPYS = str_cpys;
                                cpsbResTail[i].CD_GDW = cd;
                                cpsbResTail[i].photoPath = str_photo;
                                cpsbResTail[i].CPSB_Time = DateTime.Now;
                                cpsbResTail[i].Plate = str_plate;
                                cpsbResSide[i].picNum = picNum;
                                cpsbResTail[i].CPSB_Flag = false;
                                break;
                            }
                        }
                        #endregion
                    }
                    else
                    {
                        #region 放置到侧面识别结果中
                        for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
                        {
                            if (!cpsbResSide[i].CPSB_Flag)
                            {
                                double totalSeconds = (DateTime.Now - cpsbResSide[i].CPSB_Time).TotalSeconds;
                                if (totalSeconds > 8)
                                {
                                    //Console.WriteLine("i=" + i + ",过时8s,原车牌" + cpsbRes[i].CPH + "新车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                    InitCpsbRes(i, 2);

                                    cpsbResSide[i].CPH = str_cph;
                                    cpsbResSide[i].CPYS = str_cpys;
                                    cpsbResSide[i].CD_GDW = cd;
                                    cpsbResSide[i].photoPath = str_photo;
                                    cpsbResSide[i].CPSB_Time = DateTime.Now;
                                    cpsbResSide[i].Plate = str_plate;
                                    cpsbResSide[i].CPSB_Flag = false;
                                    //num = i;

                                    cpsbResSide[i].picNum = picNum;
                                    break;
                                }
                            }
                            else if (cpsbResTail[i].CPSB_Flag)
                            {
                                cpsbResSide[i].CPH = str_cph;
                                cpsbResSide[i].CPYS = str_cpys;
                                cpsbResSide[i].CD_GDW = cd;
                                cpsbResSide[i].photoPath = str_photo;
                                cpsbResSide[i].CPSB_Time = DateTime.Now;
                                cpsbResSide[i].Plate = str_plate;
                                cpsbResSide[i].CPSB_Flag = false;
                                cpsbResSide[i].picNum = picNum;
                                //Console.WriteLine("i=" + i + ", 车牌号:" + str_cph + ", 时间:" + recvTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                break;
                            }
                        }
                        #endregion
                    }
                }


                if (!Directory.Exists(directorySave))
                    Directory.CreateDirectory(directorySave);

                Console.WriteLine("开启截图：m_lUserID=" + m_lUserID);
                if (!HCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, 1, ref lpJpegPara, strphoto))
                {
                    iLastErr = HCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                    Console.WriteLine(str);
                    return;
                }
                else
                {
                    str = "NET_DVR_CaptureJPEGPicture succ and the saved file is " + strphoto;
                    Console.WriteLine(str);
                    logger.Info(str);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("开启截图" + ex);
                logger.Debug("开启截图", ex);
            }
        }
        #endregion

        #region 仪表数据接收事件

        private void serialPortCZY_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(15);

            int dataLen = serialPortCZY.BytesToRead;
            if (dataLen == 0)
            {
                return;
            }

            RecvCount += dataLen;

            byte[] data = new byte[dataLen];
            serialPortCZY.Read(data, 0, dataLen);

            for (int i = 0; i < dataLen; i++)
            {
                ConQueue.Enqueue(data[i]);
            }

            logger.Trace(Common.byteToHexStr(data));


            //this.Invoke(new MethodInvoker(delegate
            //{

            //}));


        }

        #endregion

        #region Listview添加数据显示
        private void ShowDataOnListview(ZH_ST_ZDCZB zh_st_zdczb)
        {
            if (listV_DateList.Items.Count > 500)
            {
                listV_DateList.Items.Clear();
            }

            ListViewItem lvi_New = new ListViewItem();
            lvi_New.SubItems.Add(zh_st_zdczb.CPH);
            lvi_New.SubItems.Add(zh_st_zdczb.CPYS);
            lvi_New.SubItems.Add(zh_st_zdczb.ZZ.ToString());
            lvi_New.SubItems.Add(zh_st_zdczb.ZS.ToString());
            lvi_New.SubItems.Add(zh_st_zdczb.CS.ToString("N"));
            lvi_New.SubItems.Add(zh_st_zdczb.FX);
            lvi_New.SubItems.Add(zh_st_zdczb.XZ.ToString());
            lvi_New.SubItems.Add(zh_st_zdczb.CXL.ToString());
            lvi_New.SubItems.Add(zh_st_zdczb.XHZL.ToString());
            lvi_New.SubItems.Add(zh_st_zdczb.CD.ToString());
            lvi_New.SubItems.Add(zh_st_zdczb.JCSJ.ToString());

            if (zh_st_zdczb.CPTX == "")
                lvi_New.SubItems.Add("0000");
            else
                lvi_New.SubItems.Add(zh_st_zdczb.CPTX);
            if (zh_st_zdczb.PLATE == "")
                lvi_New.SubItems.Add("0000");
            else
                lvi_New.SubItems.Add(zh_st_zdczb.PLATE);

            if (zh_st_zdczb.CXL > SysSet.MinCXBZ)
            {
                lvi_New.BackColor = Color.Red;
                lvi_New.ForeColor = Color.Black;
            }
            //else if (zh_st_zdczb.CXL > 0)
            //{
            //    lvi_New.BackColor = Color.Yellow;
            //    lvi_New.ForeColor = Color.Black;
            //}


            listV_DateList.Items.Insert(0, lvi_New);
            listV_DateList.SelectedItems.Clear();
            lvi_New.Selected = true;
            listV_DateList.Focus();

            switch (zh_st_zdczb.CD)
            {
                case 1:
                    if (zh_st_zdczb.CXL > SysSet.MinCXBZ)
                    {
                        lblCD1_CXL.ForeColor = Color.Red;
                        lblCD1_ZZ.ForeColor = Color.Red;
                    }
                    else
                    {
                        lblCD1_CXL.ForeColor = Color.Black;
                        lblCD1_ZZ.ForeColor = Color.Black;
                    }

                    txtCD1_CPH.Text = zh_st_zdczb.CPH;
                    lblCD1_ZZ.Text = (zh_st_zdczb.ZZ / (double)1000).ToString();
                    lblCD1_CPYS.Text = zh_st_zdczb.CPYS;
                    lblCD1_ZS.Text = zh_st_zdczb.ZS.ToString();
                    lblCD1_CS.Text = zh_st_zdczb.CS.ToString();
                    lblCD1_JCSJ.Text = zh_st_zdczb.JCSJ.ToString();
                    lblCD1_CXL.Text = zh_st_zdczb.CXL.ToString();

                    if (zh_st_zdczb.CPTX != "" && zh_st_zdczb.CPTX != "0000" && File.Exists(zh_st_zdczb.CPTX))
                    {
                        bmapCD1 = new Bitmap(zh_st_zdczb.CPTX);
                        pictureCD1.Image = bmapCD1;
                    }
                    else
                    {
                        pictureCD1.Image = null;
                    }
                    break;
                case 2:
                    if (zh_st_zdczb.CXL > SysSet.MinCXBZ)
                    {
                        lblCD2_CXL.ForeColor = Color.Red;
                        lblCD2_ZZ.ForeColor = Color.Red;
                    }
                    else
                    {
                        lblCD2_CXL.ForeColor = Color.Black;
                        lblCD2_ZZ.ForeColor = Color.Black;
                    }

                    txtCD2_CPH.Text = zh_st_zdczb.CPH;
                    lblCD2_ZZ.Text = (zh_st_zdczb.ZZ / (double)1000).ToString();
                    lblCD2_CPYS.Text = zh_st_zdczb.CPYS;
                    lblCD2_ZS.Text = zh_st_zdczb.ZS.ToString();
                    lblCD2_CS.Text = zh_st_zdczb.CS.ToString();
                    lblCD2_JCSJ.Text = zh_st_zdczb.JCSJ.ToString();
                    lblCD2_CXL.Text = zh_st_zdczb.CXL.ToString();

                    if (zh_st_zdczb.CPTX != "" && zh_st_zdczb.CPTX != "0000" && File.Exists(zh_st_zdczb.CPTX))
                    {
                        bmapCD2 = new Bitmap(zh_st_zdczb.CPTX);
                        pictureCD2.Image = bmapCD2;
                    }
                    else
                    {
                        pictureCD2.Image = null;
                    }
                    break;
                case 3:
                    if (zh_st_zdczb.CXL > SysSet.MinCXBZ)
                    {
                        lblCD3_CXL.ForeColor = Color.Red;
                        lblCD3_ZZ.ForeColor = Color.Red;
                    }
                    else
                    {
                        lblCD3_CXL.ForeColor = Color.Black;
                        lblCD3_ZZ.ForeColor = Color.Black;
                    }

                    txtCD3_CPH.Text = zh_st_zdczb.CPH;
                    lblCD3_ZZ.Text = (zh_st_zdczb.ZZ / (double)1000).ToString();
                    lblCD3_CPYS.Text = zh_st_zdczb.CPYS;
                    lblCD3_ZS.Text = zh_st_zdczb.ZS.ToString();
                    lblCD3_CS.Text = zh_st_zdczb.CS.ToString();
                    lblCD3_JCSJ.Text = zh_st_zdczb.JCSJ.ToString();
                    lblCD3_CXL.Text = zh_st_zdczb.CXL.ToString();

                    if (zh_st_zdczb.CPTX != "" && zh_st_zdczb.CPTX != "0000" && File.Exists(zh_st_zdczb.CPTX))
                    {
                        bmapCD3 = new Bitmap(zh_st_zdczb.CPTX);
                        pictureCD3.Image = bmapCD3;
                    }
                    else
                    {
                        pictureCD3.Image = null;
                    }
                    break;
                case 4:
                    //逆行车道
                    lblCD4_CXL.ForeColor = Color.Red;
                    lblCD4_ZZ.ForeColor = Color.Red;

                    txtCD4_CPH.Text = zh_st_zdczb.CPH;
                    lblCD4_ZZ.Text = (zh_st_zdczb.ZZ / (double)1000).ToString();
                    lblCD4_CPYS.Text = zh_st_zdczb.CPYS;
                    lblCD4_ZS.Text = zh_st_zdczb.ZS.ToString();
                    lblCD4_CS.Text = zh_st_zdczb.CS.ToString();
                    lblCD4_JCSJ.Text = zh_st_zdczb.JCSJ.ToString();
                    lblCD4_CXL.Text = zh_st_zdczb.CXL.ToString();

                    if (zh_st_zdczb.CPTX != "" && zh_st_zdczb.CPTX != "0000" && File.Exists(zh_st_zdczb.CPTX))
                    {
                        bmapCD4 = new Bitmap(zh_st_zdczb.CPTX);
                        pictureCD4.Image = bmapCD4;
                        bmapCD4.Save(dirUpPhoto + "\\" + zh_st_zdczb.CPTX.Substring(zh_st_zdczb.CPTX.LastIndexOf("\\")));
                    }
                    else
                    {
                        pictureCD4.Image = null;
                    }
                    break;
                default:
                    break;
            }
        }

        private void ShowDataNX(NXTable NXData)
        {
            ListViewItem lvi_New = new ListViewItem();
            lvi_New.SubItems.Add(NXData.CPH);
            lvi_New.SubItems.Add(NXData.CPYS);
            lvi_New.SubItems.Add("未知");
            lvi_New.SubItems.Add("未知");
            lvi_New.SubItems.Add("未知");
            lvi_New.SubItems.Add("逆行");
            lvi_New.SubItems.Add("未知");
            lvi_New.SubItems.Add("未知");
            lvi_New.SubItems.Add("未知");
            lvi_New.SubItems.Add("逆行道");
            lvi_New.SubItems.Add(NXData.JCSJ.ToString());

            if (NXData.CPTX == "")
                lvi_New.SubItems.Add("0000");
            else
                lvi_New.SubItems.Add(NXData.CPTX);
            if (NXData.PLATE == "")
                lvi_New.SubItems.Add("0000");
            else
                lvi_New.SubItems.Add(NXData.PLATE);


            lvi_New.BackColor = Color.Red;
            lvi_New.ForeColor = Color.Black;


            listV_DateList.Items.Insert(0, lvi_New);
            listV_DateList.SelectedItems.Clear();
            lvi_New.Selected = true;
            listV_DateList.Focus();

            lblCD4_CXL.ForeColor = Color.Red;
            lblCD4_ZZ.ForeColor = Color.Red;

            txtCD4_CPH.Text = NXData.CPH;
            lblCD4_ZZ.Text = "未知";
            lblCD4_CPYS.Text = NXData.CPYS;
            lblCD4_ZS.Text = "未知";
            lblCD4_CS.Text = "未知";
            lblCD4_JCSJ.Text = NXData.JCSJ.ToString();
            lblCD4_CXL.Text = "未知";

            if (NXData.CPTX != "" && NXData.CPTX != "0000" && File.Exists(NXData.CPTX))
            {
                bmapCD4 = new Bitmap(NXData.CPTX);
                pictureCD4.Image = bmapCD4;
            }
            else
            {
                pictureCD4.Image = null;
            }
        }

        #endregion

        #region 加载当天数据 

        private void LoadTodayData()
        {
            try
            {
                string str_sql = "select top 500 ID,CPH,CPYS,ZZ,ZS,CS,FX,XZ,CXL,XHZL,CD,JCSJ,CPTX,Plate from JZWRZS.dbo.ZH_ST_ZDCZB where CPH!='无车牌' and CPH!='未识别' order by JCSJ desc"; //and JCSJ>'2016-06-23 0:0:0'
                DataTable dt = DataBase.DbHelperSQL.Query(str_sql).Tables[0];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ListViewItem lvi_New = new ListViewItem();
                    lvi_New.SubItems.Add(dt.Rows[i]["CPH"].ToString());
                    lvi_New.SubItems.Add(dt.Rows[i]["CPYS"].ToString());
                    lvi_New.SubItems.Add(dt.Rows[i]["ZZ"].ToString());
                    lvi_New.SubItems.Add(dt.Rows[i]["ZS"].ToString());
                    lvi_New.SubItems.Add(dt.Rows[i]["CS"].ToString());
                    lvi_New.SubItems.Add(dt.Rows[i]["FX"].ToString());
                    lvi_New.SubItems.Add(dt.Rows[i]["XZ"].ToString());
                    lvi_New.SubItems.Add(dt.Rows[i]["CXL"].ToString());
                    lvi_New.SubItems.Add(dt.Rows[i]["XHZL"].ToString());
                    lvi_New.SubItems.Add(dt.Rows[i]["CD"].ToString());
                    lvi_New.SubItems.Add(dt.Rows[i]["JCSJ"].ToString());

                    if (dt.Rows[i]["CPTX"] == "")
                        lvi_New.SubItems.Add("0000");
                    else
                        lvi_New.SubItems.Add(dt.Rows[i]["CPTX"].ToString());
                    if (dt.Rows[i]["PLATE"] == "")
                        lvi_New.SubItems.Add("0000");
                    else
                        lvi_New.SubItems.Add(dt.Rows[i]["PLATE"].ToString());

                    if (int.Parse(dt.Rows[i]["CXL"].ToString()) >= SysSet.MinCXBZ && int.Parse(dt.Rows[i]["CXL"].ToString()) <= SysSet.MaxCXBZ)
                    {
                        lvi_New.BackColor = Color.Red;
                        lvi_New.ForeColor = Color.Black;
                    }
                    else if (int.Parse(dt.Rows[i]["CXL"].ToString()) > 0)
                    {
                        lvi_New.BackColor = Color.Yellow;
                        lvi_New.ForeColor = Color.Black;
                    }


                    listV_DateList.Items.Insert(0, lvi_New);
                    //listV_DateList.SelectedItems.Clear();
                    //lvi_New.Selected = true;
                    //listV_DateList.Focus();

                    //switch (int.Parse(dt.Rows[i]["CD"].ToString()))
                    //{
                    //    case 1:
                    //        if (int.Parse(dt.Rows[i]["CXL"].ToString()) >= SysSet.MinCXBZ && int.Parse(dt.Rows[i]["CXL"].ToString()) <= SysSet.MaxCXBZ)
                    //        {
                    //            lblCD1_CXL.ForeColor = Color.Red;
                    //            lblCD1_ZZ.ForeColor = Color.Red;
                    //        }
                    //        else
                    //        {
                    //            lblCD1_CXL.ForeColor = Color.Black;
                    //            lblCD1_ZZ.ForeColor = Color.Black;
                    //        }

                    //        txtCD1_CPH.Text = dt.Rows[i]["CPH"].ToString();
                    //        lblCD1_ZZ.Text = (int.Parse(dt.Rows[i]["ZZ"].ToString()) / (double)1000).ToString();
                    //        lblCD1_CPYS.Text = dt.Rows[i]["CPYS"].ToString();
                    //        lblCD1_ZS.Text = dt.Rows[i]["ZS"].ToString();
                    //        lblCD1_CS.Text = dt.Rows[i]["CS"].ToString() ;
                    //        lblCD1_JCSJ.Text = dt.Rows[i]["JCSJ"].ToString();
                    //        lblCD1_CXL.Text = dt.Rows[i]["CXL"].ToString();

                    //        if (dt.Rows[i]["CPTX"].ToString() != "" && dt.Rows[i]["CPTX"].ToString() != "0000" && File.Exists(dt.Rows[i]["CPTX"].ToString()))
                    //        {
                    //            bmapCD1 = new Bitmap(dt.Rows[i]["CPTX"].ToString());
                    //            pictureCD1.Image = bmapCD1;
                    //            bmapCD1.Save(dirUpPhoto + "\\" + dt.Rows[i]["CPTX"].ToString().Substring(dt.Rows[i]["CPTX"].ToString().LastIndexOf("\\")));
                    //        }
                    //        else
                    //        {
                    //            pictureCD1.Image = null;
                    //        }
                    //        break;
                    //    case 2:
                    //        if (int.Parse(dt.Rows[i]["CXL"].ToString()) >= SysSet.MinCXBZ && int.Parse(dt.Rows[i]["CXL"].ToString()) <= SysSet.MaxCXBZ)
                    //        {
                    //            lblCD2_CXL.ForeColor = Color.Red;
                    //            lblCD2_ZZ.ForeColor = Color.Red;
                    //        }
                    //        else
                    //        {
                    //            lblCD2_CXL.ForeColor = Color.Black;
                    //            lblCD2_ZZ.ForeColor = Color.Black;
                    //        }

                    //        txtCD2_CPH.Text = dt.Rows[i]["CPH"].ToString();
                    //        lblCD2_ZZ.Text = (int.Parse(dt.Rows[i]["ZZ"].ToString()) / (double)1000).ToString();
                    //        lblCD2_CPYS.Text = dt.Rows[i]["CPYS"].ToString();
                    //        lblCD2_ZS.Text = dt.Rows[i]["ZS"].ToString();
                    //        lblCD2_CS.Text = dt.Rows[i]["CS"].ToString() ;
                    //        lblCD2_JCSJ.Text = dt.Rows[i]["JCSJ"].ToString();
                    //        lblCD2_CXL.Text = dt.Rows[i]["CXL"].ToString();

                    //        if (dt.Rows[i]["CPTX"].ToString() != "" && dt.Rows[i]["CPTX"].ToString() != "0000" && File.Exists(dt.Rows[i]["CPTX"].ToString()))
                    //        {
                    //            bmapCD2 = new Bitmap(dt.Rows[i]["CPTX"].ToString());
                    //            pictureCD2.Image = bmapCD2;
                    //            bmapCD2.Save(dirUpPhoto + "\\" + dt.Rows[i]["CPTX"].ToString().Substring(dt.Rows[i]["CPTX"].ToString().LastIndexOf("\\")));
                    //        }
                    //        else
                    //        {
                    //            pictureCD2.Image = null;
                    //        }
                    //        break;
                    //    case 3:
                    //        if (int.Parse(dt.Rows[i]["CXL"].ToString()) >= SysSet.MinCXBZ && int.Parse(dt.Rows[i]["CXL"].ToString()) <= SysSet.MaxCXBZ)
                    //        {
                    //            lblCD3_CXL.ForeColor = Color.Red;
                    //            lblCD3_ZZ.ForeColor = Color.Red;
                    //        }
                    //        else
                    //        {
                    //            lblCD3_CXL.ForeColor = Color.Black;
                    //            lblCD3_ZZ.ForeColor = Color.Black;
                    //        }

                    //        txtCD3_CPH.Text = dt.Rows[i]["CPH"].ToString();
                    //        lblCD3_ZZ.Text = (int.Parse(dt.Rows[i]["ZZ"].ToString()) / (double)1000).ToString();
                    //        lblCD3_CPYS.Text = dt.Rows[i]["CPYS"].ToString();
                    //        lblCD3_ZS.Text = dt.Rows[i]["ZS"].ToString();
                    //        lblCD3_CS.Text = dt.Rows[i]["CS"].ToString() ;
                    //        lblCD3_JCSJ.Text = dt.Rows[i]["JCSJ"].ToString();
                    //        lblCD3_CXL.Text = dt.Rows[i]["CXL"].ToString();

                    //        if (dt.Rows[i]["CPTX"].ToString() != "" && dt.Rows[i]["CPTX"].ToString() != "0000" && File.Exists(dt.Rows[i]["CPTX"].ToString()))
                    //        {
                    //            bmapCD3 = new Bitmap(dt.Rows[i]["CPTX"].ToString());
                    //            pictureCD3.Image = bmapCD3;
                    //            bmapCD3.Save(dirUpPhoto + "\\" + dt.Rows[i]["CPTX"].ToString().Substring(dt.Rows[i]["CPTX"].ToString().LastIndexOf("\\")));
                    //        }
                    //        else
                    //        {
                    //            pictureCD3.Image = null;
                    //        }
                    //        break;
                    //    case 4:
                    //        if (int.Parse(dt.Rows[i]["CXL"].ToString()) >= SysSet.MinCXBZ && int.Parse(dt.Rows[i]["CXL"].ToString()) <= SysSet.MaxCXBZ)
                    //        {
                    //            lblCD4_CXL.ForeColor = Color.Red;
                    //            lblCD4_ZZ.ForeColor = Color.Red;
                    //        }
                    //        else
                    //        {
                    //            lblCD4_CXL.ForeColor = Color.Black;
                    //            lblCD4_ZZ.ForeColor = Color.Black;
                    //        }

                    //        txtCD4_CPH.Text = dt.Rows[i]["CPH"].ToString();
                    //        lblCD4_ZZ.Text = (int.Parse(dt.Rows[i]["ZZ"].ToString()) / (double)1000).ToString();
                    //        lblCD4_CPYS.Text = dt.Rows[i]["CPYS"].ToString();
                    //        lblCD4_ZS.Text = dt.Rows[i]["ZS"].ToString();
                    //        lblCD4_CS.Text = dt.Rows[i]["CS"].ToString() ;
                    //        lblCD4_JCSJ.Text = dt.Rows[i]["JCSJ"].ToString();
                    //        lblCD4_CXL.Text = dt.Rows[i]["CXL"].ToString();

                    //        if (dt.Rows[i]["CPTX"].ToString() != "" && dt.Rows[i]["CPTX"].ToString() != "0000" && File.Exists(dt.Rows[i]["CPTX"].ToString()))
                    //        {
                    //            bmapCD4 = new Bitmap(dt.Rows[i]["CPTX"].ToString());
                    //            pictureCD4.Image = bmapCD4;
                    //            bmapCD4.Save(dirUpPhoto + "\\" + dt.Rows[i]["CPTX"].ToString().Substring(dt.Rows[i]["CPTX"].ToString().LastIndexOf("\\")));
                    //        }
                    //        else
                    //        {
                    //            pictureCD4.Image = null;
                    //        }
                    //        break;
                    //    default:
                    //        break;
                    //}
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region 数据入库 

        /// <summary>
        /// 数据入库,正向行驶
        /// </summary>
        /// <param name="zh_st_zdczb">数据库实体对象</param>
        /// <returns>成功返回true否则返回False</returns>
        private bool DBInsert(ZH_ST_ZDCZB zh_st_zdczb)
        {
            DAL dal = new DAL();
            bool ret = false;

            try
            {
                if (dal.Add(zh_st_zdczb) > 0)
                {
                    ret = true;
                }

                //string str_localPhoto = "", str_plate = "";
                //if (zh_st_zdczb.CPTX != "" && zh_st_zdczb.CPTX != null)
                //    str_localPhoto = zh_st_zdczb.CPTX.Substring(zh_st_zdczb.CPTX.LastIndexOf("\\"));
                //if (zh_st_zdczb.PLATE != "" && zh_st_zdczb.PLATE != null)
                //    str_plate = zh_st_zdczb.PLATE.Substring(zh_st_zdczb.PLATE.LastIndexOf("\\"));

                ////写入数据库

                //string str_JZZD = string.Format("insert into jzzd.dbo.ZH_ST_ZDCZB(CPH,CPYS,ZZ,ZS,CS,CD,XZ,CXL,CPTX," +
                //                        " PLATE,JCSJ,CZY,ZDBZ,SFCX,SFXZ,FJBZ,JCZT,SJDJ,FX)values('{0}','{1}'," +
                //                        "{2},{3},{4},{5},{6},{7},'{8}','{9}','{10}','{11}','{12}',{13},{14},{15},{16},{17},'{18}');",
                //                        zh_st_zdczb.CPH.Trim(), zh_st_zdczb.CPYS, zh_st_zdczb.ZZ, zh_st_zdczb.ZS, zh_st_zdczb.CS, zh_st_zdczb.CD,
                //                        zh_st_zdczb.XZ, zh_st_zdczb.CXL, getMysqlPhotoPath(zh_st_zdczb.CPTX), getMysqlPhotoPath(zh_st_zdczb.PLATE),
                //                        zh_st_zdczb.JCSJ, SysSet.ZDMC, SysSet.ZDIP, zh_st_zdczb.SFCX, 0, 0, 0, 0, zh_st_zdczb.FX);

                //string str_ZDCZ = string.Format("insert into JZZC.dbo.ZH_ST_ZDCZB(CPH,CPYS,ZZ,ZS,CS,XZ,CXL,CPTX," +
                //                                    "JCSJ,CZY,ZDBZ,SFCX,SFXZ,FJBZ,XHZL,JCZT,CD,CDFX)values(" +
                //                                    "'{0}','{1}',{2},{3},{4},{5},{6},'{7}','{8}','{9}','{10}',{11}," +
                //                                    "'{12}',{13},{14},'{15}',{16},'{17}');",
                //                                    zh_st_zdczb.CPH.Trim(), zh_st_zdczb.CPYS, zh_st_zdczb.ZZ, zh_st_zdczb.ZS, zh_st_zdczb.CS,
                //                                    zh_st_zdczb.XZ, zh_st_zdczb.CXL, str_localPhoto, zh_st_zdczb.JCSJ, SysSet.ZDMC,
                //                                    SysSet.ZDIP, zh_st_zdczb.SFCX, 0, 0, zh_st_zdczb.XHZL, zh_st_zdczb.JCZT, zh_st_zdczb.CD, zh_st_zdczb.FX);

                //string str_ZDCZJG = string.Format("insert into JZZC.dbo.ZH_ST_ZDCZJGB(CPH,CPYS,ZZ,ZS,CS,XZ,CXL,CPTX," +
                //                                    "JCSJ,CZY,ZDBZ,SFCX,SFXZ,FJBZ,XHZL,JCZT,CD,CDFX)values(" +
                //                                    "'{0}','{1}',{2},{3},{4},{5},{6},'{7}','{8}','{9}','{10}',{11},'{12}',{13},{14},'{15}',{16},'{17}');",
                //                                    zh_st_zdczb.CPH.Trim(), zh_st_zdczb.CPYS, zh_st_zdczb.ZZ, zh_st_zdczb.ZS, zh_st_zdczb.CS,
                //                                    zh_st_zdczb.XZ, zh_st_zdczb.CXL, str_localPhoto, zh_st_zdczb.JCSJ, SysSet.ZDMC, SysSet.ZDIP, zh_st_zdczb.SFCX,
                //                                    0, 0, zh_st_zdczb.XHZL, zh_st_zdczb.JCZT, zh_st_zdczb.CD, zh_st_zdczb.FX);
                //string str_upFile = dirTxt + "\\ZD-" + SysSet.ZDIP + "-" + zh_st_zdczb.JCSJ.ToString("yyyyMMddHHmmss") + ".txt";
                //StreamWriter sw_upFile = File.CreateText(str_upFile);
                //sw_upFile.WriteLine(str_ZDCZ + str_ZDCZJG);
                //sw_upFile.Close();
            }
            catch (Exception ex)
            {
                logger.Debug("入库发生异常" + ex.ToString() + "\r\n" + Common.getProperties(zh_st_zdczb));
            }

            return ret;

        }

        private bool DBInsertNX(NXTable NXData)
        {
            DALNX dalNX = new DALNX();
            bool ret = false;

            try
            {
                if (dalNX.Add(NXData) > 0)
                {
                    ret = true;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("逆行入库发生异常" + ex.ToString() + "\r\n" + Common.getProperties(NXData));
            }

            return ret;
        }

        private static string getMysqlPhotoPath(string str_imagePath)
        {
            if (str_imagePath != "" && str_imagePath != null)
            {
                string[] strs_path = str_imagePath.Split('\\');
                string str_retImage = "";
                for (int i = 0; i < strs_path.Length - 1; i++)
                    str_retImage += strs_path[i] + "\\\\";
                str_retImage += strs_path[strs_path.Length - 1];
                return str_retImage;
            }
            else
                return "";
        }
        #endregion

        #region 将接收到的数据根据协议组合成完整的一帧数据 
        /// <summary>
        /// 将接收到的数据根据协议组合成完成的一帧数据
        /// </summary>
        /// <param name="recvData">接收到的数据</param>
        private void CombinationFrame()
        {
            byte result = new byte();
            ConcurrentQueue<byte> queue = ConQueue;

            while (true)
            {
                if (queue.Count < 8)
                {
                    //Thread.Sleep(20);
                    continue;
                }
                var query = queue.Take(8);
                byte[] head = query.ToArray();

                if (head[0] == 0xA5 && head[1] == 0x5A && head[2] == 0xCC && head[3] == 0x33)
                {//线圈信号

                    for (int i = 0; i < 8; i++)
                    {
                        queue.TryDequeue(out result);
                    }
                    GetXQZT(head);

                    //ThreadPool.QueueUserWorkItem(x => GetXQZT(queue, head));
                }
                else if (queue.Count >= 20)
                {
                    if (head[0] == 0xAA && head[1] == 0x55 && head[2] == 0xC3 && head[3] == 0x3C)
                    {
                        byte[] oneByte = new byte[20];
                        query = queue.Take(20);
                        oneByte = query.ToArray();
                        //移除使用的元素
                        for (int i = 0; i < 20; i++)
                        {
                            queue.TryDequeue(out result);
                        }
                        CalculateFrame(oneByte);
                    }
                    else
                    {
                        for (int i = 0; i < head.Length; i++)
                        {
                            if (head[i] != 0xAA || head[i] != 0xA5)
                            {
                                queue.TryDequeue(out result);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < queue.Count; i++)
                    {
                        query = queue.Take(4);
                        head = query.ToArray();
                        if (head[0] != 0xAA && head[0] != 0xA5)
                        {
                            queue.TryDequeue(out result);
                        }
                    }
                }

                //Thread.Sleep(10);
            }
        }
        #endregion

        #region 分析线圈状态 
        private void GetXQZT(byte[] head)
        {
            StringBuilder sbXQZT = new StringBuilder();
            StringBuilder sbSW = new StringBuilder();
            StringBuilder sbCF = new StringBuilder();
            StringBuilder sbXQSK = new StringBuilder();

            sbXQSK.Append(head[7] + (head[6] << 8) + (head[5] << 16));

            DateTime dtBegin, dtEnd, dt;
            dtBegin = dtEnd = dt = DateTime.Now;

            int xqzt = -1; //中间交换值 
            int xqbh = 0;

            #region 分析线圈状态

            for (int i = 1; i < 4; i++)
            {
                xqzt = Common.getIntegerSomeBit(head[4], i - 1);
                sbXQZT.Append(xqzt.ToString());

                if (xqzt == 0)
                {
                    if (XianQuan[i].XQZTChufa == -1)
                    {
                        XianQuan[i].ChufaTime = dt;
                        XianQuan[i].XQZTChufa = 0;
                        sbCF.Append(i + " ");
                        //手动抓拍，由于是中间线圈，触发时还没有判断方向，暂时是向所有登录成功的相机发送指令
                        xqbh = (i + 1);
                        //Console.WriteLine("触发抓拍线圈编号=" + xqbh);
                        if (xqbh == 1 || xqbh == 2)//1,2线圈触发第一个相机
                        {
                            //ThreadPool.QueueUserWorkItem(x => StartManuanSnap(1, (short)xqbh));
                        }
                        else if (xqbh == 3 || xqbh == 4)//2,3线圈触发第二个相机
                        {
                            //ThreadPool.QueueUserWorkItem(x => StartManuanSnap(2, (short)xqbh));
                        }
                    }
                    else if (XianQuan[i].XQZTChufa != -1 && (dt - XianQuan[i].ChufaTime).TotalSeconds > 8)
                    {
                        Console.WriteLine("距离上次触发时长大于8s，直接复位");
                        XianQuan[i].ChufaTime = dt;
                        XianQuan[i].XQZTChufa = 0;
                        sbCF.Append(i + " ");
                    }
                }
                else if (xqzt == 1)
                {
                    if (XianQuan[i].XQZTShouwei != 1 && (XianQuan[i].XQZTChufa == 0 || XianQuan[i].XQZTChufa == 2))
                    {
                        XianQuan[i].ShouweiTime = dt;
                        XianQuan[i].XQZTShouwei = 1;
                        sbSW.Append(i + " ");
                    }
                    else if (XianQuan[i].XQZTShouwei == 1 && (dt - XianQuan[i].ShouweiTime).TotalSeconds > 8)
                    {
                        Console.WriteLine("距离上次收尾时长大于8s，直接复位");
                        XianQuan[i].ShouweiTime = dt;
                        XianQuan[i].XQZTShouwei = 1;
                        sbSW.Append(i + " ");
                    }
                }
            }
            Console.WriteLine("原始线圈:" + sbXQZT + ",  " + sbCF.ToString().PadRight(8, ' ') + "触发," + sbSW.ToString().PadRight(8, ' ') + " 收尾,时间:" + dt.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            logger.Info("原始线圈:" + sbXQZT + ",  " + sbCF.ToString().PadRight(8, ' ') + "触发," + sbSW.ToString().PadRight(8, ' ') + " 收尾,时间:" + dt.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            //sbXQZT.Clear();
            sbCF.Clear();
            sbSW.Clear();

            #endregion

            #region 分析相邻线圈是否存在同时触发情况
            /*
             * 同时触发的判断条件如下
             * 1线圈：
             *      在1线圈收尾时判断2线圈是否触发，如果2线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在2线圈触发时间到1线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第5号称板的数据，如果存在说明，1,2线圈对应的为两辆车，1线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车1线圈的触发状态设置为2，且将TS12=true。
             * 2线圈：
             *      21同时
             *      由于循环时是从0开始的，这里增加TS12的判断，如果1线圈已经判断过了，2线圈直接跳过即可。
             *      在2线圈收尾时判断1线圈是否触发，如果1线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在2线圈触发时间到2线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第5号称板的数据，如果存在说明，1,2线圈对应的为两辆车，2线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车2线圈的触发状态设置为2，且将TS12=true。
             *      23同时
             *      在2线圈收尾时判断3线圈是否触发，如果3线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在2线圈触发时间到2线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第3、4、9号称板的数据，如果存在说明，2,3线圈对应的为两辆车，2线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车2线圈的触发状态设置为2，且将TS23=true。
             * 3线圈：
             *      32同时
             *      由于循环时是从0开始的，这里增加TS23的判断，如果2线圈已经判断过了，3线圈直接跳过即可。
             *      在3线圈收尾时判断2线圈是否触发，如果2线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在3线圈触发时间到3线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第3、4、9号称板的数据，如果存在说明，3,2线圈对应的为两辆车，3线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车3线圈的触发状态设置为2，且将TS23=true。
             *      34同时
             *      在3线圈收尾时判断4线圈是否触发，如果4线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在3线圈触发时间到3线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第8号称板的数据，如果存在说明，3,4线圈对应的为两辆车，3线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车3线圈的触发状态设置为2，且将TS34=true。
             * 4线圈：
             *      43同时
             *      由于循环时是从0开始的，这里增加TS34的判断，如果3线圈已经判断过了，4线圈直接跳过即可。
             *      在4线圈收尾时判断3线圈是否触发，如果3线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在4线圈触发时间到4线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第8号称板的数据，如果存在说明，4,3线圈对应的为两辆车，4线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车4线圈的触发状态设置为2，且将TS34=true。
             */
            for (int i = 0; i < sbXQZT.Length; i++)
            {
                if (sbXQZT[i] == '1')
                {
                    switch (i + 1)
                    {
                        case 1:
                            #region 判断线圈1
                            if (XianQuan[1].XQZTChufa == 0 && XianQuan[2].XQZTChufa == 0 && Math.Abs((XianQuan[1].ChufaTime - XianQuan[2].ChufaTime).TotalMilliseconds) <= SysSet.XqInterverTime)
                            {//如果1和2都触发，且时间间隔小于设定的值，认为同时触发 
                                if (TS12 == false && GetCBDataList(new int[] { 1 }, XianQuan[1].ChufaTime, XianQuan[1].ShouweiTime, false).Count == 0)
                                {//1收尾但是没有1号称板，认为是同一辆车
                                    TS12 = true;
                                    XianQuan[1].XQZTChufa = XianQuan[2].XQZTChufa = 2;

                                    Console.WriteLine("线圈 1 2 同时触发，时间为：" + XianQuan[0].ChufaTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                                    logger.Info("线圈 1 2 同时触发，时间为：" + XianQuan[0].ChufaTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                                }
                            }
                            break;
                        #endregion
                        case 2:
                            #region 判断线圈2
                            if (XianQuan[2].XQZTChufa == 0 && XianQuan[1].XQZTChufa == 0 && Math.Abs((XianQuan[2].ChufaTime - XianQuan[1].ChufaTime).TotalMilliseconds) <= SysSet.XqInterverTime)
                            {//12同时触发 
                                if (TS12 == false && GetCBDataList(new int[] { 7 }, XianQuan[2].ChufaTime, XianQuan[2].ShouweiTime, false).Count == 0)
                                {
                                    TS12 = true;
                                    XianQuan[2].XQZTChufa = XianQuan[1].XQZTChufa = 2;

                                    Console.WriteLine("线圈 2 1 同时触发，时间为：" + XianQuan[2].ChufaTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                                    logger.Info("线圈 2 1 同时触发，时间为：" + XianQuan[2].ChufaTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                                }
                            }
                            else if (XianQuan[2].XQZTChufa == 0 && XianQuan[3].XQZTChufa == 0 && Math.Abs((XianQuan[2].ChufaTime - XianQuan[3].ChufaTime).TotalMilliseconds) <= SysSet.XqInterverTime)
                            {//23同时触发 
                                if (TS23 == false && GetCBDataList(new int[] { 11 }, XianQuan[2].ChufaTime, XianQuan[2].ShouweiTime, false).Count == 0)
                                {
                                    TS23 = true;
                                    XianQuan[2].XQZTChufa = XianQuan[3].XQZTChufa = 2;

                                    Console.WriteLine("线圈 2 3 同时触发，时间为：" + XianQuan[2].ChufaTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                                    logger.Info("线圈 2 3 同时触发，时间为：" + XianQuan[2].ChufaTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                                }
                            }
                            break;
                        #endregion
                        case 3:
                            #region 判断线圈3
                            if (XianQuan[3].XQZTChufa == 0 && XianQuan[2].XQZTChufa == 0 && Math.Abs((XianQuan[3].ChufaTime - XianQuan[2].ChufaTime).TotalMilliseconds) <= SysSet.XqInterverTime)
                            {
                                if (TS23 == false && GetCBDataList(new int[] { 11 }, XianQuan[3].ChufaTime, XianQuan[3].ShouweiTime, false).Count == 0)
                                {
                                    TS23 = true;
                                    XianQuan[3].XQZTChufa = XianQuan[2].XQZTChufa = 2;

                                    Console.WriteLine("线圈 3 2 同时触发，时间为：" + XianQuan[3].ChufaTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                                    logger.Info("线圈 3 2 同时触发，时间为：" + XianQuan[3].ChufaTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                                }
                            }
                            break;
                            #endregion
                    }
                }
            }

            #endregion

            #region 收尾计算
            for (int i = 1; i < 4; i++)
            {
                if (XianQuan[i].XQZTShouwei != 1)
                { //不为收尾 
                    continue;
                }
                switch (i)
                {
                    case 1:
                        #region 1线圈收尾


                        //线圈收尾，三个线圈公用一个牌识，所以此处直接执行抓拍即可，用于匹配的picNum付给车道号，后面修改匹配规则
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 1, true)));
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(3, 1, false)));

                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 1, true, false)));
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 1, false, false)));

                        if (XianQuan[1].XQZTChufa == 0)
                        {//1单独触发收尾
                            InitXianQuanState(new int[] { 1 });

                            Console.WriteLine("线圈<1>触发...<1>收尾完成，进入称板计算");
                            ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQ(1, 1, XianQuan[1].ChufaTime.AddMilliseconds(SysSet.PreTime),
                                                        XianQuan[1].ShouweiTime.AddMilliseconds(SysSet.NextTime))));
                        }
                        else if (XianQuan[1].XQZTChufa == 2 && TS12 == true)
                        {//如果是12线圈同时触发 
                            if (XianQuan[2].XQZTShouwei == 1)
                            {//如果2线圈收尾了，则进入计算，否则等2线圈收尾
                                Console.WriteLine("线圈<1><2>同时触发...<2><1>收尾完成，进入称板计算");
                                logger.Info("线圈<1><2>同时触发...<2><1>收尾完成，进入称板计算");

                                InitXianQuanState(new int[] { 1, 2 });
                                TS12 = false;
                                /*
                                 * 需要判断1,2两个线圈哪个是最早触发的,哪个是最后收尾的
                                 */
                                GetBeginAndEndTime(new int[] { 1, 2 }, new int[] { 1, 2 }, ref dtBegin, ref dtEnd);

                                ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQ(2, 1, dtBegin.AddMilliseconds(SysSet.PreTime),
                                    dtEnd.AddMilliseconds(SysSet.NextTime))));
                            }
                        }
                        break;
                    #endregion
                    case 2:
                        #region 2线圈收尾
                        //线圈收尾，三个线圈公用一个牌识，所以此处直接执行抓拍即可，用于匹配的picNum付给车道号，后面修改匹配规则
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 2, true)));
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(3, 2, false)));

                        if (XianQuan[2].XQZTChufa == 0)
                        {//单线圈触发  

                            Console.WriteLine("线圈<2>触发....<2>收尾完成，进入称板计算");
                            logger.Info("线圈<2>触发....<2>收尾完成，进入称板计算");
                            InitXianQuanState(new int[] { 2 });
                            ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQ(2, 2, XianQuan[2].ChufaTime.AddMilliseconds(SysSet.PreTime),
                                                        XianQuan[2].ShouweiTime.AddMilliseconds(SysSet.NextTime))));
                        }
                        else if (XianQuan[2].XQZTChufa == 2 && TS12 == true && TS23 == false)
                        {//1,2同时触发，2收尾，判断1是否收尾，1收尾则计算
                            #region 12同时触发
                            if (XianQuan[1].XQZTShouwei == 1)
                            {
                                Console.WriteLine("线圈<2><1>同时触发....<1><2>收尾完成，进入称板计算");
                                logger.Info("线圈<2><1>同时触发....<1><2>收尾完成，进入称板计算");

                                InitXianQuanState(new int[] { 1, 2 });
                                TS12 = false;
                                /*
                            * 需要判断1,2两个线圈哪个是最早触发的,哪个线圈是最晚收尾的
                            */
                                GetBeginAndEndTime(new int[] { 1, 2 }, new int[] { 1, 2 }, ref dtBegin, ref dtEnd);
                                ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQ(1, 2, dtBegin.AddMilliseconds(SysSet.PreTime),
                                       dtEnd.AddMilliseconds(SysSet.NextTime))));
                            }
                            #endregion
                        }
                        else if (XianQuan[2].XQZTChufa == 2 && TS12 == false && TS23 == true)
                        {//2,3同时触发，2收尾，判断3是否收尾，3收尾则计算
                            #region 23同时触发
                            if (XianQuan[3].XQZTShouwei == 1)
                            {
                                Console.WriteLine("线圈<2><3>同时触发....<3><2>收尾完成，进入称板计算");
                                logger.Info("线圈<2><3>同时触发....<3><2>收尾完成，进入称板计算");

                                InitXianQuanState(new int[] { 2, 3 });
                                TS23 = false;
                                /*
                            * 需要判断2,3两个线圈哪个是最早触发的,哪个线圈是最晚收尾的
                            */
                                GetBeginAndEndTime(new int[] { 2, 3 }, new int[] { 2, 3 }, ref dtBegin, ref dtEnd);
                                ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQ(3, 2, dtBegin.AddMilliseconds(SysSet.PreTime),
                                       dtEnd.AddMilliseconds(SysSet.NextTime))));
                            }
                            #endregion
                        }
                        break;
                    #endregion
                    case 3:
                        #region 3线圈收尾
                        //线圈收尾，三个线圈公用一个牌识，所以此处直接执行抓拍即可，用于匹配的picNum付给车道号，后面修改匹配规则
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 3, true)));
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(3, 3, false)));

                        if (XianQuan[3].XQZTChufa == 0)
                        {//单独收尾

                            Console.WriteLine("线圈<3>触发....<3>收尾完成，进入称板计算");
                            logger.Info("线圈<3>触发....<3>收尾完成，进入称板计算");
                            InitXianQuanState(new int[] { 3 });

                            ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQ(3, 3, XianQuan[3].ChufaTime.AddMilliseconds(SysSet.PreTime),
                                XianQuan[3].ShouweiTime.AddMilliseconds(SysSet.NextTime))));
                        }
                        else if (XianQuan[3].XQZTChufa == 2 && TS23 == true && TS34 == false)
                        {
                            #region 3,2同时触发
                            if (XianQuan[2].XQZTShouwei == 1)
                            {//如果2线圈触发且收尾
                                Console.WriteLine("线圈<3><2>同时触发...<2><3>收尾完成，进入称板计算");
                                logger.Info("线圈<3><2>同时触发...<2><3>收尾完成，进入称板计算");
                                InitXianQuanState(new int[] { 2, 3 });
                                TS23 = false;
                                /*
                                  * 需要判断2,3两个线圈哪个是最早触发的,哪个是最晚收尾的
                                  */
                                GetBeginAndEndTime(new int[] { 2, 3 }, new int[] { 2, 3 }, ref dtBegin, ref dtEnd);

                                ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQ(2, 3, dtBegin.AddMilliseconds(SysSet.PreTime),
                                       dtEnd.AddMilliseconds(SysSet.NextTime))));
                            }
                            #endregion
                        }
                        break;
                    #endregion 
                    default:
                        break;
                }
            }
            #endregion

            StringBuilder sbLog = new StringBuilder();
            sbLog.Append("接收到线圈: " + Common.byteToHexStr(head) + " ");
            sbLog.Append("时刻：" + sbXQSK.ToString() + " ");
            sbLog.Append("接收时间：" + dt + " ");

            logger.Info(sbLog.ToString());

            sbXQZT.Clear();
            sbCF.Clear();
            sbSW.Clear();
            sbXQSK.Clear();
        }
        #endregion

        #region  开启手动抓拍 
        /// <summary>
        /// 开启手动抓拍
        /// </summary>
        /// <param name="sysSetIndex">使用相机配置中的第几个,第一个为1</param>
        /// <param name="picNum">车头识别结果匹配的唯一值，车头、车尾、侧面三次识别必须一致</param>
        /// <param name="TailOrSide">true车尾抓拍，false侧面抓拍</param>
        private void StartManuanSnap(int sysSetIndex, int picNum, bool TailOrSide, bool NX)
        {
            //所有登录成功的抓拍
            //for (int i = 0; i < m_lUserID.Length; i++)
            //{
            //    if (m_lUserID[i] >= 0)
            //    {
            //        ManualSnap(m_lUserID[i], SysSet.CpsbList[0].IP);
            //    }
            //}
            //指定配置中第几个相机抓拍
            int index = sysSetIndex - 1;
            string ip = SysSet.CpsbList[sysSetIndex - 1].IP;
            Console.WriteLine("开始截图,相机配置序号=" + index + ",IP=" + ip + ",TailOrSide=" + TailOrSide);

            if (m_lUserIDList[sysSetIndex - 1] > 0)
            {//如果登录成功，则抓拍
                //ManualSnap(m_lUserID[sysSetIndex - 1], SysSet.CpsbList[sysSetIndex - 1].IP, picNum, TailOrSide);
                CaptureJpeg(m_lUserIDList[sysSetIndex - 1], TailOrSide, NX, picNum);
            }
        }

        private void StartManualSnapZX(int sysSetIndex, bool TailOrSide, ref ZH_ST_ZDCZB zdczb)
        {
            try
            {
                int index = sysSetIndex - 1;
                string ip = SysSet.CpsbList[sysSetIndex - 1].IP;
                //Console.WriteLine("开始截图,相机配置序号=" + index + ",IP=" + ip + ",TailOrSide=" + TailOrSide);

                string strphoto = "";
                short cd = 0;

                string str = "";
                HCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new HCNetSDK.NET_DVR_JPEGPARA();
                lpJpegPara.wPicQuality = 0; //图像质量 Image quality
                lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 0xff-Auto(使用当前码流分辨率) 
                                            //抓图分辨率需要设备支持，更多取值请参考SDK文档

                //JPEG抓图保存成文件 Capture a JPEG picture  

                string str_id = DateTime.Now.ToString("yyyyMMddHHmmss");
                string path_li = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "\\");
                string directorySave = "";

                directorySave = dirPhoto + path_li;

                if (TailOrSide == true)
                {
                    directorySave += "\\Tail";
                    strphoto = directorySave + "\\" + cd + str_id + "-Tail.jpg";
                }
                else
                {
                    directorySave += "\\Side";
                    strphoto = directorySave + "\\" + cd + str_id + "-Side.jpg";
                }
                if (TailOrSide == true)
                {
                    #region 放置到车尾识别结果缓存中
                    zdczb.CWTX = strphoto;
                    #endregion
                }
                else
                {
                    zdczb.CMTX = strphoto;
                }

                if (!Directory.Exists(directorySave))
                    Directory.CreateDirectory(directorySave);

                if (m_lUserIDList[sysSetIndex - 1] > 0)
                {//如果登录成功，则抓拍
                    if (!HCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserIDList[sysSetIndex - 1], 1, ref lpJpegPara, strphoto))
                    {
                        iLastErr = HCNetSDK.NET_DVR_GetLastError();
                        str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                        Console.WriteLine(str);
                        return;
                    }
                    else
                    {
                        str = "NET_DVR_CaptureJPEGPicture succ and the saved file is " + strphoto;
                        //Console.WriteLine(str);
                        logger.Info(str);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("开启截图" + ex);
                logger.Debug("开启截图", ex);
            }
        }

        private void StartManualSnapNX(int sysSetIndex, bool TailOrSide, ref NXTable nxTable)
        {
            try
            {
                int index = sysSetIndex - 1;
                string ip = SysSet.CpsbList[sysSetIndex - 1].IP;
                //Console.WriteLine("开始截图,相机配置序号=" + index + ",IP=" + ip + ",TailOrSide=" + TailOrSide);

                string strphoto = "";
                short cd = 0;

                string str = "";
                HCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new HCNetSDK.NET_DVR_JPEGPARA();
                lpJpegPara.wPicQuality = 0; //图像质量 Image quality
                lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 0xff-Auto(使用当前码流分辨率) 
                                            //抓图分辨率需要设备支持，更多取值请参考SDK文档

                //JPEG抓图保存成文件 Capture a JPEG picture  

                string str_id = DateTime.Now.ToString("yyyyMMddHHmmss");
                string path_li = DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "\\");
                string directorySave = "";

                directorySave = dirPhoto + path_li;

                if (TailOrSide == true)
                {
                    directorySave += "\\Tail";
                    strphoto = directorySave + "\\" + cd + str_id + "-Tail.jpg";
                }
                else
                {
                    directorySave += "\\Side";
                    strphoto = directorySave + "\\" + cd + str_id + "-Side.jpg";
                }
                if (TailOrSide == true)
                {
                    #region 放置到车尾识别结果缓存中
                    nxTable.CWTX = strphoto;
                    #endregion
                }
                else
                {
                    nxTable.CMTX = strphoto;
                }

                if (!Directory.Exists(directorySave))
                    Directory.CreateDirectory(directorySave);

                if (m_lUserIDList[sysSetIndex - 1] > 0)
                {//如果登录成功，则抓拍
                    if (!HCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserIDList[sysSetIndex - 1], 1, ref lpJpegPara, strphoto))
                    {
                        iLastErr = HCNetSDK.NET_DVR_GetLastError();
                        str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                        Console.WriteLine(str);
                        return;
                    }
                    else
                    {
                        str = "NET_DVR_CaptureJPEGPicture succ and the saved file is " + strphoto;
                        Console.WriteLine(str);
                        logger.Info(str);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("开启截图" + ex);
                logger.Debug("开启截图", ex);
            }
        }
        #endregion

        #region 重置指定线圈状态 
        /// <summary>
        /// 初始化线圈状态
        /// </summary>
        /// <param name="Index"></param>
        private void InitXianQuanState(int[] Index)
        {
            for (int i = 0; i < Index.Length; i++)
            {
                Console.WriteLine("初始化线圈" + (Index[i]) + " 线圈状态");
                XianQuan[Index[i]].XQZTChufa = -1;
                XianQuan[Index[i]].XQZTShouwei = -1;
            }
        }
        #endregion

        #region 获取线圈同时触发情况下的最早触发的线圈时间和最晚收尾的线圈时间，用于在该时间段内查找称板数据  
        /// <summary>
        /// 获取线圈同时触发情况下的最早触发的线圈时间和最晚收尾的线圈时间，用于在该时间段内查找称板数据
        /// </summary>
        /// <param name="ChufaXQBH">触发线圈编号</param>
        /// <param name="ShouweiXQBH">收尾线圈编号</param>
        /// <param name="dtBegin">最早触发时间</param>
        /// <param name="dtEnd">最晚收尾时间</param>
        /// <param name="BuffCD">缓存的车道</param>
        private void GetBeginAndEndTime(int[] ChufaXQBH, int[] ShouweiXQBH, ref DateTime dtBegin, ref DateTime dtEnd)
        {
            var t = from p in XianQuan.AsEnumerable()
                    where ChufaXQBH.Contains(p.XQBH) && (p.XQBH > 0 && p.XQBH < 5)
                    orderby p.ChufaTime
                    select p;

            if (t.Count() > 0)
            {
                dtBegin = t.FirstOrDefault().ChufaTime;
                //dtEnd = t.LastOrDefault().ShouweiTime;
            }
            else
            {
                Console.WriteLine("没找到触发时间？？？？？？？？？？？？？？？？？？？？？");
            }
            t = from p in XianQuan.AsEnumerable()
                where p.XQZTChufa != -1 && ShouweiXQBH.Contains(p.XQBH)
                orderby p.ShouweiTime descending
                select p;

            if (t.Count() > 0)
            {
                dtEnd = t.FirstOrDefault().ShouweiTime;
            }
            else
            {
                Console.WriteLine("没找到收尾时间？？？？？？？？？？？？？？？？？？？？？");
            }
            Console.WriteLine("使用的触发时间为：" + dtBegin.ToString("yyyy/MM/dd HH:mm:ss.fff") + ",收尾时间：" + dtEnd.ToString("yyyy/MM/dd HH:mm:ss.fff"));
        }
        #endregion

        #region 这数据列表最上面插入一条数据 
        /// <summary>
        /// 在dgv列表最上面插入一条数据
        /// </summary>
        /// <param name="type">类型，线圈或者是数据</param>
        /// <param name="TDH">通道号</param>
        /// <param name="JLZ">计量值</param>
        /// <param name="SK">时刻</param>
        /// <param name="CKZ">参考值</param>
        /// <param name="XQZT">线圈状态</param>
        /// <param name="RecvCode">接收数据包</param>
        private void InsertDgvRow(string TDH, string JLZ, string SK, string SKC, string ZZ, string CKZ, string RecvTime)
        {
            DataGridViewRow row = new DataGridViewRow();
            DataGridViewTextBoxCell textboxcell;

            textboxcell = new DataGridViewTextBoxCell();
            textboxcell.Value = TDH;
            row.Cells.Add(textboxcell);

            textboxcell = new DataGridViewTextBoxCell();
            textboxcell.Value = JLZ;
            row.Cells.Add(textboxcell);

            textboxcell = new DataGridViewTextBoxCell();
            textboxcell.Value = SK;
            row.Cells.Add(textboxcell);

            textboxcell = new DataGridViewTextBoxCell();
            textboxcell.Value = SKC; //时刻差
            row.Cells.Add(textboxcell);

            textboxcell = new DataGridViewTextBoxCell();
            textboxcell.Value = ZZ; //总重
            row.Cells.Add(textboxcell);

            textboxcell = new DataGridViewTextBoxCell();
            textboxcell.Value = CKZ;
            row.Cells.Add(textboxcell);


            textboxcell = new DataGridViewTextBoxCell();
            textboxcell.Value = RecvTime;
            row.Cells.Add(textboxcell);

            this.Invoke(new MethodInvoker(delegate
            {
                if (formShow.dgvData.Rows.Count > 5000)
                {
                    formShow.dgvData.Rows.Clear();
                }
                formShow.dgvData.Rows.Insert(0, row);
            }));
        }
        #endregion

        #region 计算完整的一帧数据 
        /// <summary>
        /// 计算完整的一帧数据
        /// </summary>
        /// <param name="oneByte">组合后的完整数据帧</param>
        private void CalculateFrame(byte[] oneByte)
        {
            CBData CbData = new CBData();
            CbData.RecvTime = DateTime.Now;//.ToString("yyyy-MM-dd HH:mm:ss fff");
            CbData.TDH = oneByte[4] + 1;
            CbData.JLZ = oneByte[8] + (oneByte[7] << 8) + (oneByte[6] << 16) + (oneByte[5] << 24);
            CbData.Time = oneByte[11] + (oneByte[10] << 8) + (oneByte[9] << 16);
            CbData.KD = oneByte[13] + (oneByte[12] << 8);
            CbData.CKZ = oneByte[15] + (oneByte[14] << 8);

            //if (CbData.TDH == 1 || CbData.TDH == 2 || CbData.TDH == 3 || CbData.TDH == 4)
            //{
            //    return;
            //}
            //CBList.Add(CbData);
            CBListAdd(CbData);

            Console.WriteLine("称板数据：" + CbData.TDH + ".........时间：" + CbData.RecvTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));

            StringBuilder sb = new StringBuilder();
            sb.Append("接收到数据: " + Common.byteToHexStr(oneByte) + " ");
            sb.Append("通道号：" + CbData.TDH.ToString() + " ");
            sb.Append("计量值：" + CbData.JLZ.ToString() + " ");
            sb.Append("时刻：" + CbData.Time.ToString() + " ");
            sb.Append("参考值：" + CbData.CKZ.ToString() + " ");
            sb.Append("接收时间：" + CbData.RecvTime.ToString("yyyy/MM/dd HH:mm:ss.fff") + " ");

            logger.Info(sb.ToString());
            //InsertDgvRow("数据", CbData.TDH.ToString(), CbData.JLZ.ToString(), CbData.Time.ToString(), CbData.CKZ.ToString(), "", dt, Common.byteToHexStr(oneByte));


        }
        #endregion

        #region 根据触发和收尾线圈的时间分组称板 
        private void GroupingXQ(int xq1, int xq2, DateTime XQChufa, DateTime XQShouwei)
        {
            try
            {
                ReMoveListItem();

                string xq = xq1.ToString() + xq2.ToString();
                //Thread.CurrentThread.Join(500);//延迟，后面测试用于判断6轴车中间收尾用

                Console.WriteLine("触发时间=" + XQChufa.ToString("yyyy/MM/dd HH:mm:ss.fff") + "收尾时间=" + XQShouwei.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                switch (xq)
                {
                    case "11":
                        #region 1触发,1收尾
                        GetZZFZ(GetCBDataList(new int[] { 1, 2, 3, 4, 5 }, XQChufa, XQShouwei, true), 1);
                        break;
                    #endregion
                    case "21":
                        #region 12触发,21或者1收尾
                        GetZZFZ(GetCBDataList(new int[] { 2, 3, 4, 5, 6 }, XQChufa, XQShouwei, true), 1);
                        break;
                    #endregion
                    case "22":
                        #region 2触发,2收尾
                        GetZZFZ(GetCBDataList(new int[] { 4, 5, 6, 7, 8, 9 }, XQChufa, XQShouwei, true), 2);
                        break;
                    #endregion
                    case "12":
                        #region 21触发,12收尾或2收尾
                        GetZZFZ(GetCBDataList(new int[] { 2, 3, 4, 5, 6 }, XQChufa, XQShouwei, true), 2);
                        break;
                    #endregion
                    case "32":
                        #region 23触发,32或2收尾
                        GetZZFZ(GetCBDataList(new int[] { 6, 7, 8, 9, 10 }, XQChufa, XQShouwei, true), 2);
                        break;
                    #endregion
                    case "33":
                        #region 3触发,3收尾
                        GetZZFZ(GetCBDataList(new int[] { 7, 8, 9, 10, 11 }, XQChufa, XQShouwei, true), 3);
                        //GetZZ(GetCBDataList(new int[] { 1, 2, 3, 4, 5 }, XQChufa, XQShouwei, true), 1);
                        break;
                    #endregion
                    case "23":
                        #region 23触发,23收尾或3收尾
                        GetZZFZ(GetCBDataList(new int[] { 6, 7, 8, 9, 10 }, XQChufa, XQShouwei, true), 3);
                        break;
                    #endregion 
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger.Debug(ex.ToString(), ex);
            }
        }
        #endregion

        #region 获取用于计算重量的称板列表 
        /// <summary>
        /// 获取用于计算重量的称板列表
        /// </summary>
        /// <param name="CBBH">在线圈对应范围内的称板编号序列</param>
        /// <param name="XQChufa">触发时间</param>
        /// <param name="XQShouwei">收尾时间</param> 
        /// <returns>用于计算重量的称板列表</returns>
        private List<CBData> GetCBDataList(int[] CBBH, DateTime XQChufa, DateTime XQShouwei, bool Removable)
        {
            List<CBData> list = (from p in CBList
                                 where CBBH.Contains(p.TDH) &&
                                   (p.RecvTime >= XQChufa && p.RecvTime <= XQShouwei)
                                 orderby p.RecvTime
                                 select p).ToList();
            if (Removable)
            {
                CBListRemove(list);
            }

            return list;
        }
        #endregion

        #region 去除20s之前的数据 
        /// <summary>
        /// 去除20s之前的数据
        /// </summary>
        private void ReMoveListItem()
        {
            var except = from p in CBList
                         where (DateTime.Now - p.RecvTime).TotalSeconds >= 20
                         select p;
            if (except.Count() > 0)
            {
                CBListRemove(except.ToList());
            }
        }
        #endregion

        #region 计算总重 
        /// <summary>
        /// 计算总重
        /// </summary>
        /// <param name="query">称板数据列表</param>
        /// <param name="CD">车道号</param>  
        private void GetZZ(List<CBData> ListData, int CD)
        {
            #region 变量 

            StringBuilder sb = new StringBuilder();

            //奇数列使用的哪两块板计算
            StringBuilder sbOddCBTDH = new StringBuilder();
            //偶数列使用的哪两块板计算
            StringBuilder sbEvenCBTDH = new StringBuilder();

            foreach (var item in ListData)
            {
                sb.Append(item.TDH + " ");
            }
            Console.WriteLine("用于计算的称板：" + sb.ToString());
            logger.Info("用于计算的称板：" + sb.ToString());
            if (sb.Length < 8)
            {
                CBNotEnoughCount++;//数量++

                Console.WriteLine("用于计算的称板不够计算使用，丢弃=" + CBNotEnoughCount);
                logger.Info("用于计算的称板不够计算使用，丢弃=" + CBNotEnoughCount);
                return;
            }

            ZH_ST_ZDCZB zh_st_zdczb = new ZH_ST_ZDCZB();
            int usedCBData = 3;//默认对比
            TestData tData = new TestData();

            bool isTime = false;
            int usedTimeCha = 0; //用于计算的时间差

            //车速为km/h即千米/小时 
            double ms = 0, s, m, h;
            //奇数编号的称板队列
            List<CBData> oddList = new List<CBData>();
            //偶数编号的称板队列
            List<CBData> evenList = new List<CBData>();
            /*
             * 比较奇数列和偶数列压板数，如果一侧压板数为奇数，一侧为偶数，
             * 侧取压板数为偶数侧的重量作为总重，若两侧压板数都是偶数，侧
             * 比较后去较重的
             * 轴数使用偶数侧的总板数除以2
             */
            //奇数列压板数
            int oddCBNo = 0;
            //偶数列压板数
            int evenCBNo = 0;
            int oddCBCount = 0;
            int evenCBCount = 0;
            int CheckPSIndex = 0;//首先匹配配置中的第几个相机

            #endregion

            zh_st_zdczb.CPH = "无车牌";
            zh_st_zdczb.CPYS = "无";
            zh_st_zdczb.CPTX = "";
            zh_st_zdczb.PLATE = "";
            zh_st_zdczb.CWTX = " ";
            zh_st_zdczb.CMTX = " ";
            zh_st_zdczb.Video = " ";

            //手动截图
            ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManualSnapZX(2, true, ref zh_st_zdczb)));
            ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManualSnapZX(3, false, ref zh_st_zdczb)));
            //StartManuanSnapZX(2, true, ref zh_st_zdczb);
            //StartManuanSnapZX(3, false, ref zh_st_zdczb);

            try
            {
                List<CBData> tmpList = ListData;

                tData.ListCB = tmpList;

                /*
                 * 查询当前计算队列中称板的个数，每个通道的称板只取一个，故查询结果的和就是压板数
                 */
                var o = from r in ListData.AsQueryable()
                        group r by r.TDH into g
                        select g.OrderBy(r => r.TDH).FirstOrDefault();

                //压板数，根据压板数是三，四，五进行不同的计算
                int OnCbCount = o.ToList().Count;
                int RecvCbCount = ListData.Count();

                #region 根据第一块板的方向判断使用匹配哪个方向的车牌相机 

                if (ListData[0].TDH % 2 == 0)
                {//接收到的第一条数据如果为偶数号板，则为称重方向逆行，判断称重方向车尾相机 
                    zh_st_zdczb.FX = SysSet.CpsbList[1].FangXiang;
                    CheckPSIndex = 1;
                }
                else
                {//收到的第一条数据如果为奇数号板,则为称重方向正常行驶 
                    zh_st_zdczb.FX = SysSet.CpsbList[0].FangXiang;
                    CheckPSIndex = 0;
                }
                #endregion
                /*
                * 在计算时刻差时必须使用同一个轮压两侧称板的差值，所以按照现场称板的奇偶分两侧编号的方式
                * 必须是称板编号的差值的绝对值为1的情况下，才认为是同侧轮
                */


                var odd = from q in ListData
                          where q.TDH % 2 != 0
                          select q;
                var even = from q in ListData
                           where q.TDH % 2 == 0
                           select q;

                oddList = odd.ToList();
                evenList = even.ToList();
                oddCBCount = oddList.Count;
                evenCBCount = evenList.Count;

                //经过对比分析，轴数这里取奇数或者偶数称板组中数据较多的一组
                //对该组的称板总数和压板的数进行求商取整作为车辆轴数
                #region 获取奇偶称板总数和每侧的压板数

                #region 计算奇数和偶数列的压板数，然后确定轴数和使用那侧的重量
                o = from r in oddList.AsQueryable()
                    group r by r.TDH into g
                    select g.OrderBy(r => r.TDH).FirstOrDefault();
                oddCBNo = o.ToList().Count();

                foreach (var item in o)
                {
                    sbOddCBTDH.Append(item.TDH);
                }

                logger.Info("奇数列称板总数=" + oddCBCount + ",奇数列压板数=" + oddCBNo);
                Console.WriteLine("奇数列称板总数=" + oddCBCount + ",奇数列压板数=" + oddCBNo);

                o = from r in evenList.AsQueryable()
                    group r by r.TDH into g
                    select g.OrderBy(r => r.TDH).FirstOrDefault();
                evenCBNo = o.ToList().Count();

                foreach (var item in o)
                {
                    sbEvenCBTDH.Append(item.TDH);
                }

                logger.Info("偶数列称板总数=" + evenCBCount + ",偶数列压板数=" + evenCBNo);
                Console.WriteLine("偶数列称板总数=" + evenCBCount + ",偶数列压板数=" + evenCBNo);

                if (oddCBNo % 2 == 0 && evenCBNo % 2 == 0)
                {//两侧压板数都为偶数
                    //zh_st_zdczb.ZS = oddCBCount >= evenCBCount ? oddCBCount / 2 : evenCBCount / 2; ;//可以取任何侧作为算轴依据

                    //if (oddCBCount >= evenCBCount)
                    //{
                    //    Console.WriteLine("两侧压板数都是偶数,但奇数侧总数>=偶数，轴数=奇数(+1)/2");
                    //    zh_st_zdczb.ZS = oddCBCount % 2 == 0 ? oddCBCount / 2 : (oddCBCount + 1) / 2;
                    //}
                    //else
                    //{
                    //    zh_st_zdczb.ZS = evenCBCount % 2 == 0 ? evenCBCount / 2 : (evenCBCount + 1) / 2;
                    //    Console.WriteLine("两侧压板数都是偶数,但偶数侧总数>=奇数，轴数=偶数(+1)/2");
                    //}
                    usedCBData = 3;//奇偶对比，用大的
                }
                else if (oddCBNo % 2 == 0 && evenCBNo % 2 != 0)
                {//奇数列压板数为偶数，偶数侧压板数不是偶数
                    //这里还需要判断总数是否是2的倍数，不是的话需要+1变成2的倍数 
                    //zh_st_zdczb.ZS = oddCBCount % 2 == 0 ? oddCBCount / 2 : (oddCBCount + 1) / 2;
                    //Console.WriteLine("奇数列压板数为偶数,轴数=奇数（+1）/2");
                    usedCBData = 1;//用奇数组计算
                }
                else if (oddCBNo % 2 != 0 && evenCBNo % 2 == 0)
                {//偶数侧压板数为偶数，奇数侧不是偶数
                    //zh_st_zdczb.ZS = evenCBCount % 2 == 0 ? evenCBCount / 2 : (evenCBCount + 1) / 2;
                    ////zh_st_zdczb.ZS = evenCBCount / 2;
                    //Console.WriteLine("偶数列压板数为偶数,轴数=偶数（+1）/2");
                    usedCBData = 2;//用偶数组计算
                }
                else if (oddCBNo % 2 != 0 && evenCBNo % 2 != 0)
                {//如果两侧压板数都不是偶数，这种情况属于丢板了，暂时使用压板数大的一侧+1作为算轴依据
                    //zh_st_zdczb.ZS = oddCBCount > evenCBCount ? (oddCBCount + 1) / 2 : (evenCBCount + 1) / 2;
                    //if (oddCBCount >= evenCBCount)
                    //{
                    //    Console.WriteLine("两侧压板数都不是偶数,但奇数侧总数>=偶数，轴数=奇数(+1)/2");
                    //    zh_st_zdczb.ZS = oddCBCount % 2 == 0 ? oddCBCount / 2 : (oddCBCount + 1) / 2;
                    //}
                    //else
                    //{
                    //    zh_st_zdczb.ZS = evenCBCount % 2 == 0 ? evenCBCount / 2 : (evenCBCount + 1) / 2;
                    //    Console.WriteLine("两侧压板数都不是偶数,但偶数侧总数>=奇数，轴数=偶数(+1)/2");
                    //}
                    usedCBData = 3;
                }
                #endregion

                #region 使用每侧称板数最多的那块板的总数作为轴数的方式

                var query = (from num in
                                 (
                                 from number in ListData
                                 group number.TDH by number.TDH into g
                                 select new
                                 {
                                     number = g.Key,
                                     cnt = g.Count()
                                 }
                             )
                             orderby num.cnt descending
                             select new { num.number, num.cnt }).ToList();
                foreach (var item in query)
                {
                    Console.WriteLine("称板 " + item.number.ToString() + " 次数=" + item.cnt);

                }
                if (query.Count > 0)
                {
                    zh_st_zdczb.ZS = query[0].cnt;
                }
                #endregion

                #endregion

                if (zh_st_zdczb.ZS == 1)
                {
                    zh_st_zdczb.ZS = 2;
                }
                else if (zh_st_zdczb.ZS > 6)
                {
                    zh_st_zdczb.ZS = 6;
                }

                #region 根据压板是3,5还是4获取到时间差，计算每块称板的重量
                switch (OnCbCount)
                {
                    case 3:
                    case 5:
                        //3,5块板的情况下，时间差需要使用公用的那块板
                        //这里取出其中一个奇数号称板和一个偶数号称板的时间差，
                        //作为所有轴共用的时间差来计算

                        for (int j = 0; j < evenCBCount; j++)
                        {
                            //if (evenList[j].UESED)
                            //{
                            //    continue;
                            //}
                            if (Math.Abs(oddList[0].TDH - evenList[j].TDH) != 1 && Math.Abs(oddList[0].TDH - evenList[j].TDH) != 3)
                            {//称板号差值不为1和3则继续 
                                continue;
                            }
                            else
                            {
                                sb.Clear();

                                oddList[0].TimeCha = evenList[j].TimeCha = Math.Abs(oddList[0].Time - evenList[j].Time);
                                sb.Append("3和5块板，时间差=" + oddList[0].TimeCha + ",所用奇数列 " + oddList[0].TDH + "时间" + oddList[0].Time + " 减去 偶数列 " + evenList[j].TDH + "时间" + evenList[j].Time);

                                if (oddList[0].TimeCha > timeInterval)
                                {
                                    if (oddList[0].Time > evenList[j].Time)
                                    {
                                        oddList[0].TimeCha = evenList[j].TimeCha = (0xFFFFFF - oddList[0].Time) + evenList[j].Time;
                                        sb.Append("时间差大于设定间隔，判断中间值 ,(0xFFFFFF-奇数)+偶数=新时间差=" + oddList[0].TimeCha);
                                    }
                                    else
                                    {
                                        oddList[0].TimeCha = evenList[j].TimeCha = (0xFFFFFF - evenList[j].Time) + oddList[0].Time;
                                        sb.Append("时间差大于设定间隔，判断中间值 ,(0xFFFFFF-偶数)+奇数=新时间差=" + oddList[0].TimeCha);
                                    }
                                }
                                usedTimeCha = oddList[0].TimeCha;
                                isTime = true;
                                logger.Info(sb.ToString());
                                Console.WriteLine(sb.ToString());

                                ms = Math.Abs((oddList[0].RecvTime - evenList[j].RecvTime).TotalMilliseconds);
                                break;
                            }
                        }

                        if (!isTime)
                        {
                            logger.Info("没有找到压3块或者5块板情况下的时间差，无法进行计算");
                            Console.WriteLine("没有找到压3块或者5块板情况下的时间差，无法进行计算");
                            return;
                        }
                        //for (int i = 0; i < oddList.Count; i++)
                        //{
                        //    oddList[i].ZZ = Convert.ToInt32(K * oddList[i].JLZ / oddList[0].TimeCha);
                        //}
                        //for (int i = 0; i < evenList.Count; i++)
                        //{
                        //    evenList[i].ZZ = Convert.ToInt32(K * evenList[i].JLZ / oddList[0].TimeCha);
                        //}

                        break;
                    case 4:
                        for (int i = 0; i < oddCBCount; i++)
                        {
                            //if (oddList[i].UESED)
                            //{
                            //    continue;
                            //}
                            for (int j = 0; j < evenCBCount; j++)
                            {
                                //if (evenList[j].UESED)
                                //{
                                //    continue;
                                //}
                                if (Math.Abs(oddList[i].TDH - evenList[j].TDH) != 1 && Math.Abs(oddList[i].TDH - evenList[j].TDH) != 3)
                                {
                                    continue;
                                }
                                else
                                {
                                    sb.Clear();

                                    oddList[i].TimeCha = evenList[j].TimeCha = Math.Abs(oddList[i].Time - evenList[j].Time);
                                    sb.Append("4块板，时间差=" + oddList[0].TimeCha + ",所用奇数列 " + oddList[i].TDH + "时间" + oddList[i].Time + " 减去 偶数列 " + evenList[j].TDH + "时间" + evenList[j].Time);

                                    if (oddList[i].TimeCha > timeInterval)
                                    {
                                        logger.Info("时间差大于设置的间隔值" + timeInterval + ",奇数组时间=" + oddList[i].Time + ",偶数组时间=" + evenList[j].Time);
                                        if (oddList[i].Time > evenList[j].Time)
                                        {
                                            oddList[i].TimeCha = evenList[j].TimeCha = (0xFFFFFF - oddList[i].Time) + evenList[j].Time;
                                            sb.Append("时间差大于设定间隔，判断中间值 ,(0xFFFFFF-奇数)+偶数=新时间差=" + oddList[i].TimeCha);
                                        }
                                        else
                                        {
                                            oddList[i].TimeCha = evenList[j].TimeCha = (0xFFFFFF - evenList[j].Time) + oddList[i].Time;
                                            sb.Append("时间差大于设定间隔，判断中间值 ,(0xFFFFFF-偶数)+奇数=新时间差=" + oddList[i].TimeCha);
                                        }
                                    }
                                    usedTimeCha = oddList[i].TimeCha;
                                    isTime = true;
                                    logger.Info(sb.ToString());
                                    Console.WriteLine(sb.ToString());

                                    ms = Math.Abs((oddList[0].RecvTime - evenList[j].RecvTime).TotalMilliseconds);

                                    //oddList[i].ZZ = Convert.ToInt32(K * oddList[i].JLZ / oddList[i].TimeCha);
                                    //oddList[i].UESED = true;
                                    //evenList[j].ZZ = Convert.ToInt32(K * evenList[j].JLZ / evenList[j].TimeCha);
                                    //evenList[j].UESED = true;
                                    i = oddCBCount;//跳出循环
                                    break;
                                }
                            }
                        }
                        if (!isTime)
                        {
                            logger.Info("没有找到压4块板情况下的时间差，无法进行计算");
                            Console.WriteLine("没有找到压4块板情况下的时间差，无法进行计算");
                            return;
                        }

                        //for (int i = 0; i < oddList.Count; i++)
                        //{
                        //    oddList[i].ZZ = Convert.ToInt32(K * oddList[i].JLZ / usedTimeCha);
                        //}
                        //for (int i = 0; i < evenList.Count; i++)
                        //{
                        //    evenList[i].ZZ = Convert.ToInt32(K * evenList[i].JLZ / usedTimeCha);
                        //}
                        break;
                    default:
                        return;
                        //break;
                }
                #endregion

                #region 根据压板数确定最终总量使用奇数侧、偶数侧还是奇偶对比取大的一侧
                switch (sbOddCBTDH.ToString())
                {
                    case "13":
                    case "31":
                        K = SysSet.K13;
                        SysSet.SpeedXZPara = SysSet.SpeedXZPara13;

                        Console.WriteLine("奇数列使用的K13=" + K);
                        break;
                    case "35":
                    case "53":
                        K = SysSet.K35;
                        SysSet.SpeedXZPara = SysSet.SpeedXZPara35;
                        Console.WriteLine("奇数列使用的K35=" + K);
                        break;
                    case "57":
                    case "75":
                        K = SysSet.K57;
                        SysSet.SpeedXZPara = SysSet.SpeedXZPara57;
                        Console.WriteLine("奇数列使用的K57=" + K);
                        break;
                    case "79":
                    case "97":
                        K = SysSet.K79;
                        SysSet.SpeedXZPara = SysSet.SpeedXZPara79;
                        Console.WriteLine("奇数列使用的K79=" + K);
                        break;
                    case "911":
                    case "119":
                        K = SysSet.K911;
                        SysSet.SpeedXZPara = SysSet.SpeedXZPara911;
                        Console.WriteLine("奇数列使用的K911=" + K);
                        break;
                }

                for (int i = 0; i < oddList.Count; i++)
                {
                    oddList[i].ZZ = Convert.ToInt32(K * oddList[i].JLZ / usedTimeCha);
                }

                switch (sbEvenCBTDH.ToString())
                {
                    case "24":
                    case "42":
                        K = SysSet.K24;
                        SysSet.SpeedXZPara = SysSet.SpeedXZPara24;
                        Console.WriteLine("奇数列使用的K24=" + K);
                        break;
                    case "46":
                    case "64":
                        K = SysSet.K46;
                        SysSet.SpeedXZPara = SysSet.SpeedXZPara46;
                        Console.WriteLine("奇数列使用的K46=" + K);
                        break;
                    case "68":
                    case "86":
                        K = SysSet.K68;
                        SysSet.SpeedXZPara = SysSet.SpeedXZPara68;
                        Console.WriteLine("奇数列使用的K68=" + K);
                        break;
                    case "810":
                    case "108":
                        K = SysSet.K810;
                        SysSet.SpeedXZPara = SysSet.SpeedXZPara810;
                        Console.WriteLine("奇数列使用的K810=" + K);
                        break;
                }
                for (int i = 0; i < evenList.Count; i++)
                {
                    evenList[i].ZZ = Convert.ToInt32(K * evenList[i].JLZ / usedTimeCha);
                }

                switch (usedCBData)
                {
                    case 1:
                        long zz1 = 0;
                        long zz2 = 0;

                        switch (sbOddCBTDH.ToString())
                        {
                            case "13":
                            case "31":

                                SysSet.SpeedXZPara = SysSet.SpeedXZPara13;
                                break;
                            case "35":
                            case "53":

                                SysSet.SpeedXZPara = SysSet.SpeedXZPara35;
                                break;
                            case "57":
                            case "75":

                                SysSet.SpeedXZPara = SysSet.SpeedXZPara57;
                                break;
                            case "79":
                            case "97":

                                SysSet.SpeedXZPara = SysSet.SpeedXZPara79;
                                break;
                            case "911":
                            case "119":

                                SysSet.SpeedXZPara = SysSet.SpeedXZPara911;
                                break;
                        }

                        sb.Append("时间差为：" + oddList[0].TimeCha);
                        foreach (var item in oddList)
                        {
                            zz1 += item.ZZ;
                            item.TimeCha = usedTimeCha;
                            sb.Append(" 奇数组：ZZ=" + item.ZZ + " ");
                        }
                        sb.Append("奇数组总重=" + zz1 + "\r\n");

                        foreach (var item in evenList)
                        {
                            zz2 += item.ZZ;
                            item.TimeCha = usedTimeCha;
                            sb.Append(" 偶数组：ZZ= " + item.ZZ + " ");
                        }
                        sb.Append("偶数组总重=" + zz2 + "\r\n");
                        zh_st_zdczb.ZZ = zz1;

                        Console.WriteLine("奇数组总重=" + zz1 + ",偶数组总重=" + zz2);
                        logger.Info(sb.ToString());

                        ms = oddList[0].TimeCha * 40 / 1000;
                        s = ms / 1000;
                        m = s / 60;
                        h = m / 60;
                        zh_st_zdczb.CS = (Decimal)(0.0017 / h);
                        break;
                    case 2:
                        zz1 = 0;
                        zz2 = 0;
                        sb.Append("时间差为：" + evenList[0].TimeCha);
                        switch (sbEvenCBTDH.ToString())
                        {
                            case "24":
                            case "42":

                                SysSet.SpeedXZPara = SysSet.SpeedXZPara24;
                                break;
                            case "46":
                            case "64":

                                SysSet.SpeedXZPara = SysSet.SpeedXZPara46;
                                break;
                            case "68":
                            case "86":

                                SysSet.SpeedXZPara = SysSet.SpeedXZPara68;
                                break;
                            case "810":
                            case "108":

                                SysSet.SpeedXZPara = SysSet.SpeedXZPara810;
                                break;
                        }

                        foreach (var item in oddList)
                        {
                            zz1 += item.ZZ;
                            item.TimeCha = usedTimeCha;
                            sb.Append(" 奇数组：ZZ=" + item.ZZ + " ");
                        }
                        sb.Append("奇数组总重=" + zz1 + "\r\n");

                        foreach (var item in evenList)
                        {
                            zz2 += item.ZZ;
                            item.TimeCha = usedTimeCha;
                            sb.Append(" 偶数组：ZZ= " + item.ZZ + " ");
                        }
                        sb.Append("偶数组总重=" + zz2 + "\r\n");
                        zh_st_zdczb.ZZ = zz2;//暂定组，比较后还需要判断是否需要取单侧称板数为偶数一侧的

                        Console.WriteLine("奇数组总重=" + zz1 + ",偶数组总重=" + zz2); ;
                        logger.Info(sb.ToString());

                        ms = evenList[0].TimeCha * 40 / 1000;
                        s = ms / 1000;
                        m = s / 60;
                        h = m / 60;
                        zh_st_zdczb.CS = (Decimal)(0.0017 / h);
                        break;
                    case 3:
                        zz1 = 0;
                        zz2 = 0;
                        sb.Append("时间差为：" + evenList[0].TimeCha);
                        foreach (var item in oddList)
                        {
                            zz1 += item.ZZ;
                            item.TimeCha = usedTimeCha;
                            sb.Append(" 奇数组：ZZ= " + item.ZZ + " ");
                        }
                        sb.Append("奇数组总重=" + zz1 + "\r\n");

                        foreach (var item in evenList)
                        {
                            zz2 += item.ZZ;
                            item.TimeCha = usedTimeCha;
                            sb.Append(" 偶数组：ZZ= " + item.ZZ + " ");
                        }
                        sb.Append("偶数组总重=" + zz2 + "\r\n");

                        zh_st_zdczb.ZZ = zz1 > zz2 ? zz1 : zz2;

                        if (zz1 > zz2)
                        {
                            switch (sbOddCBTDH.ToString())
                            {
                                case "13":
                                case "31":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara13;
                                    break;
                                case "35":
                                case "53":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara35;
                                    break;
                                case "57":
                                case "75":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara57;
                                    break;
                                case "79":
                                case "97":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara79;
                                    break;
                                case "911":
                                case "119":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara911;
                                    break;
                            }
                        }
                        else
                        {
                            switch (sbEvenCBTDH.ToString())
                            {
                                case "24":
                                case "42":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara24;
                                    break;
                                case "46":
                                case "64":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara46;
                                    break;
                                case "68":
                                case "86":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara68;
                                    break;
                                case "810":
                                case "108":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara810;
                                    break;
                            }
                        }
                        Console.WriteLine("奇数组总重=" + zz1 + ",偶数组总重=" + zz2);
                        logger.Info(sb.ToString());

                        ms = oddList[0].TimeCha * 40 / 1000;
                        s = ms / 1000;
                        m = s / 60;
                        h = m / 60;
                        zh_st_zdczb.CS = (Decimal)(0.0017 / h);
                        break;
                    default:
                        break;
                }
                sb.Clear();
                #endregion

                zh_st_zdczb.CS = (int)zh_st_zdczb.CS;
                zh_st_zdczb.CD = CD;

                if (zh_st_zdczb.CS > 100)
                {
                    CSErrorCount++;
                    logger.Info("车速=" + zh_st_zdczb.CS + " 车速大于100，认为异常，抛弃 =" + CSErrorCount);
                    Console.WriteLine("车速=" + zh_st_zdczb.CS + " 车速大于100，认为异常，抛弃 =" + CSErrorCount);
                    logger.Warn("车速=" + zh_st_zdczb.CS + " 车速大于100，认为异常，抛弃= " + CSErrorCount);
                    return;
                }

                #region 计算其他轴速度

                /*经过分析的计算方法如下：
                 * 过车序列6轴 5 7 6 8 4 5 7 5 7 4 6 8 4 6 8 7 5 5 7 6 8 4 7 5 4 6 8 4 6 8
                 * 奇数组：5 7 5 7 5 7 7 5 5 7 7 5 
                 * 偶数组：6 8 4 4 6 8 4 6 8 6 8 4 4 6 8 4 6 8
                 * 方法为把整个过车序列按顺序进行奇偶分组，相邻的奇偶分为一组
                 * 如果该奇偶组在分组过程中，遇到的数该组中已经存在，则分配到
                 * 对应的奇偶组的下一组中，然后按组进行时间差的计算
                 * 也可以先按奇偶分组，之后再分别对奇偶组的数重新遍历分组，遇到
                 * 相同的数据则分到下一组
                 *  例如上面这个序列应划分如下
                 *      奇数列           偶数列
                 *   1.  5,7             6,8,4
                 *   2.  5,7             4,6,8
                 *   3.  5,7             4,6,8
                 *   4.  7,5             6,8,4
                 *   5.  5,7             4,6,8
                 *   6.  7,5             4,6,8
                 */

                #region 分组称板
                //奇数组
                List<List<CBData>> oddGroupList = GetCBGroup(oddList);
                List<List<CBData>> evenGroupList = GetCBGroup(evenList);

                #endregion

                int[] TimeChaOther = new int[zh_st_zdczb.ZS];
                Decimal[] SpeedOther = new decimal[zh_st_zdczb.ZS];
                Decimal[] msOther = new decimal[zh_st_zdczb.ZS];

                Decimal[] SpeedOtherMiSencond = new decimal[zh_st_zdczb.ZS];

                //加速度=轴的速度差/轴的时间 差
                decimal[] JiaSpeed = new decimal[zh_st_zdczb.ZS - 1];

                switch (zh_st_zdczb.ZS)
                {
                    case 2:
                        TimeChaOther[0] = Math.Abs(oddGroupList[0][0].Time - evenGroupList[0][0].Time);
                        TimeChaOther[1] = Math.Abs(oddGroupList[1][0].Time - evenGroupList[1][0].Time);

                        msOther[0] = (Decimal)(TimeChaOther[0] * 40 / 1000.0);
                        msOther[1] = (Decimal)(TimeChaOther[1] * 40 / 1000.0);
                        break;
                    case 3:
                        TimeChaOther[0] = Math.Abs(oddGroupList[0][0].Time - evenGroupList[0][0].Time);
                        TimeChaOther[1] = Math.Abs(oddGroupList[1][0].Time - evenGroupList[1][0].Time);
                        TimeChaOther[2] = Math.Abs(oddGroupList[2][0].Time - evenGroupList[2][0].Time);

                        msOther[0] = (Decimal)(TimeChaOther[0] * 40 / 1000.0);
                        msOther[1] = (Decimal)(TimeChaOther[1] * 40 / 1000.0);
                        msOther[2] = (Decimal)(TimeChaOther[2] * 40 / 1000.0);
                        break;
                    case 4:
                        TimeChaOther[0] = Math.Abs(oddGroupList[0][0].Time - evenGroupList[0][0].Time);
                        TimeChaOther[1] = Math.Abs(oddGroupList[1][0].Time - evenGroupList[1][0].Time);
                        TimeChaOther[2] = Math.Abs(oddGroupList[2][0].Time - evenGroupList[2][0].Time);
                        TimeChaOther[3] = Math.Abs(oddGroupList[3][0].Time - evenGroupList[3][0].Time);

                        msOther[0] = (Decimal)(TimeChaOther[0] * 40 / 1000.0);
                        msOther[1] = (Decimal)(TimeChaOther[1] * 40 / 1000.0);
                        msOther[2] = (Decimal)(TimeChaOther[2] * 40 / 1000.0);
                        msOther[3] = (Decimal)(TimeChaOther[3] * 40 / 1000.0);
                        break;
                    case 5:
                        TimeChaOther[0] = Math.Abs(oddGroupList[0][0].Time - evenGroupList[0][0].Time);
                        TimeChaOther[1] = Math.Abs(oddGroupList[1][0].Time - evenGroupList[1][0].Time);
                        TimeChaOther[2] = Math.Abs(oddGroupList[2][0].Time - evenGroupList[2][0].Time);
                        TimeChaOther[3] = Math.Abs(oddGroupList[3][0].Time - evenGroupList[3][0].Time);
                        TimeChaOther[4] = Math.Abs(oddGroupList[4][0].Time - evenGroupList[4][0].Time);

                        msOther[0] = (Decimal)(TimeChaOther[0] * 40 / 1000.0);
                        msOther[1] = (Decimal)(TimeChaOther[1] * 40 / 1000.0);
                        msOther[2] = (Decimal)(TimeChaOther[2] * 40 / 1000.0);
                        msOther[3] = (Decimal)(TimeChaOther[3] * 40 / 1000.0);
                        msOther[4] = (Decimal)(TimeChaOther[4] * 40 / 1000.0);
                        break;
                    case 6:
                        TimeChaOther[0] = Math.Abs(oddGroupList[0][0].Time - evenGroupList[0][0].Time);
                        TimeChaOther[1] = Math.Abs(oddGroupList[1][0].Time - evenGroupList[1][0].Time);
                        TimeChaOther[2] = Math.Abs(oddGroupList[2][0].Time - evenGroupList[2][0].Time);
                        TimeChaOther[3] = Math.Abs(oddGroupList[3][0].Time - evenGroupList[3][0].Time);
                        TimeChaOther[4] = Math.Abs(oddGroupList[4][0].Time - evenGroupList[4][0].Time);
                        TimeChaOther[5] = Math.Abs(oddGroupList[5][0].Time - evenGroupList[5][0].Time);

                        msOther[0] = (Decimal)(TimeChaOther[0] * 40 / 1000.0);
                        msOther[1] = (Decimal)(TimeChaOther[1] * 40 / 1000.0);
                        msOther[2] = (Decimal)(TimeChaOther[2] * 40 / 1000.0);
                        msOther[3] = (Decimal)(TimeChaOther[3] * 40 / 1000.0);
                        msOther[4] = (Decimal)(TimeChaOther[4] * 40 / 1000.0);
                        msOther[5] = (Decimal)(TimeChaOther[5] * 40 / 1000.0);
                        break;

                    default:
                        TimeChaOther[0] = Math.Abs(oddGroupList[0][0].Time - evenGroupList[0][0].Time);
                        TimeChaOther[1] = Math.Abs(oddGroupList[1][0].Time - evenGroupList[1][0].Time);
                        TimeChaOther[2] = Math.Abs(oddGroupList[2][0].Time - evenGroupList[2][0].Time);
                        TimeChaOther[3] = Math.Abs(oddGroupList[3][0].Time - evenGroupList[3][0].Time);
                        TimeChaOther[4] = Math.Abs(oddGroupList[4][0].Time - evenGroupList[4][0].Time);
                        TimeChaOther[5] = Math.Abs(oddGroupList[5][0].Time - evenGroupList[5][0].Time);

                        msOther[0] = (Decimal)(TimeChaOther[0] * 40 / 1000.0);
                        msOther[1] = (Decimal)(TimeChaOther[1] * 40 / 1000.0);
                        msOther[2] = (Decimal)(TimeChaOther[2] * 40 / 1000.0);
                        msOther[3] = (Decimal)(TimeChaOther[3] * 40 / 1000.0);
                        msOther[4] = (Decimal)(TimeChaOther[4] * 40 / 1000.0);
                        msOther[5] = (Decimal)(TimeChaOther[5] * 40 / 1000.0);

                        break;
                }

                decimal sOther, mOther, hOhter;
                sb.Clear();
                for (int i = 0; i < zh_st_zdczb.ZS; i++)
                {
                    sOther = (Decimal)(msOther[i] / (Decimal)1000.0);

                    SpeedOtherMiSencond[i] = (Decimal)((Decimal)1.7 / sOther);
                    sb.Append("第【" + (i + 1) + "】轴速度=" + SpeedOtherMiSencond[i] + "米/秒\r\n");

                    mOther = sOther / 60;
                    hOhter = mOther / 60;
                    SpeedOther[i] = (Decimal)0.0017 / hOhter;

                    sb.Append("第【" + (i + 1) + "】轴速度=" + SpeedOther[i] + "千米/时\r\n");
                }

                Console.WriteLine(sb.ToString());
                logger.Info(sb.ToString());

                //计算加速度
                sb.Clear();

                decimal[] jiaSpeedTimeCha = new decimal[zh_st_zdczb.ZS - 1];
                decimal[] speedCha = new decimal[zh_st_zdczb.ZS - 1];

                Console.WriteLine(" 板数=" + oddGroupList.Count);

                for (int i = 0; i < oddGroupList.Count - 1; i += 1)
                {
                    int skc = oddGroupList[i + 1][0].Time - oddGroupList[i][0].Time;
                    sb.Append("加速度时刻差值=" + skc);
                    //Console.WriteLine(sb.ToString());

                    msOther[0] = (Decimal)(skc * 40 / 1000.0);
                    sOther = msOther[i] / (Decimal)1000.0;
                    sb.Append("加速度时刻差算为秒=" + sOther);
                    //Console.WriteLine(sb.ToString());

                    decimal sdc = SpeedOtherMiSencond[i + 1] - SpeedOtherMiSencond[i];
                    sb.Append("速度差=" + sdc);
                    JiaSpeed[i] = sdc / sOther;
                    //Console.WriteLine(sb.ToString());

                    sb.Append("第【" + (i + 1) + "】轴加速度=" + JiaSpeed[i] + " 米/秒\r\n");
                    //Console.WriteLine(sb.ToString());
                }

                Console.WriteLine(sb.ToString());
                logger.Info(sb.ToString());

                #endregion

                #region 根据速度修正重量
                try
                {
                    int ZZXZPos = (int)(zh_st_zdczb.CS % SysSet.SpeedXZInterver);
                    if (ZZXZPos > 0)
                    {//取模大于0，说明有余数，修正位置段应该为除数的商+1
                        ZZXZPos = (int)(Convert.ToUInt32(zh_st_zdczb.CS) / SysSet.SpeedXZInterver) + 1;
                    }
                    else
                    {
                        ZZXZPos = (int)(Convert.ToUInt32(zh_st_zdczb.CS) / SysSet.SpeedXZInterver);
                    }

                    if (SysSet.SpeedXZPara.Length >= ZZXZPos)
                    {
                        Console.WriteLine("速度=" + zh_st_zdczb.CS + ",修正参数=" + SysSet.SpeedXZPara[ZZXZPos - 1] + ",检测重量=" + zh_st_zdczb.ZZ);
                        logger.Info("速度=" + zh_st_zdczb.CS + ",修正参数=" + SysSet.SpeedXZPara[ZZXZPos - 1] + ",检测重量=" + zh_st_zdczb.ZZ);

                        zh_st_zdczb.ZZ = (long)(zh_st_zdczb.ZZ * SysSet.SpeedXZPara[ZZXZPos - 1]);
                        Console.WriteLine("修正后总重=" + zh_st_zdczb.ZZ);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("速度修正异常" + ex.ToString());
                    logger.Debug("速度修正异常" + ex.ToString(), ex);
                }
                #endregion


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger.Debug(ex.ToString() + "\r\n" + ex.StackTrace);
            }


            //倒序排列牌识结果，有利用匹配到最新的那条识别结果
            //Thread.CurrentThread.Join(600);//延时匹配，手动抓拍牌识返回慢

            #region 匹配车牌
            try
            {


                bool isCheck = false;
                //cpsbResHead = cpsbResHead.ToList().OrderByDescending(x => x.CPSB_Time).ToArray();
                //DateTime dt = DateTime.Now;

                ////int index = 0;
                //foreach (var item in cpsbResHead)
                //{
                //    Console.WriteLine("车牌号:" + item.CPH + ",时间=" + item.CPSB_Time + ",IP=" + item.DeviceIP + ",车道=" + item.CD_GDW);
                //    if ((dt - item.CPSB_Time).TotalSeconds >= 5)
                //    {
                //        //InitCpsbRes(index);
                //        //item.CPSB_Flag = true;
                //        InitCpsbRes(item.CPSB_Time);
                //    }
                //    //index++;
                //}

                //Console.WriteLine("称板给出的方向=" + zh_st_zdczb.FX + ",CD=" + CD + ",牌识ip=" + SysSet.CpsbList[CheckPSIndex - 1].IP);
                //logger.Info("称板给出的方向=" + zh_st_zdczb.FX + ",CD=" + CD + ",牌识ip=" + SysSet.CpsbList[CheckPSIndex - 1].IP);
                //CheckCpsb(CD, ref zh_st_zdczb, CheckPSIndex);  

                #region 匹配称重方向正向行驶的图片 
                if (CheckPSIndex == 0)
                {
                    for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
                    {
                        if (!cpsbResHead[i].CPSB_Flag && cpsbResHead[i].DeviceIP == SysSet.CpsbList[0].IP && cpsbResHead[i].CD_GDW == CD)
                        //&& Math.Abs((cpsbResHead[i].CPSB_Time - ListData[0].RecvTime).TotalMilliseconds) <= 500))//需增加车道的判断
                        {

                            zh_st_zdczb.CPH = cpsbResHead[i].CPH;
                            zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
                            zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
                            zh_st_zdczb.PLATE = cpsbResHead[i].Plate;

                            //InitCpsbRes(cpsbResHead[i].CPSB_Time);
                            InitCpsbRes(i, 0);
                            isCheck = true;
                            //Console.WriteLine("匹配图片=" + zh_st_zdczb.CPTX);
                            logger.Info("匹配图片=" + zh_st_zdczb.CPTX);
                            break;
                        }
                    }
                }
                #endregion

                #region 匹配称重方向逆行的图片
                //else if (CheckPSIndex == 1)
                //{
                //if (!cpsbResHead[i].CPSB_Flag && cpsbResHead[i].DeviceIP == SysSet.CpsbList[1].IP && cpsbResHead[i].CD_GDW == CD)
                ////&& Math.Abs((cpsbResHead[i].CPSB_Time - ListData[0].RecvTime).TotalMilliseconds) <= 500))//需增加车道的判断
                //{

                //    zh_st_zdczb.CPH = cpsbResHead[i].CPH;
                //    zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
                //    zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
                //    zh_st_zdczb.PLATE = cpsbResHead[i].Plate;

                //    InitCpsbRes(i, 0);
                //    isCheck = true;
                //    Console.WriteLine("匹配图片=" + zh_st_zdczb.CPTX);
                //    logger.Info("匹配图片=" + zh_st_zdczb.CPTX);

                #region 匹配车尾和侧面
                //for (int k = 0; k < cpsbResTail.Count(); k++)
                //{
                //    if (cpsbResTail[k].picNum == CD)
                //    {
                //        zh_st_zdczb.CWTX = cpsbResTail[k].photoPath;
                //        InitCpsbRes(i, 1);
                //        break;
                //    }
                //}
                //for (int j = 0; j < cpsbResSide.Count(); j++)
                //{
                //    if (cpsbResSide[j].picNum == CD)
                //    {
                //        zh_st_zdczb.CMTX = cpsbResSide[j].photoPath;
                //        InitCpsbRes(i, 2);
                //        break;
                //    }
                //}
                #endregion

                //break;
                //}
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("匹配车牌异常" + ex.ToString());
                logger.Debug("匹配车牌异常", ex);
            }

            #endregion


            #region 计算车辆超限数据

            //if (zh_st_zdczb.ZS >= 6)
            //{
            //    zh_st_zdczb.XZ = 55 * 1000;
            //}
            //else
            //{
            //    zh_st_zdczb.XZ = zh_st_zdczb.ZS * 10000;
            //}

            switch (zh_st_zdczb.ZS)
            {
                case 2:
                    zh_st_zdczb.XZ = SysSet.Axle2WeightMax;
                    break;
                case 3:
                    zh_st_zdczb.XZ = SysSet.Axle3WeightMax;
                    break;
                case 4:
                    zh_st_zdczb.XZ = SysSet.Axle4WeightMax;
                    break;
                case 5:
                    zh_st_zdczb.XZ = SysSet.Axle5WeightMax;
                    break;
                case 6:
                    zh_st_zdczb.XZ = SysSet.Axle6WeightMax;
                    break;
                default:
                    zh_st_zdczb.XZ = SysSet.Axle6WeightMax;
                    break;
            }

            if (zh_st_zdczb.ZZ > zh_st_zdczb.XZ)
            {
                zh_st_zdczb.CXL = (int)((zh_st_zdczb.ZZ - zh_st_zdczb.XZ) * 100 / zh_st_zdczb.XZ);
                zh_st_zdczb.XHZL = zh_st_zdczb.ZZ - zh_st_zdczb.XZ;
            }
            else
            {
                zh_st_zdczb.CXL = 0;
                zh_st_zdczb.XHZL = 0;
            }

            if (zh_st_zdczb.XHZL > SysSet.MinCXBZ)
            {
                zh_st_zdczb.SFCX = 1;

                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => LedContentFrame(zh_st_zdczb)));

            }
            else
            {
                zh_st_zdczb.SFCX = 0;
            }

            if (zh_st_zdczb.CXL > 150)
            {
                SuperRangeCount++;
                logger.Info("超限率=" + zh_st_zdczb.CXL + ", 大于150，抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);
                Console.WriteLine("超限率=" + zh_st_zdczb.CXL + ", 大于150， 抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);

                logger.Warn("超限率=" + zh_st_zdczb.CXL + ", 大于150，抛弃=" + SuperRangeCount);
                return;
            }
            zh_st_zdczb.CZY = SysSet.ZDMC;
            zh_st_zdczb.FJBZ = 0;
            //switch (CD)
            //{
            //    case 1:
            //    case 2:
            //        zh_st_zdczb.FX = "文水→祁县";
            //        break;
            //    case 3:
            //    case 4:
            //        zh_st_zdczb.FX = "祁县→文水";
            //        break;
            //    default:
            //        break;
            //}

            zh_st_zdczb.JCSJ = DateTime.Now;
            zh_st_zdczb.JCZT = 0;
            zh_st_zdczb.SFXZ = 0;
            zh_st_zdczb.SJDJ = 0;
            zh_st_zdczb.ZDBZ = SysSet.ZDIP;
            zh_st_zdczb.ZX = 0;
            zh_st_zdczb.SFSC = 0;

            #endregion

            try
            {
                tData.DBData = zh_st_zdczb;

                //for (int i = 0; i < tData.ListCB.Count(); i++)
                //{
                //    InsertDgvRow(tData.ListCB[i].TDH.ToString(), tData.ListCB[i].JLZ.ToString(), tData.ListCB[i].Time.ToString(), "",
                //         zh_st_zdczb.ZZ.ToString(), tData.ListCB[i].CKZ.ToString(), tData.ListCB[i].RecvTime.ToString());

                //}


                if ((zh_st_zdczb.ZZ > SysSet.MinZL && zh_st_zdczb.ZZ < SysSet.MaxZL) && (zh_st_zdczb.ZS >= SysSet.MinZS && zh_st_zdczb.ZS <= SysSet.MaxZS))
                {
                    if (!DBInsert(zh_st_zdczb))
                    {
                        logger.Debug("入库失败！" + zh_st_zdczb.ToString());
                        DBInFailedCount++;
                        logger.Warn("入库失败！= " + DBInFailedCount);
                    }
                    DBInSuccessCount++;
                    logger.Warn("入库成功！= " + DBInSuccessCount);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        ShowDataOnListview(zh_st_zdczb);
                    }));


                    if (zh_st_zdczb.CPH != "无车牌" && zh_st_zdczb.SFCX == 1)
                    {
                        LedContentFrame(zh_st_zdczb);
                    }

                }
                else
                {
                    SuperRangeCount++;
                    logger.Info("总重或轴数不在设定的范围内，抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);
                    Console.WriteLine("总重或轴数不在设定的范围内，抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);

                    logger.Warn("总重或轴数不在设定的范围内，抛弃=" + SuperRangeCount);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger.Debug(ex.ToString(), ex);
            }
        }
        #endregion

        #region 根据每个轴的时刻差计算总重
        /// <summary>
        /// 根据每个轴的时刻差计算重量
        /// </summary>
        /// <param name="ListData">称板数据列表</param>
        /// <param name="CD">车道号</param>
        private void GetZZFZ(List<CBData> ListData, int CD)
        {
            if (SysSet.CZYName == "四方")
            {
                GetZZFZSiFang(ListData, CD);
            }
            else if (SysSet.CZYName == "华驰")
            {
                GetZZFZHuaChi(ListData, CD);
            }

        }

        /// <summary>
        /// 根据每个轴的时刻差计算重量,四方仪表使用
        /// </summary>
        /// <param name="ListData">称板数据列表</param>
        /// <param name="CD">车道号</param>
        private void GetZZFZSiFang(List<CBData> ListData, int CD)
        {
            #region 变量 

            StringBuilder sb = new StringBuilder();

            //奇数列使用的哪两块板计算
            StringBuilder sbOddCBTDH = new StringBuilder();
            //偶数列使用的哪两块板计算
            StringBuilder sbEvenCBTDH = new StringBuilder();

            foreach (var item in ListData)
            {
                sb.Append(item.TDH + " ");
            }
            Console.WriteLine("用于计算的称板：" + sb.ToString());
            logger.Info("用于计算的称板：" + sb.ToString());
            if (sb.Length < 8)
            {
                CBNotEnoughCount++;//数量++

                Console.WriteLine("用于计算的称板不够计算使用，丢弃=" + CBNotEnoughCount);
                logger.Info("用于计算的称板不够计算使用，丢弃=" + CBNotEnoughCount);
                return;
            }

            ZH_ST_ZDCZB zh_st_zdczb = new ZH_ST_ZDCZB();
            int usedCBData = 3;//默认对比
            TestData tData = new TestData();

            bool isTime = false;
            int usedTimeCha = 0; //用于计算的时间差

            //车速为km/h即千米/小时 
            double ms = 0, s, m, h;
            //奇数编号的称板队列
            List<CBData> oddList = new List<CBData>();
            //偶数编号的称板队列
            List<CBData> evenList = new List<CBData>();
            /*
             * 比较奇数列和偶数列压板数，如果一侧压板数为奇数，一侧为偶数，
             * 侧取压板数为偶数侧的重量作为总重，若两侧压板数都是偶数，侧
             * 比较后取较重的
             * 轴数使用偶数侧的总板数除以2
             */
            //奇数列压板数
            int oddCBNo = 0;
            //偶数列压板数
            int evenCBNo = 0;
            int oddCBCount = 0;
            int evenCBCount = 0;
            int CheckPSIndex = 0;//首先匹配配置中的第几个相机

            #endregion

            LogMainTable logMainTable = new LogMainTable();
            //LogDetailTable logDetailTable = new LogDetailTable();

            logMainTable.LogID = DateTime.Now.ToString("yyyyMMddHHmmsss");
            logMainTable.LogDetailTables = new List<LogDetailTable>();

            zh_st_zdczb.CPH = "无车牌";
            zh_st_zdczb.CPYS = "无";
            zh_st_zdczb.CPTX = "";
            zh_st_zdczb.PLATE = "";
            zh_st_zdczb.CWTX = " ";
            zh_st_zdczb.CMTX = " ";
            zh_st_zdczb.Video = " ";

            //手动截图
            ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManualSnapZX(2, true, ref zh_st_zdczb)));
            ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManualSnapZX(3, false, ref zh_st_zdczb)));
            //StartManuanSnapZX(2, true, ref zh_st_zdczb);
            //StartManuanSnapZX(3, false, ref zh_st_zdczb);

            try
            {
                List<CBData> tmpList = ListData;

                tData.ListCB = tmpList;

                /*
                 * 查询当前计算队列中称板的个数，每个通道的称板只取一个，故查询结果的和就是压板数
                 */
                var o = from r in ListData.AsQueryable()
                        group r by r.TDH into g
                        select g.OrderBy(r => r.TDH).FirstOrDefault();

                //压板数，根据压板数是三，四，五进行不同的计算
                int OnCbCount = o.ToList().Count;
                int RecvCbCount = ListData.Count();

                #region 根据第一块板的方向判断使用匹配哪个方向的车牌相机 

                if (ListData[0].TDH % 2 == 0)
                {//接收到的第一条数据如果为偶数号板，则为称重方向逆行，判断称重方向车尾相机 
                    zh_st_zdczb.FX = SysSet.CpsbList[1].FangXiang;
                    CheckPSIndex = 1;
                }
                else
                {//收到的第一条数据如果为奇数号板,则为称重方向正常行驶 
                    zh_st_zdczb.FX = SysSet.CpsbList[0].FangXiang;
                    CheckPSIndex = 0;
                }
                #endregion
                /*
                * 在计算时刻差时必须使用同一个轮压两侧称板的差值，所以按照现场称板的奇偶分两侧编号的方式
                * 必须是称板编号的差值的绝对值为1的情况下，才认为是同侧轮
                */


                var odd = from q in ListData
                          where q.TDH % 2 != 0
                          select q;
                var even = from q in ListData
                           where q.TDH % 2 == 0
                           select q;

                oddList = odd.ToList();
                evenList = even.ToList();
                oddCBCount = oddList.Count;
                evenCBCount = evenList.Count;

                //经过对比分析，轴数这里取奇数或者偶数称板组中数据较多的一组
                //对该组的称板总数和压板的数进行求商取整作为车辆轴数
                #region 获取奇偶称板总数和每侧的压板数

                #region 计算奇数和偶数列的压板数，然后确定轴数和使用那侧的重量
                o = from r in oddList.AsQueryable()
                    group r by r.TDH into g
                    select g.OrderBy(r => r.TDH).FirstOrDefault();
                oddCBNo = o.ToList().Count();

                foreach (var item in o)
                {
                    sbOddCBTDH.Append(item.TDH);
                }

                logger.Info("奇数列称板总数=" + oddCBCount + ",奇数列压板数=" + oddCBNo);
                Console.WriteLine("奇数列称板总数=" + oddCBCount + ",奇数列压板数=" + oddCBNo);

                o = from r in evenList.AsQueryable()
                    group r by r.TDH into g
                    select g.OrderBy(r => r.TDH).FirstOrDefault();
                evenCBNo = o.ToList().Count();

                foreach (var item in o)
                {
                    sbEvenCBTDH.Append(item.TDH);
                }

                logger.Info("偶数列称板总数=" + evenCBCount + ",偶数列压板数=" + evenCBNo);
                Console.WriteLine("偶数列称板总数=" + evenCBCount + ",偶数列压板数=" + evenCBNo);

                if (oddCBNo % 2 == 0 && evenCBNo % 2 == 0)
                {//两侧压板数都为偶数
                    //zh_st_zdczb.ZS = oddCBCount >= evenCBCount ? oddCBCount / 2 : evenCBCount / 2; ;//可以取任何侧作为算轴依据

                    //if (oddCBCount >= evenCBCount)
                    //{
                    //    Console.WriteLine("两侧压板数都是偶数,但奇数侧总数>=偶数，轴数=奇数(+1)/2");
                    //    zh_st_zdczb.ZS = oddCBCount % 2 == 0 ? oddCBCount / 2 : (oddCBCount + 1) / 2;
                    //}
                    //else
                    //{
                    //    zh_st_zdczb.ZS = evenCBCount % 2 == 0 ? evenCBCount / 2 : (evenCBCount + 1) / 2;
                    //    Console.WriteLine("两侧压板数都是偶数,但偶数侧总数>=奇数，轴数=偶数(+1)/2");
                    //}
                    usedCBData = 3;//奇偶对比，用大的
                }
                else if (oddCBNo % 2 == 0 && evenCBNo % 2 != 0)
                {//奇数列压板数为偶数，偶数侧压板数不是偶数
                    //这里还需要判断总数是否是2的倍数，不是的话需要+1变成2的倍数 
                    //zh_st_zdczb.ZS = oddCBCount % 2 == 0 ? oddCBCount / 2 : (oddCBCount + 1) / 2;
                    //Console.WriteLine("奇数列压板数为偶数,轴数=奇数（+1）/2");
                    usedCBData = 1;//用奇数组计算
                }
                else if (oddCBNo % 2 != 0 && evenCBNo % 2 == 0)
                {//偶数侧压板数为偶数，奇数侧不是偶数
                    //zh_st_zdczb.ZS = evenCBCount % 2 == 0 ? evenCBCount / 2 : (evenCBCount + 1) / 2;
                    ////zh_st_zdczb.ZS = evenCBCount / 2;
                    //Console.WriteLine("偶数列压板数为偶数,轴数=偶数（+1）/2");
                    usedCBData = 2;//用偶数组计算
                }
                else if (oddCBNo % 2 != 0 && evenCBNo % 2 != 0)
                {//如果两侧压板数都不是偶数，这种情况属于丢板了，暂时使用压板数大的一侧+1作为算轴依据
                    //zh_st_zdczb.ZS = oddCBCount > evenCBCount ? (oddCBCount + 1) / 2 : (evenCBCount + 1) / 2;
                    //if (oddCBCount >= evenCBCount)
                    //{
                    //    Console.WriteLine("两侧压板数都不是偶数,但奇数侧总数>=偶数，轴数=奇数(+1)/2");
                    //    zh_st_zdczb.ZS = oddCBCount % 2 == 0 ? oddCBCount / 2 : (oddCBCount + 1) / 2;
                    //}
                    //else
                    //{
                    //    zh_st_zdczb.ZS = evenCBCount % 2 == 0 ? evenCBCount / 2 : (evenCBCount + 1) / 2;
                    //    Console.WriteLine("两侧压板数都不是偶数,但偶数侧总数>=奇数，轴数=偶数(+1)/2");
                    //}
                    usedCBData = 3;
                }
                #endregion

                #region 使用每侧称板数最多的那块板的总数作为轴数的方式

                var query = (from num in
                                 (
                                 from number in ListData
                                 group number.TDH by number.TDH into g
                                 select new
                                 {
                                     number = g.Key,
                                     cnt = g.Count()
                                 }
                             )
                             orderby num.cnt descending
                             select new { num.number, num.cnt }).ToList();
                foreach (var item in query)
                {
                    Console.WriteLine("称板 " + item.number.ToString() + " 次数=" + item.cnt);

                }
                if (query.Count > 0)
                {
                    zh_st_zdczb.ZS = query[0].cnt;
                }

                if (zh_st_zdczb.ZS == 1)
                {
                    zh_st_zdczb.ZS = 2;
                }
                else if (zh_st_zdczb.ZS > 6)
                {
                    zh_st_zdczb.ZS = 6;
                }

                #endregion

                #endregion


                #region 计算其他轴速度

                /*经过分析的计算方法如下：
                 * 过车序列6轴 5 7 6 8 4 5 7 5 7 4 6 8 4 6 8 7 5 5 7 6 8 4 7 5 4 6 8 4 6 8
                 * 奇数组：5 7 5 7 5 7 7 5 5 7 7 5 
                 * 偶数组：6 8 4 4 6 8 4 6 8 6 8 4 4 6 8 4 6 8
                 * 方法为把整个过车序列按顺序进行奇偶分组，相邻的奇偶分为一组
                 * 如果该奇偶组在分组过程中，遇到的数该组中已经存在，则分配到
                 * 对应的奇偶组的下一组中，然后按组进行时间差的计算
                 * 也可以先按奇偶分组，之后再分别对奇偶组的数重新遍历分组，遇到
                 * 相同的数据则分到下一组
                 * 
                 *  例如上面这个序列应划分如下
                 *      奇数列           偶数列
                 *   1.  5,7             6,8,4
                 *   2.  5,7             4,6,8
                 *   3.  5,7             4,6,8
                 *   4.  7,5             6,8,4
                 *   5.  5,7             4,6,8
                 *   6.  7,5             4,6,8
                 */
                /*经过测试还出现如下情况
                 * 5 7 6 8 5 7 6 8 5 7 5 7 4 6 8 4 6 8 
                 * 这个如果按照上面的分法，将会把第一个4板分到偶数的第二组中，造成错误
                 * 1.   5,7             6,8
                 * 2.   5,7             6,8,4
                 * 3.   5,7             6,8,4
                 * 4.   5,7             6,8
                 * 实际分组如下
                 * 1.   5,7             6,8
                 * 2.   5,7             6,8
                 * 3.   5,7             4,6,8
                 * 4.   5,7             4,6,8
                 * 统计后接收时间在100毫秒之内的为同一轴分组数据
                 */

                #region 分组称板
                //奇数组
                List<List<CBData>> oddGroupList = GetCBGroup(oddList);
                List<List<CBData>> evenGroupList = GetCBGroup(evenList);
                #endregion

                int[] TimeChaFenZhou = new int[zh_st_zdczb.ZS];   //分轴时刻差
                Decimal[] SpeedFenZhou = new decimal[zh_st_zdczb.ZS];//分轴速度
                Decimal[] msFenZhou = new decimal[zh_st_zdczb.ZS];

                Decimal[] SpeedFenZhouMiSencond = new decimal[zh_st_zdczb.ZS];//分钟速度单位米/秒

                //加速度=轴的速度差/轴的时间 差
                decimal[] JiaSpeed = new decimal[zh_st_zdczb.ZS - 1];

                TimeChaFenZhou = GetFenZhouTimeCha(zh_st_zdczb.ZS, oddGroupList, evenGroupList);

                Console.WriteLine("时刻差Couunt=" + TimeChaFenZhou.Length);
                sb.Clear();

                for (int i = 0; i < TimeChaFenZhou.Length; i++)
                {
                    switch (i + 1)
                    {
                        case 1:
                            logMainTable.AxisSKC1 = TimeChaFenZhou[0];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC1);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC1);
                            break;
                        case 2:
                            logMainTable.AxisSKC2 = TimeChaFenZhou[1];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC2);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC2);
                            break;
                        case 3:
                            logMainTable.AxisSKC3 = TimeChaFenZhou[2];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC3);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC3);
                            break;
                        case 4:
                            logMainTable.AxisSKC4 = TimeChaFenZhou[3];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC4);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC4);
                            break;
                        case 5:
                            logMainTable.AxisSKC5 = TimeChaFenZhou[4];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC5);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC5);
                            break;
                        case 6:
                            logMainTable.AxisSKC6 = TimeChaFenZhou[5];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC6);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC6);
                            break;
                        case 7:
                            logMainTable.AxisSKC7 = TimeChaFenZhou[6];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC7);
                            break;
                        case 8:
                            logMainTable.AxisSKC8 = TimeChaFenZhou[7];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC8);
                            break;
                    }
                }

                //logger.Info(sb.ToString());

                decimal sOther, mOther, hOhter;
                sb.Clear();
                try
                {
                    for (int i = 0; i < zh_st_zdczb.ZS; i++)
                    {
                        sOther = (Decimal)((TimeChaFenZhou[i] * 40 / 1000.0) / 1000.0);

                        SpeedFenZhouMiSencond[i] = (Decimal)((Decimal)1.7 / sOther);
                        sb.Append("第【" + (i + 1) + "】轴速度=" + SpeedFenZhouMiSencond[i] + "米/秒\r\n");

                        mOther = sOther / 60;
                        hOhter = mOther / 60;
                        SpeedFenZhou[i] = (Decimal)0.0017 / hOhter;

                        sb.Append("第【" + (i + 1) + "】轴速度=" + SpeedFenZhou[i] + "千米/时\r\n");
                    }
                    Console.WriteLine(sb.ToString());
                    logger.Info(sb.ToString());

                }
                catch (Exception ex)
                {
                    logger.Debug(ex, ex.ToString());
                    Console.WriteLine(ex.ToString());
                }



                //计算加速度
                sb.Clear();

                //decimal[] jiaSpeedTimeCha = new decimal[zh_st_zdczb.ZS - 1];
                //decimal[] speedCha = new decimal[zh_st_zdczb.ZS - 1];

                //Console.WriteLine(" 板数=" + oddGroupList.Count);

                //for (int i = 0; i < oddGroupList.Count - 1; i += 1)
                //{
                //    int skc = oddGroupList[i + 1][0].Time - oddGroupList[i][0].Time;
                //    sb.Append("加速度时刻差值=" + skc);
                //    //Console.WriteLine(sb.ToString());

                //    msFenZhou[0] = (Decimal)(skc * 40 / 1000.0);
                //    sOther = msFenZhou[i] / (Decimal)1000.0;
                //    sb.Append("加速度时刻差算为秒=" + sOther);
                //    //Console.WriteLine(sb.ToString());

                //    decimal sdc = SpeedFenZhouMiSencond[i + 1] - SpeedFenZhouMiSencond[i];
                //    sb.Append("速度差=" + sdc);
                //    JiaSpeed[i] = sdc / sOther;
                //    //Console.WriteLine(sb.ToString());

                //    sb.Append("第【" + (i + 1) + "】轴加速度=" + JiaSpeed[i] + " 米/秒\r\n");
                //    //Console.WriteLine(sb.ToString());
                //}

                //Console.WriteLine(sb.ToString());
                //logger.Info(sb.ToString());

                #endregion 

                #region 根据压板数确定最终总量使用奇数侧、偶数侧还是奇偶对比取大的一侧

                long zzOdd = 0;
                long zzEven = 0;
                sb.Clear();
                #region 计算奇数列板重 
                switch (sbOddCBTDH.ToString())
                {
                    case "13":
                    case "31":
                        K = SysSet.K13;
                        break;
                    case "35":
                    case "53":
                        K = SysSet.K35;
                        break;
                    case "57":
                    case "75":
                        K = SysSet.K57;
                        break;
                    case "79":
                    case "97":
                        K = SysSet.K79;
                        break;
                    case "911":
                    case "119":
                        K = SysSet.K911;
                        break;
                }

                //第几组奇偶计算重量就使用第几轴的时刻差
                for (int i = 0; i < oddGroupList.Count; i++)
                {
                    for (int j = 0; j < oddGroupList[i].Count; j++)
                    {

                        //Console.WriteLine("JLZ=" + oddGroupList[i][j].JLZ + ",SKC=" + TimeChaFenZhou[i]);
                        oddGroupList[i][j].ZZ = Convert.ToInt32(K * oddGroupList[i][j].JLZ / TimeChaFenZhou[i]);
                        zzOdd += oddGroupList[i][j].ZZ;
                        //Console.WriteLine("奇数" + i +" "+ zzOdd);

                        LogDetailTable logDetailTable = new LogDetailTable();
                        logDetailTable.CBNo = oddGroupList[i][j].TDH;
                        logDetailTable.CBZZ = oddGroupList[i][j].ZZ;
                        logDetailTable.JLZ = oddGroupList[i][j].JLZ;
                        logDetailTable.SK = oddGroupList[i][j].Time;
                        logDetailTable.LogID = logMainTable.LogID;



                        logMainTable.LogDetailTables.Add(logDetailTable);

                        sb.Append(" 奇数组：ZZ" + oddGroupList[i][j].TDH + "= " + oddGroupList[i][j].ZZ + " ");
                        //Console.WriteLine(sb.ToString());
                    }
                }

                sb.Append("奇数组总重= " + zzOdd + "\r\n");
                Console.WriteLine(sb.ToString());
                #endregion
                sb.Clear();

                #region 计算偶数列板重 
                switch (sbEvenCBTDH.ToString())
                {
                    case "24":
                    case "42":
                        K = SysSet.K24;
                        break;
                    case "46":
                    case "64":
                        K = SysSet.K46;
                        break;
                    case "68":
                    case "86":
                        K = SysSet.K68;
                        break;
                    case "810":
                    case "108":
                        K = SysSet.K810;
                        break;
                }

                //第几组奇偶计算重量就使用第几轴的时刻差
                for (int i = 0; i < evenGroupList.Count; i++)
                {
                    for (int j = 0; j < evenGroupList[i].Count; j++)
                    {
                        evenGroupList[i][j].ZZ = Convert.ToInt32(K * evenGroupList[i][j].JLZ / TimeChaFenZhou[i]);
                        zzEven += evenGroupList[i][j].ZZ;

                        LogDetailTable logDetailTable = new LogDetailTable();
                        logDetailTable.CBNo = evenGroupList[i][j].TDH;
                        logDetailTable.CBZZ = evenGroupList[i][j].ZZ;
                        logDetailTable.JLZ = evenGroupList[i][j].JLZ;
                        logDetailTable.SK = evenGroupList[i][j].Time;
                        logDetailTable.LogID = logMainTable.LogID;

                        logMainTable.LogDetailTables.Add(logDetailTable);

                        sb.Append(" 偶数组：ZZ" + evenGroupList[i][j].TDH + "= " + evenGroupList[i][j].ZZ + " ");
                    }
                }
                sb.Append("偶数组总重= " + zzEven + "\r\n");
                logger.Info(sb.ToString());
                Console.WriteLine(sb.ToString());
                sb.Clear();

                #endregion

                #region 对比取最后的重量 
                switch (usedCBData)
                {
                    case 1:
                        #region 使用奇数侧作为重量 
                        switch (sbOddCBTDH.ToString())
                        {
                            case "13":
                            case "31":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara13;
                                break;
                            case "35":
                            case "53":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara35;
                                break;
                            case "57":
                            case "75":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara57;
                                break;
                            case "79":
                            case "97":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara79;
                                break;
                            case "911":
                            case "119":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara911;
                                break;
                        }

                        zh_st_zdczb.ZZ = zzOdd;
                        break;
                    #endregion
                    case 2:
                        #region 使用偶数列作为重量 
                        switch (sbEvenCBTDH.ToString())
                        {
                            case "24":
                            case "42":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara24;
                                break;
                            case "46":
                            case "64":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara46;
                                break;
                            case "68":
                            case "86":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara68;
                                break;
                            case "810":
                            case "108":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara810;
                                break;
                        }

                        zh_st_zdczb.ZZ = zzEven;
                        break;
                    #endregion
                    case 3:
                        #region 奇偶对比取较重侧 
                        zh_st_zdczb.ZZ = zzOdd > zzEven ? zzOdd : zzEven;

                        if (zzOdd > zzEven)
                        {
                            switch (sbOddCBTDH.ToString())
                            {
                                case "13":
                                case "31":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara13;
                                    break;
                                case "35":
                                case "53":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara35;
                                    break;
                                case "57":
                                case "75":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara57;
                                    break;
                                case "79":
                                case "97":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara79;
                                    break;
                                case "911":
                                case "119":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara911;
                                    break;
                            }
                        }
                        else
                        {
                            switch (sbEvenCBTDH.ToString())
                            {
                                case "24":
                                case "42":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara24;
                                    break;
                                case "46":
                                case "64":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara46;
                                    break;
                                case "68":
                                case "86":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara68;
                                    break;
                                case "810":
                                case "108":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara810;
                                    break;
                            }
                        }
                        break;
                    #endregion
                    default:
                        break;
                }
                sb.Clear();
                #endregion

                #endregion

                #region 取所有分轴的速度平均值的整数作为最后的车速 
                for (int i = 0; i < zh_st_zdczb.ZS; i++)
                {
                    zh_st_zdczb.CS += SpeedFenZhou[i];
                }

                zh_st_zdczb.CS = zh_st_zdczb.CS / zh_st_zdczb.ZS;

                zh_st_zdczb.CS = (int)zh_st_zdczb.CS;
                zh_st_zdczb.CD = CD;

                //if (zh_st_zdczb.CS > 100)
                //{
                //    CSErrorCount++;
                //    logger.Info("车速=" + zh_st_zdczb.CS + " 车速大于100，认为异常，抛弃 =" + CSErrorCount);
                //    Console.WriteLine("车速=" + zh_st_zdczb.CS + " 车速大于100，认为异常，抛弃 =" + CSErrorCount);
                //    logger.Warn("车速=" + zh_st_zdczb.CS + " 车速大于100，认为异常，抛弃= " + CSErrorCount);
                //    return;
                //}
                #endregion 

                #region 根据速度修正重量
                try
                {
                    int ZZXZPos = (int)(zh_st_zdczb.CS % SysSet.SpeedXZInterver);
                    if (ZZXZPos > 0)
                    {//取模大于0，说明有余数，修正位置段应该为除数的商+1
                        ZZXZPos = (int)(Convert.ToUInt32(zh_st_zdczb.CS) / SysSet.SpeedXZInterver) + 1;
                    }
                    else
                    {
                        ZZXZPos = (int)(Convert.ToUInt32(zh_st_zdczb.CS) / SysSet.SpeedXZInterver);
                    }

                    if (SysSet.SpeedXZPara.Length >= ZZXZPos)
                    {
                        Console.WriteLine("速度=" + zh_st_zdczb.CS + ",修正参数=" + SysSet.SpeedXZPara[ZZXZPos - 1] + ",检测重量=" + zh_st_zdczb.ZZ);
                        logger.Info("速度=" + zh_st_zdczb.CS + ",修正参数=" + SysSet.SpeedXZPara[ZZXZPos - 1] + ",检测重量=" + zh_st_zdczb.ZZ);

                        logMainTable.ZZ = (int)zh_st_zdczb.ZZ;
                        logMainTable.XZPara = (decimal)SysSet.SpeedXZPara[ZZXZPos - 1];

                        zh_st_zdczb.ZZ = (long)(zh_st_zdczb.ZZ * SysSet.SpeedXZPara[ZZXZPos - 1]);

                        logMainTable.XZZZ = (int)zh_st_zdczb.ZZ;

                        Console.WriteLine("修正后总重=" + zh_st_zdczb.ZZ);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("速度修正异常" + ex.ToString());
                    logger.Debug("速度修正异常" + ex.ToString(), ex);
                }
                #endregion 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger.Debug(ex.ToString() + "\r\n" + ex.StackTrace);
            }


            //倒序排列牌识结果，有利用匹配到最新的那条识别结果
            //Thread.CurrentThread.Join(600);//延时匹配，手动抓拍牌识返回慢

            #region 匹配车牌
            try
            {


                bool isCheck = false;
                //cpsbResHead = cpsbResHead.ToList().OrderByDescending(x => x.CPSB_Time).ToArray();
                //DateTime dt = DateTime.Now;

                ////int index = 0;
                //foreach (var item in cpsbResHead)
                //{
                //    Console.WriteLine("车牌号:" + item.CPH + ",时间=" + item.CPSB_Time + ",IP=" + item.DeviceIP + ",车道=" + item.CD_GDW);
                //    if ((dt - item.CPSB_Time).TotalSeconds >= 5)
                //    {
                //        //InitCpsbRes(index);
                //        //item.CPSB_Flag = true;
                //        InitCpsbRes(item.CPSB_Time);
                //    }
                //    //index++;
                //}

                //Console.WriteLine("称板给出的方向=" + zh_st_zdczb.FX + ",CD=" + CD + ",牌识ip=" + SysSet.CpsbList[CheckPSIndex - 1].IP);
                //logger.Info("称板给出的方向=" + zh_st_zdczb.FX + ",CD=" + CD + ",牌识ip=" + SysSet.CpsbList[CheckPSIndex - 1].IP);
                //CheckCpsb(CD, ref zh_st_zdczb, CheckPSIndex);  

                #region 匹配称重方向正向行驶的图片 
                if (CheckPSIndex == 0)
                {
                    for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
                    {
                        if (!cpsbResHead[i].CPSB_Flag && cpsbResHead[i].DeviceIP == SysSet.CpsbList[0].IP && cpsbResHead[i].CD_GDW == CD)
                        //&& Math.Abs((cpsbResHead[i].CPSB_Time - ListData[0].RecvTime).TotalMilliseconds) <= 500))//需增加车道的判断
                        {

                            zh_st_zdczb.CPH = cpsbResHead[i].CPH;
                            zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
                            zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
                            zh_st_zdczb.PLATE = cpsbResHead[i].Plate;

                            //InitCpsbRes(cpsbResHead[i].CPSB_Time);
                            InitCpsbRes(i, 0);
                            isCheck = true;
                            //Console.WriteLine("匹配图片=" + zh_st_zdczb.CPTX);
                            logger.Info("匹配图片=" + zh_st_zdczb.CPTX);
                            break;
                        }
                    }
                }
                #endregion

                #region 匹配称重方向逆行的图片
                //else if (CheckPSIndex == 1)
                //{
                //if (!cpsbResHead[i].CPSB_Flag && cpsbResHead[i].DeviceIP == SysSet.CpsbList[1].IP && cpsbResHead[i].CD_GDW == CD)
                ////&& Math.Abs((cpsbResHead[i].CPSB_Time - ListData[0].RecvTime).TotalMilliseconds) <= 500))//需增加车道的判断
                //{

                //    zh_st_zdczb.CPH = cpsbResHead[i].CPH;
                //    zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
                //    zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
                //    zh_st_zdczb.PLATE = cpsbResHead[i].Plate;

                //    InitCpsbRes(i, 0);
                //    isCheck = true;
                //    Console.WriteLine("匹配图片=" + zh_st_zdczb.CPTX);
                //    logger.Info("匹配图片=" + zh_st_zdczb.CPTX);

                #region 匹配车尾和侧面
                //for (int k = 0; k < cpsbResTail.Count(); k++)
                //{
                //    if (cpsbResTail[k].picNum == CD)
                //    {
                //        zh_st_zdczb.CWTX = cpsbResTail[k].photoPath;
                //        InitCpsbRes(i, 1);
                //        break;
                //    }
                //}
                //for (int j = 0; j < cpsbResSide.Count(); j++)
                //{
                //    if (cpsbResSide[j].picNum == CD)
                //    {
                //        zh_st_zdczb.CMTX = cpsbResSide[j].photoPath;
                //        InitCpsbRes(i, 2);
                //        break;
                //    }
                //}
                #endregion

                //break;
                //}
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("匹配车牌异常" + ex.ToString());
                logger.Debug("匹配车牌异常", ex);
            }

            #endregion


            #region 计算车辆超限数据

            //if (zh_st_zdczb.ZS >= 6)
            //{
            //    zh_st_zdczb.XZ = 55 * 1000;
            //}
            //else
            //{
            //    zh_st_zdczb.XZ = zh_st_zdczb.ZS * 10000;
            //}

            switch (zh_st_zdczb.ZS)
            {
                case 2:
                    zh_st_zdczb.XZ = SysSet.Axle2WeightMax;
                    break;
                case 3:
                    zh_st_zdczb.XZ = SysSet.Axle3WeightMax;
                    break;
                case 4:
                    zh_st_zdczb.XZ = SysSet.Axle4WeightMax;
                    break;
                case 5:
                    zh_st_zdczb.XZ = SysSet.Axle5WeightMax;
                    break;
                case 6:
                    zh_st_zdczb.XZ = SysSet.Axle6WeightMax;
                    break;
                default:
                    zh_st_zdczb.XZ = SysSet.Axle6WeightMax;
                    break;
            }

            if (zh_st_zdczb.ZZ > zh_st_zdczb.XZ)
            {
                zh_st_zdczb.CXL = (int)((zh_st_zdczb.ZZ - zh_st_zdczb.XZ) * 100 / zh_st_zdczb.XZ);
                zh_st_zdczb.XHZL = zh_st_zdczb.ZZ - zh_st_zdczb.XZ;
            }
            else
            {
                zh_st_zdczb.CXL = 0;
                zh_st_zdczb.XHZL = 0;
            }

            if (zh_st_zdczb.XHZL > SysSet.MinCXBZ)
            {
                zh_st_zdczb.SFCX = 1;

                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => LedContentFrame(zh_st_zdczb)));

            }
            else
            {
                zh_st_zdczb.SFCX = 0;
            }

            if (zh_st_zdczb.CXL > 150)
            {
                SuperRangeCount++;
                logger.Info("超限率=" + zh_st_zdczb.CXL + ", 大于150，抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);
                Console.WriteLine("超限率=" + zh_st_zdczb.CXL + ", 大于150， 抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);

                logger.Warn("超限率=" + zh_st_zdczb.CXL + ", 大于150，抛弃=" + SuperRangeCount);
                return;
            }
            zh_st_zdczb.CZY = SysSet.ZDMC;
            zh_st_zdczb.FJBZ = 0;
            //switch (CD)
            //{
            //    case 1:
            //    case 2:
            //        zh_st_zdczb.FX = "文水→祁县";
            //        break;
            //    case 3:
            //    case 4:
            //        zh_st_zdczb.FX = "祁县→文水";
            //        break;
            //    default:
            //        break;
            //}

            zh_st_zdczb.JCSJ = DateTime.Now;
            zh_st_zdczb.JCZT = 0;
            zh_st_zdczb.SFXZ = 0;
            zh_st_zdczb.SJDJ = 0;
            zh_st_zdczb.ZDBZ = SysSet.ZDIP;
            zh_st_zdczb.ZX = 0;
            zh_st_zdczb.SFSC = 0;

            #endregion

            #region 入库保存


            try
            {
                tData.DBData = zh_st_zdczb;

                //for (int i = 0; i < tData.ListCB.Count(); i++)
                //{
                //    InsertDgvRow(tData.ListCB[i].TDH.ToString(), tData.ListCB[i].JLZ.ToString(), tData.ListCB[i].Time.ToString(), "",
                //         zh_st_zdczb.ZZ.ToString(), tData.ListCB[i].CKZ.ToString(), tData.ListCB[i].RecvTime.ToString());

                //}

                logMainTable.CS = (int)zh_st_zdczb.CS;
                logMainTable.ZS = zh_st_zdczb.ZS;

                if ((zh_st_zdczb.ZZ > SysSet.MinZL && zh_st_zdczb.ZZ < SysSet.MaxZL) && (zh_st_zdczb.ZS >= SysSet.MinZS && zh_st_zdczb.ZS <= SysSet.MaxZS))
                {
                    if (!DBInsert(zh_st_zdczb))
                    {
                        logger.Debug("入库失败！" + zh_st_zdczb.ToString());
                        DBInFailedCount++;
                        logger.Warn("入库失败！= " + DBInFailedCount);
                    }
                    DBInSuccessCount++;
                    logger.Warn("入库成功！= " + DBInSuccessCount);

                    this.Invoke(new MethodInvoker(delegate
                    {
                        ShowDataOnListview(zh_st_zdczb);
                    }));


                    if (zh_st_zdczb.CPH != "无车牌" && zh_st_zdczb.SFCX == 1)
                    {
                        LedContentFrame(zh_st_zdczb);
                    }

                }
                else
                {
                    SuperRangeCount++;
                    logger.Info("总重或轴数不在设定的范围内，抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);
                    Console.WriteLine("总重或轴数不在设定的范围内，抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);

                    logger.Warn("总重或轴数不在设定的范围内，抛弃=" + SuperRangeCount);
                }

                DALLogMainTable dalLogMainTable = new DALLogMainTable();

                if (dalLogMainTable.Add(logMainTable) > 0)
                {
                    Console.WriteLine("日志信息入库成功");
                }
                else
                {
                    Console.WriteLine("日志信息入库失败");
                    logger.Info("日志信息入库失败");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger.Debug(ex, ex.ToString());
            }
            #endregion
        }
        #endregion

        #region 匹配车牌号 
        /// <summary>
        /// 匹配车牌号
        /// </summary>
        /// <param name="CD">线圈给出的车道号</param>
        /// <param name="zh_st_zdczb">入库对象</param>
        /// <param name="sysCpsbSetIndex">配置文件中牌识的序号</param>
        private void CheckCpsb(int CD, ref ZH_ST_ZDCZB zh_st_zdczb, int sysCpsbSetIndex)
        {
            bool isCheck = false;

            for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
            {
                if (!cpsbResHead[i].CPSB_Flag && (cpsbResHead[i].CD_GDW == CD))//需增加车道的判断
                {
                    zh_st_zdczb.CPH = cpsbResHead[i].CPH;
                    zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
                    zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
                    zh_st_zdczb.PLATE = cpsbResHead[i].Plate;

                    InitCpsbRes(cpsbResHead[i].CPSB_Time);
                    isCheck = true;
                    break;
                }
            }
            #region 二次匹配
            //if (isCheck == false)
            //{//1线圈
            //    for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
            //    {
            //        if (!cpsbResHead[i].CPSB_Flag && cpsbResHead[i].DeviceIP == SysSet.CpsbList[sysCpsbSetIndex - 1].IP)//需增加车道的判断
            //        {
            //            /*二次匹配时区分1,2,3,4道
            //             * 如果1道没匹配到车辆，二次匹配时寻找车道号为2的
            //             * 如果2道没匹配到车辆，二次匹配时寻找车道号为1的
            //             * 如果3道没匹配到车辆，二次匹配时寻找车道号为4的
            //             * 如果4道没匹配到车辆，二次匹配时寻找车道号为3的
            //             * 3和2道观察后决定是否增加匹配
            //             */
            //            switch (CD)
            //            {
            //                case 1:
            //                    if (cpsbResHead[i].CD_GDW == 2)
            //                    {
            //                        zh_st_zdczb.CPH = cpsbResHead[i].CPH;
            //                        zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
            //                        zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
            //                        zh_st_zdczb.PLATE = cpsbResHead[i].Plate;
            //                        InitCpsbRes(cpsbResHead[i].CPSB_Time);
            //                        isCheck = true;
            //                    }
            //                    break;
            //                case 2:
            //                    if (cpsbResHead[i].CD_GDW == 1)
            //                    {
            //                        zh_st_zdczb.CPH = cpsbResHead[i].CPH;
            //                        zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
            //                        zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
            //                        zh_st_zdczb.PLATE = cpsbResHead[i].Plate;
            //                        InitCpsbRes(cpsbResHead[i].CPSB_Time);
            //                        isCheck = true;
            //                    }
            //                    break;
            //                case 3:
            //                    if (cpsbResHead[i].CD_GDW == 4)
            //                    {
            //                        zh_st_zdczb.CPH = cpsbResHead[i].CPH;
            //                        zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
            //                        zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
            //                        zh_st_zdczb.PLATE = cpsbResHead[i].Plate;
            //                        InitCpsbRes(cpsbResHead[i].CPSB_Time);
            //                        isCheck = true;
            //                    }
            //                    break;
            //                case 4:
            //                    if (cpsbResHead[i].CD_GDW == 3)
            //                    {
            //                        zh_st_zdczb.CPH = cpsbResHead[i].CPH;
            //                        zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
            //                        zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
            //                        zh_st_zdczb.PLATE = cpsbResHead[i].Plate;
            //                        InitCpsbRes(cpsbResHead[i].CPSB_Time);
            //                        isCheck = true;
            //                    }
            //                    break;
            //                default:
            //                    break;
            //            }
            //            if (isCheck == true)
            //            {
            //                break;
            //            }
            //        }
            //    }
            //}
            #endregion
        }
        #endregion

        #region 分组奇偶称板，用于计算每轴速度和加速度以及后面计算分轴重
        /// <summary>
        ///  分组奇偶称板，用于计算每轴速度和加速度以及后面计算分轴重
        /// </summary>
        /// <param name="listCB">称板列表</param>
        /// <returns>分组后的称板列表</returns>
        private List<List<CBData>> GetCBGroup(List<CBData> listCB)
        {
            List<List<CBData>> GroupList = new List<List<CBData>>();
            List<CBData> tmpList = new List<CBData>();

            bool haved = false;

            for (int i = 0; i < listCB.Count(); i++)
            {

                if (tmpList.Count == 0)
                {
                    tmpList.Add(listCB[i]);
                    continue;
                }
                for (int j = 0; j < tmpList.Count; j++)
                {
                    if (tmpList[j].TDH == listCB[i].TDH)
                    {
                        haved = true;
                        break;
                    }
                    else if ((tmpList[j].RecvTime - listCB[i].RecvTime).TotalMilliseconds > 100)
                    {//如果当前称板接收时间与该组中的最后一条相距大于100毫秒，则认为其是下一组的数据
                        haved = true;
                        break;
                    }
                }

                if (haved == true)
                {
                    GroupList.Add(tmpList);
                    tmpList = new List<CBData>();
                    tmpList.Add(listCB[i]);
                    haved = false;
                }
                else
                {
                    tmpList.Add(listCB[i]);
                }

                if (i == listCB.Count() - 1 && tmpList.Count > 0)
                {
                    GroupList.Add(tmpList);
                }
            }

            Console.WriteLine("列表总数=" + GroupList.Count);

            Console.WriteLine(GroupList[0][0].JLZ.ToString());

            return GroupList;
        }
        #endregion

        #region 网络触发抓拍
        /// <summary>
        /// 网络触发抓拍
        /// </summary> 
        public void ContinuousShoot(int m_lUserID)
        {
            HCNetSDK.NET_DVR_SNAPCFG _SnapCfg = new HCNetSDK.NET_DVR_SNAPCFG();
            _SnapCfg.wIntervalTime = new ushort[4];
            _SnapCfg.byRelatedDriveWay = 1;
            _SnapCfg.bySnapTimes = 1;
            _SnapCfg.dwSize = (uint)Marshal.SizeOf(_SnapCfg);
            _SnapCfg.wSnapWaitTime = 100;
            _SnapCfg.wIntervalTime[0] = 100;

            if (!HCNetSDK.NET_DVR_ContinuousShoot(m_lUserID, ref _SnapCfg))
            {
                int iErrorNum = (int)HCNetSDK.NET_DVR_GetLastError();
                string s = " 网络触发抓拍失败!错误号: " + iErrorNum;//+ " 错误消息" + HCNetSDK.NET_DVR_GetErrorMsg(ref iErrorNum);
                Console.WriteLine(s);
                logger.Info(s);

            }
            else
            {
                Console.WriteLine(" 网络触发抓拍成功! 关联触发车道号:" + _SnapCfg.byRelatedDriveWay);
            }
        }
        #endregion

        #region 获取分轴的时间差 
        /// <summary>
        /// 获取分轴的时间差
        /// </summary>
        /// <param name="ZS">轴数</param>
        /// <param name="oddGroupList">奇数组的分组数据</param>
        /// <param name="evenGroupList">偶数组的分组数据</param>
        private int[] GetFenZhouTimeCha(int ZS, List<List<CBData>> oddGroupList, List<List<CBData>> evenGroupList)
        {
            int[] TimeChaFenZhou = new int[ZS];
            List<CBData> listOdd = null;
            List<CBData> listEven = null;
            bool ret = false;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < oddGroupList.Count; i++)
            {
                listOdd = oddGroupList[i];
                listEven = evenGroupList[i];
                //groupCBCount = listOdd.Count > listEven.Count ? listOdd.Count : listEven.Count; 

                //直接使用该组第一块接收到的称板数据，不固定左右侧
                if (listOdd[0].TDH == 1)
                {//1号板只能去找2号板的时刻,遍历偶数组该列的数据，找到2板
                    for (int cbEven = 0; cbEven < listEven.Count; cbEven++)
                    {
                        if (listEven[cbEven].TDH == 2)
                        {
                            TimeChaFenZhou[i] = Math.Abs(listOdd[0].Time - listEven[cbEven].Time);
                            break;
                        }
                    }
                }
                else if (listOdd[0].TDH == 11)
                {//11号板只能去找10号板的时刻,遍历偶数组该列的数据，找到2板
                    for (int cbEven = 0; cbEven < listEven.Count; cbEven++)
                    {
                        if (listEven[cbEven].TDH == 10)
                        {
                            TimeChaFenZhou[i] = Math.Abs(listOdd[0].Time - listEven[cbEven].Time);
                            break;
                        }
                    }
                }
                else
                {//其他板先找偶数比奇数板通道号小1的，没有再找大1的
                    for (int cbEven = 0; cbEven < listEven.Count; cbEven++)
                    {
                        if (listOdd[0].TDH - listEven[cbEven].TDH == 1)
                        {
                            TimeChaFenZhou[i] = Math.Abs(listOdd[0].Time - listEven[cbEven].Time);
                            ret = true;
                            break;
                        }
                    }

                    if (ret == false)
                    {//没有找到比这个板小1的数，找大1的数
                        for (int cbEven = 0; cbEven < listEven.Count; cbEven++)
                        {
                            if (listOdd[0].TDH - listEven[cbEven].TDH == -1)
                            {
                                TimeChaFenZhou[i] = Math.Abs(listOdd[0].Time - listEven[cbEven].Time);
                                ret = true;
                                break;
                            }
                        }
                    }
                }
                ret = false;
                sb.Append("第" + (i + 1) + "轴时刻差=" + TimeChaFenZhou[i]);
            }

            logger.Info(sb.ToString());

            //var jj = from p in listEven where (from pp in listOdd select pp.TDH).Contains(p.TDH - 1) select p;  
            //foreach (var item in jj)
            //{
            //    MessageBox.Show(item.ToString());
            //}




            return TimeChaFenZhou;
        }
        #endregion

        #region 找到称板编号差值为1的板 
        /// <summary>
        /// 找到称板编号差值为1的板
        /// </summary>
        /// <param name="listCB"></param>
        /// <param name="TDH"></param>
        /// <returns></returns>
        public bool Judge(List<CBData> listCB, int TDH)
        {
            for (int i = 0; i < listCB.Count; i++)
            {
                if (TDH - listCB[i].TDH == 1)
                {//如果存在奇数-偶数==1的板，说明是同一侧
                    return true;
                }
            }

            return false;
        }
        #endregion


        #region 华驰协议解析及数据处理
        private void CombinationFrameHuaChi()
        {
            Console.WriteLine("开启华驰数据处理线程......");
            byte result = new byte(); 

            while (true)
            {
                ConcurrentQueue<byte> queue = ConQueue;

                try
                {
                    //Console.WriteLine("线程状态：" + Thread.CurrentThread.ThreadState.Equals(System.Threading.ThreadState.Aborted |
                    //    System.Threading.ThreadState.Stopped | System.Threading.ThreadState.Suspended));

                    if (queue.Count < 12)
                    {//GPIO数据最低，12个字节
                     //Thread.Sleep(20);
                        continue;
                    }
                    var query = queue.Take(2);//第2位为命令位 FF 00线圈信号，FF 01称重信号
                    byte[] head = query.ToArray();

                    if (head[0] == 0xFF && head[1] == 0x00)
                    {//线圈信号

                        query = queue.Take(12);
                        head = query.ToArray();
                        if (head[11] == 0xAA && head[10] == 0xAA)
                        {
                            for (int i = 0; i < 12; i++)
                            {
                                queue.TryDequeue(out result);
                            }
                            GetXQZTHuaChi(head);
                        }
                        else
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                queue.TryDequeue(out result);
                            }
                        }
                    }
                    else if (queue.Count >= 28)
                    {
                        if (head[0] == 0xFF && head[1] == 0x01)
                        {
                            byte[] oneByte = new byte[28];
                            query = queue.Take(28);
                            oneByte = query.ToArray();
                            if (oneByte[27] == 0xAA && oneByte[26] == 0xAA)
                            {
                                //移除使用的元素
                                for (int i = 0; i < 28; i++)
                                {
                                    queue.TryDequeue(out result);
                                }
                                CalculateFrameHuaChi(oneByte);
                            }
                            else
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    queue.TryDequeue(out result);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < head.Length; i++)
                            {
                                if (head[i] != 0xFF || head[i] != 0x01)
                                {
                                    queue.TryDequeue(out result);
                                }
                            }
                        }
                    }
                    //Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("解析数据异常");
                    logger.Debug(ex, "解析数据异常" + ex.ToString());
                }
            }
        }

        #region 分析线圈状态
        private void GetXQZTHuaChi(byte[] head)
        {
            StringBuilder sbXQZT = new StringBuilder();
            StringBuilder sbSW = new StringBuilder();
            StringBuilder sbCF = new StringBuilder();
            StringBuilder sbXQSampleNo = new StringBuilder();

            //int s = head[9] + (head[8] << 8) + (head[7] << 16) + head[6] << 24;
            //sbXQSampleNo.Append(s.ToString());

            byte[] sValue = new byte[4];
            Buffer.BlockCopy(head, 6, sValue, 0, 4);
            int s = BitConverter.ToInt32(sValue, 0);
            sbXQSampleNo.Append(s.ToString());

            int dtBegin = -1, dtEnd = -1;
            DateTime dt = DateTime.Now;

            int xqzt = -1; //中间交换值 
            int xqbh = 0;

            #region 分析线圈状态

            for (int i = 1; i < 4; i++)
            {
                xqzt = Common.getIntegerSomeBit(head[3], i - 1);
                sbXQZT.Append(xqzt.ToString());
                if (xqzt == 0)
                {
                    if (XianQuan[i].XQZTChufa == -1)
                    {
                        XianQuan[i].ChufaTime = dt;
                        XianQuan[i].XQZTChufa = 0;
                        sbCF.Append(i + " ");
                        //手动抓拍，由于是中间线圈，触发时还没有判断方向，暂时是向所有登录成功的相机发送指令
                        xqbh = (i + 1);
                        //Console.WriteLine("触发抓拍线圈编号=" + xqbh);
                        //if (xqbh == 1 || xqbh == 2)//1,2线圈触发第一个相机
                        //{
                        //    //ThreadPool.QueueUserWorkItem(x => StartManuanSnap(1, (short)xqbh));
                        //}
                        //else if (xqbh == 3 || xqbh == 4)//2,3线圈触发第二个相机
                        //{
                        //    //ThreadPool.QueueUserWorkItem(x => StartManuanSnap(2, (short)xqbh));
                        //}

                        XianQuan[i].SampleNoChufa = s;
                    }

                }
                else if (xqzt == 1)
                {
                    if (XianQuan[i].XQZTShouwei != 1 && (XianQuan[i].XQZTChufa == 0 || XianQuan[i].XQZTChufa == 2))
                    {
                        XianQuan[i].ShouweiTime = dt;
                        XianQuan[i].XQZTShouwei = 1;
                        sbSW.Append(i + " ");
                        XianQuan[i].SampleNoShouwei = s;
                    }
                }
            }
            Console.WriteLine("原始线圈:" + sbXQZT + ",  " + sbCF.ToString().PadRight(8, ' ') + "触发," + sbSW.ToString().PadRight(8, ' ') + " 收尾,时间:" + dt.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            logger.Info("原始线圈:" + sbXQZT + ",  " + sbCF.ToString().PadRight(8, ' ') + "触发," + sbSW.ToString().PadRight(8, ' ') + " 收尾,时间:" + dt.ToString("yyyy/MM/dd HH:mm:ss.fff"));
            //sbXQZT.Clear();
            sbCF.Clear();
            sbSW.Clear();

            #endregion

            #region 分析相邻线圈是否存在同时触发情况
            /*
             * 同时触发的判断条件如下
             * 1线圈：
             *      在1线圈收尾时判断2线圈是否触发，如果2线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在2线圈触发时间到1线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第5号称板的数据，如果存在说明，1,2线圈对应的为两辆车，1线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车1线圈的触发状态设置为2，且将TS12=true。
             * 2线圈：
             *      21同时
             *      由于循环时是从0开始的，这里增加TS12的判断，如果1线圈已经判断过了，2线圈直接跳过即可。
             *      在2线圈收尾时判断1线圈是否触发，如果1线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在2线圈触发时间到2线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第5号称板的数据，如果存在说明，1,2线圈对应的为两辆车，2线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车2线圈的触发状态设置为2，且将TS12=true。
             *      23同时
             *      在2线圈收尾时判断3线圈是否触发，如果3线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在2线圈触发时间到2线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第3、4、9号称板的数据，如果存在说明，2,3线圈对应的为两辆车，2线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车2线圈的触发状态设置为2，且将TS23=true。
             * 3线圈：
             *      32同时
             *      由于循环时是从0开始的，这里增加TS23的判断，如果2线圈已经判断过了，3线圈直接跳过即可。
             *      在3线圈收尾时判断2线圈是否触发，如果2线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在3线圈触发时间到3线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第3、4、9号称板的数据，如果存在说明，3,2线圈对应的为两辆车，3线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车3线圈的触发状态设置为2，且将TS23=true。
             *      34同时
             *      在3线圈收尾时判断4线圈是否触发，如果4线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在3线圈触发时间到3线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第8号称板的数据，如果存在说明，3,4线圈对应的为两辆车，3线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车3线圈的触发状态设置为2，且将TS34=true。
             * 4线圈：
             *      43同时
             *      由于循环时是从0开始的，这里增加TS34的判断，如果3线圈已经判断过了，4线圈直接跳过即可。
             *      在4线圈收尾时判断3线圈是否触发，如果3线圈也是触发状态且他们之间的时间差在设定的
             *      范围内，再去查询在4线圈触发时间到4线圈当前收尾时间段内，称板缓存队列中是否存在
             *      第8号称板的数据，如果存在说明，4,3线圈对应的为两辆车，4线圈的触发状态还是设置为1;
             *      如果不存在，说明他们对应的为一辆车4线圈的触发状态设置为2，且将TS34=true。
             */
            for (int i = 0; i < sbXQZT.Length; i++)
            {
                if (sbXQZT[i] == '1')
                {
                    switch (i + 1)
                    {
                        case 1:
                            #region 判断线圈1
                            if (XianQuan[1].XQZTChufa == 0 && XianQuan[2].XQZTChufa == 0 && Math.Abs(XianQuan[1].SampleNoChufa - XianQuan[2].SampleNoChufa) <= 12)
                            {//如果1和2都触发，且时间间隔小于设定的值，认为同时触发 
                                if (TS12 == false && GetCBDataListHuaChi(new int[] { 1 }, XianQuan[1].SampleNoChufa, XianQuan[1].SampleNoShouwei, false).Count == 0)
                                {//1收尾但是没有1号称板，认为是同一辆车
                                    TS12 = true;
                                    XianQuan[1].XQZTChufa = XianQuan[2].XQZTChufa = 2;

                                    Console.WriteLine("线圈 1 2 同时触发，SampleNoChufa为：" + XianQuan[1].SampleNoChufa);
                                    logger.Info("线圈 1 2 同时触发，SampleNoChufa为：" + XianQuan[1].SampleNoChufa);
                                }
                            }
                            break;
                        #endregion
                        case 2:
                            #region 判断线圈2
                            if (XianQuan[2].XQZTChufa == 0 && XianQuan[1].XQZTChufa == 0 && Math.Abs(XianQuan[2].SampleNoChufa - XianQuan[1].SampleNoChufa) <= 12)
                            {//12同时触发 
                                if (TS12 == false && GetCBDataListHuaChi(new int[] { 7 }, XianQuan[2].SampleNoChufa, XianQuan[2].SampleNoShouwei, false).Count == 0)
                                {
                                    TS12 = true;
                                    XianQuan[2].XQZTChufa = XianQuan[1].XQZTChufa = 2;

                                    Console.WriteLine("线圈 2 1 同时触发，SampleNoChufa为：" + XianQuan[2].SampleNoChufa);
                                    logger.Info("线圈 2 1 同时触发，SampleNoChufa为：" + XianQuan[2].SampleNoChufa);
                                }
                            }
                            else if (XianQuan[2].XQZTChufa == 0 && XianQuan[3].XQZTChufa == 0 && Math.Abs(XianQuan[2].SampleNoChufa - XianQuan[3].SampleNoChufa) <= 12)
                            {//23同时触发 
                                if (TS23 == false && GetCBDataListHuaChi(new int[] { 11 }, XianQuan[2].SampleNoChufa, XianQuan[2].SampleNoShouwei, false).Count == 0)
                                {
                                    TS23 = true;
                                    XianQuan[2].XQZTChufa = XianQuan[3].XQZTChufa = 2;

                                    Console.WriteLine("线圈 2 3 同时触发，SampleNoChufa为：" + XianQuan[2].SampleNoChufa);
                                    logger.Info("线圈 2 3 同时触发，SampleNoChufa为：" + XianQuan[2].SampleNoChufa);
                                }
                            }
                            break;
                        #endregion
                        case 3:
                            #region 判断线圈3
                            if (XianQuan[3].XQZTChufa == 0 && XianQuan[2].XQZTChufa == 0 && Math.Abs(XianQuan[3].SampleNoChufa - XianQuan[2].SampleNoChufa) <= 12)
                            {
                                if (TS23 == false && GetCBDataListHuaChi(new int[] { 11 }, XianQuan[3].SampleNoChufa, XianQuan[3].SampleNoShouwei, false).Count == 0)
                                {
                                    TS23 = true;
                                    XianQuan[3].XQZTChufa = XianQuan[2].XQZTChufa = 2;

                                    Console.WriteLine("线圈 3 2 同时触发，SampleNoChufa为：" + XianQuan[3].SampleNoChufa);
                                    logger.Info("线圈 3 2 同时触发，SampleNoChufa为：" + XianQuan[3].SampleNoChufa);
                                }
                            }
                            break;
                            #endregion
                    }
                }
            }

            #endregion

            #region 收尾计算
            for (int i = 1; i < 4; i++)
            {
                if (XianQuan[i].XQZTShouwei != 1)
                { //不为收尾 
                    continue;
                }
                switch (i)
                {
                    case 1:
                        #region 1线圈收尾


                        //线圈收尾，三个线圈公用一个牌识，所以此处直接执行抓拍即可，用于匹配的picNum付给车道号，后面修改匹配规则
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 1, true)));
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(3, 1, false)));

                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 1, true, false)));
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 1, false, false)));

                        if (XianQuan[1].XQZTChufa == 0)
                        {//1单独触发收尾
                            InitXianQuanState(new int[] { 1 });

                            Console.WriteLine("线圈<1>触发...<1>收尾完成，进入称板计算");
                            ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQHuaChi(1, 1, XianQuan[1].SampleNoChufa,
                                                        XianQuan[1].SampleNoShouwei)));
                        }
                        else if (XianQuan[1].XQZTChufa == 2 && TS12 == true)
                        {//如果是12线圈同时触发 
                            if (XianQuan[2].XQZTShouwei == 1)
                            {//如果2线圈收尾了，则进入计算，否则等2线圈收尾
                                Console.WriteLine("线圈<1><2>同时触发...<2><1>收尾完成，进入称板计算");
                                logger.Info("线圈<1><2>同时触发...<2><1>收尾完成，进入称板计算");

                                /*
                                 * 需要判断1,2两个线圈哪个是最早触发的,哪个是最后收尾的
                                 */
                                GetBeginAndEndSampleNo(new int[] { 1, 2 }, new int[] { 1, 2 }, ref dtBegin, ref dtEnd);

                                InitXianQuanState(new int[] { 1, 2 });
                                TS12 = false;

                                ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQHuaChi(2, 1, dtBegin, dtEnd)));
                            }
                        }
                        break;
                    #endregion
                    case 2:
                        #region 2线圈收尾
                        //线圈收尾，三个线圈公用一个牌识，所以此处直接执行抓拍即可，用于匹配的picNum付给车道号，后面修改匹配规则
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 2, true)));
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(3, 2, false)));

                        if (XianQuan[2].XQZTChufa == 0)
                        {//单线圈触发  

                            Console.WriteLine("线圈<2>触发....<2>收尾完成，进入称板计算");
                            logger.Info("线圈<2>触发....<2>收尾完成，进入称板计算");
                            InitXianQuanState(new int[] { 2 });
                            ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQHuaChi(2, 2, XianQuan[2].SampleNoChufa, XianQuan[2].SampleNoShouwei)));
                        }
                        else if (XianQuan[2].XQZTChufa == 2 && TS12 == true && TS23 == false)
                        {//1,2同时触发，2收尾，判断1是否收尾，1收尾则计算
                            #region 12同时触发
                            if (XianQuan[1].XQZTShouwei == 1)
                            {
                                Console.WriteLine("线圈<2><1>同时触发....<1><2>收尾完成，进入称板计算");
                                logger.Info("线圈<2><1>同时触发....<1><2>收尾完成，进入称板计算");
                                /*
                            * 需要判断1,2两个线圈哪个是最早触发的,哪个线圈是最晚收尾的
                            */
                                GetBeginAndEndSampleNo(new int[] { 1, 2 }, new int[] { 1, 2 }, ref dtBegin, ref dtEnd);

                                InitXianQuanState(new int[] { 1, 2 });
                                TS12 = false;
                                ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQHuaChi(1, 2, dtBegin, dtEnd)));
                            }
                            #endregion
                        }
                        else if (XianQuan[2].XQZTChufa == 2 && TS12 == false && TS23 == true)
                        {//2,3同时触发，2收尾，判断3是否收尾，3收尾则计算
                            #region 23同时触发
                            if (XianQuan[3].XQZTShouwei == 1)
                            {
                                Console.WriteLine("线圈<2><3>同时触发....<3><2>收尾完成，进入称板计算");
                                logger.Info("线圈<2><3>同时触发....<3><2>收尾完成，进入称板计算");
                                /*
                            * 需要判断2,3两个线圈哪个是最早触发的,哪个线圈是最晚收尾的
                            */
                                GetBeginAndEndSampleNo(new int[] { 2, 3 }, new int[] { 2, 3 }, ref dtBegin, ref dtEnd);
                                InitXianQuanState(new int[] { 2, 3 });
                                TS23 = false;
                                ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQHuaChi(3, 2, dtBegin, dtEnd)));
                            }
                            #endregion
                        }
                        break;
                    #endregion
                    case 3:
                        #region 3线圈收尾
                        //线圈收尾，三个线圈公用一个牌识，所以此处直接执行抓拍即可，用于匹配的picNum付给车道号，后面修改匹配规则
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 3, true)));
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(3, 3, false)));

                        if (XianQuan[3].XQZTChufa == 0)
                        {//单独收尾

                            Console.WriteLine("线圈<3>触发....<3>收尾完成，进入称板计算");
                            logger.Info("线圈<3>触发....<3>收尾完成，进入称板计算");
                            InitXianQuanState(new int[] { 3 });

                            ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQHuaChi(3, 3, XianQuan[3].SampleNoChufa, XianQuan[3].SampleNoShouwei)));
                        }
                        else if (XianQuan[3].XQZTChufa == 2 && TS23 == true && TS34 == false)
                        {
                            #region 3,2同时触发
                            if (XianQuan[2].XQZTShouwei == 1)
                            {//如果2线圈触发且收尾
                                Console.WriteLine("线圈<3><2>同时触发...<2><3>收尾完成，进入称板计算");
                                logger.Info("线圈<3><2>同时触发...<2><3>收尾完成，进入称板计算");

                                /*
                                  * 需要判断2,3两个线圈哪个是最早触发的,哪个是最晚收尾的
                                  */
                                GetBeginAndEndSampleNo(new int[] { 2, 3 }, new int[] { 2, 3 }, ref dtBegin, ref dtEnd);
                                InitXianQuanState(new int[] { 2, 3 });
                                TS23 = false;

                                ThreadPool.QueueUserWorkItem(new WaitCallback(x => GroupingXQHuaChi(2, 3, dtBegin, dtEnd)));
                            }
                            #endregion
                        }
                        break;
                    #endregion 
                    default:
                        break;
                }
            }
            #endregion

            StringBuilder sbLog = new StringBuilder();
            sbLog.Append("接收到线圈: " + Common.byteToHexStr(head) + " ");
            sbLog.Append("sampleNo：" + sbXQSampleNo.ToString() + " ");
            //sbLog.Append("接收时间：" + dt + " ");

            logger.Info(sbLog.ToString());

            sbXQZT.Clear();
            sbCF.Clear();
            sbSW.Clear();
            sbXQSampleNo.Clear();
        }
        #endregion

        #region 计算完整的一帧数据 
        /// <summary>
        /// 计算完整的一帧数据
        /// </summary>
        /// <param name="oneByte">组合后的完整数据帧</param>
        private void CalculateFrameHuaChi(byte[] oneByte)
        {
            CBData CbData = new CBData();
            CbData.RecvTime = DateTime.Now;//.ToString("yyyy-MM-dd HH:mm:ss fff");
            CbData.TDH = oneByte[3];

            #region 高位在前
            //CbData.FrontBaseLineNum = oneByte[5] + (oneByte[4] << 8);
            //CbData.BackBaseLineNum = oneByte[7] + (oneByte[6] << 8);
            //CbData.Peakvalue = oneByte[9] + (oneByte[8] << 8);
            //CbData.KD = oneByte[13] + (oneByte[12] << 8) + (oneByte[11] << 16) + (oneByte[10] << 24);
            //CbData.JLZ = oneByte[17] + (oneByte[16] << 8) + (oneByte[15] << 16) + (oneByte[14] << 24);
            //CbData.SampleMax = oneByte[21] + (oneByte[20] << 8) + (oneByte[19] << 16) + (oneByte[18] << 24);
            //CbData.IndexMax = oneByte[25] + (oneByte[24] << 8) + (oneByte[23] << 16) + (oneByte[22] << 24);
            #endregion


            #region 低位在前
            CbData.FrontBaseLineNum = oneByte[4] + (oneByte[5] << 8);
            CbData.BackBaseLineNum = oneByte[6] + (oneByte[7] << 8);
            CbData.Peakvalue = oneByte[8] + (oneByte[9] << 8);
            CbData.KD = oneByte[10] + (oneByte[11] << 8) + (oneByte[12] << 16) + (oneByte[13] << 24);
            CbData.JLZ = oneByte[14] + (oneByte[15] << 8) + (oneByte[16] << 16) + (oneByte[17] << 24);
            CbData.SampleMax = oneByte[18] + (oneByte[19] << 8) + (oneByte[20] << 16) + (oneByte[21] << 24);
            CbData.IndexMax = oneByte[22] + (oneByte[23] << 8) + (oneByte[24] << 16) + (oneByte[25] << 24);
            #endregion

            //if (CbData.TDH == 1 || CbData.TDH == 2 || CbData.TDH == 3 || CbData.TDH == 4)
            //{
            //    return;
            //}
            //CBList.Add(CbData);
            CBListAdd(CbData);

            //Console.WriteLine("称板数据：" + CbData.TDH + ".........时间：" + CbData.RecvTime.ToString("yyyy/MM/dd HH:mm:ss.fff"));

            StringBuilder sb = new StringBuilder();
            sb.Append("接收到数据: " + Common.byteToHexStr(oneByte) + " ");
            sb.Append("通道号：" + CbData.TDH.ToString() + " ");
            sb.Append("前基线值：" + CbData.FrontBaseLineNum.ToString() + " ");
            sb.Append("后基线值：" + CbData.BackBaseLineNum.ToString() + " ");
            sb.Append("峰值：" + CbData.Peakvalue.ToString() + " ");
            sb.Append("波宽：" + CbData.KD.ToString() + " ");
            sb.Append("计量值：" + CbData.JLZ.ToString() + " ");
            sb.Append("最高点sample值：" + CbData.SampleMax.ToString() + " ");
            sb.Append("最高点indx值：" + CbData.IndexMax.ToString() + " ");
            sb.Append("接收时间：" + CbData.RecvTime.ToString("yyyy/MM/dd HH:mm:ss.fff") + " ");

            logger.Info(sb.ToString());
            //InsertDgvRow("数据", CbData.TDH.ToString(), CbData.JLZ.ToString(), CbData.Time.ToString(), CbData.CKZ.ToString(), "", dt, Common.byteToHexStr(oneByte));


        }
        #endregion

        #region 分组奇偶称板，用于计算每轴速度和加速度以及后面计算分轴重
        /// <summary>
        ///  分组奇偶称板，用于计算每轴速度和加速度以及后面计算分轴重
        /// </summary>
        /// <param name="listCB">称板列表</param>
        /// <returns>分组后的称板列表</returns>
        private List<List<CBData>> GetCBGroupHuaChi(List<CBData> listCB)
        {
            List<List<CBData>> GroupList = new List<List<CBData>>();
            List<CBData> tmpList = new List<CBData>();

            bool haved = false;
            //List<CBData> list = listCB.OrderBy(x => x.SampleMax*1024+x.IndexMax).ToList();
            List<CBData> list = listCB.OrderBy(x => x.SampleMax).ThenBy(x => x.IndexMax).ToList();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                sb.Append("通道号：" + list[i].TDH.ToString() + " ");
                sb.Append("前基线值：" + list[i].FrontBaseLineNum.ToString() + " ");
                sb.Append("后基线值：" + list[i].BackBaseLineNum.ToString() + " ");
                sb.Append("峰值：" + list[i].Peakvalue.ToString() + " ");
                sb.Append("波宽：" + list[i].KD.ToString() + " ");
                sb.Append("计量值：" + list[i].JLZ.ToString() + " ");
                sb.Append("最高点sample值：" + list[i].SampleMax.ToString() + " ");
                sb.Append("最高点indx值：" + list[i].IndexMax.ToString() + " ");
                sb.Append("接收时间：" + list[i].RecvTime.ToString("yyyy/MM/dd HH:mm:ss.fff") + " ");
                sb.Append("\r\n");
            }
            //Console.WriteLine(sb.ToString());
            logger.Info(sb.ToString());
            sb.Clear();

            //增加判定“sample差值*1024+index差值”的判定，如果差值大于10000，
            //差不多sample值差10，则认为该称板不是这辆车的数据，不加入分组队列中
            //还需要判定该差值出现的次数大于4，,即需要与其他4块板比较，依据出现的次数判断该次数据到底是否该去掉
            //如果出现一次就去掉，有可能是这个数据是对的，但用于比较的那个数据是错误的
            int num = 0;//出现次数
            //for (int i = list.Count - 1; i >= 0; i--)
            //{
            //    for (int j = 0; j < list.Count; j++)
            //    {
            //        if (Math.Abs((list[i].SampleMax - list[j].SampleMax) * 1024 + (list[i].IndexMax - list[j].IndexMax)) > 10000)
            //        {
            //            num++;
            //            if (num >= 4)
            //            {
            //                break;
            //            }
            //        }
            //    }
            //    if (num >= 4)
            //    {
            //        sb.Append("分组时丢弃称板：" + list[i].TDH + ",最高点sample值：" + list[i].SampleMax.ToString() + "最高点indx值：" + list[i].IndexMax.ToString());
            //        list.RemoveAt(i); 
            //    }
            //    else
            //    {
            //        num = 0;
            //    }
            //}

            for (int i = 0; i < list.Count(); i++)
            {
                if (tmpList.Count == 0)
                {
                    tmpList.Add(list[i]);
                    continue;
                }
                for (int j = 0; j < tmpList.Count; j++)
                {
                    if (tmpList[j].TDH == list[i].TDH)
                    {
                        haved = true;
                        break;
                    }
                }

                if (haved == true)
                {
                    GroupList.Add(tmpList);
                    tmpList = new List<CBData>();
                    tmpList.Add(list[i]);
                    haved = false;
                }
                else
                {
                    tmpList.Add(list[i]);
                }

                if (i == list.Count() - 1 && tmpList.Count > 0)
                {
                    GroupList.Add(tmpList);
                }
            }

            Console.WriteLine("列表总数=" + GroupList.Count);

            //Console.WriteLine(GroupList[0][0].JLZ.ToString());

            return GroupList;
        }
        #endregion

        #region 获取用于计算重量的称板列表 
        /// <summary>
        /// 获取用于计算重量的称板列表
        /// </summary>
        /// <param name="CBBH">在线圈对应范围内的称板编号序列</param>
        /// <param name="XQChufa">触发时间</param>
        /// <param name="XQShouwei">收尾时间</param> 
        /// <returns>用于计算重量的称板列表</returns>
        private List<CBData> GetCBDataListHuaChi(int[] CBBH, int xq1SampleNo, int xq2SampleNo, bool Removable)
        {
            List<CBData> list = (from p in CBList
                                 where CBBH.Contains(p.TDH) &&
                                   (p.SampleMax >= xq1SampleNo - 3 && p.SampleMax <= xq2SampleNo)
                                 orderby p.RecvTime
                                 select p).ToList();
            if (Removable)
            {
                CBListRemove(list);
            }

            return list;
        }
        #endregion

        #region 根据触发和收尾线圈的时间分组称板 
        private void GroupingXQHuaChi(int xq1, int xq2, int xq1SampleNo, int xq2SampleNo)
        {
            try
            {
                ReMoveListItem();

                string xq = xq1.ToString() + xq2.ToString();
                //Thread.CurrentThread.Join(500);//延迟，后面测试用于判断6轴车中间收尾用

                Console.WriteLine("触发SampleNo=" + xq1SampleNo + "收尾触发SampleNo=" + xq2SampleNo);
                switch (xq)
                {
                    case "11":
                        #region 1触发,1收尾
                        GetZZFZ(GetCBDataListHuaChi(new int[] { 1, 2, 3, 4, 5 }, xq1SampleNo, xq2SampleNo, true), 1);
                        break;
                    #endregion
                    case "21":
                        #region 12触发,21或者1收尾
                        GetZZFZ(GetCBDataListHuaChi(new int[] { 2, 3, 4, 5, 6 }, xq1SampleNo, xq2SampleNo, true), 1);
                        break;
                    #endregion
                    case "22":
                        #region 2触发,2收尾
                        GetZZFZ(GetCBDataListHuaChi(new int[] { 4, 5, 6, 7, 8, 9 }, xq1SampleNo, xq2SampleNo, true), 2);
                        break;
                    #endregion
                    case "12":
                        #region 21触发,12收尾或2收尾
                        GetZZFZ(GetCBDataListHuaChi(new int[] { 2, 3, 4, 5, 6 }, xq1SampleNo, xq2SampleNo, true), 2);
                        break;
                    #endregion
                    case "32":
                        #region 23触发,32或2收尾
                        GetZZFZ(GetCBDataListHuaChi(new int[] { 6, 7, 8, 9, 10 }, xq1SampleNo, xq2SampleNo, true), 2);
                        break;
                    #endregion
                    case "33":
                        #region 3触发,3收尾
                        GetZZFZ(GetCBDataListHuaChi(new int[] { 7, 8, 9, 10, 11 }, xq1SampleNo, xq2SampleNo, true), 3);
                        //GetZZ(GetCBDataList(new int[] { 1, 2, 3, 4, 5 }, XQChufa, XQShouwei, true), 1);
                        break;
                    #endregion
                    case "23":
                        #region 23触发,23收尾或3收尾
                        GetZZFZ(GetCBDataListHuaChi(new int[] { 6, 7, 8, 9, 10 }, xq1SampleNo, xq2SampleNo, true), 3);
                        break;
                    #endregion 
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger.Debug(ex, ex.ToString());
            }
        }
        #endregion

        #region 获取线圈同时触发情况下的最早触发的线圈时间和最晚收尾的线圈时间，用于在该时间段内查找称板数据  
        /// <summary>
        /// 获取线圈同时触发情况下的最早触发的线圈SampleNo和最晚收尾的线圈SampleNo，用于在该时间段内查找称板数据
        /// </summary>
        /// <param name="ChufaXQBH">触发线圈编号</param>
        /// <param name="ShouweiXQBH">收尾线圈编号</param>
        /// <param name="dtBegin">最早触发时间</param>
        /// <param name="dtEnd">最晚收尾时间</param>
        /// <param name="BuffCD">缓存的车道</param>
        private void GetBeginAndEndSampleNo(int[] ChufaXQBH, int[] ShouweiXQBH, ref int BeginSampleNo, ref int EndSampleNo)
        {
            var t = from p in XianQuan.AsEnumerable()
                    where ChufaXQBH.Contains(p.XQBH) && (p.XQBH > 0 && p.XQBH < 5)
                    orderby p.SampleNoChufa
                    select p;

            if (t.Count() > 0)
            {
                BeginSampleNo = t.FirstOrDefault().SampleNoChufa;
                //dtEnd = t.LastOrDefault().ShouweiTime;
            }
            else
            {
                Console.WriteLine("没找到触发SampleNo？？？？？？？？？？？？？？？？？？？？？");
            }
            t = from p in XianQuan.AsEnumerable()
                where p.XQZTChufa != -1 && ShouweiXQBH.Contains(p.XQBH)
                orderby p.SampleNoShouwei descending
                select p;

            if (t.Count() > 0)
            {
                EndSampleNo = t.FirstOrDefault().SampleNoShouwei;
            }
            else
            {
                Console.WriteLine("没找到收尾SampleNo？？？？？？？？？？？？？？？？？？？？？");
            }
            Console.WriteLine("使用的触发SampleNo为：" + BeginSampleNo + ",收尾SampleNo：" + EndSampleNo);
        }
        #endregion


        #region 获取分轴的时间差 
        /// <summary>
        /// 获取分轴的时间差,华驰仪表使用
        /// </summary>
        /// <param name="ZS">轴数</param>
        /// <param name="oddGroupList">奇数组的分组数据</param>
        /// <param name="evenGroupList">偶数组的分组数据</param>
        private int[] GetFenZhouTimeChaHuaChi(int ZS, List<List<CBData>> oddGroupList, List<List<CBData>> evenGroupList)
        {
            int[] TimeChaFenZhou = new int[ZS];
            List<CBData> listOdd = null;
            List<CBData> listEven = null;
            bool ret = false;

            //时间差=(sampleNo1-sampleNo2)*250ms+(indx1-indx2)/1024*250ms

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < oddGroupList.Count; i++)
            {
                listOdd = oddGroupList[i];
                listEven = evenGroupList[i];
                //groupCBCount = listOdd.Count > listEven.Count ? listOdd.Count : listEven.Count; 

                //直接使用该组第一块接收到的称板数据，不固定左右侧
                if (listOdd[0].TDH == 1)
                {//1号板只能去找2号板的时刻,遍历偶数组该列的数据，找到2板
                    for (int cbEven = 0; cbEven < listEven.Count; cbEven++)
                    {
                        if (listEven[cbEven].TDH == 2)
                        {
                            TimeChaFenZhou[i] = (listEven[cbEven].SampleMax - listOdd[0].SampleMax) * 250 + (listEven[cbEven].IndexMax - listOdd[0].IndexMax) * 250 / 1024;
                            break;
                        }
                    }
                }
                else if (listOdd[0].TDH == 11)
                {//11号板只能去找10号板的时刻,遍历偶数组该列的数据，找到2板
                    for (int cbEven = 0; cbEven < listEven.Count; cbEven++)
                    {
                        if (listEven[cbEven].TDH == 10)
                        {
                            TimeChaFenZhou[i] = (listEven[cbEven].SampleMax - listOdd[0].SampleMax) * 250 + (listEven[cbEven].IndexMax - listOdd[0].IndexMax) * 250 / 1024;
                            break;
                        }
                    }
                }
                else
                {//其他板先找偶数比奇数板通道号小1的，没有再找大1的
                    for (int cbEven = 0; cbEven < listEven.Count; cbEven++)
                    {
                        if (listOdd[0].TDH - listEven[cbEven].TDH == 1)
                        {
                            TimeChaFenZhou[i] = (listEven[cbEven].SampleMax - listOdd[0].SampleMax) * 250 + (listEven[cbEven].IndexMax - listOdd[0].IndexMax) * 250 / 1024;
                            ret = true;
                            break;
                        }
                    }

                    if (ret == false)
                    {//没有找到比这个板小1的数，找大1的数
                        for (int cbEven = 0; cbEven < listEven.Count; cbEven++)
                        {
                            if (listOdd[0].TDH - listEven[cbEven].TDH == -1)
                            {
                                TimeChaFenZhou[i] = (listEven[cbEven].SampleMax - listOdd[0].SampleMax) * 250 + (listEven[cbEven].IndexMax - listOdd[0].IndexMax) * 250 / 1024;
                                ret = true;
                                break;
                            }
                        }
                    }
                }
                ret = false;
                sb.Append("第" + (i + 1) + "轴时刻差=" + TimeChaFenZhou[i]);
            }

            logger.Info(sb.ToString());

            //var jj = from p in listEven where (from pp in listOdd select pp.TDH).Contains(p.TDH - 1) select p;  
            //foreach (var item in jj)
            //{
            //    MessageBox.Show(item.ToString());
            //}




            return TimeChaFenZhou;
        }
        #endregion


        #region 根据每个轴的时刻差计算重量,华驰仪表使用  
        /// <summary>
        /// 根据每个轴的时刻差计算重量,华驰仪表使用
        /// </summary>
        /// <param name="ListData">称板数据列表</param>
        /// <param name="CD">车道号</param>
        private void GetZZFZHuaChi(List<CBData> ListData, int CD)
        {
            Thread.CurrentThread.Join(500);//延迟500毫秒，等待称板数据到来

            #region 变量 

            StringBuilder sb = new StringBuilder();

            //奇数列使用的哪两块板计算
            StringBuilder sbOddCBTDH = new StringBuilder();
            //偶数列使用的哪两块板计算
            StringBuilder sbEvenCBTDH = new StringBuilder();

            foreach (var item in ListData)
            {
                sb.Append(item.TDH + " ");
            }
            Console.WriteLine("用于计算的称板：" + sb.ToString());
            logger.Info("用于计算的称板：" + sb.ToString());
            if (sb.Length < 8)
            {
                CBNotEnoughCount++;//数量++

                Console.WriteLine("用于计算的称板不够计算使用，丢弃=" + CBNotEnoughCount);
                logger.Info("用于计算的称板不够计算使用，丢弃=" + CBNotEnoughCount);
                return;
            }

            ZH_ST_ZDCZB zh_st_zdczb = new ZH_ST_ZDCZB();
            int usedCBData = 3;//默认对比  

            //车速为km/h即千米/小时 
            double ms = 0, s, m, h;
            //奇数编号的称板队列
            List<CBData> oddList = new List<CBData>();
            //偶数编号的称板队列
            List<CBData> evenList = new List<CBData>();
            /*
             * 比较奇数列和偶数列压板数，如果一侧压板数为奇数，一侧为偶数，
             * 侧取压板数为偶数侧的重量作为总重，若两侧压板数都是偶数，侧
             * 比较后取较重的
             * 轴数使用偶数侧的总板数除以2
             */
            //奇数列压板数
            int oddCBNo = 0;
            //偶数列压板数
            int evenCBNo = 0;
            int oddCBCount = 0;
            int evenCBCount = 0;
            int CheckPSIndex = 0;//首先匹配配置中的第几个相机

            #endregion

            LogMainTable logMainTable = new LogMainTable();
            //LogDetailTable logDetailTable = new LogDetailTable();

            logMainTable.LogID = DateTime.Now.ToString("yyyyMMddHHmmsss");
            logMainTable.LogDetailTables = new List<LogDetailTable>();

            zh_st_zdczb.CPH = "无车牌";
            zh_st_zdczb.CPYS = "无";
            zh_st_zdczb.CPTX = "";
            zh_st_zdczb.PLATE = "";
            zh_st_zdczb.CWTX = " ";
            zh_st_zdczb.CMTX = " ";
            zh_st_zdczb.Video = " ";

            //手动截图,断网调试注释掉
            //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManualSnapZX(2, true, ref zh_st_zdczb)));
            //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManualSnapZX(3, false, ref zh_st_zdczb)));


            try
            {
                List<CBData> tmpList = ListData;

                /*
                 * 查询当前计算队列中称板的个数，每个通道的称板只取一个，故查询结果的和就是压板数
                 */
                var o = from r in ListData.AsQueryable()
                        group r by r.TDH into g
                        select g.OrderBy(r => r.TDH).FirstOrDefault();

                //压板数，根据压板数是三，四，五进行不同的计算
                int OnCbCount = o.ToList().Count;
                int RecvCbCount = ListData.Count();

                #region 根据第一块板的方向判断使用匹配哪个方向的车牌相机 

                if (ListData[0].TDH % 2 == 0)
                {//接收到的第一条数据如果为偶数号板，则为称重方向逆行，判断称重方向车尾相机 
                    zh_st_zdczb.FX = SysSet.CpsbList[1].FangXiang;
                    CheckPSIndex = 1;
                }
                else
                {//收到的第一条数据如果为奇数号板,则为称重方向正常行驶 
                    zh_st_zdczb.FX = SysSet.CpsbList[0].FangXiang;
                    CheckPSIndex = 0;
                }
                #endregion
                /*
                * 在计算时刻差时必须使用同一个轮压两侧称板的差值，所以按照现场称板的奇偶分两侧编号的方式
                * 必须是称板编号的差值的绝对值为1的情况下，才认为是同侧轮
                */


                var odd = from q in ListData
                          where q.TDH % 2 != 0
                          select q;
                var even = from q in ListData
                           where q.TDH % 2 == 0
                           select q;

                oddList = odd.ToList();
                evenList = even.ToList();
                oddCBCount = oddList.Count;
                evenCBCount = evenList.Count;

                //经过对比分析，轴数这里取奇数或者偶数称板组中数据较多的一组
                //对该组的称板总数和压板的数进行求商取整作为车辆轴数
                #region 获取奇偶称板总数和每侧的压板数

                #region 计算奇数和偶数列的压板数，然后确定轴数和使用那侧的重量
                o = from r in oddList.AsQueryable()
                    group r by r.TDH into g
                    select g.OrderBy(r => r.TDH).FirstOrDefault();
                oddCBNo = o.ToList().Count();

                foreach (var item in o)
                {
                    sbOddCBTDH.Append(item.TDH);
                }

                logger.Info("奇数列称板总数=" + oddCBCount + ",奇数列压板数=" + oddCBNo);
                Console.WriteLine("奇数列称板总数=" + oddCBCount + ",奇数列压板数=" + oddCBNo);

                o = from r in evenList.AsQueryable()
                    group r by r.TDH into g
                    select g.OrderBy(r => r.TDH).FirstOrDefault();
                evenCBNo = o.ToList().Count();

                foreach (var item in o)
                {
                    sbEvenCBTDH.Append(item.TDH);
                }

                logger.Info("偶数列称板总数=" + evenCBCount + ",偶数列压板数=" + evenCBNo);
                Console.WriteLine("偶数列称板总数=" + evenCBCount + ",偶数列压板数=" + evenCBNo);

                if (oddCBNo % 2 == 0 && evenCBNo % 2 == 0)
                {//两侧压板数都为偶数
                    //zh_st_zdczb.ZS = oddCBCount >= evenCBCount ? oddCBCount / 2 : evenCBCount / 2; ;//可以取任何侧作为算轴依据

                    //if (oddCBCount >= evenCBCount)
                    //{
                    //    Console.WriteLine("两侧压板数都是偶数,但奇数侧总数>=偶数，轴数=奇数(+1)/2");
                    //    zh_st_zdczb.ZS = oddCBCount % 2 == 0 ? oddCBCount / 2 : (oddCBCount + 1) / 2;
                    //}
                    //else
                    //{
                    //    zh_st_zdczb.ZS = evenCBCount % 2 == 0 ? evenCBCount / 2 : (evenCBCount + 1) / 2;
                    //    Console.WriteLine("两侧压板数都是偶数,但偶数侧总数>=奇数，轴数=偶数(+1)/2");
                    //}
                    usedCBData = 3;//奇偶对比，用大的
                }
                else if (oddCBNo % 2 == 0 && evenCBNo % 2 != 0)
                {//奇数列压板数为偶数，偶数侧压板数不是偶数
                    //这里还需要判断总数是否是2的倍数，不是的话需要+1变成2的倍数 
                    //zh_st_zdczb.ZS = oddCBCount % 2 == 0 ? oddCBCount / 2 : (oddCBCount + 1) / 2;
                    //Console.WriteLine("奇数列压板数为偶数,轴数=奇数（+1）/2");
                    usedCBData = 1;//用奇数组计算
                }
                else if (oddCBNo % 2 != 0 && evenCBNo % 2 == 0)
                {//偶数侧压板数为偶数，奇数侧不是偶数
                    //zh_st_zdczb.ZS = evenCBCount % 2 == 0 ? evenCBCount / 2 : (evenCBCount + 1) / 2;
                    ////zh_st_zdczb.ZS = evenCBCount / 2;
                    //Console.WriteLine("偶数列压板数为偶数,轴数=偶数（+1）/2");
                    usedCBData = 2;//用偶数组计算
                }
                else if (oddCBNo % 2 != 0 && evenCBNo % 2 != 0)
                {//如果两侧压板数都不是偶数，这种情况属于丢板了，暂时使用压板数大的一侧+1作为算轴依据
                    //zh_st_zdczb.ZS = oddCBCount > evenCBCount ? (oddCBCount + 1) / 2 : (evenCBCount + 1) / 2;
                    //if (oddCBCount >= evenCBCount)
                    //{
                    //    Console.WriteLine("两侧压板数都不是偶数,但奇数侧总数>=偶数，轴数=奇数(+1)/2");
                    //    zh_st_zdczb.ZS = oddCBCount % 2 == 0 ? oddCBCount / 2 : (oddCBCount + 1) / 2;
                    //}
                    //else
                    //{
                    //    zh_st_zdczb.ZS = evenCBCount % 2 == 0 ? evenCBCount / 2 : (evenCBCount + 1) / 2;
                    //    Console.WriteLine("两侧压板数都不是偶数,但偶数侧总数>=奇数，轴数=偶数(+1)/2");
                    //}
                    usedCBData = 3;
                }
                #endregion

                #region 使用每侧称板数最多的那块板的总数作为轴数的方式

                var query = (from num in
                                 (
                                 from number in ListData
                                 group number.TDH by number.TDH into g
                                 select new
                                 {
                                     number = g.Key,
                                     cnt = g.Count()
                                 }
                             )
                             orderby num.cnt descending
                             select new { num.number, num.cnt }).ToList();
                foreach (var item in query)
                {
                    Console.WriteLine("称板 " + item.number.ToString() + " 次数=" + item.cnt);

                }
                if (query.Count > 0)
                {
                    zh_st_zdczb.ZS = query[0].cnt;
                }

                if (zh_st_zdczb.ZS == 1)
                {
                    zh_st_zdczb.ZS = 2;
                }
                else if (zh_st_zdczb.ZS > 6)
                {
                    zh_st_zdczb.ZS = 6;
                }

                #endregion

                #endregion


                #region 计算其他轴速度

                /*经过分析的计算方法如下：
                 * 过车序列6轴 5 7 6 8 4 5 7 5 7 4 6 8 4 6 8 7 5 5 7 6 8 4 7 5 4 6 8 4 6 8
                 * 奇数组：5 7 5 7 5 7 7 5 5 7 7 5 
                 * 偶数组：6 8 4 4 6 8 4 6 8 6 8 4 4 6 8 4 6 8
                 * 方法为把整个过车序列按顺序进行奇偶分组，相邻的奇偶分为一组
                 * 如果该奇偶组在分组过程中，遇到的数该组中已经存在，则分配到
                 * 对应的奇偶组的下一组中，然后按组进行时间差的计算
                 * 也可以先按奇偶分组，之后再分别对奇偶组的数重新遍历分组，遇到
                 * 相同的数据则分到下一组
                 * 
                 *  例如上面这个序列应划分如下
                 *      奇数列           偶数列
                 *   1.  5,7             6,8,4
                 *   2.  5,7             4,6,8
                 *   3.  5,7             4,6,8
                 *   4.  7,5             6,8,4
                 *   5.  5,7             4,6,8
                 *   6.  7,5             4,6,8
                 */
                /*经过测试还出现如下情况
                 * 5 7 6 8 5 7 6 8 5 7 5 7 4 6 8 4 6 8 
                 * 这个如果按照上面的分法，将会把第一个4板分到偶数的第二组中，造成错误
                 * 1.   5,7             6,8
                 * 2.   5,7             6,8,4
                 * 3.   5,7             6,8,4
                 * 4.   5,7             6,8
                 * 实际分组如下
                 * 1.   5,7             6,8
                 * 2.   5,7             6,8
                 * 3.   5,7             4,6,8
                 * 4.   5,7             4,6,8
                 * 统计后接收时间在100毫秒之内的为同一轴分组数据
                 */

                //华驰分组依据Index值，差值在120个index内的认为是同一组数据，否则是下组数据

                #region 分组称板
                //奇数组
                List<List<CBData>> oddGroupList = GetCBGroupHuaChi(oddList);
                List<List<CBData>> evenGroupList = GetCBGroupHuaChi(evenList);

                zh_st_zdczb.ZS = oddGroupList.Count();

                sb.Clear();
                sb.Append("奇数组如下：");
                for (int i = 0; i < oddGroupList.Count; i++)
                {
                    sb.Append("第" + i + "组=");
                    for (int j = 0; j < oddGroupList[i].Count; j++)
                    {
                        sb.Append(oddGroupList[i][j].TDH);
                    }
                    sb.Append("\r\n");
                }
                logger.Info(sb.ToString());
                sb.Clear();

                sb.Append("偶数组如下：");
                for (int i = 0; i < evenGroupList.Count; i++)
                {
                    sb.Append("第" + i + "组=");
                    for (int j = 0; j < evenGroupList[i].Count; j++)
                    {
                        sb.Append(evenGroupList[i][j].TDH);
                    }
                    sb.Append("\r\n");
                }
                logger.Info(sb.ToString());
                sb.Clear();

                #endregion

                int[] TimeChaFenZhou = new int[zh_st_zdczb.ZS];   //分轴时刻差
                Decimal[] SpeedFenZhou = new decimal[zh_st_zdczb.ZS];//分轴速度
                Decimal[] msFenZhou = new decimal[zh_st_zdczb.ZS];

                Decimal[] SpeedFenZhouMiSencond = new decimal[zh_st_zdczb.ZS];//分钟速度单位米/秒

                //加速度=轴的速度差/轴的时间 差
                decimal[] JiaSpeed = new decimal[zh_st_zdczb.ZS - 1];

                TimeChaFenZhou = GetFenZhouTimeChaHuaChi(zh_st_zdczb.ZS, oddGroupList, evenGroupList);

                Console.WriteLine("时刻差Couunt=" + TimeChaFenZhou.Length);
                sb.Clear();

                for (int i = 0; i < TimeChaFenZhou.Length; i++)
                {
                    switch (i + 1)
                    {
                        case 1:
                            logMainTable.AxisSKC1 = TimeChaFenZhou[0];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC1);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC1);
                            break;
                        case 2:
                            logMainTable.AxisSKC2 = TimeChaFenZhou[1];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC2);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC2);
                            break;
                        case 3:
                            logMainTable.AxisSKC3 = TimeChaFenZhou[2];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC3);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC3);
                            break;
                        case 4:
                            logMainTable.AxisSKC4 = TimeChaFenZhou[3];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC4);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC4);
                            break;
                        case 5:
                            logMainTable.AxisSKC5 = TimeChaFenZhou[4];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC5);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC5);
                            break;
                        case 6:
                            logMainTable.AxisSKC6 = TimeChaFenZhou[5];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC6);
                            //Console.WriteLine("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC6);
                            break;
                        case 7:
                            logMainTable.AxisSKC7 = TimeChaFenZhou[6];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC7);
                            break;
                        case 8:
                            logMainTable.AxisSKC8 = TimeChaFenZhou[7];
                            //sb.Append("第" + (i + 1) + "轴时刻差=" + logMainTable.AxisSKC8);
                            break;
                    }
                }

                //logger.Info(sb.ToString());

                decimal sOther, mOther, hOhter;
                sb.Clear();
                //速度=板距1.7米/时间差


                try
                {
                    for (int i = 0; i < zh_st_zdczb.ZS; i++)
                    {
                        sOther = (Decimal)(TimeChaFenZhou[i] / 1000.0); //这里时间计算出来的单位是毫秒，转为秒除以1000

                        //SpeedFenZhouMiSencond[i] = (Decimal)((Decimal)1.7 / sOther);
                        //sb.Append("第【" + (i + 1) + "】轴速度=" + SpeedFenZhouMiSencond[i] + "米/秒\r\n");

                        mOther = sOther / 60;
                        hOhter = mOther / 60;
                        SpeedFenZhou[i] = (Decimal)0.0017 / hOhter;

                        sb.Append("第【" + (i + 1) + "】轴速度=" + SpeedFenZhou[i] + "千米/时\r\n");
                    }
                    Console.WriteLine(sb.ToString());
                    logger.Info(sb.ToString());

                }
                catch (Exception ex)
                {
                    logger.Debug(ex, ex.ToString());
                    Console.WriteLine(ex.ToString());
                }



                //计算加速度
                sb.Clear();

                //decimal[] jiaSpeedTimeCha = new decimal[zh_st_zdczb.ZS - 1];
                //decimal[] speedCha = new decimal[zh_st_zdczb.ZS - 1];

                //Console.WriteLine(" 板数=" + oddGroupList.Count);

                //for (int i = 0; i < oddGroupList.Count - 1; i += 1)
                //{
                //    int skc = oddGroupList[i + 1][0].Time - oddGroupList[i][0].Time;
                //    sb.Append("加速度时刻差值=" + skc);
                //    //Console.WriteLine(sb.ToString());

                //    msFenZhou[0] = (Decimal)(skc * 40 / 1000.0);
                //    sOther = msFenZhou[i] / (Decimal)1000.0;
                //    sb.Append("加速度时刻差算为秒=" + sOther);
                //    //Console.WriteLine(sb.ToString());

                //    decimal sdc = SpeedFenZhouMiSencond[i + 1] - SpeedFenZhouMiSencond[i];
                //    sb.Append("速度差=" + sdc);
                //    JiaSpeed[i] = sdc / sOther;
                //    //Console.WriteLine(sb.ToString());

                //    sb.Append("第【" + (i + 1) + "】轴加速度=" + JiaSpeed[i] + " 米/秒\r\n");
                //    //Console.WriteLine(sb.ToString());
                //}

                //Console.WriteLine(sb.ToString());
                //logger.Info(sb.ToString());

                #endregion 

                #region 根据压板数确定最终总量使用奇数侧、偶数侧还是奇偶对比取大的一侧

                long zzOdd = 0;
                long zzEven = 0;
                sb.Clear();
                #region 计算奇数列板重 
                switch (sbOddCBTDH.ToString())
                {
                    case "13":
                    case "31":
                        K = SysSet.K13;
                        break;
                    case "35":
                    case "53":
                        K = SysSet.K35;
                        break;
                    case "57":
                    case "75":
                        K = SysSet.K57;
                        break;
                    case "79":
                    case "97":
                        K = SysSet.K79;
                        break;
                    case "911":
                    case "119":
                        K = SysSet.K911;
                        break;
                }

                //第几组奇偶计算重量就使用第几轴的时刻差
                for (int i = 0; i < oddGroupList.Count; i++)
                {
                    for (int j = 0; j < oddGroupList[i].Count; j++)
                    {

                        //Console.WriteLine("JLZ=" + oddGroupList[i][j].JLZ + ",SKC=" + TimeChaFenZhou[i]);
                        oddGroupList[i][j].ZZ = Convert.ToInt32(K * oddGroupList[i][j].JLZ / (double)TimeChaFenZhou[i]);
                        zzOdd += oddGroupList[i][j].ZZ;
                        //Console.WriteLine("奇数" + i +" "+ zzOdd);

                        LogDetailTable logDetailTable = new LogDetailTable();
                        logDetailTable.CBNo = oddGroupList[i][j].TDH;
                        logDetailTable.CBZZ = oddGroupList[i][j].ZZ;
                        logDetailTable.JLZ = oddGroupList[i][j].JLZ;
                        logDetailTable.SK = oddGroupList[i][j].SampleMax;
                        logDetailTable.UsedSKC = oddGroupList[i][j].IndexMax;
                        logDetailTable.LogID = logMainTable.LogID;



                        logMainTable.LogDetailTables.Add(logDetailTable);

                        sb.Append(" 奇数组：ZZ" + oddGroupList[i][j].TDH + "= " + oddGroupList[i][j].ZZ + " ");
                        //Console.WriteLine(sb.ToString());
                    }
                }

                sb.Append("奇数组总重= " + zzOdd + "\r\n");
                logger.Info(sb.ToString());
                Console.WriteLine(sb.ToString());
                #endregion
                sb.Clear();

                #region 计算偶数列板重 
                switch (sbEvenCBTDH.ToString())
                {
                    case "24":
                    case "42":
                        K = SysSet.K24;
                        break;
                    case "46":
                    case "64":
                        K = SysSet.K46;
                        break;
                    case "68":
                    case "86":
                        K = SysSet.K68;
                        break;
                    case "810":
                    case "108":
                        K = SysSet.K810;
                        break;
                }

                //第几组奇偶计算重量就使用第几轴的时刻差
                for (int i = 0; i < evenGroupList.Count; i++)
                {
                    for (int j = 0; j < evenGroupList[i].Count; j++)
                    {
                        evenGroupList[i][j].ZZ = Convert.ToInt32(K * evenGroupList[i][j].JLZ / (double)TimeChaFenZhou[i]);
                        zzEven += evenGroupList[i][j].ZZ;

                        LogDetailTable logDetailTable = new LogDetailTable();
                        logDetailTable.CBNo = evenGroupList[i][j].TDH;
                        logDetailTable.CBZZ = evenGroupList[i][j].ZZ;
                        logDetailTable.JLZ = evenGroupList[i][j].JLZ;
                        logDetailTable.SK = evenGroupList[i][j].SampleMax;
                        logDetailTable.UsedSKC = evenGroupList[i][j].IndexMax;
                        logDetailTable.LogID = logMainTable.LogID;

                        logMainTable.LogDetailTables.Add(logDetailTable);

                        sb.Append(" 偶数组：ZZ" + evenGroupList[i][j].TDH + "= " + evenGroupList[i][j].ZZ + " ");
                    }
                }
                sb.Append("偶数组总重= " + zzEven + "\r\n");
                logger.Info(sb.ToString());
                Console.WriteLine(sb.ToString());
                sb.Clear();

                #endregion

                #region 对比取最后的重量 
                switch (usedCBData)
                {
                    case 1:
                        #region 使用奇数侧作为重量 
                        switch (sbOddCBTDH.ToString())
                        {
                            case "13":
                            case "31":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara13;
                                break;
                            case "35":
                            case "53":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara35;
                                break;
                            case "57":
                            case "75":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara57;
                                break;
                            case "79":
                            case "97":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara79;
                                break;
                            case "911":
                            case "119":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara911;
                                break;
                        }

                        zh_st_zdczb.ZZ = zzOdd;
                        Console.WriteLine("使用奇数侧重量。" + sbOddCBTDH.ToString());
                        break;
                    #endregion
                    case 2:
                        #region 使用偶数列作为重量 
                        switch (sbEvenCBTDH.ToString())
                        {
                            case "24":
                            case "42":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara24;
                                break;
                            case "46":
                            case "64":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara46;
                                break;
                            case "68":
                            case "86":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara68;
                                break;
                            case "810":
                            case "108":
                                SysSet.SpeedXZPara = SysSet.SpeedXZPara810;
                                break;
                        }

                        zh_st_zdczb.ZZ = zzEven;

                        Console.WriteLine("使用偶数侧重量。" + sbEvenCBTDH.ToString());
                        break;
                    #endregion
                    case 3:
                        #region 奇偶对比取较重侧 
                        zh_st_zdczb.ZZ = zzOdd > zzEven ? zzOdd : zzEven;

                        if (zzOdd > zzEven)
                        {
                            switch (sbOddCBTDH.ToString())
                            {
                                case "13":
                                case "31":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara13;
                                    break;
                                case "35":
                                case "53":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara35;
                                    break;
                                case "57":
                                case "75":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara57;
                                    break;
                                case "79":
                                case "97":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara79;
                                    break;
                                case "911":
                                case "119":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara911;
                                    break;
                            }
                            Console.WriteLine("使用奇数侧重量。" + sbOddCBTDH.ToString());
                        }
                        else
                        {
                            switch (sbEvenCBTDH.ToString())
                            {
                                case "24":
                                case "42":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara24;
                                    break;
                                case "46":
                                case "64":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara46;
                                    break;
                                case "68":
                                case "86":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara68;
                                    break;
                                case "810":
                                case "108":

                                    SysSet.SpeedXZPara = SysSet.SpeedXZPara810;
                                    break;
                            }
                            Console.WriteLine("使用偶数侧重量。" + sbEvenCBTDH.ToString());
                        }
                        break;
                    #endregion
                    default:
                        break;
                }
                sb.Clear();
                #endregion

                #endregion

                #region 取所有分轴的速度平均值的整数作为最后的车速 
                for (int i = 0; i < zh_st_zdczb.ZS; i++)
                {
                    zh_st_zdczb.CS += SpeedFenZhou[i];
                }

                zh_st_zdczb.CS = zh_st_zdczb.CS / zh_st_zdczb.ZS;

                zh_st_zdczb.CS = (int)zh_st_zdczb.CS;
                zh_st_zdczb.CD = CD;

                //if (zh_st_zdczb.CS > 100)
                //{
                //    CSErrorCount++;
                //    logger.Info("车速=" + zh_st_zdczb.CS + " 车速大于100，认为异常，抛弃 =" + CSErrorCount);
                //    Console.WriteLine("车速=" + zh_st_zdczb.CS + " 车速大于100，认为异常，抛弃 =" + CSErrorCount);
                //    logger.Warn("车速=" + zh_st_zdczb.CS + " 车速大于100，认为异常，抛弃= " + CSErrorCount);
                //    return;
                //}
                #endregion 

                #region 根据速度修正重量
                try
                {
                    int ZZXZPos = (int)(zh_st_zdczb.CS % SysSet.SpeedXZInterver);
                    if (ZZXZPos > 0)
                    {//取模大于0，说明有余数，修正位置段应该为除数的商+1
                        ZZXZPos = (int)(Convert.ToUInt32(zh_st_zdczb.CS) / SysSet.SpeedXZInterver) + 1;
                    }
                    else
                    {
                        ZZXZPos = (int)(Convert.ToUInt32(zh_st_zdczb.CS) / SysSet.SpeedXZInterver);
                    }

                    if (SysSet.SpeedXZPara.Length >= ZZXZPos)
                    {
                        Console.WriteLine("速度=" + zh_st_zdczb.CS + ",修正参数=" + SysSet.SpeedXZPara[ZZXZPos - 1] + ",检测重量=" + zh_st_zdczb.ZZ);
                        logger.Info("速度=" + zh_st_zdczb.CS + ",修正参数=" + SysSet.SpeedXZPara[ZZXZPos - 1] + ",检测重量=" + zh_st_zdczb.ZZ);

                        logMainTable.ZZ = (int)zh_st_zdczb.ZZ;
                        logMainTable.XZPara = (decimal)SysSet.SpeedXZPara[ZZXZPos - 1];

                        zh_st_zdczb.ZZ = (long)(zh_st_zdczb.ZZ * SysSet.SpeedXZPara[ZZXZPos - 1]);

                        logMainTable.XZZZ = (int)zh_st_zdczb.ZZ;

                        Console.WriteLine("修正后总重=" + zh_st_zdczb.ZZ);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("速度修正异常" + ex.ToString());
                    logger.Debug("速度修正异常" + ex.ToString(), ex);
                }
                #endregion 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger.Debug(ex.ToString() + "\r\n" + ex.StackTrace);
            }


            //倒序排列牌识结果，有利用匹配到最新的那条识别结果
            //Thread.CurrentThread.Join(600);//延时匹配，手动抓拍牌识返回慢

            #region 匹配车牌
            try
            {


                bool isCheck = false;
                //cpsbResHead = cpsbResHead.ToList().OrderByDescending(x => x.CPSB_Time).ToArray();
                //DateTime dt = DateTime.Now;

                ////int index = 0;
                //foreach (var item in cpsbResHead)
                //{
                //    Console.WriteLine("车牌号:" + item.CPH + ",时间=" + item.CPSB_Time + ",IP=" + item.DeviceIP + ",车道=" + item.CD_GDW);
                //    if ((dt - item.CPSB_Time).TotalSeconds >= 5)
                //    {
                //        //InitCpsbRes(index);
                //        //item.CPSB_Flag = true;
                //        InitCpsbRes(item.CPSB_Time);
                //    }
                //    //index++;
                //}

                //Console.WriteLine("称板给出的方向=" + zh_st_zdczb.FX + ",CD=" + CD + ",牌识ip=" + SysSet.CpsbList[CheckPSIndex - 1].IP);
                //logger.Info("称板给出的方向=" + zh_st_zdczb.FX + ",CD=" + CD + ",牌识ip=" + SysSet.CpsbList[CheckPSIndex - 1].IP);
                //CheckCpsb(CD, ref zh_st_zdczb, CheckPSIndex);  

                #region 匹配称重方向正向行驶的图片 
                if (CheckPSIndex == 0)
                {
                    for (int i = 0; i < SysSet.CpsbList.Count * 2; i++)
                    {
                        if (!cpsbResHead[i].CPSB_Flag && cpsbResHead[i].DeviceIP == SysSet.CpsbList[0].IP && cpsbResHead[i].CD_GDW == CD)
                        //&& Math.Abs((cpsbResHead[i].CPSB_Time - ListData[0].RecvTime).TotalMilliseconds) <= 500))//需增加车道的判断
                        {

                            zh_st_zdczb.CPH = cpsbResHead[i].CPH;
                            zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
                            zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
                            zh_st_zdczb.PLATE = cpsbResHead[i].Plate;

                            //InitCpsbRes(cpsbResHead[i].CPSB_Time);
                            InitCpsbRes(i, 0);
                            isCheck = true;
                            //Console.WriteLine("匹配图片=" + zh_st_zdczb.CPTX);
                            logger.Info("匹配图片=" + zh_st_zdczb.CPTX);
                            break;
                        }
                    }
                }
                #endregion

                #region 匹配称重方向逆行的图片
                //else if (CheckPSIndex == 1)
                //{
                //if (!cpsbResHead[i].CPSB_Flag && cpsbResHead[i].DeviceIP == SysSet.CpsbList[1].IP && cpsbResHead[i].CD_GDW == CD)
                ////&& Math.Abs((cpsbResHead[i].CPSB_Time - ListData[0].RecvTime).TotalMilliseconds) <= 500))//需增加车道的判断
                //{

                //    zh_st_zdczb.CPH = cpsbResHead[i].CPH;
                //    zh_st_zdczb.CPYS = cpsbResHead[i].CPYS;
                //    zh_st_zdczb.CPTX = cpsbResHead[i].photoPath;
                //    zh_st_zdczb.PLATE = cpsbResHead[i].Plate;

                //    InitCpsbRes(i, 0);
                //    isCheck = true;
                //    Console.WriteLine("匹配图片=" + zh_st_zdczb.CPTX);
                //    logger.Info("匹配图片=" + zh_st_zdczb.CPTX);

                #region 匹配车尾和侧面
                //for (int k = 0; k < cpsbResTail.Count(); k++)
                //{
                //    if (cpsbResTail[k].picNum == CD)
                //    {
                //        zh_st_zdczb.CWTX = cpsbResTail[k].photoPath;
                //        InitCpsbRes(i, 1);
                //        break;
                //    }
                //}
                //for (int j = 0; j < cpsbResSide.Count(); j++)
                //{
                //    if (cpsbResSide[j].picNum == CD)
                //    {
                //        zh_st_zdczb.CMTX = cpsbResSide[j].photoPath;
                //        InitCpsbRes(i, 2);
                //        break;
                //    }
                //}
                #endregion

                //break;
                //}
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("匹配车牌异常" + ex.ToString());
                logger.Debug("匹配车牌异常", ex);
            }

            #endregion


            #region 计算车辆超限数据

            //if (zh_st_zdczb.ZS >= 6)
            //{
            //    zh_st_zdczb.XZ = 55 * 1000;
            //}
            //else
            //{
            //    zh_st_zdczb.XZ = zh_st_zdczb.ZS * 10000;
            //}

            switch (zh_st_zdczb.ZS)
            {
                case 2:
                    zh_st_zdczb.XZ = SysSet.Axle2WeightMax;
                    break;
                case 3:
                    zh_st_zdczb.XZ = SysSet.Axle3WeightMax;
                    break;
                case 4:
                    zh_st_zdczb.XZ = SysSet.Axle4WeightMax;
                    break;
                case 5:
                    zh_st_zdczb.XZ = SysSet.Axle5WeightMax;
                    break;
                case 6:
                    zh_st_zdczb.XZ = SysSet.Axle6WeightMax;
                    break;
                default:
                    zh_st_zdczb.XZ = SysSet.Axle6WeightMax;
                    break;
            }

            if (zh_st_zdczb.ZZ > zh_st_zdczb.XZ)
            {
                zh_st_zdczb.CXL = (int)((zh_st_zdczb.ZZ - zh_st_zdczb.XZ) * 100 / zh_st_zdczb.XZ);
                zh_st_zdczb.XHZL = zh_st_zdczb.ZZ - zh_st_zdczb.XZ;
            }
            else
            {
                zh_st_zdczb.CXL = 0;
                zh_st_zdczb.XHZL = 0;
            }

            if (zh_st_zdczb.XHZL > SysSet.MinCXBZ)
            {
                zh_st_zdczb.SFCX = 1;

                //ThreadPool.QueueUserWorkItem(new WaitCallback(x => LedContentFrame(zh_st_zdczb)));

            }
            else
            {
                zh_st_zdczb.SFCX = 0;
            }

            if (zh_st_zdczb.CXL > 150)
            {
                SuperRangeCount++;
                logger.Info("超限率=" + zh_st_zdczb.CXL + ", 大于150，抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);
                Console.WriteLine("超限率=" + zh_st_zdczb.CXL + ", 大于150， 抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);

                logger.Warn("超限率=" + zh_st_zdczb.CXL + ", 大于150，抛弃=" + SuperRangeCount);
                return;
            }
            zh_st_zdczb.CZY = SysSet.ZDMC;
            zh_st_zdczb.FJBZ = 0;
            //switch (CD)
            //{
            //    case 1:
            //    case 2:
            //        zh_st_zdczb.FX = "文水→祁县";
            //        break;
            //    case 3:
            //    case 4:
            //        zh_st_zdczb.FX = "祁县→文水";
            //        break;
            //    default:
            //        break;
            //}

            zh_st_zdczb.JCSJ = DateTime.Now;
            zh_st_zdczb.JCZT = 0;
            zh_st_zdczb.SFXZ = 0;
            zh_st_zdczb.SJDJ = 0;
            zh_st_zdczb.ZDBZ = SysSet.ZDIP;
            zh_st_zdczb.ZX = 0;
            zh_st_zdczb.SFSC = 0;

            #endregion

            #region 入库保存


            try
            {
                //tData.DBData = zh_st_zdczb;

                //for (int i = 0; i < tData.ListCB.Count(); i++)
                //{
                //    InsertDgvRow(tData.ListCB[i].TDH.ToString(), tData.ListCB[i].JLZ.ToString(), tData.ListCB[i].Time.ToString(), "",
                //         zh_st_zdczb.ZZ.ToString(), tData.ListCB[i].CKZ.ToString(), tData.ListCB[i].RecvTime.ToString());

                //}

                logMainTable.CS = (int)zh_st_zdczb.CS;
                logMainTable.ZS = zh_st_zdczb.ZS;

                if ((zh_st_zdczb.ZZ > SysSet.MinZL && zh_st_zdczb.ZZ < SysSet.MaxZL) && (zh_st_zdczb.ZS >= SysSet.MinZS && zh_st_zdczb.ZS <= SysSet.MaxZS))
                {
                    if (!DBInsert(zh_st_zdczb))
                    {
                        logger.Debug("入库失败！" + zh_st_zdczb.ToString());
                        DBInFailedCount++;
                        logger.Warn("入库失败！= " + DBInFailedCount);
                    }
                    DBInSuccessCount++;
                    logger.Warn("入库成功！= " + DBInSuccessCount);

                    DALLogMainTable dalLogMainTable = new DALLogMainTable();

                    try
                    {
                        if (dalLogMainTable.Add(logMainTable) > 0)
                        {
                            Console.WriteLine("日志信息入库成功");
                        }
                        else
                        {
                            Console.WriteLine("日志信息入库失败");
                            logger.Info("日志信息入库失败");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("日志信息入库异常");
                        logger.Debug(ex, ex.ToString());
                    }

                    this.Invoke(new MethodInvoker(delegate
                    {
                        ShowDataOnListview(zh_st_zdczb);
                    }));


                    if (zh_st_zdczb.CPH != "无车牌" && zh_st_zdczb.SFCX == 1)
                    {
                        LedContentFrame(zh_st_zdczb);
                    }

                }
                else
                {
                    SuperRangeCount++;
                    logger.Info("总重或轴数不在设定的范围内，抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);
                    Console.WriteLine("总重或轴数不在设定的范围内，抛弃。总重=" + zh_st_zdczb.ZZ + ",轴数=" + zh_st_zdczb.ZS + ",车牌号=" + zh_st_zdczb.CPH);

                    logger.Warn("总重或轴数不在设定的范围内，抛弃=" + SuperRangeCount);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger.Debug(ex, ex.ToString());
            }
            #endregion
        }
        #endregion

        #endregion


        private void listV_DateList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {


                if (listV_DateList.SelectedItems.Count == 0)
                {
                    return;
                }

                switch (listV_DateList.SelectedItems[0].SubItems[columnH_CD.Index].Text)
                {
                    case "1":
                        if (Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text) >= SysSet.MinCXBZ && Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text) <= SysSet.MaxCXBZ)
                        {
                            lblCD1_CXL.ForeColor = Color.Red;
                            lblCD1_ZZ.ForeColor = Color.Red;
                        }
                        else
                        {
                            lblCD1_CXL.ForeColor = Color.Black;
                            lblCD1_ZZ.ForeColor = Color.Black;
                        }

                        txtCD1_CPH.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CPH.Index].Text;
                        lblCD1_ZZ.Text = ((Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_ZZ.Index].Text) / (double)1000).ToString());
                        lblCD1_CPYS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CPYS.Index].Text;
                        lblCD1_ZS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_ZS.Index].Text;
                        lblCD1_CS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CS.Index].Text;
                        lblCD1_JCSJ.Text = listV_DateList.SelectedItems[0].SubItems[columnHe_JCSJ.Index].Text;
                        lblCD1_CXL.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text;

                        if (listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text != "" && listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text != "0000" && File.Exists(listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text))
                        {
                            bmapCD1 = new Bitmap(listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text);
                            pictureCD1.Image = bmapCD1;

                            //pictureCD1.Image = Image.FromFile(zh_st_zdczb.CPTX);
                        }
                        break;
                    case "2":
                        if (Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text) >= SysSet.MinCXBZ && Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text) <= SysSet.MaxCXBZ)
                        {
                            lblCD2_CXL.ForeColor = Color.Red;
                            lblCD2_ZZ.ForeColor = Color.Red;
                        }
                        else
                        {
                            lblCD2_CXL.ForeColor = Color.Black;
                            lblCD2_ZZ.ForeColor = Color.Black;
                        }

                        txtCD2_CPH.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CPH.Index].Text;
                        lblCD2_ZZ.Text = ((Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_ZZ.Index].Text) / (double)1000).ToString());
                        lblCD2_CPYS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CPYS.Index].Text;
                        lblCD2_ZS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_ZS.Index].Text;
                        lblCD2_CS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CS.Index].Text;
                        lblCD2_JCSJ.Text = listV_DateList.SelectedItems[0].SubItems[columnHe_JCSJ.Index].Text;
                        lblCD2_CXL.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text;

                        if (listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text != "" && listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text != "0000" && File.Exists(listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text))
                        {
                            bmapCD2 = new Bitmap(listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text);
                            pictureCD2.Image = bmapCD2;
                        }
                        break;
                    case "3":
                        if (Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text) >= SysSet.MinCXBZ && Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text) <= SysSet.MaxCXBZ)
                        {
                            lblCD3_CXL.ForeColor = Color.Red;
                            lblCD3_ZZ.ForeColor = Color.Red;
                        }
                        else
                        {
                            lblCD3_CXL.ForeColor = Color.Black;
                            lblCD3_ZZ.ForeColor = Color.Black;
                        }

                        txtCD3_CPH.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CPH.Index].Text;
                        lblCD3_ZZ.Text = ((Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_ZZ.Index].Text) / (double)1000).ToString());
                        lblCD3_CPYS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CPYS.Index].Text;
                        lblCD3_ZS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_ZS.Index].Text;
                        lblCD3_CS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CS.Index].Text;
                        lblCD3_JCSJ.Text = listV_DateList.SelectedItems[0].SubItems[columnHe_JCSJ.Index].Text;
                        lblCD3_CXL.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text;

                        if (listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text != "" && listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text != "0000" && File.Exists(listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text))
                        {
                            bmapCD3 = new Bitmap(listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text);
                            pictureCD3.Image = bmapCD3;
                        }
                        break;
                    case "4":
                        if (Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text) >= SysSet.MinCXBZ && Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text) <= SysSet.MaxCXBZ)
                        {
                            lblCD4_CXL.ForeColor = Color.Red;
                            lblCD4_ZZ.ForeColor = Color.Red;
                        }
                        else
                        {
                            lblCD4_CXL.ForeColor = Color.Black;
                            lblCD4_ZZ.ForeColor = Color.Black;
                        }

                        txtCD4_CPH.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CPH.Index].Text;
                        lblCD4_ZZ.Text = ((Convert.ToInt32(listV_DateList.SelectedItems[0].SubItems[columnH_ZZ.Index].Text) / (double)1000).ToString());
                        lblCD4_CPYS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CPYS.Index].Text;
                        lblCD4_ZS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_ZS.Index].Text;
                        lblCD4_CS.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CS.Index].Text;
                        lblCD4_JCSJ.Text = listV_DateList.SelectedItems[0].SubItems[columnHe_JCSJ.Index].Text;
                        lblCD4_CXL.Text = listV_DateList.SelectedItems[0].SubItems[columnH_CXL.Index].Text;

                        if (listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text != "" && listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text != "0000" && File.Exists(listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text))
                        {
                            bmapCD4 = new Bitmap(listV_DateList.SelectedItems[0].SubItems[columnH_CPTX.Index].Text);
                            pictureCD4.Image = bmapCD4;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Debug("主界面选择数据异常：" + ex.ToString(), ex);
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            frmQuery query = new frmQuery(this);
            query.Show();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
            //ThreadPool.QueueUserWorkItem(new WaitCallback(x => StartManuanSnap(2, 1, true)));
            //StartManuanSnap(2, 1, true);
            //ContinuousShoot(m_lUserID[1]);

            //CaptureJpeg(m_lUserIDListPreview[0], true, false, 123);
        }

    }
}
