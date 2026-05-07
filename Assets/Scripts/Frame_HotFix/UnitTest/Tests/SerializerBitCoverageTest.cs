#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;
using static MathUtility;

// 测试用 SerializableBit 实现
public class SerializerBitCoverageTestDummy : SerializableBit
{
	public int mX;
	public int mY;
	public override bool read(SerializerBitRead reader, bool needReadSign)
	{
		return reader.read(out mX, out mY, needReadSign);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.write(stackalloc int[2] { mX, mY }, needWriteSign);
	}
}

// SerializerBitWrite + SerializerBitRead 遗漏接口补充覆盖
// 基础 write/read(bool/int/float/long/string) + List<int> 已在 SerializerBitTest 覆盖
public static class SerializerBitCoverageTest
{
	public static void Run()
	{
		// ─── 标量类型补充 ───
		testWriteReadByte();
		testWriteReadSByte();
		testWriteReadShort();
		testWriteReadUShort();
		testWriteReadUInt();
		testWriteReadULong();
		testWriteReadDouble();

		// ─── Span 批量写入 ───
		testWriteReadSpanByte();
		testWriteReadSpanSByte();
		testWriteReadSpanShort();
		testWriteReadSpanUShort();
		testWriteReadSpanInt();
		testWriteReadSpanUInt();
		testWriteReadSpanLong();
		testWriteReadSpanULong();
		testWriteReadSpanFloat();
		testWriteReadSpanDouble();

		// ─── 多值 read 方法 ───
		testReadMultiByte();
		testReadMultiSByte();
		testReadMultiShort();
		testReadMultiUShort();
		testReadMultiInt();
		testReadMultiUInt();
		testReadMultiLong();
		testReadMultiULong();
		testReadMultiFloat();
		testReadMultiDouble();

		// ─── Vector 类型 ───
		testWriteReadVector2();
		testWriteReadVector2UShort();
		testWriteReadVector2Short();
		testWriteReadVector2Int();
		testWriteReadVector2IntMy();
		testWriteReadVector2UInt();
		testWriteReadVector3();
		testWriteReadVector3Int();
		testWriteReadVector4();

		// ─── Buffer ───
		testWriteReadBuffer();

		// ─── fillZeroToByteEnd / skipToByteEnd ───
		testFillZeroToByteEnd();
		testSkipToByteEnd();

		// ─── String（byte[] 版本+溢出） ───
		testReadStringToByteArray();
		testReadStringToByteArrayOverflow();

		// ─── CustomList ───
		testWriteReadCustomList();

		// ─── 更多 List 类型 ───
		testWriteReadListUShort();
		testWriteReadListUInt();
		testWriteReadListVector2();
		testWriteReadListVector2UShort();
		testWriteReadListVector2Int();
		testWriteReadListVector3();
		testWriteReadListVector4();
		testWriteReadListString();

		// ─── Enum ───
		testReadEnumByte();
		testReadEnumInt();
		testReadEnumList();

		// ─── 空列表 ───
		testEmptyList();

		// ─── 工具方法 ───
		testGetBitCount();
		testGetBufferSize();
		testGetBitIndex();
		testGetReadByteCount();
		testClear();

		// ─── 边界值 ───
		testExtremeValues();
		testMaxUnsignedValues();
	}

	// ===================== 标量类型补充 =====================

	private static void testWriteReadByte()
	{
		var w = new SerializerBitWrite();
		w.write((byte)0);
		w.write((byte)255);
		w.write((byte)128);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out byte v0), "byte[0]");
		assertEqual((byte)0, v0, "byte 0");
		assert(r.read(out byte v1), "byte[1]");
		assertEqual((byte)255, v1, "byte 255");
		assert(r.read(out byte v2), "byte[2]");
		assertEqual((byte)128, v2, "byte 128");
	}

	private static void testWriteReadSByte()
	{
		var w = new SerializerBitWrite();
		w.write((sbyte)0, true);
		w.write((sbyte)(sbyte.MinValue + 1), true);
		w.write(sbyte.MaxValue, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out sbyte v0, true), "sbyte[0]");
		assertEqual((sbyte)0, v0, "sbyte 0");
		assert(r.read(out sbyte v1, true), "sbyte[1]");
		assertEqual(sbyte.MinValue + 1, v1, "sbyte min");
		assert(r.read(out sbyte v2, true), "sbyte[2]");
		assertEqual(sbyte.MaxValue, v2, "sbyte max");

		// 无符号模式
		w = new SerializerBitWrite();
		w.write((sbyte)127, false);
		r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out sbyte v, false), "sbyte unsigned");
		assertEqual((sbyte)127, v, "sbyte unsigned 127");
	}

	private static void testWriteReadShort()
	{
		var w = new SerializerBitWrite();
		w.write((short)0, true);
		w.write((short)(short.MinValue + 1), true);
		w.write(short.MaxValue, true);
		w.write((short)-1, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out short v0, true), "short[0]");
		assertEqual((short)0, v0, "short 0");
		assert(r.read(out short v1, true), "short[1]");
		assertEqual(short.MinValue + 1, v1, "short min");
		assert(r.read(out short v2, true), "short[2]");
		assertEqual(short.MaxValue, v2, "short max");
		assert(r.read(out short v3, true), "short[3]");
		assertEqual((short)-1, v3, "short -1");

		// 无符号模式
		w = new SerializerBitWrite();
		w.write((short)32767, false);
		r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out short v, false), "short unsigned");
		assertEqual((short)32767, v, "short unsigned 32767");
	}

	private static void testWriteReadUShort()
	{
		var w = new SerializerBitWrite();
		w.write((ushort)0);
		w.write(ushort.MaxValue);
		w.write((ushort)50000);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out ushort v0), "ushort[0]");
		assertEqual((ushort)0, v0, "ushort 0");
		assert(r.read(out ushort v1), "ushort[1]");
		assertEqual(ushort.MaxValue, v1, "ushort max");
		assert(r.read(out ushort v2), "ushort[2]");
		assertEqual((ushort)50000, v2, "ushort 50000");
	}

	private static void testWriteReadUInt()
	{
		var w = new SerializerBitWrite();
		w.write(0u);
		w.write(uint.MaxValue);
		w.write(123456789u);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out uint v0), "uint[0]");
		assertEqual(0u, v0, "uint 0");
		assert(r.read(out uint v1), "uint[1]");
		assertEqual(uint.MaxValue, v1, "uint max");
		assert(r.read(out uint v2), "uint[2]");
		assertEqual(123456789u, v2, "uint 123456789");
	}

	private static void testWriteReadULong()
	{
		var w = new SerializerBitWrite();
		w.write(0UL);
		w.write(ulong.MaxValue);
		w.write(1234567890123UL);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out ulong v0), "ulong[0]");
		assertEqual(0UL, v0, "ulong 0");
		assert(r.read(out ulong v1), "ulong[1]");
		assertEqual(ulong.MaxValue, v1, "ulong max");
		assert(r.read(out ulong v2), "ulong[2]");
		assertEqual(1234567890123UL, v2, "ulong 1234567890123");
	}

	private static void testWriteReadDouble()
	{
		var w = new SerializerBitWrite();
		w.write(3.14159, true, 6);
		w.write(0.0, false);
		w.write(-1e100, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out double v0, true, 6), "double[0]");
		assert(Math.Abs(3.14159 - v0) < 0.001, "double pi: " + v0);
		assert(r.read(out double v1, false), "double[1]");
		assert(Math.Abs(0.0 - v1) < 0.001, "double 0: " + v1);
		assert(r.read(out double v2, true), "double[2]");
		assert(Math.Abs(0 - v2) < 0.001, "double 0: " + v2);

		// 自定义精度
		w = new SerializerBitWrite();
		w.write(1.2345, true, 5);
		r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out double dv, true, 5), "double precision 5");
		assert(Math.Abs(1.2345 - dv) < 0.00001, "double precision 5 value: " + dv);
	}

	// ===================== Span 批量写入 =====================

	private static void testWriteReadSpanByte()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<byte>(new byte[] { 10, 20, 30 }));
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new byte[3];
		var span = new Span<byte>(dst);
		assert(r.read(ref span), "Span<byte> read");
		assertEqual((byte)10, dst[0], "Span<byte>[0]");
		assertEqual((byte)20, dst[1], "Span<byte>[1]");
		assertEqual((byte)30, dst[2], "Span<byte>[2]");
	}

	private static void testWriteReadSpanSByte()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<sbyte>(new sbyte[] { -1, 0, 127 }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new sbyte[3];
		var span = new Span<sbyte>(dst);
		assert(r.read(ref span, true), "Span<sbyte> read");
		assertEqual((sbyte)-1, dst[0], "Span<sbyte>[0]");
		assertEqual((sbyte)0, dst[1], "Span<sbyte>[1]");
		assertEqual((sbyte)127, dst[2], "Span<sbyte>[2]");
	}

	private static void testWriteReadSpanShort()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<short>(new short[] { -100, 0, 200, short.MinValue + 1 }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new short[4];
		var span = new Span<short>(dst);
		assert(r.read(ref span, true), "Span<short> read");
		assertEqual((short)-100, dst[0], "Span<short>[0]");
		assertEqual((short)0, dst[1], "Span<short>[1]");
		assertEqual((short)200, dst[2], "Span<short>[2]");
		assertEqual(short.MinValue + 1, dst[3], "Span<short>[3]");
	}

	private static void testWriteReadSpanUShort()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<ushort>(new ushort[] { 0, 65535, 40000 }));
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new ushort[3];
		var span = new Span<ushort>(dst);
		assert(r.read(ref span), "Span<ushort> read");
		assertEqual((ushort)0, dst[0], "Span<ushort>[0]");
		assertEqual((ushort)65535, dst[1], "Span<ushort>[1]");
		assertEqual((ushort)40000, dst[2], "Span<ushort>[2]");
	}

	private static void testWriteReadSpanInt()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<int>(new int[] { 1, -2, 3, int.MinValue + 1, int.MaxValue }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new int[5];
		var span = new Span<int>(dst);
		assert(r.read(ref span, true), "Span<int> read");
		assertEqual(1, dst[0], "Span<int>[0]");
		assertEqual(-2, dst[1], "Span<int>[1]");
		assertEqual(3, dst[2], "Span<int>[2]");
		assertEqual(int.MinValue + 1, dst[3], "Span<int>[3]");
		assertEqual(int.MaxValue, dst[4], "Span<int>[4]");
	}

	private static void testWriteReadSpanUInt()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<uint>(new uint[] { 0, uint.MaxValue, 88888888u }));
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new uint[3];
		var span = new Span<uint>(dst);
		assert(r.read(ref span), "Span<uint> read");
		assertEqual(0u, dst[0], "Span<uint>[0]");
		assertEqual(uint.MaxValue, dst[1], "Span<uint>[1]");
		assertEqual(88888888u, dst[2], "Span<uint>[2]");
	}

	private static void testWriteReadSpanLong()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<long>(new long[] { long.MinValue + 1, 0, long.MaxValue }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new long[3];
		var span = new Span<long>(dst);
		assert(r.read(ref span, true), "Span<long> read");
		assertEqual(long.MinValue + 1, dst[0], "Span<long>[0]");
		assertEqual(0L, dst[1], "Span<long>[1]");
		assertEqual(long.MaxValue, dst[2], "Span<long>[2]");
	}

	private static void testWriteReadSpanULong()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<ulong>(new ulong[] { 0, ulong.MaxValue }));
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new ulong[2];
		var span = new Span<ulong>(dst);
		assert(r.read(ref span), "Span<ulong> read");
		assertEqual(0UL, dst[0], "Span<ulong>[0]");
		assertEqual(ulong.MaxValue, dst[1], "Span<ulong>[1]");
	}

	private static void testWriteReadSpanFloat()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<float>(new float[] { 1.5f, -2.5f, 3.14f }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new float[3];
		var span = new Span<float>(dst);
		assert(r.read(ref span, true), "Span<float> read");
		assert(Math.Abs(1.5f - dst[0]) < 0.001f, "Span<float>[0]: " + dst[0]);
		assert(Math.Abs(-2.5f - dst[1]) < 0.001f, "Span<float>[1]: " + dst[1]);
		assert(Math.Abs(3.14f - dst[2]) < 0.001f, "Span<float>[2]: " + dst[2]);
	}

	private static void testWriteReadSpanDouble()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<double>(new double[] { 1.2345, -6.789, 0.0 }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new double[3];
		var span = new Span<double>(dst);
		assert(r.read(ref span, true), "Span<double> read");
		assert(Math.Abs(1.2345 - dst[0]) < 0.001, "Span<double>[0]: " + dst[0]);
		assert(Math.Abs(-6.789 - dst[1]) < 0.001, "Span<double>[1]: " + dst[1]);
		assert(Math.Abs(0.0 - dst[2]) < 0.001, "Span<double>[2]: " + dst[2]);
	}

	// ===================== 多值 read 方法 =====================

	private static void testReadMultiByte()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<byte>(new byte[] { 10, 20, 30, 40 }));
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());

		assert(r.read(out byte v0, out byte v1, out byte v2, out byte v3), "read 4 bytes");
		assertEqual((byte)10, v0, "multi byte[0]");
		assertEqual((byte)20, v1, "multi byte[1]");
		assertEqual((byte)30, v2, "multi byte[2]");
		assertEqual((byte)40, v3, "multi byte[3]");
	}

	private static void testReadMultiSByte()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<sbyte>(new sbyte[] { -1, 0, 127, -127 }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());

		assert(r.read(out sbyte v0, out sbyte v1, out sbyte v2, out sbyte v3, true), "read 4 sbytes");
		assertEqual((sbyte)-1, v0, "multi sbyte[0]");
		assertEqual((sbyte)0, v1, "multi sbyte[1]");
		assertEqual((sbyte)127, v2, "multi sbyte[2]");
		assertEqual((sbyte)-127, v3, "multi sbyte[3]");
	}

	private static void testReadMultiShort()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<short>(new short[] { 100, -200, short.MinValue + 1 }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());

		assert(r.read(out short v0, out short v1, out short v2, true), "read 3 shorts");
		assertEqual((short)100, v0, "multi short[0]");
		assertEqual((short)-200, v1, "multi short[1]");
		assertEqual(short.MinValue + 1, v2, "multi short[2]");
	}

	private static void testReadMultiUShort()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<ushort>(new ushort[] { 100, 200, 300, 400 }));
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());

		assert(r.read(out ushort v0, out ushort v1, out ushort v2, out ushort v3), "read 4 ushorts");
		assertEqual((ushort)100, v0, "multi ushort[0]");
		assertEqual((ushort)200, v1, "multi ushort[1]");
		assertEqual((ushort)300, v2, "multi ushort[2]");
		assertEqual((ushort)400, v3, "multi ushort[3]");
	}

	private static void testReadMultiInt()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<int>(new int[] { 1, -2, 3, -4 }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());

		assert(r.read(out int v0, out int v1, out int v2, out int v3, true), "read 4 ints");
		assertEqual(1, v0, "multi int[0]");
		assertEqual(-2, v1, "multi int[1]");
		assertEqual(3, v2, "multi int[2]");
		assertEqual(-4, v3, "multi int[3]");
	}

	private static void testReadMultiUInt()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<uint>(new uint[] { 10, 20, 30, 40 }));
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());

		assert(r.read(out uint v0, out uint v1, out uint v2, out uint v3), "read 4 uints");
		assertEqual(10u, v0, "multi uint[0]");
		assertEqual(20u, v1, "multi uint[1]");
		assertEqual(30u, v2, "multi uint[2]");
		assertEqual(40u, v3, "multi uint[3]");
	}

	private static void testReadMultiLong()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<long>(new long[] { long.MinValue + 1, long.MaxValue, 0 }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());

		assert(r.read(out long v0, out long v1, out long v2, true), "read 3 longs");
		assertEqual(long.MinValue + 1, v0, "multi long[0]");
		assertEqual(long.MaxValue, v1, "multi long[1]");
		assertEqual(0L, v2, "multi long[2]");
	}

	private static void testReadMultiULong()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<ulong>(new ulong[] { 0, ulong.MaxValue, 1234567890123UL }));
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());

		assert(r.read(out ulong v0, out ulong v1, out ulong v2), "read3 ulongs");
		assertEqual(0UL, v0, "multi ulong[0]");
		assertEqual(ulong.MaxValue, v1, "multi ulong[1]");
		assertEqual(1234567890123UL, v2, "multi ulong[2]");
	}

	private static void testReadMultiFloat()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<float>(new float[] { 1.5f, -2.5f, 3.14f, -0.0f }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());

		assert(r.read(out float v0, out float v1, out float v2, out float v3, true), "read 2 floats");
		assert(Math.Abs(1.5f - v0) < 0.001f, "multi float[0]: " + v0);
		assert(Math.Abs(-2.5f - v1) < 0.001f, "multi float[1]: " + v1);
		assert(Math.Abs(3.14f - v2) < 0.001f, "multi float[2]: " + v2);
		assert(Math.Abs(-0.0f - v3) < 0.001f, "multi float[3]: " + v3);
	}

	private static void testReadMultiDouble()
	{
		var w = new SerializerBitWrite();
		w.write(new Span<double>(new double[] { 1.2345, -6.789, 0.0 }), true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());

		assert(r.read(out double v0, out double v1, out double v2, true), "read 2 doubles");
		assert(Math.Abs(1.2345 - v0) < 0.0001, "multi double[0]: " + v0);
		assert(Math.Abs(-6.789 - v1) < 0.0001, "multi double[1]: " + v1);
		assert(Math.Abs(0.0 - v2) < 0.0001, "multi double[2]: " + v2);
	}

	// ===================== Vector 类型 =====================

	private static void testWriteReadVector2()
	{
		var w = new SerializerBitWrite();
		w.write(new Vector2(1.5f, -2.5f), true);
		w.write(new Vector2(0, 0), false);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out Vector2 v0, true), "Vector2[0]");
		assert(Math.Abs(1.5f - v0.x) < 0.001f && Math.Abs(-2.5f - v0.y) < 0.001f, "Vector2 signed: " + v0);
		assert(r.read(out Vector2 v1, false), "Vector2[1]");
		assert(Math.Abs(0 - v1.x) < 0.001f && Math.Abs(0 - v1.y) < 0.001f, "Vector2 zero unsigned: " + v1);
	}

	private static void testWriteReadVector2UShort()
	{
		var w = new SerializerBitWrite();
		w.write(new Vector2UShort(100, 200));
		w.write(new Vector2UShort(0, ushort.MaxValue));

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out Vector2UShort v0), "Vector2UShort[0]");
		assertEqual(new Vector2UShort(100, 200), v0, "Vector2UShort (100,200)");
		assert(r.read(out Vector2UShort v1), "Vector2UShort[1]");
		assertEqual(new Vector2UShort(0, ushort.MaxValue), v1, "Vector2UShort extreme");
	}

	private static void testWriteReadVector2Short()
	{
		var w = new SerializerBitWrite();
		w.write(new Vector2Short(100, -200), true);
		w.write(new Vector2Short(short.MinValue + 1, short.MaxValue), true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out Vector2Short v0, true), "Vector2Short[0]");
		assertEqual(new Vector2Short(100, -200), v0, "Vector2Short (100,-200)");
		assert(r.read(out Vector2Short v1, true), "Vector2Short[1]");
		assertEqual(new Vector2Short(short.MinValue + 1, short.MaxValue), v1, "Vector2Short extreme");
	}

	private static void testWriteReadVector2Int()
	{
		var w = new SerializerBitWrite();
		w.write(new Vector2Int(10, -20), true);
		w.write(new Vector2Int(int.MaxValue, int.MinValue + 1), true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out Vector2Int v0, true), "Vector2Int[0]");
		assertEqual(new Vector2Int(10, -20), v0, "Vector2Int (10,-20)");
		assert(r.read(out Vector2Int v1, true), "Vector2Int[1]");
		assertEqual(new Vector2Int(int.MaxValue, int.MinValue + 1), v1, "Vector2Int extreme");
	}

	private static void testWriteReadVector2IntMy()
	{
		var w = new SerializerBitWrite();
		w.write(new Vector2IntMy(10, -20), true);
		w.write(new Vector2IntMy(int.MinValue + 1, int.MaxValue), true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out Vector2IntMy v0, true), "Vector2IntMy[0]");
		assertEqual(new Vector2IntMy(10, -20), v0, "Vector2IntMy (10,-20)");
		assert(r.read(out Vector2IntMy v1, true), "Vector2IntMy[1]");
		assertEqual(new Vector2IntMy(int.MinValue + 1, int.MaxValue), v1, "Vector2IntMy extreme");
	}

	private static void testWriteReadVector2UInt()
	{
		var w = new SerializerBitWrite();
		w.write(new Vector2UInt(100u, 200u));
		w.write(new Vector2UInt(0u, uint.MaxValue));

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out Vector2UInt v0), "Vector2UInt[0]");
		assertEqual(new Vector2UInt(100u, 200u), v0, "Vector2UInt (100,200)");
		assert(r.read(out Vector2UInt v1), "Vector2UInt[1]");
		assertEqual(new Vector2UInt(0u, uint.MaxValue), v1, "Vector2UInt extreme");
	}

	private static void testWriteReadVector3()
	{
		var w = new SerializerBitWrite();
		w.write(new Vector3(1f, 2f, 3f), true);
		w.write(new Vector3(-1f, 0f, float.MaxValue), true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out Vector3 v0, true), "Vector3[0]");
		assert(Math.Abs(1f - v0.x) < 0.001f && Math.Abs(2f - v0.y) < 0.001f && Math.Abs(3f - v0.z) < 0.001f, "Vector3 (1,2,3): " + v0);
		assert(r.read(out Vector3 v1, true), "Vector3[1]");
		assert(Math.Abs(-1f - v1.x) < 0.001f && Math.Abs(0f - v1.y) < 0.001f, "Vector3 extreme: " + v1);
	}

	private static void testWriteReadVector3Int()
	{
		var w = new SerializerBitWrite();
		w.write(new Vector3Int(1, 2, 3), true);
		w.write(new Vector3Int(-1, 0, int.MaxValue), true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out Vector3Int v0, true), "Vector3Int[0]");
		assertEqual(new Vector3Int(1, 2, 3), v0, "Vector3Int (1,2,3)");
		assert(r.read(out Vector3Int v1, true), "Vector3Int[1]");
		assertEqual(new Vector3Int(-1, 0, int.MaxValue), v1, "Vector3Int extreme");
	}

	private static void testWriteReadVector4()
	{
		var w = new SerializerBitWrite();
		w.write(new Vector4(1f, 2f, 3f, 4f), true);
		w.write(new Vector4(0, 0, 0, 0), false);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out Vector4 v0, true), "Vector4[0]");
		assert(Math.Abs(1f - v0.x) < 0.001f && Math.Abs(2f - v0.y) < 0.001f && Math.Abs(3f - v0.z) < 0.001f && Math.Abs(4f - v0.w) < 0.001f, "Vector4 (1,2,3,4): " + v0);
		assert(r.read(out Vector4 v1, false), "Vector4[1]");
		assert(Math.Abs(0 - v1.x) < 0.001f && Math.Abs(0 - v1.y) < 0.001f && Math.Abs(0 - v1.z) < 0.001f && Math.Abs(0 - v1.w) < 0.001f, "Vector4 zero: " + v1);
	}

	// ===================== Buffer =====================

	private static void testWriteReadBuffer()
	{
		var w = new SerializerBitWrite();
		byte[] src = { 0x10, 0x20, 0x30, 0x40, 0xFF };
		w.writeBuffer(src, src.Length);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		byte[] dst = new byte[5];
		assert(r.readBuffer(dst, 5), "readBuffer success");
		for (int i = 0; i < src.Length; ++i)
			assertEqual(src[i], dst[i], $"readBuffer[{i}]");
	}

	// ===================== fillZeroToByteEnd / skipToByteEnd =====================

	private static void testFillZeroToByteEnd()
	{
		var w = new SerializerBitWrite();
		w.write(1, true); // 写入一些位，不一定是整字节
		w.fillZeroToByteEnd();
		int byteCountAfter = w.getByteCount();
		// fill 后位下标应在字节边界上
		assertEqual(byteCountAfter * 8, w.getBitCount(), "fillZeroToByteEnd bitIndex aligned");
	}

	private static void testSkipToByteEnd()
	{
		var w = new SerializerBitWrite();
		w.write(1, true); // 写入 int(1)，位偏移不为0
		w.write(2, false);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		r.read(out int v0, true);
		r.skipToByteEnd();
		// skipToByteEnd 后 bitIndex 应对齐到字节边界（8的倍数）
		assertEqual(0, r.getBitIndex() & 7, "skipToByteEnd bitIndex aligned to byte");
	}

	// ===================== String byte[] 版本 =====================

	private static void testReadStringToByteArray()
	{
		var w = new SerializerBitWrite();
		w.writeString("Hello");

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		byte[] buf = new byte[32];
		assert(r.readString(buf, buf.Length), "readString to byte[] success");
	}

	private static void testReadStringToByteArrayOverflow()
	{
		var w = new SerializerBitWrite();
		w.writeString("This is a long string for overflow test");

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		byte[] small = new byte[5];
		// buffer 大小不足，应返回 false 但不会崩溃
		bool result = r.readString(small, small.Length);
		assert(!result, "readString to byte[] overflow returns false");
	}

	// ===================== CustomList =====================

	private static void testWriteReadCustomList()
	{
		var w = new SerializerBitWrite();
		var src = new List<SerializerBitCoverageTestDummy>();
		for (int i = 0; i < 3; ++i)
		{
			src.Add(new SerializerBitCoverageTestDummy { mX = i * 100, mY = i * -100 });
		}
		w.writeCustomList(src, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new List<SerializerBitCoverageTestDummy>();
		assert(r.readCustomList(dst, true), "readCustomList success");
		assertEqual(src.Count, dst.Count, "custom list count");
		for (int i = 0; i < src.Count; ++i)
		{
			assertEqual(src[i].mX, dst[i].mX, $"custom list[{i}].mX");
			assertEqual(src[i].mY, dst[i].mY, $"custom list[{i}].mY");
		}
	}

	// ===================== 更多 List 类型 =====================

	private static void testWriteReadListUShort()
	{
		var w = new SerializerBitWrite();
		var src = new List<ushort> { 0, 65535, 50000 };
		w.writeList(src);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new List<ushort>();
		assert(r.readList(dst), "readList ushort success");
		assertEqual(src.Count, dst.Count, "list ushort count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list ushort[{i}]");
	}

	private static void testWriteReadListUInt()
	{
		var w = new SerializerBitWrite();
		var src = new List<uint> { 0, uint.MaxValue, 123456789u };
		w.writeList(src);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new List<uint>();
		assert(r.readList(dst), "readList uint success");
		assertEqual(src.Count, dst.Count, "list uint count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list uint[{i}]");
	}

	private static void testWriteReadListVector2()
	{
		var w = new SerializerBitWrite();
		var src = new List<Vector2> { new(1.5f, -2.5f), new(0, 0) };
		w.writeList(src, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new List<Vector2>();
		assert(r.readList(dst, true), "readList Vector2 success");
		assertEqual(src.Count, dst.Count, "list Vector2 count");
		for (int i = 0; i < src.Count; ++i)
		{
			assert(Math.Abs(src[i].x - dst[i].x) < 0.001f && Math.Abs(src[i].y - dst[i].y) < 0.001f, $"list Vector2[{i}]: " + dst[i]);
		}
	}

	private static void testWriteReadListVector2UShort()
	{
		var w = new SerializerBitWrite();
		var src = new List<Vector2UShort> { new(10, 20), new(0, 65535) };
		w.writeList(src);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new List<Vector2UShort>();
		assert(r.readList(dst), "readList Vector2UShort success");
		assertEqual(src.Count, dst.Count, "list Vector2UShort count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list Vector2UShort[{i}]");
	}

	private static void testWriteReadListVector2Int()
	{
		var w = new SerializerBitWrite();
		var src = new List<Vector2Int> { new(10, -20), new(int.MaxValue, int.MinValue + 1) };
		w.writeList(src, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new List<Vector2Int>();
		assert(r.readList(dst, true), "readList Vector2Int success");
		assertEqual(src.Count, dst.Count, "list Vector2Int count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list Vector2Int[{i}]");
	}

	private static void testWriteReadListVector3()
	{
		var w = new SerializerBitWrite();
		var src = new List<Vector3> { new(1f, 2f, 3f), new(-1f, 0f, 0.01546f) };
		w.writeList(src, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new List<Vector3>();
		assert(r.readList(dst, true), "readList Vector3 success");
		assertEqual(src.Count, dst.Count, "list Vector3 count");
		for (int i = 0; i < src.Count; ++i)
		{
			assert(Math.Abs(src[i].x - dst[i].x) < 0.001f && 
				Math.Abs(src[i].y - dst[i].y) < 0.001f && 
				Math.Abs(src[i].z - dst[i].z) < 0.001f, $"list Vector3[{i}]: " + dst[i]);
		}
	}

	private static void testWriteReadListVector4()
	{
		var w = new SerializerBitWrite();
		var src = new List<Vector4> { new(1f, 2f, 3f, 4f), new(0, 0, 0, 0) };
		w.writeList(src, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new List<Vector4>();
		assert(r.readList(dst, true), "readList Vector4 success");
		assertEqual(src.Count, dst.Count, "list Vector4 count");
		for (int i = 0; i < src.Count; ++i)
		{
			assert(Math.Abs(src[i].x - dst[i].x) < 0.001f && Math.Abs(src[i].y - dst[i].y) < 0.001f && Math.Abs(src[i].z - dst[i].z) < 0.001f && Math.Abs(src[i].w - dst[i].w) < 0.001f, $"list Vector4[{i}]: " + dst[i]);
		}
	}

	private static void testWriteReadListString()
	{
		var w = new SerializerBitWrite();
		var src = new List<string> { "hello", "", "world", "测试" };
		w.writeList(src);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new List<string>();
		assert(r.readList(dst), "readList string success");
		assertEqual(src.Count, dst.Count, "list string count");
		for (int i = 0; i < src.Count; ++i)
			assertEqual(src[i], dst[i], $"list string[{i}]");
	}

	// ===================== Enum =====================

	private static void testReadEnumByte()
	{
		var w = new SerializerBitWrite();
		w.write((byte)SerializerByteTestEnum.Zero);
		w.write((byte)SerializerByteTestEnum.One);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.readEnumByte(out SerializerByteTestEnum v0), "readEnumByte[0]");
		assertEqual(SerializerByteTestEnum.Zero, v0, "enum Zero");
		assert(r.readEnumByte(out SerializerByteTestEnum v1), "readEnumByte[1]");
		assertEqual(SerializerByteTestEnum.One, v1, "enum One");
	}

	private static void testReadEnumInt()
	{
		var w = new SerializerBitWrite();
		w.write((int)SerializerByteTestEnum.Zero, true);
		w.write((int)SerializerByteTestEnum.Two, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.readEnumInt(out SerializerByteTestEnum v0, true), "readEnumInt[0]");
		assertEqual(SerializerByteTestEnum.Zero, v0, "enumInt Zero");
		assert(r.readEnumInt(out SerializerByteTestEnum v1, true), "readEnumInt[1]");
		assertEqual(SerializerByteTestEnum.Two, v1, "enumInt Two");
	}

	private static void testReadEnumList()
	{
		var w = new SerializerBitWrite();
		var src = new List<byte> { 0, 1, 2 };
		w.writeList(src);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var dst = new List<SerializerByteTestEnum>();
		assert(r.readEnumByteList(dst), "readEnumByteList success");
		assertEqual(3, dst.Count, "enum list count");
		assertEqual(SerializerByteTestEnum.Zero, dst[0], "enum list[0]");
		assertEqual(SerializerByteTestEnum.One, dst[1], "enum list[1]");
		assertEqual(SerializerByteTestEnum.Two, dst[2], "enum list[2]");
	}

	// ===================== 空列表 =====================

	private static void testEmptyList()
	{
		var w = new SerializerBitWrite();
		w.writeList(new List<int>(), false);
		w.writeList(new List<byte>());
		w.writeList(new List<Vector2>(), true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		var listInt = new List<int>();
		assert(r.readList(listInt, false), "readList empty int");
		assertEqual(0, listInt.Count, "empty int list count");

		var listByte = new List<byte>();
		assert(r.readList(listByte), "readList empty byte");
		assertEqual(0, listByte.Count, "empty byte list count");

		var listVec2 = new List<Vector2>();
		assert(r.readList(listVec2, true), "readList empty Vector2");
		assertEqual(0, listVec2.Count, "empty Vector2 list count");
	}

	// ===================== 工具方法 =====================

	private static void testGetBitCount()
	{
		var w = new SerializerBitWrite();
		assertEqual(0, w.getBitCount(), "getBitCount initially 0");
		w.write(true);
		// bool 写入 1 bit
		assertEqual(1, w.getBitCount(), "getBitCount after 1 bool bit");
		w.write(1, true);
		// int 写入后 bitCount 应大于1
		assert(w.getBitCount() > 1, "getBitCount after int");
	}

	private static void testGetBufferSize()
	{
		var w = new SerializerBitWrite();
		w.write(1, true);
		w.write(2, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assertEqual(w.getByteCount(), r.getBufferSize(), "getBufferSize matches write byte count");
	}

	private static void testGetBitIndex()
	{
		var w = new SerializerBitWrite();
		w.write(true);
		w.write(true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assertEqual(0, r.getBitIndex(), "getBitIndex initially 0");
		r.read(out bool _);
		assertEqual(1, r.getBitIndex(), "getBitIndex after 1 bool");
		r.read(out bool _);
		assertEqual(2, r.getBitIndex(), "getBitIndex after 2 bools");
	}

	private static void testGetReadByteCount()
	{
		var w = new SerializerBitWrite();
		w.write(0x12345678, true); // 写入 int

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assertEqual(0, r.getReadByteCount(), "getReadByteCount initially 0");
		r.read(out int v, true);
		assertEqual(0x12345678, v, "read int value correct");
		assert(r.getReadByteCount() > 0, "getReadByteCount after read > 0");
	}

	private static void testClear()
	{
		var w = new SerializerBitWrite();
		w.write(42, true);
		assert(w.getBitCount() > 0, "bitCount > 0 before clear");
		w.clear();
		assertEqual(0, w.getBitCount(), "bitCount == 0 after clear");
		// 清空后可继续写入
		w.write(99, true);
		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out int v, true), "read after clear");
		assertEqual(99, v, "value after clear");
	}

	// ===================== 边界值 =====================

	private static void testExtremeValues()
	{
		var w = new SerializerBitWrite();
		w.write(int.MinValue + 1, true);
		w.write(int.MaxValue, true);
		w.write(long.MinValue + 1, true);
		w.write(long.MaxValue, true);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out int iMin, true), "int min");
		assertEqual(int.MinValue + 1, iMin, "int.MinValue");
		assert(r.read(out int iMax, true), "int max");
		assertEqual(int.MaxValue, iMax, "int.MaxValue");
		assert(r.read(out long lMin, true), "long min");
		assertEqual(long.MinValue + 1, lMin, "long.MinValue");
		assert(r.read(out long lMax, true), "long max");
		assertEqual(long.MaxValue, lMax, "long.MaxValue");
	}

	private static void testMaxUnsignedValues()
	{
		var w = new SerializerBitWrite();
		w.write(byte.MaxValue);
		w.write(ushort.MaxValue);
		w.write(uint.MaxValue);
		w.write(ulong.MaxValue);

		var r = new SerializerBitRead();
		r.init(w.getBuffer(), w.getByteCount());
		assert(r.read(out byte bv), "byte max");
		assertEqual(byte.MaxValue, bv, "byte.MaxValue");
		assert(r.read(out ushort usv), "ushort max");
		assertEqual(ushort.MaxValue, usv, "ushort.MaxValue");
		assert(r.read(out uint uiv), "uint max");
		assertEqual(uint.MaxValue, uiv, "uint.MaxValue");
		assert(r.read(out ulong ulv), "ulong max");
		assertEqual(ulong.MaxValue, ulv, "ulong.MaxValue");
	}
}
#endif