using System;
using System.Text;

// Span扩展方法,提供对Span的便捷操作
public static class SpanExtension
{
	public static void ForI<T>(this Span<T> list, Action<int> action)
	{
		for (int i = 0; i < list.Length; ++i)
		{
			action(i);
		}
	}
	public static T find<T>(this Span<T> list, Predicate<T> match)
	{
		if (match == null)
		{
			return default;
		}
		foreach (T item in list)
		{
			if (match(item))
			{
				return item;
			}
		}
		return default;
	}
	public static bool find<T>(this Span<T> list, Predicate<T> match, out T value)
	{
		if (match == null)
		{
			value = default;
			return false;
		}
		foreach (T item in list)
		{
			if (match(item))
			{
				value = item;
				return true;
			}
		}
		value = default;
		return false;
	}
	public static bool find<T>(this Span<T> list, Predicate<T> match, out int index)
	{
		if (match == null)
		{
			index = -1;
			return false;
		}
		for (int i = 0; i < list.Length; ++i)
		{
			if (match(list[i]))
			{
				index = i;
				return true;
			}
		}
		index = -1;
		return false;
	}
	public static bool find<T>(this Span<T> list, Predicate<T> match, out int index, out T item)
	{
		if (match == null)
		{
			index = -1;
			item = default;
			return false;
		}
		for (int i = 0; i < list.Length; ++i)
		{
			if (match(list[i]))
			{
				index = i;
				item = list[i];
				return true;
			}
		}
		index = -1;
		item = default;
		return false;
	}
	public static bool find<T>(this Span<T> list, int startIndex, Predicate<T> match, out int index)
	{
		if (match == null)
		{
			index = -1;
			return false;
		}
		for (int i = startIndex; i < list.Length; ++i)
		{
			if (match(list[i]))
			{
				index = i;
				return true;
			}
		}
		index = -1;
		return false;
	}
	public static bool find<T>(this Span<T> list, int startIndex, int count, Predicate<T> match, out int index)
	{
		if (match == null)
		{
			index = -1;
			return false;
		}
		count = count.clampMax(list.Length);
		for (int i = 0; i < count; ++i)
		{
			if (match(list[i + startIndex]))
			{
				index = i + startIndex;
				return true;
			}
		}
		index = -1;
		return false;
	}
	public static bool isEmpty<T>(this Span<T> list)								{ return list == null || list.Length == 0; }
	public static bool contains<T>(this Span<T> list, Predicate<T> match)			{ return list != null && list.find(match) != null; }
	// 字节Span转字符串，默认UTF8编码，自动去除末尾的'\0'字符
	// bytes为null时返回null，长度为0时返回空字符串
	public static string bytesToString(this Span<byte> bytes, Encoding encoding = null)
	{
		if (bytes == null)
		{
			return null;
		}
		if (bytes.Length == 0)
		{
			return string.Empty;
		}
		// 默认为UTF8
		return (encoding ?? Encoding.UTF8).GetString(bytes).removeLastZero();
	}
	public static T random<T>(this Span<T> list)
	{
		if (list.isEmpty())
		{
			return default;
		}
		return list[UnityEngine.Random.Range(0, list.Length)];
	}
}