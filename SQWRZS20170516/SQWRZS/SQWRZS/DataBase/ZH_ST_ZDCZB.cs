using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS.DataBase
{
    /// <summary>
    /// zh_st_zdczb:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    public class ZH_ST_ZDCZB
    {
        public ZH_ST_ZDCZB() { }


        #region Model
        private int _id;
        private string _cph;
        private string _cpys;
        private long _zz;
        private int _zs;
        private decimal _cs;
        private long _xz;
        private int _cxl;
        private string _cptx;
        private DateTime _jcsj;
        private int? _zx;
        private int? _cd;
        private string _czy;
        private string _zdbz;
        private int? _sfcx;
        private int? _sfxz;
        private int? _fjbz;
        private long? _xhzl;
        private int? _jczt;
        private int? _sjdj;
        private string _plate;
        private string _fx;

        private string _cwtx;
        private string _cmtx;
        private string _video;

        private int _sfsc;
         

        /// <summary>
        /// 自增序号
        /// </summary>
        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 车牌号
        /// </summary>
        public string CPH
        {
            set { _cph = value; }
            get { return _cph; }
        }
        /// <summary>
        /// 车牌颜色
        /// </summary>
        public string CPYS
        {
            set { _cpys = value; }
            get { return _cpys; }
        }
        /// <summary>
        /// 总重
        /// </summary>
        public long ZZ
        {
            set { _zz = value; }
            get { return _zz; }
        }
        /// <summary>
        /// 轴数
        /// </summary>
        public int ZS
        {
            set { _zs = value; }
            get { return _zs; }
        }
        /// <summary>
        /// 车速
        /// </summary>
        public decimal CS
        {
            set { _cs = value; }
            get { return _cs; }
        }
        /// <summary>
        /// 限载
        /// </summary>
        public long XZ
        {
            set { _xz = value; }
            get { return _xz; }
        }
        /// <summary>
        /// 超限率
        /// </summary>
        public int CXL
        {
            set { _cxl = value; }
            get { return _cxl; }
        }
        /// <summary>
        /// 车牌图像
        /// </summary>
        public string CPTX
        {
            set { _cptx = value; }
            get { return _cptx; }
        }
        /// <summary>
        /// 检测时间
        /// </summary>
        public DateTime JCSJ
        {
            set { _jcsj = value; }
            get { return _jcsj; }
        }
        /// <summary>
        /// 轴型
        /// </summary>
        public int? ZX
        {
            set { _zx = value; }
            get { return _zx; }
        }
        /// <summary>
        /// 车道
        /// </summary>
        public int? CD
        {
            set { _cd = value; }
            get { return _cd; }
        }
        /// <summary>
        /// 操作员
        /// </summary>
        public string CZY
        {
            set { _czy = value; }
            get { return _czy; }
        }
        /// <summary>
        /// 站点IP
        /// </summary>
        public string ZDBZ
        {
            set { _zdbz = value; }
            get { return _zdbz; }
        }
        /// <summary>
        /// 是否超限,0:未超限，1：超限
        /// </summary>
        public int? SFCX
        {
            set { _sfcx = value; }
            get { return _sfcx; }
        }
        /// <summary>
        /// 是否修正
        /// </summary>
        public int? SFXZ
        {
            set { _sfxz = value; }
            get { return _sfxz; }
        }
        /// <summary>
        /// 复检标识
        /// </summary>
        public int? FJBZ
        {
            set { _fjbz = value; }
            get { return _fjbz; }
        }
        /// <summary>
        /// 卸货重量
        /// </summary>
        public long? XHZL
        {
            set { _xhzl = value; }
            get { return _xhzl; }
        }
        /// <summary>
        /// 检测状态
        /// </summary>
        public int? JCZT
        {
            set { _jczt = value; }
            get { return _jczt; }
        }
        /// <summary>
        /// 数据登记
        /// </summary>
        public int? SJDJ
        {
            set { _sjdj = value; }
            get { return _sjdj; }
        }
        /// <summary>
        /// 车牌图像（存留字段）
        /// </summary>
        public string PLATE
        {
            set { _plate = value; }
            get { return _plate; }
        }
        /// <summary>
        /// 方向
        /// </summary>
        public string FX
        {
            set { _fx = value; }
            get { return _fx; }
        }
        /// <summary>
        /// 车尾图像
        /// </summary>
        public string CWTX
        {
            get
            {
                return _cwtx;
            }

            set
            {
                _cwtx = value;
            }
        }
        /// <summary>
        /// 侧面图像
        /// </summary>
        public string CMTX
        {
            get
            {
                return _cmtx;
            }

            set
            {
                _cmtx = value;
            }
        }

        /// <summary>
        /// 侧面图像
        /// </summary>
        public string Video
        {
            get
            {
                return _video;
            }

            set
            {
                _video = value;
            }
        }

        /// <summary>
        /// 是否上传
        /// </summary>
        public int SFSC
        {
            get
            {
                return _sfsc;
            }

            set
            {
                _sfsc = value;
            }
        } 
        #endregion Model

    }
}
