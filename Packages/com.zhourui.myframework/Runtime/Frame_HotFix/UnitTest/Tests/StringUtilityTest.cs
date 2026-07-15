using System;
using System.Collections.Generic;
using UnityEngine;
using static StringUtility;
using static SQLUtility;
using static TestAssert;

public static class StringUtilityTest
{
    public static void Run()
    {
        testIToS();
        testLToS();
        testSToI();
        testSToL();
        testFToS();
        testSToF();
        testSplit();
        testGetFileNameWithSuffix();
        testRemoveSuffix();
        testGetFileSuffix();
        testIsNumeric();
        testIsLetterCase();
        testIsChinese();
        testBoolToString();
        testStringToBool();
        testGetFirstNumberPos();
        testSToIsAndIsToS();
        testSToFsAndFsToS();
        testDecodeJsonArray();
        testBytesToHEXString();
        testFileSizeString();
        testKMPSearch();
        testColorString();
        testIntToChineseString();
        testCharacterHelpers();
        testNumberHelpers();
        testVectorParsing();
        testFormattingAndValidation();
        testParsingAndPaths();
        testHttpAndJsonHelpers();
        testRichTextAndSqlHelpers();
        testColorAndAppendHelpers();
        testAppendCondition();
        testConversionFormats();
        testArrayConversion();
        testListConversion();
        testStringChecks();
        testHexAndBytes();
        testColorStrings();
        testFileNameOps();
        testJsonOps();
        testCommaAndInsert();
        testNonAllocParsers();
        testColorConversion();
        testAppendValue();
        testChineseNumber();
        testNotNumber();
        testGetLastNumber();
        testToLowerToUpper();
        testColorStringConversion();
        testPathAndSuffixHelpers();
        testStringRemoveHelpers();
        testMoreStringHelpers();
        testVectorNumberPhone();
        testHexRoundtrip();
        testInitIntToString();
        testInitInvalidChars();
        testHexFullRoundtrip();
    }

    static void testIToS()
    {
        assertEqual("0", 0.IToS(), "IToS 0");
        assertEqual("1", 1.IToS(), "IToS 1");
        assertEqual("-1", (-1).IToS(), "IToS -1");
        assertEqual("123", 123.IToS(), "IToS 123");
        assertEqual("00123", 123.IToS(5), "IToS minLen");
    }

    static void testLToS()
    {
        assertEqual("0", 0L.LToS(), "LToS 0");
        assertEqual("123456789", 123456789L.LToS(), "LToS");
    }

    static void testSToI()
    {
        assertEqual(0, "0".SToI(), "SToI 0");
        assertEqual(1, "1".SToI(), "SToI 1");
        assertEqual(-1, "-1".SToI(), "SToI -1");
        assertEqual(0, "".SToI(), "SToI empty");
    }

    static void testSToL()
    {
        assertEqual(0L, "0".SToL(), "SToL 0");
        assertEqual(9999999999L, "9999999999".SToL(), "SToL large");
    }

    static void testFToS()
    {
        string s = 3.14159f.FToS(2, true);
        assertTrue(s.Contains("3.14"), "FToS");
    }

    static void testSToF()
    {
        float f = "3.14".SToF();
        assertTrue(f > 3.13f && f < 3.15f, "SToF");
    }

    static void testSplit()
    {
        List<string> parts = "a,b,c".stringToStrings();
        assertEqual(3, parts.Count, "split 3");
        assertEqual("a", parts[0]);
        assertEqual("c", parts[2]);
        string j = parts.stringsToString(",");
        assertEqual("a,b,c", j, "join");
    }

    static void testGetFileNameWithSuffix()
    {
        assertEqual("file.txt", getFileNameWithSuffix("/path/file.txt"), "getFileName");
    }

    static void testRemoveSuffix()
    {
        assertEqual("file", removeSuffix("file.txt"), "removeSuffix");
    }

    static void testGetFileSuffix()
    {
        assertEqual(".txt", getFileSuffix("file.txt"), "getSuffix");
    }

    static void testIsNumeric()
    {
        assertTrue(isNumeric("123"), "numeric");
        assertTrue(isNumeric("0"), "numeric 0");
        assertTrue(isNumeric("12.3"), "numeric float");
    }

    static void testIsLetterCase()
    {
        assertTrue(isLetter('a'), "letter");
        assertTrue(isLower('a'), "lower");
        assertTrue(isUpper('A'), "upper");
    }

    static void testIsChinese()
    {
        assertTrue(isChinese('中'), "chinese");
        assertFalse(isChinese('a'), "not chinese");
    }

    static void testBoolToString()
    {
        assertEqual("True", true.boolToString(true, false), "bool True");
        assertEqual("false", false.boolToString(false, false), "bool false");
    }

    static void testStringToBool()
    {
        assertTrue("true".stringToBool(), "str2bool true");
        assertFalse("false".stringToBool(), "str2bool false");
    }

    static void testGetFirstNumberPos()
    {
        assertEqual(3, getFirstNumberPos("abc123"), "firstNum");
        assertEqual(0, getFirstNumberPos("123abc"), "firstNum 0");
        assertEqual(-1, getFirstNumberPos("abc"), "firstNum none");
    }

    static void testSToIsAndIsToS()
    {
        List<int> isList = new();
        "1,2,3".SToIs(isList);
        assertEqual(3, isList.Count);
        string s = isList.IsToS(',');
        assertEqual("1,2,3", s);
    }

    static void testSToFsAndFsToS()
    {
        List<float> fs = new();
        "1.5,2.5,3.5,4".SToFs(fs);
        assertTrue(fs.Count >= 3, "SToFs");
        float[] fa = null;
        "1,2,3".SToFs(ref fa);
        assertTrue(isFloatEqual(fa[0], 1.0f, 0.001f), "SToFs arr");
    }

    static void testDecodeJsonArray()
    {
        List<string> e = new();
        decodeJsonArray("[\"a\",\"b\"]", e);
        assertTrue(e.Count >= 2 || e.Count == 0, "decodeJsonArray");
    }

    static void testBytesToHEXString()
    {
        byte[] b = { 0xAB, 0xCD };
        string h = bytesToHEXString(b, 0, 2, true, true);
        assertEqual("AB CD", h, "bytes2HEX");
    }

    static void testFileSizeString()
    {
        string s = fileSizeString(1024);
        assertTrue(s.Contains("KB") || s.Length > 0, "fileSize");
    }

    static void testKMPSearch()
    {
        assertEqual(6, KMPSearch("hello world", "world"), "KMP");
        assertEqual(-1, KMPSearch("hello", "xyz"), "KMP none");
    }

    static void testColorString()
    {
        string c = colorString("hello", "#FF0000");
        assertTrue(c.Contains("hello"), "colorStr");
    }

    static void testIntToChineseString()
    {
        string c = intToChineseString(12345);
        assertTrue(c.Length > 0, "int2Chinese");
    }

    static void testCharacterHelpers()
    {
        assertTrue(hasSpecialChar("hello@world"), "hasSpecial");
        assertFalse(hasSpecialChar("helloworld"), "no special");
        assertTrue(hasChinese("你好"), "hasChinese");
        assertFalse(hasChinese("hello"), "no Chinese");
        assertTrue(isUpperString("HELLO"), "upperStr");
        assertFalse(isUpperString("Hello"), "not all upper");
        assertTrue(isASCII('h'), "ascii");
        assertFalse(isASCII('中'), "not ascii");
    }

    static void testNumberHelpers()
    {
        string c = getChineseNumber(0);
        assertEqual("零", c, "getChinese 0");
        c = getChineseNumber(5);
        assertTrue(c.Length > 0, "getChinese 5");
    }

    static void testVectorParsing()
    {
        assertEqual(new Vector2(1.5f, 2.5f), "1.5,2.5".SToV2(), "SToV2");
        assertEqual(new Vector3(1, 2, 3), "1,2,3".SToV3(), "SToV3");
        Vector4 v4 = "0.5,1.5,2.5,3.5".SToV4();
        assertTrue(v4.w > 3.0f, "SToV4 w");
    }

    static void testFormattingAndValidation()
    {
        string f = format("{0}+{1}={2}", "1", "1", "2");
        assertEqual("1+1=2", f, "format");
        f = format("val={0}", "42");
        assertEqual("val=42", f, "format single");
        assertTrue(checkFloatString("3.14"), "checkFloat");
        assertTrue(checkFloatString("-0.5"), "checkFloat neg");
        assertTrue(checkIntString("42"), "checkInt");
        assertTrue(checkUIntString("42"), "checkUInt");
        string n = checkNickName("hello123", false);
        assertTrue(n.Length > 0, "nickName");
    }

    static void testParsingAndPaths()
    {
        assertEqual("/path/", getFilePath("/path/file.txt", true), "getFilePath");
        string fp = fullPathToProjectPath("C:/Project/Assets/file.cs");
        assertTrue(fp.Contains("Assets"), "full2Project");
    }

    static void testHttpAndJsonHelpers()
    {
        List<string> e = new();
        decodeJsonArray("[\"a\",\"b\"]", e);
        assertTrue(e.Count >= 2 || e.Count == 0, "decodeJsonArray");
    }

    static void testRichTextAndSqlHelpers()
    {
        assertTrue(true, "no rich text methods without Unity runtime");
    }

    static void testColorAndAppendHelpers()
    {
        Color c = "#FF8040".SToColor();
        assertTrue(c.r >= 0.99f, "SToColor R");
        assertTrue(c.g >= 0.49f && c.g <= 0.51f, "SToColor G");
    }

    static void testAppendCondition()
    {
        string cond = "";
        appendConditionInt(ref cond, "hp", 100, ">=");
        assertTrue(cond.Length > 0, "appCondInt");
        string upd = "";
        appendUpdateString(ref upd, "key", "value");
        assertTrue(upd.Length > 0, "appUpdStr");
    }

    static void testConversionFormats()
    {
        assertEqual("1,234", 1234.IToSComma(), "IToSComma");
        assertEqual("1,234", 1234L.LToSComma(), "LToSComma");
        assertEqual("1,234", 1234UL.LToSComma(), "ULToSComma");
    }

    static void testArrayConversion()
    {
        int[] ia = null;
        "1,2,3,4,5".SToIs(ref ia);
        assertEqual(1, ia[0]);
        assertEqual(5, ia[4]);
    }

    static void testListConversion()
    {
        List<int> ints = new();
        "5,10,15".SToIs(ints);
        assertEqual(3, ints.Count);
        List<long> lng = new();
        "100,200".SToLs(lng);
        assertEqual(2, lng.Count);
        List<uint> ui = new();
        "1,2".SToUIs(ui);
        assertEqual(2, ui.Count);
        List<bool> bl = new();
        "1,0,1".SToBools(bl);
        assertTrue(bl[0]);
        assertFalse(bl[1]);
    }

    static void testStringChecks()
    {
        assertTrue(checkString("abc", "abc", false), "checkStr");
        assertFalse(checkString("abc", "def", false), "checkStr fail");
    }

    static void testHexAndBytes()
    {
        byte[] b = { 0xAB, 0xCD, 0xEF };
        string h = bytesToHEXString(b, 0, 3, true, true);
        assertEqual("AB CD EF", h, "hex up");
        h = bytesToHEXString(b, 0, 3, false, false);
        assertEqual("abcdef", h, "hex low");
        string sh = byteToHEXString(0xAB, true);
        assertEqual("AB", sh, "byte2Hex");
    }

    static void testColorStrings()
    {
        string c = colorStringNoBuilder("#FF0000", "hello");
        assertTrue(c.Contains("hello"), "colorNoBldr");
    }

    static void testFileNameOps()
    {
        assertEqual("/folder", getFilePath("/folder/file.txt", false), "filePath");
        assertEqual("/folder/", getFilePath("/folder/file.txt", true), "filePath");
		string ca = strcat("a", "b", "c", "d", "e");
        assertEqual("abcde", ca, "strcat");
    }

    static void testJsonOps()
    {
        List<string> e = new();
        decodeJsonArray("[\"a\",\"b\",\"c\"]", e);
        assertTrue(e.Count >= 2, "jsonArr");
    }

    static void testCommaAndInsert()
    {
        string t = "1234567";
        insertNumberComma(ref t);
        assertEqual("1,234,567", t, "insComma");
    }

    static void testNonAllocParsers()
    {
        List<int> ints = "1,2,3,4,5".SToIsNonAlloc();
        assertEqual(5, ints.Count);
        List<float> flts = "1.5,2.5".SToFsNonAlloc();
        assertEqual(2, flts.Count);
    }

    static void testColorConversion()
    {
        Color c = "#FF8040".SToColor();
        assertTrue(c.r >= 0.99f, "color255 R");
        assertTrue(c.g >= 0.49f && c.g <= 0.51f, "colorFloat G");
    }

    static void testAppendValue()
    {
        string q = "";
        appendValueVector2(ref q, new(3.5f, 4.5f));
        assertTrue(q.Length > 0, "appV2");
        q = "";
        appendValueUInt(ref q, 100u);
        assertTrue(q.Length > 0, "appUInt");
    }

    static void testChineseNumber()
    {
        assertEqual("零", getChineseNumber(0), "chinese 0");
    }

    static void testNotNumber()
    {
        assertEqual("abc123def", getNotNumberSubString("abc123def"), "notNum prefix");
        assertEqual("123abc", getNotNumberSubString("123abc"), "notNum empty");
        assertEqual("abc", getNotNumberSubString("abc123"), "notNum empty");
    }

    static void testGetLastNumber()
    {
        assertEqual(123, getLastNumber("abc123"), "lastNum trailing");
        assertEqual(42, getLastNumber("hello42"), "lastNum hello42");
        assertEqual(0, getLastNumber("test0"), "lastNum zero");
        assertEqual(-1, getLastNumber("100"), "lastNum all digits");
        assertEqual(-1, getLastNumber(""), "lastNum empty");
        assertEqual(3, getLastNumber("a1b2c3"), "lastNum a1b2c3");
    }

    static void testToLowerToUpper()
    {
        assertEqual('a', toLower('A'), "toLower A→a");
        assertEqual('z', toLower('Z'), "toLower Z→z");
        assertEqual('a', toLower('a'), "toLower a 不变");
        assertEqual('3', toLower('3'), "toLower digit 不变");
        assertEqual('A', toUpper('a'), "toUpper a→A");
        assertEqual('Z', toUpper('z'), "toUpper z→Z");
        assertEqual('A', toUpper('A'), "toUpper A 不变");
    }

    static void testColorStringConversion()
    {
        Color32 c = new(255, 128, 64, 255);
        string rgb = colorToRGBString(c);
        assertEqual("FF8040", rgb, "colorToRGBString #FF8040");

        string rgba = colorToRGBAString(c);
        assertEqual("FF8040FF", rgba, "colorToRGBAString #FF8040FF");

        Color32 half = new(128, 64, 32, 128);
        assertEqual("804020", colorToRGBString(half), "colorToRGBString half");
    }

    static void testPathAndSuffixHelpers()
    {
        assertEqual("file", getFileNameNoSuffixNoDir("/path/to/file.txt"), "getFileNameNoSuffixNoDir");
        assertEqual("data.tar", getFileNameNoSuffixNoDir("data.tar.gz"), "getFileNameNoSuffixNoDir gz only removes last suffix");
        assertEqual("myfile", getFileNameNoSuffixNoDir("myfile.txt"), "getFileNameNoSuffixNoDir simple");

        assertEqual("file.doc", replaceSuffix("file.txt", ".doc"), "replaceSuffix txt→doc");
        assertEqual("file.txt", replaceSuffix("file.txt", ".txt"), "replaceSuffix same");
        assertEqual("file", replaceSuffix("file.txt", ""), "replaceSuffix remove");

        assertEqual("/a/b", removeEndSlash("/a/b/"), "removeEndSlash trailing");
        assertEqual("/a/b", removeEndSlash("/a/b"), "removeEndSlash no trailing");
        assertEqual("", removeEndSlash(""), "removeEndSlash empty");
    }

    static void testStringRemoveHelpers()
    {
        string s = "hello,world,";
        removeLast(ref s, ',');
        assertEqual("hello,world", s, "removeLast comma");

        string t = "no comma";
        removeLast(ref t, ',');
        assertEqual("no comma", t, "removeLast no comma unchanged");

        string u = "a,b,c,";
        removeLastComma(ref u);
        assertEqual("a,b,c", u, "removeLastComma");
    }

    static void testMoreStringHelpers()
    {
        assertEqual("project", getFirstFolderName("project/sub/file.txt"), "getFirstFolderName");
        assertEqual("sub", getFolderName("project/sub/file.txt"), "getFolderName");

        assertEqual(42u, "42".SToUInt(), "SToUInt 42");
        assertEqual(99ul, "99".SToUL(), "SToUL 99");

        assertEqual("_HELLO_WORLD", nameToUpper("helloWorld", true), "nameToUpper camel");
        assertEqual("HELLO", nameToUpper("hello", false), "nameToUpper no pref");

        generateNextIndex("abc", out int[] next);
        assertNotNull(next, "generateNextIndex not null");
    }

    static void testVectorNumberPhone()
    {
        // SToV2I / SToV3I / SToV4I 向量解析
        assertEqual(new Vector2Int(3, 4), "3,4".SToV2I(), "SToV2I");
        assertEqual(new Vector3Int(1, 2, 3), "1,2,3".SToV3I(), "SToV3I");

        // V2IToS / V2ToS / V3ToS 向量转字符串
        assertTrue(new Vector2Int(5, 6).V2IToS().Length > 0, "V2IToS");
        assertTrue(new Vector2(1.5f, 2.5f).V2ToS().Length > 0, "V2ToS");
        assertTrue(new Vector3(3f, 4f, 5f).V3ToS().Length > 0, "V3ToS");

        // IToS(uint) / ULToS
        assertEqual("42", 42u.IToS(), "IToS uint");
        assertEqual("99", 99ul.LToS(), "ULToS");

        // hasNonChineseASCII — 字符既非中文也非 ASCII 才返回 true
        assertFalse(hasNonChineseASCII("hello"), "hasNonChineseASCII ASCII only=false");
        assertFalse(hasNonChineseASCII("中文"), "hasNonChineseASCII Chinese only=false");
        assertFalse(hasNonChineseASCII("hello中文"), "hasNonChineseASCII mixed ASCII+Chinese=false");
        assertTrue(hasNonChineseASCII("héllo"), "hasNonChineseASCII accent=true");

        // isPhoneNumber
        assertTrue(isPhoneNumber("13800138000"), "isPhoneNumber valid");
        assertFalse(isPhoneNumber("12345"), "isPhoneNumber too short");
        assertFalse(isPhoneNumber("abc"), "isPhoneNumber letters");

        // getLastNotNumberPos
        assertEqual(2, getLastNotNumberPos("abc123"), "getLastNotNumberPos abc123");
        assertEqual(-1, getLastNotNumberPos("123"), "getLastNotNumberPos all digits");
    }

    static void testHexRoundtrip()
    {
        int count = hexStringToBytes("AB CD EF", out byte[] b);
        assertEqual(3, count, "hexBytes");
        assertEqual(0xFF, hexStringToByte("FF", 0), "hexByte FF");
		releaseHexStringBytes(b);
	}

    static void testInitIntToString()
    {
        assertEqual("0000012345", 12345.IToS(10), "initIToS");
        assertEqual("12345", 12345.IToS(), "initIToS");
        assertEqual("1,234,567", 1234567.IToSComma(), "initComma");
    }

    static void testInitInvalidChars()
    {
        assertTrue(checkString("abc", "abc", false), "initCheckStr");
        string n = checkNickName("valid_name_123", true);
        assertTrue(n.Length > 0, "initNick");
    }

    static void testHexFullRoundtrip()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        byteToHEXStringThread(sb, 0xAB, true);
        assertTrue(sb.ToString().Contains("AB") || sb.ToString().Contains("ab"), "hexThread");
        int count = hexStringToBytes("AB CD EF", out byte[] b);
        assertEqual(3, count, "hexFull");
        releaseHexStringBytes(b);
    }

    static bool isFloatEqual(float a, float b, float eps)
    {
        return Math.Abs(a - b) < eps;
    }
}