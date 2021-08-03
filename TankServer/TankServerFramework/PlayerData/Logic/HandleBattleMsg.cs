using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public partial class HandlePlayerMsg {
    public void MsgStartFight(Player player,ProtocolBase protoBase) {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("StartFight");
        //条件判断
        if (player.tempData.status != PlayerTempData.Status.Room) {
            Console.WriteLine("MsgStartFight status err " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }
        if (!player.tempData.isOwner) {
            Console.WriteLine("MsgStartFight owner err " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }

        Room room = player.tempData.room;
        if (!room.CanStart()) {
            Console.WriteLine("MsgStartFight CanStart err " + player.id);
            protocol.AddInt(-1);
            player.Send(protocol);
            return;
        }

        //开始战斗
        protocol.AddInt(0);
        player.Send(protocol);
        room.StartFight();
    }

    public void MsgUpdateUnitInfo(Player player,ProtocolBase protoBase) {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        float posX = protocol.GetFloat(start, ref start);
        float posY = protocol.GetFloat(start, ref start);
        float posZ = protocol.GetFloat(start, ref start);
        float rotX = protocol.GetFloat(start, ref start);
        float rotY = protocol.GetFloat(start, ref start);
        float rotZ = protocol.GetFloat(start, ref start);
        float gunRot = protocol.GetFloat(start, ref start);
        float gunRoll = protocol.GetFloat(start, ref start);

        //获取房间
        if (player.tempData.status != PlayerTempData.Status.Fight) return;
        Room room = player.tempData.room;

        //作弊校验
        player.tempData.posX = posX;
        player.tempData.posY = posY;
        player.tempData.posZ = posZ;
        player.tempData.lastUpdateTime = Sys.GetTimeStamp();

        //广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("UpdateUnitInfo");
        protocolRet.AddString(player.id);
        protocolRet.AddFloat(posX);
        protocolRet.AddFloat(posY);
        protocolRet.AddFloat(posZ);
        protocolRet.AddFloat(rotX);
        protocolRet.AddFloat(rotY);
        protocolRet.AddFloat(rotZ);
        protocolRet.AddFloat(gunRot);
        protocolRet.AddFloat(gunRoll);
        room.Broadcast(protocolRet);
    }

    public void MsgShooting(Player player,ProtocolBase protoBase) {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        float posX = protocol.GetFloat(start, ref start);
        float posY = protocol.GetFloat(start, ref start);
        float posZ = protocol.GetFloat(start, ref start);
        float rotX = protocol.GetFloat(start, ref start);
        float rotY = protocol.GetFloat(start, ref start);
        float rotZ = protocol.GetFloat(start, ref start);
        //获取房间
        if (player.tempData.status != PlayerTempData.Status.Fight)
            return;

        Room room = player.tempData.room;
        //广播
        ProtocolBytes protoRet = new ProtocolBytes();
        protoRet.AddString("Shooting");
        protoRet.AddString(player.id);
        protoRet.AddFloat(posX);
        protoRet.AddFloat(posY);
        protoRet.AddFloat(posZ);
        protoRet.AddFloat(rotX);
        protoRet.AddFloat(rotY);
        protoRet.AddFloat(rotZ);
        room.Broadcast(protoRet);
    }

    //伤害处理
    public void MsgHit(Player player,ProtocolBase protoBase) {
        //解析协议
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        string enemyName = protocol.GetString(start, ref start);
        float damage = protocol.GetFloat(start, ref start);
        //冷却
        long lastShootTime = player.tempData.lastShootTime;
        if (Sys.GetTimeStamp() - lastShootTime < 0.5f)
            return;

        player.tempData.lastShootTime = Sys.GetTimeStamp();

        if (player.tempData.status != PlayerTempData.Status.Fight)
            return;

        Room room = player.tempData.room;

        //扣除生命值
        if (!room.dict.ContainsKey(enemyName)) {
            Console.WriteLine("MsgHit not Contains (enemyName) "+enemyName);
            return;
        }
        Player enemy = room.dict[enemyName];
        if (enemy == null)
            return;
        if (enemy.tempData.hp <= 0)
            return;
        enemy.tempData.hp -= damage;
        Console.WriteLine("MsgHit " + enemyName + " hp " + enemy.tempData.hp);

        //广播
        ProtocolBytes protoRet = new ProtocolBytes();
        protoRet.AddString("Hit");
        protoRet.AddString(player.id);
        protoRet.AddString(enemy.id);
        protoRet.AddFloat(damage);
        room.Broadcast(protoRet);

        //胜负判断
        room.UpdateWin();
    }
}

