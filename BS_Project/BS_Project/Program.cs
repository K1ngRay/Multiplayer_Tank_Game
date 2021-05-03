using System;
using System.Net;
using System.Net.Sockets;

namespace BS_Project {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine("Hello World");
            #region 同步连接
            ////创建Socket
            //Socket listenfd = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
            ////Bind
            //IPAddress ipAdr = IPAddress.Parse("192.168.1.109");
            //IPEndPoint ipEp = new IPEndPoint(ipAdr, 1234);
            //listenfd.Bind(ipEp);
            ////Listen
            //listenfd.Listen(0);
            //Console.WriteLine("[服务器]启动成功");
            //while (true) {
            //    //Accept  因为是同步，所以会阻塞
            //    Socket connfd = listenfd.Accept();  //有客户端接入就创建一个新的socket
            //    Console.WriteLine("[服务器]Accept");
            //    //Recv
            //    byte[] readBuff = new byte[1024];
            //    int count = connfd.Receive(readBuff);  //接收客户端数据
            //    string str = System.Text.Encoding.UTF8.GetString(readBuff, 0, count);
            //    Console.WriteLine("[服务器接收]"+str);
            //    //Send
            //    byte[] bytes = System.Text.Encoding.Default.GetBytes("serv echo : " + str);
            //    connfd.Send(bytes); //发回确认信息
            //}
            #endregion
            #region 异步连接
            Server server = new Server();
            server.Start("127.0.0.1", 1234);
            while (true) {
                string str = Console.ReadLine();
                if (str == "quit")
                    return;
            }
            #endregion
        }
    }
}
