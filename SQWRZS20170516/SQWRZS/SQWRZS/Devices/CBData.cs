using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS
{
    /// <summary>
    /// 称板数据
    /// </summary>
    public class CBData
    {
        private int _TDH;
        /// <summary>
        /// 通道号
        /// </summary>
        public int TDH
        {
            get { return _TDH; }
            set { _TDH = value; }
        }

        private int _JLZ;
        /// <summary>
        /// 称板计量值
        /// </summary>
        public int JLZ
        {
            get { return _JLZ; }
            set { _JLZ = value; }
        }

        private int _Time;
        /// <summary>
        /// 称板时刻
        /// </summary>
        public int Time
        {
            get { return _Time; }
            set { _Time = value; }
        }

        private int _KD;
        /// <summary>
        /// 宽度
        /// </summary>
        public int KD
        {
            get { return _KD; }
            set { _KD = value; }
        }

        private int _CKZ;
        /// <summary>
        /// 参考值
        /// </summary>
        public int CKZ
        {
            get { return _CKZ; }
            set { _CKZ = value; }
        }

        private bool _UESED;
        /// <summary>
        /// 数据是否已经使用过，使用过后的可以清除
        /// </summary>
        public bool UESED
        {
            get { return _UESED; }
            set { _UESED = value; }
        }

        private int _CurrentZS;
        /// <summary>
        /// 当前轴数
        /// </summary>
        public int CurrentZS
        {
            get { return _CurrentZS; }
            set { _CurrentZS = value; }
        }

        private DateTime _RecvTime;
        /// <summary>
        /// 接收称板数据的时间
        /// </summary>
        public DateTime RecvTime
        {
            get { return _RecvTime; }
            set { _RecvTime = value; }
        }

        private int _TimeCha;
        /// <summary>
        /// 时间差
        /// </summary>
        public int TimeCha
        {
            get { return _TimeCha; }
            set { _TimeCha = value; }
        }

        private int _ZZ;
        /// <summary>
        /// 总重
        /// </summary>
        public int ZZ
        {
            get { return _ZZ; }
            set { _ZZ = value; }
        }

        

        /*华驰仪表新增
         */

        private int _FrontBaseLineNum;
        /// <summary>
        /// 前基线值,重量开始的基线值，低位在前，高位在后
        /// </summary>
        public int FrontBaseLineNum
        {
            get
            {
                return _FrontBaseLineNum;
            }

            set
            {
                _FrontBaseLineNum = value;
            }
        }

        private int _BackBaseLineNum;
        /// <summary>
        /// 后基线值,重量结束的基线值，低位在前，高位在后
        /// </summary>
        public int BackBaseLineNum
        {
            get
            {
                return _BackBaseLineNum;
            }

            set
            {
                _BackBaseLineNum = value;
            }
        } 

        private int _Peakvalue;
        /// <summary>
        /// 波形的最大波峰值，低位在前，高位在后
        /// </summary>
        public int Peakvalue
        {
            get
            {
                return _Peakvalue;
            }

            set
            {
                _Peakvalue = value;
            }
        } 
       
        private int _SampleMax;
        /// <summary>
        /// 表示波形最高点的sample号，低位在前，高位在后
        /// </summary>
        public int SampleMax
        {
            get
            {
                return _SampleMax;
            }

            set
            {
                _SampleMax = value;
            }
        }

        private int _IndexMax;
        /// <summary>
        /// 表示波形最高点的indx值，从0-1023，低位在前，高位在后
        /// </summary>
        public int IndexMax
        {
            get
            {
                return _IndexMax;
            }

            set
            {
                _IndexMax = value;
            }
        }

    }
}
