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
		testWriteReadThroughPacket();
		testMultipleWriteReadCycles();
		testDefaultBodyFieldValues();
		testResetPropertyKeepsSendBodyInstance();
		testUnicodeBodyRoundtrip();
		testResetAfterRead();
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

	// ═════════════════════════════════════════════════════════════════
	// 组合: write 序列化 send body + read 反序列化 recv body(各自验证)
	// 注意: TestSendBody(id/name) 与 TestRecvBody(code/message) 字段不同名,
	//       不能直接 write→read 还原值
	// ═════════════════════════════════════════════════════════════════
	private static void testWriteReadThroughPacket()
	{
		TestHttpT send = new TestHttpT();
		send.mSendBody.id = 88;
		send.mSendBody.name = "链路";
		string json = send.write();
		// write 输出 JSON 包含 send body 字段值
		assertTrue(json.Contains("88"), "write JSON 包含 id 88");
		assertTrue(json.Contains("链路"), "write JSON 包含 name");

		// read 用匹配 TestRecvBody 字段的 JSON 反序列化
		TestHttpT recv = new TestHttpT();
		recv.read("{\"code\":200,\"message\":\"ok\"}");
		assertNotNull(recv.mBody, "read 后 mBody 非空");
		assertEqual(200, recv.mBody.code, "read 后 mBody.code");
		assertEqual("ok", recv.mBody.message, "read 后 mBody.message");
	}

	// 多轮 read 反序列化循环(匹配 TestRecvBody 字段)
	private static void testMultipleWriteReadCycles()
	{
		for (int i = 0; i < 3; ++i)
		{
			TestHttpT recv = new TestHttpT();
			recv.read("{\"code\":" + (100 + i) + ",\"message\":\"m" + i + "\"}");
			assertEqual(100 + i, recv.mBody.code, "第 " + (i + 1) + " 轮 code 一致");
			assertEqual("m" + i, recv.mBody.message, "第 " + (i + 1) + " 轮 message 一致");
		}
	}

	// 默认 body 字段值
	private static void testDefaultBodyFieldValues()
	{
		TestHttpT packet = new TestHttpT();
		assertEqual(0, packet.mSendBody.id, "默认 mSendBody.id 0");
		assertTrue(packet.mSendBody.name == null, "默认 mSendBody.name null");
	}

	// resetProperty 后 mSendBody 同一实例 + 字段重置
	private static void testResetPropertyKeepsSendBodyInstance()
	{
		TestHttpT packet = new TestHttpT();
		TestSendBody original = packet.mSendBody;
		packet.mSendBody.id = 55;
		packet.read("{\"code\":1,\"message\":\"x\"}");
		packet.resetProperty();
		assertTrue(ReferenceEquals(original, packet.mSendBody), "resetProperty 后 mSendBody 同一实例");
		assertEqual(0, packet.mSendBody.id, "resetProperty 后 mSendBody.id 重置 0");
		assertTrue(packet.mBody == null, "resetProperty 后 mBody null");
	}

	// ═════════════════════════════════════════════════════════════════
	// 中文 body 序列化 + read 后 reset
	// ═════════════════════════════════════════════════════════════════

	// send body 中文名字写入 JSON(字段名不匹配, 验证 write JSON 含中文)
	private static void testUnicodeBodyRoundtrip()
	{
		TestHttpT send = new TestHttpT();
		send.mSendBody.id = 66;
		send.mSendBody.name = "中文玩家名称";
		string json = send.write();
		assertTrue(json.Contains("中文玩家名称"), "write JSON 包含中文 name");
		assertTrue(json.Contains("66"), "write JSON 包含 id 66");

		// read 用匹配 TestRecvBody 的 JSON
		TestHttpT recv = new TestHttpT();
		recv.read("{\"code\":200,\"message\":\"中文响应\"}");
		assertEqual("中文响应", recv.mBody.message, "中文 message 反序列化一致");
	}

	// read 后 resetProperty: mBody 清空 + mSendBody 重置
	private static void testResetAfterRead()
	{
		TestHttpT packet = new TestHttpT();
		packet.read("{\"code\":1,\"message\":\"x\"}");
		assertNotNull(packet.mBody, "read 后 mBody 非空");
		packet.mSendBody.id = 77;
		packet.resetProperty();
		assertTrue(packet.mBody == null, "reset 后 mBody null");
		assertEqual(0, packet.mSendBody.id, "reset 后 sendBody id 0");
	}
}
