using System;
using System.Collections.Generic;
using static System.Linq.Enumerable;

public class EmptyArray<T>
{
	public static T[] mList;
	public static T[] getEmptyList()
	{
		mList ??= new T[0];
		return mList;
	}
}

public static class ArrayExtension
{
	public static void setAllDefault<T>(this T[] list)
	{
		if (list.isEmpty())
		{
			return;
		}
		for (int i = 0; i < list.Length; ++i)
		{
			list[i] = default;
		}
	}
	public static void setAllValue<T>(this T[] list, T value)
	{
		if (list.isEmpty())
		{
			return;
		}
		for (int i = 0; i < list.Length; ++i)
		{
			list[i] = value;
		}
	}
	public static T[] setRange<T>(this T[] list, List<T> other)
	{
		if (other.isEmpty())
		{
			return list;
		}
		int index = 0;
		foreach (T item in other)
		{
			list[index++] = item;
		}
		return list;
	}
	public static T[] setRange<T>(this T[] list, Span<T> other)
	{
		if (other.isEmpty())
		{
			return list;
		}
		for (int i = 0; i < other.Length; ++i)
		{
			list[i] = other[i];
		}
		return list;
	}
	public static void ForI<T>(this T[] list, Action<int> action)
	{
		if (list.isEmpty())
		{
			return;
		}
		for (int i = 0; i < list.Length; ++i)
		{
			action(i);
		}
	}
	public static void For<T>(this T[] list, Action<T> action)
	{
		if (list.isEmpty())
		{
			return;
		}
		foreach (T item in list)
		{
			action(item);
		}
	}
	public static bool find<T>(this T[] list, T value, out int index)
	{
		if (list.isEmpty())
		{
			index = -1;
			return false;
		}
		for (int i = 0; i < list.Length; ++i)
		{
			if (list[i].Equals(value))
			{
				index = i;
				return true;
			}
		}
		index = -1;
		return false;
	}
	public static int find<T>(this T[] list, T value)
	{
		if (list.isEmpty())
		{
			return -1;
		}
		for (int i = 0; i < list.Length; ++i)
		{
			if (list[i].Equals(value))
			{
				return i;
			}
		}
		return -1;
	}
	public static T find<T>(this T[] list, Predicate<T> match)
	{
		if (list.isEmpty() || match == null)
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
	public static bool find<T>(this T[] list, Predicate<T> match, out T value)
	{
		if (list.isEmpty() || match == null)
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
	public static bool find<T>(this T[] list, Predicate<T> match, out int index)
	{
		if (list.isEmpty() || match == null)
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
	public static int count<T>(this T[] list, Predicate<T> condition)
	{
		if (list.isEmpty() || condition == null)
		{
			return 0;
		}
		int curCount = 0;
		foreach (T item in list)
		{
			if (condition.Invoke(item))
			{
				++curCount;
			}
		}
		return curCount;
	}
	public static T get<T>(this T[] list, int index) 
	{
		if (list.isEmpty() || index < 0 || index >= list.Length)
		{
			return default;
		}
		return list[index];
	}
	public static int count<T>(this T[] list)										{ return list?.Length ?? 0; }
	public static bool isEmpty<T>(this T[] list)									{ return list == null || list.Length == 0; }
	public static bool contains<T>(this T[] list, T value)							{ return list != null && list.Contains(value); }
	public static bool contains<T>(this T[] list, Predicate<T> match)				{ return list != null && list.find(match) != null; }
	public static T[] safe<T>(this T[] original)									{ return original ?? EmptyArray<T>.getEmptyList(); }
	public static T first<T>(this T[] list)
	{
		if (list.isEmpty())
		{
			return default;
		}
		return list[0];
	}
	public static void inverse<T>(this T[] list)
	{
		if (list.count() <= 1)
		{
			return;
		}
		int count = list.Length;
		for (int i = 0; i < count >> 1; ++i)
		{
			(list[i], list[count - 1 - i]) = (list[count - 1 - i], list[i]);
		}
	}
}