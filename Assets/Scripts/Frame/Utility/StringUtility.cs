using System;
using System.Collections.Generic;
using UnityEngine;

public class StringUtility : BinaryUtility
{
	private static char[] mHexUpperChar = new char[] { 'A', 'B', 'C', 'D', 'E', 'F' };
	private static char[] mHexLowerChar = new char[] { 'a', 'b', 'c', 'd', 'e', 'f' };
	private static string mHexString = "ABCDEFabcdef0123456789";
	private static List<int> mTempIntList = new List<int>();
	private static List<float> mTempFloatList = new List<float>();
	private static List<string> mTempStringList = new List<string>();
	private static Dictionary<string, int> mStringToInt = new Dictionary<string, int>();
	private static string[] mIntToString = new string[1025];
	private static string[] mFloatConvertPercision = new string[] { "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7" };
	private static Dictionary<string, Vector2Int> mStringToVector2Cache;
	private static int STRING_TO_VECTOR2INT_MAX_CACHE = 10240;
	public const string EMPTY = "";
	public static new void initUtility() 
	{
		for(int i = 0; i < mIntToString.Length; ++i)
		{
			mStringToInt.Add(i.ToString(), i);
			mIntToString[i] = i.ToString();
		}
		mStringToVector2Cache = new Dictionary<string, Vector2Int>(STRING_TO_VECTOR2INT_MAX_CACHE);
	}
	// 只能使用{index}拼接
	public static string format(string format, params string[] args)
	{
		MyStringBuilder builder = FrameBase.STRING(format);
		MyStringBuilder helpBuilder = FrameBase.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.Clear();
			helpBuilder.Append("{", IToS(index), "}");
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
		FrameBase.DESTROY_STRING(helpBuilder);
		return FrameBase.END_STRING(builder);
	}
	public static string format(string format, List<string> args)
	{
		MyStringBuilder builder = FrameBase.STRING(format);
		MyStringBuilder helpBuilder = FrameBase.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.Clear();
			helpBuilder.Append("{", IToS(index), "}");
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
		FrameBase.DESTROY_STRING(helpBuilder);
		return FrameBase.END_STRING(builder);
	}
	public static string format(string format, List<int> args)
	{
		MyStringBuilder builder = FrameBase.STRING(format);
		MyStringBuilder helpBuilder = FrameBase.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.Clear();
			helpBuilder.Append("{", IToS(index), "}");
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
		FrameBase.DESTROY_STRING(helpBuilder);
		return FrameBase.END_STRING(builder);
	}
	public static string format(string format, params int[] args)
	{
		MyStringBuilder builder = FrameBase.STRING(format);
		MyStringBuilder helpBuilder = FrameBase.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.Clear();
			helpBuilder.Append("{", IToS(index), "}");
			string indexStr = helpBuilder.ToString();
			if (findFirstSubstr(builder, indexStr) < 0)
			{
				break;
			}
			if(index >= args.Length)
			{
				UnityUtility.logError("参数数量不足");
			}
			replaceAll(builder, indexStr, IToS(args[index]));
			++index;
		}
		FrameBase.DESTROY_STRING(helpBuilder);
		return FrameBase.END_STRING(builder);
	}
	public static string format(string format, List<float> args)
	{
		MyStringBuilder builder = FrameBase.STRING(format);
		MyStringBuilder helpBuilder = FrameBase.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.Clear();
			helpBuilder.Append("{", IToS(index), "}");
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
		FrameBase.DESTROY_STRING(helpBuilder);
		return FrameBase.END_STRING(builder);
	}
	public static string format(string format, params float[] args)
	{
		MyStringBuilder builder = FrameBase.STRING(format);
		MyStringBuilder helpBuilder = FrameBase.STRING();
		int index = 0;
		while (true)
		{
			helpBuilder.Clear();
			helpBuilder.Append("{", IToS(index), "}");
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
		FrameBase.DESTROY_STRING(helpBuilder);
		return FrameBase.END_STRING(builder);
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
		string numStr = str.Substring(lastPos + 1, str.Length - lastPos - 1);
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
		if(mStringToInt.TryGetValue(str, out int value))
		{
			return value;
		}
		return int.Parse(str);
	}
	public static long stringToLong(string str)
	{
		checkIntString(str);
		if (isEmpty(str))
		{
			return 0;
		}
		return long.Parse(str);
	}
	public static ulong stringToULong(string str)
	{
		checkUIntString(str);
		if (isEmpty(str))
		{
			return 0;
		}
		return ulong.Parse(str);
	}
	public static uint stringToUInt(string str)
	{
		checkUIntString(str);
		if (isEmpty(str))
		{
			return 0;
		}
		return uint.Parse(str);
	}
	public static Vector2 stringToVector2(string value, string seperate = ",")
	{
		if (isEmpty(value) || value == "0,0")
		{
			return Vector2.zero;
		}
		string[] splitList = split(value, true, seperate);
		if (splitList == null || splitList.Length < 2)
		{
			return Vector2.zero;
		}
		Vector2 v = new Vector2();
		v.x = SToF(splitList[0]);
		v.y = SToF(splitList[1]);
		return v;
	}
	public static Vector2Int stringToVector2Int(string value, string seperate = ",")
	{
		if (isEmpty(value) || value == "0,0")
		{
			return Vector2Int.zero;
		}
		if (mStringToVector2Cache.TryGetValue(value, out Vector2Int result))
		{
			return result;
		}
		string[] splitList = split(value, true, seperate);
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
	public static Vector3 stringToVector3(string value, string seperate = ",")
	{
		if (isEmpty(value) || value == "0,0,0")
		{
			return Vector3.zero;
		}
		string[] splitList = split(value, true, seperate);
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
	public static Vector4 stringToVector4(string value, string seperate = ",")
	{
		if (isEmpty(value) || value == "0,0,0,0")
		{
			return Vector4.zero;
		}
		string[] splitList = split(value, true, seperate);
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
				stream.Remove(length - 1 - i, 1);
				break;
			}
		}
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
			str.Append("\t", preTableCount);
			str.Append("\"", name, "\"", ":");
			if (returnLine)
			{
				str.Append("\r\n");
			}
		}
		str.Append("\t", preTableCount);
		str.Append("[");
		if (returnLine)
		{
			str.Append("\r\n");
		}
	}
	public static void jsonEndArray(MyStringBuilder str, int preTableCount = 0, bool returnLine = false)
	{
		removeLastComma(str);
		str.Append("\t", preTableCount);
		str.Append("],");
		if (returnLine)
		{
			str.Append("\r\n");
		}
	}
	public static void jsonStartStruct(MyStringBuilder str, string name = null, int preTableCount = 0, bool returnLine = false)
	{
		// 如果不是最外层的数组,则需要加上数组的名字
		if (!isEmpty(name))
		{
			str.Append("\t", preTableCount);
			str.Append("\"", name, "\"", ":");
			if (returnLine)
			{
				str.Append("\r\n");
			}
		}
		// 如果不是最外层且非数组元素的结构体,则需要加上结构体的名字
		str.Append("\t", preTableCount);
		str.Append("{");
		if (returnLine)
		{
			str.Append("\r\n");
		}
	}
	public static void jsonEndStruct(MyStringBuilder str, int preTableCount = 0, bool returnLine = false)
	{
		removeLastComma(str);
		str.Append("\t", preTableCount);
		str.Append("},");
		if (returnLine)
		{
			str.Append("\r\n");
		}
	}
	public static void jsonAddPair(MyStringBuilder str, string name, string value, int preTableCount = 0, bool returnLine = false)
	{
		str.Append("\t", preTableCount);
		// 如果是数组中的元素则不需要名字
		if (!isEmpty(name))
		{
			str.Append("\"", name, "\": ");
		}
		str.Append("\"", value, "\",");
		if (returnLine)
		{
			str.Append("\r\n");
		}
	}
	public static void jsonAddObject(MyStringBuilder str, string name, string value, int preTableCount = 0, bool returnLine = false)
	{
		str.Append("\t", preTableCount);
		str.Append("\"", name, "\": ", value, ",");
		if (returnLine)
		{
			str.Append("\r\n");
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
		MyStringBuilder builder = FrameBase.STRING(str);
		rightToLeft(builder);
		// 如果有文件名,则先去除文件名
		int namePos = builder.LastIndexOf('/');
		int dotPos = builder.LastIndexOf('.');
		if (dotPos > namePos)
		{
			builder.Remove(namePos);
		}
		// 再去除当前目录的父级目录
		namePos = builder.LastIndexOf('/');
		if (namePos != -1)
		{
			builder.Remove(0, namePos + 1);
		}
		return FrameBase.END_STRING(builder);
	}
	// 得到文件路径
	public static string getFilePath(string fileName)
	{
		MyStringBuilder builder = FrameBase.STRING(fileName);
		rightToLeft(builder);
		int lastPos = builder.LastIndexOf('/');
		if (lastPos != -1)
		{
			return FrameBase.END_STRING(builder.Remove(lastPos));
		}
		FrameBase.DESTROY_STRING(builder);
		return EMPTY;
	}
	public static string getFileName(string str)
	{
		MyStringBuilder builder = FrameBase.STRING(str);
		rightToLeft(builder);
		int dotPos = builder.LastIndexOf('/');
		if (dotPos != -1)
		{
			builder.Remove(0, dotPos + 1);
		}
		return FrameBase.END_STRING(builder);
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
		MyStringBuilder builder = FrameBase.STRING(str);
		rightToLeft(builder);
		// 先判断是否移除目录
		if (removeDir)
		{
			int namePos = builder.LastIndexOf('/');
			if(namePos != -1)
			{
				builder.Remove(0, namePos + 1);
			}
		}
		// 移除后缀
		int dotPos = builder.LastIndexOf('.');
		if (dotPos != -1)
		{
			builder.Remove(dotPos);
		}
		return FrameBase.END_STRING(builder);
	}
	// 如果路径最后有斜杠,则移除结尾的斜杠
	public static void removeEndSlash(ref string path)
	{
		if (endWith(path, "/"))
		{
			path = path.Substring(0, path.Length - 1);
		}
	}
	public static void addEndSlash(ref string path)
	{
		if (!isEmpty(path) && !endWith(path, "/"))
		{
			path += "/";
		}
	}
	public static string replaceSuffix(string fileName, string suffix)
	{
		return getFileNameNoSuffix(fileName) + suffix;
	}
	public static void rightToLeft(MyStringBuilder str)
	{
		str.Replace('\\', '/');
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
		str.Replace('/', '\\');
	}
	public static void leftToRight(ref string str)
	{
		str = str.Replace('/', '\\');
	}
	public static string leftToRight(string str)
	{
		return str.Replace('/', '\\');
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
	// 在使用返回值期间禁止再调用stringToFloatsNonAlloc
	public static List<float> stringToFloatsNonAlloc(string str, string seperate = ",")
	{
		stringToFloats(str, mTempFloatList, seperate);
		return mTempFloatList;
	}
	public static List<float> stringToFloats(string str, string seperate = ",")
	{
		List<float> values = new List<float>();
		stringToFloats(str, values, seperate);
		return values;
	}
	public static void stringToFloats(string str, ref float[] values, string seperate = ",")
	{
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, true, seperate);
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
	public static void stringToFloats(string str, List<float> values, string seperate = ",")
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
		string[] rangeList = split(str, true, seperate);
		int len = rangeList.Length;
		for (int i = 0; i < len; ++i)
		{
			values.Add(SToF(rangeList[i]));
		}
	}
	public static string floatsToString(float[] values, string seperate = ",")
	{
		MyStringBuilder builder = FrameBase.STRING();
		int count = values.Length;
		for (int i = 0; i < count; ++i)
		{
			builder.Append(FToS(values[i], 2));
			if (i != count - 1)
			{
				builder.Append(seperate);
			}
		}
		return FrameBase.END_STRING(builder);
	}
	public static string floatsToString(List<float> values, string seperate = ",")
	{
		MyStringBuilder builder = FrameBase.STRING();
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.Append(FToS(values[i], 2));
			if (i != count - 1)
			{
				builder.Append(seperate);
			}
		}
		return FrameBase.END_STRING(builder);
	}
	public static List<int> stringToInts(string str, string seperate = ",")
	{
		List<int> values = new List<int>();
		stringToInts(str, values, seperate);
		return values;
	}
	public static void stringToInts(string str, List<int> values, string seperate = ",")
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
		string[] rangeList = split(str, true, seperate);
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
	public static void stringToInts(string str, ref int[] values, string seperate = ",")
	{
		if (isEmpty(str))
		{
			return;
		}
		string[] rangeList = split(str, true, seperate);
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
	public static List<int> stringToIntsNonAlloc(string str, string seperate = ",")
	{
		stringToInts(str, mTempIntList, seperate);
		return mTempIntList;
	}
	public static void stringToUInts(string str, List<uint> values, string seperate = ",")
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
		string[] rangeList = split(str, true, seperate);
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
	public static void stringToUShorts(string str, List<ushort> values, string seperate = ",")
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
		string[] rangeList = split(str, true, seperate);
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
	public static void stringToBools(string str, List<bool> values, string seperate = ",")
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
		string[] rangeList = split(str, true, seperate);
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
	public static void stringToBytes(string str, List<byte> values, string seperate = ",")
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
		string[] rangeList = split(str, true, seperate);
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
	public static void stringToSBytes(string str, List<sbyte> values, string seperate = ",")
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
		string[] rangeList = split(str, true, seperate);
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
	public static string intsToString(int[] values, string seperate = ",")
	{
		MyStringBuilder builder = FrameBase.STRING();
		int count = values.Length;
		for (int i = 0; i < count; ++i)
		{
			builder.Append(IToS(values[i]));
			if (i != count - 1)
			{
				builder.Append(seperate);
			}
		}
		return FrameBase.END_STRING(builder);
	}
	public static string intsToString(List<int> values, string seperate = ",")
	{
		MyStringBuilder builder = FrameBase.STRING();
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.Append(IToS(values[i]));
			if (i != count - 1)
			{
				builder.Append(seperate);
			}
		}
		return FrameBase.END_STRING(builder);
	}
	public static string stringsToString(List<string> values, string seperate = ",")
	{
		MyStringBuilder builder = FrameBase.STRING();
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.Append(values[i]);
			if (i != count - 1)
			{
				builder.Append(seperate);
			}
		}
		return FrameBase.END_STRING(builder);
	}
	public static string stringsToString(string[] values, string seperate = ",")
	{
		MyStringBuilder builder = FrameBase.STRING();
		int count = values.Length;
		for (int i = 0; i < count; ++i)
		{
			builder.Append(values[i]);
			if (i != count - 1)
			{
				builder.Append(seperate);
			}
		}
		return FrameBase.END_STRING(builder);
	}
	public static List<string> stringToStrings(string str, string seperate = ",")
	{
		List<string> strList = new List<string>();
		string[] strArray = split(str, true, seperate);
		if (strArray != null)
		{
			strList.AddRange(strArray);
		}
		return strList;
	}
	public static void stringToStrings(string str, List<string> values, string seperate = ",")
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
		string[] rangeList = split(str, true, seperate);
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
		if(precision == 0)
		{
			return IToS((int)value);
		}
		if(removeTailZero)
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
			if(removeCount > 0)
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
		MyStringBuilder builder = FrameBase.STRING(str);
		// 从后往前插入
		for (int i = 0; i < commaCount; ++i)
		{
			builder.Insert(insertStart, ",");
			insertStart -= 3;
		}
		str = FrameBase.END_STRING(builder);
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
		MyStringBuilder builder = FrameBase.STRING();
		// 大于1亿
		if(value >= 100000000)
		{
			builder.Append(IToS(value / 100000000), "亿");
			value %= 100000000;
		}
		// 大于1万
		if(value >= 10000)
		{
			builder.Append(IToS(value / 10000), "万");
			value %= 10000;
		}
		if(value > 0)
		{
			builder.Append(value);
		}
		return FrameBase.END_STRING(builder);
	}
	// minLength表示返回字符串的最少数字个数,等于0表示不限制个数,大于0表示如果转换后的数字数量不足minLength个,则在前面补0
	public static string IToS(int value, int minLength = 0)
	{
		string retString;
		// 先尝试查表获取
		if(value >= 0 && value < mIntToString.Length)
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
	public static string IToSComma(uint value)
	{
		return IToSComma((int)value);
	}
	public static string LToS(ulong value, int minLength = 0)
	{
		return LToS((long)value, minLength);
	}
	public static string LToSComma(ulong value)
	{
		return LToSComma((long)value);
	}
	public static string LToS(long value, int minLength = 0)
	{
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
	public static string LToSComma(long value)
	{
		string retString = LToS(value);
		insertNumberComma(ref retString);
		return retString;
	}
	public static string vector2IntToString(Vector2Int value, int limitLength = 0)
	{
		return IToS(value.x, limitLength) + "," + IToS(value.y, limitLength);
	}
	public static void vector2IntToString(MyStringBuilder builder, Vector2Int value, int limitLength = 0)
	{
		builder.Append(IToS(value.x, limitLength), ",", IToS(value.y, limitLength));
	}
	public static string vector2ToString(Vector2 value, int precision = 4)
	{
		return FToS(value.x, precision) + "," + FToS(value.y, precision);
	}
	public static void vector2ToString(MyStringBuilder builder, Vector2 value, int precision = 4)
	{
		builder.Append(FToS(value.x, precision), ",", FToS(value.y, precision));
	}
	public static string vector3ToString(Vector3 value, int precision = 4)
	{
		return strcat(FToS(value.x, precision), ",", FToS(value.y, precision), ",", FToS(value.z, precision));
	}
	public static void vector3ToString(MyStringBuilder builder, Vector3 value, int precision = 4)
	{
		builder.Append(FToS(value.x, precision), ",", FToS(value.y, precision), ",", FToS(value.z, precision));
	}
	// 将str中的[begin,end)替换为reStr
	public static string replace(string str, int begin, int end, string reStr)
	{
		MyStringBuilder builder = FrameBase.STRING(str);
		replace(builder, begin, end, reStr);
		return FrameBase.END_STRING(builder);
	}
	public static void replace(MyStringBuilder str, int begin, int end, string reStr)
	{
		str.Remove(begin, end - begin);
		if(reStr.Length > 0)
		{
			str.Insert(begin, reStr);
		}
	}
	public static string replaceAll(string str, string key, string newWords)
	{
		MyStringBuilder builder = FrameBase.STRING(str);
		replaceAll(builder, key, newWords);
		return FrameBase.END_STRING(builder);
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
	public static string removeAll(string str, params string[] key)
	{
		MyStringBuilder builder = FrameBase.STRING(str);
		int keyCount = key.Length;
		for (int i = 0; i < keyCount; ++i)
		{
			replaceAll(builder, key[i], "");
		}
		return FrameBase.END_STRING(builder);
	}
	public static float SToF(string str)
	{
		checkFloatString(str, "-");
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
		return checkIntString(str, "." + valid);
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
			name = removeAll(name, "&");
			name = removeAll(name, "\\");
		}
		else
		{
			name = replaceAll(name, "&", "%26");
			name = replaceAll(name, "\\", "%5C%5C");
		}
		return name;
	}
	public static string bytesToHEXString(byte[] byteList, bool addSpace = true, bool upperOrLower = true, int count = 0)
	{
		MyStringBuilder builder = FrameBase.STRING();
		int byteCount = count > 0 ? count : byteList.Length;
		byteCount = MathUtility.getMin(byteList.Length, byteCount);
		for (int i = 0; i < byteCount; ++i)
		{
			if (addSpace)
			{
				byteToHEXString(builder, byteList[i], upperOrLower);
				if(i != byteCount - 1)
				{
					builder.Append(" ");
				}
			}
			else
			{
				byteToHEXString(builder, byteList[i], upperOrLower);
			}
		}
		return FrameBase.END_STRING(builder);
	}
	public static string byteToHEXString(byte value, bool upperOrLower = true)
	{
		MyStringBuilder builder = FrameBase.STRING();
		char[] hexChar = upperOrLower ? mHexUpperChar : mHexLowerChar;
		int high = value / 16;
		int low = value % 16;
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
		return FrameBase.END_STRING(builder);
	}
	public static void byteToHEXString(MyStringBuilder builder, byte value, bool upperOrLower = true)
	{
		char[] hexChar = upperOrLower ? mHexUpperChar : mHexLowerChar;
		int high = value / 16;
		int low = value % 16;
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
		FrameBase.ARRAY_THREAD(out bytes, MathUtility.getGreaterPow2(dataCount));
		for (int i = 0; i < dataCount; ++i)
		{
			bytes[i] = hexStringToByte(str, i * 2);
		}
		return dataCount;
	}
	public static void releaseHexStringBytes(byte[] bytes)
	{
		FrameBase.UN_ARRAY_THREAD(bytes);
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
		if(size < 1024 * 1024 * 1024)
		{
			return FToS(size * (1.0f / (1024.0f * 1024.0f)), 1) + "MB";
		}
		// 大于1GB
		return FToS(size * (1.0f / (1024.0f * 1024.0f * 1024.0f)), 1) + "GB";
	}
	// 在文本显示中将str的颜色设置为color
	public static string colorString(string color, string str)
	{
		if (isEmpty(str))
		{
			return EMPTY;
		}
		return FrameBase.END_STRING(FrameBase.STRING("<color=#", color, ">", str, "</color>"));
	}
	public static MyStringBuilder colorString(string color, MyStringBuilder str)
	{
		if (str.Length == 0)
		{
			return str;
		}
		str.InsertFront("<color=#", color, ">");
		str.Append("</color>");
		return str;
	}
	// returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
	public static int findFirstSubstr(string res, string pattern, int startPos = 0, bool returnEndIndex = false, bool sensitive = true)
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
		for (int i = startPos; i < len; ++i)
		{
			if (len - i < subLen)
			{
				continue;
			}
			int j;
			for (j = 0; j < subLen; ++j)
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
		if(returnEndIndex && posFind >= 0)
		{
			posFind += subLen;
		}
		return posFind;
	}
	// returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
	public static int findFirstSubstr(MyStringBuilder res, string pattern, int startPos = 0, bool returnEndIndex = false)
	{
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
	public static void appendValueString(ref string queryStr, string str)
	{
		queryStr += "\"" + str + "\",";
	}
	public static void appendValueString(MyStringBuilder queryStr, string str)
	{
		queryStr.Append("\"", str, "\",");
	}
	public static void appendValueVector2(ref string queryStr, Vector2 value)
	{
		queryStr += vector2ToString(value) + ",";
	}
	public static void appendValueVector2(MyStringBuilder queryStr, Vector2 value)
	{
		vector2ToString(queryStr, value);
		queryStr.Append(",");
	}
	public static void appendValueVector2Int(ref string queryStr, Vector2Int value)
	{
		queryStr += vector2IntToString(value) + ",";
	}
	public static void appendValueVector2Int(MyStringBuilder queryStr, Vector2Int value)
	{
		vector2IntToString(queryStr, value);
		queryStr.Append(",");
	}
	public static void appendValueVector3(ref string queryStr, Vector3 value)
	{
		queryStr += vector3ToString(value) + ",";
	}
	public static void appendValueVector3(MyStringBuilder queryStr, Vector3 value)
	{
		vector3ToString(queryStr, value);
		queryStr.Append(",");
	}
	public static void appendValueInt(ref string queryStr, int value)
	{
		queryStr += IToS(value) + ",";
	}
	public static void appendValueInt(MyStringBuilder queryStr, int value)
	{
		queryStr.Append(IToS(value), ",");
	}
	public static void appendValueUInt(ref string queryStr, uint value)
	{
		queryStr += IToS(value) + ",";
	}
	public static void appendValueUInt(MyStringBuilder queryStr, uint value)
	{
		queryStr.Append(IToS(value), ",");
	}
	public static void appendValueFloat(ref string queryStr, float value)
	{
		queryStr += FToS(value) + ",";
	}
	public static void appendValueFloat(MyStringBuilder queryStr, float value)
	{
		queryStr.Append(FToS(value), ",");
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
		condition.Append(col, "=\"", str, "\"", operate);
	}
	public static void appendConditionInt(ref string condition, string col, int value, string operate)
	{
		condition += col + " = " + IToS(value) + operate;
	}
	public static void appendConditionInt(MyStringBuilder condition, string col, int value, string operate)
	{
		condition.Append(col, " = ", IToS(value), operate);
	}
	public static void appendUpdateString(ref string updateStr, string col, string str)
	{
		updateStr += col + " = " + "\"" + str + "\",";
	}
	public static void appendUpdateString(MyStringBuilder updateStr, string col, string str)
	{
		updateStr.Append(col, " = \"", str, "\",");
	}
	public static void appendUpdateInt(ref string updateStr, string col, int value)
	{
		updateStr += col + " = " + IToS(value) + ",";
	}
	public static void appendUpdateInt(MyStringBuilder updateStr, string col, int value)
	{
		updateStr.Append(col, " = ", IToS(value), ",");
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
				i++;
				j++;
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
		return FrameBase.END_STRING_THREAD(FrameBase.STRING_THREAD(str0, str1, str2, str3, str4));
	}
	public static string strcat_thread(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		return FrameBase.END_STRING_THREAD(FrameBase.STRING_THREAD(str0, str1, str2, str3, str4, str5));
	}
	public static string strcat_thread(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		return FrameBase.END_STRING_THREAD(FrameBase.STRING_THREAD(str0, str1, str2, str3, str4, str5, str6));
	}
	public static string strcat_thread(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		return FrameBase.END_STRING_THREAD(FrameBase.STRING_THREAD(str0, str1, str2, str3, str4, str5, str6, str7));
	}
	public static string strcat_thread(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		return FrameBase.END_STRING_THREAD(FrameBase.STRING_THREAD(str0, str1, str2, str3, str4, str5, str6, str7, str8));
	}
	// 只能在主线程中使用的字符串拼接,当拼接小于等于4个字符串时,直接使用+号最快,GC与StringBuilder一致
	public static string strcat(string str0, string str1, string str2, string str3, string str4)
	{
		return FrameBase.END_STRING(FrameBase.STRING(str0, str1, str2, str3, str4));
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		return FrameBase.END_STRING(FrameBase.STRING(str0, str1, str2, str3, str4, str5));
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		return FrameBase.END_STRING(FrameBase.STRING(str0, str1, str2, str3, str4, str5, str6));
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		return FrameBase.END_STRING(FrameBase.STRING(str0, str1, str2, str3, str4, str5, str6, str7));
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		return FrameBase.END_STRING(FrameBase.STRING(str0, str1, str2, str3, str4, str5, str6, str7, str8));
	}
	public static string strcat(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		return FrameBase.END_STRING(FrameBase.STRING(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9));
	}
}