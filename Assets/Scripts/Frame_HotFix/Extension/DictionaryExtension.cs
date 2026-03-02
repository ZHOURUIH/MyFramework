using System;
using System.Collections.Generic;
using System.Linq;
using static FrameBaseUtility;
using static FrameUtility;
using static UnityUtility;

public static class DictionaryExtension
{
	public static void For<TKey, TValue>(this IDictionary<TKey, TValue> list, Action<KeyValuePair<TKey, TValue>> action)
	{
		if (list.isEmpty())
		{
			return;
		}
		foreach (var item in list)
		{
			action(item);
		}
	}
	public static void forKey<TKey, TValue>(this IDictionary<TKey, TValue> list, Action<TKey> action)
	{
		if (list.isEmpty())
		{
			return;
		}
		foreach (TKey item in list.Keys)
		{
			action(item);
		}
	}
	public static void forValue<TKey, TValue>(this IDictionary<TKey, TValue> list, Action<TValue> action)
	{
		if (list.isEmpty())
		{
			return;
		}
		foreach (TValue item in list.Values)
		{
			action(item);
		}
	}
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
	// 添加或者更新值,并且返回旧的值,只有当值有改变时才会返回被替换的值
	public static TValue replace<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value) where TValue : class
	{
		dic.TryGetValue(key, out TValue curValue);
		if (curValue != value)
		{
			dic[key] = value;
			return curValue;
		}
		return null;
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
	public static bool addIf<Key, Value>(this IDictionary<Key, Value> map, Key key, Value value, bool condition)
	{
		if (condition)
		{
			map.Add(key, value);
		}
		return condition;
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
	public static void addRange<Key, Value>(this Dictionary<Key, Value> map, IDictionary<Key, Value> other)
	{
		if (other == null)
		{
			return;
		}
		foreach (var item in other)
		{
			map.TryAdd(item.Key, item.Value);
		}
	}
	public static bool getKeyOfValue<TKey, TValue>(this IDictionary<TKey, TValue> dic, TValue value, out TKey key)
	{
		if (dic.isEmpty())
		{
			key = default;
			return false;
		}
		foreach (var item in dic)
		{
			if (item.Value.Equals(value))
			{
				key = item.Key;
				return true;
			}
		}
		key = default;
		return false;
	}
	public static TKey getKeyOfValue<TKey, TValue>(this IDictionary<TKey, TValue> dic, TValue value)
	{
		if (dic.isEmpty())
		{
			return default;
		}
		foreach (var item in dic)
		{
			if (item.Value.Equals(value))
			{
				return item.Key;
			}
		}
		return default;
	}
	// 返回值表示是否get成功
	public static bool getOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey key, TValue value, out TValue existValue)
	{
		if (dic.TryGetValue(key, out existValue))
		{
			return true;
		}
		dic.Add(key, value);
		return false;
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
	public static Key tryGetValueKey<Key, T>(this IDictionary<Key, T> map, T value, Key defaultValue)
	{
		foreach (var item in map)
		{
			if ((item.Value == null && value == null) ||
				(item.Value != null && value != null && item.Value.Equals(value)))
			{
				return item.Key;
			}
		}
		return defaultValue;
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
	public static int count<TKey, TValue>(this IDictionary<TKey, TValue> list, Predicate<TKey> condition)
	{
		if (list.isEmpty() || condition == null)
		{
			return 0;
		}
		int curCount = 0;
		foreach (TKey item in list.Keys)
		{
			if (condition.Invoke(item))
			{
				++curCount;
			}
		}
		return curCount;
	}
	public static int count<TKey, TValue>(this IDictionary<TKey, TValue> list, Predicate<TValue> condition)
	{
		if (list.isEmpty() || condition == null)
		{
			return 0;
		}
		int curCount = 0;
		foreach (TValue item in list.Values)
		{
			if (condition.Invoke(item))
			{
				++curCount;
			}
		}
		return curCount;
	}
	public static int count<TKey, TValue>(this IDictionary<TKey, TValue> list, Predicate2<TKey, TValue> condition)
	{
		if (list.isEmpty() || condition == null)
		{
			return 0;
		}
		int curCount = 0;
		foreach (var item in list)
		{
			if (condition.Invoke(item.Key, item.Value))
			{
				++curCount;
			}
		}
		return curCount;
	}
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
	public static bool findKey<TKey, TValue>(this IDictionary<TKey, TValue> list, Predicate<TKey> action, out TKey key)
	{
		if (list.count() == 0)
		{
			key = default;
			return false;
		}
		foreach (TKey item in list.Keys)
		{
			if (action(item))
			{
				key = item;
				return true;
			}
		}
		key = default;
		return false;
	}
	public static TKey findKey<TKey, TValue>(this IDictionary<TKey, TValue> list, Predicate<TKey> action)
	{
		if (list.count() == 0)
		{
			return default;
		}
		foreach (TKey item in list.Keys)
		{
			if (action(item))
			{
				return item;
			}
		}
		return default;
	}
	public static bool findValue<TKey, TValue>(this IDictionary<TKey, TValue> list, Predicate<TValue> action, out TValue value)
	{
		if (list.count() == 0)
		{
			value = default;
			return false;
		}
		foreach (TValue item in list.Values)
		{
			if (action(item))
			{
				value = item;
				return true;
			}
		}
		value = default;
		return false;
	}
	public static TValue findValue<TKey, TValue>(this IDictionary<TKey, TValue> list, Predicate<TValue> action)
	{
		if (list.count() == 0)
		{
			return default;
		}
		foreach (TValue item in list.Values)
		{
			if (action(item))
			{
				return item;
			}
		}
		return default;
	}
	public static bool hasKey<TKey, TValue>(this IDictionary<TKey, TValue> list, Predicate<TKey> action)
	{
		if (list.count() == 0)
		{
			return false;
		}
		foreach (TKey item in list.Keys)
		{
			if (action(item))
			{
				return true;
			}
		}
		return false;
	}
	public static bool hasValue<TKey, TValue>(this IDictionary<TKey, TValue> list, Predicate<TValue> action)
	{
		if (list.count() == 0)
		{
			return false;
		}
		foreach (TValue item in list.Values)
		{
			if (action(item))
			{
				return true;
			}
		}
		return false;
	}
}