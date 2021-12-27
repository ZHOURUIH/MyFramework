using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// 字符串相关工具函数类
public class StringUtility : BinaryUtility
{
	private static char[] mHexUpperChar = new char[] { 'A', 'B', 'C', 'D', 'E', 'F' };	// 十六进制中的大写字母
	private static char[] mHexLowerChar = new char[] { 'a', 'b', 'c', 'd', 'e', 'f' };	// 十六进制中的小写字母
	private static string mHexString = "ABCDEFabcdef0123456789";						// 十六进制中的所有字符
	private static List<int> mTempIntList = new List<int>();							// 避免GC
	private static List<float> mTempFloatList = new List<float>();						// 避免GC
	private static List<string> mTempStringList = new List<string>();					// 避免GC
	private static Dictionary<string, int> mStringToInt;								// 用于快速查找字符串转换后的整数
	private static string[] mIntToString;												// 用于快速获取整数转换后的字符串
	private static string[] mFloatConvertPercision = new string[] { "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7" };	// 浮点数转换时精度
	private static Dictionary<string, Vector2Int> mStringToVector2Cache;				// 字符串转换为2维向量的缓存
	private static int STRING_TO_VECTOR2INT_MAX_CACHE = 10240;                          // mStringToVector2Cache最大数量
	static Dictionary<string, string> mInvalidParamChars;                               // invalid characters that cannot be found in a valid method-verb or http header
	public const string EMPTY = "";														// 表示空字符串
	// 只能使用{index}拼接
	public static string format(string format, params string[] args)
	{
		// 由于连续拼接字符串时,会将其转换为数组进行传递,所以为了避免数组的创建,使用一个辅助拼接器
		MyStringBuilder builder = FrameUtility.STRING(format);
		MyStringBuilder helpBuilder = FrameUtility.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.clear();
			helpBuilder.append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (findFirstSubstr(builder, indexStr) < 0)
			{
				break;
			}
			if (index >= args.Length)
			{
				UnityUtility.logError("参数数量不足");
			}
			replaceAll(builder, indexStr, args[index]);
			++index;
		}
		FrameUtility.DESTROY_STRING(helpBuilder);
		return FrameUtility.END_STRING(builder);
	}
	public static string format(string format, List<string> args)
	{
		MyStringBuilder builder = FrameUtility.STRING(format);
		MyStringBuilder helpBuilder = FrameUtility.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.clear();
			helpBuilder.append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (findFirstSubstr(builder, indexStr) < 0)
			{
				break;
			}
			if (index >= args.Count)
			{
				UnityUtility.logError("参数数量不足");
			}
			replaceAll(builder, indexStr, args[index]);
			++index;
		}
		FrameUtility.DESTROY_STRING(helpBuilder);
		return FrameUtility.END_STRING(builder);
	}
	public static string format(string format, List<int> args)
	{
		MyStringBuilder builder = FrameUtility.STRING(format);
		MyStringBuilder helpBuilder = FrameUtility.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.clear();
			helpBuilder.append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (findFirstSubstr(builder, indexStr) < 0)
			{
				break;
			}
			if (index >= args.Count)
			{
				UnityUtility.logError("参数数量不足");
			}
			replaceAll(builder, indexStr, IToS(args[index]));
			++index;
		}
		FrameUtility.DESTROY_STRING(helpBuilder);
		return FrameUtility.END_STRING(builder);
	}
	public static string format(string format, params int[] args)
	{
		MyStringBuilder builder = FrameUtility.STRING(format);
		MyStringBuilder helpBuilder = FrameUtility.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.clear();
			helpBuilder.append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (findFirstSubstr(builder, indexStr) < 0)
			{
				break;
			}
			if (index >= args.Length)
			{
				UnityUtility.logError("参数数量不足");
			}
			replaceAll(builder, indexStr, IToS(args[index]));
			++index;
		}
		FrameUtility.DESTROY_STRING(helpBuilder);
		return FrameUtility.END_STRING(builder);
	}
	public static string format(string format, List<float> args)
	{
		MyStringBuilder builder = FrameUtility.STRING(format);
		MyStringBuilder helpBuilder = FrameUtility.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.clear();
			helpBuilder.append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (findFirstSubstr(builder, indexStr) < 0)
			{
				break;
			}
			if (index >= args.Count)
			{
				UnityUtility.logError("参数数量不足");
			}
			replaceAll(builder, indexStr, FToS(args[index]));
			++index;
		}
		FrameUtility.DESTROY_STRING(helpBuilder);
		return FrameUtility.END_STRING(builder);
	}
	public static string format(string format, params float[] args)
	{
		MyStringBuilder builder = FrameUtility.STRING(format);
		MyStringBuilder helpBuilder = FrameUtility.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.clear();
			helpBuilder.append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (findFirstSubstr(builder, indexStr) < 0)
			{
				break;
			}
			if (index >= args.Length)
			{
				UnityUtility.logError("参数数量不足");
			}
			replaceAll(builder, indexStr, FToS(args[index]));
			++index;
		}
		FrameUtility.DESTROY_STRING(helpBuilder);
		return FrameUtility.END_STRING(builder);
	}
	// 使用Length == 0判断是否为空字符串是最快的
	public static bool isEmpty(string str)
	{
		return str == null || str.Length == 0;
	}
	public static bool startWith(string oriString, string pattern, bool sensitive = true)
	{
		if (oriString.Length < pattern.Length)
		{
			return false;
		}
		string startString = oriString.Substring(0, pattern.Length);
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
		string endString = oriString.Substring(oriString.Length - pattern.Length, pattern.Length);
		if (sensitive)
		{
			return endString == pattern;
		}
		else
		{
			return endString.ToLower() == pattern.ToLower();
		}
	}
	// 移除所有的空白字符
	public static string removeAllEmpty(string str)
	{
		return removeAll(str, ' ', '\t');
	}
	// 移除开头所有的空白字符
	public static string removeStartEmpty(string str)
	{
		return removeStartAll(str, ' ', '\t');
	}
	// 移除结尾所有的空白字符
	public static string removeEndEmpty(string str)
	{
		return removeEndAll(str, ' ', '\t');
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
		return str.Substring(0, getLastNotNumberPos(str) + 1);
	}
	public static int getLastNumber(string str)
	{
		int lastPos = getLastNotNumberPos(str);
		if (lastPos == -1)
		{
			return -1;
		}
		string numStr = str.Substring(lastPos + 1);
		if (isEmpty(numStr))
		{
			return 0;
		}
		return SToI(numStr);
	}
	public static int SToI(string str)
	{
		checkIntString(str);
		if (isEmpty(str) || str == "-")
		{
			return 0;
		}
		if (mStringToInt == null)
		{
			initIntToString();
		}
		if (mStringToInt.TryGetValue(str, out int value))
		{
			return value;
		}
		return int.Parse(str);
	}
	public static long SToL(string str)
	{
		checkIntString(str);
		if (isEmpty(str))
		{
			return 0;
		}
		return long.Parse(str);
	}
	public static ulong SToUL(string str)
	{
		checkUIntString(str);
		if (isEmpty(str))
		{
			return 0;
		}
		return ulong.Parse(str);
	}
	public static uint SToUInt(string str)
	{
		checkUIntString(str);
		if (isEmpty(str))
		{
			return 0;
		}
		return uint.Parse(str);
	}
	public static Vector2 SToVector2(string value, char seperate = ',')
	{
		if (isEmpty(value) || value == "0,0")
		{
			return Vector2.zero;
		}
		string[] splitList = split(value, seperate);
		if (splitList == null || splitList.Length < 2)
		{
			return Vector2.zero;
		}
		Vector2 v = new Vector2();
		v.x = SToF(splitList[0]);
		v.y = SToF(splitList[1]);
		return v;
	}
	public static Vector2Int stringToVector2Int(string value, char seperate = ',')
	{
		if (isEmpty(value) || value == "0,0")
		{
			return Vector2Int.zero;
		}
		if (mStringToVector2Cache == null)
		{
			mStringToVector2Cache = new Dictionary<string, Vector2Int>(STRING_TO_VECTOR2INT_MAX_CACHE);
		}
		if (mStringToVector2Cache.TryGetValue(value, out Vector2Int result))
		{
			return result;
		}
		string[] splitList = split(value, seperate);
		if (splitList == null || splitList.Length < 2)
		{
			return Vector2Int.zero;
		}
		result = new Vector2Int();
		result.x = SToI(splitList[0]);
		result.y = SToI(splitList[1]);
		if (mStringToVector2Cache.Count < STRING_TO_VECTOR2INT_MAX_CACHE)
		{
			mStringToVector2Cache.Add(value, result);
		}
		return result;
	}
	public static Vector3 SToVector3(string value, char seperate = ',')
	{
		if (isEmpty(value) || value == "0,0,0")
		{
			return Vector3.zero;
		}
		string[] splitList = split(value, seperate);
		if (splitList == null || splitList.Length < 3)
		{
			return Vector3.zero;
		}
		Vector3 v = new Vector3();
		v.x = SToF(splitList[0]);
		v.y = SToF(splitList[1]);
		v.z = SToF(splitList[2]);
		return v;
	}
	public static Vector4 SToVector4(string value, char seperate = ',')
	{
		if (isEmpty(value) || value == "0,0,0,0")
		{
			return Vector4.zero;
		}
		string[] splitList = split(value, seperate);
		if (splitList == null || splitList.Length < 4)
		{
			return Vector4.zero;
		}
		Vector4 v = new Vector4();
		v.x = SToF(splitList[0]);
		v.y = SToF(splitList[1]);
		v.z = SToF(splitList[2]);
		v.w = SToF(splitList[3]);
		return v;
	}
	// 如果originStr以endString为结尾,则移除originStr结尾的endString
	public static void removeEndString(ref string originStr, string endString, bool sensitive = true)
	{
		originStr = removeEndString(originStr, endString, sensitive);
	}
	public static string removeEndString(string originStr, string endString, bool sensitive = true)
	{
		if (isEmpty(endString) ||
			isEmpty(originStr) ||
			!endWith(originStr, endString, sensitive))
		{
			return originStr;
		}
		return originStr.Substring(0, originStr.Length - endString.Length);
	}
	// 如果originStr以startString为开头,则移除originStr结尾的startString
	public static void removeStartString(ref string originStr, string startString, bool sensitive = true)
	{
		originStr = removeStartString(originStr, startString, sensitive);
	}
	public static string removeStartString(string originStr, string startString, bool sensitive = true)
	{
		if (isEmpty(startString) ||
			isEmpty(originStr) ||
			!startWith(originStr, startString, sensitive))
		{
			return originStr;
		}
		return originStr.Substring(startString.Length);
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
	public static void removeLast(MyStringBuilder stream, char key)
	{
		int length = stream.Length;
		for (int i = 0; i < length; ++i)
		{
			if (stream[length - 1 - i] == key)
			{
				stream.remove(length - 1 - i, 1);
				break;
			}
		}
	}
	public static string removeStartAll(string str, char key)
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
		if (removeStartCount > 0)
		{
			str = str.Substring(removeStartCount);
		}
		return str;
	}
	public static string removeStartAll(string str, char key0, char key1)
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
		if (removeStartCount > 0)
		{
			str = str.Substring(removeStartCount);
		}
		return str;
	}
	public static string removeLastAll(string str, char key)
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
		if (removeStartCount > 0)
		{
			str = str.Substring(removeStartCount);
		}
		return str;
	}
	public static string removeEndAll(string str, char key)
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
			str = str.Remove(removeStartCount);
		}
		return str;
	}
	public static string removeEndAll(string str, char key0, char key1)
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
			str = str.Remove(removeStartCount);
		}
		return str;
	}
	// 去掉整个stream中最后一个出现过的逗号
	public static void removeLastComma(ref string stream)
	{
		removeLast(ref stream, ',');
	}
	public static void removeLastComma(MyStringBuilder stream)
	{
		removeLast(stream, ',');
	}
	// json
	public static void jsonStartArray(MyStringBuilder str, string name = null, int preTableCount = 0, bool returnLine = false)
	{
		// 如果不是最外层的数组,则需要加上数组的名字
		if (!isEmpty(name))
		{
			str.append("\t", preTableCount);
			str.append("\"", name, "\"", ":");
			if (returnLine)
			{
				str.append("\r\n");
			}
		}
		str.append("\t", preTableCount);
		str.append("[");
		if (returnLine)
		{
			str.append("\r\n");
		}
	}
	public static void jsonEndArray(MyStringBuilder str, int preTableCount = 0, bool returnLine = false)
	{
		removeLastComma(str);
		str.append("\t", preTableCount);
		str.append("],");
		if (returnLine)
		{
			str.append("\r\n");
		}
	}
	public static void jsonStartStruct(MyStringBuilder str, string name = null, int preTableCount = 0, bool returnLine = false)
	{
		// 如果不是最外层的数组,则需要加上数组的名字
		if (!isEmpty(name))
		{
			str.append("\t", preTableCount);
			str.append("\"", name, "\"", ":");
			if (returnLine)
			{
				str.append("\r\n");
			}
		}
		// 如果不是最外层且非数组元素的结构体,则需要加上结构体的名字
		str.append("\t", preTableCount);
		str.append("{");
		if (returnLine)
		{
			str.append("\r\n");
		}
	}
	public static void jsonEndStruct(MyStringBuilder str, int preTableCount = 0, bool returnLine = false)
	{
		removeLastComma(str);
		str.append("\t", preTableCount);
		str.append("},");
		if (returnLine)
		{
			str.append("\r\n");
		}
	}
	public static void jsonAddPair(MyStringBuilder str, string name, string value, int preTableCount = 0, bool returnLine = false)
	{
		str.append("\t", preTableCount);
		// 如果是数组中的元素则不需要名字
		if (!isEmpty(name))
		{
			str.append("\"", name, "\": ");
		}
		str.append("\"", value, "\",");
		if (returnLine)
		{
			str.append("\r\n");
		}
	}
	public static void jsonAddObject(MyStringBuilder str, string name, string value, int preTableCount = 0, bool returnLine = false)
	{
		str.append("\t", preTableCount);
		str.append("\"", name, "\": ", value, ",");
		if (returnLine)
		{
			str.append("\r\n");
		}
	}
	// 解析一个数组类型的json字符串,并将每一个元素的字符串放入elementList中
	public static void decodeJsonArray(string json, List<string> elementList)
	{
		if (isEmpty(json))
		{
			return;
		}
		// 如果不是数组类型则无法解析
		if (json[0] != '[' || json[json.Length - 1] != ']')
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
			elementList.Add(json.Substring(startIndex + 1, endIndex - startIndex - 1));
			curIndex = endIndex + 1;
		}
	}
	// 解析一个json的结构体,需要所有参数都是字符串类型的
	public static void decodeJsonStringPair(string json, Dictionary<string, string> paramList)
	{
		if (isEmpty(json))
		{
			return;
		}
		string[] memberList = split(json, ',');
		if (memberList == null)
		{
			return;
		}
		for (int i = 0; i < memberList.Length; ++i)
		{
			string[] param = split(memberList[i], ':');
			if (param == null || param.Length != 2)
			{
				continue;
			}
			paramList.Add(removeAll(param[0], '\"'), removeAll(param[1], '\"'));
		}
	}
	// 绝对路径转换到相对于Asset的路径
	public static void fullPathToProjectPath(ref string path)
	{
		path = FrameDefine.P_ASSETS_PATH + path.Substring(FrameDefine.F_ASSETS_PATH.Length);
	}
	public static string fullPathToProjectPath(string path)
	{
		return FrameDefine.P_ASSETS_PATH + path.Substring(FrameDefine.F_ASSETS_PATH.Length);
	}
	public static void projectPathToFullPath(ref string path)
	{
		path = FrameDefine.F_ASSETS_PATH + path.Substring(FrameDefine.ASSETS.Length + 1);
	}
	public static string projectPathToFullPath(string path)
	{
		return FrameDefine.F_ASSETS_PATH + path.Substring(FrameDefine.ASSETS.Length + 1);
	}
	// 检查后缀,如果字符串没有指定后缀,则在后面加上后缀
	public static string checkSuffix(string path, string suffix)
	{
		if (!endWith(path, suffix))
		{
			path += suffix;
		}
		return path;
	}
	// 移除文件的后缀名
	public static string removeSuffix(string str)
	{
		int dotPos = str.IndexOf('.');
		if (dotPos != -1)
		{
			return str.Substring(0, dotPos);
		}
		return str;
	}
	public static string getFirstFolderName(string str)
	{
		rightToLeft(ref str);
		int firstPos = str.IndexOf('/');
		if (firstPos != -1)
		{
			return str.Substring(0, firstPos);
		}
		return EMPTY;
	}
	// 从文件路径中得到最后一级的文件夹名
	public static string getFolderName(string str)
	{
		MyStringBuilder builder = FrameUtility.STRING(str);
		rightToLeft(builder);

		// 如果有文件名,则先去除文件名
		int namePos = builder.lastIndexOf('/');
		int dotPos = builder.lastIndexOf('.');
		if (dotPos > namePos)
		{
			builder.remove(namePos);
		}

		// 如果是以/结尾的,则去除结尾的/
		if (builder[builder.Length - 1] == '/')
		{
			builder.remove(builder.Length - 1);
		}

		// 再去除当前目录的父级目录
		namePos = builder.lastIndexOf('/');
		if (namePos >= 0)
		{
			builder.remove(0, namePos + 1);
		}
		return FrameUtility.END_STRING(builder);
	}
	// 得到文件路径
	public static string getFilePath(string fileName, bool keepEndSlash = false)
	{
		MyStringBuilder builder = FrameUtility.STRING(fileName);
		rightToLeft(builder);
		// 从倒数第二个开始,因为即使最后一个是/也需要忽略
		int lastPos = builder.lastIndexOf('/', builder.Length - 2);
		if (lastPos < 0)
		{
			FrameUtility.DESTROY_STRING(builder);
			return EMPTY;
		}
		if (keepEndSlash)
		{
			return FrameUtility.END_STRING(builder.remove(lastPos + 1));
		}
		else
		{
			return FrameUtility.END_STRING(builder.remove(lastPos));
		}
	}
	public static string getFileName(string str)
	{
		MyStringBuilder builder = FrameUtility.STRING(str);
		rightToLeft(builder);
		int dotPos = builder.lastIndexOf('/');
		if (dotPos != -1)
		{
			builder.remove(0, dotPos + 1);
		}
		return FrameUtility.END_STRING(builder);
	}
	public static string getFileSuffix(string file)
	{
		int dotPos = file.IndexOf('.', file.LastIndexOf('/'));
		if (dotPos != -1)
		{
			return file.Substring(dotPos);
		}
		return EMPTY;
	}
	public static string getFileNameNoSuffix(string str, bool removeDir = false)
	{
		if (str == null)
		{
			return null;
		}
		MyStringBuilder builder = FrameUtility.STRING(str);
		rightToLeft(builder);
		// 先判断是否移除目录
		if (removeDir)
		{
			int namePos = builder.lastIndexOf('/');
			if (namePos != -1)
			{
				builder.remove(0, namePos + 1);
			}
		}
		// 移除后缀
		int dotPos = builder.lastIndexOf('.');
		if (dotPos != -1)
		{
			builder.remove(dotPos);
		}
		return FrameUtility.END_STRING(builder);
	}
	// 如果路径最后有斜杠,则移除结尾的斜杠
	public static void removeEndSlash(ref string path)
	{
		if (path[path.Length - 1] == '/')
		{
			path = path.Substring(0, path.Length - 1);
		}
	}
	public static string removeEndSlash(string path)
	{
		if (path[path.Length - 1] == '/')
		{
			path = path.Substring(0, path.Length - 1);
		}
		return path;
	}
	public static void addEndSlash(ref string path)
	{
		if (!isEmpty(path) && path[path.Length - 1] != '/')
		{
			path += "/";
		}
	}
	public static string addEndSlash(string path)
	{
		if (!isEmpty(path) && path[path.Length - 1] != '/')
		{
			path += "/";
		}
		return path;
	}
	public static string replaceSuffix(string fileName, string suffix)
	{
		return getFileNameNoSuffix(fileName) + suffix;
	}
	public static void rightToLeft(MyStringBuilder str)
	{
		str.replace('\\', '/');
	}
	public static void rightToLeft(ref string str)
	{
		str = str.Replace('\\', '/');
	}
	public static string rightToLeft(string str)
	{
		return str.Replace('\\', '/');
	}
	public static void leftToRight(MyStringBuilder str)
	{
		str.replace('/', '\\');
	}
	public static void leftToRight(ref string str)
	{
		str = str.Replace('/', '\\');
	}
	public static string leftToRight(string str)
	{
		return str.Replace('/', '\\');
	}
	public static char[] generateOtherASCII(params char[] exclude)
	{
		int curCount = 0;
		char[] ascii = new char[0xFF - exclude.Length];
		for (int i = 1; i < 0xFF + 1; ++i)
		{
			if (CSharpUtility.arrayContainsValue(exclude, (char)i))
			{
				continue;
			}
			if (curCount >= ascii.Length)
			{
				UnityUtility.logError("获取ASCII字符数组失败,排除列表中可能存在重复的字符");
			}
			ascii[curCount++] = (char)i;
		}
		return ascii;
	}
	public static void splitLine(string str, out string[] lines, bool removeEmpty = true)
	{
		lines = null;
		if (str.IndexOf('\n') >= 0)
		{
			lines = split(str, removeEmpty, '\n');
			if (lines == null)
			{
				return;
			}
			for (int i = 0; i < lines.Length; ++i)
			{
				lines[i] = removeAll(lines[i], '\r');
			}
		}
		else if(str.IndexOf('\r') >= 0)
		{
			lines = split(str, removeEmpty, '\r');
			if (lines == null)
			{
				return;
			}
			for (int i = 0; i < lines.Length; ++i)
			{
				lines[i] = removeAll(lines[i], '\n');
			}
		}
	}
	public static string[] split(string str, params string[] keyword)
	{
		return split(str, true, keyword);
	}
	public static string[] split(string str, bool removeEmpty, params string[] keyword)
	{
		if (isEmpty(str))
		{
			return null;
		}
		return str.Split(keyword, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
	}
	// 在使用返回值期间禁止再调用splitNonAlloc
	public static List<string> splitNonAlloc(string str, bool removeEmpty, params string[] keyword)
	{
		mTempStringList.Clear();
		if (!isEmpty(str))
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
		if (isEmpty(str))
		{
			return null;
		}
		return str.Split(keyword, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
	}
	// 在使用返回值期间禁止再调用splitNonAlloc
	public static List<string> splitNonAlloc(string str, bool removeEmpty, params char[] keyword)
	{
		mTempStringList.Clear();
		if (!isEmpty(str))
		{
			mTempStringList.AddRange(split(str, removeEmpty, keyword));
		}
		return mTempStringList;
	}
	// 在使用返回值期间禁止再调用stringToFloatsNonAlloc
	public static List<float> stringToFloatsNonAlloc(string str, char seperate = ',')
	{
		stringToFloats(str, mTempFloatList, seperate);
		return mTempFloatList;
	}
	public static List<float> stringToFloats(string str, char seperate = ',')
	{
		List<float> values = new List<float>();
		stringToFloats(str, values, seperate);
		return values;
	}
	public static void stringToFloats(string str, ref float[] values, char seperate = ',')
	{
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		int len = rangeList.Length;
		if (values != null && len != values.Length)
		{
			UnityUtility.logError("count is not equal " + str.Length);
			return;
		}
		if (values == null)
		{
			values = new float[len];
		}
		for (int i = 0; i < len; ++i)
		{
			values[i] = SToF(rangeList[i]);
		}
	}
	public static void stringToFloats(string str, List<float> values, char seperate = ',')
	{
		if (values == null)
		{
			UnityUtility.logError("values can not be null");
			return;
		}
		values.Clear();
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		int len = rangeList.Length;
		for (int i = 0; i < len; ++i)
		{
			values.Add(SToF(rangeList[i]));
		}
	}
	public static string floatsToString(float[] values, char seperate = ',')
	{
		MyStringBuilder builder = FrameUtility.STRING();
		int count = values.Length;
		for (int i = 0; i < count; ++i)
		{
			builder.append(FToS(values[i], 2));
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return FrameUtility.END_STRING(builder);
	}
	public static string floatsToString(List<float> values, char seperate = ',')
	{
		MyStringBuilder builder = FrameUtility.STRING();
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.append(FToS(values[i], 2));
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return FrameUtility.END_STRING(builder);
	}
	public static List<int> stringToInts(string str, char seperate = ',')
	{
		List<int> values = new List<int>();
		stringToInts(str, values, seperate);
		return values;
	}
	public static void stringToInts(string str, List<int> values, char seperate = ',')
	{
		if (values == null)
		{
			UnityUtility.logError("values can not be null");
			return;
		}
		values.Clear();
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		if (rangeList == null)
		{
			return;
		}
		int len = rangeList.Length;
		for (int i = 0; i < len; ++i)
		{
			values.Add(SToI(rangeList[i]));
		}
	}
	public static void stringToInts(string str, ref int[] values, char seperate = ',')
	{
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		int len = rangeList.Length;
		if (values != null && len != values.Length)
		{
			UnityUtility.logError("value.Length is not equal " + len + ", str:" + str);
			return;
		}
		if (values == null)
		{
			values = new int[len];
		}
		for (int i = 0; i < len; ++i)
		{
			values[i] = SToI(rangeList[i]);
		}
	}
	// 在使用返回值期间禁止再调用stringToIntsNonAlloc
	public static List<int> stringToIntsNonAlloc(string str, char seperate = ',')
	{
		stringToInts(str, mTempIntList, seperate);
		return mTempIntList;
	}
	public static void stringToUInts(string str, List<uint> values, char seperate = ',')
	{
		if (values == null)
		{
			UnityUtility.logError("values can not be null");
			return;
		}
		values.Clear();
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		if (rangeList == null)
		{
			return;
		}
		int len = rangeList.Length;
		for (int i = 0; i < len; ++i)
		{
			values.Add((uint)SToI(rangeList[i]));
		}
	}
	public static void stringToUShorts(string str, List<ushort> values, char seperate = ',')
	{
		if (values == null)
		{
			UnityUtility.logError("values can not be null");
			return;
		}
		values.Clear();
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		if (rangeList == null)
		{
			return;
		}
		int len = rangeList.Length;
		for (int i = 0; i < len; ++i)
		{
			values.Add((ushort)SToI(rangeList[i]));
		}
	}
	public static void stringToBools(string str, List<bool> values, char seperate = ',')
	{
		if (values == null)
		{
			UnityUtility.logError("values can not be null");
			return;
		}
		values.Clear();
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		if (rangeList == null)
		{
			return;
		}
		int len = rangeList.Length;
		for (int i = 0; i < len; ++i)
		{
			values.Add(SToI(rangeList[i]) > 0);
		}
	}
	public static void stringToBytes(string str, List<byte> values, char seperate = ',')
	{
		if (values == null)
		{
			UnityUtility.logError("values can not be null");
			return;
		}
		values.Clear();
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		if (rangeList == null)
		{
			return;
		}
		int len = rangeList.Length;
		for (int i = 0; i < len; ++i)
		{
			values.Add((byte)SToI(rangeList[i]));
		}
	}
	public static void stringToSBytes(string str, List<sbyte> values, char seperate = ',')
	{
		if (values == null)
		{
			UnityUtility.logError("values can not be null");
			return;
		}
		values.Clear();
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		if (rangeList == null)
		{
			return;
		}
		int len = rangeList.Length;
		for (int i = 0; i < len; ++i)
		{
			values.Add((sbyte)SToI(rangeList[i]));
		}
	}
	public static string intsToString(int[] values, char seperate = ',')
	{
		MyStringBuilder builder = FrameUtility.STRING();
		int count = values.Length;
		for (int i = 0; i < count; ++i)
		{
			builder.append(IToS(values[i]));
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return FrameUtility.END_STRING(builder);
	}
	public static string intsToString(List<int> values, char seperate = ',')
	{
		if (values == null)
		{
			return EMPTY;
		}
		MyStringBuilder builder = FrameUtility.STRING();
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.append(IToS(values[i]));
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return FrameUtility.END_STRING(builder);
	}
	public static string stringsToString(List<string> values, char seperate = ',')
	{
		if (values == null)
		{
			return EMPTY;
		}
		MyStringBuilder builder = FrameUtility.STRING();
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.append(values[i]);
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return FrameUtility.END_STRING(builder);
	}
	public static string stringsToString(List<string> values, string seperate)
	{
		if (values == null)
		{
			return EMPTY;
		}
		MyStringBuilder builder = FrameUtility.STRING();
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.append(values[i]);
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return FrameUtility.END_STRING(builder);
	}
	public static string stringsToString(string[] values, char seperate = ',')
	{
		if (values == null)
		{
			return EMPTY;
		}
		MyStringBuilder builder = FrameUtility.STRING();
		int count = values.Length;
		for (int i = 0; i < count; ++i)
		{
			builder.append(values[i]);
			if (i != count - 1)
			{
				builder.append(seperate);
			}
		}
		return FrameUtility.END_STRING(builder);
	}
	public static List<string> stringToStrings(string str, char seperate = ',')
	{
		List<string> strList = new List<string>();
		if (str == null)
		{
			return strList;
		}
		string[] strArray = split(str, seperate);
		if (strArray != null)
		{
			strList.AddRange(strArray);
		}
		return strList;
	}
	public static void stringToStrings(string str, List<string> values, char seperate = ',')
	{
		if (values == null)
		{
			UnityUtility.logError("values can not be null");
			return;
		}
		values.Clear();
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, seperate);
		if (rangeList == null)
		{
			return;
		}
		int len = rangeList.Length;
		for (int i = 0; i < len; ++i)
		{
			values.Add(rangeList[i]);
		}
	}
	// precision表示小数点后保留几位小数,removeTailZero表示是否去除末尾的0
	public static string FToS(float value, int precision = 4, bool removeTailZero = true)
	{
		MathUtility.checkInt(ref value);
		if (precision == 0)
		{
			return IToS((int)value);
		}
		if (removeTailZero)
		{
			// 是否非常接近数轴左边的整数
			if (MathUtility.isFloatZero(value - (int)value))
			{
				return IToS((int)value);
			}
			// 是否非常接近数轴右边的整数
			if (MathUtility.isFloatZero((int)value + 1 - value))
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
			if (removeCount > 0)
			{
				str = str.Substring(0, curLen - removeCount);
			}
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
		MyStringBuilder builder = FrameUtility.STRING(str);
		// 从后往前插入
		for (int i = 0; i < commaCount; ++i)
		{
			builder.insert(insertStart, ',');
			insertStart -= 3;
		}
		str = FrameUtility.END_STRING(builder);
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
	public static string intToChineseString(int value)
	{
		MyStringBuilder builder = FrameUtility.STRING();
		// 大于1亿
		if (value >= 100000000)
		{
			builder.append(IToS(value / 100000000), "亿");
			value %= 100000000;
		}
		// 大于1万
		if (value >= 10000)
		{
			builder.append(IToS(value / 10000), "万");
			value %= 10000;
		}
		if (value > 0)
		{
			builder.append(value);
		}
		return FrameUtility.END_STRING(builder);
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
	public static string vector2IntToString(Vector2Int value, int limitLength = 0)
	{
		return IToS(value.x, limitLength) + "," + IToS(value.y, limitLength);
	}
	public static void vector2IntToString(MyStringBuilder builder, Vector2Int value, int limitLength = 0)
	{
		builder.append(IToS(value.x, limitLength), ",", IToS(value.y, limitLength));
	}
	public static string vector2ToString(Vector2 value, int precision = 4)
	{
		return FToS(value.x, precision) + "," + FToS(value.y, precision);
	}
	public static void vector2ToString(MyStringBuilder builder, Vector2 value, int precision = 4)
	{
		builder.append(FToS(value.x, precision), ",", FToS(value.y, precision));
	}
	public static string vector3ToString(Vector3 value, int precision = 4)
	{
		return strcat(FToS(value.x, precision), ",", FToS(value.y, precision), ",", FToS(value.z, precision));
	}
	public static void vector3ToString(MyStringBuilder builder, Vector3 value, int precision = 4)
	{
		builder.append(FToS(value.x, precision), ",", FToS(value.y, precision), ",", FToS(value.z, precision));
	}
	// 将str中的[begin,end)替换为reStr
	public static string replace(string str, int begin, int end, string reStr)
	{
		MyStringBuilder builder = FrameUtility.STRING(str);
		replace(builder, begin, end, reStr);
		return FrameUtility.END_STRING(builder);
	}
	public static void replace(MyStringBuilder str, int begin, int end, string reStr)
	{
		str.remove(begin, end - begin);
		if (reStr.Length > 0)
		{
			str.insert(begin, reStr);
		}
	}
	public static string replaceAll(string str, string key, string newWords)
	{
		MyStringBuilder builder = FrameUtility.STRING(str);
		replaceAll(builder, key, newWords);
		return FrameUtility.END_STRING(builder);
	}
	public static string replaceAll(string str, char key, char newWords)
	{
		MyStringBuilder builder = FrameUtility.STRING(str);
		replaceAll(builder, key, newWords);
		return FrameUtility.END_STRING(builder);
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
			startPos = pos + newWords.Length;
		}
	}
	public static void replaceAll(MyStringBuilder builder, char key, char newWords)
	{
		int len = builder.Length;
		for (int i = 0; i < len; ++i)
		{
			if (builder[i] == key)
			{
				builder[i] = newWords;
			}
		}
	}
	// 移除str的从开头到stopChar的部分,包括stopChar,findFromStart为true表示寻找第一个stopChar,false表示寻找最后一个stopChar
	public static string removeStartUntil(string str, char stopChar, bool findFromStart)
	{
		int pos = findFromStart ? str.IndexOf(stopChar) : str.LastIndexOf(stopChar);
		if (pos < 0)
		{
			return str;
		}
		return str.Substring(pos + 1);
	}
	public static string removeAll(string str, params string[] key)
	{
		MyStringBuilder builder = FrameUtility.STRING(str);
		int keyCount = key.Length;
		for (int i = 0; i < keyCount; ++i)
		{
			replaceAll(builder, key[i], EMPTY);
		}
		return FrameUtility.END_STRING(builder);
	}
	public static string removeAll(string str, params char[] key)
	{
		MyStringBuilder builder = FrameUtility.STRING(str);
		for (int i = builder.Length - 1; i >= 0; --i)
		{
			// 判断是否是需要移除的字符
			if (CSharpUtility.arrayContainsValue(key, builder[i]))
			{
				builder.remove(i, 1);
			}
		}
		return FrameUtility.END_STRING(builder);
	}
	public static float SToF(string str)
	{
		checkFloatString(str);
		if (isEmpty(str) || str == "-")
		{
			return 0.0f;
		}
		return float.Parse(str);
	}
	public static int getLength(string str)
	{
		byte[] bytes = stringToBytes(str);
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
#if UNITY_EDITOR
		int oldStrLen = str.Length;
		for (int i = 0; i < oldStrLen; ++i)
		{
			if (valid.IndexOf(str[i]) < 0)
			{
				UnityUtility.logError("不合法的字符串:" + str);
				return false;
			}
		}
#endif
		return true;
	}
	public static bool checkFloatString(string str, string valid = EMPTY)
	{
#if UNITY_EDITOR
		return checkString(str, "-0123456789.E" + valid);
#else
		return true;
#endif
	}
	public static bool checkIntString(string str, string valid = EMPTY)
	{
#if UNITY_EDITOR
		return checkString(str, "-0123456789" + valid);
#else
		return true;
#endif
	}
	public static bool checkUIntString(string str, string valid = EMPTY)
	{
#if UNITY_EDITOR
		return checkString(str, "0123456789" + valid);
#else
		return true;
#endif
	}
	public static string checkNickName(string name, bool removeOrTransfer)
	{
		if (removeOrTransfer)
		{
			name = removeAll(name, '&');
			name = removeAll(name, '\\');
		}
		else
		{
			name = replaceAll(name, "&", "%26");
			name = replaceAll(name, "\\", "%5C%5C");
		}
		return name;
	}
	public static string bytesToHEXStringThread(byte[] byteList, int offset = 0, int count = 0, bool addSpace = true, bool upperOrLower = true)
	{
		MyStringBuilder builder = FrameUtility.STRING_THREAD();
		int byteCount = count > 0 ? count : byteList.Length - offset;
		MathUtility.clamp(ref byteCount, 0, byteList.Length - offset);
		for (int i = 0; i < byteCount; ++i)
		{
			if (addSpace)
			{
				byteToHEXString(builder, byteList[i + offset], upperOrLower);
				if (i != byteCount - 1)
				{
					builder.append(" ");
				}
			}
			else
			{
				byteToHEXString(builder, byteList[i + offset], upperOrLower);
			}
		}
		return FrameUtility.END_STRING_THREAD(builder);
	}
	public static string bytesToHEXString(byte[] byteList, int offset = 0, int count = 0, bool addSpace = true, bool upperOrLower = true)
	{
		MyStringBuilder builder = FrameUtility.STRING();
		int byteCount = count > 0 ? count : byteList.Length - offset;
		MathUtility.clamp(ref byteCount, 0, byteList.Length - offset);
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
		return FrameUtility.END_STRING(builder);
	}
	public static string byteToHEXString(byte value, bool upperOrLower = true)
	{
		MyStringBuilder builder = FrameUtility.STRING();
		char[] hexChar = upperOrLower ? mHexUpperChar : mHexLowerChar;
		int high = value / 16;
		int low = value % 16;
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
		return FrameUtility.END_STRING(builder);
	}
	public static void byteToHEXString(MyStringBuilder builder, byte value, bool upperOrLower = true)
	{
		char[] hexChar = upperOrLower ? mHexUpperChar : mHexLowerChar;
		int high = value / 16;
		int low = value % 16;
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
	public static byte hexStringToByte(string str, int start = 0)
	{
		byte highBit = 0;
		byte lowBit = 0;
		byte[] strBytes = stringToBytes(str);
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
		if (isEmpty(str) || str.Length % 2 != 0)
		{
			return 0;
		}
		int dataCount = str.Length >> 1;
		FrameUtility.ARRAY_THREAD(out bytes, MathUtility.getGreaterPow2(dataCount));
		for (int i = 0; i < dataCount; ++i)
		{
			bytes[i] = hexStringToByte(str, i * 2);
		}
		return dataCount;
	}
	public static void releaseHexStringBytes(byte[] bytes)
	{
		FrameUtility.UN_ARRAY_THREAD(bytes);
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
	public static void addSprite(MyStringBuilder originString, string spriteName, float width = 1.0f)
	{
		originString.append("<quad width=").append(width).append(" sprite=").append(spriteName).append("/>");
	}
	// 在文本显示中将str的颜色设置为color
	public static string colorStringNoBuilder(string color, string str)
	{
		if (isEmpty(str))
		{
			return EMPTY;
		}
		return "<color=#" + color + ">" + str + "</color>";
	}
	// 在文本显示中将str的颜色设置为color
	public static string colorString(string color, string str)
	{
		if (isEmpty(str))
		{
			return EMPTY;
		}
		return strcat("<color=#", color, ">", str, "</color>");
	}
	// 在文本显示中将str的颜色设置为color
	public static string colorStringThread(string color, string str)
	{
		if (isEmpty(str))
		{
			return EMPTY;
		}
		return strcat_thread("<color=#", color, ">", str, "</color>");
	}
	public static MyStringBuilder colorString(string color, MyStringBuilder str)
	{
		if (str.Length == 0)
		{
			return str;
		}
		str.insertFront("<color=#", color, ">");
		str.append("</color>");
		return str;
	}
	public static string colorToRGBString(Color32 color)
	{
		return byteToHEXString(color.r) + byteToHEXString(color.g) + byteToHEXString(color.b);
	}
	public static string colorToRGBAString(Color32 color)
	{
		return byteToHEXString(color.r) + byteToHEXString(color.g) + byteToHEXString(color.b) + byteToHEXString(color.a);
	}
	// returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
	public static int findFirstSubstr(string res, char pattern, int startPos = 0, bool sensitive = true)
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
	public static int findFirstSubstr(MyStringBuilder res, char pattern, int startPos = 0, bool sensitive = true)
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
	public static int findLastSubstr(string res, string pattern, bool sensitive, int startPos = 0)
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
	public static int findLastChar(string str, char c, int endPos = -1)
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
	public static string removeFirstBetweenPairChars(string str, char startChar, char endChar, out int startCharIndex, out int endCharIndex)
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
				--unpaired;
				if (unpaired == 0)
				{
					endCharIndex = i;
					return str.Remove(startCharIndex, endCharIndex - startCharIndex + 1);
				}
			}
		}
		return str;
	}
	// 从后往前去除两个成对个字符之间的所有字符(包含两个字符)，并返回他们的下标
	public static string removeLastBetweenPairChars(string str, char startChar, char endChar, out int startCharIndex, out int endCharIndex)
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
				--unpaired;
				if (unpaired == 0)
				{
					startCharIndex = i;
					return str.Remove(startCharIndex, endCharIndex - startCharIndex + 1);
				}
			}
		}
		return str;
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
		foreach (var element in str)
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
	public static bool isNumberic(char c) { return c >= '0' && c <= '9'; }
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
			if (!isNumberic(str[i]))
			{
				return false;
			}
		}
		return true;
	}
	public static void line(MyStringBuilder str, string line, bool returnLine = true)
	{
		if (returnLine)
		{
			str.append(line, "\r\n");
		}
		else
		{
			str.append(line);
		}
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
		FrameUtility.LIST(out List<string> macroList);
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
				isNumberic(curChar) && (!isNumberic(lastChar) || (nextChar != '\0' && !isNumberic(nextChar))))
			{
				macroList.Add(sqliteName.Substring(lastIndex, i - lastIndex));
				lastIndex = i;
			}
		}
		macroList.Add(sqliteName.Substring(lastIndex, length - lastIndex));

		MyStringBuilder headerMacro = FrameUtility.STRING();
		int elementCount = macroList.Count;
		for (int i = 0; i < elementCount; ++i)
		{
			headerMacro.append("_", macroList[i].ToUpper());
		}
		if (!preUnderLine)
		{
			headerMacro.remove(0, 1);
		}
		FrameUtility.UN_LIST(macroList);
		return FrameUtility.END_STRING(headerMacro);
	}
	public static string validateHttpString(string str)
	{
		if (mInvalidParamChars == null)
		{
			initInvalidChars();
		}
		MyStringBuilder builder = FrameUtility.STRING(str);
		foreach (var item in mInvalidParamChars)
		{
			replaceAll(builder, item.Key, item.Value);
		}
		return FrameUtility.END_STRING(builder);
	}
	public static void appendValueString(ref string queryStr, string str)
	{
		queryStr += "\"" + str + "\",";
	}
	public static void appendValueString(MyStringBuilder queryStr, string str)
	{
		queryStr.append("\"", str, "\",");
	}
	public static void appendValueVector2(ref string queryStr, Vector2 value)
	{
		queryStr += vector2ToString(value) + ",";
	}
	public static void appendValueVector2(MyStringBuilder queryStr, Vector2 value)
	{
		vector2ToString(queryStr, value);
		queryStr.append(',');
	}
	public static void appendValueVector2Int(ref string queryStr, Vector2Int value)
	{
		queryStr += vector2IntToString(value) + ",";
	}
	public static void appendValueVector2Int(MyStringBuilder queryStr, Vector2Int value)
	{
		vector2IntToString(queryStr, value);
		queryStr.append(',');
	}
	public static void appendValueVector3(ref string queryStr, Vector3 value)
	{
		queryStr += vector3ToString(value) + ",";
	}
	public static void appendValueVector3(MyStringBuilder queryStr, Vector3 value)
	{
		vector3ToString(queryStr, value);
		queryStr.append(',');
	}
	public static void appendValueInt(ref string queryStr, int value)
	{
		queryStr += IToS(value) + ",";
	}
	public static void appendValueInt(MyStringBuilder queryStr, int value)
	{
		queryStr.append(IToS(value), ",");
	}
	public static void appendValueUInt(ref string queryStr, uint value)
	{
		queryStr += IToS(value) + ",";
	}
	public static void appendValueUInt(MyStringBuilder queryStr, uint value)
	{
		queryStr.append(IToS(value), ",");
	}
	public static void appendValueFloat(ref string queryStr, float value)
	{
		queryStr += FToS(value) + ",";
	}
	public static void appendValueFloat(MyStringBuilder queryStr, float value)
	{
		queryStr.append(FToS(value), ",");
	}
	public static void appendValueFloats(ref string queryStr, List<float> floatArray)
	{
		appendValueString(ref queryStr, floatsToString(floatArray));
	}
	public static void appendValueFloats(MyStringBuilder queryStr, List<float> floatArray)
	{
		appendValueString(queryStr, floatsToString(floatArray));
	}
	public static void appendValueInts(ref string queryStr, List<int> intArray)
	{
		appendValueString(ref queryStr, intsToString(intArray));
	}
	public static void appendValueInts(MyStringBuilder queryStr, List<int> intArray)
	{
		appendValueString(queryStr, intsToString(intArray));
	}
	public static void appendConditionString(ref string condition, string col, string str, string operate)
	{
		condition += col + " = " + "\"" + str + "\"" + operate;
	}
	public static void appendConditionString(MyStringBuilder condition, string col, string str, string operate)
	{
		condition.append(col, "=\"", str, "\"", operate);
	}
	public static void appendConditionInt(ref string condition, string col, int value, string operate)
	{
		condition += col + " = " + IToS(value) + operate;
	}
	public static void appendConditionInt(MyStringBuilder condition, string col, int value, string operate)
	{
		condition.append(col, " = ", IToS(value), operate);
	}
	public static void appendUpdateString(ref string updateStr, string col, string str)
	{
		updateStr += col + " = " + "\"" + str + "\",";
	}
	public static void appendUpdateString(MyStringBuilder updateStr, string col, string str)
	{
		updateStr.append(col, " = \"", str, "\",");
	}
	public static void appendUpdateInt(ref string updateStr, string col, int value)
	{
		updateStr += col + " = " + IToS(value) + ",";
	}
	public static void appendUpdateInt(MyStringBuilder updateStr, string col, int value)
	{
		updateStr.append(col, " = ", IToS(value), ",");
	}
	public static void appendUpdateInts(ref string updateStr, string col, List<int> intArray)
	{
		appendUpdateString(ref updateStr, col, intsToString(intArray));
	}
	public static void appendUpdateInts(MyStringBuilder updateStr, string col, List<int> intArray)
	{
		appendUpdateString(updateStr, col, intsToString(intArray));
	}
	public static void appendUpdateFloats(ref string updateStr, string col, List<float> floatArray)
	{
		appendUpdateString(ref updateStr, col, floatsToString(floatArray));
	}
	public static void appendUpdateFloats(MyStringBuilder updateStr, string col, List<float> floatArray)
	{
		appendUpdateString(updateStr, col, floatsToString(floatArray));
	}
	public static int KMPSearch(string str, string pattern, int[] nextIndex = null)
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
	// 可以在子线程中使用的字符串拼接,当拼接小于等于4个字符串时,直接使用+号最快,GC与StringBuilder一致
	public static string strcat_thread(string str0, string str1, string str2, string str3, string str4)
	{
		return FrameUtility.END_STRING_THREAD(FrameUtility.STRING_THREAD(str0, str1, str2, str3, str4));
	}
	public static string strcat_thread(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		return FrameUtility.END_STRING_THREAD(FrameUtility.STRING_THREAD(str0, str1, str2, str3, str4, str5));
	}
	public static string strcat_thread(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		return FrameUtility.END_STRING_THREAD(FrameUtility.STRING_THREAD(str0, str1, str2, str3, str4, str5, str6));
	}
	public static string strcat_thread(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		return FrameUtility.END_STRING_THREAD(FrameUtility.STRING_THREAD(str0, str1, str2, str3, str4, str5, str6, str7));
	}
	public static string strcat_thread(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		return FrameUtility.END_STRING_THREAD(FrameUtility.STRING_THREAD(str0, str1, str2, str3, str4, str5, str6, str7, str8));
	}
	public static string strcat_thread(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		return FrameUtility.END_STRING_THREAD(FrameUtility.STRING_THREAD(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9));
	}
	// 只能在主线程中使用的字符串拼接,当拼接小于等于4个字符串时,直接使用+号最快,GC与StringBuilder一致
	public static string strcat(string str0, string str1, string str2, string str3, string str4)
	{
		return FrameUtility.END_STRING(FrameUtility.STRING(str0, str1, str2, str3, str4));
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		return FrameUtility.END_STRING(FrameUtility.STRING(str0, str1, str2, str3, str4, str5));
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		return FrameUtility.END_STRING(FrameUtility.STRING(str0, str1, str2, str3, str4, str5, str6));
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		return FrameUtility.END_STRING(FrameUtility.STRING(str0, str1, str2, str3, str4, str5, str6, str7));
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		return FrameUtility.END_STRING(FrameUtility.STRING(str0, str1, str2, str3, str4, str5, str6, str7, str8));
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		return FrameUtility.END_STRING(FrameUtility.STRING(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void initIntToString()
	{
		mIntToString = new string[1025];
		mStringToInt = new Dictionary<string, int>();
		for (int i = 0; i < mIntToString.Length; ++i)
		{
			string iStr = i.ToString();
			mStringToInt.Add(iStr, i);
			mIntToString[i] = iStr;
		}
	}
	protected static void initInvalidChars()
	{
		mInvalidParamChars = new Dictionary<string, string>();
		mInvalidParamChars.Add("(", "%28" );
		mInvalidParamChars.Add(")", "%29");
		mInvalidParamChars.Add("<", "%3C");
		mInvalidParamChars.Add(">", "%3E");
		mInvalidParamChars.Add("@", "%40");
		mInvalidParamChars.Add(",", "%2C");
		mInvalidParamChars.Add(";", "%3B");
		mInvalidParamChars.Add(":", "%3A");
		mInvalidParamChars.Add("\\", "%5C");
		mInvalidParamChars.Add("\"", "%22");
		mInvalidParamChars.Add("\'", "%27");
		mInvalidParamChars.Add("/", "%2F");
		mInvalidParamChars.Add("[", "%5B");
		mInvalidParamChars.Add("]", "%5D");
		mInvalidParamChars.Add("?", "%3F");
		mInvalidParamChars.Add("=", "%3D");
		mInvalidParamChars.Add("{", "%7B");
		mInvalidParamChars.Add("}", "%7D");
		mInvalidParamChars.Add(" ", "%20");
		mInvalidParamChars.Add("\t", "%09");
		mInvalidParamChars.Add("\r", "%0D");
		mInvalidParamChars.Add("\n", "%0A");
	}
}