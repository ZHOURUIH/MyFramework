#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using static StringUtility;
using static TestAssert;

// Additional StringUtility coverage for parsing and formatting helpers.
public static class StringUtilityCoverageTest
{
	public static void Run()
	{
		testNumberHelpers();
		testPathHelpers();
		testVectorParsing();
		testCharacterHelpers();
		testHttpAndJsonHelpers();
		testColorAndAppendHelpers();
	}

	private static void testNumberHelpers()
	{
		assertEqual(2, getLastNotNumberPos("abc123"), "getLastNotNumberPos should stop at the last non-digit");
		assertEqual(123, getLastNumber("abc123"), "getLastNumber should parse trailing digits");
		assertEqual(0, getLastNumber("abcdef"), "getLastNumber should return 0 when no trailing digits exist");

		string removed = "a,b,c,";
		removeLast(ref removed, ',');
		assertEqual("a,b,c", removed, "removeLast should remove the final delimiter");

		assertEqual("folder/file", removeEndSlash("folder/file/"), "removeEndSlash should trim trailing slash");
		assertEqual("data.bin", replaceSuffix("data.txt", ".bin"), "replaceSuffix should swap the suffix");
	}

	private static void testPathHelpers()
	{
		assertEqual("a", getFirstFolderName("a/b/c"), "getFirstFolderName should return the first folder");
		assertEqual("b", getFolderName("a/b/c.txt"), "getFolderName should return the last folder name");
		assertEqual("c", getFileNameNoSuffixNoDir("a/b/c.txt"), "getFileNameNoSuffixNoDir should remove folder and suffix");
		assertEqual("c.txt", getFileNameWithSuffix("a/b/c.txt"), "getFileNameWithSuffix should keep the file name");
		assertEqual(".txt", getFileSuffix("a/b/c.txt"), "getFileSuffix should return the extension");
	}

	private static void testVectorParsing()
	{
		Vector2 v2 = SToV2("1.5,2.5");
		assertEqual(new Vector2(1.5f, 2.5f), v2, "SToV2 should parse Vector2");

		Vector2Int v2i = SToV2I("7,8");
		assertEqual(new Vector2Int(7, 8), v2i, "SToV2I should parse Vector2Int");

		Vector3 v3 = SToV3("1,2,3");
		assertEqual(new Vector3(1, 2, 3), v3, "SToV3 should parse Vector3");

		Vector3Int v3i = SToV3I("7,8,9");
		assertEqual(new Vector3Int(7, 8, 9), v3i, "SToV3I should parse Vector3Int");

		Vector4 v4 = SToV4("1,2,3,4");
		assertEqual(new Vector4(1, 2, 3, 4), v4, "SToV4 should parse Vector4");

		Vector4Int v4i = SToV4I("7,8,9,10");
		assertEqual(new Vector4Int(7, 8, 9, 10), v4i, "SToV4I should parse Vector4Int");
	}

	private static void testCharacterHelpers()
	{
		assertTrue(isUpperString("ABC"), "isUpperString should accept uppercase text");
		assertFalse(isUpperString("AbC"), "isUpperString should reject mixed case");

		assertEqual('a', toLower('A'), "toLower should convert upper to lower");
		assertEqual('Z', toUpper('z'), "toUpper should convert lower to upper");

		assertTrue(isASCII('~'), "isASCII should accept ASCII");
		assertTrue(isChinese('\u4E2D'), "isChinese should accept Chinese characters");
		assertTrue(hasChinese("abc\u4E2D\u6587"), "hasChinese should detect Chinese text");
		assertFalse(hasNonChineseASCII("abc123"), "hasNonChineseASCII should allow ASCII only");
		assertFalse(hasNonChineseSymbolASCII("hello\uFF0Cworld"), "hasNonChineseSymbolASCII should allow Chinese punctuation");
		assertTrue(hasNonChineseSymbolASCII("hello\uD83D\uDE0A"), "hasNonChineseSymbolASCII should reject non-ASCII symbols");

		assertEqual(3, generateCharWidth("a\u4E2D"), "generateCharWidth should count Chinese characters as width 2");
		assertTrue(isPhoneNumber("13800138000"), "isPhoneNumber should accept a valid number");
		assertFalse(isPhoneNumber("23800138000"), "isPhoneNumber should require prefix 1");
	}

	private static void testHttpAndJsonHelpers()
	{
		string encoded = validateHttpString("a b/c?d=e");
		assertTrue(encoded.Contains("%20"), "validateHttpString should encode spaces");
		assertTrue(encoded.Contains("%2F"), "validateHttpString should encode slashes");
		assertTrue(encoded.Contains("%3F"), "validateHttpString should encode question marks");

		assertEqual("PLAYER_2_VALUE", nameToUpper("Player2Value", false), "nameToUpper should split camel case and digits");

		Dictionary<string, string> values = new();
		decodeJsonStruct("{\"id\":\"1001\",\"name\":\"hero\"}", values);
		assertEqual(2, values.Count, "decodeJsonStruct should parse key/value pairs");
		assertEqual("1001", values["id"], "decodeJsonStruct should parse id");
		assertEqual("hero", values["name"], "decodeJsonStruct should parse name");

		char[] ascii = generateOtherASCII('A', 'B');
		assertEqual(253, ascii.Length, "generateOtherASCII should exclude requested characters");
		assertFalse(Array.IndexOf(ascii, 'A') >= 0, "generateOtherASCII should exclude A");
		assertFalse(Array.IndexOf(ascii, 'B') >= 0, "generateOtherASCII should exclude B");
	}

	private static void testColorAndAppendHelpers()
	{
		Color32 color = new(0x12, 0x34, 0xAB, 0xCD);
		assertEqual("1234AB", colorToRGBString(color), "colorToRGBString should emit uppercase hex");
		assertEqual("1234ABCD", colorToRGBAString(color), "colorToRGBAString should emit uppercase hex with alpha");

		string query = "";
		appendValueString(ref query, "hero");
		appendValueInt(ref query, 42);
		appendValueFloat(ref query, 1.5f);
		assertEqual("\"hero\",42,1.5,", query, "appendValue helpers should append CSV-like fragments");
	}
}
#endif
