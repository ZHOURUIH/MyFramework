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
		testWrite_FieldFlag();
		testWrite_EmptyNoParams();
		testFieldSetAndResetSequence();
		testWriteReadRoundtrip();
		testOptionalFieldFlag();
		testMarkAllFiledToggleSequence();
		testWriteFlagStable();
		testOptionalReadDefaults();
		testResetPropertyClearsFields();
		testWriteNullWriterFlag();
		testRoundtripDifferentValues();
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

	// ─── write: 计算 fieldFlag 位掩码 ────────────────────────────────────

	private static void testWrite_FieldFlag()
	{
		// TestBytePacket 注册 3 个非可选参数 → bit 0,1,2 置 1 → fieldFlag = 0b111 = 7
		var packet = new TestBytePacket();
		packet.write(null, out ulong fieldFlag);
		assertEqual(7UL, fieldFlag, "3 个非可选参数 → fieldFlag=0b111=7");
		assertTrue((fieldFlag & (1UL << 0)) != 0, "bit0=1");
		assertTrue((fieldFlag & (1UL << 1)) != 0, "bit1=1");
		assertTrue((fieldFlag & (1UL << 2)) != 0, "bit2=1");
		assertFalse((fieldFlag & (1UL << 3)) != 0, "bit3=0");
	}

	private static void testWrite_EmptyNoParams()
	{
		// 无任何参数 → fieldFlag 恒为 0
		var packet = new NetPacketByte();
		packet.write(null, out ulong fieldFlag);
		assertEqual(0UL, fieldFlag, "无参数 → fieldFlag=0");
	}

	// ─── 组合场景: 字段设置 → resetProperty 复位 ──────────────────────────

	private static void testFieldSetAndResetSequence()
	{
		var packet = new TestBytePacket();
		packet.mIntField.set(12345);
		packet.mBoolField.set(true);
		packet.mFloatField.set(3.14f);
		assertEqual(12345, packet.mIntField.mValue, "int 字段写入");
		assertTrue(packet.mBoolField.mValue, "bool 字段写入");
		assertEqual(3.14f, packet.mFloatField.mValue, 0.0001f, "float 字段写入");
		// resetProperty 复位所有字段
		packet.resetProperty();
		assertEqual(0, packet.mIntField.mValue, "resetProperty 后 int 复位 0");
		assertFalse(packet.mBoolField.mValue, "resetProperty 后 bool 复位 false");
		assertEqual(0.0f, packet.mFloatField.mValue, 0.0001f, "resetProperty 后 float 复位 0");
	}

	// ─── 组合场景: 完整序列化往返(write → read 值一致) ──────────────────

	private static void testWriteReadRoundtrip()
	{
		var packet = new TestBytePacketRoundtrip();
		packet.mIntField.set(-987654);
		packet.mBoolField.set(true);
		packet.mFloatField.set(3.14159f);
		// 写
		var writer = new SerializerWrite();
		packet.write(writer, out ulong fieldFlag);
		assertEqual(7UL, fieldFlag, "3 个非可选字段 → flag=0b111");
		// 读回(用新实例, 模拟网络对端)
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		var packet2 = new TestBytePacketRoundtrip();
		assertTrue(packet2.read(reader, fieldFlag), "read 返回 true");
		assertEqual(-987654, packet2.mIntField.mValue, "int 往返一致");
		assertTrue(packet2.mBoolField.mValue, "bool 往返一致");
		assertEqual(3.14159f, packet2.mFloatField.mValue, 0.0001f, "float 往返一致");
	}

	// ─── 组合场景: optional 参数不进 fieldFlag ──────────────────────────

	private static void testOptionalFieldFlag()
	{
		var packet = new TestBytePacketOptional();
		packet.write(null, out ulong fieldFlag);
		// 只有非 optional 的 mIntField 置位 → bit0=1, bit1=0
		assertEqual(1UL, fieldFlag, "1 个非可选 + 1 个可选 → flag=0b01");
		assertTrue((fieldFlag & (1UL << 0)) != 0, "bit0=1(非可选)");
		assertFalse((fieldFlag & (1UL << 1)) != 0, "bit1=0(可选)");
	}

	// ─── 组合场景: markAllFiled 多次切换 ────────────────────────────────

	private static void testMarkAllFiledToggleSequence()
	{
		var packet = new TestBytePacket();
		// true → false → true → false 多次切换
		packet.markAllFiled(true);
		assertTrue(packet.mIntField.mValid, "切换1 后 int valid");
		packet.markAllFiled(false);
		assertFalse(packet.mIntField.mValid, "切换2 后 int invalid");
		packet.markAllFiled(true);
		assertTrue(packet.mBoolField.mValid, "切换3 后 bool valid");
		packet.markAllFiled(false);
		assertFalse(packet.mFloatField.mValid, "切换4 后 float invalid");
		// fieldFlag 不受 mValid 影响(只由 optional 决定)
		packet.write(null, out ulong fieldFlag);
		assertEqual(7UL, fieldFlag, "mValid 不影响 fieldFlag");
	}

	// ─── 深度组合场景 ────────────────────────────────────────────────

	// 同一实例两次 write flag 一致
	private static void testWriteFlagStable()
	{
		var packet = new TestBytePacket();
		packet.write(null, out ulong flag1);
		packet.write(null, out ulong flag2);
		assertEqual(flag1, flag2, "两次 write flag 一致");
		assertEqual(7UL, flag1, "3 字段全置位 0b111");
	}

	// 可选字段未写 → read 后默认值
	private static void testOptionalReadDefaults()
	{
		var packet = new TestBytePacketOptional();
		var writer = new SerializerWrite();
		packet.write(writer, out ulong fieldFlag);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		var packet2 = new TestBytePacketOptional();
		packet2.read(reader, fieldFlag);
		assertFalse(packet2.mBoolField.mValue, "可选字段未写 → 默认 false");
		assertEqual(0, packet2.mIntField.mValue, "非可选字段未设置 → 默认 0");
	}

	// resetProperty 清字段
	private static void testResetPropertyClearsFields()
	{
		var packet = new TestBytePacket();
		packet.mIntField.set(123);
		packet.mBoolField.set(true);
		packet.mFloatField.set(1.5f);
		packet.resetProperty();
		assertEqual(0, packet.mIntField.mValue, "reset 后 int 复位 0");
		assertFalse(packet.mBoolField.mValue, "reset 后 bool 复位 false");
		assertEqual(0.0f, packet.mFloatField.mValue, 0.001f, "reset 后 float 复位 0");
	}

	// 全字段 write(null) flag 正确
	private static void testWriteNullWriterFlag()
	{
		var packet = new TestBytePacket();
		packet.write(null, out ulong fieldFlag);
		assertEqual(7UL, fieldFlag, "null writer 时 flag 仍 0b111");
	}

	// 多值往返: 写不同值读回一致
	private static void testRoundtripDifferentValues()
	{
		int[] intValues = { -1, 0, 12345, int.MaxValue };
		bool[] boolValues = { true, false };
		foreach (int iv in intValues)
		{
			foreach (bool bv in boolValues)
			{
				var packet = new TestBytePacketRoundtrip();
				packet.mIntField.set(iv);
				packet.mBoolField.set(bv);
				packet.mFloatField.set(2.5f);
				var writer = new SerializerWrite();
				packet.write(writer, out ulong fieldFlag);
				var reader = new SerializerRead();
				reader.init(writer.getBuffer(), writer.getDataSize(), 0);
				var packet2 = new TestBytePacketRoundtrip();
				packet2.read(reader, fieldFlag);
				assertEqual(iv, packet2.mIntField.mValue, "int 往返 " + iv);
				assertEqual(bv, packet2.mBoolField.mValue, "bool 往返 " + bv);
				assertEqual(2.5f, packet2.mFloatField.mValue, 0.001f, "float 往返");
			}
		}
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

// ─── TestBytePacketRoundtrip: override write/read 支持真实序列化往返 ─────────
public class TestBytePacketRoundtrip : NetPacketByte
{
	public INT   mIntField   = new();
	public BOOL  mBoolField  = new();
	public FLOAT mFloatField = new();

	public TestBytePacketRoundtrip()
	{
		addParam(mIntField,   false);
		addParam(mBoolField,  false);
		addParam(mFloatField, false);
	}

	public override void write(SerializerWrite writer, out ulong fieldFlag)
	{
		base.write(writer, out fieldFlag);
		mIntField.write(writer);
		mBoolField.write(writer);
		mFloatField.write(writer);
	}

	public override bool read(SerializerRead reader, ulong fieldFlag)
	{
		if (!mIntField.read(reader))
		{
			return false;
		}
		if (!mBoolField.read(reader))
		{
			return false;
		}
		if (!mFloatField.read(reader))
		{
			return false;
		}
		return true;
	}

	public override void resetProperty()
	{
		base.resetProperty();
		mIntField.resetProperty();
		mBoolField.resetProperty();
		mFloatField.resetProperty();
	}
}

// ─── TestBytePacketOptional: 混合可选/非可选参数验证 fieldFlag ──────────────
public class TestBytePacketOptional : NetPacketByte
{
	public INT  mIntField  = new();
	public BOOL mBoolField = new();

	public TestBytePacketOptional()
	{
		addParam(mIntField,  false);   // 非可选 → bit0
		addParam(mBoolField, true);    // 可选 → 不进 flag
	}

	public override void resetProperty()
	{
		base.resetProperty();
		mIntField.resetProperty();
		mBoolField.resetProperty();
	}
}