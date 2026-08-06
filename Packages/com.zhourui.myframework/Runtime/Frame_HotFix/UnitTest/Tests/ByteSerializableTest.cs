using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// Byte 序列化封装类单元测试 — 覆盖 Serialize/Byte 目录下标量/集合封装类
// 对标已有的 BIT_XXX 系列(Bit封装), 补齐 Byte 封装(INT/BYTE/LONG/FLOAT/BOOL/STRING/BYTES/VECTOR2)的空白
// 覆盖: 构造默认值(mValid=true)、set/resetProperty、隐式转换、toString、read/write round-trip
// 均为纯数据封装(Serializable→ClassObject), 不依赖全局单例, 构造干净
public static class ByteSerializableTest
{
	public static void Run()
	{
		testINT_Basic();
		testINT_RoundTrip();
		testBYTE_Basic();
		testBYTE_RoundTrip();
		testLONG_Basic();
		testLONG_RoundTrip();
		testFLOAT_Basic();
		testFLOAT_RoundTrip();
		testBOOL_Basic();
		testBOOL_RoundTrip();
		testSTRING_Basic();
		testSTRING_RoundTrip();
		testBYTES_Collection();
		testBYTES_RoundTrip();
		testVECTOR2_Basic();
		testVECTOR2_RoundTrip();
		testSerializable_BaseValid();
		testImplicitConversions();
	}

	// ═════════════════════════════════════════════════════════════════
	// INT 封装
	// ═════════════════════════════════════════════════════════════════
	private static void testINT_Basic()
	{
		INT instance = new INT();
		assertEqual(0, instance.mValue, "INT 默认值为 0");
		instance.set(42);
		assertEqual(42, instance.mValue, "set 后 mValue 正确");
		instance.set(-7);
		assertEqual(-7, instance.mValue, "set 负值正确");
		instance.set(int.MaxValue);
		assertEqual(int.MaxValue, instance.mValue, "set MaxValue 正确");
		instance.resetProperty();
		assertEqual(0, instance.mValue, "resetProperty 归 0");
		INT strInstance = new INT();
		strInstance.set(123);
		assertEqual("123", strInstance.toString(), "toString 返回数值字符串");
	}

	private static void testINT_RoundTrip()
	{
		INT src = new INT();
		src.set(-123456);
		var writer = new SerializerWrite();
		src.write(writer);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		INT dst = new INT();
		bool ok = dst.read(reader);
		assertTrue(ok, "read 应返回 true");
		assertEqual(-123456, dst.mValue, "INT round-trip 值一致");
	}

	// ═════════════════════════════════════════════════════════════════
	// BYTE 封装
	// ═════════════════════════════════════════════════════════════════
	private static void testBYTE_Basic()
	{
		BYTE instance = new BYTE();
		assertEqual((byte)0, instance.mValue, "BYTE 默认值为 0");
		instance.set(0xFF);
		assertEqual((byte)0xFF, instance.mValue, "set 后 mValue 正确");
		instance.resetProperty();
		assertEqual((byte)0, instance.mValue, "resetProperty 归 0");
		BYTE strInstance = new BYTE();
		strInstance.set(255);
		assertEqual("255", strInstance.toString(), "BYTE toString 正确");
	}

	private static void testBYTE_RoundTrip()
	{
		BYTE src = new BYTE();
		src.set(0xAB);
		var writer = new SerializerWrite();
		src.write(writer);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		BYTE dst = new BYTE();
		assertTrue(dst.read(reader), "BYTE read 成功");
		assertEqual((byte)0xAB, dst.mValue, "BYTE round-trip 值一致");
	}

	// ═════════════════════════════════════════════════════════════════
	// LONG 封装
	// ═════════════════════════════════════════════════════════════════
	private static void testLONG_Basic()
	{
		LONG instance = new LONG();
		assertEqual(0L, instance.mValue, "LONG 默认值为 0");
		instance.set(987654321012345678L);
		assertEqual(987654321012345678L, instance.mValue, "set 大数正确");
		instance.resetProperty();
		assertEqual(0L, instance.mValue, "resetProperty 归 0");
	}

	private static void testLONG_RoundTrip()
	{
		LONG src = new LONG();
		src.set(-123456789012345L);
		var writer = new SerializerWrite();
		src.write(writer);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		LONG dst = new LONG();
		assertTrue(dst.read(reader), "LONG read 成功");
		assertEqual(-123456789012345L, dst.mValue, "LONG round-trip 值一致");
	}

	// ═════════════════════════════════════════════════════════════════
	// FLOAT 封装
	// ═════════════════════════════════════════════════════════════════
	private static void testFLOAT_Basic()
	{
		FLOAT instance = new FLOAT();
		assertEqual(0.0f, instance.mValue, "FLOAT 默认值为 0");
		instance.set(3.14f);
		assertEqual(3.14f, instance.mValue, "set 后 mValue 正确");
		instance.resetProperty();
		assertEqual(0.0f, instance.mValue, "resetProperty 归 0");
	}

	private static void testFLOAT_RoundTrip()
	{
		FLOAT src = new FLOAT();
		src.set(3.14159f);
		var writer = new SerializerWrite();
		src.write(writer);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		FLOAT dst = new FLOAT();
		assertTrue(dst.read(reader), "FLOAT read 成功");
		assertEqual(3.14159f, dst.mValue, 0.0001f, "FLOAT round-trip 值一致");
	}

	// ═════════════════════════════════════════════════════════════════
	// BOOL 封装
	// ═════════════════════════════════════════════════════════════════
	private static void testBOOL_Basic()
	{
		BOOL instance = new BOOL();
		assertFalse(instance.mValue, "BOOL 默认值为 false");
		instance.set(true);
		assertTrue(instance.mValue, "set(true) 后 mValue 正确");
		instance.set(false);
		assertFalse(instance.mValue, "set(false) 后 mValue 正确");
		instance.resetProperty();
		assertFalse(instance.mValue, "resetProperty 归 false");
	}

	private static void testBOOL_RoundTrip()
	{
		BOOL src = new BOOL();
		src.set(true);
		var writer = new SerializerWrite();
		src.write(writer);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		BOOL dst = new BOOL();
		assertTrue(dst.read(reader), "BOOL read 成功");
		assertTrue(dst.mValue, "BOOL round-trip 值一致");
	}

	// ═════════════════════════════════════════════════════════════════
	// STRING 封装
	// ═════════════════════════════════════════════════════════════════
	private static void testSTRING_Basic()
	{
		STRING instance = new STRING();
		assertNull(instance.mValue, "STRING 构造后 mValue 默认为 null");
		instance.set("hello");
		assertEqual("hello", instance.mValue, "set 后 mValue 正确");
		instance.resetProperty();
		assertEqual(string.Empty, instance.mValue, "resetProperty 归空串");
		STRING strInstance = new STRING();
		strInstance.set("abc");
		assertEqual("abc", strInstance.toString(), "STRING toString 返回原串");
	}

	private static void testSTRING_RoundTrip()
	{
		STRING src = new STRING();
		src.set("测试字符串123");
		var writer = new SerializerWrite();
		src.write(writer);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		STRING dst = new STRING();
		assertTrue(dst.read(reader), "STRING read 成功");
		assertEqual("测试字符串123", dst.mValue, "STRING round-trip 值一致(含中文)");
	}

	// ═════════════════════════════════════════════════════════════════
	// BYTES 集合封装
	// ═════════════════════════════════════════════════════════════════
	private static void testBYTES_Collection()
	{
		BYTES instance = new BYTES();
		assertEqual(0, instance.Count, "BYTES 初始 Count 为 0");
		instance.add(1);
		instance.add(2);
		instance.add(3);
		assertEqual(3, instance.Count, "add 后 Count 正确");
		assertEqual((byte)1, instance[0], "索引器读取正确");
		instance[1] = 9;
		assertEqual((byte)9, instance[1], "索引器写入正确");

		instance.addRange(new byte[] { 4, 5 });
		assertEqual(5, instance.Count, "addRange 后 Count 正确");

		instance.resetProperty();
		assertEqual(0, instance.Count, "resetProperty 清空集合");
	}

	private static void testBYTES_RoundTrip()
	{
		BYTES src = new BYTES();
		src.addRange(new byte[] { 10, 20, 30 });
		var writer = new SerializerWrite();
		src.write(writer);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		BYTES dst = new BYTES();
		assertTrue(dst.read(reader), "BYTES read 成功");
		assertEqual(3, dst.Count, "BYTES round-trip Count 一致");
		assertEqual((byte)10, dst[0], "BYTES round-trip 第0字节");
		assertEqual((byte)20, dst[1], "BYTES round-trip 第1字节");
		assertEqual((byte)30, dst[2], "BYTES round-trip 第2字节");
	}

	// ═════════════════════════════════════════════════════════════════
	// VECTOR2 封装
	// ═════════════════════════════════════════════════════════════════
	private static void testVECTOR2_Basic()
	{
		VECTOR2 instance = new VECTOR2();
		assertEqual(Vector2.zero, instance.mValue, "VECTOR2 默认为零向量");
		instance.set(new Vector2(3, 4));
		assertEqual(3.0f, instance.x, "x 分量正确");
		assertEqual(4.0f, instance.y, "y 分量正确");
		instance.resetProperty();
		assertEqual(Vector2.zero, instance.mValue, "resetProperty 归零");
	}

	private static void testVECTOR2_RoundTrip()
	{
		VECTOR2 src = new VECTOR2();
		src.set(new Vector2(1.5f, -2.5f));
		var writer = new SerializerWrite();
		src.write(writer);
		var reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		VECTOR2 dst = new VECTOR2();
		assertTrue(dst.read(reader), "VECTOR2 read 成功");
		assertEqual(1.5f, dst.mValue.x, 0.0001f, "VECTOR2 round-trip x");
		assertEqual(-2.5f, dst.mValue.y, 0.0001f, "VECTOR2 round-trip y");
	}

	// ═════════════════════════════════════════════════════════════════
	// Serializable 基类: 构造默认 mValid=true, resetProperty 保持
	// ═════════════════════════════════════════════════════════════════
	private static void testSerializable_BaseValid()
	{
		INT instance = new INT();
		assertTrue(instance.mValid, "Serializable 构造默认 mValid=true");
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid, "resetProperty 恢复 mValid=true");
		assertFalse(instance.mOptional, "mOptional 默认 false");
	}

	// ═════════════════════════════════════════════════════════════════
	// 隐式转换到基础类型
	// ═════════════════════════════════════════════════════════════════
	private static void testImplicitConversions()
	{
		INT i = new INT();
		i.set(100);
		int intVal = i;
		assertEqual(100, intVal, "INT 隐式转 int");

		LONG l = new LONG();
		l.set(500L);
		long longVal = l;
		assertEqual(500L, longVal, "LONG 隐式转 long");

		BOOL b = new BOOL();
		b.set(true);
		bool boolVal = b;
		assertTrue(boolVal, "BOOL 隐式转 bool");

		FLOAT f = new FLOAT();
		f.set(2.5f);
		float floatVal = f;
		assertEqual(2.5f, floatVal, 0.0001f, "FLOAT 隐式转 float");

		STRING s = new STRING();
		s.set("conv");
		string strVal = s;
		assertEqual("conv", strVal, "STRING 隐式转 string");
	}
}
