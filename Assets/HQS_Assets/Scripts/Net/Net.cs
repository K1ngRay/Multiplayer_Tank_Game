using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System.Net.Sockets;
using System;

public class Net : MonoBehaviour {


    public InputField hostInput;
    public InputField portInput;
    public InputField recvInput;
    public InputField clientInput;
    public InputField sendInput;

    Socket socket;
    string recvStr = "";
    const int BUFFER_SIZE = 1024;
    byte[] readBuffer = new byte[BUFFER_SIZE];
    #region 异步
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
            string str = System.Text.Encoding.UTF8.GetString(readBuffer, 0, count);
            if (recvStr.Length > 300)
                recvStr = "";
            recvStr += str + "\n";            
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
        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        try {
            socket.Send(bytes);
        }
        catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }
    #endregion
    #region 同步
    //public void Connection() {
    //    recvInput.text = "";
    //    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    //    string host = hostInput.text;
    //    int port = int.Parse(portInput.text);
    //    socket.Connect(host, port); //连接服务器
    //    clientInput.text = socket.LocalEndPoint.ToString();
    //    //Send
    //    string str = "Hello Unity";
    //    byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
    //    socket.Send(bytes);
    //    //Recv
    //    int count = socket.Receive(readBuffer);
    //    str = System.Text.Encoding.UTF8.GetString(readBuffer, 0, count);
    //    recvInput.text = str;
    //    //close
    //    socket.Close();
    //}
    #endregion
}
