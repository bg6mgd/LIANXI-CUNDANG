using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS.Devices
{
    /// <summary>
    /// 线圈对象类
    /// </summary>
    public class XianQuan
    {
        private int _XQBH;
        /// <summary>
        /// 线圈编号
        /// </summary>
        public int XQBH
        {
            get { return _XQBH; }
            set { _XQBH = value; }
        }

        private DateTime _ChufaTime;
        /// <summary>
        /// 触发时间
        /// </summary>
        public DateTime ChufaTime
        {
            get { return _ChufaTime; }
            set { _ChufaTime = value; }
        }

        private DateTime _ShouweiTime;
        /// <summary>
        /// 收尾时间
        /// </summary>
        public DateTime ShouweiTime
        {
            get { return _ShouweiTime; }
            set { _ShouweiTime = value; }
        }

        private int _XQZTChufa = -1;
        /// <summary>
        /// 线圈触发状态,单独触发设置为0，相邻线圈一起触发设置为2,收尾之后需要设置为-1
        /// </summary>
        public int XQZTChufa
        {
            get { return _XQZTChufa; }
            set { _XQZTChufa = value; }
        }

        private int _XQZTShouwei = -1;
        /// <summary>
        /// 线圈收尾状态,判断线圈触发状态为0或者2后设置收尾状态为1，收尾完成后设置为-1
        /// </summary>
        public int XQZTShouwei
        {
            get { return _XQZTShouwei; }
            set { _XQZTShouwei = value; }
        }

        private bool _Flag = false;
        /// <summary>
        /// 同时收尾进入计算标志，如果为true，则相邻通道已经计算过，将标志设置为false即可
        /// </summary>
        public bool Flag
        {
            get { return _Flag; }
            set { _Flag = value; }
        }

        private int _SampleNoChufa = -1;
        /// <summary>
        /// 触发线圈的SampleNo 
        /// </summary>
        public int SampleNoChufa
        {
            get { return _SampleNoChufa; }
            set { _SampleNoChufa = value; }
        }

        private int _SampleNoShouwei = -1;
        /// <summary>
        /// 收尾线圈的SampleNo 
        /// </summary>
        public int SampleNoShouwei
        {
            get { return _SampleNoShouwei; }
            set { _SampleNoShouwei = value; }
        }

    }
}
