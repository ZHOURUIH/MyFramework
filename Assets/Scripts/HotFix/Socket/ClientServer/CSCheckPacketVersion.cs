using static FrameUtility;
using static GU;

// auto generate start
// 请求服务器检查网络消息版本号是否正确,此消息应该排第一个
public class CSCheckPacketVersion : NetPacketBit
{
	public BIT_STRING mPacketVersion = new();
	public CSCheckPacketVersion()
	{
		addParam(mPacketVersion, false);
	}
	public static CSCheckPacketVersion get() { return PACKET<CSCheckPacketVersion>(); }
	public override bool read(SerializerBitRead reader, ulong fieldFlag)
	{
		bool success = true;
		success = success && mPacketVersion.read(reader);
		return success;
	}
	public override void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		base.write(writer, out fieldFlag);
		mPacketVersion.write(writer);
	}
	// auto generate end
	public static void send()
	{
		var packet = get();
		packet.mPacketVersion.set(PacketRegister.PACKET_VERSION);
		sendPacket(packet);
	}
}