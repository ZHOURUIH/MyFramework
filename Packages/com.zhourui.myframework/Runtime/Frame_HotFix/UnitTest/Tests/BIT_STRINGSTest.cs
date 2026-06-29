using System;
using System.Collections.Generic;
using static TestAssert;

public class BIT_STRINGSTest
{
	public static void Run()
	{
		testConstructorAndReset();
		testConstructorSetsValidTrue();
		testResetPropertyResetsValid();
		testAddAndCount();
		testAddRangeAndCount();
		testIndexerGetAndSet();
		testImplicitConversionToList();
		testMultipleInstances();
		testResetPropertyIdempotent();
		testMethodSignatures();
		testImplicitConversionEdgeCases();
		testAddIdempotent();
		testMixedInstances();
		testResetThenAdd();
		testListOperationsAdd();
		testListOperationsRemove();
		testListOperationsClear();
		testListOperationsContains();
		testListOperationsIndexOf();
		testListOperationsInsert();
		testListOperationsRemoveAt();
		testListOperationsCount();
		testListInCollection();
		testListEquality();
		testListHashCode();
		testListToString();
		testEnumerator();
		testTwoInstancesIndependent();
		testSetAfterReset();
		testMultipleResets();
		testLargeList();
		testEmptyList();
		testSingleElementList();
		testDuplicateValues();
		testNullStringInList();
		testEmptyStringInList();
		testLongStringInList();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorAndReset()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("hello");
		assertEqual(1, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testConstructorSetsValidTrue()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		assertTrue(instance.mValid);
	}
	private static void testResetPropertyResetsValid()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.mValid = false;
		instance.resetProperty();
		assertTrue(instance.mValid);
	}
	private static void testAddAndCount()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("");
		assertEqual(1, instance.Count);
		instance.add("hello");
		assertEqual(2, instance.Count);
		instance.add("world");
		assertEqual(3, instance.Count);
		assertEqual("", instance[0]);
		assertEqual("hello", instance[1]);
		assertEqual("world", instance[2]);
	}
	private static void testAddRangeAndCount()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.addRange(new string[] { "a", "b", "c" });
		assertEqual(3, instance.Count);
		assertEqual("a", instance[0]);
		assertEqual("b", instance[1]);
		assertEqual("c", instance[2]);
	}
	private static void testIndexerGetAndSet()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("x");
		instance.add("y");
		assertEqual("x", instance[0]);
		assertEqual("y", instance[1]);
		instance[0] = "z";
		assertEqual("z", instance[0]);
	}
	private static void testImplicitConversionToList()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("a");
		instance.add("b");
		List<string> list = instance;
		assertEqual(2, list.Count);
		assertEqual("a", list[0]);
		assertEqual("b", list[1]);
	}
	private static void testMultipleInstances()
	{
		BIT_STRINGS a = new BIT_STRINGS();
		BIT_STRINGS b = new BIT_STRINGS();
		a.add("first");
		b.add("second");
		assertEqual(1, a.Count);
		assertEqual(1, b.Count);
		assertEqual("first", a[0]);
		assertEqual("second", b[0]);
	}
	private static void testResetPropertyIdempotent()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("test");
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testMethodSignatures()
	{
		Type t = typeof(BIT_STRINGS);
		var readMethod = t.GetMethod("read");
		assertNotNull(readMethod);
		var writeMethod = t.GetMethod("write");
		assertNotNull(writeMethod);
	}
	private static void testImplicitConversionEdgeCases()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("data");
		List<string> list = instance;
		BIT_STRINGS newInstance = new BIT_STRINGS();
		newInstance.addRange(list);
		assertEqual(instance.Count, newInstance.Count);
		assertEqual(instance[0], newInstance[0]);
	}
	private static void testAddIdempotent()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("value");
		assertEqual(1, instance.Count);
		assertEqual("value", instance[0]);
	}
	private static void testMixedInstances()
	{
		BIT_STRINGS a = new BIT_STRINGS();
		BIT_STRINGS b = new BIT_STRINGS();
		a.add("a"); a.add("b");
		b.add("c");
		assertEqual(2, a.Count);
		assertEqual(1, b.Count);
	}
	private static void testResetThenAdd()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add("new");
		assertEqual(1, instance.Count);
		assertEqual("new", instance[0]);
	}
	private static void testListOperationsAdd()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.mValue.Add("x");
		instance.mValue.Add("y");
		assertEqual(2, instance.Count);
	}
	private static void testListOperationsRemove()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("a");
		instance.add("b");
		instance.add("c");
		instance.mValue.Remove("b");
		assertEqual(2, instance.Count);
		assertEqual("a", instance[0]);
		assertEqual("c", instance[1]);
	}
	private static void testListOperationsClear()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("a");
		instance.add("b");
		instance.mValue.Clear();
		assertEqual(0, instance.Count);
	}
	private static void testListOperationsContains()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("a");
		instance.add("b");
		assertTrue(instance.mValue.Contains("a"));
		assertFalse(instance.mValue.Contains("z"));
	}
	private static void testListOperationsIndexOf()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("x");
		instance.add("y");
		assertEqual(0, instance.mValue.IndexOf("x"));
		assertEqual(1, instance.mValue.IndexOf("y"));
		assertEqual(-1, instance.mValue.IndexOf("z"));
	}
	private static void testListOperationsInsert()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("a");
		instance.add("c");
		instance.mValue.Insert(1, "b");
		assertEqual(3, instance.Count);
		assertEqual("a", instance[0]);
		assertEqual("b", instance[1]);
		assertEqual("c", instance[2]);
	}
	private static void testListOperationsRemoveAt()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("a");
		instance.add("b");
		instance.add("c");
		instance.mValue.RemoveAt(1);
		assertEqual(2, instance.Count);
		assertEqual("a", instance[0]);
		assertEqual("c", instance[1]);
	}
	private static void testListOperationsCount()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		assertEqual(0, instance.Count);
		instance.add("x");
		assertEqual(1, instance.Count);
	}
	private static void testListInCollection()
	{
		List<BIT_STRINGS> list = new List<BIT_STRINGS>();
		for (int i = 0; i < 3; i++)
		{
			BIT_STRINGS item = new BIT_STRINGS();
			item.add("item" + i);
			list.Add(item);
		}
		assertEqual(3, list.Count);
	}
	private static void testListEquality()
	{
		BIT_STRINGS a = new BIT_STRINGS();
		BIT_STRINGS b = new BIT_STRINGS();
		a.add("v"); b.add("v");
		assertEqual(a.Count, b.Count);
		assertEqual(a[0], b[0]);
	}
	private static void testListHashCode()
	{
		BIT_STRINGS a = new BIT_STRINGS();
		a.add("42");
		int hash1 = a.GetHashCode();
		int hash2 = a.GetHashCode();
		assertEqual(hash1, hash2);
	}
	private static void testListToString()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("v");
		string str = instance.mValue.ToString();
		assertTrue(str.Length > 0);
	}
	private static void testEnumerator()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("a");
		instance.add("b");
		instance.add("c");
		string concat = "";
		foreach (string s in instance)
		{
			concat += s;
		}
		assertEqual("abc", concat);
	}
	private static void testTwoInstancesIndependent()
	{
		BIT_STRINGS a = new BIT_STRINGS();
		BIT_STRINGS b = new BIT_STRINGS();
		a.add("a"); a.add("b");
		b.add("c"); b.add("d");
		assertEqual(2, a.Count);
		assertEqual(2, b.Count);
	}
	private static void testSetAfterReset()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.add("after");
		assertEqual(1, instance.Count);
	}
	private static void testMultipleResets()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("x");
		instance.resetProperty();
		assertEqual(0, instance.Count);
		instance.resetProperty();
		assertEqual(0, instance.Count);
	}
	private static void testLargeList()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		for (int i = 0; i < 50; i++)
		{
			instance.add("str" + i);
		}
		assertEqual(50, instance.Count);
	}
	private static void testEmptyList()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		assertEqual(0, instance.Count);
	}
	private static void testSingleElementList()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("hello");
		assertEqual(1, instance.Count);
		assertEqual("hello", instance[0]);
	}
	private static void testDuplicateValues()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("same");
		instance.add("same");
		assertEqual(2, instance.Count);
	}
	private static void testNullStringInList()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add(null);
		assertEqual(1, instance.Count);
		assertNull(instance[0]);
	}
	private static void testEmptyStringInList()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		instance.add("");
		assertEqual(1, instance.Count);
		assertEqual("", instance[0]);
	}
	private static void testLongStringInList()
	{
		BIT_STRINGS instance = new BIT_STRINGS();
		string longStr = new string('x', 10000);
		instance.add(longStr);
		assertEqual(1, instance.Count);
		assertEqual(10000, instance[0].Length);
	}
}