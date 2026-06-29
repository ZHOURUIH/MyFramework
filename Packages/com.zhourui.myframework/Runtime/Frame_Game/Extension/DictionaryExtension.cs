using System.Collections.Generic;
using static FrameBaseUtility;

public static class DictionaryExtension
{
	public static void set<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
	{
		if (isEditor() && !dic.ContainsKey(key))
		{
			logErrorBase("字典中不包含此key,无法set");
		}
		dic[key] = value;
	}
	// 等效于CollectionExtensions.GetValueOrDefault
	public static Value get<Key, Value>(this IDictionary<Key, Value> map, Key key)
	{
		if (map == null)
		{
			return default;
		}
		map.TryGetValue(key, out Value value);
		return value;
	}
	public static Value add<Key, Value>(this IDictionary<Key, Value> map, Key key, Value value)
	{
		map.Add(key, value);
		return value;
	}
	public static Value getOrAddNew<Key, Value>(this IDictionary<Key, Value> map, Key key) where Value : new()
	{
		if (!map.TryGetValue(key, out Value value))
		{
			value = new();
			map.Add(key, value);
		}
		return value;
	}
	public static Value addNotNullKey<Key, Value>(this IDictionary<Key, Value> map, Key key, Value value)
	{
		if (key == null)
		{
			return default;
		}
		map.Add(key, value);
		return value;
	}
	public static void setRange<Key, Value>(this IDictionary<Key, Value> map, IDictionary<Key, Value> other)
	{
		map.Clear();
		if (other == null)
		{
			return;
		}
		foreach (var item in other)
		{
			map.Add(item.Key, item.Value);
		}
	}
	public static bool isEmpty<TKey, TValue>(this IDictionary<TKey, TValue> list) { return list == null || list.Count == 0; }
}