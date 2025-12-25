using System;
using System.Net;

public class NetManager : FrameSystem
{
	protected NetConnectTCPBit mServerConnect;
	protected int mKey0 = 1;
	protected int mKey1 = 2;
	protected int mKey2 = 3;
	protected int mKey3 = 4;
	public override void init()
	{
		base.init();
		mServerConnect = new();
		mServerConnect.setName("Server");
		mServerConnect.init(IPAddress.Parse("127.0.0.1"), 50002);
	}
	public void disconnect()
	{
		mServerConnect.disconnect();
	}
	public void connect(Action<bool> callback)
	{
		mServerConnect.startConnect((bool success)=>
		{ 
			// 连接服务器以后检测消息版本号
			if (success)
			{
				CSCheckPacketVersion.send();
			}
			callback?.Invoke(success);
		});
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mServerConnect.update(elapsedTime);
	}
	public override void destroy()
	{
		base.destroy();
		mServerConnect.destroy();
	}
	public void sendPacket(NetPacketBit packet)
	{
		mServerConnect.sendNetPacket(packet);
	}
	public int getPing() { return mServerConnect.getPing(); }
	public bool isConnected() { return mServerConnect.isConnected(); }
	public NetConnectTCPBit getConnect() { return mServerConnect; }
}