using System;

public class SocketPacket : SerializedData
{
	protected ISocketConnect mConnect;  // 如果是从服务器发过来的消息,记录了是从哪个服务器发送过来的
	protected NetClient mClient;        // 如果是从客户端发过来的消息,记录了是从哪个客户端发过来的
	protected uint mClientID;           // 客户端ID
	protected uint mPacketID;           // 对象实例唯一ID
	protected ushort mType;             // 消息类型
	public SocketPacket()
	{
		mPacketID = makeID();
	}
	public void setConnect(ISocketConnect connect) { mConnect = connect; }
	public virtual void init()
	{
		fillParams();
		zeroParams();
	}
	public void setPacketType(ushort type) { mType = type; }
	public ushort getPacketType() { return mType; }
	// 如果是服务器向客户端发送的消息,则需要重写该函数
	public virtual void execute() { }
	protected override void fillParams() { }
	public virtual string debugInfo() { return Typeof(this).ToString(); }
	public virtual bool showInfo() { return true; }
	public override void resetProperty()
	{
		base.resetProperty();
		zeroParams();
		mClientID = 0;
		mClient = null;
		mConnect = null;
		// mPacketID,mType不需要重置
		// mPacketID = 0;
		// mType = 0;
	}
	public void setClient(NetClient client) { mClient = client; }
	public void setClientID(uint clientID) { mClientID = clientID; }
	public Character getPlayer() { return mCharacterManager.getCharacter(mClient.getCharacterGUID()); }
	public override int GetHashCode() { return (int)mPacketID; }
	public override bool Equals(object obj) { return mPacketID == (obj as SocketPacket).mPacketID; }
}