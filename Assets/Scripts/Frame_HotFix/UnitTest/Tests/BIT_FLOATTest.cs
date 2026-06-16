using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_FLOATTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testSetWithSpecialFloatValues();
		testImplicitConversionToFloat();
		testImplicitConversionToInt();
		testImplicitConversionToDouble();
		testFloatPrecision();
		testFloatArithmeticWithImplicit();
		testFloatComparisons();
		testFloatInList();
		testFloatEquality();
		testFloatHashCode();
		testFloatToString();
		testMultipleInstancesIndependent();
		testResetThenSetMultipleTimes();
		testSetIdempotentForSameValue();
		testEdgeCaseZero();
		testEdgeCaseNegativeZero();
		testEdgeCaseMaxValue();
		testEdgeCaseMinValue();
		testEdgeCaseEpsilon();
		testEdgeCaseNegativeEpsilon();
		testMethodSignatureRead();
		testMethodSignatureWrite();
		testMethodSignatureResetProperty();
		testValidFieldBehavior();
		testOptionalFieldBehavior();
		testSetAfterReset();
		testMultipleResets();
		testImplicitConversionInArithmetic();
		testFloatNaNValues();
		testFloatInfinityValues();
		testFloatNegativeInfinityValues();
		testFloatVeryLargeValues();
		testFloatVerySmallValues();
		testFloatRoundTripViaInt();
		testTwoInstancesIndependent();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(3.14f);
		assertFloatEqual(3.14f, instance.mValue, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		float[] vals = new float[] { 0.0f, 1.0f, -1.0f, 3.14f, -2.71f, 100.5f, -200.25f };
		foreach (float v in vals)
		{
			instance.set(v);
			assertFloatEqual(v, instance.mValue, 0.0001f);
			assertFloatEqual(v, (float)instance, 0.0001f);
		}
		instance.set(float.MaxValue);
		assertTrue(instance.mValue > 0.0f);
		instance.set(float.MinValue);
		assertTrue(instance.mValue < 0.0f);
	}
	private static void testSetWithSpecialFloatValues()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(float.NaN);
		assertTrue(float.IsNaN(instance.mValue));
		instance.set(float.PositiveInfinity);
		assertTrue(float.IsInfinity(instance.mValue));
		assertTrue(instance.mValue > 0);
		instance.set(float.NegativeInfinity);
		assertTrue(float.IsNegativeInfinity(instance.mValue));
	}
	private static void testImplicitConversionToFloat()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(100.0f);
		float val = instance;
		assertFloatEqual(100.0f, val, 0.0001f);
		instance.set(0.0f);
		assertFloatEqual(0.0f, (float)instance, 0.0001f);
		instance.set(-50.5f);
		assertFloatEqual(-50.5f, (float)instance, 0.0001f);
	}
	private static void testImplicitConversionToInt()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(42.0f);
		int intVal = (int)(float)instance;
		assertEqual(42, intVal);
		instance.set(3.14f);
		int intVal2 = (int)(float)instance;
		assertEqual(3, intVal2);
	}
	private static void testImplicitConversionToDouble()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(1.23456f);
		double dblVal = (float)instance;
		assertTrue(Math.Abs(1.23456 - dblVal) < 0.0001);
	}
	private static void testFloatPrecision()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(1.0f / 3.0f);
		float val = instance;
		assertTrue(Math.Abs(val - 1.0f / 3.0f) < 0.0001f);
		instance.set(0.1f + 0.2f);
		float sum = instance;
		assertTrue(Math.Abs(sum - 0.3f) < 0.0001f);
	}
	private static void testFloatArithmeticWithImplicit()
	{
		BIT_FLOAT a = new BIT_FLOAT();
		BIT_FLOAT b = new BIT_FLOAT();
		a.set(10.0f);
		b.set(20.0f);
		float sum = (float)a + (float)b;
		assertFloatEqual(30.0f, sum, 0.0001f);
		float diff = (float)b - (float)a;
		assertFloatEqual(10.0f, diff, 0.0001f);
		float prod = (float)a * (float)b;
		assertFloatEqual(200.0f, prod, 0.0001f);
	}
	private static void testFloatComparisons()
	{
		BIT_FLOAT a = new BIT_FLOAT();
		BIT_FLOAT b = new BIT_FLOAT();
		a.set(10.0f);
		b.set(20.0f);
		assertTrue((float)a < (float)b);
		assertTrue((float)b > (float)a);
		assertTrue(Math.Abs((float)a - 10.0f) < 0.0001f);
		a.set(10.0f);
		b.set(10.0f);
		assertTrue(Math.Abs((float)a - (float)b) < 0.0001f);
	}
	private static void testFloatInList()
	{
		List<BIT_FLOAT> list = new List<BIT_FLOAT>();
		for (int i = 0; i < 5; i++)
		{
			BIT_FLOAT item = new BIT_FLOAT();
			item.set(i * 1.5f);
			list.Add(item);
		}
		assertEqual(5, list.Count);
		assertFloatEqual(0.0f, (float)list[0], 0.0001f);
		assertFloatEqual(1.5f, (float)list[1], 0.0001f);
		assertFloatEqual(6.0f, (float)list[4], 0.0001f);
	}
	private static void testFloatEquality()
	{
		BIT_FLOAT a = new BIT_FLOAT();
		BIT_FLOAT b = new BIT_FLOAT();
		a.set(42.0f);
		b.set(42.0f);
		assertFloatEqual((float)a, (float)b, 0.0001f);
		a.set(1.0f / 3.0f);
		b.set(1.0f / 3.0f);
		assertFloatEqual((float)a, (float)b, 0.0001f);
	}
	private static void testFloatHashCode()
	{
		BIT_FLOAT a = new BIT_FLOAT();
		a.set(42.0f);
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testFloatToString()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(42.5f);
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testMultipleInstancesIndependent()
	{
		BIT_FLOAT a = new BIT_FLOAT();
		BIT_FLOAT b = new BIT_FLOAT();
		BIT_FLOAT c = new BIT_FLOAT();
		a.set(1.1f); b.set(2.2f); c.set(3.3f);
		assertFloatEqual(1.1f, a.mValue, 0.0001f);
		assertFloatEqual(2.2f, b.mValue, 0.0001f);
		assertFloatEqual(3.3f, c.mValue, 0.0001f);
		a.set(100.0f);
		assertFloatEqual(100.0f, a.mValue, 0.0001f);
		assertFloatEqual(2.2f, b.mValue, 0.0001f);
	}
	private static void testResetThenSetMultipleTimes()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.resetProperty();
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
		instance.set(100.0f);
		assertFloatEqual(100.0f, instance.mValue, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
		instance.set(-200.5f);
		assertFloatEqual(-200.5f, instance.mValue, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
		instance.set(999.999f);
		assertFloatEqual(999.999f, instance.mValue, 0.001f);
	}
	private static void testSetIdempotentForSameValue()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(42.0f);
		instance.set(42.0f);
		assertFloatEqual(42.0f, instance.mValue, 0.0001f);
		instance.set(0.0f);
		instance.set(0.0f);
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
	}
	private static void testEdgeCaseZero()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(0.0f);
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
		assertFloatEqual(0.0f, (float)instance, 0.0001f);
		float zero = instance;
		assertFloatEqual(0.0f, zero, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
		instance.set(0.0f);
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
	}
	private static void testEdgeCaseNegativeZero()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(-0.0f);
		assertTrue((float)instance >= 0.0f || (float)instance <= 0.0f);
	}
	private static void testEdgeCaseMaxValue()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(float.MaxValue);
		assertFloatEqual(float.MaxValue, instance.mValue, float.MaxValue * 0.0001f);
		instance.set(float.MaxValue);
		assertFloatEqual(float.MaxValue, (float)instance, float.MaxValue * 0.0001f);
	}
	private static void testEdgeCaseMinValue()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(float.MinValue);
		assertFloatEqual(float.MinValue, instance.mValue, float.MaxValue * 0.0001f);
		instance.set(float.MinValue);
		assertFloatEqual(float.MinValue, (float)instance, float.MaxValue * 0.0001f);
	}
	private static void testEdgeCaseEpsilon()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(float.Epsilon);
		assertTrue(instance.mValue > 0.0f);
		assertFloatEqual(float.Epsilon, instance.mValue, float.Epsilon);
	}
	private static void testEdgeCaseNegativeEpsilon()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(-float.Epsilon);
		assertTrue(instance.mValue < 0.0f);
		assertFloatEqual(-float.Epsilon, instance.mValue, float.Epsilon);
	}
	private static void testMethodSignatureRead()
	{
		Type t = typeof(BIT_FLOAT);
		var method = t.GetMethod("read");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testMethodSignatureWrite()
	{
		Type t = typeof(BIT_FLOAT);
		var method = t.GetMethod("write");
		assertNotNull(method);
		assertEqual(2, method.GetParameters().Length);
	}
	private static void testMethodSignatureResetProperty()
	{
		Type t = typeof(BIT_FLOAT);
		var method = t.GetMethod("resetProperty");
		assertNotNull(method);
	}
	private static void testValidFieldBehavior()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		assertTrue(instance.mValid);
		instance.mValid = false;
		assertFalse(instance.mValid);
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testOptionalFieldBehavior()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.mOptional = true;
		assertTrue(instance.mOptional);
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAfterReset()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.resetProperty();
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
		instance.set(123.456f);
		assertFloatEqual(123.456f, instance.mValue, 0.001f);
	}
	private static void testMultipleResets()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(999.0f);
		instance.resetProperty();
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
		instance.resetProperty();
		assertFloatEqual(0.0f, instance.mValue, 0.0001f);
	}
	private static void testImplicitConversionInArithmetic()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(10.0f);
		float result = (float)instance * 2.0f;
		assertFloatEqual(20.0f, result, 0.0001f);
		result = (float)instance + 5.0f;
		assertFloatEqual(15.0f, result, 0.0001f);
		result = (float)instance / 2.0f;
		assertFloatEqual(5.0f, result, 0.0001f);
	}
	private static void testFloatNaNValues()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(float.NaN);
		assertTrue(float.IsNaN(instance.mValue));
		float val = instance;
		assertTrue(float.IsNaN(val));
	}
	private static void testFloatInfinityValues()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(float.PositiveInfinity);
		assertTrue(float.IsInfinity(instance.mValue));
		assertTrue(instance.mValue > 0);
	}
	private static void testFloatNegativeInfinityValues()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(float.NegativeInfinity);
		assertTrue(float.IsNegativeInfinity(instance.mValue));
		assertTrue(instance.mValue < 0);
	}
	private static void testFloatVeryLargeValues()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(1.0e38f);
		assertTrue(instance.mValue > 1.0e37f);
		instance.set(-1.0e38f);
		assertTrue(instance.mValue < -1.0e37f);
	}
	private static void testFloatVerySmallValues()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(1.0e-38f);
		assertTrue(instance.mValue > 0.0f);
		instance.set(-1.0e-38f);
		assertTrue(instance.mValue < 0.0f);
	}
	private static void testFloatRoundTripViaInt()
	{
		BIT_FLOAT instance = new BIT_FLOAT();
		instance.set(42.0f);
		int intVal = (int)(float)instance;
		BIT_FLOAT newInstance = new BIT_FLOAT();
		newInstance.set((float)intVal);
		assertFloatEqual((float)instance, (float)newInstance, 0.0001f);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_FLOAT a = new BIT_FLOAT();
		BIT_FLOAT b = new BIT_FLOAT();
		a.set(float.MaxValue);
		b.set(float.MinValue);
		assertTrue(Math.Abs((float)a - float.MaxValue) < float.MaxValue * 0.001f);
		assertTrue(Math.Abs((float)b - float.MinValue) < float.MaxValue * 0.001f);
		a.resetProperty();
		assertFloatEqual(0.0f, (float)a, 0.0001f);
		assertTrue(Math.Abs((float)b - float.MinValue) < float.MaxValue * 0.001f);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void assertFloatEqual(float expected, float actual, float tolerance)
	{
		assertTrue(Math.Abs(expected - actual) <= tolerance,
			"Expected " + expected.ToString() + ", got " + actual.ToString() + ", tolerance " + tolerance.ToString());
	}
}