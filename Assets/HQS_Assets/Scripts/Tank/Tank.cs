using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tank : MonoBehaviour {

    public Transform turret;
    [SerializeField]
    private float turretRotSpeed = 0.5f;
    private float turretRotTarget = 0f;
    private float turretRollTarget = 0f;


    public Transform gun;
    private float maxRoll = 10f;
    private float minRoll = -2f;

    public List<AxleInfo> axleInfos;

    //马力
    private float motor = 0;
    //最大马力
    public float maxMotorTorque;

    //制动
    private float brakeTorque;
    //最大制动
    public float maxBrakeTorque = 100;

    //转向角
    private float steering;
    //最大转向角
    public float maxSteeringAngle;

    //轮子和履带
    public List<Transform> wheelObjects;
    public List<SkinnedMeshRenderer> trackObjects;

    //音频
    public AudioSource motorAS;

    //开炮
    public GameObject bullet;
    public float lastShootTime = 0f;
    public float shootInterval = 0.5f;

    //控制类型
    public TypeClass.CtrlType ctrlType = TypeClass.CtrlType.Player;

    //血量
    public int hp = 100;
    public int maxHp = 100;
    public GameObject destroyEff;
    //血量GUI
    public Texture2D hpBarBg;
    public Texture2D hpBar;

    //准心GUI
    public Texture2D centerSight;
    public Texture2D tankSight;

    void Update() {
        PlayerCtrl();
        TankCtrl(axleInfos);

        TurretRotate(turret);
        TurretRoll(turret, gun);

        MotorSound(motorAS);
    }

    void OnGUI() {
        if (ctrlType != TypeClass.CtrlType.Player) return;
        DrawSight();
        DrawHp();
    }

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

    // 玩家输入控制
    void PlayerCtrl() {
        if (ctrlType != TypeClass.CtrlType.Player) return;
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        if (Input.GetMouseButton(0))
            Shoot(bullet, gun);

        TargetPos(turret);
        //turretRotTarget = Camera.main.transform.eulerAngles.y;
        //turretRollTarget = Camera.main.transform.eulerAngles.x;
    }

    // 坦克控制
    void TankCtrl(List<AxleInfo> axleInfos) {
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
            brakeTorque = 0;
            //前进时刹车
            if (item.leftWheel.rpm > 5 && motor < 0)
                brakeTorque = maxBrakeTorque;
            //后退时刹车
            else if (item.leftWheel.rpm < -5 && motor > 0)
                brakeTorque = maxBrakeTorque;
            item.leftWheel.brakeTorque = brakeTorque;
            item.rightWheel.brakeTorque = brakeTorque;
        }
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

    void Shoot(GameObject bullet, Transform gun) {
        if (bullet == null) {
            MyDebug.DebugNull("bullet");
            return;
        }

        if (Time.time - lastShootTime < shootInterval)
            return;

        Vector3 pos = gun.position + gun.forward * 5;
        string name = bullet.GetComponent<PoolObject>().objName;
        ObjectPool.Pop(name, bullet, pos, gun.rotation);
        lastShootTime = Time.time;
    }

    public void BeAttacked(int att) {
        if (hp <= 0) return;
        if (hp > 0) hp -= att;
        if (hp <= 0) {
            ctrlType = TypeClass.CtrlType.None;
            if (destroyEff == null) {
                MyDebug.DebugNull("destroyEff");
                return;
            }
            destroyEff.SetActive(true);
        }
    }

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
        }
        else hitPoint = ray.GetPoint(400f);

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
            Screen.height /2 - centerSight.height / 2,
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
}
