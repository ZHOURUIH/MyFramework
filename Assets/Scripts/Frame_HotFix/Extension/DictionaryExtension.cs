using System.Collections.Generic;
using System.Linq;
using static FrameBaseUtility;
using static FrameUtility;
using static UnityUtility;

public static class DictionaryExtension
{
	public static IDictionary<TKey, TValue> safe<TKey, TValue>(this IDictionary<TKey, TValue> dic)
	{
		return dic ?? new MyEmptyDictionary<TKey, TValue>();
	}
	public static void addOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
	{
		dic[key] = value;
	}
	public static void set<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
	{
		if (isEditor() && !dic.ContainsKey(key))
		{
			logError("字典中不包含此key,无法set");
		}
		dic[key] = value;
	}
	// 添加或者更新值,并且返回旧的值
	public static TValue replace<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value)
	{
		dic.TryGetValue(key, out TValue curValue);
		dic[key] = value;
		return curValue;
	}
	// 等效于CollectionExtensions.GetValueOrDefault
	public static Value get<Key, Value>(this IDictionary<Key, Value> map, Key key, Value defaultValue)
	{
		return map != null && map.TryGetValue(key, out Value value) ? value : defaultValue;
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
	public static void add<Key, Value>(this IDictionary<Key, Value> map, KeyValuePair<Key, Value> pair)
	{
		map.Add(pair.Key, pair.Value);
	}
	public static Value addClass<Key, Value>(this IDictionary<Key, Value> map, Key key) where Value : ClassObject, new() 
	{
		return map.add(key, CLASS<Value>());
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
	public static Value addNotNullValue<Key, Value>(this IDictionary<Key, Value> map, Key key, Value value)
	{
		if (value == null)
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
	public static void addRange<Key, Value>(this IDictionary<Key, Value> map, IDictionary<Key, Value> other)
	{
		if (other == null)
		{
			return;
		}
		foreach (var item in other)
		{
			map.Add(item.Key, item.Value);
		}
	}
	public static void addOrIncreaseValue<TKey>(this IDictionary<TKey, int> dic, TKey key, int increase)
	{
		if (dic.TryGetValue(key, out int curValue))
		{
			dic[key] = curValue + increase;
		}
		else
		{
			dic.Add(key, increase);
		}
	}
	public static void addOrIncreaseValue<TKey>(this IDictionary<TKey, float> dic, TKey key, float increase)
	{
		if (dic.TryGetValue(key, out float curValue))
		{
			dic[key] = curValue + increase;
		}
		else
		{
			dic.Add(key, increase);
		}
	}
	public static void setAllValue<Key, Value>(this IDictionary<Key, Value> map, Value value)
	{
		using var a = new ListScope<Key>(out var temp, map.Keys);
		foreach (Key item in temp)
		{
			map[item] = value;
		}
	}
	public static Dictionary<T0, T1> getOrAddListPersist<Key, T0, T1>(this IDictionary<Key, Dictionary<T0, T1>> map, Key key)
	{
		if (!map.TryGetValue(key, out var value))
		{
			DIC_PERSIST(out value);
			map.Add(key, value);
		}
		return value;
	}
	public static T getOrAddClass<Key, T>(this IDictionary<Key, T> map, Key key) where T : ClassObject, new()
	{
		if (!map.TryGetValue(key, out T value))
		{
			map.Add(key, CLASS(out value));
		}
		return value;
	}
	// 返回值表示是否获取到了列表中已经存在的值
	public static bool getOrAddClass<Key, T>(this IDictionary<Key, T> map, Key key, out T value) where T : ClassObject, new()
	{
		if (!map.TryGetValue(key, out value))
		{
			map.Add(key, CLASS(out value));
			return false;
		}
		return true;
	}
	// 返回值表示是否为列表中已经存在的对象出来的对象
	public static bool getOrAddNew<Key, Value>(this IDictionary<Key, Value> map, Key key, out Value value) where Value : new()
	{
		if (!map.TryGetValue(key, out value))
		{
			value = new();
			map.Add(key, value);
			return false;
		}
		return true;
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
	public static List<T> getOrAddListPersist<Key, T>(this IDictionary<Key, List<T>> map, Key key)
	{
		if (!map.TryGetValue(key, out var value))
		{
			LIST_PERSIST(out value);
			map.Add(key, value);
		}
		return value;
	}
	public static HashSet<T> getOrAddListPersist<Key, T>(this IDictionary<Key, HashSet<T>> map, Key key)
	{
		if (!map.TryGetValue(key, out var value))
		{
			SET_PERSIST(out value);
			map.Add(key, value);
		}
		return value;
	}
	public static void remove<Key, T>(this IDictionary<Key, T> map, Key key0, Key key1)
	{
		map.Remove(key0);
		map.Remove(key1);
	}
	public static void remove<Key, T>(this IDictionary<Key, T> map, Key key0, Key key1, Key key2)
	{
		map.Remove(key0);
		map.Remove(key1);
		map.Remove(key2);
	}
	public static void remove<Key, T>(this IDictionary<Key, T> map, Key key0, Key key1, Key key2, Key key3)
	{
		map.Remove(key0);
		map.Remove(key1);
		map.Remove(key2);
		map.Remove(key3);
	}
	public static void remove<Key, T>(this IDictionary<Key, T> map, Key key0, Key key1, Key key2, Key key3, Key key4)
	{
		map.Remove(key0);
		map.Remove(key1);
		map.Remove(key2);
		map.Remove(key3);
		map.Remove(key4);
	}
	public static bool isEmpty<TKey, TValue>(this IDictionary<TKey, TValue> list) { return list == null || list.Count == 0; }
	public static TValue firstValue<TKey, TValue>(this IDictionary<TKey, TValue> list) 
	{
		if (list.count() == 0)
		{
			return default;
		}
		return list.First().Value; 
	}
	public static TKey firstKey<TKey, TValue>(this IDictionary<TKey, TValue> list) 
	{
		if (list.count() == 0)
		{
			return default;
		}
		return list.First().Key; 
	}
}