using System;
using System.Collections;
using System.Collections.Generic;

// 空的字典集合,用于在字典扩展中当字典为空引用时返回空的集合
public struct MyEmptyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
	public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable, IEnumerator, IDictionaryEnumerator
	{
		private KeyValuePair<TKey, TValue> current;
		public KeyValuePair<TKey, TValue> Current { get { return current; } }
		object IEnumerator.Current { get { return new KeyValuePair<TKey, TValue>(current.Key, current.Value); } }
		DictionaryEntry IDictionaryEnumerator.Entry { get { return new DictionaryEntry(current.Key, current.Value); } }
		object IDictionaryEnumerator.Key { get { return current.Key; } }
		object IDictionaryEnumerator.Value { get { return current.Value; } }
		public bool MoveNext() { return false; }
		public void Dispose() { }
		void IEnumerator.Reset() { current = default; }
	}
	public readonly TValue this[TKey key] { get { return default; } set { } }
	public struct EmptyList<T> : ICollection<T>
	{
		public struct Enumerator : IEnumerator<T>, IDisposable, IEnumerator
		{
			private T currentKey;
			public T Current { get { return currentKey; } }
			object IEnumerator.Current { get { return currentKey; } }
			public void Dispose() { }
			public bool MoveNext() { return false; }
			void IEnumerator.Reset() { currentKey = default; }
		}
		public readonly int Count => 0;
		public readonly bool IsReadOnly => true;
		public readonly void Add(T item) { }
		public readonly void Clear() { }
		public readonly bool Contains(T item) { return false; }
		public readonly void CopyTo(T[] array, int arrayIndex) { }
		public readonly IEnumerator<T> GetEnumerator() { return new Enumerator(); }
		public readonly bool Remove(T item) { return false; }
		readonly IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(); }
	}
	public ICollection<TKey> Keys { get { return new EmptyList<TKey>(); } }
	public ICollection<TValue> Values { get { return new EmptyList<TValue>(); } }
	public readonly int Count => 0;
	public readonly bool IsReadOnly => true;
	public readonly void Add(TKey key, TValue value) { }
	public readonly void Add(KeyValuePair<TKey, TValue> item) { }
	public readonly void Clear() { }
	public readonly bool Contains(KeyValuePair<TKey, TValue> item) { return false; }
	public readonly bool ContainsKey(TKey key) { return false; }
	public readonly void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { }
	public readonly IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return new Enumerator(); }
	public readonly bool Remove(TKey key) { return false; }
	public readonly bool Remove(KeyValuePair<TKey, TValue> item) { return false; }
	public readonly bool TryGetValue(TKey key, out TValue value) { value = default; return false; }
	readonly IEnumerator IEnumerable.GetEnumerator() { return new Enumerator(); }
}