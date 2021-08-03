using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

public class Connection {
    //状态
    public enum Status {
        None,
        Connected,
    };
    //常量
    const int BUFFER_SIZE = 1024;
    //socket
    private Socket socket;
    //Buffer
    private byte[] readBuffer = new byte[BUFFER_SIZE];
    private int bufferCount = 0;
    //粘包分包
    private int msgLength = 0;
    private byte[] lenBytes = new byte[sizeof(int)];
    //协议
    public ProtocolBase proto;
    //心跳
    public float lastTickTime = 0;
    public float heartBeatTime = 30;
    //消息分发
    public MsgDistribution msgDist = new MsgDistribution();

    public Status status = Status.None;

    public bool Connect(string host, int port) {
        try {
            //socket
            socket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);
            //connect
            socket.Connect(host, port);
            //异步接收
            socket.BeginReceive(readBuffer, bufferCount,
                BUFFER_SIZE - bufferCount, SocketFlags.None,
                ReceiveCb, readBuffer);
            Debug.Log("连接成功");
            //状态
            status = Status.Connected;
            return true;
        }
        catch (Exception e) {
            Debug.Log("连接失败：" + e.Message);
            return false;
        }
    }

    public void ReceiveCb(IAsyncResult ar) {
        try {
            int count = socket.EndReceive(ar);
            bufferCount += count;
            ProcessData();
            socket.BeginReceive(readBuffer, bufferCount, BUFFER_SIZE - bufferCount,
                SocketFlags.None, ReceiveCb, readBuffer);
        }
        catch (Exception e) {
            Debug.Log("RecviceCb 失败:" + e.Message);
            status = Status.None;
        }
    }

    private void ProcessData() {
        if (bufferCount < sizeof(int))
            return;
        Array.Copy(readBuffer, lenBytes, sizeof(int));
        msgLength = BitConverter.ToInt32(lenBytes, 0);
        if (bufferCount < msgLength + sizeof(int)) {
            return;
        }
        //协议解码
        ProtocolBase protocol = proto.Decode(readBuffer, sizeof(int), msgLength);
        //Debug.Log("收到消息 " + protocol.GetDesc());
        lock (msgDist.msgList) {
            msgDist.msgList.Add(protocol);
        }
        int count = bufferCount - msgLength - sizeof(int);
        Array.Copy(readBuffer, sizeof(int) + msgLength, readBuffer, 0, count);
        bufferCount = count;
        if (bufferCount > 0) {
            ProcessData();
        }
    }

    public bool Send(ProtocolBase protocol) {
        if (status != Status.Connected) {
            Debug.LogError("[Connection]未连接");
            return true;
        }
        byte[] b = protocol.Encode();
        byte[] length = BitConverter.GetBytes(b.Length);
        byte[] sendBuffer = length.Concat(b).ToArray();
        socket.Send(sendBuffer);
        Debug.Log("发送消息" + protocol.GetDesc());
        return true;
    }

    public bool Send(ProtocolBase protocol,string cbName,MsgDistribution.Delegate cb) {
        if (status != Status.Connected)
            return false;
        msgDist.AddOnceListener(cbName, cb);
        return Send(protocol);
    }

    public bool Send(ProtocolBase protocol,MsgDistribution.Delegate cb) {
        string cbName = protocol.GetName();
        return Send(protocol, cbName, cb);
    }

    public void Update() {
        msgDist.Update();
        if(status == Status.Connected) {
            if(Time.time - lastTickTime > heartBeatTime) {
                ProtocolBase protocol = NetMgr.GetHeartBeatProtocol();
                Send(protocol);
                lastTickTime = Time.time;
            }
        }
    }

    //关闭连接
    public bool Close() {
        try {
            socket.Close();
            return true;
        }
        catch (Exception e) {
            Debug.Log("关闭失败：" + e.Message);
            return false;
        }
    }
}
