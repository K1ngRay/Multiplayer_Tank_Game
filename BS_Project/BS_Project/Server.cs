using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace BS_Project {
    public class Server {
        public Socket listenfd;
        public Connect[] connects;
        public int maxConnect = 50;

        //获取连接池索引，返回负数表示获取失败
        public int NewIndex() {
            if (connects == null)
                return -1;
            for (int i = 0; i < connects.Length; i++) {
                if (connects[i] == null) {
                    connects[i] = new Connect();
                    return i;
                }
                else if (!connects[i].isUse) {
                    return i;
                }
            }
            return -1;
        }

        //开启服务器
        public void Start(string host, int port) {
            connects = new Connect[maxConnect];
            for (int i = 0; i < maxConnect; i++) {
                connects[i] = new Connect();
            }
            //Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            //Listen
            listenfd.Listen(maxConnect);
            //Accept
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("[服务器]启动成功");
        }

        //回调
        private void AcceptCb(IAsyncResult ar) {
            try {
                Socket socket = listenfd.EndAccept(ar);
                int index = NewIndex();
                if (index < 0) {
                    Console.WriteLine("[警告]连接已满");
                    socket.Close();
                }
                else {
                    Connect conn = connects[index];
                    conn.Init(socket);
                    string adr = conn.GetAddress();
                    Console.WriteLine("客户端连接[" + adr + "]缓冲池ID：" + index);
                    conn.socket.BeginReceive(conn.readBuffer, conn.bufferCount, conn.BufferRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception e) {
                Console.WriteLine("Accept失败:" + e.Message);
            }
        }
        private void ReceiveCb(IAsyncResult ar) {
            //为什么要创建这个对象
            Connect conn = (Connect)ar.AsyncState;
            try {
                int count = conn.socket.EndReceive(ar);
                if (count<=0) {
                    Console.WriteLine("收到["+conn.GetAddress()+"]断开连接");
                    conn.Close();
                    return;
                }
                string str = System.Text.Encoding.UTF8.GetString(conn.readBuffer, 0, count);
                Console.WriteLine("收到["+conn.GetAddress()+"]数据："+str);
                str = conn.GetAddress() + ":" + str;
                byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
                //广播
                for (int i = 0; i < connects.Length; i++) {
                    if (connects[i]==null||!connects[i].isUse) {
                        continue;
                    }
                    Console.WriteLine("将消息转播给" + connects[i].GetAddress());
                    connects[i].socket.Send(bytes);
                }
                conn.socket.BeginReceive(conn.readBuffer, conn.bufferCount, conn.BufferRemain(),
                    SocketFlags.None, ReceiveCb, conn);
            }
            catch (Exception e) {
                Console.WriteLine("收到[" + conn.GetAddress() + "]断开连接");
                conn.Close();
            }
        }
    }

}
