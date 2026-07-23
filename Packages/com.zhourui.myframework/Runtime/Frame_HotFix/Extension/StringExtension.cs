using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static BinaryUtility;
using static UnityUtility;
using static FrameBaseUtility;
using static MathUtility;
using static StringUtility;

public static class StringExtension
{
    private static List<byte> mTempByteList0 = new();                                   // 避免GC,给stringToBytesNonAlloc使用的
    private static List<int> mTempIntList = new();                                      // 避免GC
    private static List<long> mTempLongList = new();                                    // 避免GC
    private static List<float> mTempFloatList = new();                                  // 避免GC
    private static List<string> mTempStringList = new();								// 避免GC
    private static Dictionary<string, int> mStringToInt;                                // 用于快速查找字符串转换后的整数
    private static string[] mIntToString;												// 用于快速获取整数转换后的字符串
    private static Dictionary<string, Vector2Int> mStringToVector2Cache;				// 字符串转换为2维向量的缓存
    private static Dictionary<string, Vector3Int> mStringToVector3Cache;                // 字符串转换为3维向量的缓存
    private static string[] mFloatConvertPercision = new string[] { "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7" };    // 浮点数转换时精度
	private static List<string> mZeroStringList = new();
    private static int STRING_TO_VECTOR2INT_MAX_CACHE = 10240;                          // mStringToVector2Cache最大数量
    public static int length(this string list) { return list?.Length ?? 0; }
	public static bool isEmpty(this string str) { return str == null || str.Length == 0; }
	public static bool contains(this string str, char c) { return str != null && str.Contains(c); }
	public static bool contains(this string str, Predicate<char> action) 
	{
		if (str.isEmpty() || action == null)
		{
			return false;
		}
		foreach (char c in str)
		{
			if (action(c))
			{
				return true;
			}
		}
		return false;
	}
	public static string range(this string str, int startIndex, int endIndexNotInclude)
	{
		if (str == null)
		{
			return str;
		}
		if (endIndexNotInclude < 0)
		{
			return str[startIndex..];
		}
		return str[startIndex..endIndexNotInclude];
	}
	// 截取从第一个key字符开始之后的字符串,不包含key
	public static string rangeFromFirstToEndExcept(this string str, char key)
	{
		if (str == null)
		{
			return str;
		}
		int startIndex = str.IndexOf(key);
		if (startIndex >= 0)
		{
			return str[(startIndex + 1)..];
		}
		return str;
	}
	// 截取从第一个key字符开始之后的字符串,包含key
	public static string rangeFromFirstToEnd(this string str, char key)
	{
		if (str == null)
		{
			return str;
		}
		int startIndex = str.IndexOf(key);
		if (startIndex >= 0)
		{
			return str[startIndex..];
		}
		return str;
	}
	// 截取从第一个key0到第一个key1之间的字符串,不包含key0和key1
	public static string rangeBetweenKeyToKey(this string str, char key0, char key1)
	{
		if (str == null)
		{
			return str;
		}
		int startIndex = str.IndexOf(key0);
		int endIndex = str.IndexOf(key1);
		if (startIndex < 0 || endIndex < 0 || endIndex < startIndex)
		{
			return str;
		}
		return str[(startIndex + 1)..endIndex];
	}
	// 截取从第一个key0到第一个key1之间的字符串,包含key0和key1
	public static string rangeBetweenKeyToKeyInclude(this string str, char key0, char key1)
	{
		if (str == null)
		{
			return str;
		}
		int startIndex = str.IndexOf(key0);
		int endIndex = str.IndexOf(key1);
		if (startIndex < 0 || endIndex < 0 || endIndex < startIndex)
		{
			return str;
		}
		return str[startIndex..(endIndex + 1)];
	}
	// 截取从第一个key0到第一个key1之间的字符串,不包含key0和key1
	public static string rangeBetweenKeyToKey(this string str, string key0, string key1)
	{
		if (str == null)
		{
			return str;
		}
		int startIndex = str.IndexOf(key0) + key0.Length;
		int endIndex = str.IndexOf(key1, startIndex);
		if (endIndex >= 0)
		{
			return str[startIndex..endIndex];
		}
		return str[startIndex..];
	}
	// 截取从第一个key0到第一个key1之间的字符串,不包含key0和key1
	public static string rangeBetweenKeyToKey(this string str, string key0, char key1)
	{
		if (str == null)
		{
			return str;
		}
		int startIndex = str.IndexOf(key0) + key0.Length;
		int endIndex = str.IndexOf(key1, startIndex);
		if (endIndex >= 0)
		{
			return str[startIndex..endIndex];
		}
		return str[startIndex..];
	}

	// 截取从第一个key0到第一个key1之间的字符串,不包含key0和key1
	public static string rangeBetweenKeyToKey(this string str, char key0, string key1)
	{
		if (str == null)
		{
			return str;
		}
		int startIndex = str.IndexOf(key0) + 1;
		int endIndex = str.IndexOf(key1, startIndex);
		if (endIndex >= 0)
		{
			return str[startIndex..endIndex];
		}
		return str[startIndex..];
	}// 截取第一个key字符之前的字符串,不包含key
	public static string rangeToFirst(this string str, char key)
	{
		if (str == null)
		{
			return str;
		}
		int endIndex = str.IndexOf(key);
		if (endIndex >= 0)
		{
			return str[0..endIndex];
		}
		return str;
	}
	// 截取从startIndex到第一个key字符之前的字符串,不包含key
	public static string rangeToFirst(this string str, int startIndex, char key)
	{
		if (str == null)
		{
			return str;
		}
		int endIndex = str.IndexOf(key, startIndex);
		if (endIndex >= 0)
		{
			return str[startIndex..endIndex];
		}
		return str[startIndex..];
	}
	// 截取从startIndex到第一个key字符之前的字符串,包含key
	public static string rangeToFirstInclude(this string str, char key)
	{
		if (str == null)
		{
			return str;
		}
		int endIndex = str.IndexOf(key);
		if (endIndex >= 0)
		{
			return str[0..(endIndex + 1)];
		}
		return str;
	}
	// 截取从startIndex到第一个key字符之前的字符串,包含key
	public static string rangeToFirstInclude(this string str, int startIndex, char key)
	{
		if (str == null)
		{
			return str;
		}
		int endIndex = str.IndexOf(key, startIndex);
		if (endIndex >= 0)
		{
			return str[startIndex..(endIndex + 1)];
		}
		return str[startIndex..];
	}
	// 截取从0到最后一个key字符之前的字符串,不包含key
	public static string rangeToLast(this string str, char key)
	{
		if (str == null)
		{
			return str;
		}
		int endIndex = str.LastIndexOf(key);
		if (endIndex >= 0)
		{
			return str[0..endIndex];
		}
		return str;
	}
	// 截取从startIndex到最后一个key字符之前的字符串,不包含key
	public static string rangeToLast(this string str, int startIndex, char key)
	{
		if (str == null)
		{
			return str;
		}
		int endIndex = str.LastIndexOf(key);
		if (endIndex >= 0)
		{
			return str[startIndex..endIndex];
		}
		return str[startIndex..];
	}
	// 截取从0到最后一个key字符之前的字符串,包含key
	public static string rangeToLastInclude(this string str, char key)
	{
		if (str == null)
		{
			return str;
		}
		int endIndex = str.LastIndexOf(key);
		if (endIndex >= 0)
		{
			return str[0..(endIndex + 1)];
		}
		return str;
	}
	// 截取从startIndex到最后一个key字符之前的字符串,包含key
	public static string rangeToLastInclude(this string str, int startIndex, char key)
	{
		if (str == null)
		{
			return str;
		}
		int endIndex = str.LastIndexOf(key);
		if (endIndex >= 0)
		{
			return str[startIndex..(endIndex + 1)];
		}
		return str[startIndex..];
	}
	// 截取从第一个key到字符串结尾,不包含key
	public static string rangeFromFirst(this string str, char key)
	{
		if (str == null)
		{
			return str;
		}
		int startIndex = str.IndexOf(key);
		if (startIndex < 0)
		{
			return str;
		}
		return str[(startIndex + 1)..];
	}
	// 截取从第一个key到字符串结尾,包含key
	public static string rangeFromFirstInclude(this string str, char key)
	{
		if (str == null)
		{
			return str;
		}
		int startIndex = str.IndexOf(key);
		if (startIndex < 0)
		{
			return str;
		}
		return str[startIndex..];
	}
	public static string substr(this string str, int startIndex, int length)
	{
		if (str == null)
		{
			return str;
		}
		return str.Substring(startIndex, length);
	}
	// 截取最后一定长度的字符串
	public static string endString(this string str, int endLength)
	{
		if (str == null || endLength > str.Length)
		{
			return str;
		}
		return str[(str.Length - endLength)..];
	}
	// 截取开始一定长度的字符串
	public static string startString(this string str, int startLength)
	{
		if (str == null || startLength < 0)
		{
			return str;
		}
		clampMax(ref startLength, str.Length);
		return str[0..startLength];
	}
	// 移除最后一定长度的字符串
	public static string removeEndCount(this string str, int removeCount)
	{
		if (str == null || removeCount <= 0)
		{
			return str;
		}
		return str[0..(str.Length - removeCount)];
	}
	// 移除开始一定长度的字符串
	public static string removeStartCount(this string str, int removeCount)
	{
		if (str == null || removeCount <= 0 || removeCount > str.Length)
		{
			return str;
		}
		return str[removeCount..];
	}
	// 如果str以pattern开头,则移除pattern的部分
	public static string removeStart(this string str, string pattern, bool caseSensitive = true)
	{
		if (str == null || pattern == null || str.Length < pattern.Length)
		{
			return str;
		}
		bool needRemove;
		if (caseSensitive)
		{
			needRemove = str.StartsWith(pattern);
		}
		else
		{
			needRemove = str.ToLower().StartsWith(pattern.ToLower());
		}
		if (needRemove)
		{
			return str[pattern.Length..];
		}
		return str;
	}
	// 如果str以pattern开头,则移除pattern的部分
	public static string removeStart(this string str, char pattern)
	{
		if (str == null || str.Length < 1)
		{
			return str;
		}
		if (str[0] == pattern)
		{
			return str[1..];
		}
		return str;
	}
	// 如果str以pattern结尾,则移除pattern的部分
	public static string removeEnd(this string str, string pattern, bool caseSensitive = true)
	{
		if (str == null || pattern == null || str.Length < pattern.Length)
		{
			return str;
		}
		bool needRemove;
		if (caseSensitive)
		{
			needRemove = str.EndsWith(pattern);
		}
		else
		{
			needRemove = str.ToLower().EndsWith(pattern.ToLower());
		}
		if (needRemove)
		{
			return str[0..(str.Length - pattern.Length)];
		}
		return str;
	}
	// 如果str以pattern结尾,则移除pattern的部分
	public static string removeEnd(this string str, char pattern)
	{
		if (str == null || str.Length < 1)
		{
			return str;
		}
		if (str[^1] == pattern)
		{
			return str[0..(str.Length - 1)];
		}
		return str;
	}
	// 移除str中开头所有的key,直到遇到一个不是key的字符
	public static string removeStartAll(this string str, char key)
	{
		int removeStartCount = str.Length;
		for (int i = 0; i < str.Length; ++i)
		{
			if (str[i] != key)
			{
				removeStartCount = i;
				break;
			}
		}
		return str[removeStartCount..];
	}
	// 移除str中开头所有的key0或者key1,直到遇到一个不是key0和key1的字符
	public static string removeStartAll(this string str, char key0, char key1)
	{
		int removeStartCount = str.Length;
		for (int i = 0; i < str.Length; ++i)
		{
			if (str[i] != key0 && str[i] != key1)
			{
				removeStartCount = i;
				break;
			}
		}
		return str[removeStartCount..];
	}
	// 从后往前移除str中结尾所有的key,直到遇到一个不是key的字符
	public static string removeEndAll(this string str, char key)
	{
		int removeStartCount = str.Length;
		for (int i = str.Length - 1; i >= 0; --i)
		{
			if (str[i] != key)
			{
				removeStartCount = i + 1;
				break;
			}
		}
		if (removeStartCount > 0 && removeStartCount < str.Length)
		{
			return str[0..removeStartCount];
		}
		return str;
	}
	// 从后往前移除str中结尾所有的key0或者key1,直到遇到一个不是key0和key1的字符
	public static string removeEndAll(this string str, char key0, char key1)
	{
		int removeStartCount = str.Length;
		for (int i = str.Length - 1; i >= 0; --i)
		{
			if (str[i] != key0 && str[i] != key1)
			{
				removeStartCount = i + 1;
				break;
			}
		}
		if (removeStartCount > 0 && removeStartCount < str.Length)
		{
			return str[0..removeStartCount];
		}
		return str;
	}
	// 判断是否以pattern开头
	public static bool startWith(this string str, string pattern, bool sensitive = true)
	{
		if (str.Length < pattern.Length)
		{
			return false;
		}
		string startString = str.startString(pattern.Length);
		if (sensitive)
		{
			return startString == pattern;
		}
		else
		{
			return startString.ToLower() == pattern.ToLower();
		}
	}
	// 判断是否以pattern结尾
	public static bool endWith(this string str, string pattern, bool sensitive = true)
	{
		if (str.Length < pattern.Length)
		{
			return false;
		}
		string endString = str.endString(pattern.Length);
		if (sensitive)
		{
			return endString == pattern;
		}
		else
		{
			return endString.ToLower() == pattern.ToLower();
		}
	}
	public static string removeAll(this string str, params string[] key)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.add(str);
		int keyCount = key.Length;
		for (int i = 0; i < keyCount; ++i)
		{
			builder.replaceAll(key[i], EMPTY);
		}
		return builder.ToString();
	}
	public static string removeAll(this string str, params char[] key)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.add(str);
		for (int i = builder.Length - 1; i >= 0; --i)
		{
			// 判断是否是需要移除的字符
			if (key.contains(builder[i]))
			{
				builder.remove(i, 1);
			}
		}
		return builder.ToString();
	}
	public static string removeAll(this string str, char key)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.add(str);
		for (int i = builder.Length - 1; i >= 0; --i)
		{
			// 判断是否是需要移除的字符
			if (key == builder[i])
			{
				builder.remove(i, 1);
			}
		}
		return builder.ToString();
	}
	// 将str中的[begin,end)替换为reStr
	public static string replace(this string str, int begin, int end, string reStr)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			builder.add(str);
			builder.replace(begin, end, reStr);
			return builder.ToString();
		}
		else
		{
			str = str.Remove(begin, end - begin);
			if (reStr.Length > 0)
			{
				str = str.Insert(begin, reStr);
			}
			return str;
		}
	}
	public static string replace(this string str, string key, string newWords)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			builder.add(str);
			builder.replace(key, newWords);
			return builder.ToString();
		}
		else
		{
			int startPos = 0;
			int pos = findFirstSubstr(str, key, startPos);
			if (pos < 0)
			{
				return str;
			}
			return replace(str, pos, pos + key.Length, newWords);
		}
	}
	public static string replaceAll(this string str, string key, string newWords)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			builder.add(str);
			builder.replaceAll(key, newWords);
			return builder.ToString();
		}
		else
		{
			int startPos = 0;
			while (true)
			{
				int pos = findFirstSubstr(str, key, startPos);
				if (pos < 0)
				{
					break;
				}
				str = replace(str, pos, pos + key.Length, newWords);
				startPos = pos + newWords.Length;
			}
			return str;
		}
	}
	public static string replaceAll(this string str, char key, char newWords)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.add(str);
		builder.replaceAll(key, newWords);
		return builder.ToString();
	}
	// 移除所有的空白字符
	public static string removeAllEmpty(this string str)
	{
		return str.removeAll(' ', '\t');
	}
	// 移除开头所有的空白字符
	public static string removeStartEmpty(this string str)
	{
		return str.removeStartAll(' ', '\t');
	}
	// 移除结尾所有的空白字符
	public static string removeEndEmpty(this string str)
	{
		return str.removeEndAll(' ', '\t');
	}
	// 如果str不以prefix开头,则在str开头加上prefix
	public static string ensurePrefix(this string str, string prefix)
	{
		if (!str.StartsWith(prefix))
		{
			return prefix + str;
		}
		return str;
	}
	// 如果str不以suffix结尾,则在str结尾加上suffix
	public static string ensureSuffix(this string str, string prefix)
	{
		if (!str.EndsWith(prefix))
		{
			return str + prefix;
		}
		return str;
	}
	// returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
	public static int findFirstSubstr(this string res, char pattern, int startPos = 0, bool sensitive = true)
	{
		if (res == null)
		{
			return -1;
		}
		if (!sensitive)
		{
			pattern = toLower(pattern);
		}
		int posFind = -1;
		int len = res.Length;
		for (int i = startPos; i < len; ++i)
		{
			if ((sensitive && res[i] == pattern) ||
				(!sensitive && toLower(res[i]) == pattern))
			{
				posFind = i;
				break;
			}
		}
		return posFind;
	}
	// returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
	public static int findFirstSubstr(this string res, string pattern, int startPos = 0, bool returnEndIndex = false, bool sensitive = true)
	{
		if (res == null)
		{
			return -1;
		}
		int posFind = -1;
		int subLen = pattern.Length;
		int len = res.Length;
		for (int i = startPos; i < len; ++i)
		{
			if (len - i < subLen)
			{
				break;
			}
			int j = 0;
			// 大小写敏感
			if (sensitive)
			{
				for (; j < subLen; ++j)
				{
					if (res[i + j] != pattern[j])
					{
						break;
					}
				}
			}
			// 大小写不敏感,则需要都转换为小写
			else
			{
				for (; j < subLen; ++j)
				{
					if (toLower(res[i + j]) != toLower(pattern[j]))
					{
						break;
					}
				}
			}
			if (j == subLen)
			{
				posFind = i;
				break;
			}
		}
		if (returnEndIndex && posFind >= 0)
		{
			posFind += subLen;
		}
		return posFind;
	}
	public static int findLastSubstr(this string res, string pattern, bool sensitive, int startPos = 0)
	{
		if (!sensitive)
		{
			// 全转换为小写
			res = res.ToLower();
			pattern = pattern.ToLower();
		}
		int posFind = -1;
		int subLen = pattern.Length;
		int len = res.Length;
		for (int i = len - subLen; i >= startPos; --i)
		{
			int j = 0;
			for (; j < subLen; ++j)
			{
				if (i + j >= 0 && i + j < len && res[i + j] != pattern[j])
				{
					break;
				}
			}
			if (j == subLen)
			{
				posFind = i;
				break;
			}
		}
		return posFind;
	}
	// 从后往前找到第一个指定字符,endPos表示从后往前找的开始的下标
	public static int findLastChar(this string str, char c, int endPos = -1)
	{
		if (endPos < 0)
		{
			endPos = str.Length - 1;
		}
		for (int i = endPos; i >= 0; --i)
		{
			if (str[i] == c)
			{
				return i;
			}
		}
		return -1;
	}
	// 从前往后去除两个成对字符之间的所有字符(包含两个字符)，并返回他们的下标
	public static string removeFirstBetweenPairChars(this string str, char startChar, char endChar, out int startCharIndex, out int endCharIndex)
	{
		endCharIndex = -1;
		startCharIndex = str.IndexOf(startChar);
		if (startCharIndex < 0)
		{
			return str;
		}

		// 未配对数量
		int unpaired = 1;
		for (int i = startCharIndex + 1; i < str.Length; ++i)
		{
			if (str[i] == startChar)
			{
				++unpaired;
			}
			else if (str[i] == endChar)
			{
				if (--unpaired == 0)
				{
					endCharIndex = i;
					return str.Remove(startCharIndex, endCharIndex - startCharIndex + 1);
				}
			}
		}
		return str;
	}
	// 从后往前去除两个成对个字符之间的所有字符(包含两个字符)，并返回他们的下标
	public static string removeLastBetweenPairChars(this string str, char startChar, char endChar, out int startCharIndex, out int endCharIndex)
	{
		startCharIndex = -1;
		endCharIndex = str.LastIndexOf(endChar);
		if (endCharIndex < 0)
		{
			return str;
		}

		// 未配对数量
		int unpaired = 1;
		for (int i = endCharIndex - 1; i >= 0; --i)
		{
			if (str[i] == endChar)
			{
				++unpaired;
			}
			else if (str[i] == startChar)
			{
				if (--unpaired == 0)
				{
					startCharIndex = i;
					return str.Remove(startCharIndex, endCharIndex - startCharIndex + 1);
				}
			}
		}
		return str;
	}
	public static bool hasLowerLetter(this string str)
	{
		int length = str.Length;
		for (int i = 0; i < length; ++i)
		{
			if (isLower(str[i]))
			{
				return true;
			}
		}
		return false;
	}
	public static string rightToLeft(this string str)
	{
		if (str == null)
		{
			return str;
		}
		return str.Replace('\\', '/');
	}
	public static string leftToRight(this string str)
	{
		if (str == null)
		{
			return null;
		}
		return str.Replace('/', '\\');
	}
	public static string addEndSlash(this string path)
	{
		if (!path.isEmpty() && path[^1] != '/')
		{
			path += "/";
		}
		return path;
	}
	// 去除字符串中所有非数字的字符串,也就是只包含0到9的字符,连小数点,负号都要去掉
	public static string keepNumberOnly(this string str)
	{
		using var a = new MyStringBuilderScope(out var builder);
		foreach (char c in str)
		{
			builder.addIf(c, isNumeric(c));
		}
		return builder.ToString();
	}
	public static string removeEndNumber(this string str)
	{
		if (isNumeric(str))
		{
			return "";
		}
		for (int i = str.Length - 1; i >= 0; --i)
		{
			if (!isNumeric(str[i]))
			{
				return str.startString(i + 1);
			}
		}
		return str;
	}
	public static byte[] toBytes(this string str, Encoding encoding = null)
	{
		if (str == null)
		{
			return null;
		}
		// 默认为UTF8
		return (encoding ?? Encoding.UTF8).GetBytes(str);
	}
	public static string convertStringFormat(this string str, Encoding source, Encoding target)
	{
		return str.toBytes(source).bytesToString(target);
	}
	public static string UTF8ToUnicode(this string str)
	{
		return convertStringFormat(str, Encoding.UTF8, Encoding.Unicode);
	}
	public static string UTF8ToGB2312(this string str)
	{
		return convertStringFormat(str, Encoding.UTF8, getGB2312());
	}
	public static string UnicodeToUTF8(this string str)
	{
		return convertStringFormat(str, Encoding.Unicode, Encoding.UTF8);
	}
	public static string UnicodeToGB2312(this string str)
	{
		return convertStringFormat(str, Encoding.Unicode, getGB2312());
	}
	public static string GB2312ToUTF8(this string str)
	{
		return convertStringFormat(str, getGB2312(), Encoding.UTF8);
	}
	public static string GB2312ToUnicode(this string str)
	{
		return convertStringFormat(str, getGB2312(), Encoding.Unicode);
	}
	public static void splitLine(this string str, out string[] lines, bool removeEmpty = true)
	{
		if (str.isEmpty())
		{
			lines = null;
			return;
		}
		lines = str.split(removeEmpty, '\n');
		for (int i = 0; i < lines.Length; ++i)
		{
			lines[i] = lines[i].removeAll('\r');
		}
	}
	public static string[] splitLine(this string str, bool removeEmpty = true)
	{
		str.splitLine(out string[] lines, removeEmpty);
		return lines;
	}
	public static string[] split(this string str, params string[] keyword)
	{
		return split(str, true, keyword);
	}
	public static string[] split(this string str, bool removeEmpty, params string[] keyword)
	{
		if (str.isEmpty())
		{
			return Array.Empty<string>();
		}
		return str.Split(keyword, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
	}
	public static string[] split(this string str, params char[] keyword)
	{
		return str.split(true, keyword);
	}
	public static string[] split(this string str, bool removeEmpty, params char[] keyword)
	{
		if (str.isEmpty())
		{
			return Array.Empty<string>();
		}
		return str.Split(keyword, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
	}
	public static int SToI(this string str)
	{
        checkIntString(str);
        if (str.isEmpty() || str == "-")
        {
            return 0;
        }
        if (mStringToInt == null)
        {
            initIntToString();
        }
        return mStringToInt.TryGetValue(str, out int value) ? value : int.Parse(str);
    }
    public static uint SToUInt(this string str)
    {
        checkUIntString(str);
        return !str.isEmpty() ? uint.Parse(str) : 0;
    }
    public static long SToL(this string str)
    {
        checkIntString(str);
        return !str.isEmpty() ? long.Parse(str) : 0;
    }
    public static ulong SToUL(this string str)
    {
        checkUIntString(str);
        return !str.isEmpty() ? ulong.Parse(str) : 0;
    }
    public static float SToF(this string str)
    {
        checkFloatString(str);
        if (str.isEmpty() || str == "-")
        {
            return 0.0f;
        }
        return float.Parse(str);
    }
    public static Vector2 SToV2(this string str, char separate = ',')
    {
        if (str.isEmpty() || str == "0,0")
        {
            return Vector2.zero;
        }
        string[] splitList = str.split(separate);
        if (splitList.count() < 2)
        {
            return Vector2.zero;
        }
        return new(splitList[0].SToF(), splitList[1].SToF());
    }
    public static Vector2Int SToV2I(this string str, char separate = ',')
    {
        if (str.isEmpty() || str == "0,0")
        {
            return Vector2Int.zero;
        }
        mStringToVector2Cache ??= new(STRING_TO_VECTOR2INT_MAX_CACHE);
        if (mStringToVector2Cache.TryGetValue(str, out Vector2Int result))
        {
            return result;
        }
        string[] splitList = str.split(separate);
        if (splitList.count() < 2)
        {
            return Vector2Int.zero;
        }
        result = new(splitList[0].SToI(), splitList[1].SToI());
        if (mStringToVector2Cache.Count < STRING_TO_VECTOR2INT_MAX_CACHE)
        {
            mStringToVector2Cache.Add(str, result);
        }
        return result;
    }
    public static Vector3 SToV3(this string str, char separate = ',')
    {
        if (str.isEmpty() || str == "0,0,0")
        {
            return Vector3.zero;
        }
        string[] splitList = str.split(separate);
        if (splitList.count() < 3)
        {
            return Vector3.zero;
        }
        return new(splitList[0].SToF(), splitList[1].SToF(), splitList[2].SToF());
    }
    public static Vector3Int SToV3I(this string str, char separate = ',')
    {
        if (str.isEmpty() || str == "0,0,0")
        {
            return Vector3Int.zero;
        }
        mStringToVector3Cache ??= new(STRING_TO_VECTOR2INT_MAX_CACHE);
        if (mStringToVector3Cache.TryGetValue(str, out Vector3Int result))
        {
            return result;
        }
        string[] splitList = str.split(separate);
        if (splitList.count() < 3)
        {
            return Vector3Int.zero;
        }
        result = new(splitList[0].SToI(), splitList[1].SToI(), splitList[2].SToI());
        if (mStringToVector3Cache.Count < STRING_TO_VECTOR2INT_MAX_CACHE)
        {
            mStringToVector3Cache.Add(str, result);
        }
        return result;
    }
    public static Vector4 SToV4(this string str, char separate = ',')
    {
        if (str.isEmpty() || str == "0,0,0,0")
        {
            return Vector4.zero;
        }
        string[] splitList = str.split(separate);
        if (splitList.count() < 4)
        {
            return Vector4.zero;
        }
        return new(splitList[0].SToF(), splitList[1].SToF(), splitList[2].SToF(), splitList[3].SToF());
    }
    public static Vector4Int SToV4I(this string str, char separate = ',')
    {
        if (str.isEmpty() || str == "0,0,0,0")
        {
            return Vector4Int.zero;
        }
        string[] splitList = str.split(separate);
        if (splitList.count() < 4)
        {
            return Vector4Int.zero;
        }
        return new(splitList[0].SToI(), splitList[1].SToI(), splitList[2].SToI(), splitList[3].SToI());
    }
    public static string IToS(this byte value, int minLength = 0)
    {
        return IToS((int)value, minLength);
    }
    public static string IToS(this sbyte value, int minLength = 0)
    {
        return IToS((int)value, minLength);
    }
    public static string IToS(this ushort value, int minLength = 0)
	{
        return IToS((int)value, minLength);
    }
    public static string IToS(this short value, int minLength = 0)
    {
        return IToS((int)value, minLength);
    }
    // minLength表示返回字符串的最少数字个数,等于0表示不限制个数,大于0表示如果转换后的数字数量不足minLength个,则在前面补0
    public static string IToS(this int value, int minLength = 0)
    {
        if (mIntToString == null)
        {
            initIntToString();
        }
        string retString;
        // 先尝试查表获取
        if (value >= 0 && value < mIntToString.Length)
        {
            retString = mIntToString[value];
        }
        else
        {
            retString = value.ToString();
        }
        int addLen = minLength - retString.Length;
        if (addLen > 0)
        {
            for (int i = 0; i < addLen; ++i)
            {
                retString = "0" + retString;
            }
        }
        return retString;
    }
    public static string IToS(this uint value, int minLength = 0)
    {
        if (mIntToString == null)
        {
            initIntToString();
        }
        string retString;
        // 先尝试查表获取
        if (value >= 0 && value < mIntToString.Length)
        {
            retString = mIntToString[value];
        }
        else
        {
            retString = value.ToString();
        }
        int addLen = minLength - retString.Length;
        if (addLen > 0)
        {
            for (int i = 0; i < addLen; ++i)
            {
                retString = "0" + retString;
            }
        }
        return retString;
    }
    public static string IToSComma(this int value)
    {
        return insertNumberComma(value.IToS());
    }
    public static string IToSComma(this uint value)
    {
        return insertNumberComma(value.IToS());
    }
    public static string FToS(this float value, int precision = 4, bool removeTailZero = true)
    {
        checkInt(ref value);
		int intValue = (int)value;
        if (precision == 0)
        {
            return intValue.IToS();
        }
        if (removeTailZero)
        {
            // 是否非常接近数轴左边的整数
            if (isFloatZero(value - intValue))
            {
                return intValue.IToS();
            }
            // 是否非常接近数轴右边的整数
            if (isFloatZero(intValue + 1 - value))
            {
                return IToS(intValue + 1);
            }
        }
        string str = value.ToString(mFloatConvertPercision[precision]);
        // 去除末尾的0
        if (removeTailZero && str.IndexOf('.') != -1)
        {
            int removeCount = 0;
            int curLen = str.Length;
            // 从后面开始遍历
            for (int i = 0; i < curLen; ++i)
            {
                char c = str[curLen - 1 - i];
                // 遇到不是0的就退出循环
                if (c != '0' && c != '.')
                {
                    removeCount = i;
                    break;
                }
                // 遇到小数点就退出循环并且需要将小数点一起去除
                else if (c == '.')
                {
                    removeCount = i + 1;
                    break;
                }
            }
            str = str.removeEndCount(removeCount);
        }
        return str;
    }
    public static string LToS(this long value, int minLength = 0)
    {
        if (mIntToString == null)
        {
            initIntToString();
        }
        string retString;
        // 先尝试查表获取
        if (value >= 0 && value < mIntToString.Length)
        {
            retString = mIntToString[value];
        }
        else
        {
            retString = value.ToString();
        }
        int addLen = minLength - retString.Length;
        if (addLen > 0)
        {
            retString = mZeroStringList[addLen] + retString;
        }
        return retString;
    }
    public static string LToSComma(this long value)
    {
        return insertNumberComma(value.LToS());
    }
    public static string LToS(this ulong value, int minLength = 0)
    {
        if (mIntToString == null)
        {
            initIntToString();
        }
        string retString;
        // 先尝试查表获取
        if (value >= 0 && value < (ulong)mIntToString.Length)
        {
            retString = mIntToString[value];
        }
        else
        {
            retString = value.ToString();
        }
        int addLen = minLength - retString.Length;
        if (addLen > 0)
        {
            retString = mZeroStringList[addLen] + retString;
        }
        return retString;
    }
    public static string LToSComma(this ulong value)
    {
        return insertNumberComma(value.LToS());
    }
    public static string V2ToS(this Vector2 value, int precision = 4)
    {
        return value.x.FToS(precision) + "," + value.y.FToS(precision);
    }
    public static string V2IToS(this Vector2Int value, int minLength = 0)
    {
        return value.x.IToS(minLength) + "," + value.y.IToS(minLength);
    }
    public static string V3ToS(this Vector3 value, int precision = 4)
    {
        return strcat(value.x.FToS(precision), ",", value.y.FToS(precision), ",", value.z.FToS(precision));
    }
    public static string V3IToS(this Vector3Int value, int minLength = 0)
    {
        return strcat(value.x.IToS(minLength), ",", value.y.IToS(minLength), ",", value.z.IToS(minLength));
    }
    public static string V4ToS(this Vector4 value, int precision = 4)
    {
        return strcat(value.x.FToS(precision), ",", value.y.FToS(precision), ",", value.z.FToS(precision), ",", value.w.FToS(precision));
    }
    public static string V4IToS(this Vector4Int value, int minLength = 0)
    {
        return strcat(value.x.IToS(minLength), ",", value.y.IToS(minLength), ",", value.z.IToS(minLength), value.w.IToS(minLength));
    }
    // 在使用返回值期间禁止再调用stringToStrings
    public static List<string> stringToStrings(this string str, bool removeEmpty, params string[] keyword)
    {
        mTempStringList.Clear();
        if (!str.isEmpty())
        {
            mTempStringList.AddRange(str.split(removeEmpty, keyword));
        }
        return mTempStringList;
    }
    // 在使用返回值期间禁止再调用stringToStrings
    public static List<string> stringToStrings(this string str, bool removeEmpty, params char[] keyword)
    {
        mTempStringList.Clear();
        if (!str.isEmpty())
        {
            mTempStringList.AddRange(str.split(removeEmpty, keyword));
        }
        return mTempStringList;
    }
    // 在使用返回值期间禁止再调用stringToFloatsNonAlloc
    public static List<float> SToFsNonAlloc(this string str, char separate = ',')
    {
        SToFs(str, mTempFloatList, separate);
        return mTempFloatList;
    }
    public static List<float> SToFs(this string str, char separate = ',')
    {
        List<float> values = new();
        SToFs(str, values, separate);
        return values;
    }
    public static void SToFs(this string str, ref float[] values, char separate = ',')
    {
        if (str.isEmpty())
        {
            return;
        }
        string[] rangeList = str.split(separate);
        int len = rangeList.Length;
        if (values != null && len != values.Length)
        {
            logError("count is not equal " + str.Length);
            return;
        }
        values ??= new float[len];
        for (int i = 0; i < len; ++i)
        {
            values[i] = rangeList[i].SToF();
        }
    }
    public static void SToFs(this string str, List<float> values, char separate = ',')
    {
        if (values == null)
        {
            logError("values can not be null");
            return;
        }
        values.Clear();
        if (str.isEmpty())
        {
            return;
        }
        foreach (string item in str.split(separate))
        {
            values.Add(item.SToF());
        }
    }
    public static string FsToS(this List<float> values, char separate = ',')
    {
        using var a = new MyStringBuilderScope(out var builder);
        int count = values.Count;
        for (int i = 0; i < count; ++i)
        {
            builder.add(values[i].FToS(2));
            builder.addIf(separate, i != count - 1);
        }
        return builder.ToString();
    }
    public static List<long> SToLs(this string str, char separate = ',')
    {
        List<long> values = new();
        SToLs(str, values, separate);
        return values;
    }
    public static void SToLs(this string str, List<long> values, char separate = ',')
    {
        if (values == null)
        {
            logError("values can not be null");
            return;
        }
        values.Clear();
        if (str.isEmpty())
        {
            return;
        }
        foreach (string item in str.split(separate).safe())
        {
            values.Add(item.SToL());
        }
    }
    public static void SToLs(this string str, ref long[] values, char separate = ',')
    {
        if (str.isEmpty())
        {
            return;
        }
        string[] rangeList = str.split(separate);
        int len = rangeList.Length;
        if (values != null && len != values.Length)
        {
            logError("value.Length is not equal " + len + ", str:" + str);
            return;
        }
        values ??= new long[len];
        for (int i = 0; i < len; ++i)
        {
            values[i] = rangeList[i].SToL();
        }
    }
    // 在使用返回值期间禁止再调用stringToLongsNonAlloc
    public static List<long> SToLsNonAlloc(this string str, char separate = ',')
    {
        SToLs(str, mTempLongList, separate);
        return mTempLongList;
    }
    public static List<int> SToIs(this string str, char separate = ',')
    {
        List<int> values = new();
        SToIs(str, values, separate);
        return values;
    }
    public static void SToIs(this string str, List<int> values, char separate = ',')
    {
        if (values == null)
        {
            logError("values can not be null");
            return;
        }
        values.Clear();
        if (str.isEmpty())
        {
            return;
        }
        foreach (string item in str.split(separate).safe())
        {
            values.Add(item.SToI());
        }
    }
    public static void SToIs(this string str, ref int[] values, char separate = ',')
    {
        if (str.isEmpty())
        {
            return;
        }
        string[] rangeList = str.split(separate);
        int len = rangeList.Length;
        if (values != null && len != values.Length)
        {
            logError("value.Length is not equal " + len + ", str:" + str);
            return;
        }
        values ??= new int[len];
        for (int i = 0; i < len; ++i)
        {
            values[i] = rangeList[i].SToI();
        }
    }
    // 在使用返回值期间禁止再调用stringToIntsNonAlloc
    public static List<int> SToIsNonAlloc(this string str, char separate = ',')
    {
        SToIs(str, mTempIntList, separate);
        return mTempIntList;
    }
    public static void SToUIs(this string str, List<uint> values, char separate = ',')
    {
        if (values == null)
        {
            logError("values can not be null");
            return;
        }
        values.Clear();
        if (str.isEmpty())
        {
            return;
        }
        foreach (string item in str.split(separate).safe())
        {
            values.Add((uint)item.SToI());
        }
    }
    public static void SToUSs(this string str, List<ushort> values, char separate = ',')
    {
        if (values == null)
        {
            logError("values can not be null");
            return;
        }
        values.Clear();
        if (str.isEmpty())
        {
            return;
        }
        foreach (string item in str.split(separate).safe())
        {
            values.Add((ushort)item.SToI());
        }
    }
    public static void SToSs(this string str, List<short> values, char separate = ',')
    {
        if (values == null)
        {
            logError("values can not be null");
            return;
        }
        values.Clear();
        if (str.isEmpty())
        {
            return;
        }
        foreach (string item in str.split(separate).safe())
        {
            values.Add((short)item.SToI());
        }
    }
    public static void SToBools(this string str, List<bool> values, char separate = ',')
    {
        if (values == null)
        {
            logError("values can not be null");
            return;
        }
        values.Clear();
        if (str.isEmpty())
        {
            return;
        }
        foreach (string item in str.split(separate).safe())
        {
            values.Add(item.SToI() > 0);
        }
    }
    // 在使用返回值期间不允许再使用此函数
    public static List<byte> SToBsNonAlloc(this string str, char separate = ',')
    {
        mTempByteList0.Clear();
        if (str.isEmpty())
        {
            return mTempByteList0;
        }
        foreach (string item in str.split(separate).safe())
        {
            mTempByteList0.Add((byte)item.SToI());
        }
        return mTempByteList0;
    }
    public static void SToBs(this string str, List<byte> values, char separate = ',')
    {
        if (values == null)
        {
            logError("values can not be null");
            return;
        }
        values.Clear();
        if (str.isEmpty())
        {
            return;
        }
        foreach (string item in str.split(separate).safe())
        {
            values.Add((byte)item.SToI());
        }
    }
    public static void SToSBs(this string str, List<sbyte> values, char separate = ',')
    {
        if (values == null)
        {
            logError("values can not be null");
            return;
        }
        values.Clear();
        if (str.isEmpty())
        {
            return;
        }
        foreach (string item in str.split(separate).safe())
        {
            values.Add((sbyte)item.SToI());
        }
    }
    public static string LsToS(this List<long> values, char separate = ',')
    {
        if (values.isEmpty())
        {
            return EMPTY;
        }
        using var a = new MyStringBuilderScope(out var builder);
        int count = values.Count;
        for (int i = 0; i < count; ++i)
        {
            builder.add(values[i].LToS());
            builder.addIf(separate, i != count - 1);
        }
        return builder.ToString();
    }
    public static string IsToS(this List<int> values, char separate = ',')
    {
        if (values.isEmpty())
        {
            return EMPTY;
        }
        using var a = new MyStringBuilderScope(out var builder);
        int count = values.Count;
        for (int i = 0; i < count; ++i)
        {
            builder.add(values[i].IToS());
            builder.addIf(separate, i != count - 1);
        }
        return builder.ToString();
    }
    public static string stringsToString(this List<string> values, string separate)
    {
        if (values.isEmpty())
        {
            return EMPTY;
        }
        using var a = new MyStringBuilderScope(out var builder);
        int count = values.Count;
        for (int i = 0; i < count; ++i)
        {
            builder.add(values[i]);
            builder.addIf(separate, i != count - 1);
        }
        return builder.ToString();
    }
    public static string stringsToString(this string[] values, string separate)
    {
        if (values.isEmpty())
        {
            return EMPTY;
        }
        using var a = new MyStringBuilderScope(out var builder);
        int count = values.Length;
        for (int i = 0; i < count; ++i)
        {
            builder.add(values[i]);
            builder.addIf(separate, i != count - 1);
        }
        return builder.ToString();
    }
    public static string stringsToString(this List<string> values, char separate = ',')
    {
        if (values.isEmpty())
        {
            return EMPTY;
        }
        using var a = new MyStringBuilderScope(out var builder);
        int count = values.Count;
        for (int i = 0; i < count; ++i)
        {
            builder.add(values[i]);
            builder.addIf(separate, i != count - 1);
        }
        return builder.ToString();
    }
    public static string stringsToString(this string[] values, char separate = ',')
    {
        if (values.isEmpty())
        {
            return EMPTY;
        }
        using var a = new MyStringBuilderScope(out var builder);
        int count = values.Length;
        for (int i = 0; i < count; ++i)
        {
            builder.add(values[i]);
            builder.addIf(separate, i != count - 1);
        }
        return builder.ToString();
    }
    public static List<string> stringToStrings(this string str, char separate = ',')
    {
        List<string> strList = new();
        if (!str.isEmpty())
        {
            strList.addRange(str.split(separate).safe());
        }
        return strList;
    }
    public static void stringToStrings(this string str, List<string> values, char separate = ',')
    {
        if (values == null)
        {
            logError("values can not be null");
            return;
        }
        values.Clear();
        if (str.isEmpty())
        {
            return;
        }
        foreach (string item in str.split(separate).safe())
        {
            values.Add(item);
        }
    }
    public static string boolToString(this bool value, bool firstUpper = false, bool fullUpper = false)
    {
        if (fullUpper)
        {
            return value ? "TRUE" : "FALSE";
        }
        if (firstUpper)
        {
            return value ? "True" : "False";
        }
        return value ? "true" : "false";
    }
    public static bool stringToBool(this string str)
    {
        return str == "true" || str == "True" || str == "TRUE";
    }
    public static Color SToColor(this string str)
    {
        if (str[0] != '#')
        {
            str = "#" + str;
        }
        ColorUtility.TryParseHtmlString(str, out Color color);
        return color;
    }
    // 百分比一般用于属性增幅之类的
    public static string toPercent(this string value, int precision = 1) { return (value.SToF() * 100).FToS(precision) + "%"; }
    public static string toPercent(this float value, int precision = 1) { return (value * 100).FToS(precision) + "%"; }
    // 几率类的一般是万分比的格式填写的,10000表示100%
    public static string toProbability(this string value) { return (value.SToF() * 0.01f).FToS() + "%"; }
    public static string toProbability(this float value) { return (value * 0.01f).FToS() + "%"; }
    public static string toProbability(this int value) { return (value * 0.01f).FToS() + "%"; }
    public static string fixedAndPercent(this int value, float percent)
    {
        if (value > 0 && percent > 0.0f)
        {
            return value.IToS() + "+" + toPercent(percent);
        }
        if (value > 0)
        {
            return value.IToS();
        }
        if (percent > 0.0f)
        {
            return toPercent(percent);
        }
        return "";
    }
	// 使用制表符将字符串补到指定的显示长度
	public static string fixedLength(this string str, int fixedLength)
	{
		if (str.Length >= fixedLength)
		{
			return str;
		}
		int needLength = fixedLength - str.Length;
		int needTable = needLength / 4 + (needLength % 4 == 0 ? 0 : 1);
		for (int i = 0; i < needTable; ++i)
		{
			str += "\t";
		}
		return str;
	}
	public static void initIntToString()
    {
        if (mIntToString != null || mStringToInt != null)
        {
            return;
        }
        mIntToString = new string[10240];
        mStringToInt = new();
        for (int i = 0; i < mIntToString.Length; ++i)
        {
            string iStr = i.ToString();
            mStringToInt.Add(iStr, i);
            mIntToString[i] = iStr;
        }
		for (int i = 0; i < 100; ++i)
		{
			string zeroStr = "";
			for (int j = 0; j < i; ++j)
			{
				zeroStr += "0";
            }
            mZeroStringList.add(zeroStr);
        }
    }
}
