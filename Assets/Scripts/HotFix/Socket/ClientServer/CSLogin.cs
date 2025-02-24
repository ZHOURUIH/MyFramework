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
	public override bool read(SerializerBitRead reader, ulong fieldFlag)
	{
		bool success = true;
		success = success && mAccount.read(reader);
		success = success && mPassword.read(reader);
		return success;
	}
	public override void write(SerializerBitWrite writer, out ulong fieldFlag)
	{
		base.write(writer, out fieldFlag);
		mAccount.write(writer);
		mPassword.write(writer);
	}
	// auto generate end
	public static void send()
	{
		sendPacket(get());
	}
}