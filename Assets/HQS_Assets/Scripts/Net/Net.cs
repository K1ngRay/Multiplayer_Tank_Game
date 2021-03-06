using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;
using System.Linq;

public class Net : MonoBehaviour {


    public InputField hostInput;
    public InputField portInput;
    public InputField recvInput;
    public InputField clientInput;
    public InputField sendInput;
    public InputField idInput;
    public InputField pwdInput;

    Socket socket;
    string recvStr = "";
    const int BUFFER_SIZE = 1024;
    byte[] readBuffer = new byte[BUFFER_SIZE];
    //沾包分包
    int bufferCount = 0;
    byte[] lenBytes = new byte[sizeof(UInt32)];
    Int32 msgLength = 0;

    ProtocolBase proto = new ProtocolBytes();
    private void Update() {
        recvInput.text = recvStr;
    }
    public void Connection() {
        recvInput.text = "";
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string host = hostInput.text;
        int port = int.Parse(portInput.text);
        try {
            socket.Connect(host, port); //连接服务器
            clientInput.text = socket.LocalEndPoint.ToString();
            //Recv
            socket.BeginReceive(readBuffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    private void ReceiveCb(IAsyncResult ar) {
        try {
            int count = socket.EndReceive(ar);
            //string str = System.Text.Encoding.UTF8.GetString(readBuffer, 0, count);
            //if (recvStr.Length > 300)
            //    recvStr = "";
            //recvStr += str + "\n";
            bufferCount += count;
            ProcessData();
            socket.BeginReceive(readBuffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null); //因为没必要转化为什么类，所以直接给null，可参考服务端的不同
        }
        catch (Exception e) {
            recvStr += "连接已断开";
            Debug.LogError(e.Message);
            socket.Close();
        }
    }

    public void Send() {
        string str = sendInput.text;
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
        byte[] length = BitConverter.GetBytes(bytes.Length);
        byte[] sendBuffer = length.Concat(bytes).ToArray();
        try {
            socket.Send(sendBuffer);
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }

    private void ProcessData() {
        if (bufferCount < sizeof(Int32)) return;
        Array.Copy(readBuffer, lenBytes, sizeof(Int32));
        msgLength = BitConverter.ToInt32(lenBytes, 0);
        if (bufferCount < msgLength + sizeof(Int32)) return;
        //处理消息
        //string str = System.Text.Encoding.UTF8.GetString(readBuffer,
        //    sizeof(Int32), msgLength);
        //recvStr = str;
        ProtocolBase protocol = proto.Decode(readBuffer, sizeof(int), msgLength);
        HandleMsg(protocol);
        //清除已处理的消息
        int count = bufferCount - msgLength - sizeof(Int32);
        Array.Copy(readBuffer, msgLength, readBuffer, 0, count);
        bufferCount = count;
        if (count > 0)
            ProcessData();
    }

    private void HandleMsg(ProtocolBase protoBase) {
        ProtocolBytes proto = (ProtocolBytes)protoBase;
        Debug.Log("接收" + proto.GetDesc());

        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        recvStr = "接收" + proto.GetName() + " " + ret.ToString();
    }

    public void OnSendClick() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("HeartBeat");
        Debug.Log("发送" + protocol.GetDesc());
        Send(protocol);
    }

    private void Send(ProtocolBase protocol) {
        byte[] bytes = protocol.Encode();
        byte[] length = BitConverter.GetBytes(bytes.Length);
        byte[] sendBuffer = length.Concat(bytes).ToArray();
        socket.Send(sendBuffer);
    }

    public void OnLoginClick() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwdInput.text);
        Debug.Log("发送 " + protocol.GetDesc());
        Send(protocol);
    }

    public void OnAddClick() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("AddScore");
        Debug.Log("发送 " + protocol.GetDesc());
        Send(protocol);
    }

    public void OnGetClick() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetScore");
        Debug.Log("发送 " + protocol.GetDesc());
        Send(protocol);
    }
}
