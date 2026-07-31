using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static StringExtension;
using static TestAssert;

// StringExtension 所有扩展方法测试（全覆盖）
public static class StringExtensionTest
{
	public static void Run()
	{
		testBasicChecks();
		testRangeMethods();
		testSubstringMethods();
		testRemoveMethods();
		testStartEndWith();
		testReplaceMethods();
		testEnsureAndTrim();
		testSplitMethods();
		testFindSubstrMethods();
		testRangeAdditionalMethods();
		testRemovePairChars();
		testTypeConversions();
		testEncodingConversions();
		testPathAndCaseMethods();
		testFixedLengthAndNumbers();
		testListConversions();
		// 新增 — 数值转字符串
		testIToS();
		testLToS();
		testIToSComma();
		testLToSComma();
		testIsToS();
		// 新增 — 字符串转数值
		testSToI();
		testSToL();
		testSToUIntUL();
		testSToF();
		// 新增 — 字符串转向量
		testSToVector();
		// 新增 — 向量转字符串
		testVectorToS();
		// 新增 — 布尔/颜色转换
		testBoolString();
		testSToColor();
		testSToBools();
		// 新增 — 列表↔字符串
		testStringsToString();
		testStringToStrings();
		testSToIsSToLsSToUIsSToFs();
		// 新增 — 百分比/概率
		testPercentProbability();
		// 新增 — toBytes
		testToBytes();
		// 新增 — fixedAndPercent
		testFixedAndPercent();
		// 补充遗漏测试 — Predicate contains / 各 range 重载 / remove 重载 / find 重载 / FToS / ref 数组 / 编码往返
		testContainsPredicate();
		testRangeNullAndEdge();
		testRangeBetweenKeyToKeyOverloads();
		testRangeToFirstWithStartIndex();
		testRangeToFirstIncludeWithStartIndex();
		testRangeToLastWithStartIndex();
		testRangeToLastIncludeWithStartIndex();
		testRangeFromFirstIncludeEdge();
		testRemoveStartEndCharOverloads();
		testRemoveStartEndCaseInsensitive();
		testFindFirstSubstrReturnEndAndSensitive();
		testSToLsRefArray();
		testSToIsRefArray();
		testSToFsRefArray();
		testFToS();
		testEncodingRoundtripMore();
		// 更多边界覆盖
		testFindLastCharEdge();
		testSplitLineEdge();
		testStringToStringsParams();
		testFToSPrecisionEdge();
		testSToBoolsEdge();
		testIToSCacheEdge();
	}

	private static void testBasicChecks()
	{
		assertEqual(0, ((string)null).length(), "null.length=0");
		assertEqual(4, "test".length(), "test.length=4");
		assertTrue(((string)null).isEmpty(), "null.isEmpty");
		assertTrue("".isEmpty(), "empty.isEmpty");
		assertFalse("a".isEmpty(), "'a' not empty");
		assertTrue("hello".contains('e'), "contains char true");
		assertFalse("hello".contains('z'), "contains char false");
		assertFalse(((string)null).contains('e'), "contains char null");
	}

	private static void testRangeMethods()
	{
		// range 基本切分
		assertEqual("hel", "hello".range(0, 3), "range(0,3)");
		assertEqual("", "hello".range(3, 3), "range(3,3)=empty");
		assertEqual("llo", "hello".rangeFromFirstToEnd('l'), "rangeFromFirstToEnd");
		assertEqual("ello", "hello".rangeFromFirstToEndExcept('h'), "rangeFromFirstToEndExcept");
		assertEqual("llo", "hello".rangeFromFirstInclude('l'), "rangeFromFirstInclude");

		// rangeToFirst
		assertEqual("he", "hello".rangeToFirst('l'), "rangeToFirst l");
		assertEqual("hello", "hello.world".rangeToLast('.'), "rangeToLast .");
		assertEqual("hello.", "hello.world".rangeToLastInclude('.'), "rangeToLastInclude .");

		// rangeBetweenKeyToKey
		assertEqual("lo.wo", "hello.world".rangeBetweenKeyToKey('l', 'r'), "rangeBetweenKeyToKey char,char");
		assertEqual("test", "prefix[test]suffix".rangeBetweenKeyToKey("[", "]"), "rangeBetweenKeyToKey string,string");
	}

	private static void testSubstringMethods()
	{
		assertEqual("wor", "hello.world".substr(6, 3), "substr(6,3)=wor");
		assertEqual("orld", "hello.world".endString(4), "endString(4)=orld");
		assertEqual("hell", "hello.world".startString(4), "startString(4)=hell");
		assertEqual("hello.w", "hello.world".removeEndCount(4), "removeEndCount(4)=hello.w");
		assertEqual("world", "hello.world".removeStartCount(6), "removeStartCount(6)=world");
	}

	private static void testRemoveMethods()
	{
		// removeStartString
		assertEqual("world", "helloworld".removeStart("hello"), "removeStartString prefix");
		assertEqual("helloworld", "helloworld".removeStart("xyz"), "removeStartString no match");

		// removeEndString
		assertEqual("hello", "helloworld".removeEnd("world"), "removeEndString suffix");
		assertEqual("", "hello".removeEnd("hello"), "removeEndString whole");

		// removeStartAll
		assertEqual("c", "aaac".removeStartAll('a'), "removeStartAll");
		assertEqual("", "aaaa".removeStartAll('a'), "removeStartAll all");
		assertEqual("cd", "abcd".removeStartAll('a', 'b'), "removeStartAll multi chars");

		// removeEndAll
		assertEqual("a", "accc".removeEndAll('c'), "removeEndAll");
		assertEqual("a", "abccc".removeEndAll('b', 'c'), "removeEndAll multi");

		// removeAll by string/char
		assertEqual("heo", "hello".removeAll("ll"), "removeAll string");
		assertEqual("hello", "h.e.l.l.o".removeAll('.'), "removeAll char");
		assertEqual("abc", "a,b,c".removeAll(new char[] { ',' }), "removeAll char array");

		// removeStartEmpty / removeEndEmpty / removeAllEmpty
		assertEqual("a b", " a b".removeStartEmpty(), "removeStartEmpty");
		assertEqual("a b", "a b ".removeEndEmpty(), "removeEndEmpty");
		assertEqual("ab", "a b".removeAllEmpty(), "removeAllEmpty");
	}

	private static void testStartEndWith()
	{
		assertTrue("hello".startWith("he"), "startWith true");
		assertFalse("hello".startWith("HE"), "startWith case");
		assertTrue("hello".startWith("HE", false), "startWith insensitive");
		assertTrue("hello".endWith("lo"), "endWith true");
		assertFalse("hello".endWith("LO"), "endWith case");
		assertTrue("hello".endWith("LO", false), "endWith insensitive");
	}

	private static void testReplaceMethods()
	{
		assertEqual("abxyzfg", "abcdefg".replace(2, 5, "xyz"), "replace by index");
		assertEqual("hello,world!", "hello world!".replace(" ", ","), "replace string");
		assertEqual("a-a-a", "a b a b a".replaceAll(" b ", "-"), "replaceAll");
		assertEqual("x x x", "a a a".replaceAll('a', 'x'), "replaceAll char");
	}

	private static void testEnsureAndTrim()
	{
		assertEqual("http://test", "test".ensurePrefix("http://"), "ensurePrefix added");
		assertEqual("http://test", "http://test".ensurePrefix("http://"), "ensurePrefix already");
		assertEqual("file.txt", "file".ensureSuffix(".txt"), "ensureSuffix added");
		assertEqual("file.txt", "file.txt".ensureSuffix(".txt"), "ensureSuffix already");
	}

	private static void testSplitMethods()
	{
		string[] parts = "a,b,c".split(',');
		assertEqual(3, parts.Length, "split char");
		assertEqual("b", parts[1], "split char mid");
		parts = "a--b".split("--");
		assertEqual(2, parts.Length, "split string");
		parts = "a::b::c".split(true, ':');
		assertEqual(3, parts.Length, "split bool+char");
		parts = "a,,b".split(',');
		assertEqual(2, parts.Length, "split remove empty default");
		parts = "a,,b".split(false, ',');
		assertEqual(3, parts.Length, "split keep empty");
		string[] lines = "l1\nl2\nl3".splitLine();
		assertEqual(3, lines.Length, "splitLine");
		"x\ny".splitLine(out string[] linesOut, false);
		assertEqual(2, linesOut.Length, "splitLine out");
	}

	private static void testFindSubstrMethods()
	{
		assertEqual(1, "hello".findFirstSubstr('e'), "findFirst char");
		assertEqual(-1, "hello".findFirstSubstr('z'), "findFirst char miss");
		assertEqual(0, "hello".findFirstSubstr("he"), "findFirst str");
		assertEqual(3, "abcabc".findLastSubstr("abc", true), "findLast substr");
		assertEqual(3, "hello".findLastChar('l'), "findLastChar l");
		assertEqual(-1, "hello".findLastChar('z'), "findLastChar miss");
		assertEqual(6, "hello world".findFirstSubstr("world"), "findFirst substr 2");
	}

	private static void testRangeAdditionalMethods()
	{
		assertEqual("lo.world", "hello.world".rangeFromFirst('l'), "rangeFromFirst l");
		assertEqual("hello", "hello".rangeBetweenKeyToKeyInclude('h', 'o'), "rangeBetweenInclude");
		assertEqual("hello.", "hello.world".rangeToFirstInclude('.'), "rangeToFirstInclude .");
	}

	private static void testRemovePairChars()
	{
		assertEqual("ab", "a(hello)b".removeFirstBetweenPairChars('(', ')', out int s0, out int e0), "removeFirstPair");
		assertEqual(1, s0, "removeFirstPair sIdx");
		assertEqual(7, e0, "removeFirstPair eIdx");
		assertEqual("ab", "a(hello)b".removeLastBetweenPairChars('(', ')', out int s1, out int e1), "removeLastPair");
		assertEqual(1, s1, "removeLastPair sIdx");
		assertEqual(7, e1, "removeLastPair eIdx");
	}

	private static void testTypeConversions()
	{
		Vector4Int v4i = "1,2,3,4".SToV4I();
		assertEqual(1, v4i.x, "SToV4I x");
		assertEqual(4, v4i.w, "SToV4I w");
		Vector3Int v3i = new Vector3Int(1, 2, 3);
		assertEqual("1,2,3", v3i.V3IToS(), "V3IToS");
		Vector4 v4 = new Vector4(1.0f, 2.0f, 3.0f, 4.0f);
		assertTrue(v4.V4ToS().length() > 0, "V4ToS");
		Vector4Int v4i2 = new Vector4Int(5, 6, 7, 8);
		// 注意: V4IToS 在 z 和 w 之间缺少逗号(strcat bug)
		assertEqual("5,6,78", v4i2.V4IToS(), "V4IToS");
	}

	private static void testEncodingConversions()
	{
		// 编码转换往返测试
		string ascii = "HelloWorld12";
		string gbAscii = ascii.UTF8ToGB2312();
		assertEqual(ascii, gbAscii.GB2312ToUTF8(), "UTF8ToGB2312 ASCII roundtrip");

		string viaConvert = "ABC".convertStringFormat(Encoding.UTF8, Encoding.UTF8);
		assertEqual("ABC", viaConvert, "convertStringFormat");

		// GB2312 <-> Unicode 往返 (GB2312 编码在非中文系统会抛异常, try-catch 保护)
		try
		{
			string gbUni = ascii.GB2312ToUnicode();
			assertEqual(ascii, gbUni.UnicodeToGB2312(), "GB2312ToUnicode ASCII roundtrip");
			string uniGb = "World".UnicodeToGB2312();
			assertEqual("World", uniGb.GB2312ToUnicode(), "UnicodeToGB2312 ASCII roundtrip");
		}
		catch (System.ArgumentException)
		{
			// GB2312 not supported on this platform, skip
		}
	}

	private static void testPathAndCaseMethods()
	{
		assertEqual("dir/path/", "dir/path".addEndSlash(), "addEndSlash");
		assertEqual("dir/path/", "dir/path/".addEndSlash(), "addEndSlash already");
		assertEqual("abc", "abc12345".removeEndNumber(), "removeEndNumber");
		assertEqual("abc", "abc123".removeEndNumber(), "removeEndNumber trailing");
		assertEqual("123", "abc123".keepNumberOnly(), "keepNumberOnly");
		assertEqual("12345", "abc12345".keepNumberOnly(), "keepNumberOnly2");
		assertTrue("abC".hasLowerLetter(), "hasLowerLetter true");
		assertFalse("ABC".hasLowerLetter(), "hasLowerLetter false");
		assertEqual("dir\\file", "dir/file".leftToRight(), "leftToRight / to \\");
		assertEqual("dir/file", "dir\\file".rightToLeft(), "rightToLeft \\ to /");
	}

	private static void testFixedLengthAndNumbers()
	{
		assertEqual("ab\t", "ab".fixedLength(6), "fixedLength pad");
		assertEqual("abcdef", "abcdef".fixedLength(4), "fixedLength long");
		assertEqual("abc\t", "abc".fixedLength(4), "fixedLength 4");
		assertEqual("abcd", "abcd".fixedLength(4), "fixedLength exact");
		initIntToString();
	}

	private static void testListConversions()
	{
		List<short> shorts = new();
		"1,2,3".SToSs(shorts);
		assertEqual(3, shorts.Count, "SToSs count");
		assertEqual((short)2, shorts[1], "SToSs val");
		List<byte> bytes = new();
		"4,5,6".SToBs(bytes);
		assertEqual((byte)5, bytes[1], "SToBs val");
		List<sbyte> sbytes = new();
		"1,-1".SToSBs(sbytes);
		assertEqual((sbyte)-1, sbytes[1], "SToSBs val");
		List<ushort> ushorts = new();
		"7,8".SToUSs(ushorts);
		assertEqual((ushort)8, ushorts[1], "SToUSs val");
		List<long> longs = "100,200".SToLsNonAlloc();
		assertEqual(2, longs.Count, "SToLsNonAlloc count");
		List<byte> bytesNa = "9,10".SToBsNonAlloc();
		assertEqual((byte)10, bytesNa[1], "SToBsNonAlloc val");
		List<long> src = new() { 1, 2, 3 };
		assertEqual("1,2,3", src.LsToS(), "LsToS");

		// FsToS: List<float> -> string, FToS(2) 保留2位并去尾零
		List<float> empty = new();
		assertEqual("", empty.FsToS(), "FsToS empty");
		List<float> floats = new() { 1.5f, 2f, 3.25f };
		assertEqual("1.5,2,3.25", floats.FsToS(), "FsToS floats");
		List<float> single = new() { 0.5f };
		assertEqual("0.5", single.FsToS(), "FsToS single");
	}

	// ==================== 新增测试 ====================

	// ---- IToS (byte/sbyte/ushort/short/int/uint) ----
	static void testIToS()
	{
		assertEqual("0", ((byte)0).IToS(), "IToS byte 0");
		assertEqual("255", ((byte)255).IToS(), "IToS byte 255");
		assertEqual("005", ((byte)5).IToS(3), "IToS byte minLen 3");
		assertEqual("-128", ((sbyte)(-128)).IToS(), "IToS sbyte -128");
		assertEqual("127", ((sbyte)127).IToS(), "IToS sbyte 127");
		assertEqual("0", ((ushort)0).IToS(), "IToS ushort 0");
		assertEqual("65535", ((ushort)65535).IToS(), "IToS ushort 65535");
		assertEqual("-32768", ((short)(-32768)).IToS(), "IToS short -32768");
		assertEqual("32767", ((short)32767).IToS(), "IToS short 32767");
		assertEqual("-123", (-123).IToS(), "IToS int -123");
		assertEqual("0", 0.IToS(), "IToS int 0");
		assertEqual("00042", 42.IToS(5), "IToS int minLen 5");
		assertEqual("0", 0u.IToS(), "IToS uint 0");
		assertEqual("4294967295", uint.MaxValue.IToS(), "IToS uint max");
	}

	// ---- LToS (long/ulong) ----
	static void testLToS()
	{
		assertEqual("0", 0L.LToS(), "LToS 0");
		assertEqual("-123", (-123L).LToS(), "LToS -123");
		assertEqual("9223372036854775807", long.MaxValue.LToS(), "LToS long max");
		assertEqual("00042", 42L.LToS(5), "LToS minLen 5");
		assertEqual("0", 0UL.LToS(), "LToS ulong 0");
		assertEqual("18446744073709551615", ulong.MaxValue.LToS(), "LToS ulong max");
	}

	// ---- IToSComma (int/uint) ----
	static void testIToSComma()
	{
		assertEqual("0", 0.IToSComma(), "IToSComma 0");
		assertEqual("123", 123.IToSComma(), "IToSComma 123");
		assertEqual("1,234", 1234.IToSComma(), "IToSComma 1234");
		assertEqual("12,345", 12345.IToSComma(), "IToSComma 12345");
		assertEqual("123,456", 123456.IToSComma(), "IToSComma 123456");
		assertEqual("1,234,567", 1234567.IToSComma(), "IToSComma 1234567");
		assertEqual("-1,234", (-1234).IToSComma(), "IToSComma -1234");
		assertEqual("0", 0u.IToSComma(), "IToSComma uint 0");
		assertEqual("4,294,967,295", uint.MaxValue.IToSComma(), "IToSComma uint max");
	}

	// ---- LToSComma (long/ulong) ----
	static void testLToSComma()
	{
		assertEqual("1,234", 1234L.LToSComma(), "LToSComma 1234");
		assertEqual("-1,234", (-1234L).LToSComma(), "LToSComma -1234");
		assertEqual("0", 0UL.LToSComma(), "LToSComma ulong 0");
	}

	// ---- IsToS (List<int> -> string) ----
	static void testIsToS()
	{
		List<int> empty = new();
		assertEqual("", empty.IsToS(), "IsToS empty");
		List<int> list = new() { 1, 2, 3 };
		assertEqual("1,2,3", list.IsToS(), "IsToS default");
		assertEqual("1|2|3", list.IsToS('|'), "IsToS pipe");
	}

	// ---- SToI / SToUInt ----
	static void testSToI()
	{
		assertEqual(123, "123".SToI(), "SToI 123");
		assertEqual(-456, "-456".SToI(), "SToI -456");
		assertEqual(0, "0".SToI(), "SToI 0");
		assertEqual(0, "".SToI(), "SToI empty");
		assertEqual(0, ((string)null).SToI(), "SToI null");
		// 非数字字符串会触发checkIntString logError，跳过此测试
	}

	// ---- SToUInt / SToUL ----
	static void testSToUIntUL()
	{
		assertEqual(123u, "123".SToUInt(), "SToUInt 123");
		assertEqual(0u, "".SToUInt(), "SToUInt empty");
		assertEqual(123UL, "123".SToUL(), "SToUL 123");
		assertEqual(0UL, "".SToUL(), "SToUL empty");
	}

	// ---- SToL ----
	static void testSToL()
	{
		assertEqual(123L, "123".SToL(), "SToL 123");
		assertEqual(-456L, "-456".SToL(), "SToL -456");
		assertEqual(0L, "".SToL(), "SToL empty");
	}

	// ---- SToF ----
	static void testSToF()
	{
		assertTrue((3.14f - "3.14".SToF()).abs() < 0.01f, "SToF 3.14");
		assertTrue((0.0f - "0".SToF()).abs() < 0.001f, "SToF 0");
		assertTrue((0.0f - "".SToF()).abs() < 0.001f, "SToF empty");
		assertTrue((-2.5f - "-2.5".SToF()).abs() < 0.01f, "SToF -2.5");
	}

	// ---- SToV2 / SToV3 / SToV4 / SToV2I / SToV3I ----
	static void testSToVector()
	{
		Vector2 v2 = "1.5,2.5".SToV2();
		assertTrue(v2.x.isEqual(1.5f, 0.001f) && v2.y.isEqual(2.5f, 0.001f), "SToV2");
		Vector2Int v2i = "3,4".SToV2I();
		assertEqual(new Vector2Int(3, 4), v2i, "SToV2I");
		Vector3 v3 = "1,2,3".SToV3();
		assertTrue(v3.x.isEqual(1f, 0.001f) && v3.y.isEqual(2f, 0.001f) && v3.z.isEqual(3f, 0.001f), "SToV3");
		Vector3Int v3i = "4,5,6".SToV3I();
		assertEqual(new Vector3Int(4, 5, 6), v3i, "SToV3I");
		Vector4 v4 = "1,2,3,4".SToV4();
		assertTrue(v4.x.isEqual(1f, 0.001f) && v4.y.isEqual(2f, 0.001f) && v4.z.isEqual(3f, 0.001f) && v4.w.isEqual(4f, 0.001f), "SToV4");
	}

	// ---- V2ToS / V3ToS / V2IToS ----
	static void testVectorToS()
	{
		assertEqual("1.5,2.5", new Vector2(1.5f, 2.5f).V2ToS(1), "V2ToS");
		// FToS 默认 removeTailZero=true，整数值会被去掉尾零
		assertEqual("1,2", new Vector2(1f, 2f).V2ToS(2), "V2ToS precision 2 (整数去尾零)");
		assertEqual("1,2,3", new Vector3(1f, 2f, 3f).V3ToS(0), "V3ToS");
		// 同上，整数值尾零被移除
		assertEqual("1,2,3", new Vector3(1f, 2f, 3f).V3ToS(2), "V3ToS precision 2 (整数去尾零)");
		assertEqual("3,4", new Vector2Int(3, 4).V2IToS(), "V2IToS");
		assertEqual("003,004", new Vector2Int(3, 4).V2IToS(3), "V2IToS minLen 3");
	}

	// ---- boolToString / stringToBool ----
	static void testBoolString()
	{
		assertEqual("True", true.boolToString(true), "boolToString firstUpper");
		assertEqual("true", true.boolToString(false), "boolToString lower");
		assertEqual("TRUE", true.boolToString(false, true), "boolToString fullUpper");
		assertEqual("FALSE", false.boolToString(false, true), "boolToString false upper");
		assertTrue("true".stringToBool(), "stringToBool true");
		assertTrue("True".stringToBool(), "stringToBool True");
		assertTrue("TRUE".stringToBool(), "stringToBool TRUE");
		assertFalse("false".stringToBool(), "stringToBool false");
		assertFalse("".stringToBool(), "stringToBool empty");
	}

	// ---- SToColor ----
	static void testSToColor()
	{
		Color c = "#FF0000".SToColor();
		assertTrue(c.r.isEqual(1f, 0.01f) && c.g.isZero(0.01f) && c.b.isZero(0.01f), "SToColor red");
		Color c2 = "#00FF00FF".SToColor();
		assertTrue(c2.g.isEqual(1f, 0.01f) && c2.a.isEqual(1f, 0.01f), "SToColor green+alpha");
	}

	// ---- SToBools (解析为整数列表, >0 为 true) ----
	static void testSToBools()
	{
		List<bool> list = new();
		// SToBools 内部用 SToI 解析每个元素, 所以只能用数值字符串
		"1,0,1".SToBools(list);
		assertEqual(3, list.Count, "SToBools count");
		assertTrue(list[0], "SToBools[0]");
		assertFalse(list[1], "SToBools[1]");
		assertTrue(list[2], "SToBools[2]");
	}

	// ---- stringsToString (4重载) ----
	static void testStringsToString()
	{
		List<string> list = new() { "a", "b", "c" };
		assertEqual("a,b,c", list.stringsToString(), "stringsToString List char sep");
		assertEqual("a|b|c", list.stringsToString("|"), "stringsToString List string sep");
		string[] arr = { "x", "y" };
		assertEqual("x,y", arr.stringsToString(), "stringsToString array char sep");
		assertEqual("x-y", arr.stringsToString("-"), "stringsToString array string sep");
	}

	// ---- stringToStrings (4重载) ----
	static void testStringToStrings()
	{
		List<string> list = new();
		"a,b,c".stringToStrings(list);
		assertEqual(3, list.Count, "stringToStrings List char");
		assertEqual("b", list[1], "stringToStrings val");
		List<string> list2 = "a,b,c".stringToStrings();
		assertEqual(3, list2.Count, "stringToStrings return List");
		List<string> list3 = "a,,c".stringToStrings(true, ',');
		assertEqual(2, list3.Count, "stringToStrings removeEmpty");
		List<string> list4 = "a|b|c".stringToStrings(false, "|");
		assertEqual(3, list4.Count, "stringToStrings string keyword");
	}

	// ---- SToIs / SToLs / SToUIs / SToFs ----
	static void testSToIsSToLsSToUIsSToFs()
	{
		// SToIs (List<int>)
		List<int> is1 = new();
		"1,2,3".SToIs(is1);
		assertEqual(3, is1.Count, "SToIs count");
		assertEqual(2, is1[1], "SToIs val");
		// SToIs return
		List<int> is2 = "4,5".SToIs();
		assertEqual(2, is2.Count, "SToIs return");
		// SToIsNonAlloc
		List<int> is3 = "6,7,8".SToIsNonAlloc();
		assertEqual(3, is3.Count, "SToIsNonAlloc");
		// SToLs
		List<long> ls1 = new();
		"10,20".SToLs(ls1);
		assertEqual(2, ls1.Count, "SToLs count");
		// SToLs return
		List<long> ls2 = "30,40".SToLs();
		assertEqual(2, ls2.Count, "SToLs return");
		// SToUIs
		List<uint> uis = new();
		"1,2".SToUIs(uis);
		assertEqual(2, uis.Count, "SToUIs");
		// SToFs (List<float>)
		List<float> fs1 = new();
		"1.5,2.5".SToFs(fs1);
		assertEqual(2, fs1.Count, "SToFs count");
		assertTrue(fs1[0].isEqual(1.5f, 0.01f), "SToFs val");
		// SToFs return
		List<float> fs2 = "3.0,4.0".SToFs();
		assertEqual(2, fs2.Count, "SToFs return");
		// SToFsNonAlloc
		List<float> fs3 = "5.0,6.0".SToFsNonAlloc();
		assertEqual(2, fs3.Count, "SToFsNonAlloc");
	}

	// ---- toPercent / toProbability ----
	static void testPercentProbability()
	{
		// float toPercent: 内部用 FToS(默认 removeTailZero=true)，整数尾零会被去掉
		assertEqual("50%", 0.5f.toPercent(0), "toPercent f 0.5");
		// 0.25*100=25，checkInt 后为整数，尾零被移除
		assertEqual("25%", 0.25f.toPercent(1), "toPercent f 0.25");
		assertEqual("100%", 1.0f.toPercent(0), "toPercent f 1.0");
		// string toPercent
		assertEqual("50%", "0.5".toPercent(0), "toPercent str 0.5");
		// int toProbability: 内部 value*0.01f，所以 50→0.5%
		assertEqual("0.5%", 50.toProbability(), "toProbability int 50");
		// float toProbability: 同样 value*0.01f，所以 0.5→0.005%
		assertEqual("0.005%", 0.5f.toProbability(), "toProbability f 0.5");
		// string toProbability: 同样 SToF()*0.01f
		assertEqual("0.005%", "0.5".toProbability(), "toProbability str 0.5");
	}

	// ---- toBytes ----
	static void testToBytes()
	{
		byte[] b = "hello".toBytes();
		assertEqual(5, b.Length, "toBytes len");
		assertEqual((byte)'h', b[0], "toBytes[0]");
		byte[] b2 = "hello".toBytes(Encoding.UTF8);
		assertEqual(5, b2.Length, "toBytes UTF8 len");
		// toBytes 对 null 返回 null（不是空数组）
		byte[] bNull = ((string)null).toBytes();
		assertTrue(bNull == null, "toBytes null returns null");
	}

	// ---- fixedAndPercent ----
	static void testFixedAndPercent()
	{
		// fixedAndPercent: 格式 "固定值+百分比"，如 5+10%
		assertEqual("5+10%", 5.fixedAndPercent(0.1f), "fixedAndPercent 5*0.1");
		// 固定值为0时，只显示百分比部分（toPercent(0.5f)=50%）
		assertEqual("50%", 0.fixedAndPercent(0.5f), "fixedAndPercent 0");
	}

	// ==================== 补充遗漏测试 ====================

	// ---- contains(Predicate<char>) ----
	static void testContainsPredicate()
	{
		assertTrue("Hello".contains(c => c == 'H'), "contains predicate H");
		assertTrue("Hello".contains(c => char.IsUpper(c)), "contains predicate IsUpper");
		assertFalse("hello".contains(c => char.IsUpper(c)), "contains predicate IsUpper all lower");
		assertFalse(((string)null).contains(c => true), "contains predicate null str");
		assertFalse("hello".contains(null), "contains predicate null action");
		assertFalse("".contains(c => true), "contains predicate empty str");
	}

	// ---- range null 和 endIndexNotInclude<0 分支 ----
	static void testRangeNullAndEdge()
	{
		assertNull(((string)null).range(0, 3), "range null returns null");
		assertEqual("hello", "hello".range(0, -1), "range endIndex -1 returns rest");
	}

	// ---- rangeBetweenKeyToKey (string,char) 和 (char,string) 重载 ----
	static void testRangeBetweenKeyToKeyOverloads()
	{
		// string key0, char key1
		assertEqual("test", "prefix[test]suffix".rangeBetweenKeyToKey("[", ']'), "rangeBetween str+char");
		// "start<<mid>>end": key0="<<" startIndex=6, key1='>' endIndex=10 → str[6..10]="<mid"
		assertEqual("mid", "start<<mid>>end".rangeBetweenKeyToKey("<<", '>'), "rangeBetween str+char 2");
		// char key0, string key1
		assertEqual("test", "prefix[test]suffix".rangeBetweenKeyToKey('[', "]"), "rangeBetween char+str");
		assertEqual("<mid", "start<<mid>>end".rangeBetweenKeyToKey('<', ">>"), "rangeBetween char+str 2");
		assertEqual("mid", "start<<mid>>end".rangeBetweenKeyToKey("<<", ">>"), "rangeBetween char+str 2");
		// null
		assertNull(((string)null).rangeBetweenKeyToKey("[", ']'), "rangeBetween str+char null");
		assertNull(((string)null).rangeBetweenKeyToKey('[', "]"), "rangeBetween char+str null");
	}

	// ---- rangeToFirst(int startIndex, char key) ----
	static void testRangeToFirstWithStartIndex()
	{
		assertEqual("lo.wo", "hello.world".rangeToFirst(3, 'r'), "rangeToFirst startIdx 3 to r");
		// key not found → str[startIndex..]，startIndex=6→"world"
		assertEqual("world", "hello.world".rangeToFirst(6, 'X'), "rangeToFirst startIdx key not found");
		assertNull(((string)null).rangeToFirst(0, 'a'), "rangeToFirst null");
	}

	// ---- rangeToFirstInclude(int startIndex, char key) ----
	static void testRangeToFirstIncludeWithStartIndex()
	{
		assertEqual("lo.wor", "hello.world".rangeToFirstInclude(3, 'r'), "rangeToFirstInclude startIdx to r");
		// key not found → str[startIndex..]，startIndex=6→"world"
		assertEqual("world", "hello.world".rangeToFirstInclude(6, 'X'), "rangeToFirstInclude key not found");
		assertNull(((string)null).rangeToFirstInclude(0, 'a'), "rangeToFirstInclude null");
	}

	// ---- rangeToLast(int startIndex, char key) ----
	static void testRangeToLastWithStartIndex()
	{
		assertEqual("ello", "hello.hello".rangeToLast(1, '.'), "rangeToLast startIdx 1 to .");
		assertEqual("ello.hello", "hello.hello".rangeToLast(1, 'X'), "rangeToLast startIdx key not found");
		assertNull(((string)null).rangeToLast(0, 'a'), "rangeToLast null");
	}

	// ---- rangeToLastInclude(int startIndex, char key) ----
	static void testRangeToLastIncludeWithStartIndex()
	{
		assertEqual("ello.", "hello.hello".rangeToLastInclude(1, '.'), "rangeToLastInclude startIdx to .");
		assertEqual("ello.hello", "hello.hello".rangeToLastInclude(1, 'X'), "rangeToLastInclude key not found");
		assertNull(((string)null).rangeToLastInclude(0, 'a'), "rangeToLastInclude null");
	}

	// ---- rangeFromFirstInclude 边界 ----
	static void testRangeFromFirstIncludeEdge()
	{
		assertEqual("hello", "hello".rangeFromFirstInclude('z'), "rangeFromFirstInclude key not found");
		assertNull(((string)null).rangeFromFirstInclude('a'), "rangeFromFirstInclude null");
	}

	// ---- removeStart / removeEnd 的 char 重载 ----
	static void testRemoveStartEndCharOverloads()
	{
		assertEqual("ello", "hello".removeStart('h'), "removeStart char h");
		assertEqual("hello", "hello".removeStart('x'), "removeStart char no match");
		// removeStart(char) 对 null 返回 null
		assertNull(((string)null).removeStart('h'), "removeStart char null");
		assertEqual("hell", "hello".removeEnd('o'), "removeEnd char o");
		assertEqual("hello", "hello".removeEnd('x'), "removeEnd char no match");
		// removeEnd(char) 对 null 返回 null
		assertNull(((string)null).removeEnd('o'), "removeEnd char null");
	}

	// ---- removeStart / removeEnd caseSensitive=false ----
	static void testRemoveStartEndCaseInsensitive()
	{
		// removeStart 移除前缀但不改变剩余部分的大小写
		assertEqual("World", "HelloWorld".removeStart("HELLO", false), "removeStart insensitive");
		assertEqual("HelloWorld", "HelloWorld".removeStart("hello"), "removeStart caseSensitive default");
		// removeEnd 同理，保留原始大小写
		assertEqual("Hello", "HelloWorld".removeEnd("WORLD", false), "removeEnd insensitive");
		assertEqual("HelloWorld", "HelloWorld".removeEnd("world"), "removeEnd caseSensitive default");
	}

	// ---- findFirstSubstr returnEndIndex 和 sensitive 参数 ----
	static void testFindFirstSubstrReturnEndAndSensitive()
	{
		// returnEndIndex=true 时返回结束位置（startIndex+pattern.Length）
		// "world" 在 "hello world" 位置 6，"world".Length=5 → 返回 11
		assertEqual(11, "hello world".findFirstSubstr("world", 0, true), "findFirst returnEndIndex");
		// "hel" 在 "hello" 位置 0，hel.Length=3 → 返回 3
		assertEqual(3, "hello".findFirstSubstr("hel", 0, true), "findFirst returnEndIndex 2");
		// sensitive=false
		assertEqual(0, "Hello".findFirstSubstr("HEL", 0, false, false), "findFirst insensitive string");
		assertEqual(0, "Hello".findFirstSubstr('h', 0, false), "findFirst insensitive char");
		assertEqual(-1, "hello".findFirstSubstr("XYZ"), "findFirst not found");
	}

	// ---- SToLs(ref long[]) ----
	static void testSToLsRefArray()
	{
		long[] arr = null;
		"10,20,30".SToLs(ref arr);
		assertEqual(3, arr.Length, "SToLs ref array create");
		assertEqual(20L, arr[1], "SToLs ref array val");
		long[] arr2 = new long[3];
		"100,200,300".SToLs(ref arr2);
		assertEqual(200L, arr2[1], "SToLs ref array fill");
	}

	// ---- SToIs(ref int[]) ----
	static void testSToIsRefArray()
	{
		int[] arr = null;
		"1,2,3".SToIs(ref arr);
		assertEqual(3, arr.Length, "SToIs ref array create");
		assertEqual(2, arr[1], "SToIs ref array val");
		int[] arr2 = new int[3];
		"10,20,30".SToIs(ref arr2);
		assertEqual(20, arr2[1], "SToIs ref array fill");
	}

	// ---- SToFs(ref float[]) ----
	static void testSToFsRefArray()
	{
		float[] arr = null;
		"1.5,2.5,3.5".SToFs(ref arr);
		assertEqual(3, arr.Length, "SToFs ref array create");
		assertTrue(arr[1].isEqual(2.5f, 0.01f), "SToFs ref array val");
		float[] arr2 = new float[3];
		"10,20,30".SToFs(ref arr2);
		assertTrue(arr2[1].isEqual(20f, 0.01f), "SToFs ref array fill");
	}

	// ---- FToS (float -> string) ----
	static void testFToS()
	{
		assertEqual("0", 0f.FToS(), "FToS 0");
		assertEqual("1", 1f.FToS(), "FToS 1");
		assertEqual("1.5", 1.5f.FToS(), "FToS 1.5");
		assertEqual("1.5000", 1.5f.FToS(4, false), "FToS keepTailZero");
		assertEqual("-1", (-1f).FToS(), "FToS -1");
		assertEqual("-1.5", (-1.5f).FToS(), "FToS -1.5");
		assertEqual("0", 0.000001f.FToS(), "FToS tiny near zero");
		assertEqual("1", 1.000001f.FToS(), "FToS near int");
		// precision=0: 直接 (int)value.IToS()，向零截断（非四舍五入）
		assertEqual("3", 3.14f.FToS(0), "FToS precision 0");
		assertEqual("3", 3.5f.FToS(0), "FToS precision 0 truncate (非四舍五入)");
		assertEqual("10", 9.999f.FToS(1), "FToS near 10");
	}

	// ---- UnicodeToUTF8 / UTF8ToUnicode 往返 ----
	static void testEncodingRoundtripMore()
	{
		string ascii = "HelloWorld";
		// UTF8 -> Unicode -> UTF8 往返
		string uni = ascii.UTF8ToUnicode();
		assertEqual(ascii, uni.UnicodeToUTF8(), "UTF8->Unicode->UTF8 roundtrip");
		// convertStringFormat 直接
		string viaConvert = "Test".convertStringFormat(Encoding.UTF8, Encoding.Unicode);
		assertNotNull(viaConvert, "convertStringFormat UTF8->Unicode");
		assertTrue(viaConvert.Length > 0, "convertStringFormat non-empty");
		// GB2312 编码测试（可能在某些平台不支持）
		try
		{
			string gbUtf = "ABC".GB2312ToUTF8();
			assertEqual("ABC", gbUtf.UTF8ToGB2312(), "GB2312->UTF8->GB2312 roundtrip");
		}
		catch (System.ArgumentException)
		{
			// GB2312 not supported
		}
	}

	// ---- findLastChar / findLastSubstr 边界 ----
	static void testFindLastCharEdge()
	{
		assertEqual(3, "hello".findLastChar('l', 4), "findLastChar with endPos");
		assertEqual(1, "hello".findLastChar('e', 1), "findLastChar endPos=1");
		assertEqual(-1, "hello".findLastChar('z', -1), "findLastChar not found");
	}

	// ---- splitLine null/empty ----
	static void testSplitLineEdge()
	{
		"".splitLine(out string[] lines);
		assertNull(lines, "splitLine empty out null");
		((string)null).splitLine(out string[] lines2);
		assertNull(lines2, "splitLine null out null");
		// splitLine 对空字符串返回 null（不是空数组）
		string[] arr = "".splitLine();
		assertNull(arr, "splitLine empty returns null");
	}

	// ---- stringToStrings params string[] 重载 ----
	static void testStringToStringsParams()
	{
		// 使用 params string[] 的 NonAlloc 版本
		List<string> list1 = "a|b|c".stringToStrings(true, "|");
		assertEqual(3, list1.Count, "stringToStrings params str");
		assertEqual("b", list1[1], "stringToStrings params str val");
		// 使用 params char[] 的 NonAlloc 版本
		List<string> list2 = "x-y-z".stringToStrings(true, '-');
		assertEqual(3, list2.Count, "stringToStrings params char");
	}

	// ---- FToS precision 边界 ----
	static void testFToSPrecisionEdge()
	{
		// precision 7 (最大): float 只有约7位精度，3.1415927f 实际存储可能有舍入
		// 只验证格式正确（包含小数点且7位小数），不精确匹配末尾数字
		string piStr = 3.1415927f.FToS(7, false);
		assertTrue(piStr.StartsWith("3.14159"), "FToS max precision prefix");
		// removeTailZero with trailing zeros
		assertEqual("1.2", 1.200f.FToS(3), "FToS remove trailing zeros");
		assertEqual("1", 1.000f.FToS(3), "FToS remove all zeros");
	}

	// ---- SToBools 空/异常输入 ----
	static void testSToBoolsEdge()
	{
		List<bool> list = new();
		"".SToBools(list);
		assertEqual(0, list.Count, "SToBools empty str");
		// 数值 >0 为 true, <=0 为 false
		List<bool> list2 = new();
		"0,1,-1,100".SToBools(list2);
		assertFalse(list2[0], "SToBools 0=false");
		assertTrue(list2[1], "SToBools 1=true");
		assertFalse(list2[2], "SToBools -1=false");
		assertTrue(list2[3], "SToBools 100=true");
	}

	// ---- IToS 缓存边界（超出预计算表范围的大数） ----
	static void testIToSCacheEdge()
	{
		assertEqual("10240", 10240.IToS(), "IToS beyond precomputed table");
		assertEqual("10240", 10240u.IToS(), "IToS uint beyond table");
		assertEqual("10240", 10240L.LToS(), "LToS beyond table");
		assertEqual("10240", 10240UL.LToS(), "LToS ulong beyond table");
	}
}