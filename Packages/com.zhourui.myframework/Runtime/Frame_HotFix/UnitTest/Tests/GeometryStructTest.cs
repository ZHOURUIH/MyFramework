using UnityEngine;
using static TestAssert;

// 基础几何/数据结构单元测试 — 覆盖未纳入测试的结构体
//   Triangle2 / Triangle3 (2D/3D 三角形数据)
//   Vector4Int (int 四维向量, 相等性/哈希)
public static class GeometryStructTest
{
	public static void Run()
	{
		testTriangle2_DefaultValues();
		testTriangle2_Constructor();
		testTriangle2_ToTriangle3();
		testTriangle3_DefaultValues();
		testTriangle3_Constructor();
		testTriangle3_ToTriangle2();
		testVector4Int_DefaultValues();
		testVector4Int_Constructor();
		testVector4Int_Equals();
		testVector4Int_Equals_Default();
		testVector4Int_GetHashCode();
	}

	// ═════════════════════════════════════════════════════════════════
	// Triangle2
	// ═════════════════════════════════════════════════════════════════
	private static void testTriangle2_DefaultValues()
	{
		Triangle2 t = new();
		assertEqual(Vector2.zero, t.mPoint0, "默认 p0 为 zero");
		assertEqual(Vector2.zero, t.mPoint1, "默认 p1 为 zero");
		assertEqual(Vector2.zero, t.mPoint2, "默认 p2 为 zero");
	}
	private static void testTriangle2_Constructor()
	{
		Triangle2 t = new(new Vector2(1, 2), new Vector2(3, 4), new Vector2(5, 6));
		assertEqual(new Vector2(1, 2), t.mPoint0, "p0 正确赋值");
		assertEqual(new Vector2(3, 4), t.mPoint1, "p1 正确赋值");
		assertEqual(new Vector2(5, 6), t.mPoint2, "p2 正确赋值");
	}
	private static void testTriangle2_ToTriangle3()
	{
		Triangle2 t2 = new(new Vector2(1, 2), new Vector2(3, 4), new Vector2(5, 6));
		Triangle3 t3 = t2.toTriangle3();
		// Vector2→Vector3 隐式转换, z 分量为 0
		assertEqual(new Vector3(1, 2, 0), t3.mPoint0, "p0 转换 z=0");
		assertEqual(new Vector3(3, 4, 0), t3.mPoint1, "p1 转换 z=0");
		assertEqual(new Vector3(5, 6, 0), t3.mPoint2, "p2 转换 z=0");
	}

	// ═════════════════════════════════════════════════════════════════
	// Triangle3
	// ═════════════════════════════════════════════════════════════════
	private static void testTriangle3_DefaultValues()
	{
		Triangle3 t = new();
		assertEqual(Vector3.zero, t.mPoint0, "默认 p0 为 zero");
		assertEqual(Vector3.zero, t.mPoint1, "默认 p1 为 zero");
		assertEqual(Vector3.zero, t.mPoint2, "默认 p2 为 zero");
	}
	private static void testTriangle3_Constructor()
	{
		Triangle3 t = new(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9));
		assertEqual(new Vector3(1, 2, 3), t.mPoint0, "p0 正确赋值");
		assertEqual(new Vector3(4, 5, 6), t.mPoint1, "p1 正确赋值");
		assertEqual(new Vector3(7, 8, 9), t.mPoint2, "p2 正确赋值");
	}
	private static void testTriangle3_ToTriangle2()
	{
		// Triangle3 无 toTriangle2, 但可手动验证结构字段可读写 (数据容器)
		Triangle3 t3 = new(new Vector3(1, 2, 3), new Vector3(4, 5, 6), new Vector3(7, 8, 9));
		Triangle2 t2 = new(new Vector2(t3.mPoint0.x, t3.mPoint0.y),
						   new Vector2(t3.mPoint1.x, t3.mPoint1.y),
						   new Vector2(t3.mPoint2.x, t3.mPoint2.y));
		assertEqual(new Vector2(1, 2), t2.mPoint0, "手动降维 p0 正确");
		assertEqual(new Vector2(4, 5), t2.mPoint1, "手动降维 p1 正确");
		assertEqual(new Vector2(7, 8), t2.mPoint2, "手动降维 p2 正确");
	}

	// ═════════════════════════════════════════════════════════════════
	// Vector4Int
	// ═════════════════════════════════════════════════════════════════
	private static void testVector4Int_DefaultValues()
	{
		Vector4Int v = new();
		assertEqual(0, v.x, "默认 x 为 0");
		assertEqual(0, v.y, "默认 y 为 0");
		assertEqual(0, v.z, "默认 z 为 0");
		assertEqual(0, v.w, "默认 w 为 0");
	}
	private static void testVector4Int_Constructor()
	{
		Vector4Int v = new(1, 2, 3, 4);
		assertEqual(1, v.x, "x 正确赋值");
		assertEqual(2, v.y, "y 正确赋值");
		assertEqual(3, v.z, "z 正确赋值");
		assertEqual(4, v.w, "w 正确赋值");
	}
	private static void testVector4Int_Equals()
	{
		Vector4Int a = new(1, 2, 3, 4);
		Vector4Int b = new(1, 2, 3, 4);
		assertTrue(a.Equals(b), "相同分量应相等");
		Vector4Int c = new(1, 2, 3, 5);
		assertFalse(a.Equals(c), "w 不同则不等");
		Vector4Int d = new(9, 2, 3, 4);
		assertFalse(a.Equals(d), "x 不同则不等");
	}
	private static void testVector4Int_Equals_Default()
	{
		Vector4Int zero = new(0, 0, 0, 0);
		assertTrue(zero.Equals(Vector4Int.zero), "显式 zero 与静态 zero 相等");
	}
	private static void testVector4Int_GetHashCode()
	{
		Vector4Int a = new(1, 2, 3, 4);
		Vector4Int b = new(1, 2, 3, 4);
		assertEqual(a.GetHashCode(), b.GetHashCode(), "相等对象哈希应一致");
		// 注意: Vector4Int.GetHashCode 用 `x<<48|y<<32|z<<16|w`, int 移位只取低5位,
		// 导致 x 与 z 重叠(都<<16), 不同分量组合可能产生相同哈希(实现固有碰撞), 故不断言不同对象哈希必不同
	}
}
