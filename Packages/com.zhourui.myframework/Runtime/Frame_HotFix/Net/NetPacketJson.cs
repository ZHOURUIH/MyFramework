
// 消息基类,字符串形式传输
public class NetPacketJson : NetPacket
{
	// 子类需要重写writeContent和readContent虚函数
	public byte[] write() { return writeContent().toBytes(); }
	public void read(byte[] data, int size) { readContent(data.bytesToString(size)); }
	public virtual string writeContent() { return null; }
	public virtual void readContent(string str) { }
	public override void resetProperty()
	{
		base.resetProperty();
	}
}