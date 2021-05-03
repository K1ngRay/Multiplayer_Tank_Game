using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.UI;

public class Walk : MonoBehaviour {

    public GameObject prefab;

    string id;
    Socket socket;
    const int BUFFER_SIZE = 1024;
    byte[] readBuffer = new byte[BUFFER_SIZE];
    //玩家列表
    Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    //消息列表
    List<string> msgList = new List<string>();


    //添加玩家
    void AddPlayer(string id, Vector3 pos) {
        GameObject player = (GameObject)Instantiate(prefab, pos, Quaternion.identity);
        TextMesh textMesh = player.GetComponentInChildren<TextMesh>();
        textMesh.text = id;
        players.Add(id, player);
    }

    //发送位置协议
    //实例："POS 127.0.0.1:1234 1 1 1"
    void SendPosProtocol(Vector3 pos) {
        GameObject player = players[id];
        //Vector3 pos = player.transform.position;

        string str = "POS ";
        str += id + " ";
        str += pos.x.ToString() + " ";
        str += pos.y.ToString() + " ";
        str += pos.z.ToString() + " ";
        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        socket.Send(bytes);
        Debug.Log("发送" + str);
    }

    //发送离开协议
    //实例："LEAVE 127.0.0.1:1234"
    void SendLeaveProtocol() {
        string str = "LEAVE ";
        str += id + " ";
        byte[] bytes = System.Text.Encoding.Default.GetBytes(str);
        socket.Send(bytes);
        Debug.Log("发送" + str);
    }

    void Move() {
        if (id == "")
            return;
        GameObject player = players[id];
        //上
        if (Input.GetKey(KeyCode.UpArrow)) {
            //player.transform.position += new Vector3(0, 0, 1);
            SendPosProtocol(player.transform.position + new Vector3(0, 0, 1));
        }
        //下
        else if (Input.GetKey(KeyCode.DownArrow)) {
            //player.transform.position += new Vector3(0, 0, -1);
            SendPosProtocol(player.transform.position + new Vector3(0, 0, -1));
        }
        //左
        else if (Input.GetKey(KeyCode.LeftArrow)) {
            //player.transform.position += new Vector3(-1, 0, 0);
            SendPosProtocol(player.transform.position + new Vector3(-1, 0, 0));
        }
        //右
        else if (Input.GetKey(KeyCode.RightArrow)) {
            //player.transform.position += new Vector3(1, 0, 0);
            SendPosProtocol(player.transform.position + new Vector3(1, 0, 0));
        }
    }

    private void OnDestroy() {
        SendLeaveProtocol();
    }

    private void Start() {
        Connect();
        //初试位置
        UnityEngine.Random.seed = (int)DateTime.Now.Ticks;
        float x = 0 + UnityEngine.Random.Range(-30, 30);
        float y = 0;
        float z = 0 + UnityEngine.Random.Range(-30, 30);
        Vector3 pos = new Vector3(x, y, z);        AddPlayer(id, pos);
        //同步
        SendPosProtocol(pos);
    }

    private void Connect() {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //conn
        socket.Connect("127.0.0.1", 1234);
        id = socket.LocalEndPoint.ToString();
        //recv
        socket.BeginReceive(readBuffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
    }

    private void ReceiveCb(IAsyncResult ar) {
        try {
            int count = socket.EndReceive(ar);

            string str = System.Text.Encoding.UTF8.GetString(readBuffer, 0, count);
            msgList.Add(str);

            socket.BeginReceive(readBuffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCb, null);
        }
        catch (Exception e) {

            socket.Close();
        }
    }

    private void Update() {
        for (int i = 0; i < msgList.Count; i++) {
            HandleMsg();
        }
        Move();
    }

    void HandleMsg() {
        if (msgList.Count <= 0) {
            return;
        }

        string str = msgList[0];
        msgList.RemoveAt(0);

        string[] args = str.Split(' ');
        if (args[0] == "POS")
            OnRecvPos(args[1], args[2], args[3], args[4]);
        else if (args[0] == "LEAVE")
            OnRecvLeave(args[1]);

    }

    void OnRecvPos(string id, string xStr, string yStr, string zStr) {
        //if (id == this.id)
        //    return; //todo:若没有这个条件，是不是会出现网络很卡时，自己也卡

        float x = float.Parse(xStr);
        float y = float.Parse(yStr);
        float z = float.Parse(zStr);
        Vector3 pos = new Vector3(x, y, z);

        if (players.ContainsKey(id)) {
            players[id].transform.position = pos;
        }
        else {
            AddPlayer(id, pos); //没有初始化该玩家，则创建该玩家
        }
    }

    public void OnRecvLeave(string id) {
        if (players.ContainsKey(id)) {
            Destroy(players[id]);
            players[id] = null;
        }
    }
}
