using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PoolObject {

    public float speed = 100f;
    public float maxLifeTime = 2f;
    public float instTime = 0f;
    public GameObject explodeEff;

    public float att = 100;

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
        if (explodeEff == null) {
            MyDebug.DebugNull("explodeEff");
            return;
        }

        Instantiate(explodeEff, transform.position, transform.rotation);
        ObjectPool.Push(objName, this.gameObject);

        //击中坦克
        Tank tank = collision.gameObject.GetComponent<Tank>();
        if (tank != null) {
            int att = GetAtt();
            tank.BeAttacked(att);
        }
    }

    public int GetAtt() {
        float att = this.att - (Time.time - instTime) * 40;
        if (att < 1) att = 1;
        return (int)att;
    }
}
