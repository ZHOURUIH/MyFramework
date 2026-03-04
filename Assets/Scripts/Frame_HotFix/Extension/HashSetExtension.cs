using System;
using System.Collections.Generic;
using static FrameUtility;

public static class HashSetExtension
{
	public static T addOrRemove<T>(this HashSet<T> list, T value, bool add)
	{
		if (add)
		{
			list.Add(value);
		}
		else
		{
			list.Remove(value);
		}
		return value;
	}
	public static T add<T>(this HashSet<T> list, T value)
	{
		list.Add(value);
		return value;
	}
	public static bool addNot<T>(this HashSet<T> list, T value, T notValue)
	{
		if (value.Equals(notValue))
		{
			return false;
		}
		list.Add(value);
		return true;
	}
	public static HashSet<TKey> addRangeKeys<TKey, TValue>(this HashSet<TKey> list, Dictionary<TKey, TValue> other)
	{
		if (other.isEmpty())
		{
			return list;
		}
		foreach (var item in other)
		{
			list.Add(item.Key);
		}
		return list;
	}
	public static HashSet<TValue> addRangeValues<TKey, TValue>(this HashSet<TValue> list, Dictionary<TKey, TValue> other)
	{
		if (other.isEmpty())
		{
			return list;
		}
		foreach (var item in other)
		{
			list.Add(item.Value);
		}
		return list;
	}
	public static HashSet<TKey> setRangeKeys<TKey, TValue>(this HashSet<TKey> list, Dictionary<TKey, TValue> other)
	{
		if (other.isEmpty())
		{
			return list;
		}
		list.Clear();
		foreach (var item in other)
		{
			list.Add(item.Key);
		}
		return list;
	}
	public static HashSet<TValue> setRangeValues<TKey, TValue>(this HashSet<TValue> list, Dictionary<TKey, TValue> other)
	{
		if (other.isEmpty())
		{
			return list;
		}
		list.Clear();
		foreach (var item in other)
		{
			list.Add(item.Value);
		}
		return list;
	}
	public static HashSet<T> setRange<T>(this HashSet<T> list, HashSet<T> other)
	{
		list.Clear();
		if (other.isEmpty())
		{
			return list;
		}
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	public static HashSet<T> addRange<T>(this HashSet<T> list, IList<T> other)
	{
		if (other.isEmpty())
		{
			return list;
		}
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	public static bool addNotNull<T>(this HashSet<T> list, T value) where T : class
	{
		if (value != null)
		{
			list.Add(value);
			return true;
		}
		return false;
	}
	public static bool addIf<T>(this HashSet<T> list, T value, bool condition)
	{
		if (condition)
		{
			return list.Add(value);
		}
		return condition;
	}
	public static T addClass<T>(this HashSet<T> list) where T : ClassObject, new()
	{
		return list.add(CLASS<T>());
	}
	// Base 必须是 T 的基类或者实现 T 的接口
	public static HashSet<Base> addRangeDerived<Base, T>(this HashSet<Base> list, IList<T> other) where Base : class where T : Base
	{
		if (other.isEmpty())
		{
			return list;
		}
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	public static bool addNotEmpty(this HashSet<string> list, string value)
	{
		if (!value.isEmpty())
		{
			list.Add(value);
			return true;
		}
		return false;
	}
	public static T first<T>(this HashSet<T> list)
	{
		foreach (T item in list)
		{
			return item;
		}
		return default;
	}
	public static T popFirst<T>(this HashSet<T> list)
	{
		T elem = default;
		foreach (T item in list)
		{
			elem = item;
			break;
		}
		list.Remove(elem);
		return elem;
	}
	public static bool contains<T>(this HashSet<T> list, Predicate<T> action)
	{
		if (list.isEmpty())
		{
			return false;
		}
		foreach (T item in list)
		{
			if (action(item))
			{
				return true;
			}
		}
		return false;
	}
}