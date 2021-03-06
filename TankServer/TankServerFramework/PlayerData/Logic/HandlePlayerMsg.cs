using System;
partial class HandlePlayerMsg {
    public void MsgGetScore(Player player, ProtocolBase protoBase) {
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("GetScore");
        protocolRet.AddInt(player.data.score);
        player.Send(protocolRet);
        Console.WriteLine("MsgGetScore " + player.id + " " + player.data.score);
    }

    public void MsgAddScore(Player player, ProtocolBase protoBase) {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start); //todo:为什么这就可以获取到数值

        player.data.score += 1;
        Console.WriteLine("MsgAddScore " + player.id + " " + player.data.score.ToString());
    }

    //获取玩家列表
    public void  MsgGetList(Player player,ProtocolBase protoBase) {
        Scene.instance.SendPlayerList(player);
    }

    //更新信息
    public void MsgUpdateInfo(Player player,ProtocolBase protoBase) {
        //获取数值
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        float x = protocol.GetFloat(start, ref start); //todo:凭什么可以获取到
        float y = protocol.GetFloat(start, ref start);
        float z = protocol.GetFloat(start, ref start);
        int score = player.data.score;
        Scene.instance.UpdateInfo(player.id, x, y, z, score);

        //广播
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("UpdateInfo");
        protocolRet.AddString(player.id);
        protocolRet.AddFloat(x);
        protocolRet.AddFloat(y);
        protocolRet.AddFloat(z);
        protocolRet.AddInt(score);
        ServNet.instance.Broadcast(protocolRet);
    }

    public void MsgGetAchieve(Player player, ProtocolBase protocol) {
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("GetAchieve");
        protocolRet.AddInt(player.data.win);
        protocolRet.AddInt(player.data.fail);
        player.Send(protocolRet);
        Console.WriteLine("MsgGetAchieve "+player.id + " " + player.data.win);
    }
}
