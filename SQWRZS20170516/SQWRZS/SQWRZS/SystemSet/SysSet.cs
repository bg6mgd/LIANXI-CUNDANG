using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace SQWRZS.SystemSet
{
    public class SysSet
    {
        public SysSet() { }

        #region SoftSet

        #region EchoOrData


        private string _SystemTitle;
        /// <summary>
        /// 系统标题
        /// </summary>
        public string SystemTitle
        {
            get { return _SystemTitle; }
            set { _SystemTitle = value; }
        }

        private string _DataSavePath;
        /// <summary>
        /// 数据存贮路径
        /// </summary>
        public string DataSavePath
        {
            get { return _DataSavePath; }
            set { _DataSavePath = value; }
        }

        private int _DataSaveDays;
        /// <summary>
        /// 数据存贮天数
        /// </summary>
        public int DataSaveDays
        {
            get { return _DataSaveDays; }
            set { _DataSaveDays = value; }
        }

        private string _ZDIP;
        /// <summary>
        /// 站点IP
        /// </summary>
        public string ZDIP
        {
            get { return _ZDIP; }
            set { _ZDIP = value; }
        }

        private string _ZDMC;
        /// <summary>
        /// 站点名称
        /// </summary>
        public string ZDMC
        {
            get { return _ZDMC; }
            set { _ZDMC = value; }
        }

        private int _MaxZS;
        /// <summary>
        /// 最大轴数
        /// </summary>
        public int MaxZS
        {
            get { return _MaxZS; }
            set { _MaxZS = value; }
        }

        private int _MinZS;
        /// <summary>
        /// 最小轴数
        /// </summary>
        public int MinZS
        {
            get { return _MinZS; }
            set { _MinZS = value; }
        }

        private int _MaxZL;
        /// <summary>
        /// 最大重量
        /// </summary>
        public int MaxZL
        {
            get { return _MaxZL; }
            set { _MaxZL = value; }
        }

        private int _MinZL;
        /// <summary>
        /// 最小重量
        /// </summary>
        public int MinZL
        {
            get { return _MinZL; }
            set { _MinZL = value; }
        }

        private int _MaxCXBZ;
        /// <summary>
        /// 最大超限标准
        /// </summary>
        public int MaxCXBZ
        {
            get { return _MaxCXBZ; }
            set { _MaxCXBZ = value; }
        }

        private int _MinCXBZ;
        /// <summary>
        /// 最小超限标准
        /// </summary>
        public int MinCXBZ
        {
            get { return _MinCXBZ; }
            set { _MinCXBZ = value; }
        }

        private int _K;
        /// <summary>
        /// K值
        /// </summary>
        public int K
        {
            get { return _K; }
            set { _K = value; }
        }

        private int _PreTime;
        /// <summary>
        /// 往线圈触发之前寻找称板的时间
        /// </summary>
        public int PreTime
        {
            get { return _PreTime; }
            set { _PreTime = value; }
        }

        private int _NextTime;
        /// <summary>
        /// 往线圈收尾之后寻找称板的时间
        /// </summary>
        public int NextTime
        {
            get { return _NextTime; }
            set { _NextTime = value; }
        }

        private int _PsInterverTime;
        /// <summary>
        /// 同一个牌识的接收结果间隔
        /// </summary>
        public int PsInterverTime
        {
            get { return _PsInterverTime; }
            set { _PsInterverTime = value; }
        }

        private int _XqInterverTime;
        /// <summary>
        /// 相邻线圈算作同时触发的时间间隔
        /// </summary>
        public int XqInterverTime
        {
            get { return _XqInterverTime; }
            set { _XqInterverTime = value; }
        }

        private int _Axle2WeightMax;
        /// <summary>
        /// 2轴最大限载
        /// </summary>
        public int Axle2WeightMax
        {
            get
            {
                return _Axle2WeightMax;
            }

            set
            {
                _Axle2WeightMax = value;
            }
        }

        private int _Axle3WeightMax;
        /// <summary>
        /// 3轴最大限载
        /// </summary>
        public int Axle3WeightMax
        {
            get
            {
                return _Axle3WeightMax;
            }

            set
            {
                _Axle3WeightMax = value;
            }
        }

        private int _Axle4WeightMax;
        /// <summary>
        /// 4轴最大限载
        /// </summary>
        public int Axle4WeightMax
        {
            get
            {
                return _Axle4WeightMax;
            }

            set
            {
                _Axle4WeightMax = value;
            }
        }

        private int _Axle5WeightMax;
        /// <summary>
        /// 5轴最大限载
        /// </summary>
        public int Axle5WeightMax
        {
            get
            {
                return _Axle5WeightMax;
            }

            set
            {
                _Axle5WeightMax = value;
            }
        }

        private int _Axle6WeightMax;
        /// <summary>
        /// 6轴最大限载
        /// </summary>
        public int Axle6WeightMax
        {
            get
            {
                return _Axle6WeightMax;
            }

            set
            {
                _Axle6WeightMax = value;
            }
        }

        private int _K13;
        /// <summary>
        /// 1和3块板参数
        /// </summary>
        public int K13
        {
            get { return _K13; }
            set { _K13 = value; }
        }

        private int _K35;
        //3和5块板参数
        public int K35
        {
            get { return _K35; }
            set { _K35 = value; }
        }

        private int _K57;
        //5和7块板参数
        public int K57
        {
            get { return _K57; }
            set { _K57 = value; }
        }

        private int _K79;
        //7和9块板参数
        public int K79
        {
            get { return _K79; }
            set { _K79 = value; }
        }

        private int _K911;
        //9和11块板参数
        public int K911
        {
            get { return _K911; }
            set { _K911 = value; }
        }

        private int _K24;
        //2和4块板参数
        public int K24
        {
            get { return _K24; }
            set { _K24 = value; }
        }

        private int _K46;
        //4和6块板参数
        public int K46
        {
            get { return _K46; }
            set { _K46 = value; }
        }

        private int _K68;
        //6和8块板参数
        public int K68
        {
            get { return _K68; }
            set { _K68 = value; }
        }

        private int _K810;
        //8和10块板参数
        public int K810
        {
            get { return _K810; }
            set { _K810 = value; }
        }
        #endregion

        #region SpeedXZ速度修正
        private int _SpeedXZInterver;
        /// <summary>
        /// 速度修正间隔，例如5代表每隔5公里一个修正参数
        /// </summary>
        public int SpeedXZInterver
        {
            get
            {
                return _SpeedXZInterver;
            }

            set
            {
                _SpeedXZInterver = value;
            }
        }

        private Double[] _SpeedXZPara;
        /// <summary>
        /// 速度修正参数，计算的重量需要乘以该段的参数
        /// </summary>
        public Double[] SpeedXZPara
        {
            get
            {
                if (_SpeedXZPara==null)
                {
                    _SpeedXZPara = new double[100];
                }
                return _SpeedXZPara;
            }

            set
            {
                _SpeedXZPara = value;
            }
        }

        private Double[] _SpeedXZPara13;
        /// <summary>
        /// 13速度修正参数，计算的重量需要乘以该段的参数
        /// </summary>
        public Double[] SpeedXZPara13
        {
            get
            {
                if (_SpeedXZPara13 == null)
                {
                    _SpeedXZPara13 = new double[100];
                }
                return _SpeedXZPara13;
            }

            set
            {
                _SpeedXZPara13 = value;
            }
        }

        private Double[] _SpeedXZPara35;
        /// <summary>
        /// 35速度修正参数，计算的重量需要乘以该段的参数
        /// </summary>
        public Double[] SpeedXZPara35
        {
            get
            {
                if (_SpeedXZPara35 == null)
                {
                    _SpeedXZPara35 = new double[100];
                }
                return _SpeedXZPara35;
            }

            set
            {
                _SpeedXZPara35 = value;
            }
        }

        private Double[] _SpeedXZPara57;
        /// <summary>
        /// 57速度修正参数，计算的重量需要乘以该段的参数
        /// </summary>
        public Double[] SpeedXZPara57
        {
            get
            {
                if (_SpeedXZPara57 == null)
                {
                    _SpeedXZPara57 = new double[100];
                }
                return _SpeedXZPara57;
            }

            set
            {
                _SpeedXZPara57 = value;
            }
        }

        private Double[] _SpeedXZPara79;
        /// <summary>
        /// 79速度修正参数，计算的重量需要乘以该段的参数
        /// </summary>
        public Double[] SpeedXZPara79
        {
            get
            {
                if (_SpeedXZPara79 == null)
                {
                    _SpeedXZPara79 = new double[100];
                }
                return _SpeedXZPara79;
            }

            set
            {
                _SpeedXZPara79 = value;
            }
        }

        private Double[] _SpeedXZPara911;
        /// <summary>
        /// 911速度修正参数，计算的重量需要乘以该段的参数
        /// </summary>
        public Double[] SpeedXZPara911
        {
            get
            {
                if (_SpeedXZPara911 == null)
                {
                    _SpeedXZPara911 = new double[100];
                }
                return _SpeedXZPara911;
            }

            set
            {
                _SpeedXZPara911 = value;
            }
        }

        private Double[] _SpeedXZPara24;
        /// <summary>
        /// 24速度修正参数，计算的重量需要乘以该段的参数
        /// </summary>
        public Double[] SpeedXZPara24
        {
            get
            {
                if (_SpeedXZPara24 == null)
                {
                    _SpeedXZPara24 = new double[100];
                }
                return _SpeedXZPara24;
            }

            set
            {
                _SpeedXZPara24 = value;
            }
        }
        private Double[] _SpeedXZPara46;
        /// <summary>
        /// 46速度修正参数，计算的重量需要乘以该段的参数
        /// </summary>
        public Double[] SpeedXZPara46
        {
            get
            {
                if (_SpeedXZPara46 == null)
                {
                    _SpeedXZPara46 = new double[100];
                }
                return _SpeedXZPara46;
            }

            set
            {
                _SpeedXZPara46 = value;
            }
        }

        private Double[] _SpeedXZPara68;
        /// <summary>
        /// 68速度修正参数，计算的重量需要乘以该段的参数
        /// </summary>
        public Double[] SpeedXZPara68
        {
            get
            {
                if (_SpeedXZPara68 == null)
                {
                    _SpeedXZPara68 = new double[100];
                }
                return _SpeedXZPara68;
            }

            set
            {
                _SpeedXZPara68 = value;
            }
        }

        private Double[] _SpeedXZPara810;
        /// <summary>
        /// 810速度修正参数，计算的重量需要乘以该段的参数
        /// </summary>
        public Double[] SpeedXZPara810
        {
            get
            {
                if (_SpeedXZPara810 == null)
                {
                    _SpeedXZPara810 = new double[100];
                }
                return _SpeedXZPara810;
            }

            set
            {
                _SpeedXZPara810 = value;
            }
        }

        #endregion

        #region DataBase

        private string _DBIP;
        /// <summary>
        /// 数据库IP
        /// </summary>
        public string DBIP
        {
            get { return _DBIP; }
            set { _DBIP = value; }
        }

        private string _DBName;
        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DBName
        {
            get { return _DBName; }
            set { _DBName = value; }
        }

        private string _DBUser;
        /// <summary>
        /// 数据库用户名
        /// </summary>
        public string DBUser
        {
            get { return _DBUser; }
            set { _DBUser = value; }
        }

        private string _DBPwd;
        /// <summary>
        /// 数据库访问密码
        /// </summary>
        public string DBPwd
        {
            get { return _DBPwd; }
            set { _DBPwd = value; }
        }
        #endregion

        #region Ftp
        private string _FtpIP;
        /// <summary>
        /// FTP地址
        /// </summary>
        public string FtpIP
        {
            get { return _FtpIP; }
            set { _FtpIP = value; }
        }

        private string _FtpPath;
        /// <summary>
        /// FTP路径
        /// </summary>
        public string FtpPath
        {
            get { return _FtpPath; }
            set { _FtpPath = value; }
        }
        private string _FtpUser;
        /// <summary>
        /// FTP用户
        /// </summary>
        public string FtpUser
        {
            get { return _FtpUser; }
            set { _FtpUser = value; }
        }

        private string _FtpPwd;
        /// <summary>
        /// FTP访问密码
        /// </summary>
        public string FtpPwd
        {
            get { return _FtpPwd; }
            set { _FtpPwd = value; }
        }

        private int _FtpPort;
        /// <summary>
        /// FTP访问端口
        /// </summary>
        public int FtpPort
        {
            get { return _FtpPort; }
            set { _FtpPort = value; }
        }
        #endregion

        #endregion

        #region DeviceSet

        #region CPSB

        private List<DeviceCPSB> _CpsbList;
        /// <summary>
        /// 牌识设备列表
        /// </summary>
        public List<DeviceCPSB> CpsbList
        {
            get
            {
                if (_CpsbList == null)
                {
                    _CpsbList = new List<DeviceCPSB>();
                }
                return _CpsbList;
            }
            set { _CpsbList = value; }
        }

        #endregion

        #region CZY
        private string _CZYName;
        /// <summary>
        /// 称重仪厂家名称标识
        /// </summary>
        public string CZYName
        {
            get
            {
                return _CZYName;
            }

            set
            {
                _CZYName = value;
            }
        }

        private string _CZYPort;
        /// <summary>
        /// 称重仪端口
        /// </summary>
        public string CZYPort
        {
            get { return _CZYPort; }
            set { _CZYPort = value; }
        }

        private int _CZYBaudRate;
        /// <summary>
        /// 称重仪波特率
        /// </summary>
        public int CZYBaudRate
        {
            get { return _CZYBaudRate; }
            set { _CZYBaudRate = value; }
        }

        private Parity _CZYParity;
        /// <summary>
        /// 称重仪校验位
        /// </summary>
        public Parity CZYParity
        {
            get { return _CZYParity; }
            set { _CZYParity = value; }
        }

        private int _CZYDataBit;
        /// <summary>
        /// 称重仪数据位
        /// </summary>
        public int CZYDataBit
        {
            get { return _CZYDataBit; }
            set { _CZYDataBit = value; }
        }

        private StopBits _CZYStopBits;
        /// <summary>
        /// 称重仪停止位
        /// </summary>
        public StopBits CZYStopBits
        {
            get { return _CZYStopBits; }
            set { _CZYStopBits = value; }
        }

       



        #endregion

        #endregion
    }
}
