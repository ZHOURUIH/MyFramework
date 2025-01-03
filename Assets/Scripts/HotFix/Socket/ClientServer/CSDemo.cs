using static FrameUtility;
using static GBH;

public class CSDemo : NetPacketBit
{
	public BIT_LONGS mDemoArray = new();
	public BIT_USHORT mDemoUShort = new();
	public CSDemo()
	{
		addParam(mDemoArray, false);
		addParam(mDemoUShort, false);
	}
	public static CSDemo get() { return PACKET<CSDemo>(); }
	public override bool read(SerializerBitRead reader, ulong fieldFlag)
	{
		bool success = true;
		success = success && mDemoArray.read(reader);
		success = success && mDemoUShort.read(reader);
		return success;
	}
	public override void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		base.write(writer, out fieldFlag);
		mDemoArray.write(writer);
		mDemoUShort.write(writer);
	}
	public static void send()
	{
		CSDemo packet = get();
		packet.mDemoArray.add(1);
		packet.mDemoUShort.mValue = 2;
		mNetManager.sendPacket(packet);
	}
}