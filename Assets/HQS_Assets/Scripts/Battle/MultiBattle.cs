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

    void Start () {
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
    public bool IsSameCamp(GameObject tank1,GameObject tank2) {
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
            int spawnID = proto.GetInt(start, ref start);
            GenerateTank(id, team, spawnID);
        }
        NetMgr.srvConn.msgDist.AddListener("UpdateUnitInfo", RecvUpdateUnitInfo);
        //NetMgr.srvConn.msgDist.AddListener("Shooting", RecvShooting);
        //NetMgr.srvConn.msgDist.AddListener("Hit", RecvHit);
        //NetMgr.srvConn.msgDist.AddListener("Result", RecvResult);
    }
    
    //产生坦克
    public void GenerateTank(string id,int team,int spawnID) {
        //获取出生点
        Transform sp = GameObject.Find("SpawnPoints").transform;
        Transform spawnPoint;
        if (team == 1) {
            Transform teamSpawnPoint = sp.GetChild(0);
            spawnPoint = teamSpawnPoint.GetChild(spawnID - 1);
        }
        else if(team == 2) {
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
        dict.Add(id, bt);

        //玩家处理
        if(id == GameMgr.Instance.id) {
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
        string protoName = proto.GetString(start,ref start);
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
}

