using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class MainClass {
    public static void Main(string[] args) {
        #region 测试数据库代码
        //DataMgr dataMgr = new DataMgr();
        ////注册
        //bool ret = dataMgr.Register("HQR", "123");
        //Console.WriteLine("注册{0}", ret ? "成功" : "失败");
        ////创建玩家
        //ret = dataMgr.CreatePlayer("HQR");
        //Console.WriteLine("创建玩家{0}", ret ? "成功" : "失败");
        ////获取玩家数据
        //PlayerData pd = dataMgr.GetPlayerData("HQR");
        //if (pd!=null) 
        //    Console.WriteLine("获取玩家数据成功 分数是："+pd.score);
        //else
        //    Console.WriteLine("获取玩家数据失败");
        ////更改数据
        //pd.score += 10;
        ////保存
        //Player p = new Player();
        //p.id = "HQR";
        //p.data = pd;
        //dataMgr.SavePlayer(p);
        ////重新读取
        //pd = dataMgr.GetPlayerData("HQR");
        //if (pd != null)
        //    Console.WriteLine("获取玩家数据成功 分数是：" + pd.score);
        //else
        //    Console.WriteLine("获取玩家数据失败");
        #endregion
        ServNet servNet = new ServNet();
        servNet.proto = new ProtocolBytes();
        servNet.Start("127.0.0.1", 1234);
        Console.ReadKey();
    }
}
