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
        if (count1 <= count2) {
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

    //房间能否开战
    public bool CanStart() {
        if (status != Status.Prepare)
            return false;

        int count1 = 0;
        int count2 = 0;
        int count3 = 0;

        foreach (Player item in dict.Values) {
            if (item.tempData.team == 1) count1++;
            if (item.tempData.team == 2) count2++;
            if (item.tempData.team == 3) count3++;
        }

        if (count1 + count2 + count3 < 2)
            return false;

        return true;
    }

    public void StartFight() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Fight");
        status = Status.Fight; //修改房间状态
        int teamPos1 = 1; //todo:为什么都要初始为1
        int teamPos2 = 1;
        int teamPos3 = 1;
        lock (dict) {
            protocol.AddInt(dict.Count);
            foreach (var item in dict.Values) {
                item.tempData.hp = 200;
                protocol.AddString(item.id);
                protocol.AddInt(item.tempData.team);
                protocol.AddFloat(item.tempData.hp);
                if (item.tempData.team == 1)
                    protocol.AddInt(teamPos1++);
                else if (item.tempData.team == 2)
                    protocol.AddInt(teamPos2++);
                else protocol.AddInt(teamPos3++);

                item.tempData.status = PlayerTempData.Status.Fight;
            }
            Broadcast(protocol);
        }
    }

    private int IsWin() {
        if (status != Status.Fight)
            return 0;

        int count1 = 0;
        int count2 = 0;
        int count3 = 0;
        foreach (var player in dict.Values) {
            PlayerTempData pt = player.tempData;
            if (pt.team == 1 && pt.hp > 0) count1++;
            if (pt.team == 2 && pt.hp > 0) count2++;
            if (pt.team == 3 && pt.hp > 0) count3++;
        }
        if (count2 <= 0 && count3 <= 0 && count1 > 0) return 1;
        if (count1 <= 0 && count3 <= 0 && count2 > 0) return 2;
        if (count1 <= 0 && count2 <= 0 && count3 > 0) return 3;
        return 0;
    }

    public void UpdateWin() {
        int isWin = IsWin();
        if (isWin == 0)
            return;
        
        //有一方胜利
        lock (dict) {
            status = Status.Prepare; //改变房间状态
            foreach (Player player in dict.Values) {
                player.tempData.status = PlayerTempData.Status.Room;
                if (player.tempData.team == isWin)
                    player.data.win++;
                else
                    player.data.fail++;
            }
        }

        //广播
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Result");
        protocol.AddInt(isWin);
        Broadcast(protocol);
    }

    //中途退出战斗
    public void ExitFight(Player player) {
        //摧毁坦克
        if (dict[player.id] != null)
            dict[player.id].tempData.hp = -1;
        //模拟客户端Hit协议
        //广播消息
        ProtocolBytes protoRet = new ProtocolBytes();
        protoRet.AddString("Hit");
        protoRet.AddString(player.id);
        protoRet.AddString(player.id); //自己杀死自己，所以id一致
        protoRet.AddFloat(float.MaxValue);
        Broadcast(protoRet);
        //添加失败次数
        if (IsWin() == 0)
            player.data.fail++;

        //胜负判断
        UpdateWin();
    }
}

