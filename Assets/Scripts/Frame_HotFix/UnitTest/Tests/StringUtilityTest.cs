#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static StringUtility;
using static TestAssert;

// StringUtility 字符串工具函数测试
// 覆盖：IToS / LToS / SToI / SToL / FToS / SToF / split /
//        getFileNameWithSuffix / removeSuffix / getFileSuffix /
//        isNumeric / isLetter / isLower / isUpper / isChinese /
//        boolToString / stringToBool / getFirstNumberPos /
//        SToIs / IsToS / SToFs / FsToS / decodeJsonArray /
//        bytesToHEXString / hexStringToByte / fileSizeString /
//        KMPSearch / colorString / intToChineseString
public static class StringUtilityTest
{
    public static void Run()
    {
        try
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
            Console.WriteLine("StringUtilityTest: All tests passed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"StringUtilityTest: Test failed - {ex.Message}");
            throw;
        }
    }

    // ─── IToS ────────────────────────────────────────────────────────────────
    private static void testIToS()
    {
        assertEqual("0",    IToS(0),          "IToS 0");
        assertEqual("1",    IToS(1),          "IToS 1");
        assertEqual("-1",   IToS(-1),         "IToS -1");
        assertEqual("123",  IToS(123),        "IToS 123");
        assertEqual("9999", IToS(9999),       "IToS 9999");
    }

    // ─── LToS ────────────────────────────────────────────────────────────────
    private static void testLToS()
    {
        assertEqual("0",    LToS(0L),         "LToS 0");
        assertEqual("123456789", LToS(123456789L), "LToS 123456789");
        assertEqual("-999999999", LToS(-999999999L), "LToS -999999999");
    }

    // ─── SToI ────────────────────────────────────────────────────────────────
    private static void testSToI()
    {
        assertEqual(0,      SToI("0"),        "SToI 0");
        assertEqual(1,      SToI("1"),        "SToI 1");
        assertEqual(-1,     SToI("-1"),       "SToI -1");
        assertEqual(123,    SToI("123"),      "SToI 123");
        assertEqual(0,      SToI(""),         "SToI 空字符串 → 0");
        try { SToI(null); assert(false, "SToI null 应抛异常"); } catch { }
    }

    // ─── SToL ────────────────────────────────────────────────────────────────
    private static void testSToL()
    {
        assertEqual(0L,     SToL("0"),        "SToL 0");
        assertEqual(123456789L, SToL("123456789"), "SToL 123456789");
        assertEqual(-999999999L, SToL("-999999999"), "SToL -999999999");
    }

    // ─── FToS ────────────────────────────────────────────────────────────────
    private static void testFToS()
    {
        assertEqual("0",    FToS(0f),         "FToS 0");
        assertEqual("1.5",  FToS(1.5f),       "FToS 1.5");
        assertEqual("-3.14", FToS(-3.14f),    "FToS -3.14");
        assertEqual("0.001", FToS(0.001f),    "FToS 0.001");
    }

    // ─── SToF ────────────────────────────────────────────────────────────────
    private static void testSToF()
    {
        assertEqual(0f,     SToF("0"),        "SToF 0");
        assertEqual(1.5f,   SToF("1.5"),      "SToF 1.5");
        assertEqual(-3.14f, SToF("-3.14"),    "SToF -3.14");
    }

    // ─── split ───────────────────────────────────────────────────────────────
    private static void testSplit()
    {
        string str = "a,b,c,d";
        string[] list = str.split(',');
        assertEqual(4, list.Length, "split count=4");
        assertEqual("a", list[0], "split[0]=a");
        assertEqual("d", list[3], "split[3]=d");

		// 空字符串
		string[] empty = "".split(',');
        assertEqual(0, empty.Length, "split 空字符串 → 空列表");

		// 无分隔符
		string[] noDelim = "abc".split(',');
        assertEqual(1, noDelim.Length, "split 无分隔符 → 1个元素");
        assertEqual("abc", noDelim[0], "split 无分隔符内容正确");
    }

    // ─── getFileNameWithSuffix ───────────────────────────────────────────────
    private static void testGetFileNameWithSuffix()
    {
        assertEqual("file.txt", getFileNameWithSuffix("/path/to/file.txt"), "getFileNameWithSuffix 带路径");
        assertEqual("file.txt", getFileNameWithSuffix("file.txt"), "getFileNameWithSuffix 无路径");
        assertEqual("", getFileNameWithSuffix(""), "getFileNameWithSuffix 空字符串");
        assertEqual("", getFileNameWithSuffix(null), "getFileNameWithSuffix null");
    }

    // ─── removeSuffix ────────────────────────────────────────────────────────
    private static void testRemoveSuffix()
    {
        assertEqual("file", removeSuffix("file.txt"), "removeSuffix .txt");
        assertEqual("archive.tar", removeSuffix("archive.tar.gz"), "removeSuffix .tar.gz");
        assertEqual("noext", removeSuffix("noext"), "removeSuffix 无后缀");
        assertEqual("", removeSuffix(""), "removeSuffix 空字符串");
    }

    // ─── getFileSuffix ───────────────────────────────────────────────────────
    private static void testGetFileSuffix()
    {
        assertEqual(".txt", getFileSuffix("file.txt"), "getFileSuffix .txt");
        assertEqual(".tar.gz", getFileSuffix("archive.tar.gz"), "getFileSuffix .tar.gz");
        assertEqual("", getFileSuffix("noext"), "getFileSuffix 无后缀");
        assertEqual("", getFileSuffix(""), "getFileSuffix 空字符串");
    }

    // ─── isNumeric ───────────────────────────────────────────────────────────
    private static void testIsNumeric()
    {
        assert(isNumeric('0'), "isNumeric 0");
        assert(isNumeric('9'), "isNumeric 9");
        assert(!isNumeric('a'), "isNumeric a → false");
        assert(!isNumeric(' '), "isNumeric 空格 → false");
    }

    // ─── isLetter / isLower / isUpper ────────────────────────────────────────
    private static void testIsLetterCase()
    {
        assert(isLetter('a'), "isLetter a");
        assert(isLetter('Z'), "isLetter Z");
        assert(!isLetter('0'), "isLetter 0 → false");
        assert(isLower('a'), "isLower a");
        assert(!isLower('A'), "isLower A → false");
        assert(isUpper('A'), "isUpper A");
        assert(!isUpper('a'), "isUpper a → false");
    }

    // ─── isChinese ───────────────────────────────────────────────────────────
    private static void testIsChinese()
    {
        assert(isChinese('中'), "isChinese 中");
        assert(isChinese('文'), "isChinese 文");
        assert(!isChinese('a'), "isChinese a → false");
        assert(!isChinese('0'), "isChinese 0 → false");
    }

    // ─── boolToString / stringToBool ─────────────────────────────────────────
    private static void testBoolToString()
    {
        assertEqual("true", boolToString(true), "boolToString true");
        assertEqual("false", boolToString(false), "boolToString false");
    }

    private static void testStringToBool()
    {
        assert(stringToBool("True"), "stringToBool True");
        assert(stringToBool("true"), "stringToBool true");
        assert(!stringToBool("False"), "stringToBool False");
        assert(!stringToBool("false"), "stringToBool false");
        assert(!stringToBool("abc"), "stringToBool 无效 → false");
    }

    // ─── getFirstNumberPos ───────────────────────────────────────────────────
    private static void testGetFirstNumberPos()
    {
        assertEqual(3, getFirstNumberPos("abc123def"), "getFirstNumberPos abc123def → 5");
        assertEqual(-1, getFirstNumberPos("abcdef"), "getFirstNumberPos 无数字 → -1");
        assertEqual(0, getFirstNumberPos("123abc"), "getFirstNumberPos 123abc → 0");
    }

    // ─── SToIs / IsToS ───────────────────────────────────────────────────────
    private static void testSToIsAndIsToS()
    {
        string str = "1,2,3,4,5";
        var list = SToIs(str);
        assertEqual(5, list.Count, "SToIs count=5");
        assertEqual(1, list[0], "SToIs[0]=1");
        assertEqual(5, list[4], "SToIs[4]=5");

        string back = IsToS(list);
        assertEqual("1,2,3,4,5", back, "IsToS 还原");
    }

    // ─── SToFs / FsToS ───────────────────────────────────────────────────────
    private static void testSToFsAndFsToS()
    {
        string str = "1.1,2.2,3.3";
        var list = SToFs(str);
        assertEqual(3, list.Count, "SToFs count=3");
        assertEqual(1.1f, list[0], "SToFs[0]=1.1");
        assertEqual(3.3f, list[2], "SToFs[2]=3.3");

        string back = FsToS(list);
        assertEqual("1.1,2.2,3.3", back, "FsToS 还原");
    }

    // ─── decodeJsonArray ─────────────────────────────────────────────────────
    private static void testDecodeJsonArray()
    {
        string json = "[1,2,3,4,5]";
        List<string> list = new();
        decodeJsonArray(json, list);
        assertEqual(5, list.Count, "decodeJsonArray count=5");
        assertEqual("1", list[0], "decodeJsonArray[0]=1");
        assertEqual("5", list[4], "decodeJsonArray[4]=5");
    }

    // ─── bytesToHEXString / hexStringToByte ──────────────────────────────────
    private static void testBytesToHEXString()
    {
        byte[] bytes = { 0xAB, 0xCD, 0xEF };
        string hex = bytesToHEXString(bytes);
        assertEqual("AB CD EF", hex, "bytesToHEXString");

        int count = hexStringToBytes(hex, out byte[] back);
        assertEqual(3, count, "hexStringToByte length=3");
        assertEqual(0xAB, back[0], "hexStringToByte[0]=0xAB");
        assertEqual(0xEF, back[2], "hexStringToByte[2]=0xEF");
    }

    // ─── fileSizeString ──────────────────────────────────────────────────────
    private static void testFileSizeString()
    {
        assertEqual("0B", fileSizeString(0), "fileSizeString 0");
        assertEqual("1KB", fileSizeString(1024), "fileSizeString 1KB");
        assertEqual("1MB", fileSizeString(1024 * 1024), "fileSizeString 1MB");
        assertEqual("1GB", fileSizeString(1024 * 1024 * 1024L), "fileSizeString 1GB");
    }

    // ─── KMPSearch ───────────────────────────────────────────────────────────
    private static void testKMPSearch()
    {
        string text = "ABABDABACDABABCABAB";
        string pattern = "ABABCABAB";
        int pos = KMPSearch(text, pattern);
        assertEqual(10, pos, "KMPSearch 找到位置");

        int notFound = KMPSearch(text, "XYZ");
        assertEqual(-1, notFound, "KMPSearch 未找到");
    }

    // ─── colorString ─────────────────────────────────────────────────────────
    private static void testColorString()
    {
        string colored = colorString("red", "FF0000");
        assert(colored.Contains("red"), "colorString 包含文本");
        assert(colored.Contains("FF0000"), "colorString 包含颜色");
    }

    // ─── intToChineseString ──────────────────────────────────────────────────
    private static void testIntToChineseString()
    {
        assertEqual("1万", intToChineseString(10000), "intToChineseString 10000");
        assertEqual("1万2345", intToChineseString(12345), "intToChineseString 12345");
        assertEqual("1亿101万2345", intToChineseString(101012345), "intToChineseString 101012345");
    }
}
#endif
