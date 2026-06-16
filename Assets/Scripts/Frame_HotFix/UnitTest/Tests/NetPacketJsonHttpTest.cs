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