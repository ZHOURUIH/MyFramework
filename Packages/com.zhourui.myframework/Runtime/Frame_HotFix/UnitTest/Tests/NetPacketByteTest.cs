using static TestAssert;

// NetPacketByte 单元测试
// 创建 / setPacketType / getPacketType / resetProperty / read / hasSign
public static class NetPacketByteTest
{
	public static void Run()
	{
		testCreate();
		testSetGetType();
		testResetProperty();
		testHasSign();
		testMarkAllFiled();
	}

	// ─── 创建 ────────────────────────────────────────────────────────────

	private static void testCreate()
	{
		var packet = new NetPacketByte();
		assertNotNull(packet, "NetPacketByte 实例不应为空");
	}

	// ─── setPacketType / getPacketType ──────────────────────────────────

	private static void testSetGetType()
	{
		var packet = new NetPacketByte();
		packet.setPacketType(42);
		assertEqual(42, packet.getPacketType(), "getPacketType 应返回 42");
	}

	// ─── resetProperty ──────────────────────────────────────────────────

	private static void testResetProperty()
	{
		var packet = new NetPacketByte();
		packet.setPacketType(99);
		packet.resetProperty();
		assertEqual(0, packet.getPacketType(), "resetProperty 后 getPacketType 应返回 0");
	}

	// ─── hasSign ─────────────────────────────────────────────────────────

	private static void testHasSign()
	{
		var packet = new NetPacketByte();
		assertTrue(packet.hasSign(), "hasSign 应返回 true");
	}

	// ─── markAllFiled ─────────────────────────────────────────────────────

	private static void testMarkAllFiled()
	{
		var packet = new TestBytePacket();
		// 默认所有字段 mValid=true
		assert(packet.mIntField.mValid,   "默认 int 字段 valid=true");
		assert(packet.mBoolField.mValid,  "默认 bool 字段 valid=true");
		assert(packet.mFloatField.mValid, "默认 float 字段 valid=true");

		// markAllFiled(false) 全部置为无效
		packet.markAllFiled(false);
		assert(!packet.mIntField.mValid,   "markAllFiled(false): int 字段 invalid");
		assert(!packet.mBoolField.mValid,  "markAllFiled(false): bool 字段 invalid");
		assert(!packet.mFloatField.mValid, "markAllFiled(false): float 字段 invalid");

		// markAllFiled(true) 全部恢复有效
		packet.markAllFiled(true);
		assert(packet.mIntField.mValid,   "markAllFiled(true): int 字段重新 valid");
		assert(packet.mBoolField.mValid,  "markAllFiled(true): bool 字段重新 valid");
		assert(packet.mFloatField.mValid, "markAllFiled(true): float 字段重新 valid");
	}
}

// ─── TestBytePacket：模拟带参数的 NetPacketByte ─────────────────────────────
// addParam 是 protected，需子类才能注册参数；字段公开以便断言 markAllFiled 效果
public class TestBytePacket : NetPacketByte
{
	public INT   mIntField   = new();
	public BOOL  mBoolField  = new();
	public FLOAT mFloatField = new();

	public TestBytePacket()
	{
		addParam(mIntField,   false);
		addParam(mBoolField,  false);
		addParam(mFloatField, false);
	}

	public override void resetProperty()
	{
		base.resetProperty();
		mIntField.resetProperty();
		mBoolField.resetProperty();
		mFloatField.resetProperty();
	}
}