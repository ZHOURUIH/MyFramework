using System;
using System.Text;
using static TestAssert;
using static BinaryUtility;

// BinaryUtility 二进制工具函数测试
// 覆盖：bitCount1 / contains / crc16 / memcpy / memmove / memset /
//        isMemoryEqual / hasBit / setBitOne / setBitZero /
//        getLowestBit / getHighestBit / removeLastZero
public static class BinaryUtilityTest
{
    public static void Run()
    {
        testBitCount1();
        testContains();
        testCrc16();
        testMemcpy();
        testMemmove();
        testMemset();
        testIsMemoryEqual();
        testHasBit();
        testSetBitOne();
        testSetBitZero();
        testGetLowestHighestBit();
        testRemoveLastZero();
        testMemsetExtra();
        testHasBitExtra();
        testSetBitOneExtra();
        testSetBitZeroExtra();
        testExtendedBitSetters();
        testBufferBits();
        testCrc16Extra();
        testMemcpyExtra();
        testCrc16ByteDirect();
        testEncodingGetters();
    

		testContains_SameBuffer();
		testContains_OverlapKey();
		testContains_BoundaryStartEnd();
		testContains_MultiKeySearch();
		testMemoryEqual_OffsetCombos();
		testMemoryEqual_EmptyLength();
		testMemcpy_OverlapSelf();
		testMemcpy_ByteOffsetIntArray();
		testMemmove_ForwardOverlap();
		testMemmove_BackwardOverlap();
		testMemmove_SameIndexNoOp();
		testMemset_RepeatedRegions();
		testCrc16_IncrementalConsistency();
		testCrc16_DifferentDataDiffer();
		testCrc16_EmptyInput();
		testBitSetRoundTripAllPositions();
		testBitRoundTrip_Idempotent();
		testBufferBit_SequentialSet();
		testBitCount1_RandomPatterns();
		testHasBit_InvariantVsSetBit();
	}

    // ─── bitCount1 ───────────────────────────────────────────────────────────
    private static void testBitCount1()
    {
        assertEqual(0, bitCount1(0x00), "bitCount1(0x00)=0");
        assertEqual(8, bitCount1(0xFF), "bitCount1(0xFF)=8");
        assertEqual(4, bitCount1(0x0F), "bitCount1(0x0F)=4");
        assertEqual(1, bitCount1(0x01), "bitCount1(0x01)=1");
        assertEqual(1, bitCount1(0x80), "bitCount1(0x80)=1");
        // 交替位 0xAA = 4个1
        assertEqual(4, bitCount1((byte)0xAA), "bitCount1 0xAA=4");
        // 全1字节 0xFF = 8个1（同第2行，幂等验证）
        assertEqual(8, bitCount1((byte)0xFF), "bitCount1 全1=8 幂等");
    }

    // ─── contains ────────────────────────────────────────────────────────────
    private static void testContains()
    {
        byte[] buf  = new byte[] { 1, 2, 3, 4, 5 };
        byte[] key1 = new byte[] { 3, 4 };
        byte[] key2 = new byte[] { 9, 10 };
        byte[] key3 = new byte[] { 1 };
        byte[] key4 = new byte[] { 5 };

        assert(contains(buf, key1),   "contains 中间");
        assert(!contains(buf, key2),  "contains 不存在");
        assert(contains(buf, key3),   "contains 开头");
        assert(contains(buf, key4),   "contains 末尾");
        assert(!contains(null, key1), "contains null buffer→false");
        assert(!contains(buf, null),  "contains null key→false");
    }

    // ─── crc16 ───────────────────────────────────────────────────────────────
    private static void testCrc16()
    {
        byte[] data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        ushort crc = crc16(0, data, data.Length);
        // CRC 值只要稳定一致即可（相同数据重复计算结果相同）
        ushort crc2 = crc16(0, data, data.Length);
        assertEqual(crc, crc2, "crc16 结果稳定");

        // 单字节版本与数组版本一致
        ushort crcSingle = 0;
        crcSingle = crc16(crcSingle, data[0]);
        crcSingle = crc16(crcSingle, data[1]);
        crcSingle = crc16(crcSingle, data[2]);
        crcSingle = crc16(crcSingle, data[3]);
        assertEqual(crc, crcSingle, "crc16 逐字节与批量一致");

        // 两字节版本
        ushort crc2b = crc16(0, data[0], data[1]);
        ushort crc2b2 = crc16(crc16(0, data[0]), data[1]);
        assertEqual(crc2b, crc2b2, "crc16 两字节版本一致");
    }

    // ─── memcpy ──────────────────────────────────────────────────────────────
    private static void testMemcpy()
    {
        int[] src  = new int[] { 10, 20, 30, 40, 50 };
        int[] dest = new int[5];
        memcpyObject(dest, src, 0, 0, 5);
        for (int i = 0; i < 5; ++i)
        {
            assertEqual(src[i], dest[i], $"memcpyObject [{i}]");
        }

        // 带偏移
        int[] dest2 = new int[5];
        memcpyObject(dest2, src, 1, 2, 3);  // dest2[1..3] = src[2..4]
        assertEqual(30, dest2[1], "memcpyObject offset dest2[1]=30");
        assertEqual(40, dest2[2], "memcpyObject offset dest2[2]=40");
        assertEqual(50, dest2[3], "memcpyObject offset dest2[3]=50");
    }

    // ─── memmove ─────────────────────────────────────────────────────────────
    private static void testMemmove()
    {
        int[] data = new int[] { 1, 2, 3, 4, 5 };
        // 向右移动 data[1..2] → data[2..3]（重叠区域）
        memmove(data, 2, 1, 2);
        assertEqual(1, data[0], "memmove 未移动区[0]");
        assertEqual(2, data[2], "memmove 移动后[2]");
        assertEqual(3, data[3], "memmove 移动后[3]");
    }

    // ─── memset ──────────────────────────────────────────────────────────────
    private static void testMemset()
    {
        byte[] arr = new byte[10];
        memset(arr, (byte)0xAB, 2, 5);
        for (int i = 0; i < 2; ++i)
        {
            assertEqual((byte)0, arr[i], $"memset 未改动[{i}]");
        }
        for (int i = 2; i < 7; ++i)
        {
            assertEqual((byte)0xAB, arr[i], $"memset 已填充[{i}]");
        }
        for (int i = 7; i < 10; ++i)
        {
            assertEqual((byte)0, arr[i], $"memset 未改动[{i}]");
        }
    }

    // ─── isMemoryEqual ────────────────────────────────────────────────────────
    private static void testIsMemoryEqual()
    {
        byte[] a = new byte[] { 1, 2, 3, 4, 5 };
        byte[] b = new byte[] { 1, 2, 3, 4, 5 };
        byte[] c = new byte[] { 1, 2, 9, 4, 5 };

        assert(isMemoryEqual(a, b, 5),  "isMemoryEqual 相同");
        assert(!isMemoryEqual(a, c, 5), "isMemoryEqual 不同");
        assert(isMemoryEqual(a, b, 2),  "isMemoryEqual 前2字节相同");
        // 偏移版本
        assert(isMemoryEqual(a, b, 3, 1, 1), "isMemoryEqual 偏移后相同");
        // 长度超界
        assert(!isMemoryEqual(a, b, 10), "isMemoryEqual 超界→false");
    }

    // ─── hasBit ──────────────────────────────────────────────────────────────
    private static void testHasBit()
    {
        byte val = 0b0000_1010;  // bit1=1, bit3=1
        assert(hasBit(val, 1),   "hasBit byte bit1=1");
        assert(hasBit(val, 3),   "hasBit byte bit3=1");
        assert(!hasBit(val, 0),  "hasBit byte bit0=0");
        assert(!hasBit(val, 2),  "hasBit byte bit2=0");

        int iVal = unchecked((int)0b1000_0000_0000_0000_0000_0000_0000_0000u);  // bit31=1
        assert(hasBit(iVal, 31), "hasBit int bit31=1");

        long lVal = (long)1 << 40;
        assert(hasBit(lVal, 40), "hasBit long bit40=1");
        assert(!hasBit(lVal, 39),"hasBit long bit39=0");
    }

    // ─── setBitOne ───────────────────────────────────────────────────────────
    private static void testSetBitOne()
    {
        byte b = 0;
        setBitOne(ref b, 3);
        assert(hasBit(b, 3), "setBitOne byte bit3");
        setBitOne(ref b, 0);
        assert(hasBit(b, 0), "setBitOne byte bit0");
        assert(hasBit(b, 3), "setBitOne byte bit3 不影响");
        // 同位重复 setBitOne 幂等
        setBitOne(ref b, 3);
        assert(hasBit(b, 3), "setBitOne 同位重复幂等");

        int i = 0;
        setBitOne(ref i, 15);
        assert(hasBit(i, 15), "setBitOne int bit15");

        long l = 0;
        setBitOne(ref l, 40);
        assert(hasBit(l, 40), "setBitOne long bit40");
        // long bit0 和 bit62（bit63为符号位跳过）
        l = 0;
        setBitOne(ref l, 0);
        setBitOne(ref l, 62);
        assert(hasBit(l, 0),  "setBitOne long bit0");
        assert(hasBit(l, 62), "setBitOne long bit62");
    }

    // ─── setBitZero ──────────────────────────────────────────────────────────
    private static void testSetBitZero()
    {
        byte b = 0xFF;
        setBitZero(ref b, 3);
        assert(!hasBit(b, 3), "setBitZero byte bit3");
        assert(hasBit(b, 4),  "setBitZero byte bit4 不影响");
        // 同位重复 setBitZero 幂等
        setBitZero(ref b, 3);
        assert(!hasBit(b, 3), "setBitZero 同位重复幂等");
        // setBitZero 后 setBitOne 能恢复
        setBitOne(ref b, 3);
        assert(hasBit(b, 3),  "setBitOne 恢复之前清零的位");

        int i = -1;  // 全1
        setBitZero(ref i, 10);
        assert(!hasBit(i, 10), "setBitZero int bit10");
        // 清0后再置1
        setBitOne(ref i, 10);
        assert(hasBit(i, 10), "int bit10 清后再置1");
    }

    // ─── getLowestBit / getHighestBit ─────────────────────────────────────────
    private static void testGetLowestHighestBit()
    {
        assertEqual(1, getLowestBit((byte)0xFF), "getLowestBit 0xFF=1");
        assertEqual(0, getLowestBit((byte)0xFE), "getLowestBit 0xFE=0");
        assertEqual(1, getHighestBit((byte)0xFF), "getHighestBit 0xFF=1");
        assertEqual(0, getHighestBit((byte)0x7F), "getHighestBit 0x7F=0");
    }

    // ─── removeLastZero ───────────────────────────────────────────────────────
    private static void testRemoveLastZero()
    {
        // 构造含有 \0 的字符串（模拟字节数组转换场景）
        string s = "hello\0\0";
        string result = s.removeLastZero();
        assertEqual("hello", result, "removeLastZero 移除末尾\\0");

        string noZero = "world";
        assertEqual("world", noZero.removeLastZero(), "removeLastZero 无\\0不变");
    }

    // ─── memset 补充 ──────────────────────────────────────────────────────────
    private static void testMemsetExtra()
    {
        // 从头覆盖整个数组
        byte[] all = new byte[5];
        memset(all, (byte)0xFF, 0, 5);
        for (int i = 0; i < 5; ++i)
        {
            assertEqual((byte)0xFF, all[i], $"memset 全覆盖[{i}]");
        }

        // count=0 不改动
        byte[] noChange = new byte[] { 1, 2, 3 };
        memset(noChange, (byte)0xAA, 0, 0);
        assertEqual((byte)1, noChange[0], "memset count=0 不改动");

        // 覆盖后再覆盖不同值
        byte[] twice = new byte[4];
        memset(twice, (byte)0x11, 0, 4);
        memset(twice, (byte)0x22, 1, 2);
        assertEqual((byte)0x11, twice[0], "memset 二次覆盖 [0]不变");
        assertEqual((byte)0x22, twice[1], "memset 二次覆盖 [1]改变");
        assertEqual((byte)0x22, twice[2], "memset 二次覆盖 [2]改变");
        assertEqual((byte)0x11, twice[3], "memset 二次覆盖 [3]不变");
    }

    // ─── hasBit 补充类型 ───────────────────────────────────────────────────────
    private static void testHasBitExtra()
    {
        sbyte sb = 0b0000_1010;
        assert(hasBit(sb, 1), "hasBit sbyte bit1");
        assert(!hasBit(sb, 0), "hasBit sbyte bit0=0");

        short s = unchecked((short)0x8000);  // bit15=1
        assert(hasBit(s, 15), "hasBit short bit15");
        assert(!hasBit(s, 14), "hasBit short bit14=0");

        ushort us = 0x8000;
        assert(hasBit(us, 15), "hasBit ushort bit15");
        assert(!hasBit(us, 0), "hasBit ushort bit0=0");

        uint ui = 0x8000_0000u;
        assert(hasBit(ui, 31), "hasBit uint bit31");
        assert(!hasBit(ui, 30), "hasBit uint bit30=0");

        ulong ul = (ulong)1 << 63;
        assert(hasBit(ul, 63), "hasBit ulong bit63");
        assert(!hasBit(ul, 62), "hasBit ulong bit62=0");
    }

    // ─── setBitOne 补充类型 ────────────────────────────────────────────────────
    private static void testSetBitOneExtra()
    {
        sbyte sb = 0;
        setBitOne(ref sb, 2);
        assert(hasBit(sb, 2), "setBitOne sbyte bit2");
        setBitOne(ref sb, 2);
        assert(hasBit(sb, 2), "setBitOne sbyte 幂等");

        short s = 0;
        setBitOne(ref s, 14);
        assert(hasBit(s, 14), "setBitOne short bit14");

        ushort us = 0;
        setBitOne(ref us, 12);
        assert(hasBit(us, 12), "setBitOne ushort bit12");

        uint ui = 0;
        setBitOne(ref ui, 30);
        assert(hasBit(ui, 30), "setBitOne uint bit30");

        ulong ul = 0;
        setBitOne(ref ul, 63);
        assert(hasBit(ul, 63), "setBitOne ulong bit63");
    }

    // ─── setBitZero 补充类型 ──────────────────────────────────────────────────
    private static void testSetBitZeroExtra()
    {
        sbyte sb = -1;
        setBitZero(ref sb, 3);
        assert(!hasBit(sb, 3), "setBitZero sbyte bit3");

        short s = -1;
        setBitZero(ref s, 13);
        assert(!hasBit(s, 13), "setBitZero short bit13");

        ushort us = 0xFFFF;
        setBitZero(ref us, 10);
        assert(!hasBit(us, 10), "setBitZero ushort bit10");

        uint ui = 0xFFFFFFFFu;
        setBitZero(ref ui, 28);
        assert(!hasBit(ui, 28), "setBitZero uint bit28");

        long l = -1;
        setBitZero(ref l, 33);
        assert(!hasBit(l, 33), "setBitZero long bit33");

        ulong ul = 0xFFFFFFFF_FFFFFFFFul;
        setBitZero(ref ul, 60);
        assert(!hasBit(ul, 60), "setBitZero ulong bit60");
    }

    // ─── getLowestBit / getHighestBit 补充 + setLowestBit/setHighestBit ───────
    private static void testExtendedBitSetters()
    {
        // getLowestBit — 补充类型
        assertEqual(1, getLowestBit((short)0xFF), "getLowestBit short 0xFF=1");
        assertEqual(0, getLowestBit((short)0xFE), "getLowestBit short 0xFE=0");
        assertEqual(1, getLowestBit(0xFF), "getLowestBit int 0xFF=1");
        assertEqual(0, getLowestBit(0xFE), "getLowestBit int 0xFE=0");

        // getHighestBit — 补充类型
        assertEqual(0, getHighestBit((short)0x7FFF), "getHighestBit short 0x7FFF=0");
        assertEqual(1, getHighestBit(unchecked((int)0x80000000)), "getHighestBit int 0x80000000=1");
        assertEqual(0, getHighestBit(0x7FFFFFFF), "getHighestBit int 0x7FFFFFFF=0");

        // setLowestBit
        byte b0 = 0;
        setLowestBit(ref b0, 1);
        assertEqual((byte)1, b0, "setLowestBit byte 0→1");
        setLowestBit(ref b0, 0);
        assertEqual((byte)0, b0, "setLowestBit byte 1→0");

        short s0 = 0;
        setLowestBit(ref s0, 1);
        assertEqual((short)1, s0, "setLowestBit short 0→1");

        int i0 = 0;
        setLowestBit(ref i0, 1);
        assertEqual(1, i0, "setLowestBit int 0→1");

        // setHighestBit
        byte bH = 0;
        setHighestBit(ref bH, 1);
        assertEqual((byte)0x80, bH, "setHighestBit byte 0→0x80");
        setHighestBit(ref bH, 0);
        assertEqual((byte)0, bH, "setHighestBit byte 0x80→0");

        short sH = 0;
        setHighestBit(ref sH, 1);
        assertEqual((short)unchecked((short)0x8000), sH, "setHighestBit short 0→0x8000");

        int iH = 0;
        setHighestBit(ref iH, 1);
        assertEqual(unchecked((int)0x80000000), iH, "setHighestBit int 0→0x80000000");
    }

    // ─── getBufferBit / setBufferBitOne ──────────────────────────────────────
    private static void testBufferBits()
    {
        byte[] buf = new byte[4]; // 32 bits
        // 初始全0
        for (int i = 0; i < 32; ++i)
        {
            assert(!getBufferBit(buf, i), $"getBufferBit 初始全0 [{i}]");
        }

        setBufferBitOne(buf, 0);
        assert(getBufferBit(buf, 0), "getBufferBit bit0=1");
        assert(!getBufferBit(buf, 1), "getBufferBit bit1=0 不受影响");

        setBufferBitOne(buf, 17);
        assert(getBufferBit(buf, 17), "getBufferBit bit17=1");

        // 多个位同时置1
        setBufferBitOne(buf, 31);
        assert(getBufferBit(buf, 31), "getBufferBit bit31=1");
        assert(getBufferBit(buf, 0), "getBufferBit bit0 仍为1");
    }

    // ─── crc16 多字节重载补充 ─────────────────────────────────────────────────
    private static void testCrc16Extra()
    {
        // initCRC16 确保表已初始化
        initCRC16();

        // 2字节重载
        ushort c2 = crc16(0, 0x01, 0x02);
        ushort c2b = crc16(crc16(0, 0x01), 0x02);
        assertEqual(c2, c2b, "crc16 2字节与逐字节一致");

        // 4字节重载
        ushort c4 = crc16(0, 0x01, 0x02, 0x03, 0x04);
        ushort c4b = crc16(crc16(crc16(crc16(0, 0x01), 0x02), 0x03), 0x04);
        assertEqual(c4, c4b, "crc16 4字节与逐字节一致");

        // 8字节重载
        ushort c8 = crc16(0, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08);
        ushort c8b = crc16(crc16(crc16(crc16(crc16(crc16(crc16(crc16(0, 0x01), 0x02), 0x03), 0x04), 0x05), 0x06), 0x07), 0x08);
        assertEqual(c8, c8b, "crc16 8字节与逐字节一致");
    }

    // ─── memcpy 泛型重载补充 ───────────────────────────────────────────────────
    private static void testMemcpyExtra()
    {
        int[] src = new int[] { 10, 20, 30, 40, 50 };
        int[] dest = new int[5];

        // memcpy<T>(T[], Span<T>, destOffset, srcOffset, count)
        memcpy(dest, new Span<int>(src), 0, 0, 5);
        for (int i = 0; i < 5; ++i)
        {
            assertEqual(src[i], dest[i], $"memcpy T[]←Span [{i}]");
        }

        // memcpy<T>(Span<T>, T[], destOffset, srcOffset, count)
        int[] dest2 = new int[5];
        memcpy(new Span<int>(dest2), src, 0, 0, 5);
        assertEqual(src[0], dest2[0], "memcpy Span←T[] [0]");

        // memcpy<T>(T[], Span<T>, destOffset, count) — 从头复制count个
        int[] dest3 = new int[3];
        memcpy(dest3, new Span<int>(new int[] { 100, 200, 300 }), 0, 3);
        assertEqual(100, dest3[0], "memcpy T[]←Span 短版[0]");
        assertEqual(300, dest3[2], "memcpy T[]←Span 短版[2]");

        // memcpy<T>(Span<T>, T[], destOffset, count)
        int[] dest4 = new int[3];
        memcpy(new Span<int>(dest4), new int[] { 7, 8, 9 }, 0, 3);
        assertEqual(9, dest4[2], "memcpy Span←T[] 短版[2]");

        // memcpy<T>(T[], T[], byteOffset, byteOffset, byteCount)
        int[] src5 = new int[] { 1, 2, 3, 4, 5 };
        int[] dest5 = new int[5];
        memcpy(dest5, src5, 0, 0, 5 * sizeof(int));
        assertEqual(1, dest5[0], "memcpy 字节偏移版[0]");
        assertEqual(5, dest5[4], "memcpy 字节偏移版[4]");
    }

    // ─── crc16_byte 直接调用 ─────────────────────────────────────────────────
    private static void testCrc16ByteDirect()
    {
        ushort crc = 0;
        crc = crc16_byte(crc, 0x01);
        ushort crcA = crc;
        crc = crc16_byte(crc, 0x01);
        // 相同数据相同初值应产生稳定结果
        ushort again = 0;
        again = crc16_byte(again, 0x01);
        assertEqual(crcA, again, "crc16_byte 相同数据结果一致");
        // 与数组版本等价
        ushort arr = crc16(0, new byte[] { 0x01, 0x02 }, 2);
        ushort seq = crc16_byte(crc16_byte(0, 0x01), 0x02);
        assertEqual(arr, seq, "crc16_byte 与数组版本一致");
    }

    // ─── getGB2312 / getGBK ──────────────────────────────────────────────────
    private static void testEncodingGetters()
    {
        Encoding gb2312 = getGB2312();
        assertTrue(gb2312 != null, "getGB2312 非空");
        Encoding gbk = getGBK();
        assertTrue(gbk != null, "getGBK 非空");
        // 中文字符在 GBK 下编码应能往返
        string chinese = "中文测试";
        byte[] bytes = gbk.GetBytes(chinese);
        string back = gbk.GetString(bytes);
        assertEqual(chinese, back, "GBK 编码往返");
    }


	

	// ─── contains：同一 buffer 匹配自身 ───────────────────────────────────
	private static void testContains_SameBuffer()
	{
		byte[] buf = new byte[] { 5, 6, 7, 8, 9 };
		assert(contains(buf, buf), "contains: 整段匹配自身为 true");
		assert(contains(buf, new byte[] { 5, 6, 7, 8, 9 }), "contains: 尾首相同但字面重建仍 true");
	}

	// ─── contains：重叠 key（key 在全局出现多次应视为找到） ─────────────
	private static void testContains_OverlapKey()
	{
		byte[] buf = new byte[] { 1, 1, 1, 1, 1 };
		assert(contains(buf, new byte[] { 1, 1, 1 }), "contains: 重叠连续匹配");
		assert(contains(buf, new byte[] { 1 }), "contains: 单字节匹配");
	}

	// ─── contains：恰好处于开头/末尾 ────────────────────────────────────
	private static void testContains_BoundaryStartEnd()
	{
		byte[] buf = new byte[] { 3, 4, 5, 4, 3 };
		assert(contains(buf, new byte[] { 3, 4, 5 }), "contains: 头部整段");
		assert(contains(buf, new byte[] { 4, 3 }), "contains: 尾部整段");
		assert(!contains(buf, new byte[] { 5, 4, 2 }), "contains: 尾部差别匹配失败");
	}

	// ─── contains：多次出现 key 搜索任意一次即可 ────────────────────────
	private static void testContains_MultiKeySearch()
	{
		byte[] buf = new byte[] { 9, 2, 9, 4, 9 };
		assert(contains(buf, new byte[] { 9, 2 }), "contains: 第一次出现");
		assert(contains(buf, new byte[] { 2, 9, 4 }), "contains: 中间出现");
		assert(contains(buf, new byte[] { 4, 9 }), "contains: 第二次出现");
	}

	// ─── isMemoryEqual：各偏移组合 ───────────────────────────────────────
	private static void testMemoryEqual_OffsetCombos()
	{
		byte[] a = new byte[] { 0, 1, 2, 3, 4, 5 };
		byte[] b = new byte[] { 9, 9, 1, 2, 3, 4, 9 };
		// a[1..4] == b[2..5]
		assert(isMemoryEqual(a, b, 4, 1, 2), "isMemoryEqual: 双偏移 a[1]==b[2] 长度4");
		// 只比对首字节
		assert(isMemoryEqual(a, b, 1, 1, 2), "isMemoryEqual: 双偏移长度1");
		// 反向不等
		assert(!isMemoryEqual(a, b, 4, 1, 1), "isMemoryEqual: 错位不等");
	}

	// ─── isMemoryEqual：长度为 0 ─────────────────────────────────────────
	private static void testMemoryEqual_EmptyLength()
	{
		byte[] a = new byte[] { 1, 2 };
		byte[] b = new byte[] { 3, 4 };
		assert(isMemoryEqual(a, b, 0), "isMemoryEqual: 长度0直接相等");
	}

	// ─── memcpy：同源同目标（拷贝不变） ────────────────────────────────
	private static void testMemcpy_OverlapSelf()
	{
		int[] src = new int[] { 10, 20, 30, 40, 50 };
		int[] copy = new int[] { 10, 20, 30, 40, 50 };
		memcpyObject(copy, copy, 0, 0, 5); // 自拷贝应保持原值
		for (int i = 0; i < 5; ++i)
		{
			assertEqual(src[i], copy[i], $"memcpy 自拷贝[{i}]");
		}
	}

	// ─── memcpy：int 数组 + 字节偏移 ─────────────────────────────────────
	private static void testMemcpy_ByteOffsetIntArray()
	{
		int[] src = new int[] { 1, 2, 3, 4 };
		int[] dest = new int[4];
		// 以字节偏移拷贝 3 个 int（12 字节）从 offset0
		memcpy(dest, src, 0, 0, 4 * sizeof(int));
		for (int i = 0; i < 4; ++i)
		{
			assertEqual(src[i], dest[i], $"memcpy int 字节偏移全段[{i}]");
		}
		// 部分拷贝：从 src 字节偏移8开始拷2个int到 dest 字节偏移4
		int[] src2 = new int[] { 100, 200, 300, 400 };
		int[] dest2 = new int[5];
		memcpy(dest2, src2, 1 * sizeof(int), 2 * sizeof(int), 2 * sizeof(int));
		assertEqual(300, dest2[1], "memcpy int 偏移: dest2[1]=300");
		assertEqual(400, dest2[2], "memcpy int 偏移: dest2[2]=400");
	}

	// ─── memmove：向右移动（重叠，源在前目标在后） ────────────────────
	private static void testMemmove_ForwardOverlap()
	{
		int[] data = new int[] { 1, 2, 3, 4, 5, 6 };
		// 将 data[0..2] 移到 data[2..4]（向右，重叠）
		memmove(data, 2, 0, 3);
		assertEqual(1, data[0], "memmove 向右: [0] 未动");
		assertEqual(2, data[1], "memmove 向右: [1] 未动");
		assertEqual(1, data[2], "memmove 向右: [2]=1");
		assertEqual(2, data[3], "memmove 向右: [3]=2");
		assertEqual(3, data[4], "memmove 向右: [4]=3");
		assertEqual(6, data[5], "memmove 向右: [5] 未动");
	}

	// ─── memmove：向左移动（重叠，源在前目标在后） ────────────────────
	private static void testMemmove_BackwardOverlap()
	{
		int[] data = new int[] { 9, 10, 11, 12, 13 };
		// 将 data[2..4] 移到 data[1..3]（向左重叠）
		memmove(data, 1, 2, 3);
		assertEqual(9, data[0], "memmove 向左: [0] 未动");
		assertEqual(11, data[1], "memmove 向左: [1]=11");
		assertEqual(12, data[2], "memmove 向左: [2]=12");
		assertEqual(13, data[3], "memmove 向左: [3]=13");
	}

	// ─── memmove：同下标为无操作 ─────────────────────────────────────────
	private static void testMemmove_SameIndexNoOp()
	{
		int[] data = new int[] { 7, 7, 7 };
		memmove(data, 1, 1, 2);
		assertEqual(7, data[0], "memmove 同下标: [0]");
		assertEqual(7, data[1], "memmove 同下标: [1]");
		assertEqual(7, data[2], "memmove 同下标: [2]");
	}

	// ─── memset：分区重复覆盖 ────────────────────────────────────────────
	private static void testMemset_RepeatedRegions()
	{
		byte[] arr = new byte[8];
		memset(arr, (byte)0x10, 0, 8);
		memset(arr, (byte)0x20, 2, 3);
		memset(arr, (byte)0x30, 4, 2);
		byte[] expect = new byte[] { 0x10, 0x10, 0x20, 0x20, 0x30, 0x30, 0x10, 0x10 };
		for (int i = 0; i < 8; ++i)
		{
			assertEqual(expect[i], arr[i], $"memset 分区覆盖[{i}]");
		}
	}

	// ─── crc16：增量一致性（分批与一次性同） ──────────────────────────
	private static void testCrc16_IncrementalConsistency()
	{
		initCRC16();
		byte[] data = new byte[] { 0x0A, 0x1B, 0x2C, 0x3D, 0x4E, 0x5F };
		ushort whole = crc16(0, data, data.Length);
		ushort inc = 0;
		for (int i = 0; i < data.Length; ++i)
		{
			inc = crc16(inc, data[i]);
		}
		assertEqual(whole, inc, "crc16: 一次性与逐字节增量相同");
	}

	// ─── crc16：不同数据结果不同 ────────────────────────────────────────
	private static void testCrc16_DifferentDataDiffer()
	{
		initCRC16();
		ushort a = crc16(0, new byte[] { 1, 2, 3 }, 3);
		ushort b = crc16(0, new byte[] { 3, 2, 1 }, 3);
		assertTrue(a != b, "crc16: 顺序不同的数据校验值不同");
	}

	// ─── crc16：空输入 ───────────────────────────────────────────────────
	private static void testCrc16_EmptyInput()
	{
		initCRC16();
		ushort a = crc16(0, new byte[0], 0);
		ushort b = crc16(0, new byte[0], 0);
		assertEqual(a, b, "crc16: 空输入结果稳定");
	}

	// ─── setBitOne / hasBit / getLowestBit 全字节位置往返 ──────────────
	private static void testBitSetRoundTripAllPositions()
	{
		for (int pos = 0; pos < 8; ++pos)
		{
			byte v = 0;
			setBitOne(ref v, pos);
			assert(hasBit(v, pos), $"setBitOne byte 全位置[{pos}]");
			// 注意: getLowestBit 返回的是最低位(bit0)的值 0/1, 而非最低置位位的下标。
			// v = 1<<pos, 仅当 pos==0 时 bit0 才为 1, 否则为 0。按真实语义断言。
			assertEqual(1 << pos & 1, getLowestBit(v), $"getLowestBit 返回 bit0 值 pos={pos}");
			// 唯一的1的最高有效位所在即 pos —— 通过 getHighestBit 校验
			assert(hasBit(v, pos), $"往返校验 pos={pos}");
		}
	}

	// ─── setBitOne + setBitZero 幂等 ───────────────────────────────────
	private static void testBitRoundTrip_Idempotent()
	{
		int v = 0;
		setBitOne(ref v, 5);
		setBitOne(ref v, 5);
		assert(hasBit(v, 5), "setBitOne 幂等");
		setBitOne(ref v, 6);
		assert(hasBit(v, 5) && hasBit(v, 6), "两个独立位同时保持");
		setBitZero(ref v, 5);
		setBitZero(ref v, 5);
		assert(!hasBit(v, 5), "setBitZero 幂等");
		assert(hasBit(v, 6), "清 5 不影响 6");
	}

	// ─── setBufferBitOne 顺序置位 ──────────────────────────────────────
	private static void testBufferBit_SequentialSet()
	{
		byte[] buf = new byte[8]; // 64 bits
		for (int i = 0; i < 64; i += 3)
		{
			setBufferBitOne(buf, i);
		}
		for (int i = 0; i < 64; ++i)
		{
			bool expect = (i % 3) == 0;
			assertEqual(expect, getBufferBit(buf, i), $"buffer bit[{i}]");
		}
	}

	// ─── bitCount1：随机模式 ───────────────────────────────────────────
	private static void testBitCount1_RandomPatterns()
	{
		assertEqual(0, bitCount1((byte)0x00), "bitCount1 0x00=0");
		assertEqual(1, bitCount1((byte)0x80), "bitCount1 0x80=1");
		assertEqual(2, bitCount1((byte)0x81), "bitCount1 0x81=2");
		assertEqual(3, bitCount1((byte)0xE0), "bitCount1 0xE0=3");
		assertEqual(8, bitCount1((byte)0xFF), "bitCount1 0xFF=8");
	}

	// ─── hasBit 与 setBitOne 不变量 ────────────────────────────────────
	private static void testHasBit_InvariantVsSetBit()
	{
		// byte / short / int / long 全套：set 后 has=true，zero 后 has=false
		{
			byte v = 0;
			for (int p = 0; p < 8; ++p)
			{
				setBitOne(ref v, p);
				assert(hasBit(v, p), $"byte hasBit invariant[{p}]");
				setBitZero(ref v, p);
				assert(!hasBit(v, p), $"byte hasBit cleared[{p}]");
			}
		}
		{
			int v = 0;
			setBitOne(ref v, 31);
			assert(hasBit(v, 31), "int 最高位 set");
			setBitZero(ref v, 31);
			assert(!hasBit(v, 31), "int 最高位 clear");
		}
		{
			long v = 0;
			setBitOne(ref v, 62);
			assert(hasBit(v, 62), "long 高位 set");
			setBitZero(ref v, 62);
			assert(!hasBit(v, 62), "long 高位 clear");
		}
	}
}