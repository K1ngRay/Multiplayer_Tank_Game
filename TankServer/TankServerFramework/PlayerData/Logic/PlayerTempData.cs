using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PlayerTempData {
    public PlayerTempData() {
        
    }

    public enum Status {
        None,
        Room,
        Fight,
    }
    public Status status;
    public Room room;
    public int team = 1;
    public bool isOwner = false;
}

