#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;

// StreamBuffer 缓冲区测试
// 覆盖：addData / removeData / merge / clear / 溢出保护 / getDataLength / getBufferSize
public static class StreamBufferTest
{
    public static void Run()
    {
        testInit();
        testAddData();
        testAddDataWithOffset();
        testOverflow();
        testRemoveData();
        testRemoveOutOfRange();
        testClear();
        testMerge();
        testGetters();
    }

    // ─── init ────────────────────────────────────────────────────────────
    private static void testInit()
    {
        var buf = new StreamBuffer(64);
        assertEqual(64, buf.getBufferSize(),  "StreamBuffer init size=64");
        assertEqual(0,  buf.getDataLength(),  "StreamBuffer init dataLen=0");
        assertNotNull(buf.getData(),          "StreamBuffer getData!=null");
    }

    // ─── addData(byte[], count) ──────────────────────────────────────────
    private static void testAddData()
    {
        var buf = new StreamBuffer(16);
        byte[] src = new byte[] { 1, 2, 3, 4, 5 };

        bool ok = buf.addData(src, 5);
        assert(ok,                           "addData 返回 true");
        assertEqual(5, buf.getDataLength(),  "addData 后 dataLen=5");

        // 检查内容
        byte[] data = buf.getData();
        assertEqual((byte)1, data[0], "addData data[0]=1");
        assertEqual((byte)3, data[2], "addData data[2]=3");
        assertEqual((byte)5, data[4], "addData data[4]=5");

        // 追加第二段
        byte[] src2 = new byte[] { 10, 20 };
        ok = buf.addData(src2, 2);
        assert(ok,                           "addData 第二段 true");
        assertEqual(7, buf.getDataLength(),  "addData 第二段后 dataLen=7");
        assertEqual((byte)10, data[5],       "addData 第二段 data[5]=10");
    }

    // ─── addData(byte[], offset, count) ─────────────────────────────────
    private static void testAddDataWithOffset()
    {
        var buf = new StreamBuffer(16);
        byte[] src = new byte[] { 0, 0, 7, 8, 9 };

        bool ok = buf.addData(src, 2, 3);   // 取 src[2..4] = {7,8,9}
        assert(ok, "addData offset true");
        assertEqual(3, buf.getDataLength(), "addData offset dataLen=3");
        byte[] data = buf.getData();
        assertEqual((byte)7, data[0], "addData offset data[0]=7");
        assertEqual((byte)9, data[2], "addData offset data[2]=9");
    }

    // ─── 溢出（容量不足）─────────────────────────────────────────────────
    private static void testOverflow()
    {
        var buf = new StreamBuffer(4);
        byte[] big = new byte[5] { 1, 2, 3, 4, 5 };

        bool ok = buf.addData(big, 5);
        assert(!ok,                         "addData 溢出返回 false");
        assertEqual(0, buf.getDataLength(), "溢出后 dataLen 不变=0");
    }

    // ─── removeData ──────────────────────────────────────────────────────
    private static void testRemoveData()
    {
        var buf = new StreamBuffer(16);
        byte[] src = new byte[] { 1, 2, 3, 4, 5 };
        buf.addData(src, 5);

        // 移除中间：start=1, count=2 → 移除 data[1..2]={2,3}
        bool ok = buf.removeData(1, 2);
        assert(ok,                           "removeData 返回 true");
        assertEqual(3, buf.getDataLength(),  "removeData 后 dataLen=3");
        byte[] data = buf.getData();
        assertEqual((byte)1, data[0], "removeData 后 data[0]=1");
        assertEqual((byte)4, data[1], "removeData 后 data[1]=4");
        assertEqual((byte)5, data[2], "removeData 后 data[2]=5");
        // 尾部应被清零
        assertEqual((byte)0, data[3], "removeData 尾部清零 data[3]=0");

        // 移除头部
        buf.removeData(0, 1);
        assertEqual(2, buf.getDataLength(), "removeData head 后 dataLen=2");
        assertEqual((byte)4, buf.getData()[0], "removeData head 后 data[0]=4");
    }

    // ─── removeData 越界 ────────────────────────────────────────────────
    private static void testRemoveOutOfRange()
    {
        var buf = new StreamBuffer(16);
        byte[] src = new byte[] { 1, 2, 3 };
        buf.addData(src, 3);

        // start+count > dataLength → 返回 false
        bool ok = buf.removeData(2, 2);   // 2+2=4 > 3
        assert(!ok,                          "removeData 越界返回 false");
        assertEqual(3, buf.getDataLength(), "removeData 越界后 dataLen 不变");
    }

    // ─── clear ───────────────────────────────────────────────────────────
    private static void testClear()
    {
        var buf = new StreamBuffer(16);
        byte[] src = new byte[] { 1, 2, 3, 4 };
        buf.addData(src, 4);

        buf.clear();
        assertEqual(0, buf.getDataLength(), "clear 后 dataLen=0");

        // clear 后仍可继续写入
        bool ok = buf.addData(src, 2);
        assert(ok,                          "clear 后 addData ok");
        assertEqual(2, buf.getDataLength(), "clear 后 addData dataLen=2");
    }

    // ─── merge ───────────────────────────────────────────────────────────
    private static void testMerge()
    {
        var buf1 = new StreamBuffer(32);
        byte[] src1 = new byte[] { 1, 2, 3 };
        buf1.addData(src1, 3);

        var buf2 = new StreamBuffer(32);
        byte[] src2 = new byte[] { 4, 5, 6 };
        buf2.addData(src2, 3);

        bool ok = buf2.merge(buf1);
        assert(ok,                            "merge 返回 true");
        assertEqual(6, buf2.getDataLength(),  "merge 后 dataLen=6");
        assertEqual((byte)4, buf2.getData()[0], "merge 后 data[0]=4");
        assertEqual((byte)1, buf2.getData()[3], "merge 后 data[3]=1");
    }

    // ─── getters ─────────────────────────────────────────────────────────
	private static void testGetters()
	{
		// StreamBuffer 内部用 ByteArrayPoolThread，要求 size 为 2 的幂次
		var buf = new StreamBuffer(128);
		assertEqual(128, buf.getBufferSize(),   "getBufferSize=128");
		assertEqual(0,   buf.getDataLength(),   "getDataLength 初始=0");

		byte[] src = new byte[10];
		buf.addData(src, 10);
		assertEqual(10, buf.getDataLength(), "getDataLength 写入后=10");
		// bufferSize 不变
		assertEqual(128, buf.getBufferSize(), "getBufferSize 写入后仍=128");
	}
}
#endif
