using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI : MonoBehaviour {

    private Tank target;
    public float sightDistance = 30f;
    private float lastSearchTargetTime = 0f;
    [SerializeField]
    private float searchTargetInterval = 3f;

    private float lastUpdateWaypointTime = float.MinValue;
    [SerializeField]
    private float updateWaypointInterval = 3f;

    [HideInInspector]
    public Tank tank;

    private TypeClass.FSMStatus status = TypeClass.FSMStatus.Patrol;
    private Path path = new Path();
    private GameObject WaypointContainer;

    private void Start() {
        InitWaypoint();
    }

    void InitWaypoint() {
        WaypointContainer = GameObject.Find("WaypointContainer");
        //if (obj)
        //    path.InitByObj(obj,true);
        int index = Random.Range(0, WaypointContainer.transform.childCount);
        path.lastPoint = index;
        if (WaypointContainer != null && WaypointContainer.transform.GetChild(index) != null) {
            Vector3 targetPos = WaypointContainer.transform.GetChild(index).position;
            path.InitByNavMeshPath(transform.position, targetPos, true);
        }
    }

    private void ChangeStatus(TypeClass.FSMStatus status) {
        if (tank == null) {
            MyDebug.DebugNull("tank");
            return;
        }
        if (tank.ctrlType != TypeClass.CtrlType.Computer)
            return;

        switch (status) {
            case TypeClass.FSMStatus.Patrol:
                PatrolStart();
                break;
            case TypeClass.FSMStatus.Attack:
                AttackStart();
                break;
            default:
                break;
        }
    }

    void Update() {
        if (tank == null) {
            MyDebug.DebugNull("tank");
            return;
        }
        if (tank.ctrlType != TypeClass.CtrlType.Computer)
            return;

        //搜寻目标
        TargetUpdate();
        //更新路点
        if (path != null && path.IsReach(transform))
            path.NextWaypoint();

        switch (status) {
            case TypeClass.FSMStatus.Patrol:
                PatrolUpdate();
                break;
            case TypeClass.FSMStatus.Attack:
                AttackUpdate();
                break;
            default:
                break;
        }
    }

    void PatrolStart() {

    }

    void AttackStart() {
        Vector3 dir = Vector3.Normalize(target.transform.position - transform.position);
        Vector3 targetPos = target.transform.position - dir * 10f;
        path.InitByNavMeshPath(transform.position, targetPos);
    }

    void PatrolUpdate() {
        if (target != null)
            ChangeStatus(TypeClass.FSMStatus.Attack);
        float interval = Time.time - lastUpdateWaypointTime;
        if (interval < updateWaypointInterval) return;
        if (path.waypoints == null || path.isFinish || interval > 20f) {
            lastUpdateWaypointTime = Time.time;
            if (WaypointContainer == null)
                WaypointContainer = GameObject.Find("WaypointContainer");

            int count = WaypointContainer.transform.childCount;
            if (count == 0) return;
            int index = Random.Range(0, count);
            Vector3 targetPos = WaypointContainer.transform.GetChild(index).position;
            if (index == path.lastPoint) index = (index + 1) % count;
            path.InitByNavMeshPath(transform.position, targetPos);
        }
    }

    void AttackUpdate() {
        //目标丢失
        if (target == null)
            ChangeStatus(TypeClass.FSMStatus.Patrol);
        float interval = Time.time - lastUpdateWaypointTime;
        if (interval < updateWaypointInterval) return;
        lastUpdateWaypointTime = Time.time;
        //对方可能会移动，需要不停更新路径
        Vector3 dir = Vector3.Normalize(target.transform.position - transform.position);
        Vector3 targetPos = target.transform.position - dir * 10f;
        path.InitByNavMeshPath(transform.position, targetPos);
    }

    //搜寻目标
    void TargetUpdate() {
        //cd
        float interval = Time.time - lastSearchTargetTime;
        if (interval < searchTargetInterval) return;

        lastSearchTargetTime = Time.time;

        if (target != null) HasTarget();
        else NoTarget();
    }

    //已有目标，判断是否丢失目标
    void HasTarget() {
        Vector3 pos = transform.position;
        Vector3 targetPos = target.transform.position;

        if (target.ctrlType == TypeClass.CtrlType.Death) {
            target = null;
        }
        else if (Vector3.Distance(pos, targetPos) > sightDistance) {
            target = null;
        }
    }

    //没有目标，主动搜索视野范围内的坦克
    void NoTarget() {
        //优先寻找生命值最少的
        int minHP = int.MaxValue;
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Tank");
        for (int i = 0; i < targets.Length; i++) {
            Tank targetTank = targets[i].GetComponent<Tank>();
            if (targetTank == null) continue; //非坦克
            if (targets[i] == gameObject) continue; //自己
            if (Battle.Instance.IsSameCamp(this.tank, targetTank)) continue; //自己人
            if (targetTank.ctrlType == TypeClass.CtrlType.Death) continue; //死亡
            Vector3 pos = this.transform.position;
            Vector3 targetPos = targets[i].transform.position;
            if (Vector3.Distance(pos, targetPos) > sightDistance) continue; //距离过远

            if (minHP > targetTank.hp) {
                target = targetTank;
                minHP = targetTank.hp;
            }
        }
    }

    //被动搜索，被攻击后，锁定对方为目标
    public void OnAttacked(Tank attackTank) {
        if (Battle.Instance.IsSameCamp(this.tank, attackTank))
            return;
        target = attackTank;
    }

    public Vector3 GetTurretTarget() {
        //没目标时，看正前方
        if (target == null) {
            float y = transform.eulerAngles.y;
            Vector3 rot = new Vector3(0f, y, 0f);
            return rot;
        }
        else {
            Vector3 pos = transform.position;
            Vector3 targetPos = target.transform.position;
            Vector3 vec = targetPos - pos;
            return Quaternion.LookRotation(vec).eulerAngles;
        }
    }

    //默认60度内都是可射击范围，返回true时，AI就开炮
    public bool IsShoot() {
        if (target == null) return false;

        //与目标的角度差
        float turretRot = tank.Turret.eulerAngles.y;
        float angle = turretRot - GetTurretTarget().y;
        if (angle < 0) angle += 360;
        if (angle < 30 || angle > 330) return true;
        else return false;
    }

    //获取转角
    public float GetSteering() {
        if (tank == null) return 0;
        Vector3 itp = transform.InverseTransformPoint(path.curWaypoint);
        if (itp.x > 1)
            return tank.MaxSteeringAngle;
        else if (itp.x < -1)
            return -tank.MaxSteeringAngle;
        else return 0;
    }

    //获取马力
    public float GetMotor() {
        if (tank == null) return 0;
        if (path.isFinish) return 0;
        Vector3 itp = transform.InverseTransformPoint(path.curWaypoint);
        float x = itp.x;
        float z = itp.z;
        float r = 6f;

        //射线检测前方有无障碍物，若有则后退
        RaycastHit hit;
        Vector3 pos = transform.position + Vector3.up;
        Ray ray = new Ray(pos, transform.forward);
        Debug.DrawLine(pos, pos + transform.forward * 5f, Color.red);
        if (Physics.Raycast(ray, out hit, 5f)) {
            path.InitByNavMeshPath(transform.position, transform.position - transform.forward * 15f);
            return -tank.MaxMotorTorque;
        }

        //仅仅把z=0作为前进后退的分界点，会出现坦克在z=0附近前后徘徊
        //故不需要倒退太多，只让x的绝对值>z的绝对值且x>半径6的范围允许后退
        if (z < 0 && Mathf.Abs(x) < -z && Mathf.Abs(x) < r)
            return -tank.MaxMotorTorque;

        else return tank.MaxMotorTorque;
    }

    public void ClearPath() {
        path.waypoints = null;
    }

    //获取制动
    public float GetBrakeTorque() {
        if (path.isFinish)
            return tank.MaxMotorTorque;
        else return 0;
    }

    private void OnDrawGizmos() {
        path.DrawWaypoints();
    }
}
