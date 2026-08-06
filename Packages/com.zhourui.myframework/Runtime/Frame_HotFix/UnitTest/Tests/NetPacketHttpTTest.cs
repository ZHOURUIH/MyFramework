using Newtonsoft.Json;
using static TestAssert;

// 泛型 http 消息类 NetPacketHttpT 的单元测试
// 覆盖 JSON 序列化/反序列化往返、默认值、resetProperty 及继承的 NetPacketHttp 行为
// CSBody 需实现 IResetProperty(发送体), SCBody 无约束(响应体)
public static class NetPacketHttpTTest
{
	// 发送体: 实现 IResetProperty, 用 public 属性保证 Newtonsoft 可序列化
	private class TestSendBody : IResetProperty
	{
		public int id;
		public string name;
		public void resetProperty()
		{
			id = 0;
			name = null;
		}
	}

	// 响应体: 普通类, 无需 IResetProperty
	private class TestRecvBody
	{
		public int code { get; set; }
		public string message { get; set; }
	}

	// 测试用泛型 http 消息子类, 设置 URL 与请求方式
	private class TestHttpT : NetPacketHttpT<TestSendBody, TestRecvBody>
	{
		public TestHttpT()
		{
			mURL = "http://unit.test/t";
			mMethod = HTTP_METHOD.GET;
		}
	}

	public static void Run()
	{
		testDefaultConstruction();
		testWriteSerializesSendBody();
		testReadDeserializesRecvBody();
		testReadEmptyJson();
		testResetProperty_ClearsBody();
		testResetProperty_ClearsSendBody();
		testInheritsHttpDefaults();
		testReadWriteRoundTrip();
		testSendBodySeparatePerInstance();
	}

	// ═════════════════════════════════════════════════════════════════
	// 默认构造: mSendBody 自动 new, mBody 为 default
	// ═════════════════════════════════════════════════════════════════
	private static void testDefaultConstruction()
	{
		TestHttpT packet = new TestHttpT();
		assertNotNull(packet.mSendBody, "mSendBody 构造时应已 new 出实例");
		assertNull(packet.mBody, "mBody 默认应为 null(引用类型 default)");
	}

	// ═════════════════════════════════════════════════════════════════
	// write(): 将 mSendBody 序列化为 JSON
	// ═════════════════════════════════════════════════════════════════
	private static void testWriteSerializesSendBody()
	{
		TestHttpT packet = new TestHttpT();
		packet.mSendBody.id = 7;
		packet.mSendBody.name = "张三";
		string json = packet.write();
		// 反序列化验证 JSON 包含发送体字段
		TestSendBody parsed = JsonConvert.DeserializeObject<TestSendBody>(json);
		assertEqual(7, parsed.id, "write 序列化应包含 id");
		assertEqual("张三", parsed.name, "write 序列化应包含 name");
	}

	// ═════════════════════════════════════════════════════════════════
	// read(string): 反序列化 JSON 到 mBody
	// ═════════════════════════════════════════════════════════════════
	private static void testReadDeserializesRecvBody()
	{
		TestHttpT packet = new TestHttpT();
		packet.read("{\"code\":200,\"message\":\"ok\"}");
		assertNotNull(packet.mBody, "read 后 mBody 应被赋值");
		assertEqual(200, packet.mBody.code, "mBody.code 应解析为 200");
		assertEqual("ok", packet.mBody.message, "mBody.message 应解析为 ok");
	}

	// ═════════════════════════════════════════════════════════════════
	// read(空串/空对象/null): 空串抛异常, 合法 JSON 不抛
	// ═════════════════════════════════════════════════════════════════
	private static void testReadEmptyJson()
	{
		TestHttpT packet = new TestHttpT();
		// 空串/空对象/null 都是边界输入, 应安全处理不抛异常(实测 Newtonsoft 空串不抛)
		packet.read("");
		packet.read("{}");
		packet.read("null");
		assertTrue(true, "空串/空对象/null JSON 不应抛异常");

		// 边界 read 之后, 仍可用正常 JSON 重新赋值
		packet.read("{\"code\":200,\"message\":\"ok\"}");
		assertNotNull(packet.mBody, "边界 read 后仍能正常反序列化");
		assertEqual(200, packet.mBody.code, "边界 read 后 mBody.code 正确");
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty: mBody 清空
	// ═════════════════════════════════════════════════════════════════
	private static void testResetProperty_ClearsBody()
	{
		TestHttpT packet = new TestHttpT();
		packet.read("{\"code\":500,\"message\":\"err\"}");
		assertNotNull(packet.mBody, "前置: mBody 已有值");
		packet.resetProperty();
		assertNull(packet.mBody, "resetProperty 后 mBody 应为 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty: mSendBody 各字段清零
	// ═════════════════════════════════════════════════════════════════
	private static void testResetProperty_ClearsSendBody()
	{
		TestHttpT packet = new TestHttpT();
		packet.mSendBody.id = 99;
		packet.mSendBody.name = "abc";
		packet.resetProperty();
		assertEqual(0, packet.mSendBody.id, "resetProperty 后 mSendBody.id 应归 0");
		assertNull(packet.mSendBody.name, "resetProperty 后 mSendBody.name 应清空");
	}

	// ═════════════════════════════════════════════════════════════════
	// 继承的 NetPacketHttp 默认行为
	// ═════════════════════════════════════════════════════════════════
	private static void testInheritsHttpDefaults()
	{
		TestHttpT packet = new TestHttpT();
		assertEqual("http://unit.test/t", packet.getUrl(), "getUrl 应返回子类构造设置的 URL");
		assertEqual(HTTP_METHOD.GET, packet.getMethod(), "getMethod 应返回子类构造设置的方法");
		assertEqual(10000, packet.timeout(), "timeout 默认 10000");
	}

	// ═════════════════════════════════════════════════════════════════
	// write 输出 JSON 字段保真(write→反序列化→字段一致)
	// ═════════════════════════════════════════════════════════════════
	private static void testReadWriteRoundTrip()
	{
		TestHttpT send = new TestHttpT();
		send.mSendBody.id = 42;
		send.mSendBody.name = "roundtrip";
		string json = send.write();

		// write 输出的 JSON 反序列化回发送体, 字段应完全还原
		TestSendBody parsed = JsonConvert.DeserializeObject<TestSendBody>(json);
		assertEqual(42, parsed.id, "round-trip id 保真");
		assertEqual("roundtrip", parsed.name, "round-trip name 保真");
	}

	// ═════════════════════════════════════════════════════════════════
	// mSendBody 每个实例独立(非 static 共享)
	// ═════════════════════════════════════════════════════════════════
	private static void testSendBodySeparatePerInstance()
	{
		TestHttpT a = new TestHttpT();
		TestHttpT b = new TestHttpT();
		a.mSendBody.id = 1;
		b.mSendBody.id = 2;
		assertEqual(1, a.mSendBody.id, "a 的 mSendBody 独立");
		assertEqual(2, b.mSendBody.id, "b 的 mSendBody 独立");
		assertFalse(ReferenceEquals(a.mSendBody, b.mSendBody), "各实例 mSendBody 不应共享");
	}
}
