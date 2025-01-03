
public static class StringExtension
{
	public static int length(this string list) { return list?.Length ?? 0; }
	public static bool isEmpty(this string str) { return str == null || str.Length == 0; }
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
	// 截取从startIndex到第一个key字符之前的字符串,不包含key
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
		if (str == null || removeCount <= 0)
		{
			return str;
		}
		return str[removeCount..];
	}
}