using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Printing;
using System.Drawing;
using System.Windows.Forms;

namespace 打印2
{
    class class1
    {
        private System.Drawing.Printing.PrintDocument docToPrint =
            new System.Drawing.Printing.PrintDocument(); //打印对象

        /*打印*/
        public void print()
        {
            docToPrint.PrintPage += new PrintPageEventHandler(this.document_PrintPage);
            docToPrint.Print();//打印
           
        }

        private void document_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Pen blackPen = new Pen(Color.Black, 1);//黑色，1像素
            //打印样式
            System.Drawing.Font f = new System.Drawing.Font("黑体", 10, System.Drawing.FontStyle.Regular);
            System.Drawing.Font TableFont = new Font("宋体", 10, FontStyle.Regular);
            System.Drawing.Font boldFont = new System.Drawing.Font("黑体", 12, System.Drawing.FontStyle.Bold);
            System.Drawing.Font msgf = new System.Drawing.Font("黑体", 14, System.Drawing.FontStyle.Regular);

            string[,] strArr = new string[2, 6];  //声明一个二维数组.
            strArr[0, 0] = "类别";
            strArr[0, 1] = "老师";
            strArr[0, 2] = "工作单位";
            strArr[0, 3] = "信息大学";
            strArr[0, 4] = "收费标准";
            strArr[0, 5] = 12 + "元";

            strArr[1, 0] = "姓名";
            strArr[1, 1] = "陈小莉";
            strArr[1, 2] = "性别";
            strArr[1, 3] = "女";
            strArr[1, 4] = "年龄";
            strArr[1, 5] = "" + 20;

            int docX = 90;
            int docY = 50;
            int tableW = 600;//表宽
            int tableH = 2 * 50;//表高
            int tableX = docX;//表的起点X坐标
            int tableY = docY;//表的起点Y坐标
            int rowWidth = 50;//行宽
            int colwidth = tableW / 6;//列宽
            for (int i = 0; i <= 2; i++)
            {
                e.Graphics.DrawLine(blackPen, tableX, tableY + i * rowWidth, tableX + tableW, tableY + i * rowWidth);
                for (int j = 0; j <= 6; j++)
                {
                    if (i != 2)
                    {
                        e.Graphics.DrawLine(blackPen,
                        tableX + j * colwidth, tableY + i * rowWidth,
                        tableX + j * colwidth, tableY + i * rowWidth + rowWidth);
                        if (j != 6)
                        {
                            if (j % 2 == 0)
                            {
                                int textWidth = Convert.ToInt32(e.Graphics.MeasureString(strArr[i, j], boldFont).Width);
                                int textHeight = Convert.ToInt32(e.Graphics.MeasureString(strArr[i, j], boldFont).Height);
                                int centerX = (colwidth - textWidth) / 2; //字体X轴居中
                                int centerY = (rowWidth - textHeight) / 2; //字体Y轴居中
                                e.Graphics.DrawString(strArr[i, j], boldFont, Brushes.Black,
                                    tableX + j * colwidth + centerX, tableY + i * rowWidth + centerY);//写列名
                            }
                            else
                            {

                                e.Graphics.DrawString(strArr[i, j], TableFont, Brushes.Black,
                                tableX + j * colwidth + 15, tableY + i * rowWidth + 15);//写列名
                            }
                        }
                    }
                }
            }
        }
        /*打印预览*/
        public void PrintPriview()
        {
            try
            {
                PrintPreviewDialog PrintPriview = new PrintPreviewDialog();
                PrintPriview.Document = CreatePrintDocument();
                PrintPriview.WindowState = FormWindowState.Maximized;
                PrintPriview.ShowDialog();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }
        /**创建打印文档*/
        private PrintDocument CreatePrintDocument()
        {
            docToPrint.PrintPage += new PrintPageEventHandler(this.document_PrintPage);
            return docToPrint;
        }
    }
}

