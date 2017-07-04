using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace arduino两路输出两路输入客户端
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
        private byte[] binary_data_1 = new byte[31];//AA 44 05 01 02 03 04 05 EA
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonTrueFalse()
        {
            if (comm.IsOpen)
            {
                button1.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;

            }
            else
            {
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ConsoleEx.AllocConsole(); //打开控制台
            //初始化下拉串口名称列表框
            string[] ports = SerialPort.GetPortNames();
            Array.Sort(ports);
            comserial.Items.AddRange(ports);
            comserial.SelectedIndex = comserial.Items.Count > 0 ? 0 : -1;
            combtl.SelectedIndex = combtl.Items.IndexOf("9600");

            //初始化SerialPort对象
            comm.NewLine = "\r\n";
            comm.RtsEnable = true;//根据实际情况吧。
            buttonTrueFalse();
            //添加事件注册
            comm.DataReceived += comm_DataReceived;
        }
        private void comm_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("1串口收到数据");
            //if (Closing) return;//如果正在关闭，忽略操作，直接返回，尽快的完成串口监听线程的一次循环
            try
            {
                Listening = true;//设置标记，说明我已经开始处理数据，一会儿要使用系统UI的。
                int n = comm.BytesToRead;//先记录下来，避免某种原因，人为的原因，操作几次之间时间长，缓存不一致
                byte[] buf = new byte[n];//声明一个临时数组存储当前来的串口数据
                received_count += n;//增加接收计数
                comm.Read(buf, 0, n);//读取缓冲数据
                Console.WriteLine(BitConverter.ToString(buf));//byte[]转换为string字符串
                this.Invoke((EventHandler)delegate
                {
                    textBox1.Text = BitConverter.ToString(buf);
                });//不能直接访问ui线程。
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //<协议解析>
                int data_1_catched = 0;//缓存记录数据是否捕获到
                //1.缓存数据
                buffer.AddRange(buf);
                //2.完整性判断
                while (buffer.Count >= 7)//至少要包含头（2字节）+长度（1字节）+校验（1字节）
                {
                    Console.WriteLine("数据完整");
                    //请不要担心使用>=，因为>=已经和>,<,=一样，是独立操作符，并不是解析成>和=2个符号
                    //2.1 查找数据头
                    if (buffer[5] == 0x0D&&buffer[6] == 0x0A)
                    {
                        Console.WriteLine("数据头正确");
                        //2.2 探测缓存数据是否有一条数据的字节，如果不够，就不用费劲的做其他验证了
                        //前面已经限定了剩余长度>=4，那我们这里一定能访问到buffer[2]这个长度
                        int len = 4;//数据长度
                        //数据完整判断第一步，长度是否足够
                        //len是数据段长度,4个字节是while行注释的3部分长度
                        if (buffer.Count < len + 3) break;//数据不够的时候什么都不做
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

                        buffer.CopyTo(0, binary_data_1, 0, 7);//复制一条完整数据到具体的数据缓存
                        data_1_catched = 1;
                        buffer.RemoveRange(0, 7);//正确分析一条数据，从缓存中移除数据。
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
                   
                    Console.WriteLine("可以解析了");
                   
                    ASCIIEncoding encoding = new ASCIIEncoding();//byte数组转为ascii码
                    string strTemp = encoding.GetString(binary_data_1, 0, 7);
                    Console.WriteLine(strTemp );
                    
                    switch (binary_data_1[4])
                    {
                        case 0x41:
                            this.Invoke((EventHandler)delegate
                            {
                                a0lab.Text = strTemp;
                            });//不能直接访问ui线程。
                            break;
                        case 0x42:
                            this.Invoke((EventHandler)delegate{
                                a1lab.Text = strTemp;
                            });//
                            break;
                        case 0x35:
                            this.Invoke((EventHandler)delegate {
                                d1lab.Text = strTemp;
                            });//
                            break;
                        case 0x34:
                            this.Invoke((EventHandler)delegate {
                                d0lab.Text = strTemp;
                            });//
                            break;
                        default:
                            break;
                    }
                   
                    
                   




                   
                    
                }
                
              
               
                builder.Clear();//清除字符串构造器的内容
                
            }
            finally
            {
                Listening = false;//我用完了，ui可以关闭串口了。
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (comm.IsOpen)
            {
                Closing = true;
                while (Listening) Application.DoEvents();
                //打开时点击，则关闭串口
                comm.Close();
            }
            else
            {
                //关闭时点击，则设置好端口，波特率后打开
                comm.PortName = comserial.Text;
                comm.BaudRate = int.Parse(combtl.Text);
                try
                {
                    comm.Open();
                }
                catch (Exception ex)
                {
                    //捕获到异常信息，创建一个新的comm对象，之前的不能用了。
                    comm = new SerialPort();
                    //现实异常信息给客户。
                    MessageBox.Show(ex.Message);
                }
            }
            //设置按钮的状态
            button5.Text = comm.IsOpen ? "Close" : "Open";
            buttonTrueFalse();
            if (comm.IsOpen)
            {
                comserial.Enabled = false;
                combtl.Enabled = false;
            }
            else
            {
                a0lab.Text = null;
                a1lab.Text = null;
                d0lab.Text = null;
                d1lab.Text = null;
                comserial.Enabled = true;
                combtl.Enabled = true;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            // List<byte> buf = new List<byte>();//填充到这个临时列表中
            //string str = "55 AA 10";
            byte[] byt = new byte[3];
            byt[0] = 0x55;
            byt[1] = 0xAA;
            byt[2] = 0x10;

            // char[] a = str.ToArray();
            //转换列表为数组后发送
            comm.Write(byt,0,3);
            //记录发送的字节数
            Console.Write(byt);
            }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] byt = new byte[3];
            byt[0] = 0x55;
            byt[1] = 0xAA;
            byt[2] = 0x11;

            // char[] a = str.ToArray();
            //转换列表为数组后发送
            comm.Write(byt, 0, 3);
            //记录发送的字节数
            Console.Write(byt);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] byt = new byte[3];
            byt[0] = 0x55;
            byt[1] = 0xAA;
            byt[2] = 0x20;

            // char[] a = str.ToArray();
            //转换列表为数组后发送
            comm.Write(byt, 0, 3);
            //记录发送的字节数
            Console.Write(byt);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] byt = new byte[3];
            byt[0] = 0x55;
            byt[1] = 0xAA;
            byt[2] = 0x21;

            // char[] a = str.ToArray();
            //转换列表为数组后发送
            comm.Write(byt, 0, 3);
            //记录发送的字节数
            Console.Write(byt);
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
