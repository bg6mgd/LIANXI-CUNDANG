using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS.Devices
{
    public class LedSend
    {
        string cph = "";
        /// <summary>
        /// 车牌号
        /// </summary>
        public string Cph
        {
            get { return cph; }
            set { cph = value; }
        }

        int sfcx = 0;
        /// <summary>
        /// 是否超限
        /// </summary>
        public int Sfcx
        {
            get { return sfcx; }
            set { sfcx = value; }
        }

        int zs = 2;
        /// <summary>
        /// 轴数
        /// </summary>
        public int Zs
        {
            get { return zs; }
            set { zs = value; }
        }

        long zz = 0;
        /// <summary>
        /// 总重
        /// </summary>
        public long Zz
        {
            get { return zz; }
            set { zz = value; }
        }

        long xhzl = 0;
        /// <summary>
        /// 卸货重量
        /// </summary>
        public long Xhzl
        {
            get { return xhzl; }
            set { xhzl = value; }
        }

        DateTime jcsj = DateTime.Now;
        /// <summary>
        /// 车辆检测记录
        /// </summary>
        public DateTime Jcsj
        {
            get { return jcsj; }
            set { jcsj = value; }
        }
    }
}
