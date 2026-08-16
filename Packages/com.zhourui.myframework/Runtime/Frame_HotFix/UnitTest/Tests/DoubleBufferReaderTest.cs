using static TestAssert;

// 补充覆盖 DoubleBufferReader 自动清空读列表并结束读取
public static class DoubleBufferReaderTest
{
	public static void Run()
	{
		testDisposeClearsReadListAndAllowsNextRead();
		testEmptyBufferDispose();
		testDisposeTwice();
		testAddAfterReaderCreated();
		testMultipleReadCycles();
		testNoDataReadListEmpty();
		testNewDataAfterDispose();
	}

	private static void testDisposeClearsReadListAndAllowsNextRead()
	{
		DoubleBuffer<int> buffer = new();
		buffer.add(1);
		buffer.add(2);

		DoubleBufferReader<int> reader = new(buffer);
		assertNotNull(reader.mReadList, "应能获取读列表");
		assertEqual(2, reader.mReadList.Count, "读列表数量错误");
		assertEqual(1, reader.mReadList[0]);
		assertEqual(2, reader.mReadList[1]);
		reader.Dispose();
		assertEqual(0, reader.mReadList.Count, "Dispose 应清空读列表");

		buffer.add(3);
		using DoubleBufferReader<int> reader2 = new(buffer);
		assertNotNull(reader2.mReadList, "Dispose 后应允许再次读取");
		assertEqual(1, reader2.mReadList.Count);
		assertEqual(3, reader2.mReadList[0]);
	}

	// ═════════════════════════════════════════════════════════════════
	// 深度组合
	// ═════════════════════════════════════════════════════════════════

	// 空 buffer → 空读列表 + dispose 安全
	private static void testEmptyBufferDispose()
	{
		DoubleBuffer<int> buffer = new();
		using DoubleBufferReader<int> reader = new(buffer);
		assertNotNull(reader.mReadList, "空 buffer 读列表非空引用");
		assertEqual(0, reader.mReadList.Count, "空 buffer 读列表空");
	}

	// Dispose 两次安全(endGet 幂等)
	private static void testDisposeTwice()
	{
		DoubleBuffer<int> buffer = new();
		buffer.add(7);
		DoubleBufferReader<int> reader = new(buffer);
		reader.Dispose();
		reader.Dispose();
		// 无异常即通过
	}

	// reader 创建后 add → 读列表不变(双缓冲: add 写另一侧)
	private static void testAddAfterReaderCreated()
	{
		DoubleBuffer<int> buffer = new();
		buffer.add(1);
		DoubleBufferReader<int> reader = new(buffer);
		assertEqual(1, reader.mReadList.Count, "创建时读 1 个");
		buffer.add(2);
		assertEqual(1, reader.mReadList.Count, "add 不影响已获取的读列表");
		reader.Dispose();
	}

	// 多轮 读→dispose→add→读
	private static void testMultipleReadCycles()
	{
		DoubleBuffer<int> buffer = new();
		// get() 内部 swap 读写索引, 每轮读到的是当轮新增的 1 个(非累计)
		for (int round = 1; round <= 3; ++round)
		{
			buffer.add(round);
			using DoubleBufferReader<int> reader = new(buffer);
			assertEqual(1, reader.mReadList.Count, "第 " + round + " 轮读到 1 个");
			assertEqual(round, reader.mReadList[0], "内容正确");
		}
	}

	// 未 add 任何数据 → 读列表空
	private static void testNoDataReadListEmpty()
	{
		DoubleBuffer<string> buffer = new();
		DoubleBufferReader<string> reader = new(buffer);
		assertEqual(0, reader.mReadList.Count, "无数据读列表空");
		reader.Dispose();
	}

	// dispose 后新数据可被下一 reader 读到
	private static void testNewDataAfterDispose()
	{
		DoubleBuffer<int> buffer = new();
		buffer.add(1);
		using (DoubleBufferReader<int> r1 = new(buffer))
		{
			assertEqual(1, r1.mReadList.Count, "第一轮读 1 个");
		}
		buffer.add(2);
		using DoubleBufferReader<int> r2 = new(buffer);
		assertEqual(1, r2.mReadList.Count, "第二轮只读新数据 1 个");
		assertEqual(2, r2.mReadList[0], "第二轮内容是新数据");
	}
}