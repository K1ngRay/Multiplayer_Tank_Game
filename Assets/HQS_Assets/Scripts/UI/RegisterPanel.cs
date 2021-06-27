using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : PanelBase {
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
        idIF = panelTrans.Find("IdIF").GetComponent<InputField>();
        pwIF = panelTrans.Find("pwIF").GetComponent<InputField>();
        registerBtn = panelTrans.Find("RegisterBtn").GetComponent<Button>();
        closeBtn = panelTrans.Find("CloseBtn").GetComponent<Button>();

        registerBtn.onClick.AddListener(OnRegisterClick);
        closeBtn.onClick.AddListener(OnCloseClick);
    }
    #endregion

    private void OnRegisterClick() {
        //用户名，密码为空
        if (idIF.text == "" || pwIF.text == "") {
            Debug.Log("用户名密码不能为空!");
            return;
        }
        if (NetMgr.srvConn.status != Connection.Status.Connected) {
            string host = "127.0.0.1";
            int port = 1234;
            NetMgr.srvConn.proto = new ProtocolBytes();
            NetMgr.srvConn.Connect(host, port);
        }

        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Register");
        protocol.AddString(idIF.text);
        protocol.AddString(pwIF.text);
        Debug.Log("发送 " + protocol.GetDesc()); //todo:看看GetDesc的实现
        NetMgr.srvConn.Send(protocol, OnRegisterBack);
    }

    private void OnCloseClick() {
        PanelMgr.Instance.OpenPanel<LoginPanel>();
        Close();
    }
    
    private void OnRegisterBack(ProtocolBase protocol) {
        //todo:看看回调怎么用
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 0) {
            Debug.Log("注册成功!");
            PanelMgr.Instance.OpenPanel<LoginPanel>("");
            Close();
        }
        else Debug.Log("注册失败");
    }
}
