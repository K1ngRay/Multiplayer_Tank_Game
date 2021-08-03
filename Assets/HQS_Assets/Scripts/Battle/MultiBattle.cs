using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiBattle : MonoBehaviour {

    private static MultiBattle instance;

    public GameObject[] tankPrefabs;

    public Dictionary<string, BattleTank> dict = new Dictionary<string, BattleTank>();

    public static MultiBattle Instance {
        get {
            return instance;
        }
    }

    void Start() {
        if (instance == null)
            instance = this;
    }

    //获取阵营 返回0表示错误
    public int GetCamp(GameObject tankObj) {
        foreach (BattleTank item in dict.Values) {
            if (item.tank.gameObject == tankObj)
                return item.camp;
        }
        return 0;
    }

    //是否同一阵营
    public bool IsSameCamp(GameObject tank1, GameObject tank2) {
        return GetCamp(tank1) == GetCamp(tank2);
    }

    //清理场景
    public void ClearBattle() {
        dict.Clear();
        GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tank");
        for (int i = 0; i < tanks.Length; i++) {
            Destroy(tanks[i]);
        }
    }

    public void StartBattle(ProtocolBytes proto) {
        //解析协议
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        if (protoName != "Fight")
            return;

        int count = proto.GetInt(start, ref start);
        //清除场景
        ClearBattle();

        for (int i = 0; i < count; i++) {
            string id = proto.GetString(start, ref start);
            int team = proto.GetInt(start, ref start);
            float initHp = proto.GetFloat(start, ref start);
            int spawnID = proto.GetInt(start, ref start);
            GenerateTank(id, team, initHp, spawnID);
        }
        NetMgr.srvConn.msgDist.AddListener("UpdateUnitInfo", RecvUpdateUnitInfo);
        NetMgr.srvConn.msgDist.AddListener("Shooting", RecvShooting);
        NetMgr.srvConn.msgDist.AddListener("Hit", RecvHit);
        NetMgr.srvConn.msgDist.AddListener("Result", RecvResult);
    }

    //产生坦克
    public void GenerateTank(string id, int team, float initHp, int spawnID) {
        //获取出生点
        Transform sp = GameObject.Find("SpawnPoints").transform;
        Transform spawnPoint;
        if (team == 1) {
            Transform teamSpawnPoint = sp.GetChild(0);
            spawnPoint = teamSpawnPoint.GetChild(spawnID - 1);
        }
        else if (team == 2) {
            Transform teamSpawnPoint = sp.GetChild(1);
            spawnPoint = teamSpawnPoint.GetChild(spawnID - 1);
        }
        else {
            Transform teamSpawnPoint = sp.GetChild(2);
            spawnPoint = teamSpawnPoint.GetChild(spawnID - 1);
        }
        if (spawnPoint == null) {
            Debug.LogError("GenerateTank: 出生点错误");
            return;
        }
        if (tankPrefabs.Length < 3) {
            Debug.LogError("坦克预设数量不够");
            return;
        }

        //创建坦克
        GameObject tankObj = Instantiate(tankPrefabs[team - 1]);
        tankObj.name = id;
        tankObj.transform.position = spawnPoint.position;
        tankObj.transform.rotation = spawnPoint.rotation;

        //列表处理
        BattleTank bt = new BattleTank();
        bt.tank = tankObj.GetComponent<Tank>();        
        bt.camp = team;
        bt.tank.hp = initHp;
        bt.tank.maxHp = initHp;
        dict.Add(id, bt);

        //玩家处理
        if (id == GameMgr.Instance.id) {
            bt.tank.ctrlType = TypeClass.CtrlType.Player;
            CameraFollow cf = Camera.main.gameObject.GetComponent<CameraFollow>();
            GameObject target = bt.tank.CameraPoint;
            cf.SetTarget(target);
        }
        else {
            bt.tank.ctrlType = TypeClass.CtrlType.Net;
            bt.tank.InitNetCtrl(); //初始化网络同步
        }
    }

    private void RecvUpdateUnitInfo(ProtocolBase protocol) {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        Vector3 nextPos;
        Vector3 nextRot;
        nextPos.x = proto.GetFloat(start, ref start);
        nextPos.y = proto.GetFloat(start, ref start);
        nextPos.z = proto.GetFloat(start, ref start);
        nextRot.x = proto.GetFloat(start, ref start);
        nextRot.y = proto.GetFloat(start, ref start);
        nextRot.z = proto.GetFloat(start, ref start);
        float turretY = proto.GetFloat(start, ref start);
        float gunX = proto.GetFloat(start, ref start);
        //处理
        Debug.Log("RecvUpdateUnitInfo " + id);
        if (!dict.ContainsKey(id)) {
            Debug.Log("RecvUpdateUnitInfo bt == null");
            return;
        }
        if (id == GameMgr.Instance.id) //跳过自己的同步
            return;
        BattleTank bt = dict[id];
        bt.tank.NetForecastInfo(nextPos, nextRot);
        bt.tank.NetTurretTarget(turretY, gunX);
    }

    private void RecvShooting(ProtocolBase protocol) {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        string id = proto.GetString(start, ref start);
        Vector3 pos;
        Vector3 rot;
        pos.x = proto.GetFloat(start, ref start);
        pos.y = proto.GetFloat(start, ref start);
        pos.z = proto.GetFloat(start, ref start);
        rot.x = proto.GetFloat(start, ref start);
        rot.y = proto.GetFloat(start, ref start);
        rot.z = proto.GetFloat(start, ref start);

        //处理
        if (!dict.ContainsKey(id)) {
            Debug.Log("RecvShooting bt == null");
            return;
        }
        BattleTank bt = dict[id];
        if (id == GameMgr.Instance.id) {
            return;
        }
        bt.tank.NetShoot(pos, rot);
    }

    private void RecvHit(ProtocolBase protocol) {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        string attId = proto.GetString(start, ref start);
        string defId = proto.GetString(start, ref start);
        float hurt = proto.GetFloat(start, ref start);
        //获取BatlleTank
        if (!dict.ContainsKey(attId)) {
            Debug.Log("RecvHit attBt = null" + attId);
            return;
        }
        BattleTank attBt = dict[attId];
        if (!dict.ContainsKey(defId)) {
            Debug.Log("RecvHit defBt = null " + defId);
            return;
        }
        BattleTank defBt = dict[defId];
        defBt.tank.NetBeAttacked(hurt, attBt.tank);
    }

    private void RecvResult(ProtocolBase protocol) {
        //解析协议
        int start = 0;
        ProtocolBytes proto = (ProtocolBytes)protocol;
        string protoName = proto.GetString(start, ref start);
        int winTeam = proto.GetInt(start, ref start);

        //弹出胜负面板
        string id = GameMgr.Instance.id;
        BattleTank bt = dict[id];
        if (bt.camp == winTeam) {
            PanelMgr.Instance.OpenPanel<ResultPanel>("", 1);
        }
        else {
            PanelMgr.Instance.OpenPanel<ResultPanel>("", 0);
        }

        //取消监听
        NetMgr.srvConn.msgDist.DelListener("UpdateUnitInfo", RecvUpdateUnitInfo);
        NetMgr.srvConn.msgDist.DelListener("Shooting", RecvShooting);
        NetMgr.srvConn.msgDist.DelListener("Hit", RecvHit);
        NetMgr.srvConn.msgDist.DelListener("Result", RecvResult);
    }
}

