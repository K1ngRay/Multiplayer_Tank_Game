using System;
using System.Net;
using System.Net.Sockets;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;



namespace BS_Project {
    public class Server {
        public Socket listenfd;
        public Connect[] connects;
        public int maxConnect = 50;
        //数据库
        MySqlConnection sqlConn;

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
            //数据库
            string connStr = "Database=msgboard;Data Source=127.0.0.1;" +
                "User Id=root;Password=rootroot;port=3306";
            sqlConn = new MySqlConnection(connStr);
            try {
                sqlConn.Open();
            }
            catch (Exception e) {
                Console.WriteLine("[数据库]链接失败"+e.Message);
                return;
            }
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

        //Socket接入回调
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
            Connect conn = (Connect)ar.AsyncState;
            try {
                int count = conn.socket.EndReceive(ar);
                if (count <= 0) {
                    Console.WriteLine("收到[" + conn.GetAddress() + "]断开连接");
                    conn.Close();
                    return;
                }
                //数据处理
                string str = System.Text.Encoding.UTF8.GetString(conn.readBuffer, 0, count);
                Console.WriteLine("收到[" + conn.GetAddress() + "]数据：" + str);
                //HandleMsg(conn, str);  //数据库处理数据             
                //不对数据库进行修改，直接广播
                byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
                for (int i = 0; i < connects.Length; i++) {
                    if (connects[i] == null || !connects[i].isUse) continue;
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

        public void HandleMsg(Connect conn,string str) {
            //获取数据
            if (str=="_GET") {
                string cmdStr = "select * from msg order by id desc limit 10;"; //按降序排序，搜索前10条数据
                MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
                try {
                    //MySqlDataReader提供了一种从数据集读取数据的方法，在调用它的Read方法后，dataReader对象指向数据集的下一条记录。如果当前是最后一条记录，那么Read方法将返回null。
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    str = "";
                    while (dataReader.Read()) {
                        str += dataReader["name"] + ":" + dataReader["msg"] + "\n\r";
                    }
                    dataReader.Close();
                    byte[] bytes = System.Text.Encoding.Default.GetBytes(str);

                    //广播
                    for (int i = 0; i < connects.Length; i++) {
                        if (connects[i] == null || !connects[i].isUse) continue;
                        Console.WriteLine("将消息转播给"+connects[i].GetAddress());
                        connects[i].socket.Send(bytes);
                    }
                    //conn.socket.Send(bytes);
                }
                catch (Exception e) {
                    Console.WriteLine("[数据库]查询失败 " + e.Message);
                }
            }
            //插入数据
            else {
                string cmdStrFormat = "insert into msg set name = '{0}',msg = '{1}';";
                string cmdStr = string.Format(cmdStrFormat, conn.GetAddress(), str);
                MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
                try {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e) {
                    Console.WriteLine("[数据库]插入失败 " + e.Message);
                }
            }
        }
    }
}
