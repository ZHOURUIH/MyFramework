
// 消息基类,字符串形式传输
// 使用JSON字符串作为序列化格式,子类需重写writeContent/readContent实现具体字段的读写
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