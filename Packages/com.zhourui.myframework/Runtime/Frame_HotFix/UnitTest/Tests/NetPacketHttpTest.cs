using static TestAssert;

// NetPacketHttp: HTTP 消息基类(纯逻辑, 直接 new 可测)
// 默认: mMethod=POST / write()=null / timeout()=10000 / execute() 空
// resetProperty 不重置 mURL/mMethod(注释说明: 在子类构造中赋值)
public static class NetPacketHttpTest
{
	public static void Run()
	{
		testDefaultMethodPost();
		testDefaultWriteNull();
		testDefaultTimeout();
		testReadEmptySafe();
		testExecuteNoOp();
		testResetPropertySafe();
		testGetUrlCustom();
		testWriteReadOverride();
		testMultipleReadCycles();
		testTimeoutCustom();
		testMultiInstanceUrlIndependent();
		testWriteReadInterleaved();
		testGetUrlEmpty();
	}

	// 默认请求方式 POST
	private static void testDefaultMethodPost()
	{
		NetPacketHttp packet = new NetPacketHttp();
		assertEqual(HTTP_METHOD.POST, packet.getMethod(), "默认请求方式 POST");
	}

	// 默认 write() 返回 null(基类不实现具体协议)
	private static void testDefaultWriteNull()
	{
		NetPacketHttp packet = new NetPacketHttp();
		assertTrue(packet.write() == null, "基类 write() 默认 null");
	}

	// 默认超时 10000
	private static void testDefaultTimeout()
	{
		NetPacketHttp packet = new NetPacketHttp();
		assertEqual(10000, packet.timeout(), "默认超时 10000");
	}

	// read(null) 空安全
	private static void testReadEmptySafe()
	{
		NetPacketHttp packet = new NetPacketHttp();
		packet.read(null);
		packet.read("");
		// 无异常即通过
	}

	// execute 空操作(sealed, 子类通过 lambda 处理)
	private static void testExecuteNoOp()
	{
		NetPacketHttp packet = new NetPacketHttp();
		packet.execute();
		// 无异常即通过
	}

	// resetProperty 后 method 保持 POST(mURL/mMethod 不重置)
	private static void testResetPropertySafe()
	{
		NetPacketHttp packet = new NetPacketHttp();
		packet.resetProperty();
		assertEqual(HTTP_METHOD.POST, packet.getMethod(), "resetProperty 后 method 保持 POST");
	}

	// 子类构造设置 mURL → getUrl 读回
	private static void testGetUrlCustom()
	{
		TestHttpPacket packet = new TestHttpPacket("http://example.com/api");
		assertEqual("http://example.com/api", packet.getUrl(), "子类构造设置 URL 读回");
		assertEqual(HTTP_METHOD.GET, packet.getMethod(), "子类构造可改请求方式");
	}

	// 子类 override write/read 生效
	private static void testWriteReadOverride()
	{
		TestHttpPacket packet = new TestHttpPacket("http://example.com/api");
		assertEqual("hello", packet.write(), "子类 override write 生效");
		int before = packet.mReadCount;
		packet.read("response");
		assertEqual(before + 1, packet.mReadCount, "子类 override read 生效");
	}

	// 多轮 read 计数递增
	private static void testMultipleReadCycles()
	{
		TestHttpPacket packet = new TestHttpPacket("http://example.com/api");
		packet.read("a");
		packet.read("b");
		packet.read("c");
		assertEqual(3, packet.mReadCount, "三轮 read 计数 3");
	}

	// 子类 override timeout 生效
	private static void testTimeoutCustom()
	{
		TestHttpPacket packet = new TestHttpPacket("http://example.com/api");
		assertEqual(9999, packet.timeout(), "子类 override timeout 生效");
	}

	// 多实例 URL 独立
	private static void testMultiInstanceUrlIndependent()
	{
		TestHttpPacket a = new TestHttpPacket("http://a.com/api");
		TestHttpPacket b = new TestHttpPacket("http://b.com/api");
		assertEqual("http://a.com/api", a.getUrl(), "a URL");
		assertEqual("http://b.com/api", b.getUrl(), "b URL");
		assertFalse(ReferenceEquals(a.getUrl(), b.getUrl()), "URL 字符串不共享引用");
	}

	// write/read 交替
	private static void testWriteReadInterleaved()
	{
		TestHttpPacket packet = new TestHttpPacket("http://example.com/api");
		packet.read("a");
		assertEqual("hello", packet.write(), "write 结果稳定");
		packet.read("b");
		packet.read("c");
		assertEqual(3, packet.mReadCount, "交替后 read 计数 3");
	}

	// 空 URL 构造
	private static void testGetUrlEmpty()
	{
		TestHttpPacket packet = new TestHttpPacket("");
		assertEqual("", packet.getUrl(), "空 URL 读回");
	}
}

// 测试辅助: 暴露 protected 字段 + 模拟子类协议
public class TestHttpPacket : NetPacketHttp
{
	public int mReadCount;

	public TestHttpPacket(string url)
	{
		mURL = url;
		mMethod = HTTP_METHOD.GET;
	}

	public override string write()
	{
		return "hello";
	}

	public override void read(string message)
	{
		++mReadCount;
	}

	public override int timeout()
	{
		return 9999;
	}

	public override void resetProperty()
	{
		base.resetProperty();
		mReadCount = 0;
	}
}
