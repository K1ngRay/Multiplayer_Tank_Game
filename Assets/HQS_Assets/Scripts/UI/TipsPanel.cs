using UnityEngine;
using UnityEngine.UI;

public class TipsPanel : PanelBase {
    private Transform panel;
    private Button bgBtn;
    private Text text;
    private Button knowBtn;
    private Button closeBtn;

    private string str;
    public override void Init(params object[] args) {
        base.Init();
        panelPath = "UI/TipsPanel";
        layer = TypeClass.PanelLayer.Tips;
        //参数arg[1]代表提示的内容
        if(args.Length == 1) {
            str = (string)args[0];
        }
    }

    public override void OnShowing() {
        base.OnShowing();
        Transform panelTrans = panelObj.transform;
        bgBtn = panelTrans.Find("BgBtn").GetComponent<Button>();
        panel = panelTrans.Find("Panel");
        text = panel.Find("ContentTxt").GetComponent<Text>();
        knowBtn = panel.Find("KnowBtn").GetComponent<Button>();
        closeBtn = panel.Find("CloseBtn").GetComponent<Button>();

        text.text = str;
        bgBtn.onClick.AddListener(OnCloseClick);
        knowBtn.onClick.AddListener(OnCloseClick);
        closeBtn.onClick.AddListener(OnCloseClick);
    }

    private void OnCloseClick() {
        Close();
    }
}
