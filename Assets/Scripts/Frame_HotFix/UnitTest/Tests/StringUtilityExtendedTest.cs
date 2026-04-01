#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;
using static StringUtility;
using static TestAssert;

public static class StringUtilityExtendedTest
{
    public static void Run()
    {
        testParsingAndPaths();
        testFormattingAndValidation();
        testRichTextAndSqlHelpers();
    }

    private static void testParsingAndPaths()
    {
        assertEqual(123, SToI("123"), "stoi");
        assertEqual(-456L, SToL("-456"), "stol");
        assertEqual(78u, SToUInt("78"), "stouint");
        assertEqual(new Vector2(1, 2), SToV2("1,2"), "stv2");
        assertEqual(new Vector3(3, 4, 5), SToV3("3,4,5"), "stv3");
        assertEqual("c.txt", getFileNameWithSuffix("a/b/c.txt"), "filename");
		assertEqual(".txt", getFileSuffix("a/b/c.txt"), "suffix");
        assertEqual("a/b/c", removeSuffix("a/b/c.txt"), "remove suffix");
        assertEqual("a", getFirstFolderName("a/b/c.txt"), "first folder");
        assertEqual("b", getFolderName("a/b/c.txt"), "folder");
    }

    private static void testFormattingAndValidation()
    {
        assertEqual("Hello 3", format("Hello {0}", "3"), "format one");
        assertTrue(checkFloatString("12.34"), "float string");
        assertTrue(checkIntString("12"), "int string");
        assertTrue(checkUIntString("12"), "uint string");
        assertTrue(isNumeric("12345"), "numeric string");
        assertTrue(hasChinese("中文"), "has chinese");
        assertTrue(hasNonChineseASCII("abc😊"), "has ascii");
        assertFalse(hasSpecialChar("abc123"), "no special char");
        assertTrue(getBytesLength("abc") > 0, "bytes length");
        assertTrue(generateCharWidth("W") > 0, "char width");
        assertEqual("True", boolToString(true, true, false), "bool string");
    }

    private static void testRichTextAndSqlHelpers()
    {
        List<string> parts = new() { "a", "b", "c" };
        assertEqual("a,b,c", stringsToString(parts, ','), "join");
        List<string> split = stringToStrings("a|b|c", '|');
        assertEqual(3, split.Count, "split count");
        assertEqual(1, KMPSearch("abcdef", "bcd"), "kmp search");
        assertEqual(-1, KMPSearch("abcdef", "xyz"), "kmp miss");
        string query = string.Empty;
        appendValueInt(ref query, 7);
        appendConditionString(ref query, "name", "bob", "=");
        appendUpdateInt(ref query, "level", 3);
        assertTrue(query.Length > 0, "sql helper");
        string encoded = validateHttpString("https://example.com/path?q=1");
        assertTrue(encoded.Contains("%3A"), "http validate colon");
        assertTrue(encoded.Contains("%2F"), "http validate slash");
        assertTrue(encoded.Contains("%3F"), "http validate question mark");
        assertTrue(colorString("FF0000", "x").Contains("<color=#FF0000>"), "color string");
        assertTrue(addSprite("origin", "icon", 1.0f).Contains("icon"), "sprite tag");
    }
}
#endif
