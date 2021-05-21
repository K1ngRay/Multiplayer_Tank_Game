class ProtocolBase {
    //解码器
    public virtual ProtocolBase Decode(byte[] readBuffer, int start, int length) {
        return new ProtocolBase();
    }
    //编码器
    public virtual byte[] Encode() {
        return new byte[] { };
    }
    //协议名字
    public virtual string GetName() {
        return "";
    }
    //协议描述
    public virtual string GetDesc() {
        return "";
    }
}
