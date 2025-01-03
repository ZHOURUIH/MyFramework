using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static MathUtility;
using static UnityUtility;
using static CSharpUtility;
using static FrameEditorUtility;

// 字符串相关工具函数类
public class StringUtility
{
	private static char[] mHexUpperChar = new char[] { 'A', 'B', 'C', 'D', 'E', 'F' };	// 十六进制中的大写字母
	private static char[] mHexLowerChar = new char[] { 'a', 'b', 'c', 'd', 'e', 'f' };	// 十六进制中的小写字母
	private static Dictionary<string, int> mStringToInt;								// 用于快速查找字符串转换后的整数
	private static string[] mIntToString;												// 用于快速获取整数转换后的字符串
	private static string[] mFloatConvertPercision = new string[] { "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7" };	// 浮点数转换时精度
	public const string EMPTY = "";                                                     // 表示空字符串
	// 只能使用{index}拼接
	public static string format(string format, string args)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(format);
		string indexStr = "{0}";
		if (findFirstSubstr(format, indexStr) >= 0)
		{
			replaceAll(builder, indexStr, args);
		}
		return builder.ToString();
	}
	// 只能使用{index}拼接
	public static string format(string format, string args0, string args1)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(format);
		string indexStr0 = "{0}";
		if (findFirstSubstr(format, indexStr0) >= 0)
		{
			replaceAll(builder, indexStr0, args0);
		}
		string indexStr1 = "{1}";
		if (findFirstSubstr(format, indexStr1) >= 0)
		{
			replaceAll(builder, indexStr1, args1);
		}
		return builder.ToString();
	}
	public static bool startWith(string oriString, string pattern, bool sensitive = true)
	{
		if (oriString.Length < pattern.Length)
		{
			return false;
		}
		string startString = oriString.startString(pattern.Length);
		if (sensitive)
		{
			return startString == pattern;
		}
		else
		{
			return startString.ToLower() == pattern.ToLower();
		}
	}
	public static bool endWith(string oriString, string pattern, bool sensitive = true)
	{
		if (oriString.Length < pattern.Length)
		{
			return false;
		}
		string endString = oriString.endString(pattern.Length);
		if (sensitive)
		{
			return endString == pattern;
		}
		else
		{
			return endString.ToLower() == pattern.ToLower();
		}
	}
	public static long SToL(string str)
	{
		checkIntString(str);
		return !str.isEmpty() ? long.Parse(str) : 0;
	}
	// 如果originStr以startString为开头,则移除originStr结尾的startString
	public static void removeStartString(ref string originStr, string startString, bool sensitive = true)
	{
		originStr = removeStartString(originStr, startString, sensitive);
	}
	public static string removeStartString(string originStr, string startString, bool sensitive = true)
	{
		if (startString.isEmpty() ||
			originStr.isEmpty() ||
			!startWith(originStr, startString, sensitive))
		{
			return originStr;
		}
		return originStr.removeStartCount(startString.Length);
	}
	// 得到文件路径
	public static string getFilePath(string fileName, bool keepEndSlash = false)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			builder.append(fileName);
			rightToLeft(builder);
			// 从倒数第二个开始,因为即使最后一个是/也需要忽略
			int lastPos = builder.lastIndexOf('/', builder.Length - 2);
			if (lastPos < 0)
			{
				return EMPTY;
			}
			builder.remove(lastPos + (keepEndSlash ? 1 : 0));
			return builder.ToString();
		}
		else
		{
			using var a = new ClassThreadScope<MyStringBuilder>(out var builder);
			builder.append(fileName);
			rightToLeft(builder);
			// 从倒数第二个开始,因为即使最后一个是/也需要忽略
			int lastPos = builder.lastIndexOf('/', builder.Length - 2);
			if (lastPos < 0)
			{
				return EMPTY;
			}
			builder.remove(lastPos + (keepEndSlash ? 1 : 0));
			return builder.ToString();
		}
	}
	// 获取文件名,带后缀
	public static string getFileNameWithSuffix(string str)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(str);
		rightToLeft(builder);
		int dotPos = builder.lastIndexOf('/');
		if (dotPos != -1)
		{
			builder.remove(0, dotPos + 1);
		}
		return builder.ToString();
	}
	public static string getFileNameThread(string str)
	{
		string builder = rightToLeft(str);
		int dotPos = builder.LastIndexOf('/');
		if (dotPos != -1)
		{
			builder = builder.Remove(0, dotPos + 1);
		}
		return builder;
	}
	public static string getFileSuffix(string file)
	{
		int dotPos = file.IndexOf('.', file.LastIndexOf('/'));
		if (dotPos != -1)
		{
			return file.removeStartCount(dotPos);
		}
		return EMPTY;
	}
	// 移除文件的后缀名
	public static string removeSuffix(string str)
	{
		if (str == null)
		{
			return null;
		}
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(str);
		rightToLeft(builder);
		// 移除后缀
		int dotPos = builder.lastIndexOf('.');
		if (dotPos != -1)
		{
			builder.remove(dotPos);
		}
		return builder.ToString();
	}
	// 获取不带后缀的文件名
	public static string getFileNameNoSuffixNoDir(string str)
	{
		if (str == null)
		{
			return null;
		}
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(str);
		rightToLeft(builder);
		int namePos = builder.lastIndexOf('/');
		if (namePos != -1)
		{
			builder.remove(0, namePos + 1);
		}
		// 移除后缀
		int dotPos = builder.lastIndexOf('.');
		if (dotPos != -1)
		{
			builder.remove(dotPos);
		}
		return builder.ToString();
	}
	// 如果路径最后有斜杠,则移除结尾的斜杠
	public static void removeEndSlash(ref string path)
	{
		if (path.isEmpty())
		{
			return;
		}
		if (path[^1] == '/' || path[^1] == '\\')
		{
			path = path.removeEndCount(1);
		}
	}
	public static string removeEndSlash(string path)
	{
		if (path.isEmpty())
		{
			return path;
		}
		if (path[^1] == '/' || path[^1] == '\\')
		{
			path = path.removeEndCount(1);
		}
		return path;
	}
	public static void addEndSlash(ref string path)
	{
		if (!path.isEmpty() && path[^1] != '/')
		{
			path += "/";
		}
	}
	public static void rightToLeft(MyStringBuilder str)
	{
		str.replace('\\', '/');
	}
	public static void rightToLeft(ref string str)
	{
		if (str == null)
		{
			return;
		}
		str = str.Replace('\\', '/');
	}
	public static string rightToLeft(string str)
	{
		if (str == null)
		{
			return null;
		}
		return str.Replace('\\', '/');
	}
	public static void splitLine(string str, out string[] lines, bool removeEmpty = true)
	{
		if (str.isEmpty())
		{
			lines = null;
			return;
		}
		if (str.Contains("\r\n"))
		{
			lines = split(str, removeEmpty, "\r\n");
		}
		else if (str.Contains("\n"))
		{
			lines = split(str, removeEmpty, '\n');
		}
		else if (str.Contains("\r"))
		{
			lines = split(str, removeEmpty, '\r');
		}
		else
		{
			lines = new string[1] { str };
		}
	}
	public static string[] splitLine(string str, bool removeEmpty = true)
	{
		splitLine(str, out string[] lines, removeEmpty);
		return lines;
	}
	public static string[] split(string str, bool removeEmpty, params string[] keyword)
	{
		if (str.isEmpty())
		{
			return null;
		}
		return str.Split(keyword, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
	}
	public static string[] split(string str, params char[] keyword)
	{
		return split(str, true, keyword);
	}
	public static string[] split(string str, bool removeEmpty, params char[] keyword)
	{
		if (str.isEmpty())
		{
			return null;
		}
		return str.Split(keyword, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
	}
	public static List<long> SToLs(string str, char seperate = ',')
	{
		List<long> values = new();
		SToLs(str, values, seperate);
		return values;
	}
	public static void SToLs(string str, IList<long> values, char seperate = ',')
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
		foreach (string item in split(str, seperate).safe())
		{
			values.Add(SToL(item));
		}
	}
	public static string stringsToString(IList<string> values, char seperate = ',')
	{
		if (values.isEmpty())
		{
			return EMPTY;
		}
		using var a = new MyStringBuilderScope(out var builder);
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.append(values[i]);
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return builder.ToString();
	}
	// precision表示小数点后保留几位小数,removeTailZero表示是否去除末尾的0
	public static string FToS(float value, int precision = 4, bool removeTailZero = true)
	{
		checkInt(ref value);
		if (precision == 0)
		{
			return IToS((int)value);
		}
		if (removeTailZero)
		{
			// 是否非常接近数轴左边的整数
			if (isFloatZero(value - (int)value))
			{
				return IToS((int)value);
			}
			// 是否非常接近数轴右边的整数
			if (isFloatZero((int)value + 1 - value))
			{
				return IToS((int)value + 1);
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
	public static string boolToString(bool value, bool firstUpper = false, bool fullUpper = false)
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
	public static bool stringToBool(string str)
	{
		return str == "true" || str == "True" || str == "TRUE";
	}
	// minLength表示返回字符串的最少数字个数,等于0表示不限制个数,大于0表示如果转换后的数字数量不足minLength个,则在前面补0
	public static string IToS(int value, int minLength = 0)
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
	public static string IToS(uint value, int minLength = 0)
	{
		return IToS((int)value, minLength);
	}
	public static string LToS(long value, int minLength = 0)
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
	public static string ULToS(ulong value, int minLength = 0)
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
			for (int i = 0; i < addLen; ++i)
			{
				retString = "0" + retString;
			}
		}
		return retString;
	}
	public static string V2ToS(Vector2 value, int precision = 4)
	{
		return FToS(value.x, precision) + "," + FToS(value.y, precision);
	}
	public static string V3ToS(Vector3 value, int precision = 4)
	{
		return strcat(FToS(value.x, precision), ",", FToS(value.y, precision), ",", FToS(value.z, precision));
	}
	public static void replace(MyStringBuilder str, int begin, int end, string reStr)
	{
		str.remove(begin, end - begin);
		if (!reStr.isEmpty())
		{
			str.insert(begin, reStr);
		}
	}
	public static void replaceAll(MyStringBuilder builder, string key, string newWords)
	{
		int startPos = 0;
		while (true)
		{
			int pos = findFirstSubstr(builder, key, startPos);
			if (pos < 0)
			{
				break;
			}
			replace(builder, pos, pos + key.Length, newWords);
			startPos = pos + newWords.length();
		}
	}
	public static string removeAll(string str, params string[] key)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(str);
		int keyCount = key.Length;
		for (int i = 0; i < keyCount; ++i)
		{
			replaceAll(builder, key[i], EMPTY);
		}
		return builder.ToString();
	}
	public static bool checkString(string str, string valid)
	{
		if (isEditor())
		{
			int oldStrLen = str.Length;
			for (int i = 0; i < oldStrLen; ++i)
			{
				if (valid.IndexOf(str[i]) < 0)
				{
					logError("不合法的字符串:" + str);
					return false;
				}
			}
		}
		return true;
	}
	public static bool checkIntString(string str, string valid = EMPTY)
	{
		if (isEditor())
		{
			return checkString(str, "-0123456789" + valid);
		}
		return true;
	}
	public static string bytesToHEXString(byte[] byteList, int offset = 0, int count = 0, bool addSpace = true, bool upperOrLower = true)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			int byteCount = count > 0 ? count : byteList.Length - offset;
			clamp(ref byteCount, 0, byteList.Length - offset);
			for (int i = 0; i < byteCount; ++i)
			{
				if (addSpace)
				{
					byteToHEXString(builder, byteList[i + offset], upperOrLower);
					if (i != byteCount - 1)
					{
						builder.append(' ');
					}
				}
				else
				{
					byteToHEXString(builder, byteList[i + offset], upperOrLower);
				}
			}
			return builder.ToString();
		}
		else
		{
			StringBuilder builder = new();
			int byteCount = count > 0 ? count : byteList.Length - offset;
			clamp(ref byteCount, 0, byteList.Length - offset);
			for (int i = 0; i < byteCount; ++i)
			{
				if (addSpace)
				{
					byteToHEXStringThread(builder, byteList[i + offset], upperOrLower);
					if (i != byteCount - 1)
					{
						builder.Append(' ');
					}
				}
				else
				{
					byteToHEXStringThread(builder, byteList[i + offset], upperOrLower);
				}
			}
			return builder.ToString();
		}
	}
	public static void byteToHEXStringThread(StringBuilder builder, byte value, bool upperOrLower = true)
	{
		char[] hexChar = upperOrLower ? mHexUpperChar : mHexLowerChar;
		// 等效于int high = value / 16;
		// 等效于int low = value % 16;
		int high = value >> 4;
		int low = value & 15;
		if (high < 10)
		{
			builder.Append((char)('0' + high));
		}
		else
		{
			builder.Append(hexChar[high - 10]);
		}
		if (low < 10)
		{
			builder.Append((char)('0' + low));
		}
		else
		{
			builder.Append(hexChar[low - 10]);
		}
	}
	public static void byteToHEXString(MyStringBuilder builder, byte value, bool upperOrLower = true)
	{
		char[] hexChar = upperOrLower ? mHexUpperChar : mHexLowerChar;
		// 等效于int high = value / 16;
		// 等效于int low = value % 16;
		int high = value >> 4;
		int low = value & 15;
		if (high < 10)
		{
			builder.append((char)('0' + high));
		}
		else
		{
			builder.append(hexChar[high - 10]);
		}
		if (low < 10)
		{
			builder.append((char)('0' + low));
		}
		else
		{
			builder.append(hexChar[low - 10]);
		}
	}
	public static string fileSizeString(long size)
	{
		// 不足1KB
		if (size < 1024)
		{
			return IToS((int)size) + "B";
		}
		// 不足1MB
		if (size < 1024 * 1024)
		{
			return FToS(size * (1.0f / 1024.0f), 1) + "KB";
		}
		// 不足1GB
		if (size < 1024 * 1024 * 1024)
		{
			return FToS(size * (1.0f / (1024.0f * 1024.0f)), 1) + "MB";
		}
		// 大于1GB
		return FToS(size * (1.0f / (1024.0f * 1024.0f * 1024.0f)), 1) + "GB";
	}
	// 在文本显示中将str的颜色设置为color
	public static string colorStringNoBuilder(string color, string str)
	{
		if (str.isEmpty())
		{
			return EMPTY;
		}
		return "<color=#" + color + ">" + str + "</color>";
	}
	// returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
	public static int findFirstSubstr(string res, string pattern, int startPos = 0, bool returnEndIndex = false, bool sensitive = true)
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
				continue;
			}
			int j = 0;
			// 大小写敏感
			if (sensitive)
			{
				for (; j < subLen; ++j)
				{
					if (i + j >= 0 && i + j < len && res[i + j] != pattern[j])
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
					if (i + j >= 0 && i + j < len && toLower(res[i + j]) != toLower(pattern[j]))
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
	// returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
	public static int findFirstSubstr(MyStringBuilder res, string pattern, int startPos = 0, bool returnEndIndex = false, bool sensitive = true)
	{
		if (res == null || res.Length < pattern.Length)
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
				continue;
			}
			int j = 0;
			// 大小写敏感
			if (sensitive)
			{
				for (; j < subLen; ++j)
				{
					if (i + j >= 0 && i + j < len && res[i + j] != pattern[j])
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
					if (i + j >= 0 && i + j < len && toLower(res[i + j]) != toLower(pattern[j]))
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
	// 将字符转换为小写字母
	public static char toLower(char c)
	{
		if (isUpper(c))
		{
			return (char)(c + ('a' - 'A'));
		}
		return c;
	}
	// 字符是否为大写字母
	public static bool isUpper(char c) { return c >= 'A' && c <= 'Z'; }
	// 字符串拼接,当拼接小于等于4个字符串时,直接使用+号最快,GC与StringBuilder一致
	public static string strcat(string str0, string str1, string str2, string str3, string str4)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			return builder.append(str0, str1, str2, str3, str4).ToString();
		}
		else
		{
			using var a = new ClassThreadScope<MyStringBuilder>(out var builder);
			return builder.append(str0, str1, str2, str3, str4).ToString();
		}
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5).ToString();
		}
		else
		{
			using var a = new ClassThreadScope<MyStringBuilder>(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5).ToString();
		}
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5, str6).ToString();
		}
		else
		{
			using var a = new ClassThreadScope<MyStringBuilder>(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5, str6).ToString();
		}
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5, str6, str7).ToString();
		}
		else
		{
			using var a = new ClassThreadScope<MyStringBuilder>(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5, str6, str7).ToString();
		}
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5, str6, str7, str8).ToString();
		}
		else
		{
			using var a = new ClassThreadScope<MyStringBuilder>(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5, str6, str7, str8).ToString();
		}
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9).ToString();
		}
		else
		{
			using var a = new ClassThreadScope<MyStringBuilder>(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9).ToString();
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void initIntToString()
	{
		mIntToString = new string[10240];
		mStringToInt = new();
		for (int i = 0; i < mIntToString.Length; ++i)
		{
			string iStr = i.ToString();
			mStringToInt.Add(iStr, i);
			mIntToString[i] = iStr;
		}
	}
}