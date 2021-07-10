using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : PanelBase {
    private InputField idIF;
    private InputField pwIF;
    private Button loginBtn;
    private Button registerBtn;
    #region 生命周期
    public override void Init(params object[] args) {
        base.Init(args);
        panelPath = "UI/LoginPanel";
        layer = TypeClass.PanelLayer.Panel;
    }

    public override void OnShowing() {
        base.OnShowing();
        Transform panelTrans = panelObj.transform;
        idIF = panelTrans.Find("IdIF").GetComponent<InputField>();
        pwIF = panelTrans.Find("pwIF").GetComponent<InputField>();
        loginBtn = panelTrans.Find("LoginBtn").GetComponent<Button>();
        registerBtn = panelTrans.Find("RegisterBtn").GetComponent<Button>();

        loginBtn.onClick.AddListener(OnLoginClick);
        registerBtn.onClick.AddListener(OnRegisterClick);
    }
    #endregion
    
    private void OnLoginClick() {
        //前端校验：用户名密码为空
        if(idIF.text == "" || pwIF.text == "") {
            PanelMgr.Instance.OpenPanel<TipsPanel>("", "用户名密码不能为空!");
            return;
        }
        if (NetMgr.srvConn.status != Connection.Status.Connected) {
            string host = "127.0.0.1";
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            if(!NetMgr.srvConn.Connect(host, port)) {
                PanelMgr.Instance.OpenPanel<TipsPanel>("", "连接服务器失败!");
            }
        }
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idIF.text);
        protocol.AddString(pwIF.text);
        Debug.Log("发送 " + protocol.GetDesc());
        //发送Login协议，并注册OnLoginBack
        NetMgr.srvConn.Send(protocol, OnLoginBack);
    }

    private void OnRegisterClick() {
        PanelMgr.Instance.OpenPanel<RegisterPanel>();
        Close();
    }

    private void OnLoginBack(ProtocolBase protocol) {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 0) {
            Debug.Log("登录成功！");
            //开始游戏
            //Walk_two.instance.StartGame(idIF.text);
            PanelMgr.Instance.OpenPanel<RoomListPanel>("");
            GameMgr.Instance.id = idIF.text;
            Close();
        }
        else PanelMgr.Instance.OpenPanel<TipsPanel>("","登录失败!");
    }
}
