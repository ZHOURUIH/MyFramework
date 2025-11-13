using System.Collections.Generic;
using static System.Linq.Enumerable;
using static FrameUtility;

public static class HashSetExtension
{
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
	public static HashSet<T> setRange<T>(this HashSet<T> list, IEnumerable<T> other)
	{
		list.Clear();
		if (other == null || other.Count() == 0)
		{
			return list;
		}
		foreach (T item in other)
		{
			list.Add(item);
		}
		return list;
	}
	public static HashSet<T> addRange<T>(this HashSet<T> list, IEnumerable<T> other)
	{
		if (other == null || other.Count() == 0)
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
	public static HashSet<Base> addRangeDerived<Base, T>(this HashSet<Base> list, IEnumerable<T> other) where Base : class where T : Base
	{
		if (other == null || other.Count() == 0)
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
}