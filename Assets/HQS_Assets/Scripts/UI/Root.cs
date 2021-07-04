using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour {

    void Start() {
        PanelMgr.Instance.OpenPanel<RoomListPanel>("");
        Application.runInBackground = true;
    }

    void Update() {
        NetMgr.Update();
    }

}
