using static StringUtility;
using static FrameUtility;
using static FrameBaseUtility;

public static class StringExtension
{
	public static int length(this string list) { return list?.Length ?? 0; }
	public static bool isEmpty(this string str) { return str == null || str.Length == 0; }
	public static bool contains(this string str, char c) { return str != null && str.Contains(c); }
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
	// 截取第一个key字符之前的字符串,不包含key
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
	public static string removeStartString(this string str, string pattern, bool caseSensive = true)
	{
		if (str == null || pattern == null || str.Length < pattern.Length)
		{
			return str;
		}
		bool needRemove;
		if (caseSensive)
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
	public static string removeStartString(this string str, char pattern)
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
	public static string removeEndString(this string str, string pattern, bool caseSensive = true)
	{
		if (str == null || pattern == null || str.Length < pattern.Length)
		{
			return str;
		}
		bool needRemove;
		if (caseSensive)
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
	public static string removeEndString(this string str, char pattern)
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
		builder.append(str);
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
		builder.append(str);
		for (int i = builder.Length - 1; i >= 0; --i)
		{
			// 判断是否是需要移除的字符
			if (arrayContains(key, builder[i]))
			{
				builder.remove(i, 1);
			}
		}
		return builder.ToString();
	}
	public static string removeAll(this string str, char key)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(str);
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
			builder.append(str);
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
			builder.append(str);
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
			builder.append(str);
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
	public static string replaceAll(string str, char key, char newWords)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(str);
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
			if (isNumeric(c))
			{
				builder.append(c);
			}
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
}