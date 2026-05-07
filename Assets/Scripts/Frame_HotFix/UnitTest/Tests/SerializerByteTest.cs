#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static TestAssert;

// 测试用 Serializable 实现
public class SerializerByteTestDummy : Serializable
{
	public int mX;
	public int mY;
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mX) && reader.read(out mY);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mX);
		writer.write(mY);
	}
}

// 测试枚举
public enum SerializerByteTestEnum : byte
{
	Zero = 0,
	One = 1,
	Two = 2,
}

// SerializerWrite + SerializerRead 全面的往返覆盖测试
// 注：底层 SerializeByteUtility 已有单独测试，此处验证 SerializerWrite/Read 包装层正确性
public static class SerializerByteTest
{
	public static void Run()
	{
		// ─── 基础标量类型 ───
		testWriteReadBool();
		testWriteReadByte();
		testWriteReadSByte();
		testWriteReadShort();
		testWriteReadUShort();
		testWriteReadInt();
		testWriteReadUInt();
		testWriteReadLong();
		testWriteReadULong();
		testWriteReadFloat();
		testWriteReadDouble();

		// ─── BigEndian ───
		testWriteReadShortBigEndian();
		testWriteReadUShortBigEndian();
		testWriteReadIntBigEndian();
		testWriteReadUIntBigEndian();
		testWriteReadLongBigEndian();
		testWriteReadULongBigEndian();
		testWriteReadFloatBigEndian();
		testWriteReadDoubleBigEndian();

		// ─── Vector 类型 ───
		testWriteReadVector2();
		testWriteReadVector2Int();
		testWriteReadVector2UInt();
		testWriteReadVector2Short();
		testWriteReadVector2UShort();
		testWriteReadVector3();
		testWriteReadVector4();

		// ─── 缓冲操作 ───
		testWriteReadBuffer();

		// ─── 字符串 ───
		testWriteReadString();
		testWriteReadStringEmpty();
		testWriteReadStringNull();
		testWriteReadStringEncoding();

		// ─── 自定义序列化 ───
		testWriteReadCustom();

		// ─── 列表 ───
		testWriteReadListByte();
		testWriteReadListSByte();
		testWriteReadListShort();
		testWriteReadListUShort();
		testWriteReadListInt();
		testWriteReadListUInt();
		testWriteReadListLong();
		testWriteReadListULong();
		testWriteReadListFloat();
		testWriteReadListDouble();
		testWriteReadListString();

		// ─── 自定义列表 ───
		testWriteReadCustomList();

		// ─── 枚举 ───
		testReadEnumByte();
		testReadEnumInt();
		testReadEnumList();

		// ─── 工具方法 ───
		testSkipIndex();
		testSetIndex();
		testClear();
		testGetBuffer();
		testGetDataSize();
		testGetIndex();
		testSetNeedCheck();

		// ─── Init 变体 ───
		testInitWithOffset();

		// ─── 边界条件 ───
		testReadOverflow();
		testNeedCheckDisabled();

		// ─── 多点连续读写 ───
		testMultipleTypesSequence();
	}

	// ===================== 基础标量类型 =====================

	private static void testWriteReadBool()
	{
		var w = new SerializerWrite();
		w.write(true);
		w.write(false);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out bool v0), "bool[0] read success");
		assert(v0, "bool[0] == true");
		assert(r.read(out bool v1), "bool[1] read success");
		assert(!v1, "bool[1] == false");
	}

	private static void testWriteReadByte()
	{
		var w = new SerializerWrite();
		w.write((byte)0);
		w.write((byte)255);
		w.write((byte)128);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out byte v0), "byte[0] read success");
		assertEqual((byte)0, v0, "byte 0");
		assert(r.read(out byte v1), "byte[1] read success");
		assertEqual((byte)255, v1, "byte 255");
		assert(r.read(out byte v2), "byte[2] read success");
		assertEqual((byte)128, v2, "byte 128");
	}

	private static void testWriteReadSByte()
	{
		var w = new SerializerWrite();
		w.write((sbyte)0);
		w.write((sbyte)-127);
		w.write((sbyte)127);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out sbyte v0), "sbyte[0] read success");
		assertEqual((sbyte)0, v0, "sbyte 0");
		assert(r.read(out sbyte v1), "sbyte[1] read success");
		assertEqual((sbyte)-127, v1, "sbyte -127");
		assert(r.read(out sbyte v2), "sbyte[2] read success");
		assertEqual((sbyte)127, v2, "sbyte 127");
	}

	private static void testWriteReadShort()
	{
		var w = new SerializerWrite();
		w.write((short)0);
		w.write(short.MinValue);
		w.write(short.MaxValue);
		w.write((short)-1);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out short v0), "short[0] read success");
		assertEqual((short)0, v0, "short 0");
		assert(r.read(out short v1), "short[1] read success");
		assertEqual(short.MinValue, v1, "short min");
		assert(r.read(out short v2), "short[2] read success");
		assertEqual(short.MaxValue, v2, "short max");
		assert(r.read(out short v3), "short[3] read success");
		assertEqual((short)-1, v3, "short -1");
	}

	private static void testWriteReadUShort()
	{
		var w = new SerializerWrite();
		w.write((ushort)0);
		w.write(ushort.MaxValue);
		w.write((ushort)12345);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out ushort v0), "ushort[0] read success");
		assertEqual((ushort)0, v0, "ushort 0");
		assert(r.read(out ushort v1), "ushort[1] read success");
		assertEqual(ushort.MaxValue, v1, "ushort max");
		assert(r.read(out ushort v2), "ushort[2] read success");
		assertEqual((ushort)12345, v2, "ushort 12345");
	}

	private static void testWriteReadInt()
	{
		var w = new SerializerWrite();
		w.write(0);
		w.write(int.MinValue);
		w.write(int.MaxValue);
		w.write(-1);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out int v0), "int[0] read success");
		assertEqual(0, v0, "int 0");
		assert(r.read(out int v1), "int[1] read success");
		assertEqual(int.MinValue, v1, "int min");
		assert(r.read(out int v2), "int[2] read success");
		assertEqual(int.MaxValue, v2, "int max");
		assert(r.read(out int v3), "int[3] read success");
		assertEqual(-1, v3, "int -1");
	}

	private static void testWriteReadUInt()
	{
		var w = new SerializerWrite();
		w.write(0u);
		w.write(uint.MaxValue);
		w.write(123456789u);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out uint v0), "uint[0] read success");
		assertEqual(0u, v0, "uint 0");
		assert(r.read(out uint v1), "uint[1] read success");
		assertEqual(uint.MaxValue, v1, "uint max");
		assert(r.read(out uint v2), "uint[2] read success");
		assertEqual(123456789u, v2, "uint 123456789");
	}

	private static void testWriteReadLong()
	{
		var w = new SerializerWrite();
		w.write(0L);
		w.write(long.MinValue);
		w.write(long.MaxValue);
		w.write(-1L);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out long v0), "long[0] read success");
		assertEqual(0L, v0, "long 0");
		assert(r.read(out long v1), "long[1] read success");
		assertEqual(long.MinValue, v1, "long min");
		assert(r.read(out long v2), "long[2] read success");
		assertEqual(long.MaxValue, v2, "long max");
		assert(r.read(out long v3), "long[3] read success");
		assertEqual(-1L, v3, "long -1");
	}

	private static void testWriteReadULong()
	{
		var w = new SerializerWrite();
		w.write(0UL);
		w.write(ulong.MaxValue);
		w.write(1234567890123UL);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out ulong v0), "ulong[0] read success");
		assertEqual(0UL, v0, "ulong 0");
		assert(r.read(out ulong v1), "ulong[1] read success");
		assertEqual(ulong.MaxValue, v1, "ulong max");
		assert(r.read(out ulong v2), "ulong[2] read success");
		assertEqual(1234567890123UL, v2, "ulong 1234567890123");
	}

	private static void testWriteReadFloat()
	{
		var w = new SerializerWrite();
		w.write(0.0f);
		w.write(3.14159f);
		w.write(-999.9f);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out float v0), "float[0] read success");
		assertEqual(0.0f, v0, "float 0");
		assert(r.read(out float v1), "float[1] read success");
		assertEqual(3.14159f, v1, "float 3.14159");
		assert(r.read(out float v2), "float[2] read success");
		assertEqual(-999.9f, v2, "float -999.9");
	}

	private static void testWriteReadDouble()
	{
		var w = new SerializerWrite();
		w.write(0.0);
		w.write(3.141592653589793);
		w.write(-1e100);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out double v0), "double[0] read success");
		assertEqual(0.0, v0, "double 0");
		assert(r.read(out double v1), "double[1] read success");
		assertEqual(3.141592653589793, v1, "double pi");
		assert(r.read(out double v2), "double[2] read success");
		assertEqual(-1e100, v2, "double -1e100");
	}

	// ===================== BigEndian =====================

	private static void testWriteReadShortBigEndian()
	{
		var w = new SerializerWrite();
		w.writeBigEndian((short)0x1234);
		w.writeBigEndian((short)(short.MinValue + 1));

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readBigEndian(out short v0), "short BE[0] read success");
		assertEqual((short)0x1234, v0, "short BE 0x1234");
		assert(r.readBigEndian(out short v1), "short BE[1] read success");
		assertEqual(short.MinValue + 1, v1, "short BE min");
	}

	private static void testWriteReadUShortBigEndian()
	{
		var w = new SerializerWrite();
		w.writeBigEndian((ushort)0);
		w.writeBigEndian(ushort.MaxValue);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readBigEndian(out ushort v0), "ushort BE[0] read success");
		assertEqual((ushort)0, v0, "ushort BE 0");
		assert(r.readBigEndian(out ushort v1), "ushort BE[1] read success");
		assertEqual(ushort.MaxValue, v1, "ushort BE max");
	}

	private static void testWriteReadIntBigEndian()
	{
		var w = new SerializerWrite();
		w.writeBigEndian(0x12345678);
		w.writeBigEndian(int.MinValue);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readBigEndian(out int v0), "int BE[0] read success");
		assertEqual(0x12345678, v0, "int BE 0x12345678");
		assert(r.readBigEndian(out int v1), "int BE[1] read success");
		assertEqual(int.MinValue, v1, "int BE min");
	}

	private static void testWriteReadUIntBigEndian()
	{
		var w = new SerializerWrite();
		w.writeBigEndian(0u);
		w.writeBigEndian(uint.MaxValue);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readBigEndian(out uint v0), "uint BE[0] read success");
		assertEqual(0u, v0, "uint BE 0");
		assert(r.readBigEndian(out uint v1), "uint BE[1] read success");
		assertEqual(uint.MaxValue, v1, "uint BE max");
	}

	private static void testWriteReadLongBigEndian()
	{
		var w = new SerializerWrite();
		w.writeBigEndian(0x123456789ABCDEF0L);
		w.writeBigEndian(0L);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readBigEndian(out long v0), "long BE[0] read success");
		assertEqual(0x123456789ABCDEF0L, v0, "long BE pattern");
		assert(r.readBigEndian(out long v1), "long BE[1] read success");
		assertEqual(0L, v1, "long BE 0");
	}

	private static void testWriteReadULongBigEndian()
	{
		var w = new SerializerWrite();
		w.writeBigEndian(0UL);
		w.writeBigEndian(ulong.MaxValue);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readBigEndian(out ulong v0), "ulong BE[0] read success");
		assertEqual(0UL, v0, "ulong BE 0");
		assert(r.readBigEndian(out ulong v1), "ulong BE[1] read success");
		assertEqual(ulong.MaxValue, v1, "ulong BE max");
	}

	private static void testWriteReadFloatBigEndian()
	{
		var w = new SerializerWrite();
		w.writeBigEndian(1.0f);
		w.writeBigEndian(-1.0f);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readBigEndian(out float v0), "float BE[0] read success");
		assertEqual(1.0f, v0, "float BE 1.0");
		assert(r.readBigEndian(out float v1), "float BE[1] read success");
		assertEqual(-1.0f, v1, "float BE -1.0");
	}

	private static void testWriteReadDoubleBigEndian()
	{
		var w = new SerializerWrite();
		w.writeBigEndian(3.141592653589793);
		w.writeBigEndian(0.0);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readBigEndian(out double v0), "double BE[0] read success");
		assertEqual(3.141592653589793, v0, "double BE pi");
		assert(r.readBigEndian(out double v1), "double BE[1] read success");
		assertEqual(0.0, v1, "double BE 0");
	}

	// ===================== Vector =====================

	private static void testWriteReadVector2()
	{
		var w = new SerializerWrite();
		w.write(new Vector2(1.5f, -2.5f));
		w.write(new Vector2(0, 0));

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out Vector2 v0), "Vector2[0] read success");
		assertEqual(new Vector2(1.5f, -2.5f), v0, "Vector2 (1.5, -2.5)");
		assert(r.read(out Vector2 v1), "Vector2[1] read success");
		assertEqual(new Vector2(0, 0), v1, "Vector2 (0, 0)");
	}

	private static void testWriteReadVector2Int()
	{
		var w = new SerializerWrite();
		w.write(new Vector2Int(10, -20));
		w.write(new Vector2Int(int.MaxValue, int.MinValue));

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out Vector2Int v0), "Vector2Int[0] read success");
		assertEqual(new Vector2Int(10, -20), v0, "Vector2Int (10, -20)");
		assert(r.read(out Vector2Int v1), "Vector2Int[1] read success");
		assertEqual(new Vector2Int(int.MaxValue, int.MinValue), v1, "Vector2Int extreme");
	}

	private static void testWriteReadVector2UInt()
	{
		var w = new SerializerWrite();
		w.write(new Vector2UInt(100u, 200u));
		w.write(new Vector2UInt(0u, uint.MaxValue));

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out Vector2UInt v0), "Vector2UInt[0] read success");
		assertEqual(new Vector2UInt(100u, 200u), v0, "Vector2UInt (100, 200)");
		assert(r.read(out Vector2UInt v1), "Vector2UInt[1] read success");
		assertEqual(new Vector2UInt(0u, uint.MaxValue), v1, "Vector2UInt extreme");
	}

	private static void testWriteReadVector2Short()
	{
		var w = new SerializerWrite();
		w.write(new Vector2Short(100, -200));
		w.write(new Vector2Short(short.MaxValue, short.MinValue));

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out Vector2Short v0), "Vector2Short[0] read success");
		assertEqual(new Vector2Short(100, -200), v0, "Vector2Short (100, -200)");
		assert(r.read(out Vector2Short v1), "Vector2Short[1] read success");
		assertEqual(new Vector2Short(short.MaxValue, short.MinValue), v1, "Vector2Short extreme");
	}

	private static void testWriteReadVector2UShort()
	{
		var w = new SerializerWrite();
		w.write(new Vector2UShort(100, 200));
		w.write(new Vector2UShort(0, ushort.MaxValue));

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out Vector2UShort v0), "Vector2UShort[0] read success");
		assertEqual(new Vector2UShort(100, 200), v0, "Vector2UShort (100, 200)");
		assert(r.read(out Vector2UShort v1), "Vector2UShort[1] read success");
		assertEqual(new Vector2UShort(0, ushort.MaxValue), v1, "Vector2UShort extreme");
	}

	private static void testWriteReadVector3()
	{
		var w = new SerializerWrite();
		w.write(new Vector3(1f, 2f, 3f));
		w.write(new Vector3(-1f, 0f, float.MaxValue));

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out Vector3 v0), "Vector3[0] read success");
		assertEqual(new Vector3(1f, 2f, 3f), v0, "Vector3 (1,2,3)");
		assert(r.read(out Vector3 v1), "Vector3[1] read success");
		assertEqual(new Vector3(-1f, 0f, float.MaxValue), v1, "Vector3 (-1,0,max)");
	}

	private static void testWriteReadVector4()
	{
		var w = new SerializerWrite();
		w.write(new Vector4(1f, 2f, 3f, 4f));
		w.write(new Vector4(0, 0, 0, 0));

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out Vector4 v0), "Vector4[0] read success");
		assertEqual(new Vector4(1f, 2f, 3f, 4f), v0, "Vector4 (1,2,3,4)");
		assert(r.read(out Vector4 v1), "Vector4[1] read success");
		assertEqual(new Vector4(0, 0, 0, 0), v1, "Vector4 zero");
	}

	// ===================== Buffer =====================

	private static void testWriteReadBuffer()
	{
		var w = new SerializerWrite();
		byte[] src = { 0x10, 0x20, 0x30, 0x40, 0xFF };
		w.writeBuffer(src, src.Length);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		byte[] dst = new byte[5];
		assert(r.readBuffer(dst, 5, 5), "readBuffer success");
		for (int i = 0; i < src.Length; ++i)
		{
			assertEqual(src[i], dst[i], $"readBuffer[{i}]");
		}
	}

	// ===================== String =====================

	private static void testWriteReadString()
	{
		var w = new SerializerWrite();
		w.writeString("Hello World");
		w.writeString("测试中文");

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readString(out string s0), "string[0] read success");
		assertEqual("Hello World", s0, "string Hello World");
		assert(r.readString(out string s1), "string[1] read success");
		assertEqual("测试中文", s1, "string chinese");
	}

	private static void testWriteReadStringEmpty()
	{
		var w = new SerializerWrite();
		w.writeString("");
		w.writeString("non-empty");

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readString(out string s0), "empty string read success");
		assertEqual("", s0, "empty string");
		assert(r.readString(out string s1), "non-empty string read success");
		assertEqual("non-empty", s1, "after empty");
	}

	private static void testWriteReadStringNull()
	{
		var w = new SerializerWrite();
		w.writeString(null);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readString(out string s), "null string read success");
		assertEqual("", s, "null becomes empty");
	}

	private static void testWriteReadStringEncoding()
	{
		var w = new SerializerWrite();
		w.writeString("UTF8 test", Encoding.UTF8);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readString(out string s, Encoding.UTF8), "string with encoding read success");
		assertEqual("UTF8 test", s, "string with UTF8 encoding");
	}

	// ===================== Custom =====================

	private static void testWriteReadCustom()
	{
		var w = new SerializerWrite();
		var obj = new SerializerByteTestDummy();
		obj.mX = 42;
		obj.mY = -99;
		w.writeCustom(obj);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		SerializerByteTestDummy readObj = new();
		assert(r.readCustom(out readObj), "readCustom success");
		assertEqual(42, readObj.mX, "custom mX");
		assertEqual(-99, readObj.mY, "custom mY");
	}

	// ===================== List =====================

	private static void testWriteReadListByte()
	{
		var w = new SerializerWrite();
		var src = new List<byte> { 1, 2, 3, 255, 0 };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<byte>();
		assert(r.readList(dst), "readList byte success");
		assertEqual(src.Count, dst.Count, "list byte count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list byte[{i}]");
	}

	private static void testWriteReadListSByte()
	{
		var w = new SerializerWrite();
		var src = new List<sbyte> { -1, 0, 127, -128 };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<sbyte>();
		assert(r.readList(dst), "readList sbyte success");
		assertEqual(src.Count, dst.Count, "list sbyte count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list sbyte[{i}]");
	}

	private static void testWriteReadListShort()
	{
		var w = new SerializerWrite();
		var src = new List<short> { -100, 0, 200, short.MaxValue, short.MinValue };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<short>();
		assert(r.readList(dst), "readList short success");
		assertEqual(src.Count, dst.Count, "list short count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list short[{i}]");
	}

	private static void testWriteReadListUShort()
	{
		var w = new SerializerWrite();
		var src = new List<ushort> { 0, 65535, 12345 };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<ushort>();
		assert(r.readList(dst), "readList ushort success");
		assertEqual(src.Count, dst.Count, "list ushort count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list ushort[{i}]");
	}

	private static void testWriteReadListInt()
	{
		var w = new SerializerWrite();
		var src = new List<int> { -1, 0, 1, int.MaxValue, int.MinValue };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<int>();
		assert(r.readList(dst), "readList int success");
		assertEqual(src.Count, dst.Count, "list int count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list int[{i}]");
	}

	private static void testWriteReadListUInt()
	{
		var w = new SerializerWrite();
		var src = new List<uint> { 0, 1, uint.MaxValue, 123456789u };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<uint>();
		assert(r.readList(dst), "readList uint success");
		assertEqual(src.Count, dst.Count, "list uint count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list uint[{i}]");
	}

	private static void testWriteReadListLong()
	{
		var w = new SerializerWrite();
		var src = new List<long> { -1L, 0L, long.MaxValue, long.MinValue };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<long>();
		assert(r.readList(dst), "readList long success");
		assertEqual(src.Count, dst.Count, "list long count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list long[{i}]");
	}

	private static void testWriteReadListULong()
	{
		var w = new SerializerWrite();
		var src = new List<ulong> { 0UL, 1UL, ulong.MaxValue };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<ulong>();
		assert(r.readList(dst), "readList ulong success");
		assertEqual(src.Count, dst.Count, "list ulong count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list ulong[{i}]");
	}

	private static void testWriteReadListFloat()
	{
		var w = new SerializerWrite();
		var src = new List<float> { 0.0f, -1.5f, 3.14f, float.MaxValue };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<float>();
		assert(r.readList(dst), "readList float success");
		assertEqual(src.Count, dst.Count, "list float count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list float[{i}]");
	}

	private static void testWriteReadListDouble()
	{
		var w = new SerializerWrite();
		var src = new List<double> { 0.0, -1.5, 3.141592653589793 };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<double>();
		assert(r.readList(dst), "readList double success");
		assertEqual(src.Count, dst.Count, "list double count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list double[{i}]");
	}

	private static void testWriteReadListString()
	{
		var w = new SerializerWrite();
		var src = new List<string> { "hello", "", "world", "测试" };
		w.writeList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<string>();
		assert(r.readList(dst), "readList string success");
		assertEqual(src.Count, dst.Count, "list string count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list string[{i}]");
	}

	// ===================== Custom List =====================

	private static void testWriteReadCustomList()
	{
		var w = new SerializerWrite();
		var src = new List<SerializerByteTestDummy>();
		for (int i = 0; i < 3; ++i)
		{
			src.Add(new SerializerByteTestDummy { mX = i * 10, mY = i * -10 });
		}
		w.writeCustomList(src);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<SerializerByteTestDummy>();
		assert(r.readCustomList(dst), "readCustomList success");
		assertEqual(src.Count, dst.Count, "custom list count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertEqual(src[i].mX, dst[i].mX, $"custom list[{i}].mX");
			assertEqual(src[i].mY, dst[i].mY, $"custom list[{i}].mY");
		}
	}

	// ===================== Enum =====================

	private static void testReadEnumByte()
	{
		var w = new SerializerWrite();
		w.write((byte)SerializerByteTestEnum.Zero);
		w.write((byte)SerializerByteTestEnum.One);
		w.write((byte)SerializerByteTestEnum.Two);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readEnumByte(out SerializerByteTestEnum v0), "readEnumByte[0]");
		assertEqual(SerializerByteTestEnum.Zero, v0, "enum Zero");
		assert(r.readEnumByte(out SerializerByteTestEnum v1), "readEnumByte[1]");
		assertEqual(SerializerByteTestEnum.One, v1, "enum One");
		assert(r.readEnumByte(out SerializerByteTestEnum v2), "readEnumByte[2]");
		assertEqual(SerializerByteTestEnum.Two, v2, "enum Two");
	}

	private static void testReadEnumInt()
	{
		var w = new SerializerWrite();
		w.write((int)SerializerByteTestEnum.Zero);
		w.write((int)SerializerByteTestEnum.One);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.readEnumInt(out SerializerByteTestEnum v0), "readEnumInt[0]");
		assertEqual(SerializerByteTestEnum.Zero, v0, "enumInt Zero");
		assert(r.readEnumInt(out SerializerByteTestEnum v1), "readEnumInt[1]");
		assertEqual(SerializerByteTestEnum.One, v1, "enumInt One");
	}

	private static void testReadEnumList()
	{
		var w = new SerializerWrite();
		var src = new List<byte> { 0, 1, 2 };
		w.write(src.Count);
		foreach (var b in src) w.write(b);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		var dst = new List<SerializerByteTestEnum>();
		assert(r.readEnumByteList(dst), "readEnumByteList success");
		assertEqual(3, dst.Count, "enum list count");
		assertEqual(SerializerByteTestEnum.Zero, dst[0], "enum list[0]");
		assertEqual(SerializerByteTestEnum.One, dst[1], "enum list[1]");
		assertEqual(SerializerByteTestEnum.Two, dst[2], "enum list[2]");
	}

	// ===================== 工具方法 =====================

	private static void testSkipIndex()
	{
		var w = new SerializerWrite();
		w.write(1);
		w.write(2);
		w.write(3);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.skipIndex(4); // 跳过第一个int
		assert(r.read(out int v), "skipIndex read");
		assertEqual(2, v, "skipIndex value after skip");
		r.skipIndex(4); // 跳过第三个int
		assertEqual(12, r.getIndex(), "skipIndex index after two skips");
	}

	private static void testSetIndex()
	{
		var w = new SerializerWrite();
		w.write(100);
		w.write(200);
		w.write(300);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.setIndex(4); // 跳到第二个int
		assert(r.read(out int v), "setIndex read");
		assertEqual(200, v, "setIndex value");
	}

	private static void testClear()
	{
		var w = new SerializerWrite();
		w.write(42);
		assert(w.getDataSize() > 0, "getDataSize after write > 0");
		w.clear();
		assertEqual(0, w.getDataSize(), "getDataSize after clear == 0");
		// 清空后可以继续写入
		w.write(99);
		w.write(1);
		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out int v), "clear then write read");
		assertEqual(99, v, "value after clear");
	}

	private static void testGetBuffer()
	{
		var w = new SerializerWrite();
		// 未写入时 buffer 为 null
		// 写入后自动创建
		w.write(42);
		assertNotNull(w.getBuffer(), "getBuffer not null after write");
	}

	private static void testGetDataSize()
	{
		var w = new SerializerWrite();
		assertEqual(0, w.getDataSize(), "getDataSize initially 0");
		w.write(42); // int = 4 bytes
		assertEqual(4, w.getDataSize(), "getDataSize after 1 int");
		w.write((byte)1); // byte = 1 byte
		assertEqual(5, w.getDataSize(), "getDataSize after int+byte");
	}

	private static void testGetIndex()
	{
		var w = new SerializerWrite();
		w.write(1);
		w.write(2);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assertEqual(0, r.getIndex(), "getIndex initially 0");
		r.read(out int _);
		assertEqual(4, r.getIndex(), "getIndex after 1 int");
		r.read(out int _);
		assertEqual(8, r.getIndex(), "getIndex after 2 ints");
	}

	private static void testSetNeedCheck()
	{
		var w = new SerializerWrite();
		w.write(42);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		r.setNeedCheck(false);
		// needCheck=false 时跳过越界检查，但数据足够时仍然可以正确读取
		assert(r.read(out int v), "setNeedCheck(false) read");
		assertEqual(42, v, "setNeedCheck(false) value");
	}

	// ===================== Init 变体 =====================

	private static void testInitWithOffset()
	{
		var w = new SerializerWrite();
		w.write(0);     // offset 0
		w.write(99);    // offset 4
		w.write(1);     // offset 8

		// 从偏移 4 开始读
		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize(), 4);
		assert(r.read(out int v), "init with offset read");
		assertEqual(99, v, "init with offset value");
		assertEqual(8, r.getIndex(), "init with offset index after read");
	}

	// ===================== 边界条件 =====================

	private static void testReadOverflow()
	{
		var w = new SerializerWrite();
		w.write(42); // 4 bytes

		var r = new SerializerRead();
		r.init(new byte[0], 0); // 空 buffer
		// 读任何值都应该失败
		assert(!r.read(out bool _), "read bool from empty buffer returns false");
		assert(!r.read(out int _), "read int from empty buffer returns false");
	}

	private static void testNeedCheckDisabled()
	{
		var w = new SerializerWrite();
		w.write(1); // 4 bytes

		var r = new SerializerRead();
		r.init(new byte[0], 0);
		r.setNeedCheck(false);
		// needCheck=false 时不检查，但实际数据不足，read 内部仍可能失败
		// 至少不会崩溃
		r.read(out bool _);
	}

	// ===================== 多点连续读写 =====================

	private static void testMultipleTypesSequence()
	{
		var w = new SerializerWrite();
		w.write(true);
		w.write((byte)0xAB);
		w.write((short)1000);
		w.write(999999);
		w.write(3.14f);
		w.writeString("seq");
		w.writeBigEndian((int)0x12345678);

		var r = new SerializerRead();
		r.init(w.getBuffer(), w.getDataSize());
		assert(r.read(out bool b), "seq bool");
		assert(b, "seq bool true");
		assert(r.read(out byte by), "seq byte");
		assertEqual((byte)0xAB, by, "seq byte");
		assert(r.read(out short s), "seq short");
		assertEqual((short)1000, s, "seq short");
		assert(r.read(out int i), "seq int");
		assertEqual(999999, i, "seq int");
		assert(r.read(out float f), "seq float");
		assertEqual(3.14f, f, "seq float");
		assert(r.readString(out string str), "seq string");
		assertEqual("seq", str, "seq string");
		assert(r.readBigEndian(out int be), "seq BE int");
		assertEqual(0x12345678, be, "seq BE int");
	}
}
#endif