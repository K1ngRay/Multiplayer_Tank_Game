using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour {

    [SerializeField]
    private float rotSpeed = 20f;
    [SerializeField]
    private float moveSpeed = 3f;

    public Transform turret;
    [SerializeField]
    private float turretRotSpeed = 0.5f;
    private float turretRotTarget = 0f;
    private float turretRollTarget = 0f;


    public Transform gun;
    private float maxRoll = 10f;
    private float minRoll = -2f;

    void Update () {
        Move();
        Rotate();

        turretRotTarget = Camera.main.transform.eulerAngles.y;
        turretRollTarget= Camera.main.transform.eulerAngles.x;
        TurretRotate();
        TurretRoll();
    }

    void Move() {
        float y = Input.GetAxis("Vertical");
        Vector3 s = y * transform.forward * moveSpeed * Time.deltaTime;
        transform.position += s;
    }

    void Rotate() {
        float x = Input.GetAxis("Horizontal");
        transform.Rotate(0f, x * rotSpeed * Time.deltaTime, 0f);
    }

    void TurretRotate() {
        if (Camera.main == null) return;
        if (turret == null) return;

        float angle = turret.eulerAngles.y - turretRotTarget;
        if (angle < 0) angle += 360;
        if (angle > turretRotSpeed && angle < 180)
            turret.Rotate(0f, -turretRotSpeed, 0f);
        else if (angle > 180 && angle < 360-turretRotSpeed)
            turret.Rotate(0f, turretRotSpeed, 0f);
    }

    void TurretRoll() {
        if (Camera.main == null) return;
        if (turret == null) return;
        if (gun == null) return;

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

}
