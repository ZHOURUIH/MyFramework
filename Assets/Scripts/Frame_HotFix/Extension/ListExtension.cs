using System;
using System.Collections.Generic;
using static System.Linq.Enumerable;
using static FrameUtility;
using static MathUtility;

public static class ListExtension
{
	public static T random<T>(this List<T> list)
	{
		if (list.isEmpty())
		{
			return default;
		}
		return list[randomInt(0, list.Count - 1)];
	}
	public static void setAllValue<T>(this IList<T> list, T value)
	{
		if (list.isEmpty())
		{
			return;
		}
		for (int i = 0; i < list.Count; ++i)
		{
			list[i] = value;
		}
	}
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
	public static void remove<T>(this List<T> list, IList<T> removeValues)
	{
		if (removeValues.isEmpty())
		{
			return;
		}
		removeValues.For(item => list.Remove(item));
	}
	public static bool remove<T>(this List<T> list, Predicate<T> condition)
	{
		if (list.isEmpty())
		{
			return false;
		}
		for (int i  = 0; i < list.Count; ++i)
		{
			if (condition(list[i]))
			{
				list.RemoveAt(i);
				return true;
			}
		}
		return false;
	}
	public static T swapToEndAndRemove<T>(this List<T> list, int index)
	{
		list.swap(index, list.Count - 1);
		return list.removeAt(list.Count - 1);
	}
	public static void addCount<T>(this List<T> list, int count)
	{
		if (count < 0)
		{
			return;
		}
		if (list.Capacity < list.Count + count)
		{
			list.Capacity = list.Count + count;
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
		if (list.Capacity < list.Count + count)
		{
			list.Capacity = list.Count + count;
		}
		for (int i = 0; i < count; ++i)
		{
			list.Add(value);
		}
	}
	public static void addRangeKeys<TKey, TValue>(this List<TKey> list, IDictionary<TKey, TValue> dic)
	{
		if (dic.isEmpty())
		{
			return;
		}
		foreach (var item in dic)
		{
			list.add(item.Key);
		}
	}
	public static void addRangeValues<TKey, TValue>(this List<TValue> list, IDictionary<TKey, TValue> dic)
	{
		if (dic.isEmpty())
		{
			return;
		}
		foreach (var item in dic)
		{
			list.add(item.Value);
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
	public static void addRangeNotNull<T>(this List<T> list, IList<T> values) where T : class
	{
		foreach (T value in values)
		{
			if (value != null)
			{
				list.Add(value);
			}
		}
	}
	public static bool addIf<T>(this List<T> list, T value, bool condition)
	{
		if (condition)
		{
			list.Add(value);
		}
		return condition;
	}
	public static bool addNotEmpty(this List<string> list, string value)
	{
		if (!value.isEmpty())
		{
			list.Add(value);
			return true;
		}
		return false;
	}
	public static bool addNot<T>(this List<T> list, T value, T notValue)
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
	public static T addNew<T>(this List<T> list) where T : new()
	{
		return list.add(new());
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
	public static bool addUniqueIf<T>(this List<T> list, T value, bool condition)
	{
		if (!condition)
		{
			return false;
		}
		if (!list.Contains(value))
		{
			list.Add(value);
			return true;
		}
		return false;
	}
	public static void addUniqueOrRemove<T>(this List<T> list, T value, bool addOrRemove)
	{
		if (addOrRemove)
		{
			list.addUnique(value);
		}
		else
		{
			list.Remove(value);
		}
	}
	public static bool addUniqueNot<T>(this List<T> list, T value, T notValue)
	{
		if (value.Equals(notValue))
		{
			return false;
		}
		if (!list.Contains(value))
		{
			list.Add(value);
			return true;
		}
		return false;
	}
	public static List<T> addRange<T>(this List<T> list, IList<T> other, int count)
	{
		clampMax(ref count, other.count());
		for (int i = 0; i < count; ++i)
		{
			list.add(other[i]);
		}
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, IList<T> other, int startIndex, int count)
	{
		clampMax(ref count, other.count()- startIndex);
		for (int i = 0; i < count; ++i)
		{
			list.add(other[i + startIndex]);
		}
		return list;
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
	public static List<T> addRange<T>(this List<T> list, IEnumerable<T> other0, IEnumerable<T> other1)
	{
		addRange(list, other0);
		addRange(list, other1);
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, IEnumerable<T> other0, IEnumerable<T> other1, IEnumerable<T> other2)
	{
		addRange(list, other0);
		addRange(list, other1);
		addRange(list, other2);
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, IEnumerable<T> other0, IEnumerable<T> other1, IEnumerable<T> other2, IEnumerable<T> other3)
	{
		addRange(list, other0);
		addRange(list, other1);
		addRange(list, other2);
		addRange(list, other3);
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
	public static IList<T> setRange<T>(this T[] list, IEnumerable<T> other)
	{
		if (other == null || other.Count() == 0)
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
		if (other == null || other.Length == 0)
		{
			return list;
		}
		for (int i = 0; i < other.Length; ++i)
		{
			list[i] = other[i];
		}
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
	public static List<TKey> setRangeKeys<TKey, TValue>(this List<TKey> list, Dictionary<TKey, TValue> dic)
	{
		list.Clear();
		if (dic.isEmpty())
		{
			return list;
		}
		foreach (var item in dic)
		{
			list.add(item.Key);
		}
		return list;
	}
	public static List<TValue> setRangeValues<TKey, TValue>(this List<TValue> list, Dictionary<TKey, TValue> dic)
	{
		list.Clear();
		if (dic.isEmpty())
		{
			return list;
		}
		foreach (var item in dic)
		{
			list.add(item.Value);
		}
		return list;
	}
	public static T get<T>(this IList<T> list, int index)
	{
		if (list.isEmpty() || index < 0 || index >= list.Count)
		{
			return default;
		}
		return list[index];
	}
	public static bool set<T>(this IList<T> list, int index, T value)
	{
		if (index < 0 || index >= list.Count)
		{
			return false;
		}
		list[index] = value;
		return true;
	}
	public static T add<T>(this List<T> list, T value)
	{
		list.Add(value);
		return value;
	}
	public static void add<T>(this List<T> list, T value0, T value1)
	{
		list.Add(value0);
		list.Add(value1);
	}
	public static void add<T>(this List<T> list, T value0, T value1, T value2)
	{
		list.Add(value0);
		list.Add(value1);
		list.Add(value2);
	}
	public static void add<T>(this List<T> list, T value0, T value1, T value2, T value3)
	{
		list.Add(value0);
		list.Add(value1);
		list.Add(value2);
		list.Add(value3);
	}
	public static void add<T>(this List<T> list, T value0, T value1, T value2, T value3, T value4)
	{
		list.Add(value0);
		list.Add(value1);
		list.Add(value2);
		list.Add(value3);
		list.Add(value4);
	}
	// 将sourceList中的所有元素添加到targetList中,并清空sourceList,返回targetList
	public static List<T> moveTo<T>(this List<T> sourceList, List<T> targetList)
	{
		if (sourceList.isEmpty())
		{
			return targetList;
		}
		targetList.AddRange(sourceList);
		sourceList.Clear();
		return targetList;
	}
	public static T popBack<T>(this List<T> list)
	{
		if (list.isEmpty())
		{
			return default;
		}
		return list.removeAt(list.Count - 1);
	}
	public static T getLast<T>(this List<T> list)
	{
		if (list.isEmpty())
		{
			return default;
		}
		return list[^1];
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
	public static void For<T>(this IEnumerable<T> list, Action<T> action)
	{
		if (list == null)
		{
			return;
		}
		foreach (T item in list)
		{
			action(item);
		}
	}
	public static void ForI<T>(this IList<T> list, Action<int> action)
	{
		for (int i = 0; i < list.count(); ++i)
		{
			action(i);
		}
	}
	public static void ForI<T>(this Span<T> list, Action<int> action)
	{
		for (int i = 0; i < list.Length; ++i)
		{
			action(i);
		}
	}
	public static T find<T>(this IList<T> list, Predicate<T> match)
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
	public static bool find<T>(this IList<T> list, Predicate<T> match, out T value)
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
	public static bool find<T>(this IList<T> list, T value, out int index)
	{
		if (list.isEmpty())
		{
			index = -1;
			return false;
		}
		for (int i = 0; i < list.Count; ++i)
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
	public static int find<T>(this IList<T> list, T value)
	{
		if (list.isEmpty())
		{
			return -1;
		}
		for (int i = 0; i < list.Count; ++i)
		{
			if (list[i].Equals(value))
			{
				return i;
			}
		}
		return -1;
	}
	public static bool findIndex<T>(this IList<T> list, Predicate<T> match, out int index)
	{
		if (list.isEmpty() || match == null)
		{
			index = -1;
			return false;
		}
		for (int i = 0; i < list.Count; ++i)
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
	public static bool find<T>(this IList<T> list, Predicate<T> match, out int index, out T item)
	{
		if (list.isEmpty() || match == null)
		{
			index = -1;
			item = default;
			return false;
		}
		for (int i = 0; i < list.Count; ++i)
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
	public static bool find<T>(this IList<T> list, int startIndex, Predicate<T> match, out int index)
	{
		if (list.isEmpty() || match == null)
		{
			index = -1;
			return false;
		}
		for (int i = startIndex; i < list.Count; ++i)
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
	public static bool find<T>(this IList<T> list, int startIndex, int count, Predicate<T> match, out int index)
	{
		if (list.isEmpty() || match == null)
		{
			index = -1;
			return false;
		}
		count = getMin(count, list.Count);
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
	public static bool findIndex<T>(this Span<T> list, Predicate<T> match, out int index)
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
	public static bool findIndex<T>(this Span<T> list, Predicate<T> match, out int index, out T item)
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
	public static bool findIndex<T>(this Span<T> list, int startIndex, Predicate<T> match, out int index)
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
	public static bool findIndex<T>(this Span<T> list, int startIndex, int count, Predicate<T> match, out int index)
	{
		if (match == null)
		{
			index = -1;
			return false;
		}
		count = getMin(count, list.Length);
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
	public static void swap<T>(this IList<T> list, int index0, int index1)
	{
		if (list.isEmpty())
		{
			return;
		}
		(list[index0], list[index1]) = (list[index1], list[index0]);
	}
	public static int count<T>(this ICollection<T> list, Predicate<T> condition)
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
	public static int count<T>(this ICollection<T> list)							{ return list?.Count ?? 0; }
	public static bool isEmptySpan<T>(this Span<T> list)							{ return list == null || list.Length == 0; }
	public static bool isEmpty<T>(this ICollection<T> list)							{ return list == null || list.Count == 0; }
	public static bool contains<T>(this ICollection<T> list, T value)				{ return list != null && list.Contains(value); }
	public static bool contains<T>(this IList<T> list, Predicate<T> match)			{ return list != null && list.find(match) != null; }
	public static bool contains<T>(this Span<T> list, Predicate<T> match)			{ return list != null && list.find(match) != null; }
	public static IEnumerable<T> safe<T>(this IEnumerable<T> original)				{ return original ?? Empty<T>(); }
	public static T first<T>(this IEnumerable<T> list)
	{
		foreach (T item in list)
		{
			return item;
		}
		return default;
	}
	public static void inverse<T>(this IList<T> list)
	{
		if (list.count() <= 1)
		{
			return;
		}
		int count = list.Count;
		for (int i = 0; i < count >> 1; ++i)
		{
			(list[i], list[count - 1 - i]) = (list[count - 1 - i], list[i]);
		}
	}
}