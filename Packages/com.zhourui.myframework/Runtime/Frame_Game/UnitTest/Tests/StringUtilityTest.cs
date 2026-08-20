using System.Collections.Generic;
using System.Text;
using static TestAssert;

// Frame_Game 精简层 StringUtility 测试(纯字符串逻辑)
public static class StringUtilityTest
{
	public static void Run()
	{
		testToPercent();
		testFToSZeroPrecision();
		testFToSDefaultPrecision();
		testStringToBytesNull();
		testStringToBytesRoundTrip();
		testBytesToStringEmpty();
		testRemoveLastZero();
		testGetFilePath();
		testGetFilePathKeepSlash();
		testGetFilePathNoDir();
		testRemoveSuffix();
		testGetFileNameNoSuffixNoDir();
		testBytesToHEXString();
		testBytesToHEXStringEmpty();
		testSplitLine();
		testSplitLineEmpty();
		testSplitChar();
		testSplitString();
		testStringsToString();
		testGetFileNameWithSuffix();
	}

	// toPercent: value*100 后按精度格式化
	static void testToPercent()
	{
		string s = StringUtility.toPercent(0.5f, 1);
		assertEqual("50.0", s, "0.5 → 50.0");
	}

	// precision 0 → 整数
	static void testFToSZeroPrecision()
	{
		assertEqual("12", StringUtility.FToS(12.7f, 0), "precision 0 取整");
	}

	// 默认精度 4
	static void testFToSDefaultPrecision()
	{
		string s = StringUtility.FToS(1.23456f, 4);
		assertEqual("1.2346", s, "精度 4 四舍五入");
	}

	// stringToBytes(null) → null
	static void testStringToBytesNull()
	{
		assertNull(StringUtility.stringToBytes(null), "null 字符串返回 null");
	}

	// 往返一致(UTF8)
	static void testStringToBytesRoundTrip()
	{
		byte[] bytes = StringUtility.stringToBytes("hello");
		assertNotNull(bytes, "bytes 非 null");
		string back = StringUtility.bytesToString(bytes);
		assertEqual("hello", back, "UTF8 往返一致");
	}

	// 空字节数组 → 空串
	static void testBytesToStringEmpty()
	{
		assertEqual("", StringUtility.bytesToString(new byte[0]), "空字节 → 空串");
	}

	// removeLastZero: 遇到第一个 \0 截断
	static void testRemoveLastZero()
	{
		assertEqual("ab", StringUtility.removeLastZero("ab\0cd"), "第一个 \\0 截断");
	}

	// getFilePath: 去掉最后一段文件名
	static void testGetFilePath()
	{
		assertEqual("a/b", StringUtility.getFilePath("a/b/c.txt"), "去掉文件名");
	}

	// getFilePath keepEndSlash
	static void testGetFilePathKeepSlash()
	{
		assertEqual("a/b/", StringUtility.getFilePath("a/b/c.txt", true), "保留结尾斜杠");
	}

	// getFilePath 无目录 → 空串
	static void testGetFilePathNoDir()
	{
		assertEqual("", StringUtility.getFilePath("c.txt"), "无目录返回空");
	}

	// removeSuffix: 去掉最后一个 . 后缀
	static void testRemoveSuffix()
	{
		assertEqual("a/b/c", StringUtility.removeSuffix("a/b/c.txt"), "去掉后缀");
	}

	// getFileNameNoSuffixNoDir: 只留文件名
	static void testGetFileNameNoSuffixNoDir()
	{
		assertEqual("c", StringUtility.getFileNameNoSuffixNoDir("a/b/c.txt"), "只留文件名");
	}

	// ═════════════════════════════════════════════════════════════════
	// 深度补充
	// ═════════════════════════════════════════════════════════════════

	// bytesToHEXString: 字节转小写十六进制
	static void testBytesToHEXString()
	{
		byte[] bytes = new byte[] { 0x0a, 0xFF, 0x12 };
		string s = StringUtility.bytesToHEXString(bytes);
		assertEqual("0aff12", s, "十六进制小写");
	}

	// bytesToHEXString: 空数组 → 空串
	static void testBytesToHEXStringEmpty()
	{
		assertEqual("", StringUtility.bytesToHEXString(new byte[0]), "空数组空串");
	}

	// splitLine: 多行分割, 移除空行
	static void testSplitLine()
	{
		string[] lines = StringUtility.splitLine("a\nb\n\nc");
		assertEqual(3, lines.Length, "3 行(空行移除)");
		assertEqual("a", lines[0], "第一行");
		assertEqual("c", lines[2], "第三行");
	}

	// splitLine: 空串 → null
	static void testSplitLineEmpty()
	{
		assertNull(StringUtility.splitLine(""), "空串返回 null");
	}

	// split(字符): 按字符分割
	static void testSplitChar()
	{
		string[] parts = StringUtility.split("a,b,,c", true, ',');
		assertEqual(3, parts.Length, "3 段(空段移除)");
		assertEqual("a", parts[0], "第一段");
	}

	// split(字符串): 按子串分割
	static void testSplitString()
	{
		string[] parts = StringUtility.split("a--b--c", true, "--");
		assertEqual(3, parts.Length, "3 段");
		assertEqual("b", parts[1], "第二段");
	}

	// stringsToString: 列表拼接
	static void testStringsToString()
	{
		List<string> values = new() { "a", "b", "c" };
		assertEqual("a,b,c", StringUtility.stringsToString(values), "逗号拼接");
		assertEqual("a|b", StringUtility.stringsToString(new List<string> { "a", "b" }, '|'), "自定义分隔符");
	}

	// getFileNameWithSuffix: 取文件名带后缀
	static void testGetFileNameWithSuffix()
	{
		assertEqual("c.txt", StringUtility.getFileNameWithSuffix("a/b/c.txt"), "带后缀文件名");
	}
}
