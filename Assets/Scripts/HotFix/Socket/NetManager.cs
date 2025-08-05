using System;
using System.Net;
using static MathUtility;
using static GDH;

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
		mServerConnect.setEncrypt(encrypt, decrypt);
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
		mServerConnect.destroy();
	}
	public void sendPacket(NetPacketBit packet)
	{
		mServerConnect.sendNetPacket(packet);
	}
	public int getPing() { return mServerConnect.getPing(); }
	public bool isConnected() { return mServerConnect.isConnected(); }
	public NetConnectTCPBit getConnect() { return mServerConnect; }
	protected void encrypt(byte[] data, int offset, int length, byte param)
	{
		int keyLength = ENCRYPT_KEY.Length;
		if (!isPow2(keyLength))
		{
			return;
		}
		// 因为keyLength长度是2的n次方,所以可以直接按位与来达到取余的效果
		int keyIndex = (param ^ 223) & (keyLength - 1);
		for (int i = 0; i < length; ++i)
		{
			byte keyChar = (byte)(ENCRYPT_KEY[keyIndex] ^ param);
			data[offset + i] += (byte)(keyChar >> 1);
			data[offset + i] ^= (byte)(((keyChar * keyIndex) & (mKey0 * mKey1)) | ((mKey2 + mKey3) * keyIndex));
			keyIndex += i;
			if (keyIndex >= keyLength)
			{
				keyIndex &= keyLength - 1;
			}
		}
	}
	protected void decrypt(byte[] data, int offset, int length, byte param)
	{
		int keyLength = ENCRYPT_KEY.Length;
		if (!isPow2(keyLength))
		{
			return;
		}
		// 因为keyLength长度是2的n次方,所以可以直接按位与来达到取余的效果
		int keyIndex = (param ^ 223) & (keyLength - 1);
		for (int i = 0; i < length; ++i)
		{
			byte keyChar = (byte)(ENCRYPT_KEY[keyIndex] ^ param);
			data[offset + i] ^= (byte)(((keyChar * keyIndex) & (mKey0 * mKey1)) | ((mKey2 + mKey3) * keyIndex));
			data[offset + i] -= (byte)(keyChar >> 1);
			keyIndex += i;
			if (keyIndex >= keyLength)
			{
				keyIndex &= keyLength - 1;
			}
		}
	}
}