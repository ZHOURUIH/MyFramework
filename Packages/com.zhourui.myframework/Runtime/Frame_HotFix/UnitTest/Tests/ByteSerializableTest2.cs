using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// Byte 序列化封装类单元测试(第二批) — 覆盖 Serialize/Byte 目录剩余封装类
// 补齐标量(SBYTE/SHORT/UINT/ULONG/USHORT)、集合(SBYTES/SHORTS/UINTS/ULONGS/USHORTS/FLOATS/LONGS/INTS)、
// 向量(VECTOR2_INT/VECTOR2_SHORT/VECTOR2_UINT/VECTOR2_USHORT/VECTOR3/VECTOR4)
// 与 ByteSerializableTest 对称, 纯数据封装(Serializable→ClassObject), 不依赖全局单例
public static class ByteSerializableTest2
{
	public static void Run()
	{
		testSBYTE();
		testSHORT();
		testUINT();
		testULONG();
		testUSHORT();

		testSBYTES_Collection();
		testSHORTS_Collection();
		testUINTS_Collection();
		testULONGS_Collection();
		testUSHORTS_Collection();
		testFLOATS_Collection();
		testLONGS_Collection();
		testINTS_Collection();
		testSTRINGS_Collection();

		testVECTOR2_INT();
		testVECTOR2_SHORT();
		testVECTOR2_UINT();
		testVECTOR2_USHORT();
		testVECTOR3();
		testVECTOR4();

		testScalarRoundTrips();
	}

	// ═════════════════════════════════════════════════════════════════
	// 标量封装: set/resetProperty/toString/隐式转换
	// ═════════════════════════════════════════════════════════════════
	private static void testSBYTE()
	{
		SBYTE instance = new SBYTE();
		assertEqual((sbyte)0, instance.mValue, "SBYTE 默认 0");
		instance.set(-128);
		assertEqual((sbyte)-128, instance.mValue, "SBYTE set 负值");
		instance.set(127);
		assertEqual((sbyte)127, instance.mValue, "SBYTE set 正极值");
		instance.resetProperty();
		assertEqual((sbyte)0, instance.mValue, "SBYTE reset 归 0");
		SBYTE conv = new SBYTE();
		conv.set(-5);
		assertEqual((sbyte)-5, (sbyte)conv, "SBYTE 隐式转换");
	}

	private static void testSHORT()
	{
		SHORT instance = new SHORT();
		assertEqual((short)0, instance.mValue, "SHORT 默认 0");
		instance.set(-32768);
		assertEqual((short)-32768, instance.mValue, "SHORT set MinValue");
		instance.set(32767);
		assertEqual((short)32767, instance.mValue, "SHORT set MaxValue");
		instance.resetProperty();
		assertEqual((short)0, instance.mValue, "SHORT reset 归 0");
	}

	private static void testUINT()
	{
		UINT instance = new UINT();
		assertEqual(0u, instance.mValue, "UINT 默认 0");
		instance.set(4294967295u);
		assertEqual(4294967295u, instance.mValue, "UINT set MaxValue");
		instance.resetProperty();
		assertEqual(0u, instance.mValue, "UINT reset 归 0");
	}

	private static void testULONG()
	{
		ULONG instance = new ULONG();
		assertEqual(0UL, instance.mValue, "ULONG 默认 0");
		instance.set(18446744073709551615UL);
		assertEqual(18446744073709551615UL, instance.mValue, "ULONG set MaxValue");
		instance.resetProperty();
		assertEqual(0UL, instance.mValue, "ULONG reset 归 0");
	}

	private static void testUSHORT()
	{
		USHORT instance = new USHORT();
		assertEqual((ushort)0, instance.mValue, "USHORT 默认 0");
		instance.set(65535);
		assertEqual((ushort)65535, instance.mValue, "USHORT set MaxValue");
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue, "USHORT reset 归 0");
	}

	// ═════════════════════════════════════════════════════════════════
	// 集合封装: add/addRange/Count/索引器/resetProperty
	// ═════════════════════════════════════════════════════════════════
	private static void testSBYTES_Collection()
	{
		SBYTES instance = new SBYTES();
		instance.add(-1);
		instance.add(2);
		assertEqual(2, instance.Count, "SBYTES add 后 Count");
		assertEqual((sbyte)-1, instance[0], "SBYTES 索引器读");
		instance[1] = 9;
		assertEqual((sbyte)9, instance[1], "SBYTES 索引器写");
		instance.addRange(new sbyte[] { 3, 4 });
		assertEqual(4, instance.Count, "SBYTES addRange 后 Count");
		instance.resetProperty();
		assertEqual(0, instance.Count, "SBYTES reset 清空");
	}

	private static void testSHORTS_Collection()
	{
		SHORTS instance = new SHORTS();
		instance.add((short)-100);
		instance.add((short)200);
		assertEqual(2, instance.Count, "SHORTS add 后 Count");
		assertEqual((short)-100, instance[0], "SHORTS 索引器读");
		instance.addRange(new short[] { 300, 400 });
		assertEqual(4, instance.Count, "SHORTS addRange 后 Count");
		instance.resetProperty();
		assertEqual(0, instance.Count, "SHORTS reset 清空");
	}

	private static void testUINTS_Collection()
	{
		UINTS instance = new UINTS();
		instance.add(1u);
		instance.add(4294967295u);
		assertEqual(2, instance.Count, "UINTS add 后 Count");
		assertEqual(4294967295u, instance[1], "UINTS 索引器读");
		instance.addRange(new uint[] { 5u, 6u });
		assertEqual(4, instance.Count, "UINTS addRange 后 Count");
		instance.resetProperty();
		assertEqual(0, instance.Count, "UINTS reset 清空");
	}

	private static void testULONGS_Collection()
	{
		ULONGS instance = new ULONGS();
		instance.add(123UL);
		instance.add(18446744073709551615UL);
		assertEqual(2, instance.Count, "ULONGS add 后 Count");
		assertEqual(18446744073709551615UL, instance[1], "ULONGS 索引器读");
		instance.resetProperty();
		assertEqual(0, instance.Count, "ULONGS reset 清空");
	}

	private static void testUSHORTS_Collection()
	{
		USHORTS instance = new USHORTS();
		instance.add(10);
		instance.add(65535);
		assertEqual(2, instance.Count, "USHORTS add 后 Count");
		assertEqual((ushort)65535, instance[1], "USHORTS 索引器读");
		instance.resetProperty();
		assertEqual(0, instance.Count, "USHORTS reset 清空");
	}

	private static void testFLOATS_Collection()
	{
		FLOATS instance = new FLOATS();
		instance.add(1.5f);
		instance.add(-2.5f);
		assertEqual(2, instance.Count, "FLOATS add 后 Count");
		assertEqual(1.5f, instance[0], 0.0001f, "FLOATS 索引器读");
		instance.resetProperty();
		assertEqual(0, instance.Count, "FLOATS reset 清空");
	}

	private static void testLONGS_Collection()
	{
		LONGS instance = new LONGS();
		instance.add(-9876543210L);
		instance.add(123456789L);
		assertEqual(2, instance.Count, "LONGS add 后 Count");
		assertEqual(-9876543210L, instance[0], "LONGS 索引器读");
		instance.resetProperty();
		assertEqual(0, instance.Count, "LONGS reset 清空");
	}

	private static void testINTS_Collection()
	{
		INTS instance = new INTS();
		instance.add(-100);
		instance.add(200);
		assertEqual(2, instance.Count, "INTS add 后 Count");
		assertEqual(-100, instance[0], "INTS 索引器读");
		instance[1] = 999;
		assertEqual(999, instance[1], "INTS 索引器写");
		instance.addRange(new int[] { 300, 400 });
		assertEqual(4, instance.Count, "INTS addRange 后 Count");
		instance.resetProperty();
		assertEqual(0, instance.Count, "INTS reset 清空");
	}

	private static void testSTRINGS_Collection()
	{
		STRINGS instance = new STRINGS();
		instance.add("hello");
		instance.add("world");
		assertEqual(2, instance.Count, "STRINGS add 后 Count");
		assertEqual("hello", instance[0], "STRINGS 索引器读");
		instance[1] = "changed";
		assertEqual("changed", instance[1], "STRINGS 索引器写");
		instance.addRange(new string[] { "a", "中文" });
		assertEqual(4, instance.Count, "STRINGS addRange 后 Count");
		// 隐式转换到 List<string>
		List<string> list = instance;
		assertEqual(4, list.Count, "STRINGS 隐式转换到 List<string>");
		assertEqual("中文", list[3], "STRINGS 隐式转换内容正确");
		// round-trip: 写入→读出 数据一致(含中文)
		STRINGS instance2 = new STRINGS();
		instance2.addRange(new string[] { "one", "中文", "three" });
		SerializerWrite writer = new SerializerWrite();
		instance2.write(writer);
		STRINGS read = new STRINGS();
		SerializerRead reader = new SerializerRead();
		reader.init(writer.getBuffer(), writer.getDataSize(), 0);
		read.read(reader);
		assertEqual(3, read.Count, "STRINGS round-trip Count 一致");
		assertEqual("中文", read[1], "STRINGS round-trip 中文内容一致");
		read.resetProperty();
		assertEqual(0, read.Count, "STRINGS reset 清空");
	}

	// ═════════════════════════════════════════════════════════════════
	// 向量封装: set/resetProperty/分量/round-trip
	// ═════════════════════════════════════════════════════════════════
	private static void testVECTOR2_INT()
	{
		VECTOR2_INT instance = new VECTOR2_INT();
		assertEqual(Vector2Int.zero, instance.mValue, "VECTOR2_INT 默认零向量");
		instance.set(new Vector2Int(3, -4));
		assertEqual(3, instance.x, "VECTOR2_INT x 分量");
		assertEqual(-4, instance.y, "VECTOR2_INT y 分量");
		instance.resetProperty();
		assertEqual(Vector2Int.zero, instance.mValue, "VECTOR2_INT reset 归零");
	}

	private static void testVECTOR2_SHORT()
	{
		VECTOR2_SHORT instance = new VECTOR2_SHORT();
		instance.set(new Vector2Short((short)1, (short)2));
		assertEqual((short)1, instance.x, "VECTOR2_SHORT x 分量");
		assertEqual((short)2, instance.y, "VECTOR2_SHORT y 分量");
		instance.resetProperty();
		assertEqual((short)0, instance.x, "VECTOR2_SHORT reset x 归 0");
	}

	private static void testVECTOR2_UINT()
	{
		VECTOR2_UINT instance = new VECTOR2_UINT();
		instance.set(new Vector2UInt(7u, 8u));
		assertEqual(7u, instance.x, "VECTOR2_UINT x 分量");
		assertEqual(8u, instance.y, "VECTOR2_UINT y 分量");
		instance.resetProperty();
		assertEqual(0u, instance.x, "VECTOR2_UINT reset x 归 0");
	}

	private static void testVECTOR2_USHORT()
	{
		VECTOR2_USHORT instance = new VECTOR2_USHORT();
		instance.set(new Vector2UShort((ushort)9, (ushort)10));
		assertEqual((ushort)9, instance.x, "VECTOR2_USHORT x 分量");
		assertEqual((ushort)10, instance.y, "VECTOR2_USHORT y 分量");
		instance.resetProperty();
		assertEqual((ushort)0, instance.x, "VECTOR2_USHORT reset x 归 0");
	}

	private static void testVECTOR3()
	{
		VECTOR3 instance = new VECTOR3();
		assertEqual(Vector3.zero, instance.mValue, "VECTOR3 默认零向量");
		instance.set(new Vector3(1, 2, 3));
		assertEqual(1.0f, instance.x, "VECTOR3 x 分量");
		assertEqual(2.0f, instance.y, "VECTOR3 y 分量");
		assertEqual(3.0f, instance.z, "VECTOR3 z 分量");
		instance.resetProperty();
		assertEqual(Vector3.zero, instance.mValue, "VECTOR3 reset 归零");
	}

	private static void testVECTOR4()
	{
		VECTOR4 instance = new VECTOR4();
		assertEqual(Vector4.zero, instance.mValue, "VECTOR4 默认零向量");
		instance.set(new Vector4(1, 2, 3, 4));
		assertEqual(1.0f, instance.x, "VECTOR4 x 分量");
		assertEqual(4.0f, instance.w, "VECTOR4 w 分量");
		instance.resetProperty();
		assertEqual(Vector4.zero, instance.mValue, "VECTOR4 reset 归零");
	}

	// ═════════════════════════════════════════════════════════════════
	// 标量 round-trip 汇总
	// ═════════════════════════════════════════════════════════════════
	private static void testScalarRoundTrips()
	{
		// SBYTE
		SBYTE sbyteSrc = new SBYTE();
		sbyteSrc.set(-100);
		var w1 = new SerializerWrite();
		sbyteSrc.write(w1);
		var r1 = new SerializerRead();
		r1.init(w1.getBuffer(), w1.getDataSize(), 0);
		SBYTE sbyteDst = new SBYTE();
		assertTrue(sbyteDst.read(r1), "SBYTE read 成功");
		assertEqual((sbyte)-100, sbyteDst.mValue, "SBYTE round-trip");

		// SHORT
		SHORT shortSrc = new SHORT();
		shortSrc.set(-12345);
		var w2 = new SerializerWrite();
		shortSrc.write(w2);
		var r2 = new SerializerRead();
		r2.init(w2.getBuffer(), w2.getDataSize(), 0);
		SHORT shortDst = new SHORT();
		assertTrue(shortDst.read(r2), "SHORT read 成功");
		assertEqual((short)-12345, shortDst.mValue, "SHORT round-trip");

		// UINT
		UINT uintSrc = new UINT();
		uintSrc.set(4000000000u);
		var w3 = new SerializerWrite();
		uintSrc.write(w3);
		var r3 = new SerializerRead();
		r3.init(w3.getBuffer(), w3.getDataSize(), 0);
		UINT uintDst = new UINT();
		assertTrue(uintDst.read(r3), "UINT read 成功");
		assertEqual(4000000000u, uintDst.mValue, "UINT round-trip");

		// ULONG
		ULONG ulongSrc = new ULONG();
		ulongSrc.set(123456789012345UL);
		var w4 = new SerializerWrite();
		ulongSrc.write(w4);
		var r4 = new SerializerRead();
		r4.init(w4.getBuffer(), w4.getDataSize(), 0);
		ULONG ulongDst = new ULONG();
		assertTrue(ulongDst.read(r4), "ULONG read 成功");
		assertEqual(123456789012345UL, ulongDst.mValue, "ULONG round-trip");

		// USHORT
		USHORT ushortSrc = new USHORT();
		ushortSrc.set(54321);
		var w5 = new SerializerWrite();
		ushortSrc.write(w5);
		var r5 = new SerializerRead();
		r5.init(w5.getBuffer(), w5.getDataSize(), 0);
		USHORT ushortDst = new USHORT();
		assertTrue(ushortDst.read(r5), "USHORT read 成功");
		assertEqual((ushort)54321, ushortDst.mValue, "USHORT round-trip");

		// INTS 集合 round-trip
		INTS intsSrc = new INTS();
		intsSrc.addRange(new int[] { 1, -2, 3 });
		var w6 = new SerializerWrite();
		intsSrc.write(w6);
		var r6 = new SerializerRead();
		r6.init(w6.getBuffer(), w6.getDataSize(), 0);
		INTS intsDst = new INTS();
		assertTrue(intsDst.read(r6), "INTS read 成功");
		assertEqual(3, intsDst.Count, "INTS round-trip Count");
		assertEqual(1, intsDst[0], "INTS round-trip 第0");
		assertEqual(-2, intsDst[1], "INTS round-trip 第1");
		assertEqual(3, intsDst[2], "INTS round-trip 第2");

		// VECTOR3 round-trip
		VECTOR3 vecSrc = new VECTOR3();
		vecSrc.set(new Vector3(1.5f, -2.5f, 3.5f));
		var w7 = new SerializerWrite();
		vecSrc.write(w7);
		var r7 = new SerializerRead();
		r7.init(w7.getBuffer(), w7.getDataSize(), 0);
		VECTOR3 vecDst = new VECTOR3();
		assertTrue(vecDst.read(r7), "VECTOR3 read 成功");
		assertEqual(1.5f, vecDst.mValue.x, 0.0001f, "VECTOR3 round-trip x");
		assertEqual(3.5f, vecDst.mValue.z, 0.0001f, "VECTOR3 round-trip z");
	}
}
