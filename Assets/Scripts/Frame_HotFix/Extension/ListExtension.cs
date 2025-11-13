using System;
using System.Collections.Generic;
using static System.Linq.Enumerable;
using static FrameUtility;

public static class ListExtension
{
	public static bool removeIf<T>(this List<T> list, T value, bool condition)
	{
		if (!condition)
		{
			return false;
		}
		return list.Remove(value);
	}
	public static T removeAtIf<T>(this List<T> list, int index, bool condition)
	{
		if (!condition)
		{
			return default;
		}
		T value = list[index];
		list.RemoveAt(index);
		return value;
	}
	public static T removeAt<T>(this List<T> list, int index)
	{
		T value = list[index];
		list.RemoveAt(index);
		return value;
	}
	public static void addCount<T>(this List<T> list, int count)
	{
		if (count < 0)
		{
			return;
		}
		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		for (int i = 0; i < count; ++i)
		{
			list.Add(default);
		}
	}
	public static void addCount<T>(this List<T> list, T value, int count)
	{
		if (count < 0)
		{
			return;
		}
		if (list.Capacity < count)
		{
			list.Capacity = count;
		}
		for (int i = 0; i < count; ++i)
		{
			list.Add(value);
		}
	}
	public static bool addNotNull<T>(this List<T> list, T value) where T : class
	{
		if (value != null)
		{
			list.Add(value);
			return true;
		}
		return false;
	}
	public static bool addIf<T>(this IList<T> list, T value, bool condition)
	{
		if (condition)
		{
			list.Add(value);
		}
		return condition;
	}
	public static bool addNotEmpty(this IList<string> list, string value)
	{
		if (!value.isEmpty())
		{
			list.Add(value);
			return true;
		}
		return false;
	}
	public static bool addNot<T>(this IList<T> list, T value, T notValue)
	{
		if (value.Equals(notValue))
		{
			return false;
		}
		list.Add(value);
		return true;
	}
	public static T addClass<T>(this List<T> list) where T : ClassObject, new()
	{
		return list.add(CLASS<T>());
	}
	public static bool addUnique<T>(this List<T> list, T value)
	{
		if (!list.Contains(value))
		{
			list.Add(value);
			return true;
		}
		return false;
	}
	public static List<T> addRange<T>(this List<T> list, IEnumerable<T> other)
	{
		if (list == null || other == null || other.Count() == 0)
		{
			return list;
		}
		list.Capacity = list.Count + other.Count();
		list.AddRange(other);
		return list;
	}
	// T0 必须是 T1 的基类或者实现 T1 的接口
	public static List<Base> addRangeDerived<Base, T>(this List<Base> list, IEnumerable<T> other) where Base : class where T : Base
	{
		if (other == null || other.Count() == 0)
		{
			return list;
		}
		list.Capacity = list.Count + other.Count();
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	// 由于params T[]类型匹配IEnumerable<T>和Span<T>是有二义性,所以改为不同的函数名
	public static List<T> addRangeSpan<T>(this List<T> list, Span<T> other)
	{
		list.Capacity = list.Count + other.Length;
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	public static List<T> addRangeSpan<T>(this List<T> list, Span<T> other, int count)
	{
		list.Capacity = list.Count + count;
		for (int i = 0; i < count; ++i)
		{
			list.Add(other[i]);
		}
		return list;
	}
	public static List<T> setRange<T>(this List<T> list, IEnumerable<T> other)
	{
		list.Clear();
		if (other == null || other.Count() == 0)
		{
			return list;
		}
		list.AddRange(other);
		return list;
	}
	public static List<Base> setRangeDerived<Base, T>(this List<Base> list, IEnumerable<T> other) where Base : class where T : Base
	{
		list.Clear();
		if (other == null || other.Count() == 0)
		{
			return list;
		}
		list.Capacity = other.Count();
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	public static List<T> setRangeSpan<T>(this List<T> list, Span<T> other)
	{
		list.Clear();
		list.Capacity = other.Length;
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	public static List<T> setRangeSpan<T>(this List<T> list, Span<T> other, int count)
	{
		list.Clear();
		list.Capacity = other.Length;
		for (int i = 0; i < count; ++i)
		{
			list.Add(other[i]);
		}
		return list;
	}
	public static T get<T>(this IList<T> list, int index)
	{
		if (list == null || index < 0 || index >= list.Count)
		{
			return default;
		}
		return list[index];
	}
	public static T add<T>(this IList<T> list, T value)
	{
		list.Add(value);
		return value;
	}
	public static void add<T>(this IList<T> list, T value0, T value1)
	{
		list.Add(value0);
		list.Add(value1);
	}
	public static void add<T>(this IList<T> list, T value0, T value1, T value2)
	{
		list.Add(value0);
		list.Add(value1);
		list.Add(value2);
	}
	public static void add<T>(this IList<T> list, T value0, T value1, T value2, T value3)
	{
		list.Add(value0);
		list.Add(value1);
		list.Add(value2);
		list.Add(value3);
	}
	public static void add<T>(this IList<T> list, T value0, T value1, T value2, T value3, T value4)
	{
		list.Add(value0);
		list.Add(value1);
		list.Add(value2);
		list.Add(value3);
		list.Add(value4);
	}
	// 将sourceList中的所有元素添加到list中,并清空sourceList
	public static List<T> move<T>(this List<T> list, IList<T> sourceList)
	{
		list.AddRange(sourceList);
		sourceList.Clear();
		return list;
	}
	public static T popBack<T>(this List<T> list)
	{
		if (list == null || list.Count == 0)
		{
			return default;
		}
		return list.removeAt(list.Count - 1);
	}
	public static T getLast<T>(this List<T> list)
	{
		int count = list.count();
		if (count == 0)
		{
			return default;
		}
		return list[count - 1];
	}
	public static bool isSubList<T>(this List<T> list, IList<T> subList)
	{
		if (list.isEmpty() || subList.isEmpty())
		{
			return false;
		}
		if (list.Count < subList.Count)
		{
			return false;
		}
		for (int i = 0; i < list.Count; ++i)
		{
			if (list.Count - i < subList.Count)
			{
				break;
			}
			int j = 0;
			for (; j < subList.Count; ++j)
			{
				if (!list[i + j].Equals(subList[j]))
				{
					break;
				}
			}
			if (j == subList.Count)
			{
				return true;
			}
		}
		return false;
	}
	public static bool findIndex<T>(this List<T> list, Predicate<T> match, out int index)
	{
		index = list.FindIndex(0, list.Count, match);
		return index >= 0;
	}
	public static bool findIndex<T>(this List<T> list, Predicate<T> match, out int index, out T item)
	{
		index = list.FindIndex(0, list.Count, match);
		if (index >= 0)
		{
			item = list[index];
		}
		else
		{
			item = default;
		}
		return index >= 0;
	}
	public static bool findIndex<T>(this List<T> list, int startIndex, Predicate<T> match, out int index)
	{
		index = list.FindIndex(startIndex, list.Count - startIndex, match);
		return index >= 0;
	}
	public static bool findIndex<T>(this List<T> list, int startIndex, int count, Predicate<T> match, out int index)
	{
		index = list.FindIndex(startIndex, count, match);
		return index >= 0;
	}
	public static void swap<T>(this List<T> list, int index0, int index1)
	{
		(list[index0], list[index1]) = (list[index1], list[index0]);
	}
	public static int count<T>(this ICollection<T> list)							{ return list?.Count ?? 0; }
	public static bool isEmptySpan<T>(this Span<T> list)							{ return list == null || list.Length == 0; }
	public static bool isEmpty<T>(this ICollection<T> list)							{ return list == null || list.Count == 0; }
	public static IEnumerable<T> safe<T>(this IEnumerable<T> original)				{ return original ?? Empty<T>(); }
}