using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : PanelBase {
    private Button closeBtn;

    public override void Init(params object[] args) {
        base.Init(args);
        panelPath = "InfoPanel";
        layer = TypeClass.PanelLayer.Panel;
    }

    public override void OnShowing() {
        base.OnShowing();
        Transform panelTrans = panelObj.transform;
        closeBtn = panelTrans.Find("CloseBtn").GetComponent<Button>();

        closeBtn.onClick.AddListener(OnCloseClick);
    }

    private void OnCloseClick() {
        Close();
    }
}
