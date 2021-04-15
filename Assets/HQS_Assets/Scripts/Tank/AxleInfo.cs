using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxleInfo {
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor; //指明是否将发动机的马力传送给轴上的轮子
    public bool steering; //指明轮子是否转向，汽车都是前轮转向，因此前轴的steering为true，后轴的steering为false
}
