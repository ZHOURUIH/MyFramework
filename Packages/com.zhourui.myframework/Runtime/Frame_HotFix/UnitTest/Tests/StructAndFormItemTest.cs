using System.IO;
using UnityEngine;
using static TestAssert;

// 补充覆盖 Utility/Struct 中的简单数据结构与表单写入逻辑
public static class StructAndFormItemTest
{
	public static void Run()
	{
		testVector4IntEqualsAndZero();
		testTriangle2ToTriangle3();
		testIntersectResultStructsCanStoreValues();
		testFormItemParamWriteAndReset();
		testFormItemFileWriteAndReset();
		testPointBasic();
		testPointToIndex();
		testPointFromIndex();
		testPointEquals();
		testPointGetHashCode();
		testVector2ShortBasic();
		testVector2ShortEquals();
		testVector2ShortGetHashCode();
		testVector2ShortToVec2();
		testVector2ShortToVec2Int();
		testVector2UIntBasic();
		testVector2UIntEquals();
		testVector2UIntGetHashCode();
		testVector2UIntToVec2();
		testVector2UIntToVec2Int();
		testVector2UShortBasic();
		testVector2UShortEquals();
		testVector2UShortGetHashCode();
		testVector2UShortToVec2();
		testVector2UShortToVec2Int();
	}

	private static void testVector4IntEqualsAndZero()
	{
		Vector4Int value = new(1, 2, 3, 4);
		assertTrue(value.Equals(new Vector4Int(1, 2, 3, 4)), "相同分量应相等");
		assertFalse(value.Equals(new Vector4Int(1, 2, 3, 5)), "不同分量不应相等");
		assertTrue(Vector4Int.zero.Equals(new Vector4Int(0, 0, 0, 0)), "zero 应为默认零值");
	}

	private static void testTriangle2ToTriangle3()
	{
		Triangle2 triangle = new(new Vector2(1, 2), new Vector2(3, 4), new Vector2(5, 6));
		Triangle3 converted = triangle.toTriangle3();
		assertEqual(new Vector3(1, 2, 0), converted.mPoint0, "点0转换错误");
		assertEqual(new Vector3(3, 4, 0), converted.mPoint1, "点1转换错误");
		assertEqual(new Vector3(5, 6, 0), converted.mPoint2, "点2转换错误");
	}

	private static void testIntersectResultStructsCanStoreValues()
	{
		TriangleIntersectResult result2 = new()
		{
			mIntersectPoint = new Vector2(1, 2),
			mLinePoint0 = new Vector2(3, 4),
			mLinePoint1 = new Vector2(5, 6),
		};
		assertEqual(new Vector2(1, 2), result2.mIntersectPoint);
		assertEqual(new Vector2(3, 4), result2.mLinePoint0);
		assertEqual(new Vector2(5, 6), result2.mLinePoint1);

		TriangleIntersectResult3 result3 = new()
		{
			mIntersectPoint = new Vector3(1, 2, 3),
			mLinePoint0 = new Vector3(4, 5, 6),
			mLinePoint1 = new Vector3(7, 8, 9),
		};
		assertEqual(new Vector3(1, 2, 3), result3.mIntersectPoint);
		assertEqual(new Vector3(4, 5, 6), result3.mLinePoint0);
		assertEqual(new Vector3(7, 8, 9), result3.mLinePoint1);

		PolygonIntersectResult polygonResult = new()
		{
			mIntersectPoint0 = new Vector2(1, 1),
			mIntersectPoint1 = new Vector2(2, 2),
			mLine0 = new Line2(Vector2.zero, Vector2.one),
			mLine1 = new Line2(Vector2.right, Vector2.up),
		};
		assertEqual(new Vector2(1, 1), polygonResult.mIntersectPoint0);
		assertEqual(new Vector2(2, 2), polygonResult.mIntersectPoint1);
		assertEqual(Vector2.zero, polygonResult.mLine0.mStart);
		assertEqual(Vector2.up, polygonResult.mLine1.mEnd);
	}

	private static void testFormItemParamWriteAndReset()
	{
		FormItemParam item = new("token", "abc");
		using MemoryStream stream = new();
		item.write(stream, "BOUNDARY");
		string text = stream.ToArray().bytesToString();
		assertTrue(text.Contains("--BOUNDARY"), "应写入 boundary");
		assertTrue(text.Contains("name=\"token\""), "应写入字段名");
		assertTrue(text.Contains("abc"), "应写入字段值");

		item.resetProperty();
		assertNull(item.mKey, "reset 后 key 应清空");
		assertNull(item.mValue, "reset 后 value 应清空");
	}

	private static void testFormItemFileWriteAndReset()
	{
		byte[] file = { 1, 2, 3, 4, 5 };
		FormItemFile item = new(file, 3, "test.bin");
		using MemoryStream stream = new();
		item.write(stream, "BOUNDARY");
		byte[] data = stream.ToArray();
		string header = data.bytesToString();
		assertTrue(header.Contains("--BOUNDARY"), "应写入 boundary");
		assertTrue(header.Contains("filename=\"test.bin\""), "应写入文件名");
		assertEqual((byte)1, data[data.Length - 3], "文件内容第1字节错误");
		assertEqual((byte)2, data[data.Length - 2], "文件内容第2字节错误");
		assertEqual((byte)3, data[data.Length - 1], "文件内容第3字节错误");

		item.resetProperty();
		assertNull(item.mFileContent, "reset 后文件内容应清空");
		assertNull(item.mFileName, "reset 后文件名应清空");
		assertEqual(0, item.mFileLength, "reset 后长度应为0");
	}

	//------------------------------------------------------------------------------------------------------------------------------
	// Point 测试
	private static void testPointBasic()
	{
		Point p = new(3, 5);
		assertEqual(3, p.x, "Point x");
		assertEqual(5, p.y, "Point y");

		Point p2 = new(new Vector2Int(7, 9));
		assertEqual(7, p2.x, "Point Vector2Int x");
		assertEqual(9, p2.y, "Point Vector2Int y");
	}

	private static void testPointToIndex()
	{
		// y * width + x
		Point p = new(2, 3);
		assertEqual(3 * 10 + 2, p.toIndex(10), "Point toIndex");

		Point p2 = new(0, 0);
		assertEqual(0, p2.toIndex(10), "Point toIndex zero");
	}

	private static void testPointFromIndex()
	{
		// index % width, index / width
		Point p = Point.fromIndex(32, 10);
		assertEqual(2, p.x, "Point fromIndex x");
		assertEqual(3, p.y, "Point fromIndex y");

		Point p2 = Point.fromIndex(0, 10);
		assertEqual(0, p2.x, "Point fromIndex zero x");
		assertEqual(0, p2.y, "Point fromIndex zero y");
	}

	private static void testPointEquals()
	{
		Point a = new(1, 2);
		Point b = new(1, 2);
		assertTrue(a.Equals(b), "Point 相等");
		assertTrue(b.Equals(a), "Point 相等对称");

		Point c = new(3, 4);
		assertFalse(a.Equals(c), "Point 不等");
	}

	private static void testPointGetHashCode()
	{
		Point a = new(1, 2);
		Point b = new(1, 2);
		assertEqual(a.GetHashCode(), b.GetHashCode(), "Point GetHashCode 相等对象应一致");

		// 同一对象多次调用应一致
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2, "Point GetHashCode 幂等");
	}

	//------------------------------------------------------------------------------------------------------------------------------
	// Vector2Short 测试
	private static void testVector2ShortBasic()
	{
		Vector2Short v = new(10, 20);
		assertEqual((short)10, v.x, "Vector2Short x");
		assertEqual((short)20, v.y, "Vector2Short y");

		Vector2Short v2 = new(-5, -10);
		assertEqual((short)(-5), v2.x, "Vector2Short 负数 x");
		assertEqual((short)(-10), v2.y, "Vector2Short 负数 y");
	}

	private static void testVector2ShortEquals()
	{
		Vector2Short a = new(100, 200);
		Vector2Short b = new(100, 200);
		assertTrue(a.Equals(b), "Vector2Short 相等");

		Vector2Short c = new(100, 201);
		assertFalse(a.Equals(c), "Vector2Short 不等");
	}

	private static void testVector2ShortGetHashCode()
	{
		Vector2Short a = new(1, 2);
		Vector2Short b = new(1, 2);
		assertEqual(a.GetHashCode(), b.GetHashCode(), "Vector2Short GetHashCode 相等对象应一致");

		// 验证实现: (ushort)x << 16 | (ushort)y
		int expected = ((ushort)(short)1 << 16) | (ushort)(short)2;
		assertEqual(expected, a.GetHashCode(), "Vector2Short GetHashCode 实现");
	}

	private static void testVector2ShortToVec2()
	{
		Vector2Short v = new(30, 40);
		Vector2 result = v.toVec2();
		assertEqual(30.0f, result.x, "Vector2Short toVec2 x");
		assertEqual(40.0f, result.y, "Vector2Short toVec2 y");
	}

	private static void testVector2ShortToVec2Int()
	{
		Vector2Short v = new(50, 60);
		Vector2Int result = v.toVec2Int();
		assertEqual(50, result.x, "Vector2Short toVec2Int x");
		assertEqual(60, result.y, "Vector2Short toVec2Int y");
	}

	//------------------------------------------------------------------------------------------------------------------------------
	// Vector2UInt 测试
	private static void testVector2UIntBasic()
	{
		Vector2UInt v = new(10u, 20u);
		assertEqual(10u, v.x, "Vector2UInt x");
		assertEqual(20u, v.y, "Vector2UInt y");

		Vector2UInt v2 = new(0u, uint.MaxValue);
		assertEqual(0u, v2.x, "Vector2UInt 0 x");
		assertEqual(uint.MaxValue, v2.y, "Vector2UInt MaxValue y");
	}

	private static void testVector2UIntEquals()
	{
		Vector2UInt a = new(100u, 200u);
		Vector2UInt b = new(100u, 200u);
		assertTrue(a.Equals(b), "Vector2UInt 相等");

		Vector2UInt c = new(100u, 201u);
		assertFalse(a.Equals(c), "Vector2UInt 不等");
	}

	private static void testVector2UIntGetHashCode()
	{
		Vector2UInt a = new(1u, 2u);
		Vector2UInt b = new(1u, 2u);
		assertEqual(a.GetHashCode(), b.GetHashCode(), "Vector2UInt GetHashCode 相等对象应一致");

		// 验证实现: (int)(x << 16 | y)
		int expected = (int)(1u << 16 | 2u);
		assertEqual(expected, a.GetHashCode(), "Vector2UInt GetHashCode 实现");
	}

	private static void testVector2UIntToVec2()
	{
		Vector2UInt v = new(30u, 40u);
		Vector2 result = v.toVec2();
		assertEqual(30.0f, result.x, "Vector2UInt toVec2 x");
		assertEqual(40.0f, result.y, "Vector2UInt toVec2 y");
	}

	private static void testVector2UIntToVec2Int()
	{
		Vector2UInt v = new(50u, 60u);
		Vector2Int result = v.toVec2Int();
		assertEqual(50, result.x, "Vector2UInt toVec2Int x");
		assertEqual(60, result.y, "Vector2UInt toVec2Int y");
	}

	//------------------------------------------------------------------------------------------------------------------------------
	// Vector2UShort 测试
	private static void testVector2UShortBasic()
	{
		Vector2UShort v = new(10, 20);
		assertEqual((ushort)10, v.x, "Vector2UShort x");
		assertEqual((ushort)20, v.y, "Vector2UShort y");

		Vector2UShort v2 = new(0, ushort.MaxValue);
		assertEqual((ushort)0, v2.x, "Vector2UShort 0 x");
		assertEqual(ushort.MaxValue, v2.y, "Vector2UShort MaxValue y");
	}

	private static void testVector2UShortEquals()
	{
		Vector2UShort a = new(100, 200);
		Vector2UShort b = new(100, 200);
		assertTrue(a.Equals(b), "Vector2UShort 相等");

		Vector2UShort c = new(100, 201);
		assertFalse(a.Equals(c), "Vector2UShort 不等");
	}

	private static void testVector2UShortGetHashCode()
	{
		Vector2UShort a = new(1, 2);
		Vector2UShort b = new(1, 2);
		assertEqual(a.GetHashCode(), b.GetHashCode(), "Vector2UShort GetHashCode 相等对象应一致");

		// 验证实现: x << 16 | y
		int expected = ((ushort)1 << 16) | (ushort)2;
		assertEqual(expected, a.GetHashCode(), "Vector2UShort GetHashCode 实现");
	}

	private static void testVector2UShortToVec2()
	{
		Vector2UShort v = new(30, 40);
		Vector2 result = v.toVec2();
		assertEqual(30.0f, result.x, "Vector2UShort toVec2 x");
		assertEqual(40.0f, result.y, "Vector2UShort toVec2 y");
	}

	private static void testVector2UShortToVec2Int()
	{
		Vector2UShort v = new(50, 60);
		Vector2Int result = v.toVec2Int();
		assertEqual(50, result.x, "Vector2UShort toVec2Int x");
		assertEqual(60, result.y, "Vector2UShort toVec2Int y");
	}
}