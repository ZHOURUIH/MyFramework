using static CSharpUtility;

// 与服务器交互的消息包基类
public class NetPacket : ClassObject
{
	protected NetConnect mConnect;		// 记录了是从哪个服务器发送过来的
	protected int mPacketID;            // 对象实例唯一ID
	protected ushort mType;             // 消息类型
	public NetPacket()
	{
		mPacketID = makeID();
	}
	public void setConnect(NetConnect connect) { mConnect = connect; }
	public void setPacketType(ushort type) { mType = type; }
	public ushort getPacketType() { return mType; }
	// 如果是服务器向客户端发送的消息,则需要重写该函数
	public virtual void execute() { }
	public virtual string debugInfo() { return GetType().ToString(); }
	public virtual bool showInfo() { return true; }
	public override void resetProperty()
	{
		base.resetProperty();
		mConnect = null;
		// mPacketID,mType不需要重置
		// mPacketID = 0;
		mType = 0;
	}
	public override int GetHashCode() { return mPacketID; }
	public override bool Equals(object obj) { return mPacketID == (obj as NetPacket).mPacketID; }
}