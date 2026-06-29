using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_VECTOR2_SHORTTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversionToVector2Short();
		testXAndYProperties();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testMethodSignatures();
		testImplicitConversionEdgeCases();
		testSetIdempotent();
		testMixedInstances();
		testResetThenSet();
		testVector2ShortZero();
		testVector2ShortArithmetic();
		testVector2ShortComparison();
		testVector2ShortInList();
		testVector2ShortEquality();
		testVector2ShortHashCode();
		testVector2ShortToString();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testVector2ShortLargeValues();
		testVector2ShortNegativeValues();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short((short)3, (short)4));
		assertEqual((short)3, instance.mValue.x);
		assertEqual((short)4, instance.mValue.y);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue.x);
		assertEqual((short)0, instance.mValue.y);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short(0, 0));
		assertEqual((short)0, instance.mValue.x);
		instance.set(new Vector2Short((short)1, (short)2));
		assertEqual((short)1, instance.mValue.x);
		assertEqual((short)2, instance.mValue.y);
		instance.set(new Vector2Short((short)-3, (short)-4));
		assertEqual((short)-3, instance.mValue.x);
	}
	private static void testImplicitConversionToVector2Short()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short((short)5, (short)6));
		Vector2Short val = instance;
		assertEqual((short)5, val.x);
		assertEqual((short)6, val.y);
	}
	private static void testXAndYProperties()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short((short)10, (short)20));
		assertEqual((short)10, instance.x);
		assertEqual((short)20, instance.y);
	}
	private static void testMultipleInstances()
	{
		BIT_VECTOR2_SHORT a = new BIT_VECTOR2_SHORT();
		BIT_VECTOR2_SHORT b = new BIT_VECTOR2_SHORT();
		a.set(new Vector2Short((short)1, (short)2));
		b.set(new Vector2Short((short)3, (short)4));
		assertEqual((short)1, a.mValue.x);
		assertEqual((short)3, b.mValue.x);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short((short)9, (short)9));
		instance.resetProperty();
		assertEqual((short)0, instance.mValue.x);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue.x);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_VECTOR2_SHORT);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short((short)3, (short)4));
		Vector2Short v = instance;
		BIT_VECTOR2_SHORT newInstance = new BIT_VECTOR2_SHORT();
		newInstance.set(v);
		assertEqual(((Vector2Short)instance).x, ((Vector2Short)newInstance).x);
	}
	private static void testSetIdempotent()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		Vector2Short val = new Vector2Short((short)4, (short)5);
		instance.set(val);
		instance.set(val);
		assertEqual((short)4, instance.mValue.x);
	}
	private static void testMixedInstances()
	{
		BIT_VECTOR2_SHORT a = new BIT_VECTOR2_SHORT();
		BIT_VECTOR2_SHORT b = new BIT_VECTOR2_SHORT();
		a.set(new Vector2Short((short)10, (short)20));
		b.set(new Vector2Short((short)30, (short)40));
		a.resetProperty();
		assertEqual((short)0, a.mValue.x);
		assertEqual((short)30, b.mValue.x);
	}
	private static void testResetThenSet()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.resetProperty();
		assertEqual((short)0, instance.mValue.x);
		instance.set(new Vector2Short((short)10, (short)20));
		assertEqual((short)10, instance.mValue.x);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue.x);
	}
	private static void testVector2ShortZero()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short(0, 0));
		assertEqual((short)0, instance.mValue.x);
		assertEqual((short)0, instance.mValue.y);
	}
	private static void testVector2ShortArithmetic()
	{
		BIT_VECTOR2_SHORT a = new BIT_VECTOR2_SHORT();
		BIT_VECTOR2_SHORT b = new BIT_VECTOR2_SHORT();
		a.set(new Vector2Short((short)1, (short)2));
		b.set(new Vector2Short((short)3, (short)4));
		Vector2Short sum = new Vector2Short((short)(((Vector2Short)a).x + ((Vector2Short)b).x), (short)(((Vector2Short)a).y + ((Vector2Short)b).y));
		assertEqual((short)4, sum.x);
		assertEqual((short)6, sum.y);
	}
	private static void testVector2ShortComparison()
	{
		BIT_VECTOR2_SHORT a = new BIT_VECTOR2_SHORT();
		BIT_VECTOR2_SHORT b = new BIT_VECTOR2_SHORT();
		a.set(new Vector2Short((short)1, (short)2));
		b.set(new Vector2Short((short)1, (short)2));
		assertEqual(((Vector2Short)a).x, ((Vector2Short)b).x);
	}
	private static void testVector2ShortInList()
	{
		List<BIT_VECTOR2_SHORT> list = new List<BIT_VECTOR2_SHORT>();
		for (int i = 0; i < 3; i++)
		{
			BIT_VECTOR2_SHORT item = new BIT_VECTOR2_SHORT();
			item.set(new Vector2Short((short)i, (short)(i * 2)));
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testVector2ShortEquality()
	{
		BIT_VECTOR2_SHORT a = new BIT_VECTOR2_SHORT();
		BIT_VECTOR2_SHORT b = new BIT_VECTOR2_SHORT();
		a.set(new Vector2Short((short)4, (short)5));
		b.set(new Vector2Short((short)4, (short)5));
		assertEqual(((Vector2Short)a).x, ((Vector2Short)b).x);
	}
	private static void testVector2ShortHashCode()
	{
		BIT_VECTOR2_SHORT a = new BIT_VECTOR2_SHORT();
		a.set(new Vector2Short((short)4, (short)5));
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testVector2ShortToString()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short((short)1, (short)2));
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_VECTOR2_SHORT a = new BIT_VECTOR2_SHORT();
		BIT_VECTOR2_SHORT b = new BIT_VECTOR2_SHORT();
		a.set(new Vector2Short((short)10, (short)20));
		b.set(new Vector2Short((short)30, (short)40));
		a.resetProperty();
		assertEqual((short)0, a.mValue.x);
		assertEqual((short)30, b.mValue.x);
	}
	private static void testSetAfterReset()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.resetProperty();
		assertEqual((short)0, instance.mValue.x);
		instance.set(new Vector2Short((short)7, (short)8));
		assertEqual((short)7, instance.mValue.x);
	}
	private static void testMultipleResets()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short((short)9, (short)9));
		instance.resetProperty();
		assertEqual((short)0, instance.mValue.x);
		instance.resetProperty();
		assertEqual((short)0, instance.mValue.x);
	}
	private static void testVector2ShortLargeValues()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short(short.MaxValue, short.MinValue));
		assertEqual(short.MaxValue, instance.mValue.x);
		assertEqual(short.MinValue, instance.mValue.y);
	}
	private static void testVector2ShortNegativeValues()
	{
		BIT_VECTOR2_SHORT instance = new BIT_VECTOR2_SHORT();
		instance.set(new Vector2Short((short)-100, (short)-200));
		assertEqual((short)-100, instance.mValue.x);
		assertEqual((short)-200, instance.mValue.y);
		Vector2Short v = instance;
		assertEqual((short)-100, v.x);
		assertEqual((short)-200, v.y);
	}
}