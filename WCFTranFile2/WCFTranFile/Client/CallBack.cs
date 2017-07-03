using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Contract;

namespace Client
{
    delegate void DownFile(CustomFileInfo currentDownFile);
    class CallBack : ICallBack
    {
        #region ICallBack 成员

        DownFile down = null;
        public CallBack(DownFile down)
        {
            this.down = down;
        }
        static object obj = new object();
        public bool DownFile(CustomFileInfo currentDownFile)
        {
            lock (obj)
            {
                if (down != null)
                {
                    down.Invoke(currentDownFile);
                    return true;
                }
                return false;
            }
        }

        #endregion
    }
}
