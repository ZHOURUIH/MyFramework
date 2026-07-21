
// 表示一个与服务器的连接
public class NetConnect : CommandReceiver
{
	protected EncryptPacket mEncryptPacket;     // 加密函数
	protected DecryptPacket mDecryptPacket;     // 解密函数
	protected string mLabel;					// 服务器名字
	public NetConnect()
	{
		mEncryptPacket = encrypt;
		mDecryptPacket = decrypt;
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
		byte[] keys = FrameSettings.getEncryptKey();
		int[] keyHeler = FrameSettings.getEncryptKeyHelper();
		int keyLength = keys.Length;
		int keyIndex = (param ^ 223) & (keyLength - 1);
		for (int i = 0; i < length; ++i)
		{
			byte keyChar = (byte)(keys[keyIndex] ^ param);
			data[offset + i] ^= keyChar;
			data[offset + i] += (byte)(keyChar >> 1);
			data[offset + i] ^= (byte)(((keyChar * keyIndex) & (keyHeler[0] * keyHeler[1])) | ((keyHeler[2] + keyHeler[3]) * keyIndex));
			keyIndex += i;
			// 因为长度是2的n次方,所以可以直接按位与来达到取余的效果
			keyIndex &= keyLength - 1;
		}
	}
	protected static void decrypt(byte[] data, int offset, int length, byte param)
	{
		byte[] keys = FrameSettings.getEncryptKey();
		int[] keyHeler = FrameSettings.getEncryptKeyHelper();
		int keyLength = keys.Length;
		int keyIndex = (param ^ 223) & (keyLength - 1);
		for (int i = 0; i < length; ++i)
		{
			byte keyChar = (byte)(keys[keyIndex] ^ param);
			data[offset + i] ^= (byte)(((keyChar * keyIndex) & (keyHeler[0] * keyHeler[1])) | ((keyHeler[2] + keyHeler[3]) * keyIndex));
			data[offset + i] -= (byte)(keyChar >> 1);
			data[offset + i] ^= keyChar;
			keyIndex += i;
			// 因为长度是2的n次方,所以可以直接按位与来达到取余的效果
			keyIndex &= keyLength - 1;
		}
	}
}