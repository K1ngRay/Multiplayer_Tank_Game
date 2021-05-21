using System;
partial class HandleConnMsg {
    public void MsgHeatBeat(Conn conn,ProtocolBase protoBase) {
        conn.lastTickTime = Sys.GetTimeStamp();
        Console.WriteLine("[更新心跳时间]"+conn.GetAddress());
    }
}
