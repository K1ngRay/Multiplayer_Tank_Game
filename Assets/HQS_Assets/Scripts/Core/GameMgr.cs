using UnityEngine;

public class GameMgr : MonoBehaviour {

    private static GameMgr instance;

    public static GameMgr Instance {
        get {
            return instance;
        }
    }

    public string id = "Tank";

    private void Awake() {
        if (instance == null)
            instance = this;
    }
}
