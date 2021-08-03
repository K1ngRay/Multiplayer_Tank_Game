using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PoolObject {

    public float speed = 100f;
    public float maxLifeTime = 4f;
    public float instTime = 0f;
    public GameObject explodePrefab;

    public float att = 100;
    public AudioClip explodeClip;

    [HideInInspector]
    public Tank attackTank;

    void OnEnable() {
        instTime = Time.time;
    }

    // Update is called once per frame
    void Update() {
        transform.position += transform.forward * speed * Time.deltaTime;
        if (Time.time - instTime > maxLifeTime) {
            ObjectPool.Push(objName, this.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (explodePrefab == null) {
            MyDebug.DebugNull("explodeEff");
            return;
        }
        if (explodeClip == null) {
            MyDebug.DebugNull("explodeClip");
            return;
        }

        GameObject explodeObj = Instantiate(explodePrefab, transform.position, transform.rotation);   //爆炸音效
        AudioSource audioSource = explodeObj.AddComponent<AudioSource>();
        audioSource.spatialBlend = (Time.time - instTime) * 1.5f;
        audioSource.PlayOneShot(explodeClip);
        ObjectPool.Push(objName, this.gameObject);

        //击中坦克
        Tank tank = collision.gameObject.GetComponent<Tank>();
        if (tank != null) {
            float att = GetAtt();
            //tank.BeAttacked(att, attackTank); //单机
            tank.SendHit(tank.name, att); //联机
        }
    }

    public float GetAtt() {
        float att = this.att - (Time.time - instTime) * 40;
        if (att < 1) att = 1;
        return att;
    }
}
