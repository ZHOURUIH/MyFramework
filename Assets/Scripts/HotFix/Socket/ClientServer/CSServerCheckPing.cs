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
	public override bool read(SerializerBitRead reader, bool needReadSign, ulong fieldFlag)
	{
		bool success = true;
		success = success && mIndex.read(reader, needReadSign);
		return success;
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign, out ulong fieldFlag)
	{
		base.write(writer, needWriteSign, out fieldFlag);
		mIndex.write(writer, needWriteSign);
	}
	protected override bool generateHasSignInternal()
	{
		if (mIndex < 0)
		{
			return true;
		}
		return false;
	}
	// auto generate end
	public static void send()
	{
		sendPacket(get());
	}
}