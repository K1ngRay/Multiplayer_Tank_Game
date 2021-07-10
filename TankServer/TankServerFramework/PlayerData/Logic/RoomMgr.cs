using System;
using System.Collections.Generic;
using System.Linq;

public class RoomMgr {
    public static RoomMgr instance;
    public RoomMgr() {
        if (instance == null)
            instance = this;
    }

    public List<Room> list = new List<Room>();

    public void CreateRoom(Player player) {
        Room room = new Room();
        lock (list) {
            list.Add(room);
            room.AddPlayer(player);
        }
    }

    public void LeaveRoom(Player player) {
        PlayerTempData tempData = player.tempData;
        if (tempData.status == PlayerTempData.Status.None)
            return;

        Room room = tempData.room;
        lock (list) {
            room.DelPlayer(player.id);
            if (room.dict.Count == 0) {
                list.Remove(room);
            }
        }
    }

    //列表
    public ProtocolBytes GetRoomList() {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("GetRoomList");
        int count = list.Count;
        //房间数量
        protocol.AddInt(count);
        for (int i = 0; i < count; i++) {
            Room room = list[i];
            protocol.AddInt(room.dict.Count);
            protocol.AddInt((int)room.status);
        }
        return protocol;
    }

}

