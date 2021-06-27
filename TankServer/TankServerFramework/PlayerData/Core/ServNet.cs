using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Reflection;
class ServNet {
    public Socket listenfd;
    public Conn[] conns;
    public int maxConn = 50;

    public ProtocolBase proto;

    System.Timers.Timer timer = new System.Timers.Timer(1000);
    public long heartBeatTime = 180;

    public HandleConnMsg handleConnMsg = new HandleConnMsg();
    public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();
    public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent();
    //单例
    public static ServNet instance;
    public ServNet() {
        instance = this;
    }

    //获取连接池索引，返回负数表示获取失败
    public int NewIndex() {
        if (conns == null) {
            return -1;
        }
        for (int i = 0; i < conns.Length; i++) {
            if (conns[i] == null) {
                conns[i] = new Conn();
                return i;
            }
            else if (!conns[i].isUse) {
                return i;
            }
        }
        return -1;
    }

    //开启服务器
    public void Start(string host, int port) {
        conns = new Conn[maxConn];
        for (int i = 0; i < maxConn; i++) {
            conns[i] = new Conn();
        }
        //Socket
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Bind
        IPAddress ipArr = IPAddress.Parse(host);
        IPEndPoint ipEp = new IPEndPoint(ipArr, port);
        listenfd.Bind(ipEp);
        //Listen
        listenfd.Listen(maxConn);
        //Accept
        listenfd.BeginAccept(AcceptCb, null);
        Console.WriteLine("[服务器]启动成功");

        //定时器
        timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
        timer.AutoReset = false;
        timer.Enabled = true;
    }
    private void AcceptCb(IAsyncResult ar) {
        try {
            Socket socket = listenfd.EndAccept(ar);
            int index = NewIndex();
            if (index < 0) {
                Console.WriteLine("[警告]连接已满");
                socket.Close();
            }
            else {
                Conn conn = conns[index];
                conn.Init(socket);
                string adr = conn.GetAddress();
                Console.WriteLine("客户端连接[" + adr + "]缓冲池ID：" + index);
                conn.socket.BeginReceive(conn.readBuffer, conn.bufferCount,
                    conn.BufferRemain(), SocketFlags.None, ReceiveCb, conn);
            }
            listenfd.BeginAccept(AcceptCb, null);
        }
        catch (Exception e) {
            Console.WriteLine("Accept失败：" + e.Message);
        }
    }

    private void ReceiveCb(IAsyncResult ar) {
        Conn conn = (Conn)ar.AsyncState;
        try {
            int count = conn.socket.EndReceive(ar);
            if (count <= 0) {
                Console.WriteLine("收到[" + conn.GetAddress() + "]断开连接");
                conn.Close();
                return;
            }
            //数据处理
            conn.bufferCount += count;
            ProcessData(conn);
            //继续接收
            conn.socket.BeginReceive(conn.readBuffer, conn.bufferCount,
                conn.BufferRemain(), SocketFlags.None, ReceiveCb, conn);
        }
        catch (Exception e) {
            Console.WriteLine("收到[" + conn.GetAddress() + "]断开连接");
            conn.Close();
        }
    }

    //关闭
    public void Close() {
        for (int i = 0; i < conns.Length; i++) {
            Conn conn = conns[i];
            if (conn == null) continue;
            if (!conn.isUse) continue;
            lock (conn) { //防止线程竞争，服务端框架中至少会有主线程、异步回调、心跳定时器线程处理同一连接。
                conn.Close();
            }
        }
    }

    private void ProcessData(Conn conn) {
        //小于字节长度
        if (conn.bufferCount < sizeof(Int32)) return;
        Array.Copy(conn.readBuffer, conn.lenBytes, sizeof(Int32));
        conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0); //获取长度
        if (conn.bufferCount < conn.msgLength + sizeof(Int32))
            return;
        //string str = System.Text.Encoding.UTF8.GetString(conn.readBuffer,
        //    sizeof(Int32), conn.msgLength);

        //Console.WriteLine("收到消息[" + conn.GetAddress() + "]" + str);
        //if (str == "HeatBeat")
        //    conn.lastTickTime = Sys.GetTimeStamp();
        //Send(conn, str);
        //处理消息
        ProtocolBase protocol = proto.Decode(conn.readBuffer, sizeof(int), conn.msgLength);
        HandleMsg(conn, protocol);
        //删除长度字节
        int count = conn.bufferCount - conn.msgLength - sizeof(Int32);
        Array.Copy(conn.readBuffer, sizeof(Int32) + conn.msgLength,
            conn.readBuffer, 0, count);
        conn.bufferCount = count;
        if (conn.bufferCount > 0) {
            ProcessData(conn);
        }
    }

    public void Send(Conn conn,string str) {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
        byte[] length = BitConverter.GetBytes(bytes.Length);
        byte[] sendBuffer = length.Concat(bytes).ToArray(); //todo:这是什么操作
        try {
            conn.socket.BeginSend(sendBuffer, 0, sendBuffer.Length,
                SocketFlags.None, null, null);
        }
        catch (Exception e) {
            Console.WriteLine("[发送消息]"+conn.GetAddress()+" : "+e.Message);            
        }
    }

    public void Send(Conn conn,ProtocolBase protocol) {
        byte[] bytes = protocol.Encode();
        byte[] length = BitConverter.GetBytes(bytes.Length);
        byte[] sendBuffer = length.Concat(bytes).ToArray();
        try {
            conn.socket.BeginSend(sendBuffer, 0, sendBuffer.Length,
                SocketFlags.None, null, null);
        }
        catch (Exception e) {
            Console.WriteLine("[发送消息]" + conn.GetAddress() + " : " + e.Message);
        }
    }

    public void Broadcast(ProtocolBase protocol) {
        for (int i = 0; i < conns.Length; i++) {
            if (!conns[i].isUse) continue;
            if (conns[i].player == null) continue;
            Send(conns[i], protocol);
        }
    }

    private void HandleMainTimer(object sender,System.Timers.ElapsedEventArgs e) {
        //HeartBeat();
        timer.Start();
    }

    private void HandleMsg(Conn conn,ProtocolBase protoBase) {
        string name = protoBase.GetName();
        string methodName = "Msg" + name; //todo:值得好好学学

        if(conn.player == null || name == "HeatBeat" || name == "Logout") {
            MethodInfo mm = handleConnMsg.GetType().GetMethod(methodName);
            if (mm == null) {
                string str = "[警告]HandleMsg:没有处理连接方法";
                Console.WriteLine(str+methodName);
                return;
            }
            Object[] obj = new object[] { conn, protoBase };
            Console.WriteLine("[处理连接信息]" + conn.GetAddress() + ":" + name);
            mm.Invoke(handleConnMsg, obj);
        }
        else {
            MethodInfo mm = handlePlayerMsg.GetType().GetMethod(methodName);
            if (mm == null) {
                string str = "[警告]HandleMsg没有处理玩家方法";
                Console.WriteLine(str + methodName);
                return;
            }
            Object[] obj = new object[] { conn.player, protoBase };
            Console.WriteLine("[处理玩家消息]" + conn.player.id + ":" + name);
            mm.Invoke(handlePlayerMsg, obj);
        }
    }

    //心跳回调函数，每秒执行一次
    private void HeartBeat() {
        long timeNow = Sys.GetTimeStamp();
        //Console.WriteLine("[主定时器运行]");
        for (int i = 0; i < conns.Length; i++) {
            Conn conn = conns[i];
            if (conn == null) continue;
            if (!conn.isUse) continue;
            if (conn.lastTickTime < timeNow - heartBeatTime) { 
                Console.WriteLine("[心跳引起断开连接]"+conn.GetAddress());
                lock (conn) {
                    conn.Close();
                }
            }
        }
    }

    public void Print() {
        Console.WriteLine("===服务器登录信息===");
        for (int i = 0; i < conns.Length; i++) {
            if (conns[i] == null) continue;
            if (!conns[i].isUse) continue;

            string str = "连接[" + conns[i].GetAddress() + "]";
            if (conns[i].player!=null) {
                str += "玩家id " + conns[i].player.id;
            }
            Console.WriteLine(str);
        }
    }
}
