using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : PanelBase {

    private Button bg;
    private Transform panel;
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
            int result = (int)args[0];
            isWin = (result == 1);
        }
    }

    public override void OnShowing() {
        base.OnShowing();
        Transform panelTrans = panelObj.transform;
        bg = panelTrans.Find("Bg").GetComponent<Button>();
        panel = panelTrans.Find("Panel");
        closeBtn = panel.Find("CloseBtn").GetComponent<Button>();
        winImg= panel.Find("WinImg").GetComponent<Image>();
        failImg = panel.Find("FailImg").GetComponent<Image>();
        resultTxt= panel.Find("ResultTxt").GetComponent<Text>();

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

        closeBtn.onClick.AddListener(OnClickClose);
        bg.onClick.AddListener(OnClickClose);
    }

    private void OnClickClose() {
        //PanelMgr.Instance.OpenPanel<TitlePanel>("");
        MultiBattle.Instance.ClearBattle();
        PanelMgr.Instance.OpenPanel<RoomPanel>("");
        Close();
    }
}
