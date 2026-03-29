
// auto generate start
// 主动向客户端发送延迟检测消息,也需要排在前面
public class SCServerCheckPing : NetPacketBit
{
	public BIT_INT mIndex = new();
	public SCServerCheckPing()
	{
		addParam(mIndex, false);
	}
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
	public override void execute()
	{}
}