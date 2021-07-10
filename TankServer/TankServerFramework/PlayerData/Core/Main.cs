﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class MainClass {
    public static void Main(string[] args) {
        DataMgr dataMgr = new DataMgr();
        ServNet servNet = new ServNet();
        servNet.proto = new ProtocolBytes();
        servNet.Start("127.0.0.1", 1234);
        RoomMgr roomMgr = new RoomMgr();
        //Scene scene = new Scene();

        while (true) {
            string str = Console.ReadLine();
            switch (str) {
                case "quit":
                    servNet.Close();
                    return;
                case "print":
                    servNet.Print();
                    break;
                default:
                    break;
            }
        }
    }
}
