using static TestAssert;

// NetPacket 基类单元测试 — 覆盖报文基类的连接/类型存取、实例唯一性、默认行为、重置
public static class NetPacketTest
{
	public static void Run()
	{
		testDefaultValues();
		testSetGetPacketType();
		testSetGetConnect();
		testInstanceUniqueness();
		testEqualsSelf();
		testExecuteNoop();
		testDebugInfo();
		testShowInfoDefault();
		testResetProperty();
	}

	// 测试用子类(非抽象,用于覆盖基类虚方法)
	private class TestPacket : NetPacket { }

	// ─── 构造默认值 ───────────────────────────────────────────────────
	private static void testDefaultValues()
	{
		var pkt = new TestPacket();
		assertNull(pkt.getConnect(), "构造后连接为 null");
		assertEqual((ushort)0, pkt.getPacketType(), "构造后消息类型为 0");
	}

	// ─── setPacketType / getPacketType ────────────────────────────────
	private static void testSetGetPacketType()
	{
		var pkt = new TestPacket();
		pkt.setPacketType(0x1234);
		assertEqual((ushort)0x1234, pkt.getPacketType(), "setPacketType 后 getPacketType 返回该值");
		pkt.setPacketType(0);
		assertEqual((ushort)0, pkt.getPacketType(), "setPacketType 可设置为 0");
	}

	// ─── setConnect / getConnect ──────────────────────────────────────
	private static void testSetGetConnect()
	{
		var pkt = new TestPacket();
		pkt.setConnect(null);
		assertNull(pkt.getConnect(), "setConnect(null) 后 getConnect 返回 null");
	}

	// ─── 实例唯一性(mPacketID 由 makeID 生成,互不相同) ─────────────────
	private static void testInstanceUniqueness()
	{
		var a = new TestPacket();
		var b = new TestPacket();
		assertNotEquals(a, b, "不同实例 Equals 应为 false");
		assertNotEquals(a.GetHashCode(), b.GetHashCode(), "不同实例 GetHashCode 应不同");
	}

	// ─── Equals 自身为 true ───────────────────────────────────────────
	private static void testEqualsSelf()
	{
		var pkt = new TestPacket();
		assertTrue(pkt.Equals(pkt), "Equals 自身为 true");
		assertEqual(pkt.GetHashCode(), pkt.GetHashCode(), "自身 GetHashCode 稳定");
	}

	// ─── execute 空实现调用安全 ───────────────────────────────────────
	private static void testExecuteNoop()
	{
		var pkt = new TestPacket();
		pkt.execute();
		assertNotNull(pkt, "execute 后对象仍有效");
	}

	// ─── debugInfo 返回类型名 ─────────────────────────────────────────
	private static void testDebugInfo()
	{
		var pkt = new TestPacket();
		assertEqual(typeof(TestPacket).ToString(), pkt.debugInfo(), "debugInfo 返回类型全名");
	}

	// ─── showInfo 默认 true ───────────────────────────────────────────
	private static void testShowInfoDefault()
	{
		var pkt = new TestPacket();
		assertTrue(pkt.showInfo(), "基类 showInfo 默认返回 true");
	}

	// ─── resetProperty 重置 ───────────────────────────────────────────
	private static void testResetProperty()
	{
		var pkt = new TestPacket();
		pkt.setConnect(null);
		pkt.setPacketType(0x7777);
		pkt.resetProperty();
		assertNull(pkt.getConnect(), "reset 后连接清空");
		assertEqual((ushort)0, pkt.getPacketType(), "reset 后消息类型归 0");
	}

	// ─── 反向断言辅助 ─────────────────────────────────────────────────
	private static void assertNotEquals(object a, object b, string message)
	{
		if (a.Equals(b) || a.GetHashCode() == b.GetHashCode())
		{
			throw new System.Exception("Assertion failed: " + message);
		}
	}
}
