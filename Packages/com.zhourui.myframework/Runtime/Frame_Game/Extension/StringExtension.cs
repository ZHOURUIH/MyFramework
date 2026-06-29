using System.Text;

public static class StringExtension
{
	public static bool isEmpty(this string str) { return str == null || str.Length == 0; }
	// 截取开始一定长度的字符串
	public static string startString(this string str, int startLength)
	{
		if (str == null || startLength < 0)
		{
			return str;
		}
		return str[0..startLength];
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
	// 截取最后一定长度的字符串
	public static string endString(this string str, int endLength)
	{
		if (str == null || endLength > str.Length)
		{
			return str;
		}
		return str[(str.Length - endLength)..];
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
	public static string removeAll(this string str, char key)
	{
		StringBuilder builder = new();
		builder.Append(str);
		for (int i = builder.Length - 1; i >= 0; --i)
		{
			// 判断是否是需要移除的字符
			if (key == builder[i])
			{
				builder.Remove(i, 1);
			}
		}
		return builder.ToString();
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
	public static string addEndSlash(this string path)
	{
		if (!path.isEmpty() && path[^1] != '/')
		{
			path += "/";
		}
		return path;
	}
	public static string rightToLeft(this string str)
	{
		if (str == null)
		{
			return str;
		}
		return str.Replace('\\', '/');
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
}