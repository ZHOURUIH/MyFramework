using static FrameUtility;
using static GU;

// auto generate start
// 主动向客户端发送延迟检测消息,也需要排在前面
public class SCServerCheckPing : NetPacketBit
{
	public BIT_INT mIndex = new();
	public SCServerCheckPing()
	{
		addParam(mIndex, false);
	}
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
	public override void execute()
	{}
}