using System.Collections.Generic;
using System.Linq;

public static class ListExtension
{
	public static bool addUnique<T>(this List<T> list, T value)
	{
		if (!list.Contains(value))
		{
			list.Add(value);
			return true;
		}
		return false;
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
	public static List<T> setRange<T>(this List<T> list, IList<T> other)
	{
		list.Clear();
		if (other == null || other.Count() == 0)
		{
			return list;
		}
		list.AddRange(other);
		return list;
	}
	public static bool isEmpty<T>(this ICollection<T> list) { return list == null || list.Count == 0; }
	public static int count<T>(this ICollection<T> list) { return list != null ? list.Count : 0; }
}