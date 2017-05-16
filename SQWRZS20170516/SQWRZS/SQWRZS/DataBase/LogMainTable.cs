using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS.DataBase
{
    /// <summary>
	/// 实体类LogMainTable 。(属性说明自动提取数据库字段的描述信息)
	/// </summary>
	[Serializable]
    public class LogMainTable
    {
        public LogMainTable()
        { }
        #region Model
        private string _logid;
        private int? _axisskc4;
        private int? _axisskc5;
        private int? _axisskc6;
        private int? _axisskc7;
        private int? _axisskc8;
        private int _zz;
        private int _cs;
        private decimal _xzpara;
        private int _xzzz;
        private int _zs;
        private int? _axisskc1;
        private int? _axisskc2;
        private int? _axisskc3;
        /// <summary>
        /// 
        /// </summary>
        public string LogID
        {
            set { _logid = value; }
            get { return _logid; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? AxisSKC4
        {
            set { _axisskc4 = value; }
            get { return _axisskc4; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? AxisSKC5
        {
            set { _axisskc5 = value; }
            get { return _axisskc5; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? AxisSKC6
        {
            set { _axisskc6 = value; }
            get { return _axisskc6; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? AxisSKC7
        {
            set { _axisskc7 = value; }
            get { return _axisskc7; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? AxisSKC8
        {
            set { _axisskc8 = value; }
            get { return _axisskc8; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int ZZ
        {
            set { _zz = value; }
            get { return _zz; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int CS
        {
            set { _cs = value; }
            get { return _cs; }
        }
        /// <summary>
        /// 
        /// </summary>
        public decimal XZPara
        {
            set { _xzpara = value; }
            get { return _xzpara; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int XZZZ
        {
            set { _xzzz = value; }
            get { return _xzzz; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int ZS
        {
            set { _zs = value; }
            get { return _zs; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? AxisSKC1
        {
            set { _axisskc1 = value; }
            get { return _axisskc1; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? AxisSKC2
        {
            set { _axisskc2 = value; }
            get { return _axisskc2; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int? AxisSKC3
        {
            set { _axisskc3 = value; }
            get { return _axisskc3; }
        }
        #endregion Model

        private List<LogDetailTable> _logdetailtables;
        /// <summary>
        /// 子类 
        /// </summary>
        public List<LogDetailTable> LogDetailTables
        {
            set { _logdetailtables = value; }
            get { return _logdetailtables; }
        }

    } 
}
