#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static TestAssert;
using static BIT_STRING;

public class BIT_STRINGTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testSetAndGet();
		testImplicitConversion();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testEdgeCases();
		testMethodSignatures();
		testMixedInstances();
		testResetThenSet();
		testNullString();
		testEmptyString();
		testWhitespaceString();
		testLongString();
		testStringWithSpecialCharacters();
		testStringWithUnicode();
		testStringWithEscapeSequences();
		testStringComparisons();
		testStringInList();
		testStringEquality();
		testStringHashCode();
		testStringLength();
		testStringConcatenation();
		testStringSubstring();
		testStringSplitting();
		testStringFormatting();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testStringImplicitConversionToInt();
		testStringImplicitConversionToFloat();
		testStringToUpper();
		testStringToLower();
		testStringTrim();
		testStringContains();
		testStringStartsWith();
		testStringEndsWith();
		testStringReplace();
		testStringIndexOf();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("hello");
		assertEqual("hello", (string)instance);
		instance.resetProperty();
		assertEqual("", instance.mValue);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_STRING instance = new BIT_STRING();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testSetAndGet()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("");
		assertEqual("", instance.mValue);
		instance.set("hello");
		assertEqual("hello", instance.mValue);
		assertEqual("hello", (string)instance);
		instance.set("world");
		assertEqual("world", instance.mValue);
		instance.set("test123");
		assertEqual("test123", instance.mValue);
		instance.set("with spaces");
		assertEqual("with spaces", instance.mValue);
		instance.set("with\ttab");
		assertEqual("with\ttab", instance.mValue);
		instance.set("with\nnewline");
		assertEqual("with\nnewline", instance.mValue);
	}
	private static void testImplicitConversion()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("test");
		string val = instance;
		assertEqual("test", val);
		instance.set("");
		string valEmpty = instance;
		assertEqual("", valEmpty);
	}
	private static void testMultipleInstances()
	{
		BIT_STRING a = new BIT_STRING();
		BIT_STRING b = new BIT_STRING();
		BIT_STRING c = new BIT_STRING();
		a.set("a"); b.set("b"); c.set("c");
		assertEqual("a", a.mValue);
		assertEqual("b", b.mValue);
		assertEqual("c", c.mValue);
		a.set("changed");
		assertEqual("changed", a.mValue);
		assertEqual("b", b.mValue);
		assertEqual("c", c.mValue);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("test");
		instance.resetProperty();
		assertEqual("", instance.mValue);
		instance.resetProperty();
		assertEqual("", instance.mValue);
	}
	private static void testEdgeCases()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("");
		assertEqual("", instance.mValue);
		string longStr = new string('x', 1000);
		instance.set(longStr);
		assertEqual(1000, instance.mValue.Length);
		instance.set("!@#$%^&*()");
		assertEqual("!@#$%^&*()", instance.mValue);
	}
	private static void testMethodSignatures()
	{
		System.Type t = typeof(BIT_STRING);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		assertEqual(2, readMethod.GetParameters().Length);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
		assertEqual(2, writeMethod.GetParameters().Length);
		var resetMethod = t.GetMethod("resetProperty");
		assertNotNull(resetMethod);
	}
	private static void testMixedInstances()
	{
		BIT_STRING a = new BIT_STRING();
		BIT_STRING b = new BIT_STRING();
		a.set("longstring");
		b.set("");
		assertTrue(a.mValue.Length > 0);
		assertEqual("", b.mValue);
		a.resetProperty();
		assertEqual("", a.mValue);
		assertEqual("", b.mValue);
	}
	private static void testResetThenSet()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.resetProperty();
		assertEqual("", instance.mValue);
		instance.set("after reset");
		assertEqual("after reset", instance.mValue);
		instance.resetProperty();
		assertEqual("", instance.mValue);
		instance.set("final");
		assertEqual("final", instance.mValue);
	}
	private static void testNullString()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set(null);
		assertNull(instance.mValue);
	}
	private static void testEmptyString()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("");
		assertEqual("", instance.mValue);
		assertEqual(0, instance.mValue.Length);
	}
	private static void testWhitespaceString()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set(" \t\n\r");
		assertEqual(" \t\n\r", instance.mValue);
	}
	private static void testLongString()
	{
		BIT_STRING instance = new BIT_STRING();
		string longStr = new string('x', 10000);
		instance.set(longStr);
		assertEqual(10000, instance.mValue.Length);
	}
	private static void testStringWithSpecialCharacters()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("!@#$%^&*()");
		assertEqual("!@#$%^&*()", instance.mValue);
		instance.set("<>?,./:\";'{}[]|\\");
		assertEqual("<>?,./:\";'{}[]|\\", instance.mValue);
	}
	private static void testStringWithUnicode()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("Hello 世界");
		assertEqual("Hello 世界", instance.mValue);
		instance.set("🎉🎊🎈");
		assertEqual("🎉🎊🎈", instance.mValue);
	}
	private static void testStringWithEscapeSequences()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("line1\nline2");
		assertEqual("line1\nline2", instance.mValue);
		instance.set("tab\there");
		assertEqual("tab\there", instance.mValue);
	}
	private static void testStringComparisons()
	{
		BIT_STRING a = new BIT_STRING();
		BIT_STRING b = new BIT_STRING();
		a.set("abc"); b.set("def");
		assertTrue(string.Compare((string)a, (string)b) < 0);
		a.set("xyz"); b.set("xyz");
		assertEqual(0, string.Compare((string)a, (string)b));
	}
	private static void testStringInList()
	{
		List<BIT_STRING> list = new List<BIT_STRING>();
		string[] strs = new string[] { "one", "two", "three" };
		foreach (var s in strs)
		{
			BIT_STRING item = new BIT_STRING();
			item.set(s);
			list.Add(item);
		}
		assertEqual(3, list.Count);
		assertEqual("one", (string)list[0]);
		assertEqual("two", (string)list[1]);
		assertEqual("three", (string)list[2]);
	}
	private static void testStringEquality()
	{
		BIT_STRING a = new BIT_STRING();
		BIT_STRING b = new BIT_STRING();
		a.set("hello"); b.set("hello");
		assertEqual((string)a, (string)b);
	}
	private static void testStringHashCode()
	{
		BIT_STRING a = new BIT_STRING();
		a.set("hello");
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testStringLength()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("hello");
		assertEqual(5, instance.mValue.Length);
		instance.set("");
		assertEqual(0, instance.mValue.Length);
	}
	private static void testStringConcatenation()
	{
		BIT_STRING a = new BIT_STRING();
		BIT_STRING b = new BIT_STRING();
		a.set("hello"); b.set("world");
		string concat = (string)a + (string)b;
		assertEqual("helloworld", concat);
	}
	private static void testStringSubstring()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("hello world");
		string sub = instance.mValue.Substring(0, 5);
		assertEqual("hello", sub);
	}
	private static void testStringSplitting()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("a,b,c");
		string[] parts = instance.mValue.Split(',');
		assertEqual(3, parts.Length);
		assertEqual("a", parts[0]);
		assertEqual("b", parts[1]);
		assertEqual("c", parts[2]);
	}
	private static void testStringFormatting()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set(string.Format("Value: {0}", 42));
		assertEqual("Value: 42", instance.mValue);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_STRING a = new BIT_STRING();
		BIT_STRING b = new BIT_STRING();
		a.set("longstring");
		b.set("");
		assertTrue(a.mValue.Length > 0);
		assertEqual("", b.mValue);
		a.resetProperty();
		assertEqual("", a.mValue);
		assertEqual("", b.mValue);
	}
	private static void testSetAfterReset()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.resetProperty();
		assertEqual("", instance.mValue);
		instance.set("after reset");
		assertEqual("after reset", instance.mValue);
	}
	private static void testMultipleResets()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("test");
		instance.resetProperty();
		assertEqual("", instance.mValue);
		instance.resetProperty();
		assertEqual("", instance.mValue);
	}
	private static void testStringImplicitConversionToInt()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("42");
		int val = int.Parse((string)instance);
		assertEqual(42, val);
	}
	private static void testStringImplicitConversionToFloat()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("3.14");
		float val = float.Parse((string)instance);
		assertTrue(Math.Abs(3.14f - val) < 0.001f);
	}
	private static void testStringToUpper()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("hello");
		string upper = instance.mValue.ToUpper();
		assertEqual("HELLO", upper);
	}
	private static void testStringToLower()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("HELLO");
		string lower = instance.mValue.ToLower();
		assertEqual("hello", lower);
	}
	private static void testStringTrim()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("  hello  ");
		string trimmed = instance.mValue.Trim();
		assertEqual("hello", trimmed);
	}
	private static void testStringContains()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("hello world");
		assertTrue(instance.mValue.Contains("world"));
		assertFalse(instance.mValue.Contains("xyz"));
	}
	private static void testStringStartsWith()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("hello world");
		assertTrue(instance.mValue.StartsWith("hello"));
		assertFalse(instance.mValue.StartsWith("world"));
	}
	private static void testStringEndsWith()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("hello world");
		assertTrue(instance.mValue.EndsWith("world"));
		assertFalse(instance.mValue.EndsWith("hello"));
	}
	private static void testStringReplace()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("hello world");
		string replaced = instance.mValue.Replace("world", "there");
		assertEqual("hello there", replaced);
	}
	private static void testStringIndexOf()
	{
		BIT_STRING instance = new BIT_STRING();
		instance.set("hello world");
		assertEqual(6, instance.mValue.IndexOf("world"));
		assertEqual(-1, instance.mValue.IndexOf("xyz"));
	}
}
#endif
