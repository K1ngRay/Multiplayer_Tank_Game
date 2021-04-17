using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Path {

    public Vector3[] waypoints;     //路点集合
    public Vector3 curWaypoint;
    public int lastPoint = -1; //上一次选择的路点，用于避免AI先后选择同一路点 
    private int index = -1; //当前路径上各点的索引
    public float deviation = 5; //误差
    public bool isLoop = false;
    public bool isFinish = false;

    public bool IsReach(Transform trans) {
        Vector3 pos = trans.position;
        float dist = Vector3.Distance(curWaypoint, pos);
        return dist < deviation;
    }

    public void NextWaypoint() {
        if (index < 0) return;
        if (index < waypoints.Length - 1)
            index++;
        else {
            if (isLoop) index = 0;
            else
                isFinish = true;
        }
        curWaypoint = waypoints[index];
    }

    public void InitByObj(GameObject obj, bool isLoop = false) {
        int length = obj.transform.childCount;
        if (length == 0) {
            waypoints = null;
            index = -1;
            Debug.LogWarning("Path.InitByObj length==0");
            return;
        }
        waypoints = new Vector3[length];
        for (int i = 0; i < length; i++) {
            Transform trans = obj.transform.GetChild(i);
            waypoints[i] = trans.position;
        }
        index = 0;
        curWaypoint = waypoints[index];
        this.isLoop = isLoop;
        isFinish = false;
    }

    public void InitByNavMeshPath(Vector3 pos, Vector3 targetPos, bool isLoop = false) {
        waypoints = null;
        index = -1;
        //计算路径
        NavMeshPath navPath = new NavMeshPath();
        bool hasFoundPath = NavMesh.CalculatePath(pos, targetPos, NavMesh.AllAreas, navPath);
        if (!hasFoundPath) return;

        int length = navPath.corners.Length;
        waypoints = new Vector3[length];
        for (int i = 0; i < length; i++) {
            waypoints[i] = navPath.corners[i];
        }
        index = 0;
        curWaypoint = waypoints[index];
        //this.isLoop = isLoop;
        isFinish = false;
    }

    public void DrawWaypoints() {
        if (waypoints == null) return;
        int length = waypoints.Length;
        
        for (int i = 0; i < length; i++) {
            if (i == index) {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(waypoints[i], 1);
            }
            else {
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(waypoints[i], Vector3.one);
            }
            if (i > 0) {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(waypoints[i - 1], waypoints[i]);
            }
        }
        if (isLoop) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(waypoints[length - 1], waypoints[0]);
        }
    }
}
