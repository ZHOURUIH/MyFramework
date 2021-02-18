using System;

public class SocketPacket : SerializedData, IClassObject
{
	public ISocketConnect mConnect; // 如果是从服务器发过来的消息,记录了是从哪个服务器发送过来的
	public NetClient mClient;       // 如果是从客户端发过来的消息,记录了是从哪个客户端发过来的
	public uint mClientID;          // 客户端ID
	public uint mID;                // 对象实例唯一ID
	public ushort mType;            // 消息类型
	public SocketPacket()
	{
		mID = makeID();
	}
	public void setConnect(ISocketConnect connect) { mConnect = connect; }
	public virtual void init(ushort type)
	{
		mType = type;
		fillParams();
		zeroParams();
	}
	public ushort getPacketType() { return mType; }
	// 如果是服务器向客户端发送的消息,则需要重写该函数
	public virtual void execute() { }
	protected override void fillParams() { }
	public virtual string debugInfo(){return Typeof(this).ToString();}
	public virtual bool showInfo() { return true; }
	public void resetProperty()
	{
		zeroParams();
	}
	public Character getPlayer()
	{
		return mCharacterManager.getCharacter(mClient.getCharacterGUID());
	}
	public override int GetHashCode() { return (int)mID; }
	public override bool Equals(object obj) { return mID == (obj as SocketPacket).mID; }
}