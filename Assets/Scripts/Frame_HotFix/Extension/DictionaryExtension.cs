using System;
using System.Collections.Generic;
using System.Linq;
using static FrameBaseUtility;
using static FrameUtility;
using static UnityUtility;

public class EmptyDictionary<TKey, TValue>
{
	public static Dictionary<TKey, TValue> mList;
	public static Dictionary<TKey, TValue> getEmptyList()
	{
		mList ??= new();
		return mList;
	}
}

public static class DictionaryExtension
{
	public static void For<TKey, TValue>(this Dictionary<TKey, TValue> list, Action<KeyValuePair<TKey, TValue>> action)
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
	public static void forKey<TKey, TValue>(this Dictionary<TKey, TValue> list, Action<TKey> action)
	{
		if (list.isEmpty())
		{
			return;
		}
		foreach (var item in list)
		{
			action(item.Key);
		}
	}
	public static void forValue<TKey, TValue>(this Dictionary<TKey, TValue> list, Action<TValue> action)
	{
		if (list.isEmpty())
		{
			return;
		}
		foreach (var item in list)
		{
			action(item.Value);
		}
	}
	public static Dictionary<TKey, TValue> safe<TKey, TValue>(this Dictionary<TKey, TValue> dic)
	{
		return dic ?? EmptyDictionary<TKey, TValue>.getEmptyList();
	}
	public static void addOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
	{
		dic[key] = value;
	}
	public static void set<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value)
	{
		if (isEditor() && !dic.ContainsKey(key))
		{
			logError("字典中不包含此key,无法set");
		}
		dic[key] = value;
	}
	// 添加或者更新值,并且返回旧的值,只有当值有改变时才会返回被替换的值
	public static TValue replace<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value) where TValue : class
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
	public static Value get<Key, Value>(this Dictionary<Key, Value> map, Key key, Value defaultValue)
	{
		return map != null && map.TryGetValue(key, out Value value) ? value : defaultValue;
	}
	// 等效于CollectionExtensions.GetValueOrDefault
	public static Value get<Key, Value>(this Dictionary<Key, Value> map, Key key)
	{
		if (map == null)
		{
			return default;
		}
		map.TryGetValue(key, out Value value);
		return value;
	}
	public static bool addIf<Key, Value>(this Dictionary<Key, Value> map, Key key, Value value, bool condition)
	{
		if (condition)
		{
			map.Add(key, value);
		}
		return condition;
	}
	public static bool addIf<Key, Value>(this Dictionary<Key, Value> map, KeyValuePair<Key, Value> item, bool condition)
	{
		if (condition)
		{
			map.Add(item.Key, item.Value);
		}
		return condition;
	}
	public static Value add<Key, Value>(this Dictionary<Key, Value> map, Key key, Value value)
	{
		map.Add(key, value);
		return value;
	}
	public static void add<Key, Value>(this Dictionary<Key, Value> map, KeyValuePair<Key, Value> pair)
	{
		map.Add(pair.Key, pair.Value);
	}
	public static Value addClass<Key, Value>(this Dictionary<Key, Value> map, Key key) where Value : ClassObject, new() 
	{
		return map.add(key, CLASS<Value>());
	}
	public static Value addNotNullKey<Key, Value>(this Dictionary<Key, Value> map, Key key, Value value)
	{
		if (key == null)
		{
			return default;
		}
		map.Add(key, value);
		return value;
	}
	public static Value addNotNullValue<Key, Value>(this Dictionary<Key, Value> map, Key key, Value value)
	{
		if (value == null)
		{
			return default;
		}
		map.Add(key, value);
		return value;
	}
	public static Dictionary<Key, Value> setRange<Key, Value>(this Dictionary<Key, Value> map, Dictionary<Key, Value> other)
	{
		map.Clear();
		if (other.isEmpty())
		{
			return map;
		}
		foreach (var item in other)
		{
			map.Add(item.Key, item.Value);
		}
		return map;
	}
	public static Dictionary<Key, Value> addRange<Key, Value>(this Dictionary<Key, Value> map, Dictionary<Key, Value> other)
	{
		if (other.isEmpty())
		{
			return map;
		}
		foreach (var item in other)
		{
			map.TryAdd(item.Key, item.Value);
		}
		return map;
	}
	public static bool getKeyOfValue<TKey, TValue>(this Dictionary<TKey, TValue> dic, TValue value, out TKey key)
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
	public static TKey getKeyOfValue<TKey, TValue>(this Dictionary<TKey, TValue> dic, TValue value)
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
	public static bool getOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, TValue value, out TValue existValue)
	{
		if (dic.TryGetValue(key, out existValue))
		{
			return true;
		}
		dic.Add(key, value);
		return false;
	}
	public static void addOrIncreaseValue<TKey>(this Dictionary<TKey, int> dic, TKey key, int increase)
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
	public static void addOrIncreaseValue<TKey>(this Dictionary<TKey, float> dic, TKey key, float increase)
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
	public static Dictionary<Key, Value> setAllValue<Key, Value>(this Dictionary<Key, Value> map, Value value)
	{
		using var a = new ListScope<Key>(out var temp);
		temp.setRangeKeys(map);
		foreach (Key item in temp)
		{
			map[item] = value;
		}
		return map;
	}
	public static Dictionary<T0, T1> getOrAddListPersist<Key, T0, T1>(this Dictionary<Key, Dictionary<T0, T1>> map, Key key)
	{
		if (!map.TryGetValue(key, out var value))
		{
			map.Add(key, DIC_PERSIST(out value));
		}
		return value;
	}
	public static T getOrAddClass<Key, T>(this Dictionary<Key, T> map, Key key) where T : ClassObject, new()
	{
		if (!map.TryGetValue(key, out T value))
		{
			map.Add(key, CLASS(out value));
		}
		return value;
	}
	// 返回值表示是否获取到了列表中已经存在的值
	public static bool getOrAddClass<Key, T>(this Dictionary<Key, T> map, Key key, out T value) where T : ClassObject, new()
	{
		if (!map.TryGetValue(key, out value))
		{
			map.Add(key, CLASS(out value));
			return false;
		}
		return true;
	}
	// 返回值表示是否为列表中已经存在的对象出来的对象
	public static bool getOrAddNew<Key, Value>(this Dictionary<Key, Value> map, Key key, out Value value) where Value : new()
	{
		if (!map.TryGetValue(key, out value))
		{
			value = new();
			map.Add(key, value);
			return false;
		}
		return true;
	}
	public static Value getOrAddNew<Key, Value>(this Dictionary<Key, Value> map, Key key) where Value : new()
	{
		if (!map.TryGetValue(key, out Value value))
		{
			value = new();
			map.Add(key, value);
		}
		return value;
	}
	public static List<T> getOrAddListPersist<Key, T>(this Dictionary<Key, List<T>> map, Key key)
	{
		if (!map.TryGetValue(key, out var value))
		{
			map.Add(key, LIST_PERSIST(out value));
		}
		return value;
	}
	public static HashSet<T> getOrAddListPersist<Key, T>(this Dictionary<Key, HashSet<T>> map, Key key)
	{
		if (!map.TryGetValue(key, out var value))
		{
			map.Add(key, SET_PERSIST(out value));
		}
		return value;
	}
	public static Key tryGetValueKey<Key, T>(this Dictionary<Key, T> map, T value, Key defaultValue)
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
	public static bool removeIf<Key, T>(this Dictionary<Key, T> map, Key key0, bool condition)
	{
		if (condition)
		{
			map.Remove(key0);
		}
		return condition;
	}
	public static void remove<Key, T>(this Dictionary<Key, T> map, List<Key> keys)
	{
		if (keys.isEmpty())
		{
			return;
		}
		foreach (Key key in keys)
		{
			map.Remove(key);
		}
	}
	public static void removeKeys<Key, T, T0>(this Dictionary<Key, T> map, Dictionary<Key, T0> other)
	{
		if (other.isEmpty())
		{
			return;
		}
		foreach (var item in other)
		{
			map.Remove(item.Key);
		}
	}
	public static void remove<Key, T>(this Dictionary<Key, T> map, Key key0, Key key1)
	{
		map.Remove(key0);
		map.Remove(key1);
	}
	public static void remove<Key, T>(this Dictionary<Key, T> map, Key key0, Key key1, Key key2)
	{
		map.Remove(key0);
		map.Remove(key1);
		map.Remove(key2);
	}
	public static void remove<Key, T>(this Dictionary<Key, T> map, Key key0, Key key1, Key key2, Key key3)
	{
		map.Remove(key0);
		map.Remove(key1);
		map.Remove(key2);
		map.Remove(key3);
	}
	public static void remove<Key, T>(this Dictionary<Key, T> map, Key key0, Key key1, Key key2, Key key3, Key key4)
	{
		map.Remove(key0);
		map.Remove(key1);
		map.Remove(key2);
		map.Remove(key3);
		map.Remove(key4);
	}
	public static bool isEmpty<TKey, TValue>(this Dictionary<TKey, TValue> list) { return list == null || list.Count == 0; }
	public static int count<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<TKey> condition)
	{
		if (list.isEmpty() || condition == null)
		{
			return 0;
		}
		int curCount = 0;
		foreach (var item in list)
		{
			if (condition.Invoke(item.Key))
			{
				++curCount;
			}
		}
		return curCount;
	}
	public static int count<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<TValue> condition)
	{
		if (list.isEmpty() || condition == null)
		{
			return 0;
		}
		int curCount = 0;
		foreach (var item in list)
		{
			if (condition.Invoke(item.Value))
			{
				++curCount;
			}
		}
		return curCount;
	}
	public static int count<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate2<TKey, TValue> condition)
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
	public static TValue firstValue<TKey, TValue>(this Dictionary<TKey, TValue> list) 
	{
		if (list.count() == 0)
		{
			return default;
		}
		return list.First().Value; 
	}
	public static TKey firstKey<TKey, TValue>(this Dictionary<TKey, TValue> list) 
	{
		if (list.count() == 0)
		{
			return default;
		}
		return list.First().Key; 
	}
	public static bool find<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<KeyValuePair<TKey, TValue>> action, out KeyValuePair<TKey, TValue> value)
	{
		if (list.count() == 0)
		{
			value = default;
			return false;
		}
		foreach (var item in list)
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
	public static bool findKey<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<KeyValuePair<TKey, TValue>> action, out TKey key)
	{
		if (list.count() == 0)
		{
			key = default;
			return false;
		}
		foreach (var item in list)
		{
			if (action(item))
			{
				key = item.Key;
				return true;
			}
		}
		key = default;
		return false;
	}
	public static TKey findKey<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<KeyValuePair<TKey, TValue>> action)
	{
		if (list.count() == 0)
		{
			return default;
		}
		foreach (var item in list)
		{
			if (action(item))
			{
				return item.Key;
			}
		}
		return default;
	}
	public static bool findKey<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<TKey> action, out TKey key)
	{
		if (list.count() == 0)
		{
			key = default;
			return false;
		}
		foreach (var item in list)
		{
			if (action(item.Key))
			{
				key = item.Key;
				return true;
			}
		}
		key = default;
		return false;
	}
	public static TKey findKey<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<TKey> action)
	{
		if (list.count() == 0)
		{
			return default;
		}
		foreach (var item in list)
		{
			if (action(item.Key))
			{
				return item.Key;
			}
		}
		return default;
	}
	public static bool findValue<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<TValue> action, out TValue value)
	{
		if (list.count() == 0)
		{
			value = default;
			return false;
		}
		foreach (var item in list)
		{
			if (action(item.Value))
			{
				value = item.Value;
				return true;
			}
		}
		value = default;
		return false;
	}
	public static TValue findValue<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<TValue> action)
	{
		if (list.count() == 0)
		{
			return default;
		}
		foreach (var item in list)
		{
			if (action(item.Value))
			{
				return item.Value;
			}
		}
		return default;
	}
	public static bool findValue<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<KeyValuePair<TKey, TValue>> action, out TValue value)
	{
		if (list.count() == 0)
		{
			value = default;
			return false;
		}
		foreach (var item in list)
		{
			if (action(item))
			{
				value = item.Value;
				return true;
			}
		}
		value = default;
		return false;
	}
	public static TValue findValue<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<KeyValuePair<TKey, TValue>> action)
	{
		if (list.count() == 0)
		{
			return default;
		}
		foreach (var item in list)
		{
			if (action(item))
			{
				return item.Value;
			}
		}
		return default;
	}
	public static bool containsKey<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<TKey> action)
	{
		if (list.count() == 0)
		{
			return false;
		}
		foreach (var item in list)
		{
			if (action(item.Key))
			{
				return true;
			}
		}
		return false;
	}
	public static bool containsValue<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<TValue> action)
	{
		if (list.count() == 0)
		{
			return false;
		}
		foreach (var item in list)
		{
			if (action(item.Value))
			{
				return true;
			}
		}
		return false;
	}
	public static bool containsValue<TKey, TValue>(this Dictionary<TKey, TValue> list, TValue value)
	{
		if (list.count() == 0)
		{
			return false;
		}
		foreach (var item in list)
		{
			if (value.Equals(item.Value))
			{
				return true;
			}
		}
		return false;
	}
	public static bool contains<TKey, TValue>(this Dictionary<TKey, TValue> list, Predicate<KeyValuePair<TKey, TValue>> action)
	{
		if (list.count() == 0)
		{
			return false;
		}
		foreach (var item in list)
		{
			if (action(item))
			{
				return true;
			}
		}
		return false;
	}
	public static int count<TKey, TValue>(this Dictionary<TKey, TValue> list)		{ return list?.Count ?? 0; }
	public static KeyValuePair<TKey, TValue> first<TKey, TValue>(this Dictionary<TKey, TValue> list) 
	{
		foreach (var item in list)
		{
			return item;
		}
		return default;
	}
}