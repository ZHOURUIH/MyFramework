using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

public class Vector2IntMyTest
{
	public void Run()
	{
		testConstructor();
		testEquals();
		testEqualsEdgeCases();
		testGetHashCode();
		testToVec2();
		testToVec2Int();
		testToVec2IntCornerCases();
		testEqualsWithSameValues();
		testEqualsWithDifferentValues();
		testToVec2Precision();
		testToVec2IntPrecision();
		testEqualsConsistency();
		testGetHashCodeConsistency();
		testVector2IntMyAsIEquatable();
		testEqualsSymmetry();
		testEqualsTransitivity();
		testGetHashCodeDistribution();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private void testConstructor()
	{
		Vector2IntMy v = new(1, 2);
		assertEqual(1, v.x);
		assertEqual(2, v.y);
		Vector2IntMy v2 = new(-100, 200);
		assertEqual(-100, v2.x);
		assertEqual(200, v2.y);
		Vector2IntMy v3 = new(0, 0);
		assertEqual(0, v3.x);
		assertEqual(0, v3.y);
		Vector2IntMy v4 = new(int.MaxValue, int.MinValue);
		assertEqual(int.MaxValue, v4.x);
		assertEqual(int.MinValue, v4.y);
	}
	private void testEquals()
	{
		Vector2IntMy a = new(1, 2);
		Vector2IntMy b = new(1, 2);
		assertTrue(a.Equals(b));
		assertTrue(b.Equals(a));
	}
	private void testEqualsEdgeCases()
	{
		Vector2IntMy a = new(0, 0);
		Vector2IntMy b = new(0, 0);
		assertTrue(a.Equals(b));
		Vector2IntMy c = new(int.MaxValue, int.MaxValue);
		Vector2IntMy d = new(int.MaxValue, int.MaxValue);
		assertTrue(c.Equals(d));
		Vector2IntMy e = new(int.MinValue, int.MinValue);
		Vector2IntMy f = new(int.MinValue, int.MinValue);
		assertTrue(e.Equals(f));
	}
	private void testGetHashCode()
	{
		Vector2IntMy v = new(1, 2);
		int hash = v.GetHashCode();
		// 根据实现: return x << 16 | y;
		int expectedHash = (1 << 16) | 2;
		assertEqual(expectedHash, hash);
	}
	private void testToVec2()
	{
		Vector2IntMy v = new(3, 4);
		Vector2 result = v.toVec2();
		assertEqual(3.0f, result.x);
		assertEqual(4.0f, result.y);
	}
	private void testToVec2Int()
	{
		Vector2IntMy v = new(5, 6);
		Vector2Int result = v.toVec2Int();
		assertEqual(5, result.x);
		assertEqual(6, result.y);
	}
	private void testToVec2IntCornerCases()
	{
		Vector2IntMy v = new(-10, 20);
		Vector2Int result = v.toVec2Int();
		assertEqual(-10, result.x);
		assertEqual(20, result.y);
		Vector2IntMy v2 = new(0, 0);
		Vector2Int result2 = v2.toVec2Int();
		assertEqual(0, result2.x);
		assertEqual(0, result2.y);
	}
	private void testEqualsWithSameValues()
	{
		Vector2IntMy a = new(100, 200);
		Vector2IntMy b = new(100, 200);
		assertTrue(a.Equals(b));
		assertTrue(b.Equals(a));
	}
	private void testEqualsWithDifferentValues()
	{
		Vector2IntMy a = new(1, 2);
		Vector2IntMy b = new(1, 3);
		assertFalse(a.Equals(b));
		Vector2IntMy c = new(3, 2);
		assertFalse(a.Equals(c));
		Vector2IntMy d = new(999, 999);
		assertFalse(a.Equals(d));
	}
	private void testToVec2Precision()
	{
		Vector2IntMy v = new(123, 456);
		Vector2 result = v.toVec2();
		assertEqual(123.0f, result.x);
		assertEqual(456.0f, result.y);
		Vector2IntMy v2 = new(-789, 0);
		Vector2 result2 = v2.toVec2();
		assertEqual(-789.0f, result2.x);
		assertEqual(0.0f, result2.y);
	}
	private void testToVec2IntPrecision()
	{
		Vector2IntMy v = new(1000, 2000);
		Vector2Int result = v.toVec2Int();
		assertEqual(1000, result.x);
		assertEqual(2000, result.y);
	}
	private void testEqualsConsistency()
	{
		Vector2IntMy a = new(42, 42);
		Vector2IntMy b = new(42, 42);
		assertTrue(a.Equals(b));
		assertTrue(a.Equals(b)); // 再次调用，结果应一致
		assertTrue(a.Equals(b));
	}
	private void testGetHashCodeConsistency()
	{
		Vector2IntMy v = new(7, 8);
		int hash1 = v.GetHashCode();
		int hash2 = v.GetHashCode();
		int hash3 = v.GetHashCode();
		assertEqual(hash1, hash2);
		assertEqual(hash2, hash3);
	}
	private void testVector2IntMyAsIEquatable()
	{
		Vector2IntMy a = new(1, 1);
		Vector2IntMy b = new(1, 1);
		IEquatable<Vector2IntMy> equatable = a;
		assertTrue(equatable.Equals(b));
	}
	private void testEqualsSymmetry()
	{
		Vector2IntMy a = new(10, 20);
		Vector2IntMy b = new(10, 20);
		assertTrue(a.Equals(b));
		assertTrue(b.Equals(a));
	}
	private void testEqualsTransitivity()
	{
		Vector2IntMy a = new(5, 5);
		Vector2IntMy b = new(5, 5);
		Vector2IntMy c = new(5, 5);
		assertTrue(a.Equals(b));
		assertTrue(b.Equals(c));
		assertTrue(a.Equals(c));
	}
	private void testGetHashCodeDistribution()
	{
		// 测试哈希码分布（基本检查）
		Vector2IntMy[] vectors = new Vector2IntMy[100];
		for (int i = 0; i < 100; i++)
		{
			vectors[i] = new Vector2IntMy(i, i * 2);
		}
		HashSet<int> hashSet = new();
		foreach (Vector2IntMy v in vectors)
		{
			hashSet.Add(v.GetHashCode());
		}
		// 100 个不同值，哈希集大小应 > 0
		assertTrue(hashSet.Count > 0);
	}
}