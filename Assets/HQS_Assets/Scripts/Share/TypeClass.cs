using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeClass {
    public enum CtrlType {
        Death,
        Player,
        Computer,
        Net
    }

    public enum FSMStatus {
        Patrol,
        Attack
    }

    public enum PanelLayer {
        Panel,
        Tips,
    }
}
