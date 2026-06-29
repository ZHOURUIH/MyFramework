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
}