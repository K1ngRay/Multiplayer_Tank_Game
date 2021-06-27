using UnityEngine.UI;
using UnityEngine;

public class TitlePanel : PanelBase {

    private Button startBtn;
    private Button infoBtn;
    private Button settingBtn;

    public override void Init(params object[] args) {
        base.Init(args);
        panelPath = "UI/TitlePanel";
        layer = TypeClass.PanelLayer.Panel;
    }

    public override void OnShowing() {
        base.OnShowing();
        Transform panelTrans = panelObj.transform;
        startBtn = panelTrans.Find("StartBtn").GetComponent<Button>();
        infoBtn = panelTrans.Find("InfoBtn").GetComponent<Button>();
        settingBtn = panelTrans.Find("SettingBtn").GetComponent<Button>();

        startBtn.onClick.AddListener(OnStartClick);
        infoBtn.onClick.AddListener(OnInfoClick);
        settingBtn.onClick.AddListener(OnSettingClick);
    }

    private void OnStartClick() {
        Battle.Instance.StartThreeCampBattle();
        Close();
    }

    private void OnInfoClick() {
        PanelMgr.Instance.OpenPanel<InfoPanel>("");
    }

    private void OnSettingClick() {
        PanelMgr.Instance.OpenPanel<OptionPanel>();
    }
}
