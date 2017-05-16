using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Printing;
using System.Xml;
using NLog;

namespace SQWRZS
{
    public partial class frmQuery : Form
    {
        int rowsPerPage = 0, pageCount = 0, page_i = 0, pages = 0, totalCount;//每页打印行数，打印页数，已打印记录数，已打印页数，需打印总记录数

        string str_printSql;
        DataTable dt_print;

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private frmMain main = null;

        public frmQuery()
        {
            InitializeComponent();
        }

        public frmQuery(frmMain main)
        {
            InitializeComponent();
            this.main = main;
        }

        private void butt_canel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DataQuery_Load(object sender, EventArgs e)
        {
            defaultDate();
            //GetGraphics();
            combo_CXL.Text = ">";
            combo_ZS.Text = ">";
            combo_ZZ.Text = ">";

        }

        private void defaultDate()
        {
            dateTP_Start.Value = DateTime.Now.AddDays(-1);
            textB_SHour.Text = DateTime.Now.Hour.ToString();
            textB_SMinute.Text = DateTime.Now.Minute.ToString();
            textB_SSecond.Text = DateTime.Now.Second.ToString();
            textB_EHour.Text = DateTime.Now.Hour.ToString();
            textB_EMinute.Text = DateTime.Now.Minute.ToString();
            textB_ESecond.Text = DateTime.Now.Second.ToString();
        }

        private void clearText()
        {
            //defaultDate();
            text_CPH.Text = "";
            textB_CXL.Text = "";
            textB_ZS.Text = "";
            textB_ZZ.Text = "";
            textB_CD.Text = "";
            combo_CXL.Text = ">";
            combo_ZS.Text = ">";
            combo_ZZ.Text = ">";
            listV_DataQuery.Items.Clear();
            pict_Photo.Image = null;
            lab_CXSJ.Text = "";
            lab_WCX.Text = "";
            lab_CXZS.Text = "";
            lab_CXXY6.Text = "";
            lab_CXDY6.Text = "";
        }

        private void butt_reset_Click(object sender, EventArgs e)
        {
            defaultDate();
            clearText();
            combo_ZZ.Visible = true;
            combo_ZS.Visible = true;
            combo_CXL.Visible = true;
            textB_CXL.Enabled = true;
            text_CPH.Focus();
            str_printSql = "";
            listV_DataQuery.SelectedItems.Clear();
            listV_DataQuery.Items.Clear();
        }

        private void textB_ZS_KeyPress(object sender, KeyPressEventArgs e)
        {
            //判断按键是不是输入的类型
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8)
                e.Handled = true;

        }

        private void textB_ZZ_KeyPress(object sender, KeyPressEventArgs e)
        {
            //判断按键是不是输入的类型
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8)
                e.Handled = true;
        }

        private void textB_CXL_KeyPress(object sender, KeyPressEventArgs e)
        {
            //判断按键是不是输入的类型
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8)
                e.Handled = true;
        }

        private void textB_SHour_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8)
                e.Handled = true;
            lab_message.Text = "";
            if (textB_SHour.Text.Length == 1)
            {
                int i1;   //十位，即第一个输入值
                int i2;   //个位，即第二个输入值
                bool b1 = false, b2 = false;
                b1 = int.TryParse(textB_SHour.Text, out i1);
                b2 = int.TryParse(textB_SHour.Text + e.KeyChar.ToString(), out i2);

                if (i2 > 24)
                {
                    textB_SHour.Text = "";
                    lab_message.Text = "输入的起始时间中<<小时>>不能大于24，请重新输入！";
                    e.Handled = true;
                }
                else
                    lab_message.Text = "";
            }
        }
        /// <summary>
        /// 处理复制某个数值（个位或十位）或整个数值时，总体数值大于24的情况
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textB_SHour_TextChanged(object sender, EventArgs e)
        {
            if (textB_SHour.Text.Length == 2)
            {
                int i;
                int.TryParse(textB_SHour.Text, out i);
                if (i > 24)
                {
                    textB_SHour.Text = "";
                    lab_message.Text = "输入的起始时间中<<小时>>不能大于24，请重新输入！";
                }
                else
                    lab_message.Text = "";
            }
        }

        private void textB_SMinute_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8)
                e.Handled = true;
            lab_message.Text = "";
            if (textB_SMinute.Text.Length == 1)
            {
                int i1;   //十位，即第一个输入值
                int i2;   //个位，即第二个输入值
                bool b1 = false, b2 = false;
                b1 = int.TryParse(textB_SMinute.Text, out i1);
                b2 = int.TryParse(textB_SMinute.Text + e.KeyChar.ToString(), out i2);

                if (i2 > 60)
                {
                    textB_SMinute.Text = "";
                    lab_message.Text = "输入的起始时间中<<分钟>不能大于60，请重新输入！";
                    e.Handled = true;
                }
                else
                    lab_message.Text = "";
            }
        }

        private void textB_SMinute_TextChanged(object sender, EventArgs e)
        {
            if (textB_SMinute.Text.Length == 2)
            {
                int i;
                int.TryParse(textB_SMinute.Text, out i);
                if (i > 60)
                {
                    textB_SMinute.Text = "";
                    lab_message.Text = "输入的起始时间中<<分钟>>不能大于60，请重新输入！";
                }
                else
                    lab_message.Text = "";
            }
        }

        private void textB_SSecond_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8)
                e.Handled = true;
            lab_message.Text = "";
            if (textB_SSecond.Text.Length == 1)
            {
                int i1;   //十位，即第一个输入值
                int i2;   //个位，即第二个输入值
                bool b1 = false, b2 = false;
                b1 = int.TryParse(textB_SSecond.Text, out i1);
                b2 = int.TryParse(textB_SSecond.Text + e.KeyChar.ToString(), out i2);

                if (i2 > 60)
                {
                    textB_SSecond.Text = "";
                    lab_message.Text = "输入的起始时间中<<秒>>不能大于60，请重新输入！";
                    e.Handled = true;
                }
                else
                    lab_message.Text = "";
            }
        }

        private void textB_SSecond_TextChanged(object sender, EventArgs e)
        {
            if (textB_SSecond.Text.Length == 2)
            {
                int i;
                int.TryParse(textB_SSecond.Text, out i);
                if (i > 60)
                {
                    textB_SSecond.Text = "";
                    lab_message.Text = "输入的起始时间中<<秒>>不能大于60，请重新输入！";
                }
                else
                    lab_message.Text = "";
            }
        }

        private void textB_EHour_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8)
                e.Handled = true;
            lab_message.Text = "";
            if (textB_EHour.Text.Length == 1)
            {
                int i1;   //十位，即第一个输入值
                int i2;   //个位，即第二个输入值
                bool b1 = false, b2 = false;
                b1 = int.TryParse(textB_EHour.Text, out i1);
                b2 = int.TryParse(textB_EHour.Text + e.KeyChar.ToString(), out i2);

                if (i2 > 24)
                {
                    textB_EHour.Text = "";
                    lab_message.Text = "输入的截止时间中<<小时>不能大于24，请重新输入！";
                    e.Handled = true;
                }
                else
                    lab_message.Text = "";
            }
        }

        private void textB_EHour_TextChanged(object sender, EventArgs e)
        {
            if (textB_EHour.Text.Length == 2)
            {
                int i;
                int.TryParse(textB_EHour.Text, out i);
                if (i > 24)
                {
                    textB_EHour.Text = "";
                    lab_message.Text = "输入的截止时间中<<小时>>不能大于24，请重新输入！";
                }
                else
                    lab_message.Text = "";
            }
        }

        private void textB_EMinute_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8)
                e.Handled = true;
            lab_message.Text = "";
            if (textB_EMinute.Text.Length == 1)
            {
                int i1;   //十位，即第一个输入值
                int i2;   //个位，即第二个输入值
                bool b1 = false, b2 = false;
                b1 = int.TryParse(textB_EMinute.Text, out i1);
                b2 = int.TryParse(textB_EMinute.Text + e.KeyChar.ToString(), out i2);

                if (i2 > 60)
                {
                    textB_EMinute.Text = "";
                    lab_message.Text = "输入的截止时间中<<分钟>不能大于60，请重新输入！";
                    e.Handled = true;
                }
                else
                    lab_message.Text = "";
            }
        }

        private void textB_EMinute_TextChanged(object sender, EventArgs e)
        {
            if (textB_EMinute.Text.Length == 2)
            {
                int i;
                int.TryParse(textB_EMinute.Text, out i);
                if (i > 60)
                {
                    textB_EMinute.Text = "";
                    lab_message.Text = "输入的截止时间中<<分钟>>不能大于60，请重新输入！";
                }
                else
                    lab_message.Text = "";
            }
        }

        private void textB_ESecond_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8)
                e.Handled = true;
            lab_message.Text = "";
            if (textB_ESecond.Text.Length == 1)
            {
                int i1;   //十位，即第一个输入值
                int i2;   //个位，即第二个输入值
                bool b1 = false, b2 = false;
                b1 = int.TryParse(textB_ESecond.Text, out i1);
                b2 = int.TryParse(textB_ESecond.Text + e.KeyChar.ToString(), out i2);

                if (i2 > 60)
                {
                    textB_ESecond.Text = "";
                    lab_message.Text = "输入的截止时间中<<秒>>不能大于60，请重新输入！";
                    e.Handled = true;
                }
                else
                    lab_message.Text = "";
            }
        }

        private void textB_ESecond_TextChanged(object sender, EventArgs e)
        {
            if (textB_ESecond.Text.Length == 2)
            {
                int i;
                int.TryParse(textB_ESecond.Text, out i);
                if (i > 60)
                {
                    textB_ESecond.Text = "";
                    lab_message.Text = "输入的截止时间中<<秒>>不能大于60，请重新输入！";
                }
                else
                    lab_message.Text = "";
            }
        }

        private void butt_OK_Click(object sender, EventArgs e)
        {
            string str_CPH = text_CPH.Text.Trim();
            string str_ZS = textB_ZS.Text.Trim();
            string str_ZZ = textB_ZZ.Text.Trim();
            string str_CXL = textB_CXL.Text.Trim();
            string str_CD = textB_CD.Text.Trim();

            string str_StartTime = dateTP_Start.Value.ToShortDateString() + " " + textB_SHour.Text.Trim() + ":" + textB_SMinute.Text.Trim() + ":" + textB_SSecond.Text.Trim();
            string str_EndTime = dateTP_End.Value.ToShortDateString() + " " + textB_EHour.Text.Trim() + ":" + textB_EMinute.Text.Trim() + ":" + textB_ESecond.Text.Trim();
            string str_symbol_ZS = combo_ZS.Text;
            string str_symbol_ZZ = combo_ZZ.Text;
            string str_symbol_CXL = combo_CXL.Text;
            string str_sql;

            if (DateTime.Compare(Convert.ToDateTime(str_StartTime), Convert.ToDateTime(str_EndTime)) >= 0)
            {
                MessageBox.Show("起始时间晚于或等于截止日期，请重新选择！", "时间错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            listV_DataQuery.Items.Clear();
            if (str_CPH == "")
            {
                if (str_ZS == "" && str_ZZ == "" && str_CXL == "" && str_CD == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_ZS == "" && str_ZZ == "" && str_CXL == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where CD=" + str_CD + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_ZS == "" && str_ZZ == "" && str_CD == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where CXL" + str_symbol_CXL + str_CXL + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_CD == "" && str_ZZ == "" && str_CXL == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where ZS" + str_symbol_ZS + str_ZS + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_ZS == "" && str_CD == "" && str_CXL == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where ZZ" + str_symbol_ZZ + str_ZZ + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_ZS == "" && str_ZZ == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where CXL" + str_symbol_CXL + str_CXL + " and CD=" + str_CD + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_ZS == "" && str_CXL == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where ZZ" + str_symbol_ZZ + str_ZZ + " and CD=" + str_CD + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_ZS == "" && str_CD == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where ZZ" + str_symbol_ZZ + str_ZZ + " and CXL" + str_symbol_CXL + str_CXL + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_ZZ == "" && str_CXL == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where ZS" + str_symbol_ZS + str_ZS + " and CD=" + str_CD + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_ZZ == "" && str_CD == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where ZS" + str_symbol_ZS + str_ZS + " and CXL" + str_symbol_CXL + str_CXL + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_CXL == "" && str_CD == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where ZS" + str_symbol_ZS + str_ZS + " and ZZ" + str_symbol_ZZ + str_ZZ + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
                else if (str_ZS == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where CXL" + str_symbol_CXL + str_CXL + " and ZZ" + str_symbol_ZZ + str_ZZ + " and CD=" + str_CD + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' and ZZ" + str_symbol_ZZ + str_ZZ + " order by JCSJ";
                else if (str_ZZ == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where CXL" + str_symbol_CXL + str_CXL + " and ZS" + str_symbol_ZS + str_ZS + " and CD=" + str_CD + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' and ZS" + str_symbol_ZS + str_ZS + " order by JCSJ";
                else if (str_CXL == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where ZZ" + str_symbol_ZZ + str_ZZ + " and ZS" + str_symbol_ZS + str_ZS + " and CD=" + str_CD + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' and ZS" + str_symbol_ZS + str_ZS + " order by JCSJ";
                else if (str_CD == "")
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where ZZ" + str_symbol_ZZ + str_ZZ + " and ZS" + str_symbol_ZS + str_ZS + " and CXL" + str_symbol_CXL + str_CXL + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' and ZS" + str_symbol_ZS + str_ZS + " order by JCSJ";
                else
                    str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where CXL" + str_symbol_CXL + str_CXL + " and ZS" + str_symbol_ZS + str_ZS + " and CD=" + str_CD + " and ZZ" + str_symbol_ZZ + str_ZZ + " and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' and ZS" + str_symbol_ZS + str_ZS + " and ZZ" + str_symbol_ZZ + str_ZZ + " order by JCSJ";
            }
            else
                str_sql = "select ID,CPH,CPYS,ZZ,ZS,FX,CXL,JCSJ,CD,CPTX,CS from JZWRZS.dbo.ZH_ST_ZDCZB where CPH='" + str_CPH + "' and JCSJ>='" + str_StartTime + "' and JCSJ<='" + str_EndTime + "' order by JCSJ";
            if (str_sql == "")
                return;

            try
            {
                str_printSql = str_sql;

                DataTable dt = DataBase.DbHelperSQL.Query(str_sql).Tables[0];   //JZZD.BLL.SelectManager.GetDataTable(str_sql);
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("没有查询到符合条件的记录！", "查询结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                lab_CXZS.Text = dt.Rows.Count.ToString();
                int in_CXDY6 = 0;    //超限率大于6数量
                int in_CXXY6 = 0;    //超限率大于0小于6数量
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ListViewItem lvi = new ListViewItem(dt.Rows[i]["ID"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["CPH"].ToString());
                    if (dt.Rows[i]["CPYS"].ToString() == "")
                        lvi.SubItems.Add("无");
                    else
                        lvi.SubItems.Add(dt.Rows[i]["CPYS"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["ZZ"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["ZS"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["FX"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["CXL"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["JCSJ"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["CD"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["CS"].ToString());
                    lvi.SubItems.Add(dt.Rows[i]["CPTX"].ToString());

                    if (int.Parse(dt.Rows[i]["CXL"].ToString()) >= main.SysSet.MinCXBZ && int.Parse(dt.Rows[i]["CXL"].ToString()) <= main.SysSet.MaxCXBZ)
                    {
                        lvi.BackColor = Color.Red;
                        lvi.ForeColor = Color.Black;
                    }
                    else if (int.Parse(dt.Rows[i]["CXL"].ToString()) > 0)
                    {
                        lvi.BackColor = Color.Yellow;
                        lvi.ForeColor = Color.Black;
                    }
                    listV_DataQuery.Items.Add(lvi);

                    if (i == 0)
                    {
                        string str_imagePath = dt.Rows[0]["CPTX"].ToString();
                        if (str_imagePath != "" && File.Exists(str_imagePath))
                            pict_Photo.Image = Image.FromFile(str_imagePath);
                        else
                            pict_Photo.Image = null;
                    }

                    if (int.Parse(dt.Rows[i]["CXL"].ToString()) > 6)
                        in_CXDY6++;
                    else if (int.Parse(dt.Rows[i]["CXL"].ToString()) > 0)
                        in_CXXY6++;
                }
                lab_CXDY6.Text = in_CXDY6.ToString();
                lab_CXXY6.Text = in_CXXY6.ToString();
                lab_WCX.Text = (dt.Rows.Count - in_CXDY6 - in_CXXY6).ToString();
                lab_CXSJ.Text = DateTime.Now.ToString();
                dt.Clear();
                dt.Dispose();
            }
            catch (Exception ex)
            {
                //调试端点 
                logger.Debug("查询数据库失败！" + ex.ToString());
                MessageBox.Show("查询数据库失败！", "查询异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listV_DataQuery_Click(object sender, EventArgs e)
        {
            if (listV_DataQuery.SelectedItems.Count == 0)
                return;
            string str_imagePath = listV_DataQuery.SelectedItems[0].SubItems[10].Text.ToString();
            if (str_imagePath != "" && File.Exists(str_imagePath))
                pict_Photo.Image = Image.FromFile(str_imagePath);
            else
                pict_Photo.Image = null;
            text_CPH.Text = listV_DataQuery.SelectedItems[0].SubItems[1].Text.ToString();
            combo_CXL.Visible = false;
            combo_ZS.Visible = false;
            combo_ZZ.Visible = false;
            textB_CXL.Enabled = false;
            //textB_ZS.Text = listV_DataQuery.SelectedItems[0].SubItems[4].Text.ToString();
            textB_CXL.Text = listV_DataQuery.SelectedItems[0].SubItems[6].Text.ToString();
            //textB_ZZ.Text = listV_DataQuery.SelectedItems[0].SubItems[3].Text.ToString();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                int colCount = 0; //每个页面的列数
                int dataCount = 0;//导出数据的总数

                StringBuilder sbColTitle = new StringBuilder();
                string Content = string.Empty;
                StringBuilder sbContent = new StringBuilder();

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "文本文件(*.txt)|*.txt|电子表格文件(*.xls)|*.xls";
                sfd.FilterIndex = 2;
                sfd.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                sfd.Title = "导出查询数据到";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StreamWriter textFile = new StreamWriter(sfd.FileName, false, Encoding.Default);
                    colCount = listV_DataQuery.Columns.Count;

                    for (int i = 0; i < colCount; i++)
                    {
                        if (listV_DataQuery.Columns[i].Width > 5)
                        {
                            sbColTitle.Append(listV_DataQuery.Columns[i].Text.ToString() + "\t");
                        }
                    }

                    foreach (ListViewItem lvi in listV_DataQuery.Items) //遍历所有项
                    {
                        for (int i = 0; i < colCount; i++)
                        {
                            if (listV_DataQuery.Columns[i].Width > 5)
                            {
                                sbContent.Append(lvi.SubItems[i].Text + "\t");
                            }
                        }
                        sbContent.Append("\r\n");
                        dataCount++;
                    }

                    textFile.Write(sbColTitle.ToString() + "\r\n" + sbContent.ToString());
                    textFile.Close();
                    MessageBox.Show("导出完成！共导出" + dataCount + "条数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    //System.Threading.ParameterizedThreadStart ParStart = new System.Threading.ParameterizedThreadStart(exportData);
                    //System.Threading.Thread myThread = new System.Threading.Thread(ParStart);
                    //object o = sfd.FileName;
                    //myThread.Start(o);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出数据失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pict_Photo_DoubleClick(object sender, EventArgs e)
        {
            frmPic formPic = new frmPic();
            try
            {
                string str_imagePath = listV_DataQuery.SelectedItems[0].SubItems[10].Text.ToString();
                if (str_imagePath != "" && File.Exists(str_imagePath))
                {
                    formPic.pictureBox1.Image = Image.FromFile(str_imagePath);
                    formPic.Show();
                }
                else
                    formPic.pictureBox1.Image = null;

            }
            catch (Exception)
            {
            }

        }

    }
}
