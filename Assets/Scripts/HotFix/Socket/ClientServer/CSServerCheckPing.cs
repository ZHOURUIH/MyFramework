using static FrameUtility;
using static GU;

// auto generate start
// 客户端回复主动发送延迟检测消息,需要排在前面
public class CSServerCheckPing : NetPacketBit
{
	public BIT_INT mIndex = new();
	public CSServerCheckPing()
	{
		addParam(mIndex, false);
	}
	public static CSServerCheckPing get() { return PACKET<CSServerCheckPing>(); }
	public override bool read(SerializerBitRead reader, ulong fieldFlag)
	{
		bool success = true;
		success = success && mIndex.read(reader);
		return success;
	}
	public override void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		base.write(writer, out fieldFlag);
		mIndex.write(writer);
	}
	// auto generate end
	public static void send()
	{
		sendPacket(get());
	}
}