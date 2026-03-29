using System;
using System.Collections.Generic;
using static FrameUtility;
using static MathUtility;

public class EmptyList<T>
{
	public static List<T> mList;
	public static List<T> getEmptyList()
	{
		mList ??= new();
		return mList;
	}
}

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
	public static void setAllDefault<T>(this List<T> list)
	{
		if (list.isEmpty())
		{
			return;
		}
		for (int i = 0; i < list.Count; ++i)
		{
			list[i] = default;
		}
	}
	public static void setAllValue<T>(this List<T> list, T value)
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
	public static void remove<T>(this List<T> list, List<T> removeValues)
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
	public static void addRangeKeys<TKey, TValue>(this List<TKey> list, Dictionary<TKey, TValue> dic)
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
	public static void addRangeValues<TKey, TValue>(this List<TValue> list, Dictionary<TKey, TValue> dic)
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
	public static void addRangeNotNull<T>(this List<T> list, List<T> values) where T : class
	{
		for (int i = 0; i < values.Count; ++i)
		{
			list.addNotNull(values[i]);
		}
	}
	public static void addRangeNotNull<T>(this List<T> list, T[] values) where T : class
	{
		for (int i = 0; i < values.Length; ++i)
		{
			list.addNotNull(values[i]);
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
	public static List<T> addRange<T>(this List<T> list, List<T> other, int count)
	{
		clampMax(ref count, other.count());
		if (list.Capacity < list.Count + count)
		{
			list.Capacity = list.Count + count;
		}
		for (int i = 0; i < count; ++i)
		{
			list.add(other[i]);
		}
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, List<T> other, int startIndex, int count)
	{
		clampMax(ref count, other.count()- startIndex);
		if (list.Capacity < list.Count + count)
		{
			list.Capacity = list.Count + count;
		}
		for (int i = 0; i < count; ++i)
		{
			list.add(other[i + startIndex]);
		}
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, List<T> other)
	{
		if (list == null || other == null || other.Count == 0)
		{
			return list;
		}
		list.AddRange(other);
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, List<T> other0, List<T> other1)
	{
		int totalCount = list.Count + other0.count() + other1.count();
		if (list.Capacity < totalCount)
		{
			list.Capacity = totalCount;
		}
		addRange(list, other0);
		addRange(list, other1);
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, List<T> other0, List<T> other1, List<T> other2)
	{
		int totalCount = list.Count + other0.count() + other1.count() + other2.count();
		if (list.Capacity < totalCount)
		{
			list.Capacity = totalCount;
		}
		addRange(list, other0);
		addRange(list, other1);
		addRange(list, other2);
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, List<T> other0, List<T> other1, List<T> other2, List<T> other3)
	{
		int totalCount = list.Count + other0.count() + other1.count() + other2.count() + other3.count();
		if (list.Capacity < totalCount)
		{
			list.Capacity = totalCount;
		}
		addRange(list, other0);
		addRange(list, other1);
		addRange(list, other2);
		addRange(list, other3);
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, T[] other, int count)
	{
		clampMax(ref count, other.count());
		for (int i = 0; i < count; ++i)
		{
			list.add(other[i]);
		}
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, T[] other, int startIndex, int count)
	{
		clampMax(ref count, other.count() - startIndex);
		if (list.Capacity < list.Count + count)
		{
			list.Capacity = list.Count + count;
		}
		for (int i = 0; i < count; ++i)
		{
			list.add(other[i + startIndex]);
		}
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, T[] other)
	{
		if (list == null || other == null || other.Length == 0)
		{
			return list;
		}
		list.AddRange(other);
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, T[] other0, T[] other1)
	{
		int totalCount = list.Count + other0.count() + other1.count();
		if (list.Capacity < totalCount)
		{
			list.Capacity = totalCount;
		}
		addRange(list, other0);
		addRange(list, other1);
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, T[] other0, T[] other1, T[] other2)
	{
		int totalCount = list.Count + other0.count() + other1.count() + other2.count();
		if (list.Capacity < totalCount)
		{
			list.Capacity = totalCount;
		}
		addRange(list, other0);
		addRange(list, other1);
		addRange(list, other2);
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, T[] other0, T[] other1, T[] other2, T[] other3)
	{
		int totalCount = list.Count + other0.count() + other1.count() + other2.count() + other3.count();
		if (list.Capacity < totalCount)
		{
			list.Capacity = totalCount;
		}
		addRange(list, other0);
		addRange(list, other1);
		addRange(list, other2);
		addRange(list, other3);
		return list;
	}
	// T0 必须是 T1 的基类或者实现 T1 的接口
	public static List<Base> addRangeDerived<Base, T>(this List<Base> list, List<T> other) where Base : class where T : Base
	{
		if (other.isEmpty())
		{
			return list;
		}
		if (list.Capacity < list.Count + other.Count)
		{
			list.Capacity = list.Count + other.Count;
		}
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	// T0 必须是 T1 的基类或者实现 T1 的接口
	public static List<Base> addRangeDerived<Base, T>(this List<Base> list, T[] other) where Base : class where T : Base
	{
		if (other.isEmpty())
		{
			return list;
		}
		if (list.Capacity < list.Count + other.Length)
		{
			list.Capacity = list.Count + other.Length;
		}
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, Span<T> other)
	{
		if (list.Capacity < list.Count + other.Length)
		{
			list.Capacity = list.Count + other.Length;
		}
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	public static List<T> addRange<T>(this List<T> list, Span<T> other, int count)
	{
		if (list.Capacity < list.Count + count)
		{
			list.Capacity = list.Count + count;
		}
		for (int i = 0; i < count; ++i)
		{
			list.Add(other[i]);
		}
		return list;
	}
	public static List<T> setRange<T>(this List<T> list, List<T> other)
	{
		list.Clear();
		if (other == null || other.Count == 0)
		{
			return list;
		}
		list.AddRange(other);
		return list;
	}
	public static List<T> setRange<T>(this List<T> list, T[] other)
	{
		list.Clear();
		if (other == null || other.Length == 0)
		{
			return list;
		}
		list.AddRange(other);
		return list;
	}
	public static List<Base> setRangeDerived<Base, T>(this List<Base> list, List<T> other) where T : Base
	{
		list.Clear();
		if (other == null || other.Count == 0)
		{
			return list;
		}
		if (list.Capacity < list.Count + other.Count)
		{
			list.Capacity = list.Count + other.Count;
		}
		foreach (T item in other)
		{
			list.add(item);
		}
		return list;
	}
	public static List<Base> setRangeDerived<Base, T>(this List<Base> list, T[] other) where T : Base
	{
		list.Clear();
		if (other == null || other.Length == 0)
		{
			return list;
		}
		if (list.Capacity < list.Count + other.Length)
		{
			list.Capacity = list.Count + other.Length;
		}
		foreach (Base item in other)
		{
			list.add(item);
		}
		return list;
	}
	public static List<T> setRange<T>(this List<T> list, Span<T> other)
	{
		list.Clear();
		if (list.Capacity < other.Length)
		{
			list.Capacity = other.Length;
		}
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	public static List<T> setRange<T>(this List<T> list, Span<T> other, int count)
	{
		list.Clear();
		if (list.Capacity < other.Length)
		{
			list.Capacity = other.Length;
		}
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
	public static T get<T>(this List<T> list, int index)
	{
		if (list.isEmpty() || index < 0 || index >= list.Count)
		{
			return default;
		}
		return list[index];
	}
	public static bool set<T>(this List<T> list, int index, T value)
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
	public static bool isSubList<T>(this List<T> list, List<T> subList)
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
	public static void For<T>(this List<T> list, Action<T> action)
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
	public static void ForI<T>(this List<T> list, Action<int> action)
	{
		for (int i = 0; i < list.count(); ++i)
		{
			action(i);
		}
	}
	public static T find<T>(this List<T> list, Predicate<T> match)
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
	public static bool find<T>(this List<T> list, Predicate<T> match, out T value)
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
	public static bool find<T>(this List<T> list, T value, out int index)
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
	public static int find<T>(this List<T> list, T value)
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
	public static bool find<T>(this List<T> list, Predicate<T> match, out int index)
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
	public static bool find<T>(this List<T> list, Predicate<T> match, out int index, out T item)
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
	public static bool find<T>(this List<T> list, int startIndex, Predicate<T> match, out int index)
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
	public static bool find<T>(this List<T> list, int startIndex, int count, Predicate<T> match, out int index)
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
	public static void swap<T>(this List<T> list, int index0, int index1)
	{
		if (list.isEmpty())
		{
			return;
		}
		(list[index0], list[index1]) = (list[index1], list[index0]);
	}
	// 比较两个列表是否完全一致
	public static bool isSame<T>(this List<T> list0, List<T> list1)
	{
		if (list0 == null && list1 == null)
		{
			return true;
		}
		if (list0 == null || list1 == null)
		{
			return false;
		}
		int count = list0.Count;
		if (count != list1.Count)
		{
			return false;
		}
		for (int i = 0; i < count; ++i)
		{
			if (!EqualityComparer<T>.Default.Equals(list0[i], list1[i]))
			{
				return false;
			}
		}
		return true;
	}
	public static int count<T>(this List<T> list, Predicate<T> condition)
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
	public static int count<T>(this List<T> list)									{ return list?.Count ?? 0; }
	public static bool isEmpty<T>(this List<T> list)								{ return list == null || list.Count == 0; }
	public static bool contains<T>(this List<T> list, T value)						{ return list != null && list.Contains(value); }
	public static bool contains<T>(this List<T> list, Predicate<T> match)			{ return list != null && list.find(match) != null; }
	public static List<T> safe<T>(this List<T> original)							{ return original ?? EmptyList<T>.getEmptyList(); }
	public static T first<T>(this List<T> list)
	{
		foreach (T item in list)
		{
			return item;
		}
		return default;
	}
	public static T first<T>(this List<T> list, Predicate<T> action)
	{
		foreach (T item in list)
		{
			if (action(item))
			{
				return item;
			}
		}
		return default;
	}
	public static void inverse<T>(this List<T> list)
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