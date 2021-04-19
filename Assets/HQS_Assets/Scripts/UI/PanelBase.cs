using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour {
    [HideInInspector]
    public string panelPath;
    [HideInInspector]
    public GameObject panelObj;
    [HideInInspector]
    public TypeClass.PanelLayer layer;
    public object[] args;

    //生命周期
    public virtual void Init(params object[] args) {
        this.args = args;
    }

    public virtual void OnShowing() {

    }

    public virtual void OnShowed() {

    }

    public virtual void Update() {

    }

    public virtual void OnClosing() {

    }

    public virtual void OnClosed() {

    }

    protected virtual void Close() {
        string name = this.GetType().ToString();
        PanelMgr.Instance.ClosePanel(name);
    }

}
