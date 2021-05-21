using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Threading;

class Conn {
    //缓冲区大小
    public const int BUFFER_SIZE = 1024;
    //Socket
    public Socket socket;
    public bool isUse = false;
    public byte[] readBuffer = new byte[BUFFER_SIZE];
    public int bufferCount = 0;
    //粘包分包
    public byte[] lenBytes = new byte[sizeof(UInt32)]; //四个字节的数组，用于存储消息长度
    public Int32 msgLength = 0;
    //心跳时间
    public long lastTickTime = long.MinValue;
    //对应的Player
    public Player player;

    public Conn() {
        readBuffer = new byte[BUFFER_SIZE];
    }

    public void Init(Socket socket) {
        this.socket = socket;
        isUse = true;
        bufferCount = 0;
        //心跳处理
        lastTickTime=Sys.GetTimeStamp();
    }

    //剩余的BUFFER
    public int BufferRemain() {
        return BUFFER_SIZE - bufferCount;
    }

    //获取客户端地址
    public string GetAddress() {
        if (!isUse)
            return "无法获取地址";
        return socket.RemoteEndPoint.ToString();
    }

    //关闭
    public void Close() {
        if (!isUse)
            return;
        if (player!=null) {
            //玩家退出处理
            //player.Logout();
            return;
        }
        Console.WriteLine("[断开连接]"+GetAddress());
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        isUse = false;
    }

    //发送协议
    public void Send() {

    }
}

