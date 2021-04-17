﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Battle : MonoBehaviour {
    [Serializable]
    public struct Camp {
        public Transform[] spawnPoints;
        public int number;
        public Color color;
    }
    [SerializeField]
    private Camp[] camps;
    public BattleTank[] battleTanks;
    public GameObject[] tankPrefabs;

    private static Battle instance;

    public static Battle Instance {
        get {
            return instance;
        }
    }

    void Start() {
        if (instance == null)
            instance = this;
        if (camps.Length == 2) {
            StartThreeCampBattle(camps[0].number, camps[1].number, 0);
        }
        else if (camps.Length == 3) {
            StartThreeCampBattle(camps[0].number, camps[1].number, camps[2].number);
        }
    }

    // Update is called once per frame
    void Update() {

    }

    void StartThreeCampBattle(int t1, int t2, int t3 = 0) {
        ClearBattle();
        battleTanks = new BattleTank[t1 + t2 + t3];
        if (camps.Length < 2 || (t3 > 0 && camps.Length < 3)) {
            Debug.Log("阵营数与阵营生成点数不匹配");
            return;
        }
        for (int i = 0; i < camps[0].spawnPoints.Length && i < t1; i++) {
            GenerateTank(1, i, camps[0], i);
        }
        for (int i = 0; i < camps[1].spawnPoints.Length && i < t2; i++) {
            GenerateTank(2, i, camps[1], t1 + i);
        }
        if (t3 > 0) {
            for (int i = 0; i < camps[2].spawnPoints.Length && i < t3; i++) {
                GenerateTank(3, i, camps[2], t1 + t2 + i);
            }
        }
        CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
        if (cf == null) {
            MyDebug.DebugNull("cf");
            return;
        }
        Tank player = battleTanks[0].tank;
        player.ctrlType = TypeClass.CtrlType.Player;
        GameObject cameraPoint = player.CameraPoint;
        cf.SetTarget(cameraPoint);
    }

    public void GenerateTank(int camp, int num, Camp campGroup, int index) {
        Transform trans = campGroup.spawnPoints[num];
        if (trans == null) {
            MyDebug.DebugNull("trans");
            return;
        }
        Vector3 pos = trans.position;
        Quaternion rot = trans.rotation;
        GameObject prefab = tankPrefabs[camp - 1];

        //不采用对象池，因为坦克属性较多，重置属性较麻烦
        GameObject tankObj = Instantiate(prefab, pos, rot);
        Tank tmpTank = tankObj.GetComponent<Tank>();
        tmpTank.ctrlType = TypeClass.CtrlType.Computer;
        battleTanks[index] = new BattleTank();
        battleTanks[index].tank = tmpTank;
        battleTanks[index].camp = camp;
    }

    public int GetCamp(Tank tank) {
        for (int i = 0; i < battleTanks.Length; i++) {
            BattleTank battleTank = battleTanks[i];
            if (battleTank == null) return 0;
            if (battleTank.tank == tank)
                return battleTank.camp;
        }
        return 0;
    }

    public bool IsSameCamp(Tank tank1, Tank tank2) {
        return GetCamp(tank1) == GetCamp(tank2);
    }

    public bool IsWin(int camp) {
        for (int i = 0; i < battleTanks.Length; i++) {
            Tank tank = battleTanks[i].tank;
            if (battleTanks[i].camp != camp)
                if (tank.hp > 0 || tank.ctrlType != TypeClass.CtrlType.Death)
                    return false;
        }
        Debug.Log(camp + "阵营获胜");
        return true;
    }

    public bool IsWin(Tank tank) {
        int camp = GetCamp(tank);
        return IsWin(camp);
    }

    public void ClearBattle() {
        //不使用对象池，因为重新初始化坦克很麻烦
        GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tank");
        for (int i = 0; i < tanks.Length; i++) {
            Destroy(tanks[i]);
        }
    }

    private void OnDrawGizmos() {
        DrawSpawnPoint();
    }
    void DrawSpawnPoint() {
        for (int i = 0; i < camps.Length; i++) {
            for (int j = 0; j < camps[i].spawnPoints.Length; j++) {
                Transform trans = camps[i].spawnPoints[j];
                if (trans != null) {
                    camps[i].color.a = 1;
                    Gizmos.color = camps[i].color;
                    Gizmos.DrawSphere(trans.position, 1f);
                }
            }
        }
    }
}