using System.Collections;
using System;
using System.Collections.Generic;

public class SocketPacket : SerializedData, IClassObject
{
	public int mType;
	public ISocketConnect mConnect; // 如果是从服务器发过来的消息,记录了是从哪个服务器发送过来的
	public NetClient mClient;       // 如果是从客户端发过来的消息,记录了是从哪个客户端发过来的
	public uint mClientID;			// 客户端ID
	public void setConnect(ISocketConnect connect) { mConnect = connect; }
	public virtual void init(int type)
	{
		mType = type;
		fillParams();
		zeroParams();
	}
	public int getPacketType() { return mType; }
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
}