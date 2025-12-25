using static MathUtility;
using static FrameDefine;
using static UnityUtility;

// 表示一个与服务器的连接
public class NetConnect : CommandReceiver
{
	protected EncryptPacket mEncryptPacket;     // 加密函数
	protected DecryptPacket mDecryptPacket;     // 解密函数
	protected string mLabel;					// 服务器名字
	protected const int mKey0 = 41;
	protected const int mKey1 = 3;
	protected const int mKey2 = 600;
	protected const int mKey3 = 34;
	public NetConnect()
	{
		mEncryptPacket = encrypt;
		mDecryptPacket = decrypt;
		if (!isPow2(ENCRYPT_KEY.Length))
		{
			logError("加密密钥的长度需要为2的n次方");
		}
	}
	public void setLabel(string label) { mLabel = label; }
	public string getLabel() { return mLabel; }
	public override void resetProperty()
	{
		base.resetProperty();
		mEncryptPacket = encrypt;
		mDecryptPacket = decrypt;
		mLabel = null;
	}
	public void setEncrypt(EncryptPacket encrypt, DecryptPacket decrypt)
	{
		mEncryptPacket = encrypt;
		mDecryptPacket = decrypt;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 此处只是提供一个默认的加密方法,应用层可以写一个自己的加密方法
	protected static void encrypt(byte[] data, int offset, int length, byte param)
	{
		int keyLength = ENCRYPT_KEY.Length;
		int keyIndex = (param ^ 223) & (keyLength - 1);
		for (int i = 0; i < length; ++i)
		{
			byte keyChar = (byte)(ENCRYPT_KEY[keyIndex] ^ param);
			data[offset + i] ^= keyChar;
			data[offset + i] += (byte)(keyChar >> 1);
			data[offset + i] ^= (byte)(((keyChar * keyIndex) & (mKey0 * mKey1)) | ((mKey2 + mKey3) * keyIndex));
			keyIndex += i;
			// 因为长度是2的n次方,所以可以直接按位与来达到取余的效果
			keyIndex &= keyLength - 1;
		}
	}
	protected static void decrypt(byte[] data, int offset, int length, byte param)
	{
		int keyLength = ENCRYPT_KEY.Length;
		int keyIndex = (param ^ 223) & (keyLength - 1);
		for (int i = 0; i < length; ++i)
		{
			byte keyChar = (byte)(ENCRYPT_KEY[keyIndex] ^ param);
			data[offset + i] ^= (byte)(((keyChar * keyIndex) & (mKey0 * mKey1)) | ((mKey2 + mKey3) * keyIndex));
			data[offset + i] -= (byte)(keyChar >> 1);
			data[offset + i] ^= keyChar;
			keyIndex += i;
			// 因为长度是2的n次方,所以可以直接按位与来达到取余的效果
			keyIndex &= keyLength - 1;
		}
	}
}