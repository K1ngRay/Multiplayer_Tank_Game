using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PlayerTempData {
    public PlayerTempData() {
        
    }

    //房间相关
    public enum Status {
        None,
        Room,
        Fight,
    }
    public Status status;
    public Room room;
    public int team = 1;
    public bool isOwner = false;

    //战场相关
    public long lastUpdateTime;
    public long lastShootTime;
    public float posX;
    public float posY;
    public float posZ;
    public float hp = 100;
}

