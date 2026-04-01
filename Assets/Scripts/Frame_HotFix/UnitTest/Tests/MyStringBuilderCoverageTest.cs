#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;
using static StringUtility;

// Additional coverage for MyStringBuilder.
public static class MyStringBuilderCoverageTest
{
	public static void Run()
	{
		testAddVariants();
		testAddIfAndRepeat();
		testInsertReplaceRemove();
		testFindFunctions();
		testJsonHelpers();
		testPathHelpers();
		testColorAndValueHelpers();
		testComplexSequence();
	}

	private static void testAddVariants()
	{
		var builder = new MyStringBuilder();
		builder.add("a");
		builder.add('b');
		builder.add(1);
		builder.add((uint)2);
		builder.add(true);
		builder.add(false);
		builder.add(3L);
		builder.add(4UL);
		builder.add(new Vector2(1, 2), 0);
		builder.add(new Vector3(3, 4, 5), 0);
		builder.add(new Color32(0x12, 0x34, 0x56, 0x78));
		assertTrue(builder.ToString().Contains("ab1"), "add variants should append primitive values");
		assertTrue(builder.ToString().Contains("true"), "add variants should append bool text");
		assertTrue(builder.ToString().Contains("1,2"), "add variants should append vector2");
		assertTrue(builder.ToString().Contains("3,4,5"), "add variants should append vector3");
	}

	private static void testAddIfAndRepeat()
	{
		var builder = new MyStringBuilder();
		builder.add("x");
		builder.addIf("y", true);
		builder.addIf("z", false);
		builder.addRepeat("!", 3);
		builder.addLine("line1");
		builder.addLine("line2", "tail");
		assertEqual("xy!!!line1\r\nline2tail\r\n", builder.ToString(), "addIf/addRepeat/addLine");
	}

	private static void testInsertReplaceRemove()
	{
		var builder = new MyStringBuilder();
		builder.add("abcdef");
		builder.insert(3, "_");
		assertEqual("abc_def", builder.ToString(), "insert should work");

		builder.replace('c', 'C');
		assertEqual("abC_def", builder.ToString(), "replace char should work");

		builder.replace("ab", "AB");
		assertEqual("ABC_def", builder.ToString(), "replace string should work");

		builder.remove(3, 1);
		assertEqual("ABCdef", builder.ToString(), "remove should delete the requested range");

		builder.removeLast('f');
		assertEqual("ABCde", builder.ToString(), "removeLast should delete last matching char");
	}

	private static void testFindFunctions()
	{
		var builder = new MyStringBuilder();
		builder.add("Hello World Hello");
		assertEqual(0, builder.findFirstSubstr("Hello"), "findFirstSubstr should find from front");
		assertEqual(12, builder.findFirstSubstr("Hello", 1), "findFirstSubstr with startPos should skip first match");
		assertEqual(5, builder.findFirstSubstr(" ", 0), "findFirstSubstr char should find spaces");
		assertEqual(5, builder.findFirstSubstr("Hello", 0, true), "findFirstSubstr returnEndIndex should return end");
		assertEqual(0, builder.findFirstSubstr("hello", 0, false, false), "findFirstSubstr should support insensitive search");
		assertEqual(16, builder.lastIndexOf('o'), "lastIndexOf should scan from end");
		assertEqual(4, builder.indexOf('o'), "indexOf should scan from start");
		assertTrue(builder.endWith('o'), "endWith should accept the last char");
		assertFalse(builder.endWith('x'), "endWith should reject a different char");
	}

	private static void testJsonHelpers()
	{
		var builder = new MyStringBuilder();
		builder.jsonStartStruct("root", 0, true);
		builder.jsonAddPair("id", "1001", 1, true);
		builder.jsonAddObject("meta", "{\"a\":1}", 1, true);
		builder.jsonEndStruct(true, 0, true);
		string text = builder.ToString();
		assertTrue(text.Contains("\"root\""), "json helpers should include struct name");
		assertTrue(text.Contains("\"id\": \"1001\""), "json helpers should include string pair");
		assertTrue(text.Contains("\"meta\""), "json helpers should include object pair");
		assertTrue(text.Contains("}"), "json helpers should close struct");

		builder.clear();
		builder.jsonStartArray("items", 0, true);
		builder.add("1,").add("2,");
		builder.jsonEndArray(0, true);
		assertTrue(builder.ToString().Contains("\"items\""), "json array should include name");
	}

	private static void testPathHelpers()
	{
		var builder = new MyStringBuilder();
		builder.add(@"a\b\c");
		builder.rightToLeft();
		assertEqual("a/b/c", builder.ToString(), "rightToLeft should normalize slashes");

		builder.leftToRight();
		assertEqual("a\\b\\c", builder.ToString(), "leftToRight should normalize slashes");
	}

	private static void testColorAndValueHelpers()
	{
		var builder = new MyStringBuilder();
		builder.colorString("FF0000", "red");
		assertEqual("<color=#FF0000>red</color>", builder.ToString(), "colorString should wrap text");

		builder.clear();
		builder.colorString("00FF00", "a", "b", "c");
		assertEqual("<color=#00FF00>abc</color>", builder.ToString(), "colorString with multiple parts should wrap text");

		builder.clear();
		builder.addValueString("hero");
		builder.addValueInt(7);
		builder.addValueFloat(1.5f);
		builder.addValueUInt(9);
		assertEqual("\"hero\",7,1.5,9,", builder.ToString(), "value helpers should append comma-separated fragments");
	}

	private static void testComplexSequence()
	{
		var builder = new MyStringBuilder();
		builder.add("A");
		builder.addIf("-", true);
		builder.addRepeat("B", 2);
		builder.insertFront("X", "Y", "Z");
		builder.replaceAll("B", "C");
		builder.addLine("tail");
		string text = builder.ToString();
		assertTrue(text.StartsWith("XYZ"), "complex sequence should insert at front");
		assertTrue(text.Contains("CC"), "complex sequence should replace all matches");
		assertTrue(text.Contains("tail"), "complex sequence should keep appended line");
	}
}
#endif
