using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour {

    void Start() {
        PanelMgr.Instance.OpenPanel<LoginPanel>("");
        Application.runInBackground = true;
    }

    void Update() {
        NetMgr.Update();
    }

}
