using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace SQWRZS
{
    public partial class frmDataShow : Form
    {
        private frmMain main = null;
        /// <summary>
        /// 两个称板之间的时刻差不大于总时刻的一半，及FFFFFF/2=0x7FFFFF，即8388607,大于该值的话认为前一个时刻被清零
        /// </summary>
        private int timeInterval = 8388607;

        public frmDataShow()
        {
            InitializeComponent();
        }

        public frmDataShow(frmMain main)
        {
            InitializeComponent();
            this.main = main;
        }

        private void btnCBZZ_Click(object sender, EventArgs e)
        {
            if (dgvData.SelectedRows.Count != 2)
            {
                MessageBox.Show("选择需要计算时刻差的两行数据");
                return;
            }
            //for (int i = 0; i < dgvData.SelectedRows.Count; i++)
            //{
            //    if (dgvData.SelectedRows[i].Cells["ColDataType"].Value.ToString() == "线圈")
            //    {
            //        MessageBox.Show("选择称板数据，不要选择线圈信号数据");
            //        return;
            //    }
            //}


            int time1 = Convert.ToInt32(dgvData.SelectedRows[0].Cells["ColSK"].Value);
            int time2 = Convert.ToInt32(dgvData.SelectedRows[1].Cells["ColSK"].Value);
            int result = Math.Abs(time2 - time1);

            if (result > timeInterval)
            {//判断时刻差的范围
                if (time1 > time2)
                {
                    result = (0xFFFFFF - time1) + time2;
                }
                else
                {
                    result = (0xFFFFFF - time2) + time1;
                }
            }

            dgvData.SelectedRows[0].Cells["ColSKC"].Value = result;
            dgvData.SelectedRows[1].Cells["ColSKC"].Value = result;

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            DataBase.DALCBData dalCBData = new DataBase.DALCBData();

            DataSet ds = dalCBData.GetList("");

            ExportExcel(ds.Tables[0], "D:\\CBData.xls");
        }

        private void btnSaveToDB_Click(object sender, EventArgs e)
        {
            StringBuilder sbTDH = new StringBuilder();
            StringBuilder sbJLZ = new StringBuilder();
            StringBuilder sbSK = new StringBuilder();
            StringBuilder sbSKC = new StringBuilder();
            StringBuilder sbCKZ = new StringBuilder();
            StringBuilder sbYSSJ = new StringBuilder();
            StringBuilder sbDT = new StringBuilder();

            for (int i = 0; i < dgvData.Rows.Count - 1; i++)
            {
                if (dgvData.Rows[i].Cells["ColDataType"].Value.ToString() != "线圈")
                {
                    //for (int j = 1; j < dgvData.Columns.Count; j++)
                    //{
                    sbTDH.Append(dgvData.Rows[i].Cells["ColTDH"].Value + ",");
                    sbJLZ.Append(dgvData.Rows[i].Cells["ColJLZ"].Value + ",");
                    sbSK.Append(dgvData.Rows[i].Cells["ColSK"].Value + ",");
                    sbSKC.Append(dgvData.Rows[i].Cells["ColSKC"].Value + ",");
                    sbCKZ.Append(dgvData.Rows[i].Cells["ColCKZ"].Value + ",");
                    sbDT.Append(dgvData.Rows[i].Cells["ColRecvTime"].Value + ",");
                    sbYSSJ.Append(dgvData.Rows[i].Cells["ColRecvCode"].Value + ",");
                    // }
                }
            }

            DataBase.DALCBData dalCBData = new DataBase.DALCBData();
            DataBase.CBDataTable tabCBData = new DataBase.CBDataTable();

            tabCBData.TDH = sbTDH.ToString();
            tabCBData.JLZ = sbJLZ.ToString();
            tabCBData.SK = sbSK.ToString();
            tabCBData.SKC = sbSKC.ToString();
            tabCBData.CKZ = sbCKZ.ToString();
            tabCBData.JSSJ = sbDT.ToString();
            tabCBData.YSSJ = sbYSSJ.ToString();

            if (dalCBData.Add(tabCBData) > 0)
            {
                MessageBox.Show("保存成功");

                dgvData.Rows.Clear();
            }
            else
            {
                MessageBox.Show("保存失败");
            }

        }

        /// <summary>
        /// 将DataTable中的数据导出到Excel（支持Excel2003和Excel2007）
        /// </summary>
        /// <param name="dt"> DataTable</param>
        /// <param name="url">Excel保存的路径DataTable</param>
        /// <returns>导出成功返回True，否则返回false</ returns >
        public bool ExportExcel(System.Data.DataTable dt, string url)
        {
            bool flag = false;
            //Microsoft.Office.Interop.Excel.Application objExcel = null;
            //Workbook objWorkbook = null;
            //Worksheet objsheet = null;
            try
            {
            //    //申明对象
            //    objExcel = new Microsoft.Office.Interop.Excel.Application();
            //    objWorkbook = objExcel.Workbooks.Add(Missing.Value);
            //    objsheet = (Worksheet)objWorkbook.ActiveSheet;

            //    //设置Excel不可见
            //    objExcel.Visible = false;
            //    objExcel.DisplayAlerts = false;

            //    //设置Excel字段类型全部为字符串
            //    objsheet.Cells.NumberFormat = "@";

            //    //向Excel中写入表格的标头
            //    int displayColumnsCount = 1;
            //    for (int i = 0; i < dt.Columns.Count; i++)
            //    {
            //        objExcel.Cells[1, displayColumnsCount] = dt.Columns[i].ColumnName.Trim();
            //        displayColumnsCount++;
            //    }
            //    //向Excel中逐行逐列写入表格中的数据
            //    for (int row = 0; row < dt.Rows.Count; row++)
            //    {
            //        displayColumnsCount = 1;
            //        for (int col = 0; col < dt.Columns.Count; col++)
            //        {
            //            try
            //            {

            //                objExcel.Cells[row + 2, displayColumnsCount] = dt.Rows[row][col].ToString().Trim();
            //                displayColumnsCount++;

            //            }
            //            catch (Exception ex)
            //            {

            //            }
            //        }
            //    }
            //    //保存文件
            //    objWorkbook.SaveAs(url, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value,
            //                        Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlShared, Missing.Value,
            //                        Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                flag = true;
            //    MessageBox.Show("导出完成,位置为D:\\CBData.xls", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                flag = false;
                //MessageBox.Show(ex.Message, "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                ////关闭Excel应用
                //if (objWorkbook != null) objWorkbook.Close(Missing.Value, Missing.Value, Missing.Value);
                //if (objExcel.Workbooks != null) objExcel.Workbooks.Close();
                //if (objExcel != null) objExcel.Quit();

                ////杀死进程
                //KillProcess("Excel");
                //objsheet = null;
                //objWorkbook = null;
                //objExcel = null;
            }
            return flag;
        }


        /// <summary>
        /// 根据进程名称杀死进程 
        /// </summary>
        /// <param name=" ProcessName "> DataTable</param>
        public void KillProcess(string ProcessName)
        {
            System.Diagnostics.Process myproc = new System.Diagnostics.Process();
            try
            {
                foreach (System.Diagnostics.Process thisproc in System.Diagnostics.Process.GetProcessesByName(ProcessName))
                {
                    if (!thisproc.CloseMainWindow())
                    {
                        thisproc.Kill();
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception("", ex);
            }
        }
        /*
         * C# DataGridView添加新行的2个方法
         * 方法一：
         * 代码如下:

            int index=this.dataGridView1.Rows.Add();
            this.dataGridView1.Rows[index].Cells[0].Value = "1"; 
            this.dataGridView1.Rows[index].Cells[1].Value = "2"; 
            this.dataGridView1.Rows[index].Cells[2].Value = "监听";

            利用dataGridView1.Rows.Add()事件为DataGridView控件增加新的行，
         * 该函数返回添加新行的索引号，即新行的行号，然后可以
         * 通过该索引号操作该行的各个单元格，
         * 如dataGridView1.Rows[index].Cells[0].Value = "1"。
         * 这是很常用也是很简单的方法。
         * 
         * 方法二：
            复制代码 代码如下:

            DataGridViewRow row = new DataGridViewRow();
            DataGridViewTextBoxCell textboxcell = new DataGridViewTextBoxCell();
            textboxcell.Value = "aaa";
            row.Cells.Add(textboxcell);
            DataGridViewComboBoxCell comboxcell = new DataGridViewComboBoxCell();
            row.Cells.Add(comboxcell);
            dataGridView1.Rows.Add(row);
            方法二比方法一要复杂一些，但是在一些特殊场合非常实用，例如，要在新行中的某些单元格添加下拉框、按钮之类的控件时，该方法很有帮助。
            DataGridViewRow row = new DataGridViewRow(); 是创建DataGridView的行对象，DataGridViewTextBoxCell是单元格的内容是个 TextBox，DataGridViewComboBoxCell是单元格的内容是下拉列表框，同理可知，DataGridViewButtonCell是单元格的内容是个按钮，等等。textboxcell是新创建的单元格的对象，可以为该对象添加其属性。然后通过row.Cells.Add(textboxcell)为row对象添加textboxcell单元格。要添加其他的单元格，用同样的方法即可。
            最后通过dataGridView1.Rows.Add(row)为dataGridView1控件添加新的行row。
         */

    }
}
