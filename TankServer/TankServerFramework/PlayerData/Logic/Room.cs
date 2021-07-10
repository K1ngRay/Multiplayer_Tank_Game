using System;
using System.Collections.Generic;
using System.Linq;

public class Room {
    public enum Status {
        Prepare = 1,
        Fight = 2,
    }
    public Status status = Status.Prepare;
    public int maxPlayer = 9;
    public Dictionary<string, Player> dict = new Dictionary<string, Player>();

    public bool AddPlayer(Player player) {
        lock (dict) {
            if (dict.Count >= maxPlayer) {
                return false;
            }

            PlayerTempData tempData = player.tempData;
            tempData.room = this;
            tempData.team = SwitchTeam();
            tempData.status = PlayerTempData.Status.Room;

            if (dict.Count == 0)
                tempData.isOwner = true;
            string id = player.id;
            dict.Add(id, player);
        }
        return true;
    }

    //分配队伍
    public int SwitchTeam() {
        int count1 = 0;
        int count2 = 0;
        int count3 = 0;
        foreach (var item in dict.Values) {
            if (item.tempData.team == 1) count1++;
            if (item.tempData.team == 2) count2++;
            if (item.tempData.team == 3) count3++;
        }
        if(count1 <= count2) {
            if (count3 < count1) return 3;
            else return 1;
        }
        else {
            if (count3 < count2) return 3;
            else return 2;
        }
    }
    
    public void DelPlayer(string id) {
        lock (dict) {
            if (!dict.ContainsKey(id)) return;

            bool isOwner = dict[id].tempData.isOwner;
            dict[id].tempData.isOwner = false;
            dict[id].tempData.status = PlayerTempData.Status.None;
            dict.Remove(id);
            if (isOwner)
                UpdateOwner();
        }
    }

    private void UpdateOwner() {
        lock (dict) {
            if (dict.Count <= 0) return;

            foreach (var item in dict.Values) {
                item.tempData.isOwner = false;
            }
            Player p = dict.Values.First();
            p.tempData.isOwner = true;
        }
    }

    //广播
    public void Broadcast(ProtocolBase protocol) {
        foreach (var item in dict.Values) {
            item.Send(protocol);
        }
    }

    public ProtocolBytes GetRoomInfo() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomInfo");
        //房间信息
        protocol.AddInt(dict.Count);
        //每个玩家的信息
        foreach (var item in dict.Values) {
            protocol.AddString(item.id);
            protocol.AddInt(item.tempData.team);
            protocol.AddInt(item.data.win);
            protocol.AddInt(item.data.fail);
            int isOwner = item.tempData.isOwner ? 1 : 0;
            protocol.AddInt(isOwner);
        }
        return protocol;
    }
}

