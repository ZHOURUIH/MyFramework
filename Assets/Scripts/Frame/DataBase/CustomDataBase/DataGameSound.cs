using System;

public class DataGameSound : Data
{
	public int mSoundID;
	public byte[] mSoundFileName = new byte[64];
	public byte[] mDescribe = new byte[64];
	public float mVolumeScale;
	protected static Serializer mSerializer = new Serializer();
	public override void read(byte[] data)
	{
		mSerializer.init(data);
		mSerializer.read(out mSoundID);
		mSerializer.readBuffer(mSoundFileName, 64, 64);
		mSerializer.readBuffer(mDescribe, 64, 64);
		mSerializer.read(out mVolumeScale);
	}
	public override int getDataSize()
	{
		return sizeof(int) + 64 + 64 + sizeof(float);
	}
}