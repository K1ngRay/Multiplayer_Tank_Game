using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : PanelBase {
    private Transform tipBG; 
    private InputField idIF;
    private InputField pwIF;
    private Button registerBtn;
    private Button closeBtn;

    #region 生命周期
    public override void Init(params object[] args) {
        base.Init(args);
        panelPath = "UI/RegisterPanel";
        layer = TypeClass.PanelLayer.Panel;
    }
    public override void OnShowing() {
        base.OnShowing();
        Transform panelTrans = panelObj.transform;
        tipBG = panelTrans.Find("tipBG");
        idIF = tipBG.Find("IdIF").GetComponent<InputField>();
        pwIF = tipBG.Find("pwIF").GetComponent<InputField>();
        registerBtn = tipBG.Find("RegisterBtn").GetComponent<Button>();
        closeBtn = tipBG.Find("CloseBtn").GetComponent<Button>();

        registerBtn.onClick.AddListener(OnRegisterClick);
        closeBtn.onClick.AddListener(OnCloseClick);
    }
    #endregion

    private void OnRegisterClick() {
        //用户名，密码为空
        if (idIF.text == "" || pwIF.text == "") {
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

        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Register");
        protocol.AddString(idIF.text);
        protocol.AddString(pwIF.text);
        Debug.Log("发送 " + protocol.GetDesc());
        NetMgr.srvConn.Send(protocol, OnRegisterBack);
    }

    private void OnCloseClick() {
        PanelMgr.Instance.OpenPanel<LoginPanel>("");
        Close();
    }
    
    private void OnRegisterBack(ProtocolBase protocol) {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 0) {
            PanelMgr.Instance.OpenPanel<TipsPanel>("", "注册成功!");
            PanelMgr.Instance.OpenPanel<LoginPanel>("");
            Close();
        }
        else PanelMgr.Instance.OpenPanel<TipsPanel>("", "注册失败!");

    }
}
