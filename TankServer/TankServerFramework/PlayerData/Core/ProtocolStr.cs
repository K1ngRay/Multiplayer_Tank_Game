class ProtocolStr : ProtocolBase {
    public string str;
    public override ProtocolBase Decode(byte[] readBuffer, int start, int length) {
        ProtocolStr protocol = new ProtocolStr();
        protocol.str = System.Text.Encoding.UTF8.GetString(readBuffer, start, length);
        return (ProtocolBase)protocol;
    }

    public override byte[] Encode() {
        byte[] b = System.Text.Encoding.UTF8.GetBytes(str);
        return b;
    }

    public override string GetName() {
        if (str.Length == 0) return "";
        return str.Split(',')[0];
    }

    public override string GetDesc() {
        return str;
    }
}

