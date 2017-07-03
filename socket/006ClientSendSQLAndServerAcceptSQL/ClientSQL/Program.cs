using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//数据库引用
using System.Data;
using System.Data.OleDb;

//Socket引用
using System.Net;
using System.Net.Sockets;

namespace ClientSQL
{
    class Program
    {
        /// <summary>
        /// 数据库连接
        /// </summary>
        /// <returns>数据库连接对象</returns>
        public static OleDbConnection getConn() 
        {
            string connstr = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=G:\\users\\brizer\\documents\\visual studio 2010\\Projects\\socket\\test.mdb";
            OleDbConnection tempconn = new OleDbConnection(connstr);
            return tempconn;
        }
        /// <summary>
        /// 获取需要的信息
        /// </summary>
        /// <param name="id">通过参数查询</param>
        /// <returns>得到查询结果的字节流</returns>
        public static string getValueFromId(string id)
        {
            string tempValue = "";//定义返回值
            try
            {
                OleDbConnection conn = getConn();//得到连接对象
                string strCom = "Select * from testTable where id =" + id;
                OleDbCommand myCommand = new OleDbCommand(strCom, conn);
                conn.Open();
                OleDbDataReader reader;
                reader = myCommand.ExecuteReader();//执行命令并得到相应的DataReader
                //下面将得到的值赋给tempValue对象
                if (reader.Read())
                {
                    tempValue = "ID:" + reader["id"].ToString();
                    tempValue += "温度:" + reader["温度"].ToString();
                    tempValue += "长度:" + reader["长度"].ToString();
                }
                else
                {
                    tempValue = "没有该记录";
                }
            }
            catch (Exception e)
            {
 
            }
            return tempValue;
        }
        /// <summary>
        /// 将字节流传给服务端
        /// </summary>
        /// <param name="value">需要传送的内容</param>
        public static void sendToServer(string value)
        {
            const int BufferSize = 8192;//缓存大小，8192字节，可以保存4096个汉字和英文字符
             Console.WriteLine("Client Running...");
            TcpClient client = new TcpClient();
            try
            {
                client.Connect("localhost", 8500);//与服务器连接
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            //打印连接到的服务器信息
            Console.WriteLine("Server Connected! {0}-->{1}", client.Client.LocalEndPoint, client.Client.RemoteEndPoint);

            //定义要传送的信息
            string msg = value;
            NetworkStream streamToServer = client.GetStream();

            //发送信息
            byte[] buffer = Encoding.Unicode.GetBytes(msg);//获得缓存
            streamToServer.Write(buffer, 0, buffer.Length);//发往服务器
            Console.WriteLine("Sent:{0}", msg);
            
            //接收信息
            do
            {
                buffer = new byte[BufferSize];
                int bytesRead = streamToServer.Read(buffer, 0, BufferSize);//一直等待客户端传信息
                Console.WriteLine("Reading data,{0} bytes...", bytesRead);

                //获得请求的字符串
                msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received:{0}", msg);
                string result = getValueFromId(msg);
                Console.WriteLine("结果:{0}", result);

                //将结果发送给服务端
                msg = result;
                byte[] bufferWrite = Encoding.Unicode.GetBytes(result);
                streamToServer.Write(bufferWrite, 0, bufferWrite.Length);//发往服务器

            } while (true);

            //按Q退出
            Console.WriteLine("\n\n 输入 \"Q\"键退出。");
            ConsoleKey key;
            do
            {
                key = Console.ReadKey(true).Key;
            } while (key != ConsoleKey.Q);
        }


        static void Main(string[] args)
        {

            //获取需要的数据信息
            string value;
            value = getValueFromId("2");
            //Console.WriteLine(value);
            //Console.ReadKey();
            
            //通过TCP传到服务端
            sendToServer(value);
        }
    }
}
