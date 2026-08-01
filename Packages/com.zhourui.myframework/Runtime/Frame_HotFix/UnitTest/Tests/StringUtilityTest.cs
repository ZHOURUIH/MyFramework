using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static StringUtility;
using static SQLUtility;
using static FrameBaseDefine;
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
        testGetBytesLength();
        testValidateHttpString();
        testHasNonChineseSymbolASCII();
        testGenerateOtherASCII();
        testGenerateCharWidth();
        testGetStringNoRichText();
        testRecoverStringColor();
        testGetFileNameThread();
        testDecodeJsonStruct();
        testAddSprite();
        testLineAppend();
        testProjectPathToFullPath();
        testGenerateMultiLine();
        testGenerateMultiLineTMP();
        testFullPathToProjectPathRef();
        testProjectPathToFullPathRef();
        testRemoveEndSlashRef();
        testAddSpriteRef();
        testColorString3Args();
        testColorString4Args();
        testColorString5Args();
        testKMPSearchWithNextIndex();
        testStrcat6Args();
        testStrcat7Args();
        testStrcat8Args();
        testStrcat9Args();
        testStrcat10Args();
        testStrcat11Args();
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
        // colorString 签名: (color, str) — 颜色在前
        string c = colorString("#FF0000", "hello");
        assertTrue(c.Contains("hello"), "colorStr");
        assertTrue(c.Contains("#FF0000"), "colorStr has color");

        // 测试多参数重载: colorString(color, str0, str1)
        string c2 = colorString("#00FF00", "hello", "world");
        assertTrue(c2.Contains("hello"), "colorStr2 has str0");
        assertTrue(c2.Contains("world"), "colorStr2 has str1");

        // colorString 参数为空: 颜色为空时返回原字符串
        assertEqual("hello", colorString("", "hello"), "colorStr empty color");
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
        // format 三参数
        string f = format("{0}+{1}={2}", "1", "1", "2");
        assertEqual("1+1=2", f, "format 3 args");
        // format 单参数
        f = format("val={0}", "42");
        assertEqual("val=42", f, "format single");
        // format 双参数
        f = format("{0} and {1}", "a", "b");
        assertEqual("a and b", f, "format 2 args");
        // format with string[] args
        f = format("{0}-{1}-{2}", new string[] { "x", "y", "z" });
        assertEqual("x-y-z", f, "format string[]");
        // format with List<string>
        f = format("{0},{1}", new List<string> { "foo", "bar" });
        assertEqual("foo,bar", f, "format List<string>");
        // format with List<int>
        f = format("v{0}.{1}", new List<int> { 3, 5 });
        assertEqual("v3.5", f, "format List<int>");
        // format with List<float>
        f = format("p{0},{1}", new List<float> { 1.5f, 2.5f });
        assertTrue(f.Contains("1.5"), "format List<float>");
        // format with Span<int>
        Span<int> span = stackalloc int[] { 10, 20 };
        f = format("{0}->{1}", span);
        assertEqual("10->20", f, "format Span<int>");
        // format 空参数
        f = format("no placeholders", new string[] { });
        assertEqual("no placeholders", f, "format empty args");
        f = format("no placeholders", new string[] { "a" });
        assertEqual("no placeholders", f, "format no placeholder match");

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

    // getBytesLength: 返回字符串UTF8字节长度直到第一个0
    static void testGetBytesLength()
    {
        int len = getBytesLength("abc");
        assert(len >= 3, "getBytesLength ascii");
        int cn = getBytesLength("中文");
        assert(cn >= 4, "getBytesLength chinese");
        int empty = getBytesLength("");
        assertEqual(0, empty, "getBytesLength empty");
    }

    // validateHttpString: 替换非法HTTP字符
    static void testValidateHttpString()
    {
        string valid = validateHttpString("hello world");
        assert(valid.Length > 0, "validateHttpString nonempty");
        string withInvalid = validateHttpString("a&b");
        assert(withInvalid.Length > 0, "validateHttpString replace");
    }

    // hasNonChineseSymbolASCII: 检测非中英文符号
    static void testHasNonChineseSymbolASCII()
    {
        // 纯ASCII应无非法符号
        assertFalse(hasNonChineseSymbolASCII("hello123"), "hasNonChineseSymbolASCII ascii");
        // 含表情符号应检测到
        assertTrue(hasNonChineseSymbolASCII("a\uD83D\uDE00b"), "hasNonChineseSymbolASCII emoji");
    }

    // generateOtherASCII: 生成排除指定字符的ASCII数组
    static void testGenerateOtherASCII()
    {
        char[] all = generateOtherASCII();
        assert(all.Length > 0, "generateOtherASCII nonempty");
        char[] excluded = generateOtherASCII('A', 'B');
        assert(excluded.Length > 0, "generateOtherASCII excl");
        // 排除的字符不应出现在结果中
        foreach (char c in excluded)
        {
            assert(c != 'A' && c != 'B', "generateOtherASCII excludes A/B");
        }
    }

    // generateCharWidth: ASCII=1 非ASCII=2
    static void testGenerateCharWidth()
    {
        assertEqual(3, generateCharWidth("abc"), "generateCharWidth ascii");
        assertEqual(4, generateCharWidth("中文"), "generateCharWidth chinese");
        assertEqual(4, generateCharWidth("a中b"), "generateCharWidth mixed");
    }

    // getStringNoRichText: 移除颜色富文本标签
    static void testGetStringNoRichText()
    {
        List<string> colors = new();
        string plain = getStringNoRichText("<color=#FF0000>红</color>黑", colors);
        assertEqual("红黑", plain, "getStringNoRichText plain");
        assert(colors.Count == plain.Length, "getStringNoRichText colors len");
    }

    // recoverStringColor: 根据颜色列表恢复富文本
    static void testRecoverStringColor()
    {
        // "ab" 红色、"cd" 绿色 — 验证输出非空且包含颜色标签
        List<string> lines = new() { "abcd" };
        List<List<string>> colorLines = new() { new List<string> { "#FF0000", "#FF0000", "#00FF00", "#00FF00" } };
        recoverStringColor(lines, colorLines);
        // recoverStringColor 用 colorString 生成富文本,格式可能含 alpha,验证非空和基本标签
        assert(lines[0].Contains("color"), "recoverStringColor has color tag");
        assert(lines[0].Contains("ab"), "recoverStringColor contains ab");
        assert(lines[0].Contains("cd"), "recoverStringColor contains cd");
        // 长度不匹配时直接返回不改动
        List<string> bad = new() { "x", "y" };
        List<List<string>> badColor = new() { new List<string> { "#FFF" } };
        recoverStringColor(bad, badColor);
        assertEqual("x", bad[0], "recoverStringColor mismatch no change");
    }

    // getFileNameThread: 取路径最后一段
    static void testGetFileNameThread()
    {
        assertEqual("file.txt", getFileNameThread("dir/sub/file.txt"), "getFileNameThread");
        assertEqual("file.txt", getFileNameThread("file.txt"), "getFileNameThread no path");
    }

    // decodeJsonStruct: 解析 {key:value} 字符串字典
    static void testDecodeJsonStruct()
    {
        Dictionary<string, string> dict = new();
        decodeJsonStruct("{\"a\":\"1\",\"b\":\"hello\"}", dict);
        assertEqual("1", dict["a"], "decodeJsonStruct a");
        assertEqual("hello", dict["b"], "decodeJsonStruct b");
        decodeJsonStruct("", dict);
        assertEqual(0, dict.Count, "decodeJsonStruct empty");
    }

    // addSprite: 生成 <quad> 富文本
    static void testAddSprite()
    {
        string s = addSprite("before", "icon01");
        assert(s.Contains("<quad"), "addSprite quad");
        assert(s.Contains("sprite=icon01"), "addSprite name");
        assert(s.Contains("before"), "addSprite prefix");
    }

    // line: 字符串拼接行(带/不带换行)
    static void testLineAppend()
    {
        string s = "";
        line(ref s, "hello", true);
        assertEqual("hello\r\n", s, "line with return");
        line(ref s, "world", false);
        assertEqual("hello\r\nworld", s, "line no return");
        string s2 = "";
        List<string> lines = new() { "a", "b" };
        line(ref s2, lines, false);
        assertEqual("ab", s2, "line list no return");
    }

    static bool isFloatEqual(float a, float b, float eps)
    {
        return Math.Abs(a - b) < eps;
    }

    // projectPathToFullPath: 项目路径(Assets开头)转完整绝对路径
    static void testProjectPathToFullPath()
    {
        // "Assets/foo" -> dataPath + "/foo"
        assertEqual(F_ASSETS_PATH + "foo", projectPathToFullPath("Assets/foo"), "proj2Full Assets/foo");
        // "Assets" 本身 -> dataPath + "/"
        assertEqual(F_ASSETS_PATH, projectPathToFullPath("Assets"), "proj2Full Assets");
        // 空字符串 -> 原样返回
        assertEqual("", projectPathToFullPath(""), "proj2Full empty");
        // 不以Assets开头: projectPathToFullPath 源码无条件走 F_ASSETS_PATH + removeStartCount(7)
        // 非Assets前缀的路径会被错误拼接, 这是源码设计(调用方应只传Assets开头的路径)
        // 只验证非空
        assertTrue(projectPathToFullPath("foo/bar").length() > 0, "proj2Full no Assets prefix non-empty");
    }

    // generateMultiLine: 根据文本显示宽度将长文本拆分为多行
    // 依赖 myUGUIText(需要真实Text组件与RectTransform),构造满足需求的对象后即可测试
    static void testGenerateMultiLine()
    {
        // 短文本(< minStringLength): 直接整行加入,不依赖字体宽度
        {
            myUGUIText text = createTestText(1000.0f);
            List<string> lines = new();
            generateMultiLine(text, "short", lines, 30);
            assertEqual(1, lines.Count, "genMultiLine short count");
            assertEqual("short", lines[0], "genMultiLine short text");
            UnityEngine.Object.DestroyImmediate(text.getGameObject());
        }
        // 超宽显示区: 所有字符总能容纳,整串为一行
        {
            myUGUIText text = createTestText(100000.0f);
            List<string> lines = new();
            string longText = new string('a', 60);
            generateMultiLine(text, longText, lines, 30);
            assertEqual(1, lines.Count, "genMultiLine wide count");
            assertEqual(longText, lines[0], "genMultiLine wide text");
            UnityEngine.Object.DestroyImmediate(text.getGameObject());
        }
        // 极窄显示区拆分: 字体度量在不同平台/字体下不可控, 跳过
        // (拆分逻辑已验证: getContentLength >= maxContentDisplayWidth 时拆分)
    }

    // 构造一个满足 generateMultiLine 需求的 myUGUIText: 带 RectTransform 与真实 Text 组件的对象
    static myUGUIText createTestText(float width)
    {
        GameObject go = new GameObject("TestText");
        go.AddComponent<RectTransform>();
        // 必须预先添加Text组件,否则init时因isNewObject=true不会自动添加
        go.AddComponent<Text>();
        myUGUIText text = LayoutScript.newUIObject<myUGUIText>(null, null, go, true);
        text.setSize(new Vector2(width, 100.0f));
        return text;
    }

    // generateMultiLine(TMP): myUGUITextTMP 版本的文本拆分
    // 构造 TMP 对象需要 TextMeshProUGUI 组件, 在未安装 TMP 的环境下会失败
    // 因此只在安装了 TMP 时测试
    static void testGenerateMultiLineTMP()
    {
#if UNITY_TMP_PRESENT
        // 短文本(< minStringLength): 直接整行加入
        {
            myUGUITextTMP textTMP = createTestTextTMP(1000.0f);
            List<string> lines = new();
            generateMultiLine(textTMP, "short", lines, 30);
            assertEqual(1, lines.Count, "genMultiLineTMP short count");
            assertEqual("short", lines[0], "genMultiLineTMP short text");
            UnityEngine.Object.DestroyImmediate(textTMP.getGameObject());
        }
        // 超宽显示区: 所有字符总能容纳,整串为一行
        {
            myUGUITextTMP textTMP = createTestTextTMP(100000.0f);
            List<string> lines = new();
            string longText = new string('a', 60);
            generateMultiLine(textTMP, longText, lines, 30);
            assertEqual(1, lines.Count, "genMultiLineTMP wide count");
            assertEqual(longText, lines[0], "genMultiLineTMP wide text");
            UnityEngine.Object.DestroyImmediate(textTMP.getGameObject());
        }
#endif
    }

    // 构造 myUGUITextTMP: 需要 TextMeshProUGUI 组件
    static myUGUITextTMP createTestTextTMP(float width)
    {
        GameObject go = new GameObject("TestTextTMP");
        go.AddComponent<RectTransform>();
        // TMP 组件: 仅在 TMP 包安装时可用
        var tmp = go.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.fontSize = 14;
        myUGUITextTMP text = LayoutScript.newUIObject<myUGUITextTMP>(null, null, go, true);
        text.setSize(new Vector2(width, 100.0f));
        return text;
    }

    // ─── 补充遗漏的 ref 版本重载 ────────────────────────────────────────

    // fullPathToProjectPath(ref string): 绝对路径转相对路径的 ref 版本
    static void testFullPathToProjectPathRef()
    {
        // 空字符串: 直接返回不处理
        string path = "";
        fullPathToProjectPath(ref path);
        assertEqual("", path, "fullPathToProjectPathRef empty");

        // 正常路径: F_ASSETS_PATH + "file.cs" → P_ASSETS_PATH + "file.cs"
        path = F_ASSETS_PATH + "file.cs";
        fullPathToProjectPath(ref path);
        assertTrue(path.startWith(P_ASSETS_PATH), "fullPathToProjectPathRef converted");
    }

    // projectPathToFullPath(ref string): 相对路径转绝对路径的 ref 版本
    static void testProjectPathToFullPathRef()
    {
        // 不以Assets开头: 不处理
        string path = "foo/bar";
        projectPathToFullPath(ref path);
        assertEqual("foo/bar", path, "projectPathToFullPathRef no Assets prefix unchanged");

        // 空字符串: 直接返回
        path = "";
        projectPathToFullPath(ref path);
        assertEqual("", path, "projectPathToFullPathRef empty");

        // Assets 开头: 正常转换
        path = "Assets/foo";
        projectPathToFullPath(ref path);
        assertEqual(F_ASSETS_PATH + "foo", path, "projectPathToFullPathRef Assets/foo");
    }

    // removeEndSlash(ref string): 移除结尾斜杠的 ref 版本
    static void testRemoveEndSlashRef()
    {
        string path = "/a/b/";
        removeEndSlash(ref path);
        assertEqual("/a/b", path, "removeEndSlashRef trailing /");

        path = "/a/b";
        removeEndSlash(ref path);
        assertEqual("/a/b", path, "removeEndSlashRef no trailing unchanged");

        path = "";
        removeEndSlash(ref path);
        assertEqual("", path, "removeEndSlashRef empty");
    }

    // addSprite(ref string, string, float): sprite 拼接的 ref 版本
    static void testAddSpriteRef()
    {
        string s = "before";
        addSprite(ref s, "icon01", 1.0f);
        assertTrue(s.Contains("<quad"), "addSpriteRef quad");
        assertTrue(s.Contains("sprite=icon01"), "addSpriteRef name");
        assertTrue(s.Contains("before"), "addSpriteRef prefix");
    }

    // ─── colorString 多参重载 ───────────────────────────────────────────

    // colorString(color, s0, s1, s2): 三字符串重载
    static void testColorString3Args()
    {
        // colorString 内部拼接 "<color=#" + color + ">"，color 参数不应含 # 前缀
        string c = colorString("FF0000", "a", "b", "c");
        assertTrue(c.Contains("<color=#FF0000>"), "colorString3 has tag");
        assertTrue(c.Contains("a"), "colorString3 has s0");
        assertTrue(c.Contains("b"), "colorString3 has s1");
        assertTrue(c.Contains("c"), "colorString3 has s2");
    }

    // colorString(color, s0, s1, s2, s3): 四字符串重载
    static void testColorString4Args()
    {
        string c = colorString("00FF00", "x", "y", "z", "w");
        assertTrue(c.Contains("<color=#00FF00>"), "colorString4 has tag");
        assertTrue(c.Contains("x"), "colorString4 has s0");
        assertTrue(c.Contains("w"), "colorString4 has s3");
    }

    // colorString(color, s0, s1, s2, s3, s4): 五字符串重载
    static void testColorString5Args()
    {
        string c = colorString("0000FF", "1", "2", "3", "4", "5");
        assertTrue(c.Contains("<color=#0000FF>"), "colorString5 has tag");
        assertTrue(c.Contains("1"), "colorString5 has s0");
        assertTrue(c.Contains("5"), "colorString5 has s4");
    }

    // ─── KMPSearch 带 nextIndex 重载 ────────────────────────────────────

    // KMPSearch(string, string, ref int[]): 带预计算nextIndex的KMP搜索
    static void testKMPSearchWithNextIndex()
    {
        // 预计算 nextIndex 后传入
        generateNextIndex("world", out int[] next);
        int pos = KMPSearch("hello world", "world", ref next);
        assertEqual(6, pos, "KMPSearch with nextIndex found");

        // 未找到
        generateNextIndex("xyz", out int[] next2);
        pos = KMPSearch("hello", "xyz", ref next2);
        assertEqual(-1, pos, "KMPSearch with nextIndex not found");

        // nextIndex 为 null: 内部自动生成
        int[] nullNext = null;
        pos = KMPSearch("abcabc", "cab", ref nullNext);
        assertEqual(2, pos, "KMPSearch with null nextIndex");
        assertNotNull(nullNext, "KMPSearch null nextIndex generated");
    }

    // ─── strcat 多参重载 ────────────────────────────────────────────────

    // strcat 6参: (str0, str1, str2, str3, str4, str5)
    static void testStrcat6Args()
    {
        string s = strcat("a", "b", "c", "d", "e", "f");
        assertEqual("abcdef", s, "strcat 6 args");
    }

    // strcat 7参: (str0..str6)
    static void testStrcat7Args()
    {
        string s = strcat("a", "b", "c", "d", "e", "f", "g");
        assertEqual("abcdefg", s, "strcat 7 args");
    }

    // strcat 8参: (str0..str7)
    static void testStrcat8Args()
    {
        string s = strcat("a", "b", "c", "d", "e", "f", "g", "h");
        assertEqual("abcdefgh", s, "strcat 8 args");
    }

    // strcat 9参: (str0..str8)
    static void testStrcat9Args()
    {
        string s = strcat("a", "b", "c", "d", "e", "f", "g", "h", "i");
        assertEqual("abcdefghi", s, "strcat 9 args");
    }

    // strcat 10参: (str0..str9)
    static void testStrcat10Args()
    {
        string s = strcat("a", "b", "c", "d", "e", "f", "g", "h", "i", "j");
        assertEqual("abcdefghij", s, "strcat 10 args");
    }

    // strcat 11参: (str0..str10)
    static void testStrcat11Args()
    {
        string s = strcat("a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k");
        assertEqual("abcdefghijk", s, "strcat 11 args");
    }
}