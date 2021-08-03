using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Tank : MonoBehaviour {

    [SerializeField]
    private GameObject cameraPoint;
    [SerializeField]
    private Transform turret;
    [SerializeField]
    private float turretRotSpeed = 0.5f;
    private float turretRotTarget = 0f; //炮塔横向旋转的目标指
    private float turretRollTarget = 0f; //炮口竖向旋转的目标值


    [SerializeField]
    private Transform gun;
    private float maxRoll = 5f;
    private float minRoll = -8f;

    public List<AxleInfo> axleInfos;

    //马力
    private float motor = 0;
    //最大马力
    [SerializeField]
    private float maxMotorTorque;

    //制动
    private float brakeTorque;
    //最大制动
    [SerializeField]
    private float maxBrakeTorque = 100;

    //转向角
    private float steering;
    //最大转向角
    [SerializeField]
    private float maxSteeringAngle;

    //轮子和履带
    [SerializeField]
    private List<Transform> wheelObjects;
    [SerializeField]
    private List<SkinnedMeshRenderer> trackObjects;

    //音频
    [SerializeField]
    private AudioSource motorAS;
    [SerializeField]
    private AudioSource shootAS;

    //开炮
    [SerializeField]
    private GameObject bullet;
    public float shootInterval = 0.5f;
    private float lastShootTime = 0f;

    //控制类型
    public TypeClass.CtrlType ctrlType = TypeClass.CtrlType.Player;

    //血量
    public int hp = 100;
    public int maxHp = 100;
    [SerializeField]
    private GameObject destroyEff;
    //血量GUI
    [SerializeField]
    private Texture2D hpBarBg;
    [SerializeField]
    private Texture2D hpBar;

    //准心GUI
    [SerializeField]
    private Texture2D centerSight;
    [SerializeField]
    private Texture2D[] tankSights;
    private Texture2D tankSight;

    //击杀UI
    [SerializeField]
    private Texture2D killUI;
    private float killTime = float.MinValue;

    [SerializeField]
    private Texture2D arrowUI;

    //AI
    private AI ai;

    //导航障碍
    public NavMeshObstacle navObstacle;

    //网络同步
    private float lastSendInfoTime = float.MinValue;
    private float lastRecvInfoTime = float.MinValue;
    //同步时间间隔
    float delta = 1;
    //last
    private Vector3 lastPos;
    private Vector3 lastRot;
    //forecast
    private Vector3 forePos;
    private Vector3 foreRot;


    public Transform Turret {
        get {
            return turret;
        }
    }

    public float MaxMotorTorque {
        get {
            return maxMotorTorque;
        }
    }

    public float MaxBrakeTorque {
        get {
            return maxBrakeTorque;
        }
    }

    public float MaxSteeringAngle {
        get {
            return maxSteeringAngle;
        }
    }

    public GameObject CameraPoint {
        get {
            return cameraPoint;
        }
    }

    void Start() {
        if (ctrlType == TypeClass.CtrlType.Computer) {
            ai = gameObject.AddComponent<AI>();
            ai.tank = this;
        }
        if (tankSights[0] != null)
            tankSight = tankSights[0];
    }

    void Update() {

        //网络同步
        if (ctrlType == TypeClass.CtrlType.Net) {
            NetUpdate();
            return;
        }

        PlayerCtrl();
        ComputerCtrl();
        DeathCtrl();

        TankMove(axleInfos);
        TurretRotate(turret);
        TurretRoll(turret, gun);

        MotorSound(motorAS);

    }

    void OnGUI() {
        if (ctrlType == TypeClass.CtrlType.Computer) {
            DrawArrowUI(Battle.Instance.GetCamp(this));
        }
        if (ctrlType == TypeClass.CtrlType.Player) {
            DrawSight();
            DrawHp();
            DrawKillUI();
        }
    }

    // 玩家输入控制
    void PlayerCtrl() {
        if (ctrlType != TypeClass.CtrlType.Player) return;
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");

        TargetPos(turret);
        if (Input.GetMouseButton(0))
            Shoot(bullet, gun);
        if (Input.GetKey(KeyCode.Space)) {
            motor = 0;
            steering = 0;
            brakeTorque = maxBrakeTorque;
        }

        //网络同步
        if (Time.time - lastSendInfoTime > 0.2f) {
            SendUnitInfo();

            lastSendInfoTime = Time.time;
        }
    }

    //电脑控制
    void ComputerCtrl() {
        if (ctrlType != TypeClass.CtrlType.Computer) return;
        if (ai == null) {
            MyDebug.DebugNull("ai");
            return;
        }
        Vector3 rot = ai.GetTurretTarget();
        turretRotTarget = rot.y;
        turretRollTarget = rot.x;

        //移动
        steering = ai.GetSteering();
        motor = ai.GetMotor();
        brakeTorque = ai.GetBrakeTorque();
        //开炮
        if (ai.IsShoot())
            Shoot(bullet, gun);
    }

    //死亡下的控制参数
    void DeathCtrl() {
        if (ctrlType != TypeClass.CtrlType.Death) return;

        motor = 0;
        steering = 0;
        brakeTorque = maxBrakeTorque;
        if (navObstacle != null)
            navObstacle.enabled = true;
    }

    #region 坦克逻辑
    // 炮塔旋转
    void TurretRotate(Transform turret) {
        if (Camera.main == null) {
            MyDebug.DebugNull("Camera.main");
            return;
        }
        if (turret == null) {
            MyDebug.DebugNull("turret");
            return;
        }

        float angle = turret.eulerAngles.y - turretRotTarget;
        if (angle < 0) angle += 360;
        if (angle > turretRotSpeed && angle < 180)
            turret.Rotate(0f, -turretRotSpeed, 0f);
        else if (angle > 180 && angle < 360 - turretRotSpeed)
            turret.Rotate(0f, turretRotSpeed, 0f);
    }

    // 炮塔枪口上下移动
    void TurretRoll(Transform turret, Transform gun) {
        if (Camera.main == null) {
            MyDebug.DebugNull("Camera.main");
            return;
        }
        if (turret == null) {
            MyDebug.DebugNull("turret");
            return;
        }
        if (gun == null) {
            MyDebug.DebugNull("gun");
            return;
        }

        Vector3 worldEuler = gun.eulerAngles;
        Vector3 localEuler = gun.localEulerAngles;

        //设置世界坐标系
        worldEuler.x = turretRollTarget;
        gun.eulerAngles = worldEuler;

        //设置本地坐标系
        Vector3 euler = gun.localEulerAngles;
        if (euler.x > 180) euler.x -= 360;
        if (euler.x > maxRoll) euler.x = maxRoll;
        if (euler.x < minRoll) euler.x = minRoll;
        gun.localEulerAngles = new Vector3(euler.x, localEuler.y, localEuler.z);
    }

    // 坦克控制
    void TankMove(List<AxleInfo> axleInfos) {
        if (axleInfos == null) {
            MyDebug.DebugNull("axleInfos");
            return;
        }

        foreach (var item in axleInfos) {
            //转向
            if (item.steering) {
                item.leftWheel.steerAngle = steering;
                item.rightWheel.steerAngle = steering;
            }
            //马力
            if (item.motor) {
                item.leftWheel.motorTorque = motor;
                item.rightWheel.motorTorque = motor;
            }
            //制动
            //前进时刹车
            if (item.leftWheel.rpm > 5 && motor < 0)
                brakeTorque = maxBrakeTorque;
            //后退时刹车
            else if (item.leftWheel.rpm < -5 && motor > 0)
                brakeTorque = maxBrakeTorque;
            item.leftWheel.brakeTorque = brakeTorque;
            item.rightWheel.brakeTorque = brakeTorque;
        }
        brakeTorque = 0;
        //转动轮子履带
        if (axleInfos[1] != null) {
            WheelsRotation(axleInfos[1].leftWheel, wheelObjects);
            TrackMove(trackObjects, wheelObjects, axleInfos);
        }
    }

    // 轮子转动
    void WheelsRotation(WheelCollider collider, List<Transform> wheels) {
        if (wheels == null) {
            MyDebug.DebugNull("wheels");
            return;
        }

        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);

        foreach (var item in wheels) {
            item.rotation = rot;
        }
    }

    // 履带移动
    void TrackMove(List<SkinnedMeshRenderer> tracks, List<Transform> wheels, List<AxleInfo> axleInfos) {
        if (tracks == null) {
            MyDebug.DebugNull("tracks");
            return;
        }
        if (wheels == null) {
            MyDebug.DebugNull("wheels");
            return;
        }
        if (axleInfos == null) {
            MyDebug.DebugNull("axleInfos");
            return;
        }

        float offset = 0f;
        if (wheels[0] != null)
            offset = wheels[0].localEulerAngles.x / 90f;

        foreach (var item in tracks) {
            if (axleInfos[1] != null && Mathf.Abs(axleInfos[0].leftWheel.rpm) > 0.5f) {
                Material mtl = item.material;
                mtl.mainTextureOffset = new Vector2(-offset, 0f);
            }
        }
    }

    void MotorSound(AudioSource motorAS) {
        if (motorAS == null) {
            MyDebug.DebugNull("motorAS");
            return;
        }
        motorAS.spatialBlend = 1;
        if (motor != 0 && !motorAS.isPlaying) {
            motorAS.loop = true;
            if (motorAS.clip != null)
                motorAS.Play();
            else MyDebug.DebugNull("motorAS.clip");
        }
        else if (motor == 0)
            motorAS.Stop();
    }

    void Shoot(GameObject bulletPrefab, Transform gun) {
        if (bulletPrefab == null) {
            MyDebug.DebugNull("bullet");
            return;
        }
        if (gun == null) {
            MyDebug.DebugNull("gun");
            return;
        }
        PoolObject poolObj = bulletPrefab.GetComponent<PoolObject>();
        if (poolObj == null) return;

        if (Time.time - lastShootTime < shootInterval)
            return;

        if (shootAS != null && shootAS.clip != null) {
            shootAS.spatialBlend = 1;
            shootAS.Play();
        }
        Vector3 pos = gun.position + gun.forward * 5;
        GameObject bulletObj = ObjectPool.Pop(poolObj.objName, bulletPrefab, pos, gun.rotation);
        //为炮弹标记自己的信息
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null) {
            bullet.attackTank = this;
        }
        lastShootTime = Time.time;
    }

    public void BeAttacked(int att, Tank attackTank) {
        if (hp <= 0) return;
        if (hp > 0) {
            hp -= att;

            //AI
            if (ai != null && ctrlType == TypeClass.CtrlType.Computer)
                ai.OnAttacked(attackTank);
        }
        if (hp <= 0) {
            ctrlType = TypeClass.CtrlType.Death;
            if (destroyEff == null) {
                MyDebug.DebugNull("destroyEff");
                return;
            }
            destroyEff.SetActive(true);
            if (attackTank == null) {
                MyDebug.DebugNull("attackTank");
                return;
            }
            if (ai != null)
                ai.ClearPath();
            if (!Battle.Instance.IsPlayerFail(this))
                Battle.Instance.IsWin(attackTank);
            if (!Battle.Instance.IsSameCamp(this, attackTank))
                attackTank.StartDrawKill();
        }
    }
    #endregion

    #region UI
    //计算屏幕准心
    void TargetPos(Transform turret) {
        if (turret == null) {
            MyDebug.DebugNull("turret");
            return;
        }

        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;

        //从屏幕中心射出一条射线，获取射线对象
        Vector3 centerVec3 = new Vector3(Screen.width / 2, Screen.height / 2, 0f);
        Ray ray = Camera.main.ScreenPointToRay(centerVec3);

        //射线检测
        if (Physics.Raycast(ray, out hit, 400f)) {
            hitPoint = hit.point;
        }
        else hitPoint = ray.GetPoint(400f);

        Vector3 dir = hitPoint - turret.position;
        Quaternion angle = Quaternion.LookRotation(dir);
        turretRotTarget = angle.eulerAngles.y;
        turretRollTarget = angle.eulerAngles.x;
    }

    //计算炮口准心
    public Vector3 CalExplodePoint(Transform gun) {
        if (gun == null) {
            MyDebug.DebugNull("gun");
            return Vector3.zero;
        }

        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;

        Vector3 pos = gun.position + gun.forward * 5f;
        Ray ray = new Ray(pos, gun.forward);
        if (Physics.Raycast(ray, out hit, 400f)) {
            hitPoint = hit.point;
            Tank targetTank = hit.transform.GetComponent<Tank>();
            if (targetTank != null && !MultiBattle.Instance.IsSameCamp(this.gameObject, targetTank.gameObject) && tankSights[1] != null)
                tankSight = tankSights[1];
            else if (tankSights[0] != null)
                tankSight = tankSights[0];
        }
        else {
            hitPoint = ray.GetPoint(400f);
            if (tankSights[0] != null)
                tankSight = tankSights[0];
        }

        return hitPoint;
    }

    void DrawSight() {
        if (gun == null) {
            MyDebug.DebugNull("gun");
            return;
        }
        if (Camera.main == null) {
            MyDebug.DebugNull("Camera.main");
            return;
        }
        if (tankSight == null) {
            MyDebug.DebugNull("tankSight");
            return;
        }

        Vector3 expPoint = CalExplodePoint(gun);
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(expPoint);

        Rect tankRect = new Rect(
            screenPoint.x - tankSight.width / 2,
            Screen.height - screenPoint.y - tankSight.height / 2,
            tankSight.width,
            tankSight.height);
        GUI.DrawTexture(tankRect, tankSight);

        if (centerSight == null) {
            MyDebug.DebugNull("centerSight");
            return;
        }
        Rect centerRect = new Rect(
            Screen.width / 2 - centerSight.width / 2,
            Screen.height / 2 - centerSight.height / 2,
            centerSight.width,
            centerSight.height);
        GUI.DrawTexture(centerRect, centerSight);
    }

    void DrawHp() {
        if (hpBarBg == null) {
            MyDebug.DebugNull("hpBarBg");
            return;
        }
        Rect bgRect = new Rect(30, Screen.height - hpBarBg.height - 15,
            hpBarBg.width, hpBarBg.height);
        GUI.DrawTexture(bgRect, hpBarBg);

        if (hpBar == null) {
            MyDebug.DebugNull("hpBar");
            return;
        }
        float width = hp * 102 / maxHp;
        Rect hpRect = new Rect(bgRect.x + 29, bgRect.y + 9, width, hpBar.height);
        GUI.DrawTexture(hpRect, hpBar);

        //文字
        string text = hp.ToString() + "/" + maxHp.ToString();
        Rect txtRect = new Rect(bgRect.x + 80, bgRect.y - 10, 50, 50);
        GUI.Label(txtRect, text);
    }

    void StartDrawKill() {
        if (ctrlType == TypeClass.CtrlType.Player)
            killTime = Time.time;
    }
    void DrawKillUI() {
        if (killUI == null) {
            MyDebug.DebugNull("killUI");
            return;
        }
        if (Time.time - killTime < 2f) {
            Rect rect = new Rect(Screen.width / 2 - killUI.width / 2, 30, killUI.width, killUI.height);
            GUI.DrawTexture(rect, killUI);
        }
    }

    void DrawArrowUI(int camp) {
        if (arrowUI == null) {
            MyDebug.DebugNull("arrowUI");
            return;
        }
        Vector3 point = Camera.main.WorldToScreenPoint(cameraPoint.transform.position);
        float distance = Vector3.Distance(Camera.main.transform.position, cameraPoint.transform.position);
        float d = 50f / distance;
        if (d < 0.3f) d = 0.5f;
        else if (d > 1f) d = 1f;
        Rect arrowRect = new Rect(
            point.x - arrowUI.width * d / 2,
            Screen.height - point.y - arrowUI.height * d / 2 - 10f,
            arrowUI.width * d,
            arrowUI.height * d);
        GUI.DrawTexture(arrowRect, arrowUI);
    }

    #endregion

    #region 网络
    public void SendUnitInfo() {
        ProtocolBytes proto = new ProtocolBytes();
        proto.AddString("UpdateUnitInfo");
        //位置旋转
        Vector3 pos = transform.position;
        Vector3 rot = transform.eulerAngles;
        proto.AddFloat(pos.x);
        proto.AddFloat(pos.y);
        proto.AddFloat(pos.z);
        proto.AddFloat(rot.x);
        proto.AddFloat(rot.y);
        proto.AddFloat(rot.z);
        //炮塔
        float angleY = turretRotTarget;
        proto.AddFloat(angleY);
        //炮管
        float angleX = turretRollTarget;
        proto.AddFloat(angleX);
        NetMgr.srvConn.Send(proto);
    }

    //初始化位置预测数据
    public void InitNetCtrl() {
        lastPos = transform.position;
        lastRot = transform.eulerAngles;
        forePos = transform.position;
        foreRot = transform.eulerAngles;
        //对于网络同步，不依赖物理系统
        Rigidbody r = GetComponent<Rigidbody>();
        r.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void NetForecastInfo(Vector3 nextPos, Vector3 curRot) {
        //时间
        delta = Time.time - lastRecvInfoTime;
        //预测的位置
        if (delta > 0.3f) {
            forePos = nextPos;
            foreRot = curRot;
        }
        else {
            forePos = lastPos + (nextPos - lastPos) * 2;
            foreRot = lastRot + (curRot - lastRot) * 2;
        }

        //更新
        lastPos = nextPos;
        lastRot = curRot;
        lastRecvInfoTime = Time.time;
    }

    public void NetUpdate() {
        //当前位置
        Vector3 pos = transform.position;
        Vector3 rot = transform.eulerAngles;
        //更新位置
        if (delta > 0) {
            transform.position = Vector3.Lerp(pos, forePos, delta);
            transform.rotation = Quaternion.Lerp(Quaternion.Euler(rot), Quaternion.Euler(foreRot), delta);
        }

        TurretRotate(turret);
        TurretRoll(turret, gun);
        //轮子履带马达音效
        NetWheelsRotation();
    }

    public void NetTurretTarget(float y, float x) {
        turretRotTarget = y;
        turretRollTarget = x;
    }

    public void NetWheelsRotation() {
        float z = transform.InverseTransformPoint(forePos).z;
        ////判断坦克是否正在移动
        //if (Mathf.Abs(z) < 0.1f || delta <= 0.05f) {
        //    if (motorAS.isPlaying) {
        //        Debug.Log("motorAS.Stop()");
        //        motorAS.Stop();
        //    }
        //    return;
        //}

        //轮子
        foreach (var wheel in wheelObjects) {
            wheel.localEulerAngles += new Vector3(360 * z / delta, 0, 0);
        }
        //履带
        float offset = -wheelObjects[0].localEulerAngles.x / 90f;
        foreach (var track in trackObjects) {
            Material mtl = track.material;
            mtl.mainTextureOffset = new Vector2(0, offset);
        }
        ////声音
        //if (!motorAS.isPlaying) {
        //    motorAS.loop = true;
        //    if (motorAS.clip != null)
        //        motorAS.Play();
        //    Debug.Log("motorAS.Play()");
        //}
    }


    #endregion
}

