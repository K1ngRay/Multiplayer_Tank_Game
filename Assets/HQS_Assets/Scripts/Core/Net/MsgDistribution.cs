using System.Collections.Generic;
using UnityEngine;

public class MsgDistribution {
    public int num = 15; //每帧处理消息的数量

    public List<ProtocolBase> msgList = new List<ProtocolBase>();

    public delegate void Delegate(ProtocolBase proto);

    private Dictionary<string, Delegate> eventDic = new Dictionary<string, Delegate>();
    private Dictionary<string, Delegate> onceDic = new Dictionary<string, Delegate>();

    //更新
    public void Update() {
        for (int i = 0; i < num; i++) {
            if (msgList.Count > 0) {
                DispatchMsgEvent(msgList[0]);
                lock (msgList) {
                    msgList.RemoveAt(0);
                }
            }
            else break;
        }
    }

    //消息分发
    public void DispatchMsgEvent(ProtocolBase protocol) {
        if (protocol == null) return;
        string name = protocol.GetName();
        Debug.Log("分发处理消息 " + name);
        if (eventDic.ContainsKey(name)) {
            eventDic[name](protocol);
        }
        if (onceDic.ContainsKey(name)) {
            onceDic[name](protocol);
            onceDic[name] = null;
            onceDic.Remove(name);
        }
    }

    //注册监听
    public void AddListener(string name, Delegate cb) {
        if (eventDic.ContainsKey(name))
            eventDic[name] += cb; //todo:这样不会加入要多委托吗？
        else
            eventDic[name] = cb;
    }

    //添加单词监听
    public void AddOnceListener(string name, Delegate cb) {
        if (eventDic.ContainsKey(name))
            eventDic[name] += cb;
        else
            eventDic[name] = cb;
    }

    //删除监听
    public void DelListener(string name, Delegate cb) {
        if (eventDic.ContainsKey(name)) {
            eventDic[name] -= cb;            
            if (eventDic[name] == null)
                eventDic.Remove(name);
        }
    }

    //删除单次监听
    public void DelOnceListener(string name, Delegate cb) {
        if (onceDic.ContainsKey(name)) {
            onceDic[name] -= cb;
            if (onceDic[name] == null)
                onceDic.Remove(name);
        }
    }
}
