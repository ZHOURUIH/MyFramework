using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static MathUtility;

public class StringUtility : BinaryUtility
{
	private static List<int> mTempIntList = new();
	private static List<float> mTempFloatList = new();
	private static List<string> mTempStringList = new();
	private static string[] mFloatConvertPercision = new string[] { "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7" };
	private static char[] mHexUpperChar = new char[] { 'A', 'B', 'C', 'D', 'E', 'F' };  // 十六进制中的大写字母
	private static char[] mHexLowerChar = new char[] { 'a', 'b', 'c', 'd', 'e', 'f' };  // 十六进制中的小写字母
	public const string EMPTY = "";
	// 使用Length == 0判断是否为空字符串是最快的
	public static bool isEmpty(string str)
	{
		return str == null || str.Length == 0;
	}
	public static bool isUpper(char value) { return value >= 'A' && value <= 'Z'; }
	public static bool isLower(char value) { return value >= 'a' && value <= 'z'; }
	public static bool isNumber(char value) { return value >= '0' && value <= '9'; }
	public static string toLower(string str)
	{
		byte[] bytes = stringToBytes(str);
		int size = bytes.Length;
		for(int i = 0; i < size; ++i)
		{
			if (isUpper((char)bytes[i]))
			{
				bytes[i] += 'a' - 'A';
			}
		}
		return bytesToString(bytes);
	}
	public static string toUpper(string str)
	{
		byte[] bytes = stringToBytes(str);
		int size = bytes.Length;
		for (int i = 0; i < size; ++i)
		{
			if (isLower((char)bytes[i]))
			{
				bytes[i] -= 'a' - 'A';
			}
		}
		return bytesToString(bytes);
	}
	public static string removeAll(string str, params string[] key)
	{
		int keyCount = key.Length;
		for (int i = 0; i < keyCount; ++i)
		{
			str = replaceAll(str, key[i], "");
		}
		return str;
	}
	public static string replaceAll(string str, string key, string newWords)
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
	public static string replace(string str, int begin, int end, string reStr)
	{
		str = str.Remove(begin, end - begin);
		if (reStr.Length > 0)
		{
			str = str.Insert(begin, reStr);
		}
		return str;
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
	public static Vector2 SToVector2(string value, string seperate = ",")
	{
		if (isEmpty(value) || value == "0,0")
		{
			return Vector2.Zero;
		}
		string[] splitList = split(value, true, seperate);
		if (splitList == null || splitList.Length < 2)
		{
			return Vector2.Zero;
		}
		Vector2 v = new();
		v.X = SToF(splitList[0]);
		v.Y = SToF(splitList[1]);
		return v;
	}
	public static Vector2Int SToVector2Int(string value, string seperate = ",")
	{
		if (isEmpty(value) || value == "0,0")
		{
			return Vector2Int.zero;
		}
		string[] splitList = split(value, true, seperate);
		if (splitList == null || splitList.Length < 2)
		{
			return Vector2Int.zero;
		}
		Vector2Int v = new();
		v.x = SToI(splitList[0]);
		v.y = SToI(splitList[1]);
		return v;
	}
	public static Vector3 SToVector3(string value, string seperate = ",")
	{
		if (isEmpty(value) || value == "0,0,0")
		{
			return Vector3.Zero;
		}
		string[] splitList = split(value, true, seperate);
		if (splitList == null || splitList.Length < 3)
		{
			return Vector3.Zero;
		}
		Vector3 v = new();
		v.X = SToF(splitList[0]);
		v.Y = SToF(splitList[1]);
		v.Z = SToF(splitList[2]);
		return v;
	}
	public static Vector3Int SToVector3Int(string value, string seperate = ",")
	{
		if (isEmpty(value) || value == "0,0,0")
		{
			return Vector3Int.zero;
		}
		string[] splitList = split(value, true, seperate);
		if (splitList == null || splitList.Length < 3)
		{
			return Vector3Int.zero;
		}
		Vector3Int v = new();
		v.x = SToI(splitList[0]);
		v.y = SToI(splitList[1]);
		v.z = SToI(splitList[2]);
		return v;
	}
	public static Vector4 SToVector4(string value, string seperate = ",")
	{
		if (isEmpty(value) || value == "0,0,0,0")
		{
			return Vector4.Zero;
		}
		string[] splitList = split(value, true, seperate);
		if (splitList == null || splitList.Length < 4)
		{
			return Vector4.Zero;
		}
		Vector4 v = new();
		v.X = SToF(splitList[0]);
		v.Y = SToF(splitList[1]);
		v.Z = SToF(splitList[2]);
		v.W = SToF(splitList[3]);
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
	// 去掉整个stream中最后一个出现过的逗号
	public static void removeLastComma(ref string stream)
	{
		removeLast(ref stream, ',');
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
		str = rightToLeft(str);
		// 如果有文件名,则先去除文件名
		int namePos = str.LastIndexOf('/');
		int dotPos = str.LastIndexOf('.');
		if (dotPos > namePos)
		{
			str = str.Remove(namePos);
		}
		// 再去除当前目录的父级目录
		namePos = str.LastIndexOf('/');
		if (namePos != -1)
		{
			str = str.Remove(0, namePos + 1);
		}
		return str;
	}
	// 得到文件路径
	public static string getFilePath(string fileName)
	{
		fileName = rightToLeft(fileName);
		int lastPos = fileName.LastIndexOf('/', fileName.Length - 2);
		if (lastPos != -1)
		{
			return fileName.Remove(lastPos);
		}
		return EMPTY;
	}
	public static string getFileName(string str)
	{
		str = rightToLeft(str);
		int dotPos = str.LastIndexOf('/');
		if (dotPos != -1)
		{
			str = str.Remove(0, dotPos + 1);
		}
		return str;
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
		str = rightToLeft(str);
		// 先判断是否移除目录
		if (removeDir)
		{
			int namePos = str.LastIndexOf('/');
			if(namePos != -1)
			{
				str = str.Remove(0, namePos + 1);
			}
		}
		// 移除后缀
		int dotPos = str.LastIndexOf('.');
		if (dotPos != -1)
		{
			str = str.Remove(dotPos);
		}
		return str;
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
	public static void rightToLeft(ref string str)
	{
		str = str.Replace('\\', '/');
	}
	public static string rightToLeft(string str)
	{
		return str.Replace('\\', '/');
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
	public static void splitLine(string str, out string[] lines, bool removeEmpty = true)
	{
		if (isEmpty(str))
		{
			lines = null;
			return;
		}
		if (str.IndexOf('\n') >= 0)
		{
			lines = split(str, removeEmpty, "\n");
			if (lines == null)
			{
				return;
			}
			for (int i = 0; i < lines.Length; ++i)
			{
				lines[i] = removeAll(lines[i], "\r");
			}
		}
		else if (str.IndexOf('\r') >= 0)
		{
			lines = split(str, removeEmpty, "\r");
			if (lines == null)
			{
				return;
			}
			for (int i = 0; i < lines.Length; ++i)
			{
				lines[i] = removeAll(lines[i], "\n");
			}
		}
		else
		{
			lines = new string[1] { str };
		}
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
		List<float> values = new();
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
			return;
		}
		values ??= new float[len];
		for (int i = 0; i < len; ++i)
		{
			values[i] = SToF(rangeList[i]);
		}
	}
	public static void stringToFloats(string str, List<float> values, string seperate = ",")
	{
		if (values == null)
		{
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
		string str = "";
		int count = values.Length;
		for (int i = 0; i < count; ++i)
		{
			str += FToS(values[i], 2);
			if (i != count - 1)
			{
				str += seperate;
			}
		}
		return str;
	}
	public static string floatsToString(List<float> values, string seperate = ",")
	{
		string str = "";
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			str += FToS(values[i], 2);
			if (i != count - 1)
			{
				str += seperate;
			}
		}
		return str;
	}
	public static List<Vector2> stringToVector2s(string str, string seperate = ",")
	{
		List<Vector2> values = new();
		stringToVector2s(str, values, seperate);
		return values;
	}
	public static void stringToVector2s(string str, List<Vector2> values, string seperate = ",")
	{
		if (values == null)
		{
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
			values.Add(SToVector2(rangeList[i]));
		}
	}
	public static List<Vector2Int> stringToVector2Ints(string str, string seperate = ",")
	{
		List<Vector2Int> values = new();
		stringToVector2Ints(str, values, seperate);
		return values;
	}
	public static void stringToVector2Ints(string str, List<Vector2Int> values, string seperate = ",")
	{
		if (values == null)
		{
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
			values.Add(SToVector2Int(rangeList[i]));
		}
	}
	public static List<Vector3> stringToVector3s(string str, string seperate = ",")
	{
		List<Vector3> values = new();
		stringToVector3s(str, values, seperate);
		return values;
	}
	public static void stringToVector3s(string str, List<Vector3> values, string seperate = ",")
	{
		if (values == null)
		{
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
			values.Add(SToVector3(rangeList[i]));
		}
	}
	public static List<Vector3Int> stringToVector3Ints(string str, string seperate = ",")
	{
		List<Vector3Int> values = new();
		stringToVector3Ints(str, values, seperate);
		return values;
	}
	public static void stringToVector3Ints(string str, List<Vector3Int> values, string seperate = ",")
	{
		if (values == null)
		{
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
			values.Add(SToVector3Int(rangeList[i]));
		}
	}
	public static List<int> stringToInts(string str, string seperate = ",")
	{
		List<int> values = new();
		stringToInts(str, values, seperate);
		return values;
	}
	public static void stringToInts(string str, List<int> values, string seperate = ",")
	{
		if (values == null)
		{
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
			return;
		}
		values ??= new int[len];
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
		string str = "";
		int count = values.Length;
		for (int i = 0; i < count; ++i)
		{
			str += IToS(values[i]);
			if (i != count - 1)
			{
				str += seperate;
			}
		}
		return str;
	}
	public static string intsToString(List<int> values, string seperate = ",")
	{
		string str = "";
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			str += IToS(values[i]);
			if (i != count - 1)
			{
				str += seperate;
			}
		}
		return str;
	}
	public static string stringsToString(List<string> values, string seperate = ",")
	{
		string str = "";
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			str += values[i];
			if (i != count - 1)
			{
				str += seperate;
			}
		}
		return str;
	}
	public static string stringsToString(string[] values, string seperate = ",")
	{
		string str = "";
		int count = values.Length;
		for (int i = 0; i < count; ++i)
		{
			str += values[i];
			if (i != count - 1)
			{
				str += seperate;
			}
		}
		return str;
	}
	public static List<string> stringToStrings(string str, string seperate = ",")
	{
		string[] strArray = split(str, true, seperate);
		if (strArray != null)
		{
			return new(strArray);
		}
		return new();
	}
	public static void stringToStrings(string str, List<string> values, string seperate = ",")
	{
		if (values == null)
		{
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
		if(precision == 0)
		{
			return IToS((int)value);
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
		// 从后往前插入
		for (int i = 0; i < commaCount; ++i)
		{
			str = str.Insert(insertStart, ",");
			insertStart -= 3;
		}
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
		string retString = value.ToString();
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
		string retString = value.ToString();
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
		string retString = value.ToString();
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
	// 移除str的从开头到stopChar的部分,包括stopChar,findFromStart为true表示寻找第一个stopChar,false表示寻找最后一个stopChar
	public static string removeStart(string str, char stopChar, bool findFromStart)
	{
		int pos = findFromStart ? str.IndexOf(stopChar) : str.LastIndexOf(stopChar);
		if(pos < 0)
		{
			return str;
		}
		return str.Substring(pos + 1);
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
		int oldStrLen = str.Length;
		for (int i = 0; i < oldStrLen; ++i)
		{
			if (valid.IndexOf(str[i]) < 0)
			{
				return false;
			}
		}
		return true;
	}
	public static bool checkFloatString(string str, string valid = EMPTY)
	{
		return checkIntString(str, "." + valid);
	}
	public static bool checkIntString(string str, string valid = EMPTY)
	{
		return checkString(str, "-0123456789" + valid);
	}
	public static bool checkUIntString(string str, string valid = EMPTY)
	{
		return checkString(str, "0123456789" + valid);
	}
	public static int getCharCount(string str, char key)
	{
		int count = 0;
		int length = str.Length;
		for (int i = 0; i < length; ++i)
		{
			if (str[i] == key)
			{
				++count;
			}
		}
		return count;
	}
	public static int getStringWidth(string str)
	{
		return str.Length + getCharCount(str, '\t') * 3;
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
	public static string colorStringNoBuilder(string color, string str)
	{
		if (isEmpty(str))
		{
			return EMPTY;
		}
		return "<color=#" + color + ">" + str + "</color>";
	}
	// returnEndIndex表示返回值是否是字符串结束的下一个字符的下标
	public static int findFirstSubstr(string res, string pattern, int startPos = 0, bool returnEndIndex = false, bool sensitive = true)
	{
		if(res == null)
		{
			return -1;
		}
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
	public static void byteToHEXString(StringBuilder builder, byte value, bool upperOrLower = true)
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
	public static string bytesToHEXString(byte[] byteList, int offset = 0, int count = 0, bool addSpace = true, bool upperOrLower = true)
	{
		StringBuilder builder = new();
		int byteCount = count > 0 ? count : byteList.Length - offset;
		clamp(ref byteCount, 0, byteList.Length - offset);
		for (int i = 0; i < byteCount; ++i)
		{
			if (addSpace)
			{
				byteToHEXString(builder, byteList[i + offset], upperOrLower);
				if (i != byteCount - 1)
				{
					builder.Append(' ');
				}
			}
			else
			{
				byteToHEXString(builder, byteList[i + offset], upperOrLower);
			}
		}
		return builder.ToString();
	}
}