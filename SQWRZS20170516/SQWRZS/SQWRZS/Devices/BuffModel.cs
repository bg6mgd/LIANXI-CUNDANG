using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS.Devices
{
    /// <summary>
    /// 车辆数据缓存类
    /// </summary>
    public class BuffModel
    {
        /// <summary>
        /// 缓存标识，有缓存数据为true，无缓存数据为false
        /// </summary>
        public bool BuffFlag { get; set; }
        /// <summary>
        /// 缓存完成的时间
        /// </summary>
        public DateTime BuffFinishedTime { get; set; }
        /// <summary>
        /// 缓存称板数据
        /// </summary>
        public List<CBData> ListCB { get; set; }
        /// <summary>
        /// 缓存触发线圈
        /// </summary>
        public XianQuan ChufaXQ { get; set; }
        /// <summary>
        /// 缓存收尾线圈
        /// </summary>
        public XianQuan ShouweiXQ { get; set; }
        /// <summary>
        /// 缓存时间差
        /// </summary>
        public int TimeCha { get; set; }

        /// <summary>
        /// 缓存计算后的数据信息
        /// </summary>
        public SQWRZS.DataBase.ZH_ST_ZDCZB zh_st_zdczb { get; set; }
    }
}
