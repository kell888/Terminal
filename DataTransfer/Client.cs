using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Linq;

namespace DataTransfer
{
    public class ReceiveArgs : EventArgs
    {
        string msg;

        public string Msg
        {
            get { return msg; }
        }
        byte[] data;

        public byte[] Data
        {
            get { return data; }
        }

        public ReceiveArgs(byte[] data)
        {
            this.data = data;
            char[] cs = Array.ConvertAll<byte, char>(data, a => (char)a);
            this.msg = new string(cs);
        }
    }

    public class Client
    {
        private Thread threadclient;//客户端用于接收数据的线程
        private Socket socketClient;//客户端用于接收数据的套接字
        private volatile bool connect;
        public static string HeartBeat = Common.HeartBeat;
        public static string LeaveMsg = Common.LeaveMsg;
        public static string ExitMsg = Common.ExitMsg;
        public static string RunMsg = Common.RunMsg;
        public static string StopMsg = Common.StopMsg;
        public static int BufferSize = Common.BufferSize;
        public static int ReceiveTimeout = Common.ReceiveTimeout;
        private bool reconnect = true;
        IPEndPoint server;
        private List<string> dict = new List<string>();
        private SocketAddress lastSocket;
        private uint heartBeatInterval = 5;//单位：秒
        public event EventHandler<ReceiveArgs> Receiving;
        System.Timers.Timer timer1;

        private void OnReceiving(ReceiveArgs e)
        {
            if (Receiving != null)
                Receiving(this, e);
        }

        /// <summary>
        /// 向服务器发送正在运行的信号
        /// </summary>
        public void Running()
        {
            if (!socketClient.Connected || socketClient.LocalEndPoint == null)
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip;
                int port;
                Common.GetServerSocket(out ip, out port);
                s.Connect(ip, port);
                byte[] run = System.Text.Encoding.UTF8.GetBytes(RunMsg + s.LocalEndPoint.ToString());
                s.Send(run);
            }
            else
            {
                byte[] run = System.Text.Encoding.UTF8.GetBytes(RunMsg + socketClient.LocalEndPoint.Create(lastSocket ?? socketClient.LocalEndPoint.Serialize()).ToString());
                socketClient.Send(run);
            }
        }

        /// <summary>
        /// 向服务器发送正在停止的信号
        /// </summary>
        public void Stopping()
        {
            if (!socketClient.Connected || socketClient.LocalEndPoint == null)
            {
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip;
                int port;
                Common.GetServerSocket(out ip, out port);
                s.Connect(ip, port);
                byte[] run = System.Text.Encoding.UTF8.GetBytes(StopMsg + s.LocalEndPoint.ToString());
                s.Send(run);
            }
            else
            {
                byte[] stop = System.Text.Encoding.UTF8.GetBytes(StopMsg + socketClient.LocalEndPoint.Create(lastSocket ?? socketClient.LocalEndPoint.Serialize()).ToString());
                socketClient.Send(stop);
            }
        }

        public Client()
        {
            timer1 = new System.Timers.Timer(1000 * heartBeatInterval);
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
        }

        ~Client()
        {
            Disconnect(true);
        }

        void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Stop();
            if (socketClient != null)
            {
                bool flag = true;
                string rawData;
                connect = TestConnectToServer(ref flag, out rawData);
                if (connect && rawData.Length > HeartBeat.Length)
                {//获取当前在线的客户端列表
                    string clients = rawData.Substring(HeartBeat.Length);
                    string[] cs = clients.Split(';');
                    dict = new List<string>(cs);
                }
                //不是心跳包(flag=false)就丢弃不理
                if (!connect && flag)
                {
                    if (Reconnect)
                    {
                        //ShowMsg("-----心跳监测：服务器已经断开连接！重连中...");
                        Connect();
                    }
                    else
                    {
                        //ShowMsg("-----心跳监测：服务器已经断开连接！-----");
                    }
                }
            }
            timer1.Start();
        }

        /// <summary>
        /// 测试是否能连接到服务器
        /// </summary>
        /// <param name="flag">是否为心跳包</param>
        /// <param name="rawData">返回服务器发来的原始数据</param>
        /// <returns></returns>
        public bool TestConnectToServer(ref bool flag, out string rawData)
        {
            bool connect = false;
            rawData = string.Empty;
            //给服务器发送心跳包
            byte[] hb = Array.ConvertAll<char, byte>(HeartBeat.ToCharArray(), a => (byte)a);
            try
            {
                socketClient.Send(hb);
                //接收服务器的回复
                byte[] arrMsgRec = new byte[BufferSize];
                int length = socketClient.Receive(arrMsgRec);
                if (length >= HeartBeat.Length)
                {
                    rawData = new string(Array.ConvertAll<byte, char>(arrMsgRec.Take<byte>(length).ToArray(), a => (char)a));
                    connect = rawData.Length >= HeartBeat.Length && rawData.Substring(0, HeartBeat.Length) == HeartBeat;
                }
                else
                {
                    flag = false;
                }
            }
            catch (SocketException e)
            {
                //ShowMsg(e.Message);
            }
            return connect;
        }

        public void Connect(IPAddress ip, int port)
        {
            connect = true;
            server = new IPEndPoint(ip, port);
            Connect();
        }

        private void NewThread()
        {
            CloseThread();
            //开启一个线程实例
            threadclient = new Thread(new ThreadStart(RecMsg));
            //设为后台线程
            threadclient.IsBackground = true;
            //启动线程
            threadclient.Start();
        }

        public void Disconnect(bool exit = false)
        {
            connect = false;
            if (socketClient.Connected)
            {
                IAsyncResult e = null;
                if (exit)
                    e = Exit(socketClient.LocalEndPoint.Serialize());
                else
                    e = OutLine(socketClient.LocalEndPoint.Serialize());
                if (e != null)
                {
                    while (!e.IsCompleted)
                    {
                        Thread.Sleep(10);
                    }
                }
                CloseThread();
                if (!exit)
                {
                    if (dict != null)
                        dict.Clear();
                    if (timer1.Enabled)
                        timer1.Stop();
                }
            }
        }

        private void CloseThread()
        {
            if (threadclient != null && threadclient.ThreadState != ThreadState.Aborted)
            {
                try
                {
                    threadclient.Abort();
                    threadclient.Join();
                }
                catch (Exception e)
                {
                    //ShowMsg(e.Message);
                }
            }
        }

        private IAsyncResult OutLine(SocketAddress socketAddress)
        {
            IAsyncResult e = null;
            if (socketAddress != null)
            {
                try
                {
                    byte[] info = System.Text.Encoding.UTF8.GetBytes(LeaveMsg + socketClient.LocalEndPoint.Create(socketAddress).ToString());
                    e = socketClient.BeginSend(info, 0, info.Length, SocketFlags.None, new AsyncCallback(AsyncSend), socketClient);
                }
                catch (Exception ex)
                {
                    //ShowMsg("----下线通知发送失败：" + ex.Message + "----");
                }
            }
            return e;
        }

        private IAsyncResult Exit(SocketAddress socketAddress)
        {
            IAsyncResult e = null;
            if (socketAddress != null)
            {
                try
                {
                    byte[] info = System.Text.Encoding.UTF8.GetBytes(ExitMsg + socketClient.LocalEndPoint.Create(socketAddress).ToString());
                    e = socketClient.BeginSend(info, 0, info.Length, SocketFlags.None, new AsyncCallback(AsyncSend), socketClient);
                }
                catch
                { }
            }
            return e;
        }

        private void AsyncSend(IAsyncResult e)
        {
            Socket socketClient = e.AsyncState as Socket;
            if (socketClient != null)
            {
                socketClient.EndSend(e);
            }
        }

        private void Connect()
        {
            //这里必须创建新的Socket对象，否则不允许再次socketClient.Connect(point)！
            //先将上一个Socket信息保存下来：
            if (socketClient != null && socketClient.LocalEndPoint != null)
                lastSocket = socketClient.LocalEndPoint.Serialize();
            socketClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketClient.ReceiveTimeout = ReceiveTimeout;
            if (!socketClient.Connected)
            {
                try
                {
                    IAsyncResult e = socketClient.BeginConnect(server, new AsyncCallback(AsyncConnect), socketClient);
                    if (e != null)
                    {
                        while (!e.IsCompleted)
                        {
                            Thread.Sleep(10);
                        }
                    }
                    if (!timer1.Enabled)
                        timer1.Start();
                }
                catch (SocketException ex)
                {
                    connect = false;
                    //ShowMsg(ex.Message);
                }
            }
        }

        private void AsyncConnect(IAsyncResult e)
        {
            Socket socketClient = e.AsyncState as Socket;
            if (socketClient != null)
            {
                try
                {
                    socketClient.EndConnect(e);
                    connect = true;
                    IAsyncResult ee = OutLine(lastSocket);//这里不需要阻塞同步
                    if (ee != null)
                    {
                        while (!ee.IsCompleted)
                        {
                            Thread.Sleep(10);
                        }
                    }
                    NewThread();
                    //ShowMsg("---------服务器连接成功--------");
                }
                catch (Exception ex)
                {
                    //ShowMsg(ex.Message);
                }
            }
        }

        //线程中用于接收数据的方法
        public void RecMsg()
        {
            while (connect)
            {
                if (!connect)
                {
                    //ShowMsg("---------服务器断开成功--------");
                    break;
                }
                if (socketClient.Available > 0)
                {
                    try
                    {
                        //接收数据
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
                            string recStr = new string(Array.ConvertAll<byte, char>(raw, a => (char)a));

                            if (recStr.Length >= HeartBeat.Length && recStr.Substring(0, HeartBeat.Length) == HeartBeat)
                            {
                                //排除心跳包
                            }
                            else
                            {
                                byte[] data;
                                string target = Common.GetTarget(recStr, out data);
                                OnReceiving(new ReceiveArgs(data));
                                //if (data != null)
                                //{
                                //string msg = new string(Array.ConvertAll<byte, char>(data, a => (char)a));
                                //if (target == string.Empty || target == server.ToString())
                                //ShowMsg("服务器对我说：" + msg);
                                //else
                                //ShowMsg("客户端[" + target + "]对我说：" + msg);
                                //}
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        connect = false;
                        //ShowMsg("-----服务器已经断开连接！-----");
                        socketClient.Close();
                        //结束当前线程
                        Thread.CurrentThread.Abort();
                    }
                }
            }
        }

        /// <summary>
        /// 往服务器发送数据
        /// </summary>
        public void SendData(string client, byte[] data)
        {
            if (socketClient != null)
            {
                string endpoint = Common.EndPointBegin + client + Common.EndPointEnd;
                try
                {
                    string rawData = endpoint + new string(Array.ConvertAll<byte, char>(data, a => (char)a));
                    byte[] arrMsg = Array.ConvertAll<char, byte>(rawData.ToCharArray(), a => (byte)a);
                    socketClient.Send(arrMsg);
                    //if (client == string.Empty || client == server.ToString())
                    //ShowMsg("我对服务器说：" + sendMsg);
                    //else
                    //ShowMsg("我对客户端[" + client + "]说：" + sendMsg);
                }
                catch (Exception e)
                {
                    connect = false;
                    //ShowMsg("已经断开连接！请重新连接服务器");
                }
            }
        }
        /// <summary>
        /// 单位：秒
        /// </summary>
        public uint HeartBeatInterval
        {
            get
            {
                return heartBeatInterval;
            }
            set
            {
                heartBeatInterval = value;
                timer1.Interval = 1000 * heartBeatInterval;
            }
        }

        public bool Reconnect
        {
            get
            {
                return reconnect;
            }
            set
            {
                reconnect = value;
            }
        }
    }
}
