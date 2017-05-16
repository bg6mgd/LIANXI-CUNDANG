using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS.DataBase
{
    /// <summary>
    /// 实体类LogDetailTable 。(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    public class LogDetailTable
    {
        public LogDetailTable()
        { }
        #region Model
        private int _id;
        private string _logid;
        private int _cbno;
        private int _jlz;
        private int _sk;
        private int _cbzz;
        private int _usedskc = 0;
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
        public string LogID
        {
            set { _logid = value; }
            get { return _logid; }
        }
        /// <summary>
        /// 称板编号
        /// </summary>
        public int CBNo
        {
            set { _cbno = value; }
            get { return _cbno; }
        }
        /// <summary>
        /// 计量值
        /// </summary>
        public int JLZ
        {
            set { _jlz = value; }
            get { return _jlz; }
        }
        /// <summary>
        /// 时刻（四方仪表），最高点sample值（华驰仪表）
        /// </summary>
        public int SK
        {
            set { _sk = value; }
            get { return _sk; }
        }
        /// <summary>
        /// 称板计算后的板重
        /// </summary>
        public int CBZZ
        {
            set { _cbzz = value; }
            get { return _cbzz; }
        }
        /// <summary>
        /// 使用的时刻差（四方仪表），最高点index值（华驰仪表）
        /// </summary>
        public int UsedSKC
        {
            set { _usedskc = value; }
            get { return _usedskc; }
        }
        #endregion Model
    }
}
