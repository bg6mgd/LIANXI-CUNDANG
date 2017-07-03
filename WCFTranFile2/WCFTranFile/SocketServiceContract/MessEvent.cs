using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketServiceContract
{


    public class MessBuffer : System.EventArgs
    {
        /// <summary>
        /// 获取数据发送接收完成时间
        /// </summary>
        public string MessTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        /// <summary>
        /// 数据发送或接收的客户端SOCKET
        /// </summary>
        public System.Net.Sockets.Socket Client;
        /// <summary>
        /// 发送或接收到的数据包
        /// </summary>
        public byte[] ReviceBuffer;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ReBuffer">发送或接收到的数据包</param>
        /// <param name="BuffClient">数据发送或接收的客户端SOCKET</param>
        public MessBuffer(byte[] ReBuffer, System.Net.Sockets.Socket BuffClient)
        {
            this.ReviceBuffer = ReBuffer;
            this.Client = BuffClient;
        }
    }
    public delegate void MessBufferHandle(object sender, MessBuffer e);
    public class ReviceBuffer : System.EventArgs
    {
        /// <summary>
        /// 获取数据发送接收完成时间
        /// </summary>
        public string MessTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        /// <summary>
        /// 数据发送或接收的客户端SOCKET
        /// </summary>
        public System.Net.Sockets.Socket Client;
        /// <summary>
        /// 发送或接收到的数据包
        /// </summary>
        public byte[] ReviceByte;
        /// <summary>
        /// SOCKET错误
        /// </summary>
        public System.Net.Sockets.SocketError Error;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="ReBuffer">发送或接收到的数据包</param>
        /// <param name="BuffClient">数据发送或接收的客户端SOCKET</param>
        public ReviceBuffer(byte[] ReBuffer, System.Net.Sockets.Socket BuffClient, System.Net.Sockets.SocketError SocketErr)
        {
            this.ReviceByte = ReBuffer;
            this.Client = BuffClient;
            this.Error = SocketErr;
        }
    }
    public delegate void ReviceBufferHandle(object sender, ReviceBuffer e);

}
