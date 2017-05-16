using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS.DataBase
{
    /// <summary>
    /// CBData:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    class CBDataTable
    {
        public CBDataTable()
        { }
        #region Model
        private int _id;
        private string _tdh;
        private string _jlz;
        private string _sk;
        private string _skc;
        private string _ckz;
        private string _jssj;
        private string _yssj;
        /// <summary>
        /// 
        /// </summary>
        public int ID
        {
            set { _id = value; }
            get { return _id; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string TDH
        {
            set { _tdh = value; }
            get { return _tdh; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string JLZ
        {
            set { _jlz = value; }
            get { return _jlz; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string SK
        {
            set { _sk = value; }
            get { return _sk; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string SKC
        {
            set { _skc = value; }
            get { return _skc; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string CKZ
        {
            set { _ckz = value; }
            get { return _ckz; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string JSSJ
        {
            set { _jssj = value; }
            get { return _jssj; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string YSSJ
        {
            set { _yssj = value; }
            get { return _yssj; }
        }
        #endregion Model
    }
}
