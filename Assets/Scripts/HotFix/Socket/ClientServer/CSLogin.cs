using static FrameUtility;
using static GU;

// auto generate start
// 登录角色
public class CSLogin : NetPacketBit
{
	public BIT_STRING mAccount = new();
	public BIT_STRING mPassword = new();
	public CSLogin()
	{
		addParam(mAccount, false);
		addParam(mPassword, false);
	}
	public static CSLogin get() { return PACKET<CSLogin>(); }
	public override bool read(SerializerBitRead reader, bool needReadSign, ulong fieldFlag)
	{
		bool success = true;
		success = success && mAccount.read(reader, needReadSign);
		success = success && mPassword.read(reader, needReadSign);
		return success;
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign, out ulong fieldFlag)
	{
		base.write(writer, needWriteSign, out fieldFlag);
		mAccount.write(writer, needWriteSign);
		mPassword.write(writer, needWriteSign);
	}
	protected override bool generateHasSignInternal()
	{
		return false;
	}
	// auto generate end
	public static void send()
	{
		sendPacket(get());
	}
}