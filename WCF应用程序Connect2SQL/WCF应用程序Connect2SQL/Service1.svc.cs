using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace WCF应用程序Connect2SQL
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“Service1”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 Service1.svc 或 Service1.svc.cs，然后开始调试。
    public class Service1 : IService1
    {
        SqlConnection strCon = new SqlConnection("server = (local); database=test;uid=sa;pwd=sa");
      public void openSql()
        {
            strCon.Open();
        }
        public void closeSql()
        {
            strCon.Close();
        }
        public DataSet querySql()
        {
            try
            {
                openSql();
                string strSql = "SELECT TNAME,TINTRO FROM TEST1";
                DataSet ds = new DataSet();
                SqlDataAdapter s = new SqlDataAdapter(strSql, strCon);
                s.Fill(ds);
                return ds;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                closeSql();
            }
        }
        
    }
}
