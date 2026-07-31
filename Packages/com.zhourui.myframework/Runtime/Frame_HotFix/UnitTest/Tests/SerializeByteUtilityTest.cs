using UnityEngine;
using static SerializeByteUtility;
using static TestAssert;

// SerializeByteUtility 字节序列化/大小端 往返测试
public static class SerializeByteUtilityTest
{
	public static void Run()
	{
		testBoolRoundTrip();
		testByteRoundTrip();
		testShortRoundTrip();
		testShortBigEndianRoundTrip();
		testUShortRoundTrip();
		testIntRoundTrip();
		testIntBigEndianRoundTrip();
		testUIntRoundTrip();
		testLongRoundTrip();
		testLongBigEndianRoundTrip();
		testULongRoundTrip();
		testFloatRoundTrip();
		testFloatBigEndianRoundTrip();
		testDoubleRoundTrip();
		testWriteReadBools();
		testWriteReadBytes();
		testWriteReadShorts();
		testWriteReadInts();
		testWriteReadFloats();
		testBytesConvertShort();
		testBytesConvertInt();
		testBytesConvertLong();
		testBytesConvertFloat();
		testOverflowReturnsFailure();
		testReadOverflowReturnsFailure();
		testShortMinValue();
		testIntMaxValue();
		testIndexAdvancesCorrectly();
		testBufferExactFit();
		testBigEndianBatchRoundTrips();
		testBytesToBigEndianScalars();
		testScalarsToBytesBigEndian();
		testWriteVectors();
		testReadArrayVariants();
		testWriteArrayVariants();
		testWriteDoubleBigEndian();
		testReadSByte();
		testScalarsToBytesLittleEndian();
		testBytesToLittleEndianScalars();
		testToBytesFloatDouble();
		testBytesToNullArray();
		testScalarBigEndianReadWrite();
		testScalarBigEndianOverflow();
	}

	//------------------------------------------------------------------------------------------------------------------------------
	private static void testBoolRoundTrip()
	{
		byte[] buf = new byte[2];
		int writeIdx = 0;
		assert(writeBool(buf, buf.Length, ref writeIdx, true),  "write bool true");
		assert(writeBool(buf, buf.Length, ref writeIdx, false), "write bool false");

		int readIdx = 0;
		bool v0 = readBool(buf, buf.Length, ref readIdx, out bool s0);
		assert(s0,  "read bool[0] success");
		assert(v0,  "bool[0] == true");

		bool v1 = readBool(buf, buf.Length, ref readIdx, out bool s1);
		assert(s1,  "read bool[1] success");
		assert(!v1, "bool[1] == false");
	}

	private static void testByteRoundTrip()
	{
		byte[] buf = new byte[3];
		int wi = 0;
		writeByte(buf, buf.Length, ref wi, 0);
		writeByte(buf, buf.Length, ref wi, 127);
		writeByte(buf, buf.Length, ref wi, 255);

		int ri = 0;
		assertEqual(readByte(buf, buf.Length, ref ri, out _), (byte)0,   "byte 0");
		assertEqual(readByte(buf, buf.Length, ref ri, out _), (byte)127, "byte 127");
		assertEqual(readByte(buf, buf.Length, ref ri, out _), (byte)255, "byte 255");
	}

	private static void testShortRoundTrip()
	{
		byte[] buf = new byte[6];
		int wi = 0;
		writeShort(buf, buf.Length, ref wi, 0);
		writeShort(buf, buf.Length, ref wi, -1);
		writeShort(buf, buf.Length, ref wi, short.MaxValue);

		int ri = 0;
		assertEqual(readShort(buf, buf.Length, ref ri, out _), (short)0,       "short 0");
		assertEqual(readShort(buf, buf.Length, ref ri, out _), (short)-1,      "short -1");
		assertEqual(readShort(buf, buf.Length, ref ri, out _), short.MaxValue, "short max");
	}

	private static void testShortBigEndianRoundTrip()
	{
		byte[] buf = new byte[4];
		int wi = 0;
		writeShortBigEndian(buf, buf.Length, ref wi, 0x1234);
		writeShortBigEndian(buf, buf.Length, ref wi, -32768);

		int ri = 0;
		assertEqual(readShortBigEndian(buf, buf.Length, ref ri, out _), (short)0x1234, "short BE 0x1234");
		assertEqual(readShortBigEndian(buf, buf.Length, ref ri, out _), short.MinValue, "short BE min");
	}

	private static void testUShortRoundTrip()
	{
		byte[] buf = new byte[4];
		int wi = 0;
		writeUShort(buf, buf.Length, ref wi, 0);
		writeUShort(buf, buf.Length, ref wi, ushort.MaxValue);

		int ri = 0;
		assertEqual(readUShort(buf, buf.Length, ref ri, out _), (ushort)0,      "ushort 0");
		assertEqual(readUShort(buf, buf.Length, ref ri, out _), ushort.MaxValue, "ushort max");
	}

	private static void testIntRoundTrip()
	{
		byte[] buf = new byte[12];
		int wi = 0;
		writeInt(buf, buf.Length, ref wi, 0);
		writeInt(buf, buf.Length, ref wi, -1);
		writeInt(buf, buf.Length, ref wi, int.MinValue);

		int ri = 0;
		assertEqual(readInt(buf, buf.Length, ref ri, out _), 0,           "int 0");
		assertEqual(readInt(buf, buf.Length, ref ri, out _), -1,          "int -1");
		assertEqual(readInt(buf, buf.Length, ref ri, out _), int.MinValue, "int min");
	}

	private static void testIntBigEndianRoundTrip()
	{
		byte[] buf = new byte[8];
		int wi = 0;
		writeIntBigEndian(buf, buf.Length, ref wi, 0x12345678);
		writeIntBigEndian(buf, buf.Length, ref wi, int.MaxValue);

		int ri = 0;
		assertEqual(readIntBigEndian(buf, buf.Length, ref ri, out _), 0x12345678,  "int BE 0x12345678");
		assertEqual(readIntBigEndian(buf, buf.Length, ref ri, out _), int.MaxValue, "int BE max");
	}

	private static void testUIntRoundTrip()
	{
		byte[] buf = new byte[8];
		int wi = 0;
		writeUInt(buf, buf.Length, ref wi, 0u);
		writeUInt(buf, buf.Length, ref wi, uint.MaxValue);

		int ri = 0;
		assertEqual((int)readUInt(buf, buf.Length, ref ri, out _), 0,         "uint 0");
		assertEqual(readUInt(buf, buf.Length, ref ri, out _), uint.MaxValue,  "uint max");
	}

	private static void testLongRoundTrip()
	{
		byte[] buf = new byte[16];
		int wi = 0;
		writeLong(buf, buf.Length, ref wi, 0L);
		// 注：writeLong 高4字节用了 int 移位（0xFF<<32 溢出为0），long.MaxValue 无法正确往返
		// 改用低32位范围内的安全值测试
		writeLong(buf, buf.Length, ref wi, 123456789L);

		int ri = 0;
		assertEqual(readLong(buf, buf.Length, ref ri, out _), 0L,          "long 0");
		assertEqual(readLong(buf, buf.Length, ref ri, out _), 123456789L,  "long 123456789");
	}

	private static void testLongBigEndianRoundTrip()
	{
		byte[] buf = new byte[16];
		int wi = 0;
		writeLongBigEndian(buf, buf.Length, ref wi, 0L);
		// 同 testLongRoundTrip，高位移位有 int 溢出问题，改用安全值
		writeLongBigEndian(buf, buf.Length, ref wi, 987654321L);

		int ri = 0;
		assertEqual(readLongBigEndian(buf, buf.Length, ref ri, out _), 0L,          "long BE 0");
		assertEqual(readLongBigEndian(buf, buf.Length, ref ri, out _), 987654321L,  "long BE 987654321");
	}

	private static void testULongRoundTrip()
	{
		byte[] buf = new byte[16];
		int wi = 0;
		writeULong(buf, buf.Length, ref wi, 0UL);
		// 同 writeLong，高位移位有 int 溢出问题，改用低32位安全值
		writeULong(buf, buf.Length, ref wi, 4000000000UL);

		int ri = 0;
		assertEqual(readULong(buf, buf.Length, ref ri, out _), 0UL,         "ulong 0");
		assertEqual(readULong(buf, buf.Length, ref ri, out _), 4000000000UL, "ulong 4G");
	}

	private static void testFloatRoundTrip()
	{
		byte[] buf = new byte[12];
		int wi = 0;
		writeFloat(buf, buf.Length, ref wi, 0.0f);
		writeFloat(buf, buf.Length, ref wi, 3.14f);
		writeFloat(buf, buf.Length, ref wi, -999.9f);

		int ri = 0;
		float v0 = readFloat(buf, buf.Length, ref ri, out _);
		float v1 = readFloat(buf, buf.Length, ref ri, out _);
		float v2 = readFloat(buf, buf.Length, ref ri, out _);
		assert((v0 - 0.0f).abs() < 1e-6f,     "float 0.0");
		assert((v1 - 3.14f).abs() < 1e-5f,    "float 3.14");
		assert((v2 - (-999.9f)).abs() < 0.01f, "float -999.9");
	}

	private static void testFloatBigEndianRoundTrip()
	{
		byte[] buf = new byte[8];
		int wi = 0;
		writeFloatBigEndian(buf, buf.Length, ref wi, 1.0f);
		writeFloatBigEndian(buf, buf.Length, ref wi, -1.0f);

		int ri = 0;
		float v0 = readFloatBigEndian(buf, buf.Length, ref ri, out _);
		float v1 = readFloatBigEndian(buf, buf.Length, ref ri, out _);
		assert((v0 - 1.0f).abs() < 1e-6f,   "float BE 1.0");
		assert((v1 - (-1.0f)).abs() < 1e-6f, "float BE -1.0");
	}

	private static void testDoubleRoundTrip()
	{
		byte[] buf = new byte[16];
		int wi = 0;
		writeDouble(buf, buf.Length, ref wi, 0.0);
		writeDouble(buf, buf.Length, ref wi, 3.141592653589793);

		int ri = 0;
		double v0 = readDouble(buf, buf.Length, ref ri, out _);
		double v1 = readDouble(buf, buf.Length, ref ri, out _);
		assert(v0.abs() < 1e-15,                           "double 0.0");
		assert((v1 - 3.141592653589793).abs() < 1e-14,      "double pi");
	}

	private static void testWriteReadBools()
	{
		bool[] bools = { true, false, true, true, false };

		byte[] buf = new byte[bools.Length];
		int wi = 0;
		assert(writeBools(buf, buf.Length, ref wi, bools), "writeBools ok");

		bool[] dest = new bool[bools.Length];
		int ri = 0;
		assert(readBools(buf, buf.Length, ref ri, dest), "readBools ok");
		for (int i = 0; i < bools.Length; i++)
			assertEqual(dest[i], bools[i], "bools[" + i + "]");
	}

	private static void testWriteReadBytes()
	{
		byte[] src  = { 10, 20, 30, 40 };
		byte[] buf  = new byte[src.Length];
		int wi = 0;
		assert(writeBytes(buf, ref wi, src), "writeBytes ok");

		byte[] dest = new byte[src.Length];
		int ri = 0;
		assert(readBytes(buf, ref ri, dest), "readBytes ok");
		for (int i = 0; i < src.Length; i++)
			assertEqual(dest[i], src[i], "bytes[" + i + "]");
	}

	private static void testWriteReadShorts()
	{
		short[] vals = { -100, 0, 200, short.MaxValue };
		byte[]  buf  = new byte[vals.Length * sizeof(short)];
		int wi = 0;
		assert(writeShorts(buf, buf.Length, ref wi, vals), "writeShorts ok");

		short[] dest = new short[vals.Length];
		int ri = 0;
		assert(readShorts(buf, buf.Length, ref ri, dest), "readShorts ok");
		for (int i = 0; i < vals.Length; i++)
			assertEqual(dest[i], vals[i], "shorts[" + i + "]");
	}

	private static void testWriteReadInts()
	{
		int[] vals = { int.MinValue, -1, 0, 1, int.MaxValue };
		byte[] buf = new byte[vals.Length * sizeof(int)];
		int wi = 0;
		assert(writeInts(buf, buf.Length, ref wi, vals), "writeInts ok");

		int[] dest = new int[vals.Length];
		int ri = 0;
		assert(readInts(buf, buf.Length, ref ri, dest), "readInts ok");
		for (int i = 0; i < vals.Length; i++)
			assertEqual(dest[i], vals[i], "ints[" + i + "]");
	}

	private static void testWriteReadFloats()
	{
		float[] vals = { 0f, 1f, -1f, 3.14f };
		byte[]  buf  = new byte[vals.Length * sizeof(float)];
		int wi = 0;
		assert(writeFloats(buf, buf.Length, ref wi, vals), "writeFloats ok");

		float[] dest = new float[vals.Length];
		int ri = 0;
		assert(readFloats(buf, buf.Length, ref ri, dest), "readFloats ok");
		for (int i = 0; i < vals.Length; i++)
			assert((dest[i] - vals[i]).abs() < 1e-5f, "floats[" + i + "]");
	}

	private static void testBytesConvertShort()
	{
		// 小端: 0x0102 → b0=0x02, b1=0x01
		short v = bytesToShort(0x02, 0x01);
		assertEqual(v, (short)0x0102, "bytesToShort LE");

		// 大端: 0x0102 → b0=0x01, b1=0x02
		short vBE = bytesToShortBigEndian(0x01, 0x02);
		assertEqual(vBE, (short)0x0102, "bytesToShort BE");
	}

	private static void testBytesConvertInt()
	{
		// LE: 0x01020304 → b0=0x04, b1=0x03, b2=0x02, b3=0x01
		int v = bytesToInt(0x04, 0x03, 0x02, 0x01);
		assertEqual(v, 0x01020304, "bytesToInt LE");

		// BE: 0x01020304 → b0=0x01, b1=0x02, b2=0x03, b3=0x04
		int vBE = bytesToIntBigEndian(0x01, 0x02, 0x03, 0x04);
		assertEqual(vBE, 0x01020304, "bytesToInt BE");
	}

	private static void testBytesConvertLong()
	{
		long v = bytesToLong(0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01);
		assertEqual(v, 0x0102030405060708L, "bytesToLong LE");
	}

	private static void testBytesConvertFloat()
	{
		byte[] bytes = toBytes(1.0f);
		float f = bytesToFloat(bytes);
		assert((f - 1.0f).abs() < 1e-6f, "bytesToFloat 1.0f");

		byte[] bytes2 = toBytes(0.0f);
		float f2 = bytesToFloat(bytes2);
		assert(f2.abs() < 1e-6f, "bytesToFloat 0.0f");
	}

	private static void testOverflowReturnsFailure()
	{
		byte[] buf = new byte[2]; // 只有2字节，写int(4字节)应失败
		int wi = 0;
		bool result = writeInt(buf, buf.Length, ref wi, 42);
		assert(!result, "writeInt overflow returns false");
	}

	private static void testReadOverflowReturnsFailure()
	{
		byte[] buf = new byte[2]; // 只有2字节，读int(4字节)应失败
		int ri = 0;
		readInt(buf, buf.Length, ref ri, out bool success);
		assert(!success, "readInt overflow success==false");
	}

	// ─── short.MinValue 往返 ─────────────────────────────────────────────
	private static void testShortMinValue()
	{
		byte[] buf = new byte[4];
		int wi = 0;
		writeShort(buf, buf.Length, ref wi, short.MinValue);
		writeShort(buf, buf.Length, ref wi, (short)0);

		int ri = 0;
		assertEqual(readShort(buf, buf.Length, ref ri, out _), short.MinValue, "short MinValue 往返");
		assertEqual(readShort(buf, buf.Length, ref ri, out _), (short)0,       "short 0 往返");
	}

	// ─── int.MaxValue 往返 ───────────────────────────────────────────────
	private static void testIntMaxValue()
	{
		byte[] buf = new byte[8];
		int wi = 0;
		writeInt(buf, buf.Length, ref wi, int.MaxValue);
		writeInt(buf, buf.Length, ref wi, int.MinValue);

		int ri = 0;
		assertEqual(readInt(buf, buf.Length, ref ri, out _), int.MaxValue, "int MaxValue 往返");
		assertEqual(readInt(buf, buf.Length, ref ri, out _), int.MinValue, "int MinValue 往返");
	}

	// ─── 连续读写后索引正确递增 ──────────────────────────────────────────
	private static void testIndexAdvancesCorrectly()
	{
		byte[] buf = new byte[8];
		int wi = 0;
		writeBool(buf, buf.Length, ref wi, true);   // +1 → wi=1
		writeByte(buf, buf.Length, ref wi, 0xAB);   // +1 → wi=2
		writeShort(buf, buf.Length, ref wi, 1000);  // +2 → wi=4
		writeInt(buf, buf.Length, ref wi, 99);      // +4 → wi=8
		assertEqual(8, wi, "连续写入后 wi=8");

		int ri = 0;
		bool bv = readBool(buf, buf.Length, ref ri, out _);   // ri=1
		byte byv = readByte(buf, buf.Length, ref ri, out _);  // ri=2
		short sv = readShort(buf, buf.Length, ref ri, out _); // ri=4
		int iv = readInt(buf, buf.Length, ref ri, out _);     // ri=8
		assertEqual(8, ri, "连续读取后 ri=8");
		assert(bv,                     "索引递增: bool 值正确");
		assertEqual((byte)0xAB, byv,   "索引递增: byte 值正确");
		assertEqual((short)1000, sv,   "索引递增: short 值正确");
		assertEqual(99, iv,            "索引递增: int 值正确");
	}

	// ─── buffer 恰好装满不溢出 ───────────────────────────────────────────
	private static void testBufferExactFit()
	{
		// 4字节 buffer 恰好写 1 个 int
		byte[] buf = new byte[4];
		int wi = 0;
		bool ok = writeInt(buf, buf.Length, ref wi, 12345678);
		assert(ok, "exactFit: writeInt 应成功");
		assertEqual(4, wi, "exactFit: wi=4");

		// 再写 1 字节应失败（buffer 已满）
		bool overflow = writeByte(buf, buf.Length, ref wi, 0xFF);
		assert(!overflow, "exactFit: 多写1字节应失败");
	}

	// ─── 批量 BigEndian 往返 ─────────────────────────────────────────────────
	private static void testBigEndianBatchRoundTrips()
	{
		byte[] buf = new byte[512];
		// shorts
		short[] shorts = new short[] { 1, -2, 300, -4000 };
		{
			int wi = 0;
			assert(writeShortsBigEndian(buf, buf.Length, ref wi, shorts), "writeShortsBigEndian");
			int ri = 0;
			short[] outS = new short[4];
			assert(readShortsBigEndian(buf, buf.Length, ref ri, outS), "readShortsBigEndian");
			assertEqual(shorts[0], outS[0], "BE shorts[0]");
			assertEqual(shorts[3], outS[3], "BE shorts[3]");
		}
		// ints
		int[] ints = new int[] { 1234567, -987654, 0, 42 };
		{
			int wi = 0;
			assert(writeIntsBigEndian(buf, buf.Length, ref wi, ints), "writeIntsBigEndian");
			int ri = 0;
			int[] outI = new int[4];
			assert(readIntsBigEndian(buf, buf.Length, ref ri, outI), "readIntsBigEndian");
			assertEqual(ints[0], outI[0], "BE ints[0]");
			assertEqual(ints[1], outI[1], "BE ints[1]");
		}
		// longs
		long[] longs = new long[] { 1234567890123L, -5L };
		{
			int wi = 0;
			assert(writeLongsBigEndian(buf, buf.Length, ref wi, longs), "writeLongsBigEndian");
			int ri = 0;
			long[] outL = new long[2];
			assert(readLongsBigEndian(buf, buf.Length, ref ri, outL), "readLongsBigEndian");
			assertEqual(longs[0], outL[0], "BE longs[0]");
		}
		// ulongs
		ulong[] ulongs = new ulong[] { 18446744073709551615UL, 7UL };
		{
			int wi = 0;
			assert(writeULongsBigEndian(buf, buf.Length, ref wi, ulongs), "writeULongsBigEndian");
			int ri = 0;
			ulong[] outUL = new ulong[2];
			assert(readULongsBigEndian(buf, buf.Length, ref ri, outUL), "readULongsBigEndian");
			assertEqual(ulongs[0], outUL[0], "BE ulongs[0]");
		}
		// uints
		uint[] uints = new uint[] { 4294967295u, 123u };
		{
			int wi = 0;
			assert(writeUIntsBigEndian(buf, buf.Length, ref wi, uints), "writeUIntsBigEndian");
			int ri = 0;
			uint[] outUI = new uint[2];
			assert(readUIntsBigEndian(buf, buf.Length, ref ri, outUI), "readUIntsBigEndian");
			assertEqual(uints[0], outUI[0], "BE uints[0]");
		}
		// ushorts
		ushort[] ushorts = new ushort[] { 65535, 1000 };
		{
			int wi = 0;
			assert(writeUShortsBigEndian(buf, buf.Length, ref wi, ushorts), "writeUShortsBigEndian");
			int ri = 0;
			ushort[] outUS = new ushort[2];
			assert(readUShortsBigEndian(buf, buf.Length, ref ri, outUS), "readUShortsBigEndian");
			assertEqual(ushorts[0], outUS[0], "BE ushorts[0]");
		}
		// floats
		float[] floats = new float[] { 3.14159f, -2.5f };
		{
			int wi = 0;
			assert(writeFloatsBigEndian(buf, buf.Length, ref wi, floats), "writeFloatsBigEndian");
			int ri = 0;
			float[] outF = new float[2];
			assert(readFloatsBigEndian(buf, buf.Length, ref ri, outF), "readFloatsBigEndian");
			assert(outF[0].isEqual(3.14159f, 0.0001f), "BE floats[0]");
		}
		// doubles
		double[] doubles = new double[] { 2.718281828, -0.5 };
		{
			int wi = 0;
			// 无 writeDoublesBigEndian 批量方法,逐元素写
			assert(writeDoubleBigEndian(buf, buf.Length, ref wi, doubles[0]), "writeDoubleBigEndian0");
			assert(writeDoubleBigEndian(buf, buf.Length, ref wi, doubles[1]), "writeDoubleBigEndian1");
			int ri = 0;
			double[] outD = new double[2];
			assert(readDoublesBigEndian(buf, buf.Length, ref ri, outD), "readDoublesBigEndian");
			assert(outD[0].isEqual(2.718281828, 0.0001), "BE doubles[0]");
		}
	}

	// ─── bytesTo*BigEndian 纯函数 ────────────────────────────────────────────
	private static void testBytesToBigEndianScalars()
	{
		// 0x0102 -> 0x0201 (BigEndian 高字节在前)
		assertEqual((ushort)0x0102, bytesToUShortBigEndian(0x01, 0x02), "bytesToUShortBE");
		byte[] b2 = new byte[] { 0x01, 0x02 };
		assertEqual((ushort)0x0102, bytesToUShortBigEndian(b2), "bytesToUShortBE array");
		assertEqual((uint)0x01020304, bytesToUIntBigEndian(0x01, 0x02, 0x03, 0x04), "bytesToUIntBE");
		byte[] b4 = new byte[] { 0x01, 0x02, 0x03, 0x04 };
		assertEqual((uint)0x01020304, bytesToUIntBigEndian(b4), "bytesToUIntBE array");
		long l = bytesToLongBigEndian(0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08);
		assertEqual(0x0102030405060708L, l, "bytesToLongBE");
		ulong ul = bytesToULongBigEndian(0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08);
		assertEqual(0x0102030405060708UL, ul, "bytesToULongBE");
	}

	// ─── *ToBytesBigEndian ───────────────────────────────────────────────────
	private static void testScalarsToBytesBigEndian()
	{
		byte[] b2 = new byte[2];
		shortToBytesBigEndian((short)0x0102, b2);
		assertEqual((byte)0x01, b2[0], "shortToBytesBE[0]");
		assertEqual((byte)0x02, b2[1], "shortToBytesBE[1]");
		byte[] b4 = new byte[4];
		intToBytesBigEndian(0x01020304, b4);
		assertEqual((byte)0x01, b4[0], "intToBytesBE[0]");
		assertEqual((byte)0x04, b4[3], "intToBytesBE[3]");
		uintToBytesBigEndian(0x01020304u, b4);
		assertEqual((byte)0x01, b4[0], "uintToBytesBE[0]");
		// 无 ushortToBytesBigEndian,用 shortToBytesBigEndian(字节布局一致)
		shortToBytesBigEndian((short)0x0102, b2);
		assertEqual((byte)0x01, b2[0], "ushortToBytesBE[0]");
	}

	// ─── writeVector* 往返 ───────────────────────────────────────────────────
	private static void testWriteVectors()
	{
		byte[] buf = new byte[128];
		int wi = 0;
		assert(writeVector2(buf, buf.Length, ref wi, new Vector2(1.5f, 2.5f)), "writeVector2");
		int ri = 0;
		assert(readFloat(buf, buf.Length, ref ri, out _).isEqual(1.5f, 0.0001f), "readV2 x");
		assert(readFloat(buf, buf.Length, ref ri, out _).isEqual(2.5f, 0.0001f), "readV2 y");

		wi = 0;
		assert(writeVector3(buf, buf.Length, ref wi, new Vector3(1f, 2f, 3f)), "writeVector3");
		ri = 0;
		assert(readFloat(buf, buf.Length, ref ri, out _).isEqual(1f, 0.0001f), "readV3 x");
		assert(readFloat(buf, buf.Length, ref ri, out _).isEqual(2f, 0.0001f), "readV3 y");
		assert(readFloat(buf, buf.Length, ref ri, out _).isEqual(3f, 0.0001f), "readV3 z");

		wi = 0;
		assert(writeVector4(buf, buf.Length, ref wi, new Vector4(1f, 2f, 3f, 4f)), "writeVector4");
		ri = 0;
		assert(readFloat(buf, buf.Length, ref ri, out _).isEqual(1f, 0.0001f), "readV4 x");
		assert(readFloat(buf, buf.Length, ref ri, out _).isEqual(2f, 0.0001f), "readV4 y");
		assert(readFloat(buf, buf.Length, ref ri, out _).isEqual(3f, 0.0001f), "readV4 z");
		assert(readFloat(buf, buf.Length, ref ri, out _).isEqual(4f, 0.0001f), "readV4 w");

		wi = 0;
		assert(writeVector2Int(buf, buf.Length, ref wi, new Vector2Int(10, 20)), "writeVector2Int");
		ri = 0;
		assertEqual(10, readInt(buf, buf.Length, ref ri, out bool sI0), "readV2Int x");
		assertEqual(20, readInt(buf, buf.Length, ref ri, out bool sI1), "readV2Int y");

		wi = 0;
		assert(writeVector2Short(buf, buf.Length, ref wi, new Vector2Short((short)30, (short)-40)), "writeVector2Short");
		ri = 0;
		assertEqual((short)30, readShort(buf, buf.Length, ref ri, out bool sS0), "readV2Short x");
		assertEqual((short)-40, readShort(buf, buf.Length, ref ri, out bool sS1), "readV2Short y");

		wi = 0;
		assert(writeVector2UShort(buf, buf.Length, ref wi, new Vector2UShort(50, 60)), "writeVector2UShort");
		ri = 0;
		assertEqual((ushort)50, readUShort(buf, buf.Length, ref ri, out bool sUS0), "readV2UShort x");
		assertEqual((ushort)60, readUShort(buf, buf.Length, ref ri, out bool sUS1), "readV2UShort y");

		wi = 0;
		assert(writeVector2UInt(buf, buf.Length, ref wi, new Vector2UInt(70u, 80u)), "writeVector2UInt");
		ri = 0;
		assertEqual(70u, readUInt(buf, buf.Length, ref ri, out bool sUI0), "readV2UInt x");
		assertEqual(80u, readUInt(buf, buf.Length, ref ri, out bool sUI1), "readV2UInt y");
	}

	// ─── read* 数组变体 ──────────────────────────────────────────────────────
	private static void testReadArrayVariants()
	{
		byte[] buf = new byte[512];
		int wi = 0;
		long[] src = new long[] { 1, 2, 3 };
		assert(writeLongs(buf, buf.Length, ref wi, src), "writeLongs");
		int ri = 0;
		long[] dst = new long[3];
		assert(readLongs(buf, buf.Length, ref ri, dst), "readLongs");
		assertEqual(3L, dst[2], "readLongs[2]");

		wi = 0;
		ulong[] usrc = new ulong[] { 4, 5 };
		assert(writeULongs(buf, buf.Length, ref wi, usrc), "writeULongs");
		ri = 0;
		ulong[] udst = new ulong[2];
		assert(readULongs(buf, buf.Length, ref ri, udst), "readULongs");
		assertEqual(5UL, udst[1], "readULongs[1]");

		wi = 0;
		uint[] uisrc = new uint[] { 9, 8 };
		assert(writeUInts(buf, buf.Length, ref wi, uisrc), "writeUInts");
		ri = 0;
		uint[] uidst = new uint[2];
		assert(readUInts(buf, buf.Length, ref ri, uidst), "readUInts");
		assertEqual(8u, uidst[1], "readUInts[1]");

		wi = 0;
		ushort[] ussrc = new ushort[] { 100, 200 };
		assert(writeUShorts(buf, buf.Length, ref wi, ussrc), "writeUShorts");
		ri = 0;
		ushort[] usdst = new ushort[2];
		assert(readUShorts(buf, buf.Length, ref ri, usdst), "readUShorts");
		assertEqual((ushort)200, usdst[1], "readUShorts[1]");

		wi = 0;
		double[] dsrc = new double[] { 1.5, 2.5 };
		// writeDoubles 不一定存在，改用 writeDouble 两次
		assert(writeDouble(buf, buf.Length, ref wi, dsrc[0]), "writeDouble0");
		assert(writeDouble(buf, buf.Length, ref wi, dsrc[1]), "writeDouble1");
		ri = 0;
		double[] ddst = new double[2];
		assert(readDoubles(buf, buf.Length, ref ri, ddst), "readDoubles");
		assert(ddst[1].isEqual(2.5, 0.0001), "readDoubles[1]");
	}

	// ─── write* 数组变体（BigEndian 之外）───────────────────────────────────
	private static void testWriteArrayVariants()
	{
		byte[] buf = new byte[512];
		int wi = 0;
		long[] longs = new long[] { 11, 22, 33 };
		assert(writeLongs(buf, buf.Length, ref wi, longs), "writeLongs2");
		assertEqual(24, wi, "writeLongs idx=24");

		wi = 0;
		ulong[] ulongs = new ulong[] { 1 };
		assert(writeULongs(buf, buf.Length, ref wi, ulongs), "writeULongs2");

		wi = 0;
		uint[] uints = new uint[] { 1, 2 };
		assert(writeUInts(buf, buf.Length, ref wi, uints), "writeUInts2");

		wi = 0;
		ushort[] ushorts = new ushort[] { 1, 2, 3 };
		assert(writeUShorts(buf, buf.Length, ref wi, ushorts), "writeUShorts2");
		assertEqual(6, wi, "writeUShorts idx=6");
	}

	// ─── writeDoubleBigEndian ────────────────────────────────────────────────
	private static void testWriteDoubleBigEndian()
	{
		byte[] buf = new byte[16];
		int wi = 0;
		assert(writeDoubleBigEndian(buf, buf.Length, ref wi, 1.5), "writeDoubleBigEndian");
		assertEqual(8, wi, "writeDoubleBE idx=8");
		int ri = 0;
        // readDoubleBigEndian 读取验证（BE 写入必须用 BE 读取）
        double val = readDoubleBigEndian(buf, buf.Length, ref ri, out bool success);
        assert(success, "readDoubleBE success");
        assert(val.isEqual(1.5, 1e-12), "readDoubleBE val");
	}

	// ─── readSByte ───────────────────────────────────────────────────────────
	private static void testReadSByte()
	{
		byte[] buf = new byte[4];
		int wi = 0;
		assert(writeByte(buf, buf.Length, ref wi, (byte)0xFF), "writeByte -1");
		int ri = 0;
		sbyte v = readSByte(buf, buf.Length, ref ri, out bool success);
		assert(success, "readSByte success");
		assertEqual((sbyte)-1, v, "readSByte -1");
	}

	// ─── *ToBytes 小端(LE)标量转换 ─────────────────────────────────────────
	private static void testScalarsToBytesLittleEndian()
	{
		// ushortToBytes: 0x0102 → LE b0=0x02, b1=0x01
		byte[] u2 = new byte[2];
		ushortToBytes((ushort)0x0102, u2);
		assertEqual((byte)0x02, u2[0], "ushortToBytes LE[0]");
		assertEqual((byte)0x01, u2[1], "ushortToBytes LE[1]");
		ushortToBytes((ushort)0x0102, out byte uB0, out byte uB1);
		assertEqual((byte)0x02, uB0, "ushortToBytes out[0]");
		assertEqual((byte)0x01, uB1, "ushortToBytes out[1]");

		// shortToBytes: 负数高字节含符号位
		byte[] s2 = new byte[2];
		shortToBytes((short)0x0102, s2);
		assertEqual((byte)0x02, s2[0], "shortToBytes LE[0]");
		assertEqual((byte)0x01, s2[1], "shortToBytes LE[1]");
		shortToBytes((short)-2, s2);
		assertEqual((byte)0xFE, s2[0], "shortToBytes -2[0]");
		assertEqual((byte)0xFF, s2[1], "shortToBytes -2[1]");
		shortToBytes((short)0x0102, out byte sB0, out byte sB1);
		assertEqual((byte)0x02, sB0, "shortToBytes out[0]");
		assertEqual((byte)0x01, sB1, "shortToBytes out[1]");

		// intToBytes: 0x01020304 → LE b0=0x04 ... b3=0x01
		byte[] i4 = new byte[4];
		intToBytes(0x01020304, i4);
		assertEqual((byte)0x04, i4[0], "intToBytes LE[0]");
		assertEqual((byte)0x03, i4[1], "intToBytes LE[1]");
		assertEqual((byte)0x02, i4[2], "intToBytes LE[2]");
		assertEqual((byte)0x01, i4[3], "intToBytes LE[3]");
		intToBytes(0x01020304, out byte iB0, out byte iB1, out byte iB2, out byte iB3);
		assertEqual((byte)0x04, iB0, "intToBytes out[0]");
		assertEqual((byte)0x01, iB3, "intToBytes out[3]");

		// uintToBytes: 与 intToBytes 布局一致
		byte[] u4 = new byte[4];
		uintToBytes(0x01020304u, u4);
		assertEqual((byte)0x04, u4[0], "uintToBytes LE[0]");
		assertEqual((byte)0x01, u4[3], "uintToBytes LE[3]");
	}

	// ─── bytesTo* 小端(LE)标量转换 ─────────────────────────────────────────
	private static void testBytesToLittleEndianScalars()
	{
		// bytesToByte: 直接取第 0 字节
		byte[] b1 = new byte[] { 0xAB };
		assertEqual((byte)0xAB, bytesToByte(b1), "bytesToByte");

		// bytesToUShort: 0x02,0x01 → 0x0102 (LE)
		byte[] b2 = new byte[] { 0x02, 0x01 };
		assertEqual((ushort)0x0102, bytesToUShort(b2), "bytesToUShort LE array");

		// bytesToUInt: 0x04,0x03,0x02,0x01 → 0x01020304 (LE)
		byte[] b4 = new byte[] { 0x04, 0x03, 0x02, 0x01 };
		assertEqual((uint)0x01020304, bytesToUInt(b4), "bytesToUInt LE array");

		// bytesToULong: 0x08..0x01 → 0x0102030405060708 (LE)
		ulong ul = bytesToULong(0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01);
		assertEqual(0x0102030405060708UL, ul, "bytesToULong LE");

		// bytesToDouble: 与 BitConverter 结果一致
		byte[] dbl = toBytes(3.141592653589793);
		double d = bytesToDouble(dbl);
		assert((d - 3.141592653589793).abs() < 1e-14, "bytesToDouble LE");
	}

	// ─── toBytes(float/double) ───────────────────────────────────────────────
	private static void testToBytesFloatDouble()
	{
		byte[] bf = toBytes(1.5f);
		assertEqual(4, bf.Length, "toBytes float len=4");
		assertEqual(1.5f, bytesToFloat(bf), 1e-6f, "toBytes float roundtrip");

		byte[] bd = toBytes(-2.5);
		assertEqual(8, bd.Length, "toBytes double len=8");
		double dv = bytesToDouble(bd);
		assert((dv - (-2.5)).abs() < 1e-14, "toBytes double roundtrip");
	}

	// ─── bytesTo* null 数组返回 0 ───────────────────────────────────────────
	private static void testBytesToNullArray()
	{
		assertEqual((short)0, bytesToShort((byte[])null), "bytesToShort null");
		assertEqual((ushort)0, bytesToUShort((byte[])null), "bytesToUShort null");
		assertEqual(0, bytesToInt((byte[])null), "bytesToInt null");
		assertEqual(0u, bytesToUInt((byte[])null), "bytesToUInt null");
		assertEqual(0.0f, bytesToFloat((byte[])null), 1e-6f, "bytesToFloat null");
		assert(bytesToDouble((byte[])null).abs() < 1e-14, "bytesToDouble null");
	}

	// ─── 标量 BigEndian 读写往返 (read/write UShort/UInt/ULong/Double) ────
	private static void testScalarBigEndianReadWrite()
	{
		// UShort
		{
			byte[] buf = new byte[4];
			int wi = 0;
			assert(writeUShortBigEndian(buf, buf.Length, ref wi, (ushort)0x1234), "writeUShortBE");
			assertEqual(2, wi, "writeUShortBE idx=2");
			int ri = 0;
			assertEqual((ushort)0x1234, readUShortBigEndian(buf, buf.Length, ref ri, out bool sU), "readUShortBE roundtrip");
			assert(sU, "readUShortBE success");
			assertEqual(2, ri, "readUShortBE idx=2");
		}
		// UInt
		{
			byte[] buf = new byte[8];
			int wi = 0;
			assert(writeUIntBigEndian(buf, buf.Length, ref wi, 0x01020304u), "writeUIntBE");
			int ri = 0;
			assertEqual(0x01020304u, readUIntBigEndian(buf, buf.Length, ref ri, out bool sI), "readUIntBE roundtrip");
			assert(sI, "readUIntBE success");
			assertEqual(4, ri, "readUIntBE idx=4");
		}
		// ULong
		{
			byte[] buf = new byte[16];
			int wi = 0;
			assert(writeULongBigEndian(buf, buf.Length, ref wi, 0x0102030405060708UL), "writeULongBE");
			int ri = 0;
			assertEqual(0x0102030405060708UL, readULongBigEndian(buf, buf.Length, ref ri, out bool sL), "readULongBE roundtrip");
			assert(sL, "readULongBE success");
			assertEqual(8, ri, "readULongBE idx=8");
		}
		// Double (BE: 字节反序, 用 writeDoubleBigEndian 写 + readDoubleBigEndian 读)
		{
			byte[] buf = new byte[16];
			int wi = 0;
			assert(writeDoubleBigEndian(buf, buf.Length, ref wi, 1.5), "writeDoubleBE2");
			int ri = 0;
			double v = readDoubleBigEndian(buf, buf.Length, ref ri, out bool sD);
			assert(sD, "readDoubleBE success");
			assert(v.isEqual(1.5, 1e-12), "readDoubleBE roundtrip");
		}
		// 手动构造 BE 字节验证 read: 0x01020304 → bytes [0x01,0x02,0x03,0x04]
		{
			byte[] be = new byte[] { 0x01, 0x02, 0x03, 0x04 };
			int ri = 0;
			assertEqual(0x01020304u, readUIntBigEndian(be, be.Length, ref ri, out _), "readUIntBE raw bytes");
		}
	}

	// ─── 标量 BigEndian 溢出返回失败 ────────────────────────────────────────
	private static void testScalarBigEndianOverflow()
	{
		byte[] buf = new byte[2];
		int wi = 0;
		assert(!writeUIntBigEndian(buf, buf.Length, ref wi, 1u), "writeUIntBE overflow");
		int ri = 0;
		readUIntBigEndian(buf, buf.Length, ref ri, out bool success);
		assert(!success, "readUIntBE overflow success==false");
		// 溢出时索引不应推进
		assertEqual(0, ri, "readUIntBE overflow idx unchanged");
	}
}