using UnityEngine.UI;
using UnityEngine;

public class RoomListPanel : PanelBase {
    private Transform gradePanel;
    private Text playerNameTxt;
    private Text winTxt;
    private Text loseTxt;

    private Transform listPanel;
    private Transform contentTrans;
    private GameObject roomPrefab;
    private Transform btnObj;
    private Button newBtn;
    private Button refreshBtn;

    private Button closeBtn;

    public override void Init(params object[] args) {
        base.Init();
        panelPath = "UI/RoomListPanel";
        layer = TypeClass.PanelLayer.Panel;
    }

    public override void OnShowing() {
        //获取组件
        base.OnShowing();
        Transform panelTrans = panelObj.transform;
        gradePanel = panelTrans.Find("GradePanel");
        playerNameTxt = gradePanel.Find("Text").GetComponent<Text>();
        winTxt = gradePanel.Find("WinCountTxt").GetComponent<Text>();
        loseTxt = gradePanel.Find("LoseCountTxt").GetComponent<Text>();

        listPanel = panelTrans.Find("ListPanel");
        contentTrans = listPanel.Find("Scroll View").Find("Viewport").Find("Content");
        roomPrefab = contentTrans.Find("RoomPrefab").gameObject;
        btnObj = listPanel.Find("BtnObj");
        newBtn = btnObj.Find("NewBtn").GetComponent<Button>();
        refreshBtn = btnObj.Find("RefreshBtn").GetComponent<Button>();

        closeBtn = panelTrans.Find("CloseBtn").GetComponent<Button>();

        roomPrefab.SetActive(false);
        newBtn.onClick.AddListener(OnNewClick);
        refreshBtn.onClick.AddListener(OnRefreshClick);
        closeBtn.onClick.AddListener(OnCloseClick);

        //监听
        NetMgr.srvConn.msgDist.AddListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.AddListener("GetRoomList", RecvGetRoomList);

        //发送查询
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        NetMgr.srvConn.Send(protocol);

        protocol = new ProtocolBytes();
        protocol.AddString("GetAchieve");
        NetMgr.srvConn.Send(protocol);

        //测试
        ProtocolBytes a = new ProtocolBytes();
        a.AddString("GetRoomInfo");
        a.AddInt(3);

        a.AddInt(1);
        a.AddInt(2);

        a.AddInt(3);
        a.AddInt(1);

        a.AddInt(3);
        a.AddInt(1);
        RecvGetRoomList(a);

    }

    public override void OnClosing() {
        NetMgr.srvConn.msgDist.DelListener("GetAchieve", RecvGetAchieve);
        NetMgr.srvConn.msgDist.DelListener("GetRoomList", RecvGetRoomList);
    }

    private void OnCloseClick() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Logout");
        NetMgr.srvConn.Send(protocol, OnCloseBack);
    }

    private void OnNewClick() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("CreateRoom");
        NetMgr.srvConn.Send(protocol, OnNewBack);
    }

    private void OnRefreshClick() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        NetMgr.srvConn.Send(protocol);
    }

    private void RecvGetAchieve(ProtocolBase protocol) {
        //解析协议
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int win = proto.GetInt(start, ref start);
        int lose = proto.GetInt(start, ref start);
        //处理
        playerNameTxt.text = "指挥官：" + GameMgr.Instance.id;
        winTxt.text = win.ToString();
        loseTxt.text = lose.ToString();
    }

    private void RecvGetRoomList(ProtocolBase protocol) {
        //清空
        ClearRoomUnit();
        //解析协议
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int count = proto.GetInt(start, ref start);
        for (int i = 0; i < count; i++) {
            int num = proto.GetInt(start, ref start);
            int status = proto.GetInt(start, ref start);
            GenerateRoomUnit(i, num, status);
        }
    }

    private void ClearRoomUnit() {
        for (int i = 0; i < contentTrans.childCount; i++) {
            if (contentTrans.GetChild(i).name.Contains("Clone")) {
                Destroy(contentTrans.GetChild(i).gameObject);
            }
        }
    }

    /// <summary>
    /// 创建房间单元
    /// </summary>
    /// <param name="i">房间序号</param>
    /// <param name="num">房间内玩家人数</param>
    /// <param name="status">房间状态，1-准备中 2-战斗中</param>
    private void GenerateRoomUnit(int i,int num,int status) {
        //添加房间
        contentTrans.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, (i+1) * 150f);
        GameObject clone = Instantiate(roomPrefab);
        clone.transform.SetParent(contentTrans);
        clone.transform.localScale = Vector3.one;
        clone.SetActive(true);

        //房间信息
        Transform trans = clone.transform;
        Text idTxt = trans.Find("IdTxt").GetComponent<Text>();
        Text countTxt = trans.Find("CountTxt").GetComponent<Text>();
        Text statusTxt = trans.Find("StatusTxt").GetComponent<Text>();
        Button joinBtn = trans.Find("JoinBtn").GetComponent<Button>();

        idTxt.text = "序号：" + (i + 1).ToString();
        countTxt.text = "人数：" + num.ToString();
        if(status == 1) {
            statusTxt.text = "状态：准备中";
            statusTxt.color = Color.black;
        }
        else {
            statusTxt.text = "状态：战斗中";
            statusTxt.color = Color.red;
        }
        joinBtn.onClick.AddListener(delegate () {
            OnJoinClick(i.ToString());
        });
    }

    public void OnJoinClick(string roomId) {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("EnterRoom");

        protocol.AddInt(int.Parse(roomId));
        NetMgr.srvConn.Send(protocol, OnJoinClickBakc);
        Debug.Log("请求加入房间 " + roomId);
    }

    public void OnJoinClickBakc(ProtocolBase protocol) {
        //解析协议
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 0) {
            PanelMgr.Instance.OpenPanel<RoomPanel>("");
            Close();
        }
        else {
            PanelMgr.Instance.OpenPanel<TipsPanel>("", "进入房间失败");
        }
    }

    public void OnNewBack(ProtocolBase protocol) {
        //解析参数
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        //处理
        if(ret == 0) {
            Debug.Log("创建房间成功");
            PanelMgr.Instance.OpenPanel<RoomPanel>("");
            Close();
        }
        else {
            PanelMgr.Instance.OpenPanel<TipsPanel>("", "创建房间失败");
        }
    }

    public void OnCloseBack(ProtocolBase protocol) {
        PanelMgr.Instance.OpenPanel<TipsPanel>("", "登出成功!");
        PanelMgr.Instance.OpenPanel<LoginPanel>("");
        NetMgr.srvConn.Close();
    }
}
