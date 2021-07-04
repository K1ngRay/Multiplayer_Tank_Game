using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : PanelBase {

    private List<Transform> prefabs = new List<Transform>();
    private Transform listPanel;
    private Transform contentTrans;
    private Button closeBtn;
    private Button startBtn;

    public override void Init(params object[] args) {
        base.Init(args);
        panelPath = "UI/RoomPanel";
        layer = TypeClass.PanelLayer.Panel;
    }
    public override void OnShowing() {
        base.OnShowing();
        //组件
        Transform panelTrans = panelObj.transform;
        listPanel = panelTrans.Find("ListPanel");
        contentTrans = listPanel.Find("Content");
        //最多9个玩家
        for (int i = 0; i < 9; i++) {
            string name = "PlayerPrefab" + i.ToString();
            Transform prefab = contentTrans.Find(name);
            prefabs.Add(prefab);
        }
        closeBtn = listPanel.Find("CloseBtn").GetComponent<Button>();
        startBtn = listPanel.Find("StartBtn").GetComponent<Button>();

        closeBtn.onClick.AddListener(OnCloseClick);
        startBtn.onClick.AddListener(OnStartClick);

        //监听
        NetMgr.srvConn.msgDist.AddListener("GetRoomInfo", RecvGetRoomInfo);
        NetMgr.srvConn.msgDist.AddListener("Fight", RecvFight);

        //发送查询
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomInfo");
        NetMgr.srvConn.Send(protocol);

        //测试
        ProtocolBytes a = new ProtocolBytes();
        a.AddString("GetRoomInfo");
        a.AddInt(3);

        a.AddString("Killer");
        a.AddInt(1);
        a.AddInt(15);
        a.AddInt(18);
        a.AddInt(0);

        a.AddString("FireGod");
        a.AddInt(2);
        a.AddInt(15);
        a.AddInt(18);
        a.AddInt(0);

        a.AddString("123131");
        a.AddInt(3);
        a.AddInt(15);
        a.AddInt(18);
        a.AddInt(1);
        RecvGetRoomInfo(a);
    }

    public override void OnClosing() {
        NetMgr.srvConn.msgDist.DelListener("GetRoomInfo", RecvGetRoomInfo);
        NetMgr.srvConn.msgDist.DelListener("Fight", RecvFight);
    }


    private void OnCloseClick() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("LeaveRoom");
        NetMgr.srvConn.Send(protocol, OnCloseBack);
    }

    private void OnStartClick() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartFight");
        NetMgr.srvConn.Send(protocol, OnStartBack);
    }

    private void RecvGetRoomInfo(ProtocolBase protocol) {
        //获取总数
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        int i = 0;
        for (; i < count; i++) {
            string name = proto.GetString(start, ref start);
            int camp = proto.GetInt(start, ref start);
            int win = proto.GetInt(start, ref start);
            int fail = proto.GetInt(start, ref start);
            int identity = proto.GetInt(start, ref start);

            //信息处理
            Transform trans = prefabs[i];
            Transform infoTrans = trans.Find("InfoObj");
            Transform waitTrans = trans.Find("WaitObj");
            infoTrans.gameObject.SetActive(true);
            waitTrans.gameObject.SetActive(false);
            Text nameTxt = infoTrans.Find("NameTxt").GetComponent<Text>();
            Text campTxt = infoTrans.Find("CampTxt").GetComponent<Text>();
            Text gradeTxt = infoTrans.Find("GradeTxt").GetComponent<Text>();
            Text identityTxt = infoTrans.Find("IdentityTxt").GetComponent<Text>();

            nameTxt.text = "名字:" + name;
            campTxt.text = "阵营:";
            if (camp == 1) {
                campTxt.text += "红";
                infoTrans.GetComponent<Image>().color = new Color(1, 0, 0, 0.7f);
            }
            else if (camp == 2) {
                campTxt.text += "蓝";
                infoTrans.GetComponent<Image>().color = new Color(0, 0, 1, 0.7f);
            }
            else {
                campTxt.text += "绿";
                infoTrans.GetComponent<Image>().color = new Color(0, 1, 0, 0.7f);
            }
            gradeTxt.text = "胜利:" + win.ToString() + "  失败:" + fail.ToString();
            if (identity == 1) {
                identityTxt.text = "[房主]";
            }
            else if (GameMgr.Instance.id == name) {
                identityTxt.text = "[我自己]";
            }
            else identityTxt.text = "";
        }
        for (; i < 9; i++) {
            Transform trans = prefabs[i];
            Transform infoTrans = trans.Find("InfoObj");
            Transform waitTrans = trans.Find("WaitObj");
            infoTrans.gameObject.SetActive(false);
            waitTrans.gameObject.SetActive(true);
            waitTrans.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        }
    }

    private void RecvFight(ProtocolBase protocol) {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        //todo:多人战斗开始
        Close();
    }

    private void OnCloseBack(ProtocolBase protocol) {
        //解析
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 0) {
            Debug.Log("退出房间成功");
            PanelMgr.Instance.OpenPanel<RoomListPanel>("");
            Close();
        }
        else {
            PanelMgr.Instance.OpenPanel<TipsPanel>("", "退出失败!");
        }
    }

    private void OnStartBack(ProtocolBase protocol) {
        //解析
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        //处理
        if(ret != 0) {
            PanelMgr.Instance.OpenPanel<TipsPanel>("", "开始游戏失败!");
        }
    }
}
