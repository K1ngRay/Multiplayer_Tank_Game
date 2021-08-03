using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    private GameObject target;

    [SerializeField]
    private float distance = 8f;
    [SerializeField]
    private float maxDis = 22f;
    [SerializeField]
    private float minDis = 5f;
    [SerializeField]
    private float zoomSpeed = 0.5f;

    [SerializeField]
    private float rot = 0f; //横向旋转角度
    [SerializeField]
    private float rotSpeed = 2f;

    [SerializeField]
    private float roll = 30f * Mathf.PI * 2 / 360; //纵向旋转角度
    [SerializeField]
    private float rollSpeed = 2f;
    [SerializeField]
    private float maxRoll = 38f * Mathf.PI * 2 / 360f;
    [SerializeField]
    private float minRoll = -10f * Mathf.PI * 2 / 360f;
    void Start() {
    }

    // Update is called once per frame
    void LateUpdate() {
        if (target == null) return;
        if (Camera.main == null) return;

        ReFresh();
        Rotate();
        Roll();
        Zoom();
    }

    public void SetTarget(GameObject target) {
        if (target == null) {
            MyDebug.DebugNull("target");
            return;
        }

        this.target = target;
    }

    void ReFresh() {
        Vector3 targetPos = target.transform.position;
        Vector3 cameraPos;
        float d = distance * Mathf.Cos(roll); //水平距离
        float height = distance * Mathf.Sin(roll);
        cameraPos.z = targetPos.z - d * Mathf.Cos(rot);
        cameraPos.x = targetPos.x + d * Mathf.Sin(rot);
        cameraPos.y = targetPos.y + height;
        Camera.main.transform.position = cameraPos;
        Camera.main.transform.LookAt(target.transform);
    }

    void Rotate() {
        float w = Input.GetAxis("Mouse X") * rotSpeed * Time.deltaTime;
        rot -= w;
    }

    void Roll() {
        float w = Input.GetAxis("Mouse Y") * rollSpeed * Time.deltaTime;
        roll -= w;
        if (roll > maxRoll)
            roll = maxRoll;
        else if (roll < minRoll)
            roll = minRoll;
    }

    void Zoom() {
        float w = Input.GetAxis("Mouse ScrollWheel");

        if (w > 0) {
            if (distance > minDis)
                distance -= zoomSpeed;
        }
        else if (w < 0) {
            if (distance < maxDis)
                distance += zoomSpeed;
        }
    }
}
