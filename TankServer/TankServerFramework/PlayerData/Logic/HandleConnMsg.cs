using System;
public partial class HandleConnMsg {
    public void MsgHeartBeat(Conn conn,ProtocolBase protoBase) {
        conn.lastTickTime = Sys.GetTimeStamp();
        Console.WriteLine("[更新心跳时间]"+conn.GetAddress());
    }

    public void MsgRegister(Conn conn, ProtocolBase protoBase) {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        string id = protocol.GetString(start, ref start);
        string pw = protocol.GetString(start, ref start);
        string strFormat = "[收到注册协议]" + conn.GetAddress();
        Console.WriteLine(strFormat + " 用户名:" + id + " 密码:" + pw);
        protocol = new ProtocolBytes();
        protocol.AddString("Register");

        if (DataMgr.instance.Register(id,pw)) {
            protocol.AddInt(0);
        }
        else {
            protocol.AddInt(-1);
        }
        //创建角色
        DataMgr.instance.CreatePlayer(id);
        //返回协议给客户端
        conn.Send(protocol);
    }

    public void MsgLogin(Conn conn,ProtocolBase protoBase) {
        int start = 0;
        ProtocolBytes protocol = (ProtocolBytes)protoBase;
        string protoName = protocol.GetString(start, ref start);
        string id = protocol.GetString(start, ref start);
        string pw = protocol.GetString(start, ref start);
        string strFormat = "[收到登录协议]" + conn.GetAddress();
        Console.WriteLine(strFormat + " 用户名:" + id + " 密码:" + pw);

        //构建返回协议
        ProtocolBytes protocolRet = new ProtocolBytes();
        protocolRet.AddString("Login");

        //验证
        if (!DataMgr.instance.CheckPassword(id, pw)) {
            protocolRet.AddInt(-1);
            conn.Send(protocolRet);
            return;
        }

        //是否已经登录
        ProtocolBytes protoLogout = new ProtocolBytes();
        protoLogout.AddString("Logout");
        if (!Player.KickOff(id,protoLogout)) {
            protocolRet.AddInt(-1);
            conn.Send(protocolRet);
            return;
        }

        //获取玩家数据
        PlayerData playerData = DataMgr.instance.GetPlayerData(id);
        if (playerData == null) {
            protocolRet.AddInt(-1);
            conn.Send(protocolRet);
            return;
        }
        conn.player = new Player(id, conn);
        conn.player.data = playerData;
        //事件触发
        ServNet.instance.handlePlayerEvent.OnLogin(conn.player);
        //返回
        protocolRet.AddInt(0);
        conn.Send(protocolRet);
        return;
    }

    public void MsgLogout(Conn conn,ProtocolBase protoBase) {
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Logout");
        protocol.AddInt(0);
        if (conn.player == null) {
            conn.Send(protocol);
            conn.Close();
        }
        else {
            conn.Send(protocol);
            conn.player.Logout();
        }
    }

}
