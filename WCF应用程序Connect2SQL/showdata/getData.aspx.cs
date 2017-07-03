using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
//using showdata;

namespace ShowData
{
    public partial class getData : System.Web.UI.Page
    {

        showdata.host.Service1Client sql = new showdata.host.Service1Client();

        protected void Page_Load(object sender, EventArgs e)
        {
            showData.DataSource = sql.querySql();
            showData.DataBind();
        }
    }
}