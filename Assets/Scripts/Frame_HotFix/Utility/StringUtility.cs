using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static MathUtility;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 字符串相关工具函数类
public class StringUtility
{
	public static char[] mHexUpperChar = new char[] { 'A', 'B', 'C', 'D', 'E', 'F' };   // 十六进制中的大写字母
	public static char[] mHexLowerChar = new char[] { 'a', 'b', 'c', 'd', 'e', 'f' };	// 十六进制中的小写字母
	private static string mHexString = "ABCDEFabcdef0123456789";                        // 十六进制中的所有字符
	private static List<byte> mTempByteList0 = new();									// 避免GC,给stringToBytesNonAlloc使用的
	private static List<int> mTempIntList = new();										// 避免GC
	private static List<long> mTempLongList = new();									// 避免GC
	private static List<float> mTempFloatList = new();									// 避免GC
	private static List<string> mTempStringList = new();								// 避免GC
	private static Dictionary<string, int> mStringToInt;								// 用于快速查找字符串转换后的整数
	private static string[] mIntToString;												// 用于快速获取整数转换后的字符串
	private static string[] mFloatConvertPercision = new string[] { "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7" };	// 浮点数转换时精度
	private static Dictionary<string, Vector2Int> mStringToVector2Cache;				// 字符串转换为2维向量的缓存
	private static Dictionary<string, Vector3Int> mStringToVector3Cache;				// 字符串转换为3维向量的缓存
	private static int STRING_TO_VECTOR2INT_MAX_CACHE = 10240;                          // mStringToVector2Cache最大数量
	private static Dictionary<string, string> mInvalidParamChars;                       // invalid characters that cannot be found in a valid method-verb or http header
	private static List<char> mChineseSymbol;											// 中文的标点
	public const string EMPTY = "";                                                     // 表示空字符串
	// 只能使用{index}拼接
	public static string format(string format, string args)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(format);
		string indexStr = "{0}";
		if (format.findFirstSubstr(indexStr) >= 0)
		{
			builder.replaceAll(indexStr, args);
		}
		return builder.ToString();
	}
	// 只能使用{index}拼接
	public static string format(string format, string args0, string args1)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(format);
		string indexStr0 = "{0}";
		if (format.findFirstSubstr(indexStr0) >= 0)
		{
			builder.replaceAll(indexStr0, args0);
		}
		string indexStr1 = "{1}";
		if (format.findFirstSubstr(indexStr1) >= 0)
		{
			builder.replaceAll(indexStr1, args1);
		}
		return builder.ToString();
	}
	// 只能使用{index}拼接
	public static string format(string format, string args0, string args1, string args2)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(format);
		string indexStr0 = "{0}";
		if (format.findFirstSubstr(indexStr0) >= 0)
		{
			builder.replaceAll(indexStr0, args0);
		}
		string indexStr1 = "{1}";
		if (format.findFirstSubstr(indexStr1) >= 0)
		{
			builder.replaceAll(indexStr1, args1);
		}
		string indexStr2 = "{2}";
		if (format.findFirstSubstr(indexStr2) >= 0)
		{
			builder.replaceAll(indexStr2, args2);
		}
		return builder.ToString();
	}
	public static string format(string format, Span<int> args)
	{
		if (args.Length == 0)
		{
			return format;
		}

		using var a = new MyStringBuilderScope2(out var builder, out var helpBuilder);
		builder.append(format);
		int index = 0;
		while (index < args.Length)
		{
			helpBuilder.clear();
			helpBuilder.append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (format.findFirstSubstr(indexStr) >= 0)
			{
				builder.replaceAll(indexStr, IToS(args[index]));
			}
			++index;
		}
		return builder.ToString();
	}
	public static string format(string format, IList<string> args)
	{
		if (args.count() == 0)
		{
			return format;
		}

		using var a = new MyStringBuilderScope2(out var builder, out var helpBuilder);
		builder.append(format);
		int index = 0;
		while (index < args.Count)
		{
			helpBuilder.clear();
			helpBuilder.append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (format.findFirstSubstr(indexStr) >= 0)
			{
				builder.replaceAll(indexStr, args[index]);
			}
			++index;
		}
		return builder.ToString();
	}
	public static string format(string format, IList<int> args)
	{
		using var a = new MyStringBuilderScope2(out var builder, out var helpBuilder);
		builder.append(format);
		int index = 0;
		while (index < args.Count)
		{
			helpBuilder.clear();
			helpBuilder.append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (format.findFirstSubstr(indexStr) >= 0)
			{
				builder.replaceAll(indexStr, IToS(args[index]));
			}
			++index;
		}
		return builder.ToString();
	}
	public static string format(string format, IList<float> args)
	{
		using var a = new MyStringBuilderScope2(out var builder, out var helpBuilder);
		builder.append(format);
		int index = 0;
		while (index < args.Count)
		{
			helpBuilder.clear();
			helpBuilder.append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (format.findFirstSubstr(indexStr) >= 0)
			{
				builder.replaceAll(indexStr, FToS(args[index]));
			}
			++index;
		}
		return builder.ToString();
	}
	public static Color SToColor(string str)
	{
		if (str[0] != '#')
		{
			str = "#" + str;
		}
		ColorUtility.TryParseHtmlString(str, out Color color);
		return color;
	}
	public static int getFirstNumberPos(string str)
	{
		int strLen = str.Length;
		for (int i = 0; i < strLen; ++i)
		{
			if (str[i] <= '9' && str[i] >= '0')
			{
				return i;
			}
		}
		return -1;
	}
	public static int getLastNotNumberPos(string str)
	{
		int strLen = str.Length;
		for (int i = 0; i < strLen; ++i)
		{
			if (str[strLen - i - 1] > '9' || str[strLen - i - 1] < '0')
			{
				return strLen - i - 1;
			}
		}
		return -1;
	}
	public static string getNotNumberSubString(string str)
	{
		return str.startString(getLastNotNumberPos(str) + 1);
	}
	public static int getLastNumber(string str)
	{
		int lastPos = getLastNotNumberPos(str);
		if (lastPos == -1)
		{
			return -1;
		}
		string numStr = str.removeStartCount(lastPos + 1);
		return !numStr.isEmpty() ? SToI(numStr) : 0;
	}
	public static int SToI(string str)
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
	public static long SToL(string str)
	{
		checkIntString(str);
		return !str.isEmpty() ? long.Parse(str) : 0;
	}
	public static ulong SToUL(string str)
	{
		checkUIntString(str);
		return !str.isEmpty() ? ulong.Parse(str) : 0;
	}
	public static uint SToUInt(string str)
	{
		checkUIntString(str);
		return !str.isEmpty() ? uint.Parse(str) : 0;
	}
	public static Vector2 SToV2(string value, char seperate = ',')
	{
		if (value.isEmpty() || value == "0,0")
		{
			return Vector2.zero;
		}
		string[] splitList = split(value, seperate);
		if (splitList.count() < 2)
		{
			return Vector2.zero;
		}
		return new(SToF(splitList[0]), SToF(splitList[1]));
	}
	public static Vector2Int SToV2I(string value, char seperate = ',')
	{
		if (value.isEmpty() || value == "0,0")
		{
			return Vector2Int.zero;
		}
		mStringToVector2Cache ??= new(STRING_TO_VECTOR2INT_MAX_CACHE);
		if (mStringToVector2Cache.TryGetValue(value, out Vector2Int result))
		{
			return result;
		}
		string[] splitList = split(value, seperate);
		if (splitList.count() < 2)
		{
			return Vector2Int.zero;
		}
		result = new(SToI(splitList[0]), SToI(splitList[1]));
		if (mStringToVector2Cache.Count < STRING_TO_VECTOR2INT_MAX_CACHE)
		{
			mStringToVector2Cache.Add(value, result);
		}
		return result;
	}
	public static Vector3 SToV3(string value, char seperate = ',')
	{
		if (value.isEmpty() || value == "0,0,0")
		{
			return Vector3.zero;
		}
		string[] splitList = split(value, seperate);
		if (splitList.count() < 3)
		{
			return Vector3.zero;
		}
		return new(SToF(splitList[0]), SToF(splitList[1]), SToF(splitList[2]));
	}
	public static Vector3Int SToV3I(string value, char seperate = ',')
	{
		if (value.isEmpty() || value == "0,0,0")
		{
			return Vector3Int.zero;
		}
		mStringToVector3Cache ??= new(STRING_TO_VECTOR2INT_MAX_CACHE);
		if (mStringToVector3Cache.TryGetValue(value, out Vector3Int result))
		{
			return result;
		}
		string[] splitList = split(value, seperate);
		if (splitList.count() < 3)
		{
			return Vector3Int.zero;
		}
		result = new(SToI(splitList[0]), SToI(splitList[1]), SToI(splitList[2]));
		if (mStringToVector3Cache.Count < STRING_TO_VECTOR2INT_MAX_CACHE)
		{
			mStringToVector3Cache.Add(value, result);
		}
		return result;
	}
	public static Vector4 SToV4(string value, char seperate = ',')
	{
		if (value.isEmpty() || value == "0,0,0,0")
		{
			return Vector4.zero;
		}
		string[] splitList = split(value, seperate);
		if (splitList.count() < 4)
		{
			return Vector4.zero;
		}
		return new(SToF(splitList[0]), SToF(splitList[1]), SToF(splitList[2]), SToF(splitList[3]));
	}
	public static Vector4Int SToV4I(string value, char seperate = ',')
	{
		if (value.isEmpty() || value == "0,0,0,0")
		{
			return Vector4Int.zero;
		}
		string[] splitList = split(value, seperate);
		if (splitList.count() < 4)
		{
			return Vector4Int.zero;
		}
		return new(SToI(splitList[0]), SToI(splitList[1]), SToI(splitList[2]), SToI(splitList[3]));
	}
	// 移除整个stream中的最后一个出现过的字符key
	public static void removeLast(ref string stream, char key)
	{
		int lastCommaPos = stream.LastIndexOf(key);
		if (lastCommaPos != -1)
		{
			stream = stream.Remove(lastCommaPos, 1);
		}
	}
	// 去掉整个stream中最后一个出现过的逗号
	public static void removeLastComma(ref string stream)
	{
		removeLast(ref stream, ',');
	}
	// 解析一个数组类型的json字符串,并将每一个元素的字符串放入elementList中
	public static void decodeJsonArray(string json, List<string> elementList)
	{
		elementList.Clear();
		if (json.isEmpty())
		{
			return;
		}
		// 如果不是数组类型则无法解析
		if (json[0] != '[' || json[^1] != ']')
		{
			return;
		}
		int curIndex = 0;
		while (true)
		{
			int startIndex = json.IndexOf('{', curIndex);
			if (startIndex < 0)
			{
				break;
			}
			int endIndex = json.IndexOf('}', startIndex);
			if (endIndex < 0)
			{
				break;
			}
			elementList.Add(json.range(startIndex + 1, endIndex));
			curIndex = endIndex + 1;
		}
	}
	// 解析一个json的结构体,需要所有参数都是字符串类型的
	public static void decodeJsonStruct(string json, Dictionary<string, string> paramList)
	{
		paramList.Clear();
		if (json.isEmpty())
		{
			return;
		}
		json = json.removeStartString("{");
		json = json.removeEndString("}");
		json = json.removeAll('\r', '\n', '\t');
		using var a = new ListScope<string>(out var tempStrList);
		bool inString = false;
		int lastElementStart = 0;
		for (int i = 0; i < json.Length; ++i)
		{
			// 遍历到了最后一个字符,则直接放入列表
			if (i == json.Length - 1)
			{
				tempStrList.Add(json.range(lastElementStart, i));
				break;
			}
			// 遇到双引号就是切换是否为字符串的标记
			if (json[i] == '\"')
			{
				inString = !inString;
				continue;
			}
			// 在非字符串中遇到了逗号,表示元素的分隔
			if (!inString && json[i] == ',')
			{
				tempStrList.Add(json.range(lastElementStart, i));
				// 跳过这个逗号
				lastElementStart = i + 1;
			}
		}

		for (int i = 0; i < tempStrList.Count; ++i)
		{
			string[] param = split(tempStrList[i], ':');
			if (param.count() != 2)
			{
				continue;
			}
			paramList.Add(param[0].removeAll('\"'), param[1].removeStartEmpty().removeAll('\"'));
		}
	}
	// 绝对路径转换到相对于Asset的路径
	public static void fullPathToProjectPath(ref string path)
	{
		if (path.isEmpty())
		{
			return;
		}
		path = P_ASSETS_PATH + path.removeStartCount(F_ASSETS_PATH.Length);
	}
	public static string fullPathToProjectPath(string path)
	{
		if (path.isEmpty())
		{
			return path;
		}
		return P_ASSETS_PATH + path.removeStartCount(F_ASSETS_PATH.Length);
	}
	public static void projectPathToFullPath(ref string path)
	{
		if (path.isEmpty())
		{
			return;
		}
		if (path == ASSETS)
		{
			path = F_ASSETS_PATH;
			return;
		}
		path = F_ASSETS_PATH + path.removeStartCount(ASSETS.Length + 1);
	}
	public static string projectPathToFullPath(string path)
	{
		if (path.isEmpty())
		{
			return path;
		}
		if (path == ASSETS)
		{
			return F_ASSETS_PATH;
		}
		return F_ASSETS_PATH + path.removeStartCount(ASSETS.Length + 1);
	}
	public static string getFirstFolderName(string str)
	{
		str = str.rightToLeft();
		int firstPos = str.IndexOf('/');
		if (firstPos != -1)
		{
			return str.startString(firstPos);
		}
		return EMPTY;
	}
	// 从文件路径中得到最后一级的文件夹名
	public static string getFolderName(string str)
	{
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(str);
		builder.rightToLeft();

		// 如果有文件名,则先去除文件名
		int namePos = builder.lastIndexOf('/');
		int dotPos = builder.lastIndexOf('.');
		if (dotPos > namePos)
		{
			builder.remove(namePos);
		}

		// 如果是以/结尾的,则去除结尾的/
		if (builder[^1] == '/')
		{
			builder.remove(builder.Length - 1);
		}

		// 再去除当前目录的父级目录
		namePos = builder.lastIndexOf('/');
		if (namePos >= 0)
		{
			builder.remove(0, namePos + 1);
		}
		return builder.ToString();
	}
	// 得到文件路径
	public static string getFilePath(string fileName, bool keepEndSlash = false)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			builder.append(fileName);
			builder.rightToLeft();
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
			builder.rightToLeft();
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
		builder.rightToLeft();
		int dotPos = builder.lastIndexOf('/');
		if (dotPos != -1)
		{
			builder.remove(0, dotPos + 1);
		}
		return builder.ToString();
	}
	public static string getFileNameThread(string str)
	{
		string builder = str.rightToLeft();
		int dotPos = builder.LastIndexOf('/');
		if (dotPos != -1)
		{
			builder = builder[(dotPos + 1)..];
		}
		return builder;
	}
	// 获得文件的后缀名,带.号
	public static string getFileSuffix(string file)
	{
		int dotPos = file.IndexOf('.', clampMin(file.LastIndexOf('/')));
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
		builder.rightToLeft();
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
		builder.rightToLeft();
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
			path = path.startString(path.Length - 1);
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
			path = path.startString(path.Length - 1);
		}
		return path;
	}
	public static string replaceSuffix(string fileName, string suffix)
	{
		return removeSuffix(fileName) + suffix;
	}
	public static char[] generateOtherASCII(params char[] exclude)
	{
		int curCount = 0;
		char[] ascii = new char[0xFF - exclude.Length];
		for (int i = 1; i < 0xFF + 1; ++i)
		{
			if (arrayContains(exclude, (char)i))
			{
				continue;
			}
			if (curCount >= ascii.Length)
			{
				logError("获取ASCII字符数组失败,排除列表中可能存在重复的字符");
			}
			ascii[curCount++] = (char)i;
		}
		return ascii;
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
	public static string[] split(string str, params string[] keyword)
	{
		return split(str, true, keyword);
	}
	public static string[] split(string str, bool removeEmpty, params string[] keyword)
	{
		if (str.isEmpty())
		{
			return null;
		}
		return str.Split(keyword, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
	}
	// 在使用返回值期间禁止再调用strintToStrings
	public static List<string> strintToStrings(string str, bool removeEmpty, params string[] keyword)
	{
		mTempStringList.Clear();
		if (!str.isEmpty())
		{
			mTempStringList.AddRange(split(str, removeEmpty, keyword));
		}
		return mTempStringList;
	}
	// 在使用返回值期间禁止再调用strintToStrings
	public static List<string> strintToStrings(string str, bool removeEmpty, params char[] keyword)
	{
		mTempStringList.Clear();
		if (!str.isEmpty())
		{
			mTempStringList.AddRange(split(str, removeEmpty, keyword));
		}
		return mTempStringList;
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
	// 在使用返回值期间禁止再调用stringToFloatsNonAlloc
	public static List<float> SToFsNonAlloc(string str, char seperate = ',')
	{
		SToFs(str, mTempFloatList, seperate);
		return mTempFloatList;
	}
	public static List<float> SToFs(string str, char seperate = ',')
	{
		List<float> values = new();
		SToFs(str, values, seperate);
		return values;
	}
	public static void SToFs(string str, ref float[] values, char seperate = ',')
	{
		if (str.isEmpty())
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		int len = rangeList.Length;
		if (values != null && len != values.Length)
		{
			logError("count is not equal " + str.Length);
			return;
		}
		values ??= new float[len];
		for (int i = 0; i < len; ++i)
		{
			values[i] = SToF(rangeList[i]);
		}
	}
	public static void SToFs(string str, IList<float> values, char seperate = ',')
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
		foreach (string item in split(str, seperate))
		{
			values.Add(SToF(item));
		}
	}
	public static string FsToS(IList<float> values, char seperate = ',')
	{
		using var a = new MyStringBuilderScope(out var builder);
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.append(FToS(values[i], 2));
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return builder.ToString();
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
	public static void SToLs(string str, ref long[] values, char seperate = ',')
	{
		if (str.isEmpty())
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		int len = rangeList.Length;
		if (values != null && len != values.Length)
		{
			logError("value.Length is not equal " + len + ", str:" + str);
			return;
		}
		values ??= new long[len];
		for (int i = 0; i < len; ++i)
		{
			values[i] = SToL(rangeList[i]);
		}
	}
	// 在使用返回值期间禁止再调用stringToLongsNonAlloc
	public static List<long> SToLsNonAlloc(string str, char seperate = ',')
	{
		SToLs(str, mTempLongList, seperate);
		return mTempLongList;
	}
	public static List<int> SToIs(string str, char seperate = ',')
	{
		List<int> values = new();
		SToIs(str, values, seperate);
		return values;
	}
	public static void SToIs(string str, IList<int> values, char seperate = ',')
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
			values.Add(SToI(item));
		}
	}
	public static void SToIs(string str, ref int[] values, char seperate = ',')
	{
		if (str.isEmpty())
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		int len = rangeList.Length;
		if (values != null && len != values.Length)
		{
			logError("value.Length is not equal " + len + ", str:" + str);
			return;
		}
		values ??= new int[len];
		for (int i = 0; i < len; ++i)
		{
			values[i] = SToI(rangeList[i]);
		}
	}
	// 在使用返回值期间禁止再调用stringToIntsNonAlloc
	public static List<int> SToIsNonAlloc(string str, char seperate = ',')
	{
		SToIs(str, mTempIntList, seperate);
		return mTempIntList;
	}
	public static void SToUIs(string str, IList<uint> values, char seperate = ',')
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
			values.Add((uint)SToI(item));
		}
	}
	public static void SToUSs(string str, IList<ushort> values, char seperate = ',')
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
			values.Add((ushort)SToI(item));
		}
	}
	public static void SToSs(string str, IList<short> values, char seperate = ',')
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
			values.Add((short)SToI(item));
		}
	}
	public static void SToBools(string str, IList<bool> values, char seperate = ',')
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
			values.Add(SToI(item) > 0);
		}
	}
	// 在使用返回值期间不允许再使用此函数
	public static List<byte> SToBsNonAlloc(string str, char seperate = ',')
	{
		mTempByteList0.Clear();
		if (str.isEmpty())
		{
			return mTempByteList0;
		}
		foreach (string item in split(str, seperate).safe())
		{
			mTempByteList0.Add((byte)SToI(item));
		}
		return mTempByteList0;
	}
	public static void SToBs(string str, IList<byte> values, char seperate = ',')
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
			values.Add((byte)SToI(item));
		}
	}
	public static void SToSBs(string str, IList<sbyte> values, char seperate = ',')
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
			values.Add((sbyte)SToI(item));
		}
	}
	public static string LsToS(IList<long> values, char seperate = ',')
	{
		if (values.isEmpty())
		{
			return EMPTY;
		}
		using var a = new MyStringBuilderScope(out var builder);
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.append(LToS(values[i]));
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return builder.ToString();
	}
	public static string IsToS(IList<int> values, char seperate = ',')
	{
		if (values.isEmpty())
		{
			return EMPTY;
		}
		using var a = new MyStringBuilderScope(out var builder);
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.append(IToS(values[i]));
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return builder.ToString();
	}
	public static string stringsToString(IList<string> values, string seperate)
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
	public static List<string> stringToStrings(string str, char seperate = ',')
	{
		List<string> strList = new();
		if (!str.isEmpty())
		{
			strList.addRange(split(str, seperate).safe());
		}
		return strList;
	}
	public static void stringToStrings(string str, IList<string> values, char seperate = ',')
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
			values.Add(item);
		}
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
	// 给数字字符串以千为单位添加逗号
	public static void insertNumberComma(ref string str)
	{
		int length = str.Length;
		int commaCount = length / 3;
		if (length > 0 && length % 3 == 0)
		{
			commaCount -= 1;
		}
		int insertStart = length % 3;
		if (insertStart == 0)
		{
			insertStart = 3;
		}
		insertStart += 3 * (commaCount - 1);
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(str);
		// 从后往前插入
		for (int i = 0; i < commaCount; ++i)
		{
			builder.insert(insertStart, ',');
			insertStart -= 3;
		}
		str = builder.ToString();
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
	// 函数名中的int仅表示整数的意思,并非特指int类型
	public static string intToChineseString(long value)
	{
		using var a = new MyStringBuilderScope(out var builder);
		// 大于1亿
		if (value >= 100000000)
		{
			builder.append(LToS(value / 100000000), "亿");
			value %= 100000000;
		}
		// 大于1万
		if (value >= 10000)
		{
			builder.append(LToS(value / 10000), "万");
			value %= 10000;
		}
		if (value > 0)
		{
			builder.append(value);
		}
		return builder.ToString();
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
	public static string IToSComma(int value)
	{
		string retString = IToS(value);
		insertNumberComma(ref retString);
		return retString;
	}
	public static string LToSComma(long value)
	{
		string retString = LToS(value);
		insertNumberComma(ref retString);
		return retString;
	}
	public static string ULToSComma(ulong value)
	{
		string retString = ULToS(value);
		insertNumberComma(ref retString);
		return retString;
	}
	public static string IToSComma(uint value)
	{
		return IToSComma((int)value);
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
	public static string V2IToS(Vector2Int value, int limitLength = 0)
	{
		return IToS(value.x, limitLength) + "," + IToS(value.y, limitLength);
	}
	public static string V2ToS(Vector2 value, int precision = 4)
	{
		return FToS(value.x, precision) + "," + FToS(value.y, precision);
	}
	public static string V3ToS(Vector3 value, int precision = 4)
	{
		return strcat(FToS(value.x, precision), ",", FToS(value.y, precision), ",", FToS(value.z, precision));
	}
	public static float SToF(string str)
	{
		checkFloatString(str);
		if (str.isEmpty() || str == "-")
		{
			return 0.0f;
		}
		return float.Parse(str);
	}
	public static int getBytesLength(string str)
	{
		byte[] bytes = BinaryUtility.stringToBytes(str);
		for (int i = 0; i < bytes.Length; ++i)
		{
			if (bytes[i] == 0)
			{
				return i;
			}
		}
		return bytes.Length;
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
	public static bool checkFloatString(string str, string valid = EMPTY)
	{
		if (isEditor())
		{
			return checkString(str, "-0123456789.E" + valid);
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
	public static bool checkUIntString(string str, string valid = EMPTY)
	{
		if (isEditor())
		{
			return checkString(str, "0123456789" + valid);
		}
		return true;
	}
	public static string checkNickName(string name, bool removeOrTransfer)
	{
		if (removeOrTransfer)
		{
			name = name.removeAll('&');
			name = name.removeAll('\\');
		}
		else
		{
			name = name.replaceAll("&", "%26");
			name = name.replaceAll("\\", "%5C%5C");
		}
		return name;
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
					builder.byteToHEXString(byteList[i + offset], upperOrLower);
					if (i != byteCount - 1)
					{
						builder.append(' ');
					}
				}
				else
				{
					builder.byteToHEXString(byteList[i + offset], upperOrLower);
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
	public static string byteToHEXString(byte value, bool upperOrLower = true)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
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
			return builder.ToString();
		}
		else
		{
			StringBuilder builder = new();
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
	public static byte hexStringToByte(string str, int start = 0)
	{
		byte highBit = 0;
		byte lowBit = 0;
		byte[] strBytes = BinaryUtility.stringToBytes(str);
		byte highBitChar = strBytes[start];
		byte lowBitChar = strBytes[start + 1];
		if (highBitChar >= 'A' && highBitChar <= 'F')
		{
			highBit = (byte)(10 + highBitChar - 'A');
		}
		else if (highBitChar >= 'a' && highBitChar <= 'f')
		{
			highBit = (byte)(10 + highBitChar - 'a');
		}
		else if (highBitChar >= '0' && highBitChar <= '9')
		{
			highBit = (byte)(highBitChar - '0');
		}
		if (lowBitChar >= 'A' && lowBitChar <= 'F')
		{
			lowBit = (byte)(10 + lowBitChar - 'A');
		}
		else if (lowBitChar >= 'a' && lowBitChar <= 'f')
		{
			lowBit = (byte)(10 + lowBitChar - 'a');
		}
		else if (lowBitChar >= '0' && lowBitChar <= '9')
		{
			lowBit = (byte)(lowBitChar - '0');
		}
		return (byte)(highBit << 4 | lowBit);
	}
	// 返回值表示bytes的有效长度,需要调用releaseHexStringBytes释放数组
	public static int hexStringToBytes(string str, out byte[] bytes)
	{
		bytes = null;
		checkString(str, mHexString);
		if (str.isEmpty() || (str.Length & 1) != 0)
		{
			return 0;
		}
		int dataCount = str.Length >> 1;
		ARRAY_BYTE_THREAD(out bytes, getGreaterPow2(dataCount));
		for (int i = 0; i < dataCount; ++i)
		{
			bytes[i] = hexStringToByte(str, i << 1);
		}
		return dataCount;
	}
	public static void releaseHexStringBytes(byte[] bytes)
	{
		UN_ARRAY_BYTE_THREAD(ref bytes);
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
	public static string addSprite(string originString, string spriteName, float width = 1.0f)
	{
		return strcat(originString, "<quad width=", FToS(width), " sprite=", spriteName, "/>");
	}
	public static void addSprite(ref string originString, string spriteName, float width = 1.0f)
	{
		originString = addSprite(originString, spriteName, width);
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
	// 在文本显示中将str的颜色设置为color
	public static string colorString(string color, string str)
	{
		if (color.isEmpty())
		{
			return str;
		}
		if (str.isEmpty())
		{
			return EMPTY;
		}
		return strcat("<color=#", color, ">", str, "</color>");
	}
	public static string colorString(string color, string str0, string str1)
	{
		return strcat("<color=#", color, ">", str0, str1, "</color>");
	}
	public static string colorString(string color, string str0, string str1, string str2)
	{
		return strcat("<color=#", color, ">", str0, str1, str2, "</color>");
	}
	public static string colorString(string color, string str0, string str1, string str2, string str3)
	{
		return strcat("<color=#", color, ">", str0, str1, str2, str3, "</color>");
	}
	public static string colorString(string color, string str0, string str1, string str2, string str3, string str4)
	{
		return strcat("<color=#", color, ">", str0, str1, str2, str3, str4, "</color>");
	}
	public static string colorToRGBString(Color32 color)
	{
		return byteToHEXString(color.r) + byteToHEXString(color.g) + byteToHEXString(color.b);
	}
	public static string colorToRGBAString(Color32 color)
	{
		return byteToHEXString(color.r) + byteToHEXString(color.g) + byteToHEXString(color.b) + byteToHEXString(color.a);
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
	// 将字符转换为大写字母
	public static char toUpper(char c)
	{
		if (isLower(c))
		{
			return (char)(c + ('A' - 'a'));
		}
		return c;
	}
	// 是否为字母
	public static bool isLetter(char c) { return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z'; }
	// 字符是否为小写字母
	public static bool isLower(char c) { return c >= 'a' && c <= 'z'; }
	// 字符是否为大写字母
	public static bool isUpper(char c) { return c >= 'A' && c <= 'Z'; }
	// 是否为全大写字母
	public static bool isUpperString(string str)
	{
		foreach (char element in str)
		{
			if (isLower(element))
			{
				return false;
			}
		}
		return true;
	}
	// 是否有除数字字母以外的字符
	public static bool hasSpecialChar(string str)
	{
		return Regex.IsMatch(str, "[^0-9a-zA-Z\u4e00-\u9fa5]");
	}
	// 是否是整数或浮点数的字符串
	public static bool isNumeric(string value)
	{
		return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
	}
	// 是否为数字
	public static bool isNumeric(char c) { return c >= '0' && c <= '9'; }
	public static bool isChinese(char c) { return c >= 0x4E00 && c <= 0x9FBB; }
	public static bool isASCII(char c) { return c >= 0 && c <= 0x7F; }
	public static bool hasChinese(string str) 
	{
		int length = str.Length;
		for (int i = 0; i < length; ++i)
		{
			if (isChinese(str[i]))
			{
				return true;
			}
		}
		return false;
	}
	// 是否有除了中文和ASCII以外的字符
	public static bool hasNonChineseASCII(string str)
	{
		int length = str.Length;
		for (int i = 0; i < length; ++i)
		{
			if (!isChinese(str[i]) && !isASCII(str[i]))
			{
				return true;
			}
		}
		return false;
	}
	// 计算字符的显示宽度,英文字母的宽度为1,汉字的宽度为2
	public static int generateCharWidth(string str)
	{
		int width = 0;
		for (int i = 0; i < str.Length; ++i)
		{
			width += str[i] <= 0x7F ? 1 : 2;
		}
		return width;
	}
	// 判断一个字符串是否为有效的手机号
	public static bool isPhoneNumber(string str)
	{
		int length = str.Length;
		// 手机号固定11位,以1开头
		if (length != 11 || str[0] != '1')
		{
			return false;
		}
		for (int i = 0; i < length; ++i)
		{
			if (!isNumeric(str[i]))
			{
				return false;
			}
		}
		return true;
	}
	// 允许中文标点通过检测
	public static bool hasNonChineseSymbolASCII(string str)
	{
		mChineseSymbol ??= new() { '，', '。', '；', '‘', '“', '”', '？', '、', '【', '】', '·', '！', '￥', '…', '（', '）' };
		int length = str.Length;
		for (int i = 0; i < length; i++)
		{
			if (!isChinese(str[i]) && !isASCII(str[i]) && !mChineseSymbol.Contains(str[i]))
			{
				return true;
			}
		}
		return false;
	}
	public static void line(ref string str, string line, bool returnLine = true)
	{
		if (returnLine)
		{
			str += line + "\r\n";
		}
		else
		{
			str += line;
		}
	}
	public static string nameToUpper(string sqliteName, bool preUnderLine)
	{
		// 根据大写字母拆分
		using var a = new ListScope<string>(out var macroList);
		int length = sqliteName.Length;
		int lastIndex = 0;
		// 从1开始,因为第0个始终都是大写,会截取出空字符串,最后一个字母也肯不会被分割
		for (int i = 1; i < length; ++i)
		{
			// 以大写字母为分隔符,但是连续的大写字符不能被分隔
			// 非连续数字也会分隔
			char curChar = sqliteName[i];
			char lastChar = sqliteName[i - 1];
			char nextChar = i + 1 < length ? sqliteName[i + 1] : '\0';
			if (isUpper(curChar) && (!isUpper(lastChar) || (nextChar != '\0' && !isUpper(nextChar))) ||
				isNumeric(curChar) && (!isNumeric(lastChar) || (nextChar != '\0' && !isNumeric(nextChar))))
			{
				macroList.Add(sqliteName.range(lastIndex, i));
				lastIndex = i;
			}
		}
		macroList.Add(sqliteName.range(lastIndex, length));

		using var b = new MyStringBuilderScope(out var headerMacro);
		foreach (string item in macroList)
		{
			headerMacro.append("_", item.ToUpper());
		}
		if (!preUnderLine)
		{
			headerMacro.remove(0, 1);
		}
		return headerMacro.ToString();
	}
	public static string validateHttpString(string str)
	{
		if (mInvalidParamChars == null)
		{
			initInvalidChars();
		}
		using var a = new MyStringBuilderScope(out var builder);
		builder.append(str);
		foreach (var item in mInvalidParamChars)
		{
			builder.replaceAll(item.Key, item.Value);
		}
		return builder.ToString();
	}
	public static void appendValueString(ref string queryStr, string str)
	{
		queryStr += "\"" + str + "\",";
	}
	public static void appendValueVector2(ref string queryStr, Vector2 value)
	{
		queryStr += V2ToS(value) + ",";
	}
	public static void appendValueVector2Int(ref string queryStr, Vector2Int value)
	{
		queryStr += V2IToS(value) + ",";
	}
	public static void appendValueVector3(ref string queryStr, Vector3 value)
	{
		queryStr += V3ToS(value) + ",";
	}
	public static void appendValueInt(ref string queryStr, int value)
	{
		queryStr += IToS(value) + ",";
	}
	public static void appendValueUInt(ref string queryStr, uint value)
	{
		queryStr += IToS(value) + ",";
	}
	public static void appendValueFloat(ref string queryStr, float value)
	{
		queryStr += FToS(value) + ",";
	}
	public static void appendValueFloats(ref string queryStr, List<float> floatArray)
	{
		appendValueString(ref queryStr, FsToS(floatArray));
	}
	public static void appendValueInts(ref string queryStr, List<int> intArray)
	{
		appendValueString(ref queryStr, IsToS(intArray));
	}
	public static void appendConditionString(ref string condition, string col, string str, string operate)
	{
		condition += col + " = " + "\"" + str + "\"" + operate;
	}
	public static void appendConditionInt(ref string condition, string col, int value, string operate)
	{
		condition += col + " = " + IToS(value) + operate;
	}
	public static void appendUpdateString(ref string updateStr, string col, string str)
	{
		updateStr += col + " = " + "\"" + str + "\",";
	}
	public static void appendUpdateInt(ref string updateStr, string col, int value)
	{
		updateStr += col + " = " + IToS(value) + ",";
	}
	public static void appendUpdateInts(ref string updateStr, string col, List<int> intArray)
	{
		appendUpdateString(ref updateStr, col, IsToS(intArray));
	}
	public static void appendUpdateFloats(ref string updateStr, string col, List<float> floatArray)
	{
		appendUpdateString(ref updateStr, col, FsToS(floatArray));
	}
	public static int KMPSearch(string str, string pattern)
	{
		int[] nextIndex = null;
		return KMPSearch(str, pattern, ref nextIndex);
	}
	public static int KMPSearch(string str, string pattern, ref int[] nextIndex)
	{
		int sLen = str.Length;
		int pLen = pattern.Length;
		if (pLen == 0 || sLen < pLen)
		{
			return -1;
		}
		if (nextIndex == null)
		{
			generateNextIndex(pattern, out nextIndex);
		}
		int i = 0;
		int j = 0;
		while (i < sLen && j < pLen)
		{
			// 如果j = -1，或者当前字符匹配成功（即S[i] == P[j]），都令i++，j++      
			if (j == -1 || str[i] == pattern[j])
			{
				++i;
				++j;
			}
			else
			{
				// 如果j != -1，且当前字符匹配失败（即S[i] != P[j]），则令 i 不变，j = next[j]      
				// next[j]即为j所对应的next值        
				j = nextIndex[j];
			}
		}
		if (j == pLen)
		{
			return i - j;
		}
		return -1;
	}
	public static void generateNextIndex(string pattern, out int[] next)
	{
		int pLen = pattern.Length;
		if (pLen == 0)
		{
			next = null;
			return;
		}
		next = new int[pLen];
		next[0] = -1;
		int k = -1;
		int j = 0;
		while (j < pLen - 1)
		{
			// p[k]表示前缀，p[j]表示后缀    
			if (k == -1 || pattern[j] == pattern[k])
			{
				++j;
				++k;
				// 较之前next数组求法，改动在下面4行  
				if (pattern[j] != pattern[k])
				{
					// 之前只有这一行 
					next[j] = k;
				}
				else
				{
					// 因为不能出现p[j] = p[ next[j ]]，所以当出现时需要继续递归，k = next[k] = next[next[k]]  
					next[j] = next[k];
				}
			}
			else
			{
				k = next[k];
			}
		}
	}
	public static void recoverStringColor(List<string> lineList, List<List<string>> colorLineList)
	{
		if (lineList.Count != colorLineList.Count)
		{
			return;
		}
		using var a = new ListScope<KeyValuePair<string, string>>(out var tempList);
		for (int i = 0; i < colorLineList.Count; ++i)
		{
			// 处理一行文字的颜色,找到每段相同颜色的连续字符,将其连同颜色放入一个列表中
			int colorStart = -1;
			string curColor = null;
			List<string> colorLine = colorLineList[i];
			string strLine = lineList[i];
			int lineLength = colorLine.Count;
			tempList.Clear();
			for (int j = 0; j < lineLength; ++j)
			{
				if (colorStart == -1)
				{
					colorStart = j;
					curColor = colorLine[j];
				}
				// 遇到不一样的颜色或者已经遍历到了最后一个,则截取这段字符
				else if (colorLine[j] != curColor || j == lineLength - 1)
				{
					if (j == lineLength - 1)
					{
						tempList.Add(new(strLine.removeStartCount(colorStart), curColor));
					}
					else
					{
						tempList.Add(new(strLine.range(colorStart, j), curColor));
						colorStart = j;
						curColor = colorLine[j];
					}
				}
			}
			// 再将各个分段的字符串使用颜色拼接起来
			using var b = new MyStringBuilderScope(out var builder);
			foreach (var item in tempList)
			{
				if (item.Value == null)
				{
					builder.append(item.Key);
				}
				else
				{
					builder.colorString(item.Value, item.Key);
				}
			}
			lineList[i] = builder.ToString();
		}
	}
	// 将文本拆分为多行来显示,originString应该是不带富文本标签的字符串,否则会影响字符长度的计算
	// 默认每一行至少可以容纳30个字符,所以都是从30开始截取字符串,为了提高效率
	public static void generateMultiLine(myUGUIText textWindow, string originString, List<string> lineList, int minStringLength = 30)
	{
		if (originString.Length < minStringLength)
		{
			lineList.Add(originString);
			return;
		}
		int maxContentDisplayWidth = (int)textWindow.getWindowSize().x;
		int charIndex = minStringLength;
		int startIndex = 0;
		while (true)
		{
			if (startIndex + charIndex >= originString.Length)
			{
				lineList.Add(originString.removeStartCount(startIndex));
				break;
			}
			if (getContentLength(textWindow.getTextComponent(), originString.substr(startIndex, charIndex)) >= maxContentDisplayWidth)
			{
				// 超出以后需要减少一个字符,避免超出显示范围
				--charIndex;
				lineList.Add(originString.substr(startIndex, charIndex));
				startIndex += charIndex;
				charIndex = minStringLength;
			}
			else
			{
				++charIndex;
			}
		}
	}
	// 将文本拆分为多行来显示,originString应该是不带富文本标签的字符串,否则会影响字符长度的计算
	// 默认每一行至少可以容纳30个字符,所以都是从30开始截取字符串,为了提高效率
	public static void generateMultiLine(myUGUITextTMP textWindow, string originString, List<string> lineList, int minStringLength = 30)
	{
		if (originString.Length < minStringLength)
		{
			lineList.Add(originString);
			return;
		}
		int maxContentDisplayWidth = (int)textWindow.getWindowSize().x;
		int charIndex = minStringLength;
		int startIndex = 0;
		while (true)
		{
			if (startIndex + charIndex >= originString.Length)
			{
				lineList.Add(originString.removeStartCount(startIndex));
				break;
			}
			if (getContentLength(textWindow.getTextComponent(), originString.substr(startIndex, charIndex)) >= maxContentDisplayWidth)
			{
				// 超出以后需要减少一个字符,避免超出显示范围
				--charIndex;
				lineList.Add(originString.substr(startIndex, charIndex));
				startIndex += charIndex;
				charIndex = minStringLength;
			}
			else
			{
				++charIndex;
			}
		}
	}
	// 将富文本还原为原始的字符串,暂时只考虑颜色,charColorList的输出长度与返回字符串的长度一致,其中每个元素表示相同下标的字符的颜色
	public static string getStringNoRichText(string originContent, List<string> charColorList)
	{
		charColorList.addCount(originContent.Length);
		// 暂时只考虑富文本的颜色
		string colorPrefix = "<color=#";
		string colorSuffix = "</color>";
		// 从后往前移除富文本标签
		while (true)
		{
			int colorStart = originContent.IndexOf(colorPrefix);
			if (colorStart < 0)
			{
				break;
			}
			int prefixEndIndex = originContent.IndexOf('>', colorStart);
			if (prefixEndIndex < 0)
			{
				break;
			}
			int suffixIndex = originContent.IndexOf(colorSuffix, prefixEndIndex);
			if (suffixIndex < 0)
			{
				break;
			}
			string color = originContent.range(colorStart + colorPrefix.Length, prefixEndIndex);
			int colorCharCount = suffixIndex - prefixEndIndex - 1;
			for (int i = 0; i < colorCharCount; ++i)
			{
				charColorList[colorStart + i] = color;
			}
			originContent = originContent.Remove(suffixIndex, colorSuffix.Length);
			originContent = originContent.Remove(colorStart, prefixEndIndex - colorStart + 1);
		}
		charColorList.RemoveRange(originContent.Length, charColorList.Count - originContent.Length);
		return originContent;
	}
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
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9, string str10)
	{
		if (isMainThread())
		{
			using var a = new MyStringBuilderScope(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9, str10).ToString();
		}
		else
		{
			using var a = new ClassThreadScope<MyStringBuilder>(out var builder);
			return builder.append(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9, str10).ToString();
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
	protected static void initInvalidChars()
	{
		mInvalidParamChars = new()
		{
			{ "(", "%28" },
			{ ")", "%29" },
			{ "<", "%3C" },
			{ ">", "%3E" },
			{ "@", "%40" },
			{ ",", "%2C" },
			{ ";", "%3B" },
			{ ":", "%3A" },
			{ "\\", "%5C" },
			{ "\"", "%22" },
			{ "\'", "%27" },
			{ "/", "%2F" },
			{ "[", "%5B" },
			{ "]", "%5D" },
			{ "?", "%3F" },
			{ "=", "%3D" },
			{ "{", "%7B" },
			{ "}", "%7D" },
			{ " ", "%20" },
			{ "\t", "%09" },
			{ "\r", "%0D" },
			{ "\n", "%0A" }
		};
	}
}