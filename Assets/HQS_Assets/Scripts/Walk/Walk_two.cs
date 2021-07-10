using System.Collections.Generic;
using UnityEngine;

public class Walk_two : MonoBehaviour {

    public GameObject prefab;
    //上一次移动的时间
    [HideInInspector]
    public float lastMoveTime;
    //单例
    public static Walk_two instance;

    private Dictionary<string, GameObject> playersDict = new Dictionary<string, GameObject>();
    //Self
    private string playerid = "";

	void Start () {
        instance = this;
	}
    
    //添加玩家
    void AddPlayer(string id,Vector3 pos,int score) {
        GameObject player = Instantiate(prefab, pos, Quaternion.identity);
        TextMesh textMesh = player.GetComponentInChildren<TextMesh>();
        textMesh.text = id + ":" + score;
        playersDict.Add(id, player);
    }

    //删除玩家
    void DelPlayer(string id) {
        if (playersDict.ContainsKey(id)) {
            Destroy(playersDict[id]);
            playersDict.Remove(id);
        }
    }

    //更新分数
    public void UpdateScore(string id,int score) {
        GameObject player = playersDict[id];
        if (player == null) return;

        TextMesh textMesh = player.GetComponentInChildren<TextMesh>();
        textMesh.text = id + ":" + score;
    }

    //更新信息
    public void UpdateInfo(string id,Vector3 pos,int score) {
        //更新自己的分数
        if (id == playerid) {
            UpdateScore(id, score);
            return;
        }
        //更新别人的分数
        //已经初始化的玩家
        if (playersDict.ContainsKey(id)) {
            playersDict[id].transform.position = pos;
            UpdateScore(id, score);
        }
        //尚未初始化的玩家
        else {
            AddPlayer(id, pos, score);
        }
    }

    public void StartGame(string id) {
        playerid = id;

        //添加自己
        Random.seed = (int)System.DateTime.Now.Ticks;
        float x = 10 + Random.Range(-3, 3);
        float y = 0;
        float z = 10 + Random.Range(-3, 3);
        Vector3 pos = new Vector3(x, y, z);
        AddPlayer(playerid, pos, 0);

        //同步
        SendPos();

        //获取列表
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("GetList"); //发送GetList协议获取玩家列表
        NetMgr.srvConn.Send(proto, GetList);
        NetMgr.srvConn.msgDist.AddListener("UpdateInfo", UpdateInfo);
        NetMgr.srvConn.msgDist.AddListener("PlayerLeave", PlayerLeave);
    }

    void SendPos() {
        GameObject player = playersDict[playerid];
        Vector3 pos = player.transform.position;

        //消息
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("UpdateInfo");
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(pos.z);
        NetMgr.srvConn.Send(proto);
    }

    //更新列表
    public void GetList(ProtocolBase protocol) {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        //获取头部数值
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        //遍历
        for (int i = 0; i < count; i++) {
            string id = proto.GetString(start, ref start);
            float x = proto.GetFloat(start, ref start);
            float y = proto.GetFloat(start, ref start);
            float z = proto.GetFloat(start, ref start);
            int score = proto.GetInt(start, ref start);
            Vector3 pos = new Vector3(x, y, z);
            UpdateInfo(id, pos, score);
        }
    }

    //更新信息
    public void UpdateInfo(ProtocolBase protocol) {
        //获取数值
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        float x = proto.GetFloat(start, ref start);
        float y = proto.GetFloat(start, ref start);
        float z = proto.GetFloat(start, ref start);
        int score = proto.GetInt(start, ref start);
        Vector3 pos = new Vector3(x, y, z);
        UpdateInfo(id, pos, score);
    }

    //玩家离开
    public void PlayerLeave(ProtocolBase protocol) {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        //获取数值
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        DelPlayer(id);
    }

    void Move() {
        if (playerid == "") return;
        if (playersDict[playerid] == null) return;
        if (Time.time - lastMoveTime < 0.1) return;
        lastMoveTime = Time.time;
        GameObject player = playersDict[playerid];

        //键盘输入     
        if (Input.GetKey(KeyCode.UpArrow)) {
            player.transform.position += Vector3.forward;
            SendPos();
        }
        else if (Input.GetKey(KeyCode.DownArrow)) {
            player.transform.position += Vector3.back;
            SendPos();
        }
        else if (Input.GetKey(KeyCode.LeftArrow)) {
            player.transform.position += Vector3.left;
            SendPos();
        }
        else if (Input.GetKey(KeyCode.RightArrow)) {
            player.transform.position += Vector3.right;
            SendPos();
        }
        //分数
        else if (Input.GetKey(KeyCode.Space)) {
            ProtocolBytes proto = new ProtocolBytes();
            proto.AddString("AddScore");
            NetMgr.srvConn.Send(proto);
        }
    }

    void Update() {
        Move();
    }
}
