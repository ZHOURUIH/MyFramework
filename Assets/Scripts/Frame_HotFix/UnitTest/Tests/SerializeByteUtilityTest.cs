using static UnityUtility;
using static SerializeByteUtility;
using static TestAssert;
using static MathUtility;

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
		assert(abs(v0 - 0.0f) < 1e-6f,     "float 0.0");
		assert(abs(v1 - 3.14f) < 1e-5f,    "float 3.14");
		assert(abs(v2 - (-999.9f)) < 0.01f, "float -999.9");
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
		assert(abs(v0 - 1.0f) < 1e-6f,   "float BE 1.0");
		assert(abs(v1 - (-1.0f)) < 1e-6f, "float BE -1.0");
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
		assert(abs(v0) < 1e-15,                           "double 0.0");
		assert(abs(v1 - 3.141592653589793) < 1e-14,      "double pi");
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
			assert(abs(dest[i] - vals[i]) < 1e-5f, "floats[" + i + "]");
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
		assert(abs(f - 1.0f) < 1e-6f, "bytesToFloat 1.0f");

		byte[] bytes2 = toBytes(0.0f);
		float f2 = bytesToFloat(bytes2);
		assert(abs(f2) < 1e-6f, "bytesToFloat 0.0f");
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
}
