using System;
using System.Net;

public class NetManager : FrameSystem
{
	protected NetConnectTCPFrame mServerConnect;
	protected byte[] EncryptKey;
	public NetManager()
	{
		EncryptKey = new byte[16] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};
	}
	public override void init()
	{
		base.init();
		mServerConnect = new NetConnectTCPFrame();
		mServerConnect.setName("Server");
		mServerConnect.init(null, 0, 30.0f);
		mServerConnect.setPingAction(() => { });
		mServerConnect.setHeartBeatAction(()=> { });
		mServerConnect.setEncrypt(encrypt, decrypt);
	}
	public void disconnect()
	{
		mServerConnect.disconnect();
	}
	public void connect()
	{
		mServerConnect.startConnect(true);
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mServerConnect.update(elapsedTime);
	}
	public override void destroy()
	{
		mServerConnect.destroy();
	}
	public void setIPPort(string ip, int port)
	{
		mServerConnect.setIPAddress(IPAddress.Parse(ip));
		mServerConnect.setPort(port);
	}
	public void sendPacket(Type type)
	{
		mServerConnect.sendNetPacket(type);
	}
	public void sendPacket(NetPacketTCP packet)
	{
		mServerConnect.sendNetPacket(packet);
	}
	public int getPing() { return mServerConnect.getPing(); }
	public NetConnectTCPFrame getConnect() { return mServerConnect; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void encrypt(byte[] data, int offset, int length)
	{
		int keyIndex = 0;
		for (int i = 0; i < length; ++i)
		{
			data[offset + i] ^= EncryptKey[keyIndex];
			++keyIndex;
			if (keyIndex >= EncryptKey.Length)
			{
				keyIndex = 0;
			}
		}
	}
	protected void decrypt(byte[] data, int offset, int length)
	{
		int keyIndex = 0;
		for (int i = 0; i < length; ++i)
		{
			data[offset + i] ^= EncryptKey[keyIndex];
			++keyIndex;
			if (keyIndex >= EncryptKey.Length)
			{
				keyIndex = 0;
			}
		}
	}
}