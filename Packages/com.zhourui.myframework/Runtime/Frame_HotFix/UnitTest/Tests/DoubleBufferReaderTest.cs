using static TestAssert;

// 补充覆盖 DoubleBufferReader 自动清空读列表并结束读取
public static class DoubleBufferReaderTest
{
	public static void Run()
	{
		testDisposeClearsReadListAndAllowsNextRead();
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
}