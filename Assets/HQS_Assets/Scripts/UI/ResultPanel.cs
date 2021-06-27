using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : PanelBase {

    private Image winImg;
    private Image failImg;
    private Text resultTxt;
    private Button closeBtn;
    private bool isWin;

    public override void Init(params object[] args) {
        base.Init(args);
        panelPath = "UI/ResultPanel";
        layer = TypeClass.PanelLayer.Panel;
        if (args.Length == 1) {
            int camp = (int)args[0];
            isWin = (camp == 1);
        }
    }

    public override void OnShowing() {
        base.OnShowing();
        Transform panelTrans = panelObj.transform;
        closeBtn = panelTrans.Find("CloseBtn").GetComponent<Button>();
        closeBtn.onClick.AddListener(OnClickClose);
        winImg= panelTrans.Find("WinImg").GetComponent<Image>();
        failImg = panelTrans.Find("FailImg").GetComponent<Image>();
        resultTxt= panelTrans.Find("ResultTxt").GetComponent<Text>();

        if (isWin) {
            failImg.enabled = false;
            winImg.enabled = true;
            resultTxt.text = "WIN";
        }
        else {
            failImg.enabled = true;
            winImg.enabled = false;
            resultTxt.text = "LOSE";
        }
    }

    private void OnClickClose() {
        PanelMgr.Instance.OpenPanel<TitlePanel>("");
        Close();
    }
}
