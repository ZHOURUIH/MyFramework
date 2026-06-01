#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;
using static BIT_VECTOR2_USHORT;

public class BIT_VECTOR2_USHORTTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToVector2UShort();
		testXAndYProperties();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testMethodSignatures();
		testImplicitConversionEdgeCases();
		testSetIdempotent();
		testMixedInstances();
		testResetThenSet();
		testVector2UShortZero();
		testVector2UShortArithmetic();
		testVector2UShortComparison();
		testVector2UShortInList();
		testVector2UShortEquality();
		testVector2UShortHashCode();
		testVector2UShortToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testVector2UShortLargeValues();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.set(new Vector2UShort((ushort)3, (ushort)4));
		assertEqual((ushort)3, instance.mValue.x);
		assertEqual((ushort)4, instance.mValue.y);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue.x);
		assertEqual((ushort)0, instance.mValue.y);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.set(new Vector2UShort(0, 0));
		assertEqual((ushort)0, instance.mValue.x);
		instance.set(new Vector2UShort((ushort)1, (ushort)2));
		assertEqual((ushort)1, instance.mValue.x);
		assertEqual((ushort)2, instance.mValue.y);
		instance.set(new Vector2UShort(ushort.MaxValue, ushort.MaxValue));
		assertEqual(ushort.MaxValue, instance.mValue.x);
	}
	private static void testImplicitConversionToVector2UShort()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.set(new Vector2UShort((ushort)5, (ushort)6));
		Vector2UShort val = instance;
		assertEqual((ushort)5, val.x);
		assertEqual((ushort)6, val.y);
	}
	private static void testXAndYProperties()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.set(new Vector2UShort((ushort)10, (ushort)20));
		assertEqual((ushort)10, instance.x);
		assertEqual((ushort)20, instance.y);
	}
	private static void testMultipleInstances()
	{
		BIT_VECTOR2_USHORT a = new BIT_VECTOR2_USHORT();
		BIT_VECTOR2_USHORT b = new BIT_VECTOR2_USHORT();
		a.set(new Vector2UShort((ushort)1, (ushort)2));
		b.set(new Vector2UShort((ushort)3, (ushort)4));
		assertEqual((ushort)1, a.mValue.x);
		assertEqual((ushort)3, b.mValue.x);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.set(new Vector2UShort((ushort)9, (ushort)9));
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue.x);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue.x);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_VECTOR2_USHORT);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.set(new Vector2UShort((ushort)3, (ushort)4));
		Vector2UShort v = instance;
		BIT_VECTOR2_USHORT newInstance = new BIT_VECTOR2_USHORT();
		newInstance.set(v);
		assertEqual(((Vector2UShort)instance).x, ((Vector2UShort)newInstance).x);
	}
	private static void testSetIdempotent()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		Vector2UShort val = new Vector2UShort((ushort)4, (ushort)5);
		instance.set(val);
		instance.set(val);
		assertEqual((ushort)4, instance.mValue.x);
	}
	private static void testMixedInstances()
	{
		BIT_VECTOR2_USHORT a = new BIT_VECTOR2_USHORT();
		BIT_VECTOR2_USHORT b = new BIT_VECTOR2_USHORT();
		a.set(new Vector2UShort((ushort)10, (ushort)20));
		b.set(new Vector2UShort((ushort)30, (ushort)40));
		a.resetProperty();
		assertEqual((ushort)0, a.mValue.x);
		assertEqual((ushort)30, b.mValue.x);
	}
	private static void testResetThenSet()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue.x);
		instance.set(new Vector2UShort((ushort)10, (ushort)20));
		assertEqual((ushort)10, instance.mValue.x);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue.x);
	}
	private static void testVector2UShortZero()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.set(new Vector2UShort(0, 0));
		assertEqual((ushort)0, instance.mValue.x);
		assertEqual((ushort)0, instance.mValue.y);
	}
	private static void testVector2UShortArithmetic()
	{
		BIT_VECTOR2_USHORT a = new BIT_VECTOR2_USHORT();
		BIT_VECTOR2_USHORT b = new BIT_VECTOR2_USHORT();
		a.set(new Vector2UShort((ushort)1, (ushort)2));
		b.set(new Vector2UShort((ushort)3, (ushort)4));
		Vector2UShort sum = new Vector2UShort((ushort)(((Vector2UShort)a).x + ((Vector2UShort)b).x), (ushort)(((Vector2UShort)a).y + ((Vector2UShort)b).y));
		assertEqual((ushort)4, sum.x);
		assertEqual((ushort)6, sum.y);
	}
	private static void testVector2UShortComparison()
	{
		BIT_VECTOR2_USHORT a = new BIT_VECTOR2_USHORT();
		BIT_VECTOR2_USHORT b = new BIT_VECTOR2_USHORT();
		a.set(new Vector2UShort((ushort)1, (ushort)2));
		b.set(new Vector2UShort((ushort)1, (ushort)2));
		assertEqual(((Vector2UShort)a).x, ((Vector2UShort)b).x);
	}
	private static void testVector2UShortInList()
	{
		List<BIT_VECTOR2_USHORT> list = new List<BIT_VECTOR2_USHORT>();
		for (int i = 0; i < 3; i++)
		{
			BIT_VECTOR2_USHORT item = new BIT_VECTOR2_USHORT();
			item.set(new Vector2UShort((ushort)i, (ushort)(i * 2)));
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testVector2UShortEquality()
	{
		BIT_VECTOR2_USHORT a = new BIT_VECTOR2_USHORT();
		BIT_VECTOR2_USHORT b = new BIT_VECTOR2_USHORT();
		a.set(new Vector2UShort((ushort)4, (ushort)5));
		b.set(new Vector2UShort((ushort)4, (ushort)5));
		assertEqual(((Vector2UShort)a).x, ((Vector2UShort)b).x);
	}
	private static void testVector2UShortHashCode()
	{
		BIT_VECTOR2_USHORT a = new BIT_VECTOR2_USHORT();
		a.set(new Vector2UShort((ushort)4, (ushort)5));
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testVector2UShortToString()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.set(new Vector2UShort((ushort)1, (ushort)2));
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_VECTOR2_USHORT a = new BIT_VECTOR2_USHORT();
		BIT_VECTOR2_USHORT b = new BIT_VECTOR2_USHORT();
		a.set(new Vector2UShort((ushort)10, (ushort)20));
		b.set(new Vector2UShort((ushort)30, (ushort)40));
		a.resetProperty();
		assertEqual((ushort)0, a.mValue.x);
		assertEqual((ushort)30, b.mValue.x);
	}
	private static void testSetAfterReset()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue.x);
		instance.set(new Vector2UShort((ushort)7, (ushort)8));
		assertEqual((ushort)7, instance.mValue.x);
	}
	private static void testMultipleResets()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.set(new Vector2UShort((ushort)9, (ushort)9));
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue.x);
		instance.resetProperty();
		assertEqual((ushort)0, instance.mValue.x);
	}
	private static void testVector2UShortLargeValues()
	{
		BIT_VECTOR2_USHORT instance = new BIT_VECTOR2_USHORT();
		instance.set(new Vector2UShort(ushort.MaxValue, ushort.MaxValue));
		assertEqual(ushort.MaxValue, instance.mValue.x);
		assertEqual(ushort.MaxValue, instance.mValue.y);
	}
}
#endif
