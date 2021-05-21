using System;

class Player {
    //id
    public string id;
    //连接
    public Conn conn;
    //玩家数据
    public PlayerData data;
    //临时数据
    public PlayerTempData tempData;

    public Player(string id,Conn conn) {
        this.id = id;
        this.conn = conn;
        tempData = new PlayerTempData();
    }

    public void Send(ProtocolBase proto) {
        if (conn == null) return;
        ServNet.instance.Send(conn, proto);
    }

    /// <summary>
    /// 踢下线
    /// </summary>
    /// <param name="id">指明到踢下线的玩家id</param>
    /// <param name="proto">指明要给玩家发送怎样的消息</param>
    /// <returns></returns>
    public static bool KickOff(string id,ProtocolBase proto) {
        Conn[] conns = ServNet.instance.conns;
        for (int i = 0; i < conns.Length; i++) {
            if (conns[i] == null) continue;
            if (!conns[i].isUse) continue;
            if (conns[i].player == null) continue;
            if (conns[i].player.id == id) {
                lock (conns[i].player) {
                    if (proto != null)
                        conns[i].player.Send(proto);

                    return conns[i].player.Logout();
                }
            }
        }
        return true;
    }

    public bool Logout() {
        //事件处理
        //
        //保存
        if (!DataMgr.instance.SavePlayer(this))
            return false;
        //下线
        conn.player = null;
        conn.Close();
        return true;
    }
}
