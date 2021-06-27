using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PanelMgr : MonoBehaviour {

    private static PanelMgr instance;
    private GameObject canvas;
    public Dictionary<string, PanelBase> dic;
    private Dictionary<TypeClass.PanelLayer, Transform> layerDic;

    public static PanelMgr Instance {
        get {
            return instance;
        }
    }
    void Awake() {
        instance = this;
        InitLayer();
        dic = new Dictionary<string, PanelBase>();
    }

    private void InitLayer() {
        canvas = GameObject.Find("Canvas");
        if (canvas == null) {
            MyDebug.DebugNull("canvas");
            return;
        }
        layerDic = new Dictionary<TypeClass.PanelLayer, Transform>();
        foreach (TypeClass.PanelLayer item in Enum.GetValues(typeof(TypeClass.PanelLayer))) {
            string name = item.ToString();
            Transform trans = canvas.transform.Find(name);
            if (trans != null)
                layerDic.Add(item, trans);
        }
    }

    public void OpenPanel<T>(string path = "", params object[] args) where T : PanelBase {
        //已经打开
        string name = typeof(T).ToString();
        if (dic.ContainsKey(name)) return;

        PanelBase panel = canvas.AddComponent<T>();
        panel.Init(args);
        dic.Add(name, panel);
        //加载
        path = path != "" ? path : panel.panelPath;
        GameObject panelPrefab = Resources.Load<GameObject>(path);
        if (panelPrefab == null) {
            Debug.LogError("Open Panel Fail,can't find PanelPath:" + path);
            return;
        }
        panel.panelObj = Instantiate(panelPrefab);
        Transform panelTrans = panel.panelObj.transform;
        TypeClass.PanelLayer layer = panel.layer;
        Transform parent = layerDic[layer];
        panelTrans.SetParent(parent, false);
        panel.OnShowing();
        panel.OnShowed();
    }

    public void ClosePanel(string name) {
        PanelBase panel = (PanelBase)dic[name];
        if (panel == null) return;

        panel.OnClosing();
        dic.Remove(name);
        panel.OnClosed();
        GameObject.Destroy(panel.panelObj);
        Component.Destroy(panel);
    }
}
