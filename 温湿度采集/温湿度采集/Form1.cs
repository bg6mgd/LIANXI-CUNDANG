using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Threading;

namespace 温湿度采集
{
   

    
    public partial class Form1 : Form
    {
        private SerialPort comm = new SerialPort();
        private StringBuilder builder = new StringBuilder();//避免在事件处理方法中反复的创建，定义到外面。
        private long received_count = 0;//接收计数
        private long send_count = 0;//发送计数
        private bool Listening = false;//是否没有执行完invoke相关操作
        private bool Closing = false;//是否正在关闭串口，执行Application.DoEvents，并阻止再次invoke
        private List<byte> buffer = new List<byte>(8192);//默认分配1页内存，并始终限制不允许超过
        private byte[] binary_data_1 = new byte[31];//53 54 32 30 2E 30 30 32 31 2E 30 30 36 39 2E 38 30 

        double wend;
        double shid;
        public Form1()
        {
            InitializeComponent();
            InitChart();
            InitChart2();
            ConsoleEx.AllocConsole();
            
        }
        System.Windows.Forms.Timer chartTimer = new System.Windows.Forms.Timer();

        
        private void InitChart()
        {
            DateTime time = DateTime.Now;
            chartTimer.Interval = 1000;
           
            wendu.DoubleClick += wendu_DoubleClick;

            Series series = wendu.Series[0];
            series.ChartType = SeriesChartType.Spline;

            wendu.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            wendu.ChartAreas[0].AxisX.ScaleView.Size = 5;
            wendu.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            wendu.ChartAreas[0].AxisX.ScrollBar.Enabled = true;

            chartTimer.Start();

        }
        private void InitChart2()
        {
            DateTime time1 = DateTime.Now;
            chartTimer.Interval = 1000;

           // wendu.DoubleClick += wendu_DoubleClick;

            Series series = shidu.Series[0];
            series.ChartType = SeriesChartType.Spline;

            shidu.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm:ss";
            shidu.ChartAreas[0].AxisX.ScaleView.Size = 5;
            shidu.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            shidu.ChartAreas[0].AxisX.ScrollBar.Enabled = true;

            chartTimer.Start();

        }

        void wendu_DoubleClick(object sender, EventArgs e)
        {
            wendu.ChartAreas[0].AxisX.ScaleView.Size = 5;
            wendu.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            wendu.ChartAreas[0].AxisX.ScrollBar.Enabled = true;
            //throw new NotImplementedException();
        }

       


        private void Form1_Load(object sender, EventArgs e)
        {
            comm.PortName = "COM3";
            comm.BaudRate = 9600;
            comm.DataBits = 7;
            comm.Open();
            comm.NewLine = "\r\n";
            comm.RtsEnable = true;//根据实际情况吧。

            //添加事件注册
            comm.DataReceived += comm_DataReceived;
        }

        private void comm_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (Closing) return;//如果正在关闭，忽略操作，直接返回，尽快的完成串口监听线程的一次循环
            try
            {
                Listening = true;//设置标记，说明我已经开始处理数据，一会儿要使用系统UI的。
                int n = comm.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
                byte[] buf = new byte[n];//声明一个临时数组存储当前来的串口数据
                received_count += n;//增加接收计数
                comm.Read(buf, 0, n);//读取缓冲数据

                /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //<协议解析>
                int data_1_catched = 0;//缓存记录数据是否捕获到
                //1.缓存数据
                buffer.AddRange(buf);
                //2.完整性判断
                while (buffer.Count >= 17)//至少要包含头（2字节）+长度（1字节）+校验（1字节）
                {
                    //请不要担心使用>=，因为>=已经和>,<,=一样，是独立操作符，并不是解析成>和=2个符号
                    //2.1 查找数据头
                    if (buffer[0] == 0x53)
                    {
                        //2.2 探测缓存数据是否有一条数据的字节，如果不够，就不用费劲的做其他验证了
                        //前面已经限定了剩余长度>=4，那我们这里一定能访问到buffer[2]这个长度
                        int len = 17;//数据长度
                        //数据完整判断第一步，长度是否足够
                        //len是数据段长度,4个字节是while行注释的3部分长度
                        if (buffer.Count < len) break;//数据不够的时候什么都不做
                                                          //这里确保数据长度足够，数据头标志找到，我们开始计算校验
                                                          //2.3 校验数据，确认数据正确
                                                          //异或校验，逐个字节异或得到校验码
                        #region 校验和
                        //byte checksum = 0;
                        //for (int i = 0; i < len + 3; i++)//len+3表示校验之前的位置
                        //{
                        //    checksum ^= buffer[i];
                        //}
                        //if (checksum != buffer[len + 3]) //如果数据校验失败，丢弃这一包数据
                        //{
                        //    buffer.RemoveRange(0, len + 4);//从缓存中删除错误数据
                        //    continue;//继续下一次循环
                        //}
                        //至此，已经被找到了一条完整数据。我们将数据直接分析，或是缓存起来一起分析
                        //我们这里采用的办法是缓存一次，好处就是如果你某种原因，数据堆积在缓存buffer中
                        //已经很多了，那你需要循环的找到最后一组，只分析最新数据，过往数据你已经处理不及时
                        //了，就不要浪费更多时间了，这也是考虑到系统负载能够降低。
                        #endregion

                        buffer.CopyTo(0, binary_data_1, 0, 17);//复制一条完整数据到具体的数据缓存
                        data_1_catched = 1;
                        buffer.RemoveRange(0, 17);//正确分析一条数据，从缓存中移除数据。
                    }
                    else
                    {
                        //这里是很重要的，如果数据开始不是头，则删除数据
                        buffer.RemoveAt(0);
                    }
                }
                //分析数据
                if (data_1_catched == 1)
                {
                    foreach (byte str in binary_data_1)
                         Console.Write("{0} ", Convert.ToChar(str));
                    int h1 = binary_data_1[2]-48;
                    Console.WriteLine( h1);
                    int h2 = binary_data_1[3] - 48;
                    int h3 = binary_data_1[5] - 48;
                    int h4 = binary_data_1[6] - 48;
                    double h = h1 * 10 + h2 + h3 * 0.1 + h4 * 0.01;
                    int w1 = binary_data_1[7] - 48;
               
                    int w2 = binary_data_1[8] - 48;
                    int w3 = binary_data_1[10] - 48;
                    int w4 = binary_data_1[11] - 48;
                    double w = w1 * 10 + w2 + w3 * 0.1 + w4 * 0.01;
                    //Console.WriteLine("{0}   {1}  {2}  {3} {4}" ,h1,h2,h3,h4,h);
                    shid = h;
                    wend = w;
                    this.Invoke((EventHandler)(delegate {
                        Series series = shidu.Series[0];
                        series.Points.AddXY(DateTime.Now, shid);
                        shidu.ChartAreas[0].AxisX.ScaleView.Position = series.Points.Count - 5;
                        Series series1 = wendu.Series[0];
                        series1.Points.AddXY(DateTime.Now, wend);
                        wendu.ChartAreas[0].AxisX.ScaleView.Position = series.Points.Count - 5;
                    }));


                    //int to1 = (Convert.ToInt32(binary_data_1[22]) - 48);
                    //int to2 = (Convert.ToInt32(binary_data_1[23]) - 48);
                    //int t = t1 * 10 + t2;
                    //int h = h1 * 10 + h2;
                    //int to = to1 * 10 + to2;
                    //DateTime d = DateTime.Now;
                    //Console.Write(d);
                    #region 连接数据库
                    string constr = "data source=(local);initial catalog=ww;integrated security=true";
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        //con.Open();
                        string sql = string.Format("insert into hj values('{0}','{1}','{2}',getdate())", w, h, w+h);
                        using (SqlCommand cmd = new SqlCommand(sql, con))
                        {
                            con.Open();
                            int r = cmd.ExecuteNonQuery();
                            if (r > 0)
                            {
                                Console.Write("charuchengg");
                               // Thread.Sleep(5000);
                                Console.Clear();
                                //ConsoleEx.FreeConsole();


                            }
                            else
                            {
                                Console.Write("shibai");
                            }
                        }
                    }
                    #endregion

                    //int val = Convert.ToInt16((b1 << 8) + b2);//两个字节合并
                    //int b3 = Convert.ToInt16(binary_data_1[6]);
                    //int b4 = Convert.ToInt16(binary_data_1[7]);
                    //int b5 = Convert.ToInt16(binary_data_1[8]);
                    //int b6 = Convert.ToInt16(binary_data_1[9]);
                    //int b7 = Convert.ToInt16(binary_data_1[10]);
                    //int czbh1 = Convert.ToInt16(binary_data_1[11]);
                    //int czbh2 = Convert.ToInt16(binary_data_1[12]);
                    //int sd = Convert.ToInt32((czbh1 << 8) + czbh2);
                    //int jsd = Convert.ToInt32(binary_data_1[13]);
                    //Console.WriteLine("{0}年{1}月{2}日{3}时{4}分{5}秒，编号时{6}，是否超限{7}", Convert.ToString(val),
                    //    Convert.ToString(b3), Convert.ToString(b4), Convert.ToString(b5), Convert.ToString(b6),
                    //    Convert.ToString(b7), Convert.ToString(sd), Convert.ToString(jsd ));
                    //Console.WriteLine("H的值{0} }",  h);
                    //Console.WriteLine(b2);
                    //foreach (byte str in binary_data_1)
                    //    Console.Write("{0} ", Convert.ToChar(str));






                }

                //如果需要别的协议，只要扩展这个data_n_catched就可以了。往往我们协议多的情况下，还会包含数据编号，给来的数据进行
                //编号，协议优化后就是： 头+编号+长度+数据+校验
                //</协议解析>
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////

                builder.Clear();//清除字符串构造器的内容

                #region MyRegion
                //因为要访问ui资源，所以需要使用invoke方式同步ui。
                //this.Invoke((EventHandler)(delegate
                //{
                //    //判断是否是显示为16禁止
                //    if (checkBoxHexView.Checked)
                //    {
                //        //依次的拼接出16进制字符串
                //        foreach (byte b in buf)
                //        {
                //            builder.Append(b.ToString("X2") + " ");
                //        }
                //    }
                //    else
                //    {
                //        //直接按ASCII规则转换成字符串
                //        builder.Append(Encoding.ASCII.GetString(buf));
                //    }
                //    //追加的形式添加到文本框末端，并滚动到最后。
                //    this.txGet.AppendText(builder.ToString());
                //    //修改接收计数
                //    labelGetCount.Text = "Get:" + received_count.ToString();
                //}));

                #endregion

            }
            finally
            {
                Listening = false;//我用完了，ui可以关闭串口了。
                
            }
            
            
            //throw new NotImplementedException();
        }
    }
    public class ConsoleEx
    {
        /// <summary>
        /// 启动控制台
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();

        /// <summary>
        /// 释放控制台
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
    }

}
