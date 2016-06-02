using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

namespace DataTransfer
{
    public class StatusArgs : EventArgs
    {
        string ip;

        public string IP
        {
            get { return ip; }
        }

        int status = 1;

        public int Status
        {
            get { return status; }
        }

        public StatusArgs(string endPoint, int status)
        {
            string[] ipport = endPoint.Split(':');
            if (ipport.Length == 2)
            {
                this.ip = ipport[0];
            }
            else
            {
                this.ip = endPoint;
            }

            this.status = status;
        }
    }
    public class Server
    {
        private Thread threadWatch;//负责监听客户端连接请求的线程
        //用来保存客户端连接套接字
        private Dictionary<string, Socket> dict = new Dictionary<string, Socket>();
        /// <summary>
        /// 在线的客户端列表
        /// </summary>
        public Dictionary<string, Socket> Clients
        {
            get { return dict; }
        }
        private Socket socketWatch;//负责监听的套接字
        private volatile bool listen;

        public bool IsListening
        {
            get { return listen; }
            set { listen = value; }
        }
        public static string HeartBeat = Common.HeartBeat;
        public static string LeaveMsg = Common.LeaveMsg;
        public static string ExitMsg = Common.ExitMsg;
        public static string RunMsg = Common.RunMsg;
        public static string StopMsg = Common.StopMsg;
        public static int BufferSize = Common.BufferSize;
        public static int ReceiveTimeout = Common.ReceiveTimeout;
        IPEndPoint server;
        public event EventHandler<StatusArgs> RefreshStatus;

        private void OnRefreshStatus(StatusArgs e)
        {
            if (RefreshStatus != null)
                RefreshStatus(this, e);
        }

        private void StopListen()
        {
            try
            {
                listen = false;
                CloseThread();
                socketWatch.Close();
                //socketWatch.Dispose();
                //socketWatch.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.AcceptConnection, false);
                //ShowMsg("------服务器停止成功------");
            }
            catch { }
        }

        private void CloseThread()
        {
            if (threadWatch != null && threadWatch.ThreadState != ThreadState.Aborted)
            {
                try
                {
                    threadWatch.Abort();
                    //threadWatch.Join();
                }
                catch (Exception e)
                {
                    //ShowMsg(e.Message);
                }
            }
        }

        private void StartListen(int port)
        {
            try
            {
                listen = true;
                //取得IP地址
                IPAddress ip = IPAddress.Any;//IPAddress.Parse(this.txtIP.Text.Trim());
                //创建一个网络端点
                server = new IPEndPoint(ip, port);
                listen = Listen();
            }
            catch { }
        }

        private bool Listen()
        {
            bool listen = true;
            //创建一个基于IPV4的TCP协议的Socket对象
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socketWatch.Bind(server);
                socketWatch.Listen(0);
                CloseThread();
                threadWatch = new Thread(WatchConnection);
                threadWatch.IsBackground = true;//设置为后台
                threadWatch.Start();// 启动线程
            }
            catch (Exception e)
            {
                listen = false;
                //ShowMsg(e.Message);
            }
            return listen;
        }

        //监听客户端连接
        void WatchConnection()
        {
            while (true)
            {
                if (listen)
                {
                    IAsyncResult e = socketWatch.BeginAccept(new AsyncCallback(NewConnection), socketWatch);
                    if (e != null)
                    {
                        while (!e.IsCompleted)
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
            }
        }

        private void NewConnection(IAsyncResult e)
        {
            Socket sock = e.AsyncState as Socket;
            if (sock != null)
            {
                try
                {
                    //创建监听的连接套接字
                    Socket sockConnection = sock.EndAccept(e);
                    sockConnection.ReceiveTimeout = ReceiveTimeout;
                    //创建连接成功添加到dict对象里面
                    string key = sockConnection.RemoteEndPoint.ToString();
                    dict.Add(key, sockConnection);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate { RecMsg(sockConnection); }));
                    ////为客户端套接字连接开启新的线程用于接收客户端发送的数据
                    //Thread t = new Thread(RecMsg);
                    ////设置为后台线程
                    //t.IsBackground = true;
                    ////为客户端连接开启线程
                    //t.Start(sockConnection);
                    //ShowMsg("-----[" + key + "]客户端连接进入-----");
                }
                catch (Exception ex)
                {
                    //ShowMsg(ex.Message);
                }
            }
        }

        /// <summary>
        /// 接收客户端信息的线程执行代码
        /// </summary>
        /// <param name="o"></param>
        private void RecMsg(object o)
        {
            while (listen)
            {
                try
                {
                    //接收数据
                    Socket socketClient = o as Socket;
                    List<byte> append = new List<byte>();
                    while (socketClient.Available > 0)
                    {
                        //创建一个字节数组接收数据
                        byte[] arrMsgRec = new byte[BufferSize];
                        int length = socketClient.Receive(arrMsgRec);
                        append.AddRange(arrMsgRec.Take<byte>(length).ToArray());
                    }
                    if (append.Count > 0)
                    {
                        byte[] raw = append.ToArray();
                        //bool crc = Common.CRC16(raw, append.Count);
                        //if (!crc)
                        //    continue;
                        append = new List<byte>(raw);
                        char[] cs = Array.ConvertAll<byte, char>(raw, a => (char)a);
                        string rawData = new string(cs);
                        if (rawData != HeartBeat)//正常的消息或者下线的消息
                        {
                            byte[] data;
                            string target = Common.GetTarget(rawData, out data);
                            if (target != string.Empty && data != null)//数据转发给其他客户端的数据(如果有数据的话)
                            {
                                if (target == server.ToString())//指明是发给服务器的数据
                                {
                                    //string strMsgRec = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                                    //ShowMsg(socketClient.RemoteEndPoint.ToString() + "说：" + strMsgRec);
                                    Common.ProcessMyselfData(socketClient.RemoteEndPoint as IPEndPoint, data);
                                }
                                else
                                {//数据转发给其他的客户端
                                    string msg = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                                    if (dict.ContainsKey(target))
                                    {
                                        Socket socketSend = dict[target];
                                        //发送信息
                                        try
                                        {
                                            string source = Common.EndPointBegin + socketClient.RemoteEndPoint.ToString() + Common.EndPointEnd;
                                            byte[] from = Array.ConvertAll<char, byte>(source.ToCharArray(), a => (byte)a);
                                            List<byte> full = new List<byte>(from);
                                            full.AddRange(data);
                                            socketSend.Send(full.ToArray());
                                        }
                                        catch (Exception ex)
                                        {
                                            //ShowMsg("-----客户端[" + target + "]已经断开！-----");
                                        }
                                    }
                                }
                            }
                            else//默认发给服务器的数据
                            {
                                string strMsgRec = System.Text.Encoding.UTF8.GetString(append.ToArray(), 0, append.Count);
                                if (strMsgRec.Length > LeaveMsg.Length && strMsgRec.Substring(0, LeaveMsg.Length) == LeaveMsg)//下线消息
                                {
                                    string endpoint = strMsgRec.Substring(LeaveMsg.Length);
                                    //ShowMsg("-----客户端[" + endpoint + "]已经断开！-----");
                                    dict.Remove(endpoint);
                                }
                                else if (strMsgRec.Length > ExitMsg.Length && strMsgRec.Substring(0, ExitMsg.Length) == ExitMsg)//退出消息
                                {
                                    string endpoint = strMsgRec.Substring(ExitMsg.Length);
                                    //ShowMsg("-----客户端[" + endpoint + "]已经退出！-----");
                                    dict.Remove(endpoint);
                                }
                                else if (strMsgRec.Length > RunMsg.Length && strMsgRec.Substring(0, RunMsg.Length) == RunMsg)//终端运行消息
                                {
                                    string endpoint = strMsgRec.Substring(RunMsg.Length);
                                    //ShowMsg("-----客户端[" + endpoint + "]的终端正在运行！-----");
                                    OnRefreshStatus(new StatusArgs(endpoint, 1));
                                }
                                else if (strMsgRec.Length > StopMsg.Length && strMsgRec.Substring(0, StopMsg.Length) == StopMsg)//终端停止消息
                                {
                                    string endpoint = strMsgRec.Substring(StopMsg.Length);
                                    //ShowMsg("-----客户端[" + endpoint + "]的终端已经停止！-----");
                                    OnRefreshStatus(new StatusArgs(endpoint, 0));
                                }
                                else//正常消息
                                {
                                    //ShowMsg(socketClient.RemoteEndPoint.ToString() + "说：" + strMsgRec);
                                    Common.ProcessMyselfData(socketClient.RemoteEndPoint as IPEndPoint, append.ToArray());
                                }
                            }
                        }
                        else//客户端心跳包
                        {
                            //给客户端回复心跳包+客户端列表
                            byte[] hb = Array.ConvertAll<char, byte>(HeartBeat.ToCharArray(), a => (byte)a);
                            byte[] clients = Array.ConvertAll<char, byte>(string.Join(";", dict.Keys.ToArray<string>()).ToCharArray(), a => (byte)a);
                            List<byte> union = new List<byte>(hb);
                            union.AddRange(clients);
                            socketClient.Send(union.ToArray());
                        }
                    }
                }
                catch (Exception e)
                {
                    Socket socketClient = o as Socket;
                    //ShowMsg("-----客户端[" + socketClient.RemoteEndPoint.ToString() + "]已经断开！-----");
                    //MessageBox.Show("监听错误：" + e.Message);
                    //移除dict里的客户端套接字
                    dict.Remove(socketClient.RemoteEndPoint.ToString());
                    //关闭套接字
                    socketClient.Close();
                    //结束当前线程
                    Thread.CurrentThread.Abort();
                }
            }
        }

        private List<string> GetClientsByIP(string address)
        {
            List<string> clients = new List<string>();
            foreach (string key in dict.Keys)
            {
                if (key.StartsWith(address))
                    clients.Add(key);
            }
            return clients;
        }

        /// <summary>
        /// 启动客户端
        /// </summary>
        /// <param name="address">可以加端口的地址</param>
        public void Run(string address)
        {
            List<string> target = GetClientsByIP(address);
            if (target.Count > 0)
            {
                Socket client = dict[target[0]];
                byte[] run = Array.ConvertAll<char, byte>(RunMsg.ToCharArray(), a => (byte)a);
                SendData(client, run);
            }
        }

        /// <summary>
        /// 停止客户端
        /// </summary>
        /// <param name="address"></param>
        public void Stop(string address)
        {
            List<string> target = GetClientsByIP(address);
            if (target.Count > 0)
            {
                Socket client = dict[target[0]];
                byte[] stop = Array.ConvertAll<char, byte>(StopMsg.ToCharArray(), a => (byte)a);
                SendData(client, stop);
            }
        }

        /// <summary>
        /// 给客户端发送信息
        /// </summary>
        public void SendData(Socket client, byte[] data)
        {
            if (client != null)
            {
                //发送信息
                try
                {
                    client.Send(data);
                }
                catch (Exception e)
                {
                    //ShowMsg("-----客户端[" + client + "]已经断开！-----");
                }
            }
        }

        public bool IsTheClientOnline(EndPoint endPoint)
        {
            if (dict.ContainsKey(endPoint.ToString()))
            {
                Socket socketSend = dict[endPoint.ToString()];
                try
                {
                    byte[] hb = Array.ConvertAll<char, byte>(HeartBeat.ToCharArray(), a => (byte)a);
                    socketSend.Send(hb);
                    return true;
                }
                catch (SocketException e)
                {
                    //ShowMsg(e.Message);
                }
            }
            return false;
        }

        /// <summary>
        /// 配置文档中一定要有ServerSocket配置项
        /// </summary>
        public void Start()
        {
            IPAddress ip;
            int port;
            Common.GetServerSocket(out ip, out port);
            StartListen(port);
        }

        public void End()
        {
            StopListen();
        }
    }
}
