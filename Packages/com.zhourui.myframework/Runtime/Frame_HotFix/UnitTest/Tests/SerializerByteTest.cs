using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// SerializerRead / SerializerWrite(Byte 版本) 单元测试
// 采用"写入→读出 round-trip"方式验证数据一致性,覆盖基本类型/大小端/Vector/字符串/列表/缓冲区
public static class SerializerByteTest
{
	public static void Run()
	{
		testPrimitiveRoundTrip();
		testBigEndianRoundTrip();
		testVectorRoundTrip();
		testStringRoundTrip();
		testListRoundTrip();
		testBufferWriteRead();
		testIndexControl();
		testClearAndReset();
		testReadEnumByte();
		testReadEnumInt();
		testReadEnumLong();
		testReadEnumByteList();
		testMultiValueSequence();
		testStringUnicode();
		testReadPastEnd();
		testEmptyListRoundTrip();
		testReinitReuse();
		testNestedBuffer();
	}

	// ─── 基本类型 round-trip ──────────────────────────────────────────
	private static void testPrimitiveRoundTrip()
	{
		var writer = new SerializerWrite();
		writer.write(true);
		writer.write((byte)0xAB);
		writer.write((sbyte)-5);
		writer.write((short)-1234);
		writer.write((ushort)65535);
		writer.write(-123456);
		writer.write((uint)4000000000u);
		writer.write(-9876543210123456L);
		writer.write(18446744073709551615UL);
		writer.write(3.14159f);
		writer.write(-2.718281828459045);

		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		bool readBool; byte readByte; sbyte readSByte; short readShort; ushort readUShort;
		int readInt; uint readUInt; long readLong; ulong readULong; float readFloat; double readDouble;
		reader.read(out readBool);
		reader.read(out readByte);
		reader.read(out readSByte);
		reader.read(out readShort);
		reader.read(out readUShort);
		reader.read(out readInt);
		reader.read(out readUInt);
		reader.read(out readLong);
		reader.read(out readULong);
		reader.read(out readFloat);
		reader.read(out readDouble);

		assertEqual(true, readBool, "bool round-trip");
		assertEqual((byte)0xAB, readByte, "byte round-trip");
		assertEqual((sbyte)-5, readSByte, "sbyte round-trip");
		assertEqual((short)-1234, readShort, "short round-trip");
		assertEqual((ushort)65535, readUShort, "ushort round-trip");
		assertEqual(-123456, readInt, "int round-trip");
		assertEqual(4000000000u, readUInt, "uint round-trip");
		assertEqual(-9876543210123456L, readLong, "long round-trip");
		assertEqual(18446744073709551615UL, readULong, "ulong round-trip");
		assertEqual(3.14159f, readFloat, 0.00001f, "float round-trip");
		// round-trip 无损, double 用精确比较
		assertEqual(readDouble, -2.718281828459045, "double round-trip");
	}

	// ─── 大小端 round-trip ────────────────────────────────────────────
	private static void testBigEndianRoundTrip()
	{
		var writer = new SerializerWrite();
		writer.writeBigEndian((short)0x1234);
		writer.writeBigEndian((ushort)0xABCD);
		writer.writeBigEndian(0x12345678);
		writer.writeBigEndian(0x9ABCDEF0u);
		writer.writeBigEndian(0x123456789ABCDEF0L);
		writer.writeBigEndian(0x1122334455667788UL);
		writer.writeBigEndian(1.5f);
		writer.writeBigEndian(2.25);

		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		short readShort; ushort readUShort; int readInt; uint readUInt; long readLong; ulong readULong;
		float readFloat; double readDouble;
		reader.readBigEndian(out readShort);
		reader.readBigEndian(out readUShort);
		reader.readBigEndian(out readInt);
		reader.readBigEndian(out readUInt);
		reader.readBigEndian(out readLong);
		reader.readBigEndian(out readULong);
		reader.readBigEndian(out readFloat);
		reader.readBigEndian(out readDouble);

		assertEqual((short)0x1234, readShort, "big endian short");
		assertEqual((ushort)0xABCD, readUShort, "big endian ushort");
		assertEqual(0x12345678, readInt, "big endian int");
		assertEqual(0x9ABCDEF0u, readUInt, "big endian uint");
		assertEqual(0x123456789ABCDEF0L, readLong, "big endian long");
		assertEqual(0x1122334455667788UL, readULong, "big endian ulong");
		assertEqual(1.5f, readFloat, 0.00001f, "big endian float");
		assertEqual(readDouble, 2.25, "big endian double");
	}

	// ─── Vector round-trip ────────────────────────────────────────────
	private static void testVectorRoundTrip()
	{
		var writer = new SerializerWrite();
		writer.write(new Vector2(1.5f, -2.5f));
		writer.write(new Vector2Int(3, -4));
		writer.write(new Vector3(5.5f, -6.5f, 7.5f));
		writer.write(new Vector4(1f, 2f, 3f, 4f));

		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		Vector2 v2; Vector2Int v2i; Vector3 v3; Vector4 v4;
		reader.read(out v2);
		reader.read(out v2i);
		reader.read(out v3);
		reader.read(out v4);

		assertEqual(new Vector2(1.5f, -2.5f), v2, "Vector2 round-trip");
		assertEqual(new Vector2Int(3, -4), v2i, "Vector2Int round-trip");
		assertEqual(new Vector3(5.5f, -6.5f, 7.5f), v3, "Vector3 round-trip");
		assertEqual(new Vector4(1f, 2f, 3f, 4f), v4, "Vector4 round-trip");
	}

	// ─── 字符串 round-trip ────────────────────────────────────────────
	private static void testStringRoundTrip()
	{
		var writer = new SerializerWrite();
		writer.writeString("hello");
		writer.writeString(null);
		writer.writeString("中文测试");

		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		string s1; string s2; string s3;
		reader.readString(out s1);
		reader.readString(out s2);
		reader.readString(out s3);

		assertEqual("hello", s1, "字符串 round-trip");
		assertEqual("", s2, "null/空字符串 round-trip(读回为 EMPTY 空串)");
		assertEqual("中文测试", s3, "中文字符串 round-trip");
	}

	// ─── List round-trip ──────────────────────────────────────────────
	private static void testListRoundTrip()
	{
		var writer = new SerializerWrite();
		var srcList = new List<int> { 1, 2, 3, -4, 100 };
		writer.writeList(srcList);

		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		var outList = new List<int>();
		bool success = reader.readList(outList);
		assertTrue(success, "readList 返回成功");
		assertEqual(srcList.Count, outList.Count, "List 长度一致");
		for (int i = 0; i < srcList.Count; ++i)
		{
			assertEqual(srcList[i], outList[i], "List 元素一致, index=" + i);
		}
	}

	// ─── 缓冲区写入读取 ───────────────────────────────────────────────
	private static void testBufferWriteRead()
	{
		var writer = new SerializerWrite();
		byte[] src = { 10, 20, 30, 40, 50 };
		writer.writeBuffer(src, src.Length);
		writer.write((byte)0x7F);

		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		byte[] outBuf = new byte[src.Length];
		bool success = reader.readBuffer(outBuf, src.Length);
		assertTrue(success, "readBuffer 返回成功");
		for (int i = 0; i < src.Length; ++i)
		{
			assertEqual(src[i], outBuf[i], "缓冲区元素一致, index=" + i);
		}
		byte tail;
		reader.read(out tail);
		assertEqual((byte)0x7F, tail, "缓冲区后追加的字节正确");
	}

	// ─── 下标控制 ─────────────────────────────────────────────────────
	private static void testIndexControl()
	{
		var writer = new SerializerWrite();
		writer.write((byte)1);
		writer.write((byte)2);
		writer.write((byte)3);
		writer.write((byte)4);
		assertEqual(4, writer.getDataSize(), "写入 4 字节后 dataSize 为 4");

		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		byte b;
		reader.read(out b);
		assertEqual((byte)1, b, "第一个字节正确");
		reader.skipIndex(1);
		reader.read(out b);
		assertEqual((byte)3, b, "skip 1 字节后读到第 3 个字节");
		reader.setIndex(1);
		reader.read(out b);
		assertEqual((byte)2, b, "setIndex(1) 后读到第 2 个字节");
		assertEqual(2, reader.getIndex(), "读后 index 为 2");
	}

	// ─── clear 与 resetProperty ───────────────────────────────────────
	private static void testClearAndReset()
	{
		var writer = new SerializerWrite();
		writer.write((byte)1);
		writer.write((byte)2);
		assertEqual(2, writer.getDataSize(), "clear 前 dataSize 为 2");
		writer.clear();
		assertEqual(0, writer.getDataSize(), "clear 后 dataSize 归 0");
		writer.write((byte)9);
		assertEqual(1, writer.getDataSize(), "clear 后可重新写入");

		var reader = new SerializerRead();
		reader.init(new byte[] { 1, 2, 3 }, 3, 0);
		assertEqual(3, reader.getDataSize(), "init 后 dataSize 为 3");
		byte b;
		reader.read(out b);
		assertEqual((byte)1, b, "init 后正常读取");
		reader.resetProperty();
		assertEqual(0, reader.getDataSize(), "resetProperty 后 dataSize 归 0");
	}

	// ═════════════════════════════════════════════════════════════════
	// readEnumByte — write(byte) 对称回环
	// ═════════════════════════════════════════════════════════════════
	private static void testReadEnumByte()
	{
		var writer = new SerializerWrite();
		writer.write((byte)5);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		bool ok = reader.readEnumByte<TestByteEnum>(out TestByteEnum value);
		assertTrue(ok, "readEnumByte 应返回 true");
		assertEqual(TestByteEnum.Five, value, "readEnumByte 读回写入的枚举值 5");
	}

	// ═════════════════════════════════════════════════════════════════
	// readEnumInt — write(int) 对称回环
	// ═════════════════════════════════════════════════════════════════
	private static void testReadEnumInt()
	{
		var writer = new SerializerWrite();
		writer.write(12345);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		bool ok = reader.readEnumInt<TestByteEnumInt>(out TestByteEnumInt value);
		assertTrue(ok, "readEnumInt 应返回 true");
		assertEqual(TestByteEnumInt.Val12345, value, "readEnumInt 读回写入的枚举值 12345");
	}

	// ═════════════════════════════════════════════════════════════════
	// readEnumLong — write(long) 对称回环
	// ═════════════════════════════════════════════════════════════════
	private static void testReadEnumLong()
	{
		var writer = new SerializerWrite();
		writer.write(9876543210L);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		bool ok = reader.readEnumLong<TestByteEnumLong>(out TestByteEnumLong value);
		assertTrue(ok, "readEnumLong 应返回 true");
		assertEqual(TestByteEnumLong.Big, value, "readEnumLong 读回写入的枚举值 9876543210");
	}

	// ═════════════════════════════════════════════════════════════════
	// readEnumByteList — writeList(List<byte>) 对称回环
	// ═════════════════════════════════════════════════════════════════
	private static void testReadEnumByteList()
	{
		var writer = new SerializerWrite();
		writer.writeList(new List<byte> { 1, 5, 0 });
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		List<TestByteEnum> list = new List<TestByteEnum>();
		bool ok = reader.readEnumByteList(list);
		assertTrue(ok, "readEnumByteList 应返回 true");
		assertEqual(3, list.Count, "读回 3 个枚举");
		assertEqual(TestByteEnum.One, list[0], "第 0 个枚举为 One");
		assertEqual(TestByteEnum.Five, list[1], "第 1 个枚举为 Five");
		assertEqual(TestByteEnum.Zero, list[2], "第 2 个枚举为 Zero");
	}

	// ─── 组合场景 ────────────────────────────────────────────────────

	// 混合类型顺序写读: int→bool→string→float 链
	private static void testMultiValueSequence()
	{
		var writer = new SerializerWrite();
		writer.write(12345);
		writer.write(true);
		writer.writeString("hello");
		writer.write(2.5f);

		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		int i; bool b; string s; float f;
		reader.read(out i);
		reader.read(out b);
		reader.readString(out s);
		reader.read(out f);
		assertEqual(12345, i, "int 读回");
		assertTrue(b, "bool 读回");
		assertEqual("hello", s, "string 读回");
		assertEqual(2.5f, f, 0.0001f, "float 读回");
	}

	// 特殊字符串: 中文/空/长字符串往返
	private static void testStringUnicode()
	{
		var writer = new SerializerWrite();
		writer.writeString("中文测试");
		writer.writeString("");
		writer.writeString(new string('a', 1000));

		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		string s1; string s2; string s3;
		reader.readString(out s1);
		reader.readString(out s2);
		reader.readString(out s3);
		assertEqual("中文测试", s1, "中文往返");
		assertEqual("", s2, "空字符串往返");
		assertEqual(1000, s3.Length, "长字符串长度往返");
	}

	// 越界读取返回 false(安全)
	private static void testReadPastEnd()
	{
		var writer = new SerializerWrite();
		writer.write((byte)1);   // 1 字节
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		int value;
		// 读 int(4 字节) 超出 1 字节 → 返回 false
		bool ok = reader.read(out value);
		assertFalse(ok, "越界读 int 返回 false");
	}

	// 空列表往返
	private static void testEmptyListRoundTrip()
	{
		var writer = new SerializerWrite();
		writer.writeList(new List<int>());
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		List<int> list = new List<int>();
		bool ok = reader.readList(list);
		assertTrue(ok, "空列表读回成功");
		assertEqual(0, list.Count, "空列表读回 0 个");
	}

	// 同一 reader 重新 init 复用
	private static void testReinitReuse()
	{
		var writer1 = new SerializerWrite();
		writer1.write(111);
		var writer2 = new SerializerWrite();
		writer2.write(222);

		var reader = new SerializerRead();
		// 第一次读
		reader.init(writer1.getBuffer(), writer1.getDataSize(), 0);
		int v1;
		reader.read(out v1);
		assertEqual(111, v1, "第一次读回 111");
		// 重新 init 到第二个缓冲区
		reader.init(writer2.getBuffer(), writer2.getDataSize(), 0);
		int v2;
		reader.read(out v2);
		assertEqual(222, v2, "重新 init 后读回 222");
	}

	// writeBuffer 嵌套后继续写
	private static void testNestedBuffer()
	{
		var writer = new SerializerWrite();
		byte[] inner = { 7, 8, 9 };
		writer.write((byte)1);
		writer.writeBuffer(inner, inner.Length);
		writer.write((byte)2);

		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		byte b1; byte b2;
		byte[] outBuf = new byte[inner.Length];
		reader.read(out b1);
		reader.readBuffer(outBuf, inner.Length);
		reader.read(out b2);
		assertEqual((byte)1, b1, "嵌套前字节");
		assertEqual((byte)2, b2, "嵌套后字节");
		for (int i = 0; i < inner.Length; ++i)
		{
			assertEqual(inner[i], outBuf[i], "嵌套缓冲第 " + i + " 字节");
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// readEnum 测试用枚举(不同底层类型)
// ═════════════════════════════════════════════════════════════════
public enum TestByteEnum : byte
{
	Zero = 0,
	One = 1,
	Five = 5,
}
public enum TestByteEnumInt : int
{
	Val12345 = 12345,
}
public enum TestByteEnumLong : long
{
	Big = 9876543210L,
}
