using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS.DataBase
{ 
    /// <summary>
    /// 逆行表:实体类(属性说明自动提取数据库字段的描述信息)
    /// </summary>
    [Serializable]
    public class NXTable
    {
        public NXTable()
        { }
        #region Model
        private int _id;
        private string _cph;
        private string _cpys;
        private string _cptx;
        private string _plate;
        private string _cwtx;
        private string _cmtx;
        private string _video;
        private DateTime _jcsj;
        /// <summary>
        /// ID
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
        /// 车头图像
        /// </summary>
        public string CPTX
        {
            set { _cptx = value; }
            get { return _cptx; }
        }
        /// <summary>
        /// 车牌图像
        /// </summary>
        public string PLATE
        {
            set { _plate = value; }
            get { return _plate; }
        }
        /// <summary>
        /// 车尾图像
        /// </summary>
        public string CWTX
        {
            set { _cwtx = value; }
            get { return _cwtx; }
        }
        /// <summary>
        /// 侧面图像
        /// </summary>
        public string CMTX
        {
            set { _cmtx = value; }
            get { return _cmtx; }
        }
        /// <summary>
        /// 视频
        /// </summary>
        public string Video
        {
            set { _video = value; }
            get { return _video; }
        }
        /// <summary>
        /// 检测时间
        /// </summary>
        public DateTime JCSJ
        {
            set { _jcsj = value; }
            get { return _jcsj; }
        }
        #endregion Model

    }
}
