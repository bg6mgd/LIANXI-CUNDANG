using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS
{
    /// <summary>
    /// 车牌识别结果
    /// </summary>
    public class CpsbResult
    {
        public string CPH;
        public string CPYS;
        public short CD_GDW;
        public string Plate;
        public string photoPath;
        public DateTime CPSB_Time;
        public string DeviceIP;//设备ip地址
        public string CDFX;
        public int  picNum;
        /// <summary>
        /// 抓拍类型,0默认车头,1车尾,2侧面
        /// </summary>
        public int SnapType = 0;
        /// <summary>
        /// 牌识是否可以存放，true可以，false不可以
        /// </summary>
        public bool CPSB_Flag;

        public CpsbResult()
        {
            CPH = "";
            CPYS = "";
            CD_GDW = 0;
            Plate = "";
            photoPath = "";
            CPSB_Time = DateTime.Now;  //默认时间，对比时不为空 
            CPSB_Flag = true;
            DeviceIP = "";
            CDFX = "";
        }
    }
}
