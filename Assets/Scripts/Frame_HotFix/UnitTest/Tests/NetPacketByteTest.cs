#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
}
#endif
