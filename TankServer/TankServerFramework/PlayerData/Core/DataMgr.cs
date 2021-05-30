using System;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary; //序列化需要的引用
using System.IO;

class DataMgr {
    MySqlConnection sqlConn;

    //单例
    public static DataMgr instance;
    public DataMgr() {
        instance = this;
        Connect();
    }

    private void Connect() {
        string connStr = "Database=game;DataSource=127.0.0.1;" +
            "User Id=root;Password=rootroot;port=3306";
        sqlConn = new MySqlConnection(connStr);
        try {
            sqlConn.Open();
        }
        catch (Exception e) {
            Console.WriteLine("[DataMgr]Connect Error:" + e.Message);
        }
    }

    //判断用户是否存在
    private bool CanRegister(string id) {
        if (!IsSafeStr(id)) {
            Console.WriteLine("[DataMgr]CanRegister " + id + "含有非法字符");
            return false;
        }
        //查询id是否存在
        string cmdStr = string.Format("select * from user where id ='{0}';", id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        try {
            MySqlDataReader dataReader = cmd.ExecuteReader(); //todo:这个函数干什么用的
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return !hasRows;
        }
        catch (Exception e) {
            Console.WriteLine("[DataMgr]CanRegister fail " + e.Message);
            return false;
        }
    }

    //注册
    public bool Register(string id, string pw) {
        if (!IsSafeStr(id) || !IsSafeStr(pw)) {
            Console.WriteLine("[DateMgr]使用非法字符");
            return false;
        }
        if (!CanRegister(id)) {
            Console.WriteLine("[DataMgr]Register 已注册");
            return false;
        }
        string cmdStr = string.Format("insert into user set id = '{0}',password = '{1}';", id, pw);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        try {
            cmd.ExecuteNonQuery(); //todo:这个呢？干嘛用的
            return true;
        }
        catch (Exception e) {
            Console.WriteLine("[DataMgr]Register fail " + e.Message);
            return false;
        }
    }

    //防止注入
    private bool IsSafeStr(string str) {
        return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
    }

    public bool CreatePlayer(string id) {
        if (!IsSafeStr(id)) {
            Console.WriteLine("[DataMgr]CanRegister " + id + "含有非法字符");
            return false;
        }

        IFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        PlayerData playerData = new PlayerData();
        try {
            formatter.Serialize(stream, playerData); //将初始数据序列化
        }
        catch (Exception e) {
            Console.WriteLine("[DataMgr]CreatePlayer 序列化"+e.Message);
            return false;
        }
        byte[] byteArr = stream.ToArray(); //将流文件转化为二进制数组

        //写入数据库
        //@data代表参数名，程序会从cmd的参数表中找到名为 @data的参数并填入sql语句中
        string cmdStr = string.Format("insert into player set id = '{0}',data = @data;", id);

        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        cmd.Parameters.Add("@data", MySqlDbType.Blob);
        cmd.Parameters[0].Value = byteArr;
        try {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e) {
            Console.WriteLine("[DataMgr]CreatePlayer 写入" + e.Message);
            return false;
        }
    }

    public bool CheckPassword(string id,string pw) {
        if (!IsSafeStr(id) || !IsSafeStr(pw))
            return false;

        string cmdStr = string.Format("select * from user where id = '{0}' and password = '{1}';", id, pw);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        try {
            MySqlDataReader dataReader = cmd.ExecuteReader();
            bool hasRows = dataReader.HasRows;
            dataReader.Close();
            return hasRows;
        }
        catch (Exception e) {
            Console.WriteLine("[DataMgr]CheckPassword "+e.Message);
            return false;
        }
    }

    public PlayerData GetPlayerData(string id) {
        if (!IsSafeStr(id)) return null;
        PlayerData playerData = null;
        string cmdStr = string.Format("select * from player where id = '{0}';", id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        byte[] buffer = new byte[1];
        try {
            MySqlDataReader dataReader = cmd.ExecuteReader();
            if (!dataReader.HasRows) {
                dataReader.Close();
                return playerData;
            }
            dataReader.Read();
            long len = dataReader.GetBytes(1, 0, null, 0, 0); //1是data
            buffer = new byte[len];
            dataReader.GetBytes(1, 0, buffer, 0, (int)len);
            dataReader.Close();
        }
        catch (Exception e) {
            Console.WriteLine("[DataMgr]GetPlayerData "+e.Message);
            return playerData;
        }
        //反序列化
        MemoryStream stream = new MemoryStream(buffer);
        try {
            BinaryFormatter formatter = new BinaryFormatter();
            playerData = (PlayerData)formatter.Deserialize(stream);
            return playerData;
        }
        catch (SerializationException e) {
            Console.WriteLine("[DataMgr]GetPlayerData "+e.Message);
            return playerData;
        }
    }

    public bool SavePlayer(Player player) {
        string id = player.id;
        PlayerData playerData = player.data;
        //序列化
        IFormatter formatter = new BinaryFormatter();
        MemoryStream stream = new MemoryStream();
        try {
            formatter.Serialize(stream, playerData);
        }
        catch (Exception e) {
            Console.WriteLine("[DataMgr]SavePlayer 序列化" + e.Message);
        }
        byte[] byteArr = stream.ToArray();
        //写入数据库
        string formatStr = "update player set data=@data where id = '{0}';";
        string cmdStr = string.Format(formatStr, player.id);
        MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
        cmd.Parameters.Add("@data", MySqlDbType.Blob);
        cmd.Parameters[0].Value = byteArr;
        try {
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception e) {
            Console.WriteLine("[DataMgr]SavePlayer 写入"+e.Message);
            return false;
        }
    }
}
