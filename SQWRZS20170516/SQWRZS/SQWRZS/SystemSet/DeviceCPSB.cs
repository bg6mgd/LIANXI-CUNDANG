using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS.SystemSet
{
    public class DeviceCPSB
    {
        public DeviceCPSB() { }

        private string _IP;
        /// <summary>
        /// 车牌识别IP
        /// </summary>
        public string IP
        {
            get { return _IP; }
            set { _IP = value; }
        }

        private string _User;
        /// <summary>
        /// 登录用户
        /// </summary>
        public string User
        {
            get { return _User; }
            set { _User = value; }
        }

        private string _Pwd;
        /// <summary>
        /// 访问密码
        /// </summary>
        public string Pwd
        {
            get { return _Pwd; }
            set { _Pwd = value; }
        }

        private int _NetPort;
        /// <summary>
        /// 网络端口
        /// </summary>
        public int NetPort
        {
            get { return _NetPort; }
            set { _NetPort = value; }
        }

        private string _FangXiang;
        /// <summary>
        /// 方向
        /// </summary>
        public string FangXiang
        {
            get { return _FangXiang; }
            set { _FangXiang = value; }
        }
    }
}
