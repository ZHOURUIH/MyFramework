using System.Net;
using UnityEngine.Networking;
using static TestAssert;

// 补充覆盖 Json/Http 消息基类与接收数据结构
public static class NetPacketJsonHttpTest
{
	public static void Run()
	{
		testNetPacketJsonWriteAndRead();
		testNetPacketHttpDefaultsAndOverride();
		testReceivedDataInfoStoresValues();
		testJsonWriteEmpty();
		testJsonReadNull();
		testMultipleReadCycles();
		testHttpResetProperty();
		testJsonResetProperty();
		testJsonPacketInstancesIndependent();
		testJsonReadEmptyTwice();
	}

	private static void testNetPacketJsonWriteAndRead()
	{
		TestJsonPacket packet = new();
		packet.mWriteContent = "{\"ok\":true}";
		byte[] bytes = packet.write();
		assertEqual(packet.mWriteContent, bytes.bytesToString(), "write 应返回 writeContent 的字节");
		packet.read("hello".toBytes(), 5);
		assertEqual("hello", packet.mReadContent, "read 应将字节转成字符串后传给 readContent");
	}

	private static void testNetPacketHttpDefaultsAndOverride()
	{
		NetPacketHttp basePacket = new();
		assertEqual(HTTP_METHOD.POST, basePacket.getMethod(), "Http 基类默认方法应为 POST");
		assertNull(basePacket.getUrl(), "Http 基类默认 URL 应为空");
		assertEqual(10000, basePacket.timeout(), "Http 基类默认超时时间错误");
		assertNull(basePacket.write(), "Http 基类默认 write 为空");
		basePacket.read("ignored");

		TestHttpPacket packet = new();
		assertEqual("http://unit.test/api", packet.getUrl(), "子类 URL 设置错误");
		assertEqual(HTTP_METHOD.GET, packet.getMethod(), "子类方法设置错误");
		assertEqual("payload", packet.write(), "子类 write 错误");
		packet.read("response");
		assertEqual("response", packet.mReadMessage, "子类 read 错误");
		assertEqual(1234, packet.timeout(), "子类 timeout 错误");
		packet.execute(); // sealed 空实现, 调用不应出错
	}

	private static void testReceivedDataInfoStoresValues()
	{
		bool called = false;
		ReceivedDataInfo info = new()
		{
			mCallback = _ => called = true,
			mPacketType = typeof(TestHttpPacket),
			mData = "data",
			mStatus = WebExceptionStatus.Timeout,
			mWebStatus = UnityWebRequest.Result.ConnectionError,
			mCode = HttpStatusCode.BadGateway,
			mWebCode = 502,
		};
		assertEqual(typeof(TestHttpPacket), info.mPacketType);
		assertEqual("data", info.mData);
		assertEqual(WebExceptionStatus.Timeout, info.mStatus);
		assertEqual(UnityWebRequest.Result.ConnectionError, info.mWebStatus);
		assertEqual(HttpStatusCode.BadGateway, info.mCode);
		assertEqual(502L, info.mWebCode);
		info.mCallback(null);
		assertTrue(called, "回调应可正常保存并调用");
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合场景
	// ═════════════════════════════════════════════════════════════════

	// writeContent 空字符串 → write 空字节
	private static void testJsonWriteEmpty()
	{
		TestJsonPacket packet = new();
		packet.mWriteContent = "";
		byte[] bytes = packet.write();
		assertEqual(0, bytes.Length, "空 writeContent → 空字节");
	}

	// read 空字节 → mReadContent 空
	private static void testJsonReadNull()
	{
		TestJsonPacket packet = new();
		packet.read(new byte[0], 0);
		assertEqual("", packet.mReadContent, "read 空字节 → 空内容");
	}

	// 多轮 read 覆盖内容
	private static void testMultipleReadCycles()
	{
		TestJsonPacket packet = new();
		packet.read("first".toBytes(), 5);
		assertEqual("first", packet.mReadContent, "第一轮 read");
		packet.read("second".toBytes(), 6);
		assertEqual("second", packet.mReadContent, "第二轮 read 覆盖");
		packet.read("".toBytes(), 0);
		assertEqual("", packet.mReadContent, "第三轮 read 清空");
	}

	// Http resetProperty: mReadMessage 清空 + method 保持
	private static void testHttpResetProperty()
	{
		TestHttpPacket packet = new();
		packet.read("data");
		assertEqual("data", packet.mReadMessage, "read 前内容");
		packet.resetProperty();
		assertTrue(packet.mReadMessage == null, "resetProperty 后 mReadMessage null");
		assertEqual(HTTP_METHOD.GET, packet.getMethod(), "resetProperty 后 method 保持 GET");
	}

	// Json resetProperty: 内容清空
	private static void testJsonResetProperty()
	{
		TestJsonPacket packet = new();
		packet.mWriteContent = "{\"a\":1}";
		packet.read("resp".toBytes(), 4);
		packet.resetProperty();
		assertTrue(packet.mWriteContent == null, "resetProperty 后 writeContent null");
		assertTrue(packet.mReadContent == null, "resetProperty 后 readContent null");
	}

	// 两实例独立: 各自写读不互相影响
	private static void testJsonPacketInstancesIndependent()
	{
		TestJsonPacket a = new();
		TestJsonPacket b = new();
		a.mWriteContent = "{\"a\":1}";
		b.mWriteContent = "{\"b\":2}";
		assertEqual("{\"a\":1}", a.mWriteContent, "a 内容独立");
		assertEqual("{\"b\":2}", b.mWriteContent, "b 内容独立");
		a.read("first".toBytes(), 5);
		b.read("second".toBytes(), 6);
		assertEqual("first", a.mReadContent, "a 读取独立");
		assertEqual("second", b.mReadContent, "b 读取独立");
	}

	// 两次 read 空字节幂等
	private static void testJsonReadEmptyTwice()
	{
		TestJsonPacket packet = new();
		packet.read(new byte[0], 0);
		packet.read(new byte[0], 0);
		assertEqual("", packet.mReadContent, "两次空 read 结果一致");
	}

	private class TestJsonPacket : NetPacketJson
	{
		public string mWriteContent;
		public string mReadContent;
        public override void resetProperty()
        {
            base.resetProperty();
			mWriteContent = null;
			mReadContent = null;
        }
		public override string writeContent() { return mWriteContent; }
		public override void readContent(string str) { mReadContent = str; }
	}

	private class TestHttpPacket : NetPacketHttp
	{
		public string mReadMessage;
		public TestHttpPacket()
		{
			mURL = "http://unit.test/api";
			mMethod = HTTP_METHOD.GET;
		}
        public override void resetProperty()
        {
            base.resetProperty();
			mReadMessage = null;
        }
		public override string write() { return "payload"; }
		public override void read(string message) { mReadMessage = message; }
		public override int timeout() { return 1234; }
	}
}