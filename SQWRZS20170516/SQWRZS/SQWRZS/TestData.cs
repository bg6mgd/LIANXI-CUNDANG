using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQWRZS
{
    public class TestData
    {
        DataBase.ZH_ST_ZDCZB _DBData;
        /// <summary>
        /// 入库的对象数据
        /// </summary>
        public DataBase.ZH_ST_ZDCZB DBData
        {
            get { return _DBData; }
            set { _DBData = value; }
        }

        private List<CBData> _ListCB;
        /// <summary>
        /// 称板数据
        /// </summary>
        public List<CBData> ListCB
        {
            get { return _ListCB; }
            set { _ListCB = value; }
        }
    }
}
