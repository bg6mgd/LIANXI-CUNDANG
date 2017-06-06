using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
namespace Maticsoft.Web.test.Table_1
{
    public partial class Show : Page
    {        
        		public string strid=""; 
		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				#warning 代码生成提示：显示页面,请检查确认该语句是否正确
				ShowInfo();
			}
		}
		
	private void ShowInfo()
	{
		Maticsoft.BLL.test.Table_1 bll=new Maticsoft.BLL.test.Table_1();
		Maticsoft.Model.test.Table_1 model=bll.GetModel();
		this.lblname.Text=model.name;
		this.lblnianling.Text=model.nianling;
		this.lblshenggao.Text=model.shenggao;
		this.lblbianhao.Text=model.bianhao;

	}


    }
}
