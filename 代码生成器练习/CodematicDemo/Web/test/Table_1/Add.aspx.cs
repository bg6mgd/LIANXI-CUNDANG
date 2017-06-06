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
using Maticsoft.Common;
using LTP.Accounts.Bus;
namespace Maticsoft.Web.test.Table_1
{
    public partial class Add : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
                       
        }

        		protected void btnSave_Click(object sender, EventArgs e)
		{
			
			string strErr="";
			if(this.txtname.Text.Trim().Length==0)
			{
				strErr+="name不能为空！\\n";	
			}
			if(this.txtnianling.Text.Trim().Length==0)
			{
				strErr+="nianling不能为空！\\n";	
			}
			if(this.txtshenggao.Text.Trim().Length==0)
			{
				strErr+="shenggao不能为空！\\n";	
			}
			if(this.txtbianhao.Text.Trim().Length==0)
			{
				strErr+="bianhao不能为空！\\n";	
			}

			if(strErr!="")
			{
				MessageBox.Show(this,strErr);
				return;
			}
			string name=this.txtname.Text;
			string nianling=this.txtnianling.Text;
			string shenggao=this.txtshenggao.Text;
			string bianhao=this.txtbianhao.Text;

			Maticsoft.Model.test.Table_1 model=new Maticsoft.Model.test.Table_1();
			model.name=name;
			model.nianling=nianling;
			model.shenggao=shenggao;
			model.bianhao=bianhao;

			Maticsoft.BLL.test.Table_1 bll=new Maticsoft.BLL.test.Table_1();
			bll.Add(model);
			Maticsoft.Common.MessageBox.ShowAndRedirect(this,"保存成功！","add.aspx");

		}


        public void btnCancle_Click(object sender, EventArgs e)
        {
            Response.Redirect("list.aspx");
        }
    }
}
