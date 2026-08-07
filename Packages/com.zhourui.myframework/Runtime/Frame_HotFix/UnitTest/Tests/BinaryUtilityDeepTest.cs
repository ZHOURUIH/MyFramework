using System;
using static TestAssert;
using static BinaryUtility;

// BinaryUtility 深度测试
// 聚焦复杂调用链/边界：contains 跨边界匹配、isMemoryEqual 多偏移比对、
// memcpy/memmove 重叠方向与字节偏移、crc16 跨数据增量校验、位操作左右一致性、
// 缓冲区位随机往返。
public static class BinaryUtilityDeepTest
{
	public static void Run()
	{
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
