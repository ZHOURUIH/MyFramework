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
		testExecuteSealedNoop();
		testResetPropertyKeepsUrl();
		testTimeoutOverrideIndependent();
		testMultiInstanceWriteIndependent();
		testReadAfterReset();
		testWriteDoesNotAffectUrl();
		testReadDoesNotAffectUrl();
		testExecuteDoesNotAffectUrl();
		testReadNullSafe();
		testWriteTwice();
		testReadDifferentParams();
		testExecuteAfterWriteRead();
		testResetPropertyThenExecute();
		testMultiInstanceReadIndependent();
		testExecuteAfterWrite();
		testReadAfterExecute();
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

	// 两实例 timeout 独立
	private static void testTimeoutOverrideIndependent()
	{
		TestHttpPacket a = new TestHttpPacket("http://a.com/api");
		TestHttpPacket b = new TestHttpPacket("http://b.com/api");
		assertEqual(9999, a.timeout(), "a timeout");
		assertEqual(9999, b.timeout(), "b timeout");
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

	// ═════════════════════════════════════════════════════════════════
	// 深度组合
	// ═════════════════════════════════════════════════════════════════

	// execute sealed 空操作
	private static void testExecuteSealedNoop()
	{
		NetPacketHttp packet = new NetPacketHttp();
		packet.execute();
		TestHttpPacket t = new TestHttpPacket("http://example.com/api");
		t.execute();
		// 无异常即通过
	}

	// resetProperty 保留 URL(注释明确 mURL 不重置)
	private static void testResetPropertyKeepsUrl()
	{
		TestHttpPacket packet = new TestHttpPacket("http://keep.com/api");
		packet.resetProperty();
		assertEqual("http://keep.com/api", packet.getUrl(), "resetProperty 后 URL 保留");
	}

	// 两实例 write 独立
	private static void testMultiInstanceWriteIndependent()
	{
		TestHttpPacket a = new TestHttpPacket("http://a.com/api");
		TestHttpPacket b = new TestHttpPacket("http://b.com/api");
		assertEqual("hello", a.write(), "a write 结果");
		assertEqual("hello", b.write(), "b write 结果");
	}

	// reset 后 read 计数仍递增
	private static void testReadAfterReset()
	{
		TestHttpPacket packet = new TestHttpPacket("http://example.com/api");
		packet.read("a");
		packet.resetProperty();
		packet.read("b");
		assertEqual(1, packet.mReadCount, "resetProperty 清计数(mReadCount=0) 后 read 重新计数");
	}

	// ═════════════════════════════════════════════════════════════════
	// write/read/execute 不影响 URL
	// ═════════════════════════════════════════════════════════════════

	// write 后 URL 保留
	private static void testWriteDoesNotAffectUrl()
	{
		TestHttpPacket packet = new TestHttpPacket("http://write.com/api");
		packet.write();
		assertEqual("http://write.com/api", packet.getUrl(), "write 后 URL 保留");
	}

	// read 后 URL 保留
	private static void testReadDoesNotAffectUrl()
	{
		TestHttpPacket packet = new TestHttpPacket("http://read.com/api");
		packet.read("msg");
		assertEqual("http://read.com/api", packet.getUrl(), "read 后 URL 保留");
	}

	// execute 后 URL 保留
	private static void testExecuteDoesNotAffectUrl()
	{
		TestHttpPacket packet = new TestHttpPacket("http://exec.com/api");
		packet.execute();
		assertEqual("http://exec.com/api", packet.getUrl(), "execute 后 URL 保留");
	}

	// ═════════════════════════════════════════════════════════════════
	// read/write/execute 组合
	// ═════════════════════════════════════════════════════════════════

	// read(null) 空安全
	private static void testReadNullSafe()
	{
		TestHttpPacket packet = new TestHttpPacket("http://null.com/api");
		packet.read(null);
		assertEqual(1, packet.mReadCount, "read(null) 计数 1");
	}

	// write 多次调用
	private static void testWriteTwice()
	{
		TestHttpPacket packet = new TestHttpPacket("http://twice.com/api");
		assertEqual("hello", packet.write(), "第一次 write");
		assertEqual("hello", packet.write(), "第二次 write 相同");
	}

	// read 不同参数计数
	private static void testReadDifferentParams()
	{
		TestHttpPacket packet = new TestHttpPacket("http://params.com/api");
		packet.read("a");
		packet.read("b");
		packet.read("");
		assertEqual(3, packet.mReadCount, "3 次 read 计数 3");
	}

	// write→read→execute 顺序
	private static void testExecuteAfterWriteRead()
	{
		TestHttpPacket packet = new TestHttpPacket("http://seq.com/api");
		packet.write();
		packet.read("msg");
		packet.execute();
		assertEqual(1, packet.mReadCount, "顺序执行后 read 计数 1");
	}

	// resetProperty 后 execute
	private static void testResetPropertyThenExecute()
	{
		TestHttpPacket packet = new TestHttpPacket("http://reset.com/api");
		packet.read("before");
		packet.resetProperty();
		packet.execute();
		assertEqual(0, packet.mReadCount, "resetProperty 清计数后 execute 无 read");
	}

	// 两实例 read 计数独立
	private static void testMultiInstanceReadIndependent()
	{
		TestHttpPacket a = new TestHttpPacket("http://a2.com/api");
		TestHttpPacket b = new TestHttpPacket("http://b2.com/api");
		a.read("x");
		assertEqual(1, a.mReadCount, "a 计数 1");
		assertEqual(0, b.mReadCount, "b 计数 0");
	}

	// write 后 execute
	private static void testExecuteAfterWrite()
	{
		TestHttpPacket packet = new TestHttpPacket("http://w.com/api");
		packet.write();
		packet.execute();
		assertEqual("hello", packet.write(), "execute 后 write 仍返回 hello");
	}

	// execute 后 read
	private static void testReadAfterExecute()
	{
		TestHttpPacket packet = new TestHttpPacket("http://r.com/api");
		packet.execute();
		packet.read("after");
		assertEqual(1, packet.mReadCount, "execute 后 read 计数 1");
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

