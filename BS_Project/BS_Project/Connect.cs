using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace BS_Project {
    public class Connect {
        public const int BUFFER_SIZE = 1024;
        public Socket socket;
        public bool isUse = false;
        public byte[] readBuffer = new byte[BUFFER_SIZE];
        public int bufferCount = 0;

        public Connect() {
            readBuffer = new byte[BUFFER_SIZE];
        }

        public void Init(Socket socket) {
            this.socket = socket;
            isUse = true;
            bufferCount = 0;
        }

        //缓冲区剩余的字节数
        public int BufferRemain() {
            return BUFFER_SIZE - bufferCount;
        }

        //获取客户端地址
        public string GetAddress() {
            if (!isUse) {
                return "无法获取地址";
            }
            return socket.RemoteEndPoint.ToString();//获取客户端地址和端口
        }

        public void Close() {
            if (!isUse)
                return;
            Console.WriteLine("[断开连接]"+GetAddress());
            socket.Close();
            isUse = false;
        }
    }
}
