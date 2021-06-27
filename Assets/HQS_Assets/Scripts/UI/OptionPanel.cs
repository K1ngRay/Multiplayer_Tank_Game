using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionPanel : PanelBase {

    private Button startBtn;
    private Dropdown redDrd;
    private Dropdown blueDrd;
    private Dropdown greenDrd;
    private Button closeBtn;

    public override void Init(params object[] args) {
        base.Init(args);
        panelPath = "UI/OptionPanel";
        layer = TypeClass.PanelLayer.Panel;
    }

    public override void OnShowing() {
        base.OnShowing();
        Transform panelTrans = panelObj.transform;
        startBtn = panelTrans.Find("StartBtn").GetComponent<Button>();
        closeBtn = panelTrans.Find("CloseBtn").GetComponent<Button>();
        redDrd = panelTrans.Find("RedDrd").GetComponent<Dropdown>();
        blueDrd = panelTrans.Find("BlueDrd").GetComponent<Dropdown>();
        greenDrd = panelTrans.Find("GreenDrd").GetComponent<Dropdown>();

        startBtn.onClick.AddListener(OnStartClick);
        closeBtn.onClick.AddListener(OnCloseClick);
        redDrd.onValueChanged.AddListener(OnRedValueChanged);
        blueDrd.onValueChanged.AddListener(OnBlueValueChanged);
        greenDrd.onValueChanged.AddListener(OnGreenValueChanged);
    }

    private void OnStartClick() {
        int t1 = redDrd.value + 1;
        int t2 = blueDrd.value + 1;
        int t3 = greenDrd.value;
        Battle.Instance.SetTankNumber(t1, t2, t3);
        Battle.Instance.StartThreeCampBattle();
        PanelMgr.Instance.ClosePanel("TitlePanel");
        Close();
    }

    private void OnCloseClick() {
        Close();
    }

    private void OnRedValueChanged(int i) {
        redDrd.captionText.text = "我军数量：" + (i + 1).ToString();
    }
    private void OnBlueValueChanged(int i) {
        blueDrd.captionText.text = "蓝军数量：" + (i + 1).ToString();
    }
    private void OnGreenValueChanged(int i) {
        greenDrd.captionText.text = "绿军数量：" + (i).ToString();
    }

}
