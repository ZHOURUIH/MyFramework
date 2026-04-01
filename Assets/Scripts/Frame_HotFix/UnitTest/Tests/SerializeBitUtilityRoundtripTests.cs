#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static SerializeBitUtility;
using static TestAssert;

// SerializeBitUtility writeBit / readBit 往返（不经过 NetPacket）
public static class SerializeBitUtilityRoundtripTests
{
	public static void Run()
	{
		testIntSignedRoundtrip();
		testLongSignedRoundtrip();
		testShortByteUshortBoolRoundtrip();
	}

	private static void testIntSignedRoundtrip()
	{
		byte[] buf = new byte[262144];
		int bufLen = buf.Length;
		for (int v = -80000; v <= 80000; v += 173)
		{
			// writeBit 只会把需要为 1 的 bit 写进去，不会清零为 0 的 bit；
			// 为保证 length/value 的 0 位不会被上一次残留的 1 干扰，需要每轮清空缓冲区。
			System.Array.Clear(buf, 0, bufLen);

			int bit = 0;
			assert(writeBit(buf, bufLen, ref bit, v, true), "wb i");
			int used = bitCountToByteCount(bit);
			int br = 0;
			// bufferSize 传入完整容量，避免容量估算误差影响读入
			assert(readBit(buf, bufLen, ref br, out int rv, true), "rb i");
			assertEqual(v, rv, "i " + v);
			assertEqual(bit, br, "bit len i " + v);
		}
	}

	private static void testLongSignedRoundtrip()
	{
		byte[] buf = new byte[262144];
		int bufLen = buf.Length;
		long step = 900719925474099L;
		for (long v = long.MinValue / 4; v <= long.MaxValue / 4; v += step)
		{
			System.Array.Clear(buf, 0, bufLen);

			int bit = 0;
			assert(writeBit(buf, bufLen, ref bit, v, true), "wb L");
			int used = bitCountToByteCount(bit);
			int br = 0;
			assert(readBit(buf, bufLen, ref br, out long rv, true), "rb L");
			assertEqual(v, rv, "L " + v);
		}
	}

	private static void testShortByteUshortBoolRoundtrip()
	{
		byte[] buf = new byte[65536];
		int bufLen = buf.Length;
		for (int iter = -30000; iter <= 30000; iter += 997)
		{
			System.Array.Clear(buf, 0, bufLen);

			short s = (short)iter;
			int bit = 0;
			assert(writeBit(buf, bufLen, ref bit, s, true), "wb s");
			int used = bitCountToByteCount(bit);
			int br = 0;
			assert(readBit(buf, bufLen, ref br, out short rs, true), "rb s");
			assertEqual(s, rs, "s " + s);
		}
		for (int iter = 0; iter <= 500; iter++)
		{
			System.Array.Clear(buf, 0, bufLen);

			byte y = (byte)(iter % 256);
			int bit = 0;
			assert(writeBit(buf, bufLen, ref bit, y), "wb b");
			int used = bitCountToByteCount(bit);
			int br = 0;
			assert(readBit(buf, bufLen, ref br, out byte rb), "rb b");
			assertEqual(y, rb, "b " + y);
		}
		for (int iter = 0; iter < 80000; iter += 131)
		{
			System.Array.Clear(buf, 0, bufLen);

			ushort u = (ushort)iter;
			int bit = 0;
			assert(writeBit(buf, bufLen, ref bit, u), "wb us");
			int used = bitCountToByteCount(bit);
			int br = 0;
			assert(readBit(buf, bufLen, ref br, out ushort ru), "rb us");
			assertEqual(u, ru, "us " + u);
		}
		int bitB = 0;
		System.Array.Clear(buf, 0, bufLen);
		assert(writeBit(buf, bufLen, ref bitB, true), "wb bool t");
		assert(writeBit(buf, bufLen, ref bitB, false), "wb bool f");
		int uB = bitCountToByteCount(bitB);
		int rbB = 0;
		assert(readBit(buf, bufLen, ref rbB, out bool bt), "rb bt");
		assert(readBit(buf, bufLen, ref rbB, out bool bf), "rb bf");
		assert(bt, "bool t");
		assert(!bf, "bool f");
	}
}
#endif
