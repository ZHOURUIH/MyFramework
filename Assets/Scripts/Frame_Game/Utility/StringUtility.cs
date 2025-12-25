using System;
using System.Collections.Generic;
using System.Text;

// 字符串相关工具函数类
public class StringUtility
{
	private static char[] mHexLowerChar = new char[] { 'a', 'b', 'c', 'd', 'e', 'f' };   // 十六进制中的小写字母
	private static string[] mFloatConvertPrecision = new string[] { "f0", "f1", "f2", "f3", "f4", "f5", "f6", "f7" };   // 浮点数转换时精度
	public static string toPercent(float value, int precision = 1) { return FToS(value * 100, precision); }
	// precision表示小数点后保留几位小数,removeTailZero表示是否去除末尾的0
	public static string FToS(float value, int precision = 4)
	{
		if (precision == 0)
		{
			return ((int)value).ToString();
		}
		return value.ToString(mFloatConvertPrecision[precision]);
	}
	// 得到文件路径
	public static string getFilePath(string fileName, bool keepEndSlash = false)
	{
		fileName = fileName.Replace('\\', '/');
		// 从倒数第二个开始,因为即使最后一个是/也需要忽略
		int lastPos = fileName.LastIndexOf('/', fileName.Length - 2);
		if (lastPos < 0)
		{
			return "";
		}
		return fileName[..(lastPos + (keepEndSlash ? 1 : 0))];
	}
	// 移除文件的后缀名
	public static string removeSuffix(string str)
	{
		if (str == null)
		{
			return null;
		}
		str = str.Replace('\\', '/');
		// 移除后缀
		int dotPos = str.LastIndexOf('.');
		if (dotPos != -1)
		{
			str = str[..dotPos];
		}
		return str;
	}
	// 获取不带后缀的文件名
	public static string getFileNameNoSuffixNoDir(string str)
	{
		if (str == null)
		{
			return null;
		}
		str = str.Replace('\\', '/');
		int namePos = str.LastIndexOf('/');
		if (namePos != -1)
		{
			str = str.Remove(0, namePos + 1);
		}
		// 移除后缀
		int dotPos = str.LastIndexOf('.');
		if (dotPos != -1)
		{
			str = str[..dotPos];
		}
		return str;
	}
	public static string bytesToHEXString(byte[] byteList)
	{
		StringBuilder builder = new();
		for (int i = 0; i < byteList.Length; ++i)
		{
			byteToHEXString(builder, byteList[i]);
		}
		return builder.ToString();
	}
	public static string[] splitLine(string str, bool removeEmpty = true)
	{
		splitLine(str, out string[] lines, removeEmpty);
		return lines;
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
	public static string[] split(string str, bool removeEmpty, params char[] keyword)
	{
		if (str.isEmpty())
		{
			return null;
		}
		return str.Split(keyword, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
	}
	public static string[] split(string str, bool removeEmpty, params string[] keyword)
	{
		if (str.isEmpty())
		{
			return null;
		}
		return str.Split(keyword, removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
	}
	public static string stringsToString(IList<string> values, char saperate = ',')
	{
		if (values.isEmpty())
		{
			return "";
		}
		StringBuilder builder = new();
		int count = values.Count;
		for (int i = 0; i < count; ++i)
		{
			builder.Append(values[i]);
			if (i != count - 1)
			{
				builder.Append(saperate);
			}
		}
		return builder.ToString();
	}
	// 获取文件名,带后缀
	public static string getFileNameWithSuffix(string str)
	{
		str = str.rightToLeft();
		int dotPos = str.LastIndexOf('/');
		if (dotPos != -1)
		{
			str = str.Remove(0, dotPos + 1);
		}
		return str;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static void byteToHEXString(StringBuilder builder, byte value)
	{
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
			builder.Append(mHexLowerChar[high - 10]);
		}
		if (low < 10)
		{
			builder.Append((char)('0' + low));
		}
		else
		{
			builder.Append(mHexLowerChar[low - 10]);
		}
	}
}