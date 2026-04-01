#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
        string result = removeLastZero(s);
        assertEqual("hello", result, "removeLastZero 移除末尾\\0");

        string noZero = "world";
        assertEqual("world", removeLastZero(noZero), "removeLastZero 无\\0不变");
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
}
#endif
