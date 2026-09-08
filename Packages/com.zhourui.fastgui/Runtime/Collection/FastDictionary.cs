using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

/// <summary>
/// A GC-friendly dictionary intended for Unity hot paths.
///
/// Key design points:
/// 1. Updating an existing value is not a structural change and does not change
///    mStructureVersion, so Keys/Values/pair enumeration may continue safely.
/// 2. Keys / Values are readonly struct views and all direct enumerators are
///    structs, so direct foreach does not allocate.
/// 3. Buckets use 1-based heads (0 = empty). New bucket arrays therefore need
///    no -1 initialization and Clear can use the native Array.Clear path.
/// 4. Entry capacity and bucket capacity are decoupled. A requested capacity of
///    100,000 creates 100,000 entries instead of rounding entries to 131,072;
///    only the bucket array is rounded to a power of two.
/// 5. Bucket selection always applies one cheap high/low-bit mix. This removes
///    the per-operation default/custom-comparer branch while still protecting
///    power-of-two buckets from comparers with weak low hash bits.
/// 6. Miss-heavy paths check the raw 1-based bucket head before converting it
///    to an Entry index, so empty buckets return before the subtract/Entry load.
/// 7. ContainsKey probes directly instead of routing through a helper, avoiding
///    a hot-path call boundary on Unity runtimes that do not inline it consistently.
/// 8. Remove intentionally keeps a System.Dictionary-like standalone control
///    flow. Forcing this large method inline regressed miss-heavy workloads.
/// 9. Add/TryAdd use dedicated expanded insert paths.
/// 10. The indexer setter contains the set/insert hot path directly, avoiding an
///     extra helper call on Android IL2CPP while preserving non-structural
///     existing-value updates.
/// 11. The default comparer is represented by a null sentinel. Default hashing
///    uses key.GetHashCode() directly (a constrained call for value-type keys)
///    instead of dispatching through IEqualityComparer<TKey>.GetHashCode.
///    Custom comparers still use the exact comparer supplied by the caller.
///    This specifically targets Android IL2CPP, where successful insert and
///    sparse-miss paths showed stable interface-dispatch overhead.
/// 12. ContainsValue scans active entries directly with a cached
///    EqualityComparer<TValue>.Default. It adds standard Dictionary API
///    coverage without changing any existing key hot path.
/// 13. Remove(key, out value) performs lookup, unlink and value return in one
///    traversal. The existing Remove(key) path is deliberately left untouched.
/// 14. IEnumerable<KeyValuePair<TKey,TValue>> is implemented only as a compatibility
///    layer so collection initializers and interface-based APIs compile. Direct foreach
///    still binds to the public struct GetEnumerator() path and remains allocation-free.
/// </summary>
public class FastDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
	private struct Entry
	{
		public int mHashCode;
		public int mNext;
		public TKey mKey;
		public TValue mValue;
	}

	public readonly struct KeyCollection
	{
		private readonly FastDictionary<TKey, TValue> mDictionary;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal KeyCollection(FastDictionary<TKey, TValue> dictionary)
		{
			mDictionary = dictionary;
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => mDictionary.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public KeyEnumerator GetEnumerator()
		{
			return new KeyEnumerator(mDictionary);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(TKey key)
		{
			return mDictionary.ContainsKey(key);
		}

		public void CopyTo(TKey[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			}
			if (array.Length - arrayIndex < mDictionary.Count)
			{
				throw new ArgumentException("Destination array is too small.", nameof(array));
			}

			Entry[] entries = mDictionary.mEntries;
			int count = mDictionary.mCount;

			for (int i = 0; i < count; ++i)
			{
				if (entries[i].mHashCode >= 0)
				{
					array[arrayIndex++] = entries[i].mKey;
				}
			}
		}
	}

	public readonly struct ValueCollection
	{
		private readonly FastDictionary<TKey, TValue> mDictionary;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ValueCollection(FastDictionary<TKey, TValue> dictionary)
		{
			mDictionary = dictionary;
		}

		public int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => mDictionary.Count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ValueEnumerator GetEnumerator()
		{
			return new ValueEnumerator(mDictionary);
		}

		public void CopyTo(TValue[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}
			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			}
			if (array.Length - arrayIndex < mDictionary.Count)
			{
				throw new ArgumentException("Destination array is too small.", nameof(array));
			}

			Entry[] entries = mDictionary.mEntries;
			int count = mDictionary.mCount;

			for (int i = 0; i < count; ++i)
			{
				if (entries[i].mHashCode >= 0)
				{
					array[arrayIndex++] = entries[i].mValue;
				}
			}
		}
	}

	public struct KeyEnumerator
	{
		private readonly FastDictionary<TKey, TValue> mDictionary;
		private readonly Entry[] mEntries;
		private readonly int mCount;
		private readonly int mStructureVersion;
		private int mIndex;
		private TKey mCurrent;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal KeyEnumerator(FastDictionary<TKey, TValue> dictionary)
		{
			mDictionary = dictionary;
			mEntries = dictionary.mEntries;
			mCount = dictionary.mCount;
			mStructureVersion = dictionary.mStructureVersion;
			mIndex = 0;
			mCurrent = default;
		}

		public readonly TKey Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => mCurrent;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (mStructureVersion != mDictionary.mStructureVersion)
			{
				throw new InvalidOperationException(
					"Dictionary was structurally modified during enumeration.");
			}

			while ((uint)mIndex < (uint)mCount)
			{
				ref Entry entry = ref mEntries[mIndex++];
				if (entry.mHashCode >= 0)
				{
					mCurrent = entry.mKey;
					return true;
				}
			}

			mCurrent = default;
			return false;
		}
	}

	public struct ValueEnumerator
	{
		private readonly FastDictionary<TKey, TValue> mDictionary;
		private readonly Entry[] mEntries;
		private readonly int mCount;
		private readonly int mStructureVersion;
		private int mIndex;
		private TValue mCurrent;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ValueEnumerator(FastDictionary<TKey, TValue> dictionary)
		{
			mDictionary = dictionary;
			mEntries = dictionary.mEntries;
			mCount = dictionary.mCount;
			mStructureVersion = dictionary.mStructureVersion;
			mIndex = 0;
			mCurrent = default;
		}

		public readonly TValue Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => mCurrent;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (mStructureVersion != mDictionary.mStructureVersion)
			{
				throw new InvalidOperationException(
					"Dictionary was structurally modified during enumeration.");
			}

			while ((uint)mIndex < (uint)mCount)
			{
				ref Entry entry = ref mEntries[mIndex++];
				if (entry.mHashCode >= 0)
				{
					mCurrent = entry.mValue;
					return true;
				}
			}

			mCurrent = default;
			return false;
		}
	}

	public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
	{
		private readonly FastDictionary<TKey, TValue> mDictionary;
		private readonly Entry[] mEntries;
		private readonly int mCount;
		private readonly int mStructureVersion;
		private int mIndex;
		private KeyValuePair<TKey, TValue> mCurrent;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Enumerator(FastDictionary<TKey, TValue> dictionary)
		{
			mDictionary = dictionary;
			mEntries = dictionary.mEntries;
			mCount = dictionary.mCount;
			mStructureVersion = dictionary.mStructureVersion;
			mIndex = 0;
			mCurrent = default;
		}

		public readonly KeyValuePair<TKey, TValue> Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => mCurrent;
		}

		readonly object IEnumerator.Current
		{
			get => mCurrent;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (mStructureVersion != mDictionary.mStructureVersion)
			{
				throw new InvalidOperationException(
					"Dictionary was structurally modified during enumeration.");
			}

			while ((uint)mIndex < (uint)mCount)
			{
				ref Entry entry = ref mEntries[mIndex++];
				if (entry.mHashCode >= 0)
				{
					mCurrent = new KeyValuePair<TKey, TValue>(entry.mKey, entry.mValue);
					return true;
				}
			}

			mCurrent = default;
			return false;
		}

		public readonly void Dispose()
		{
		}

		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private const int DEFAULT_CAPACITY = 4;
	private const int MAX_CAPACITY = 0x7FEFFFFF;
	private const int MAX_POWER_OF_TWO_BUCKETS = 1 << 30;

	private static readonly bool sKeyIsValueType = typeof(TKey).IsValueType;
	private static readonly EqualityComparer<TKey> sDefaultComparer = EqualityComparer<TKey>.Default;
	private static readonly EqualityComparer<TValue> sDefaultValueComparer = EqualityComparer<TValue>.Default;

	// Bucket value is entry index + 1. 0 means empty.
	private int[] mBuckets;
	private Entry[] mEntries;
	private int mBucketMask;

	// mCount includes active entries and removed slots below mCount.
	private int mCount;
	private int mFreeList;
	private int mFreeCount;
	private int mStructureVersion;

	// Null means EqualityComparer<TKey>.Default. Custom comparers are stored
	// directly. The null sentinel lets the default hash path call
	// key.GetHashCode() without an IEqualityComparer interface dispatch.
	private readonly IEqualityComparer<TKey> mComparer;

	// Cached struct views keep Keys/Values allocation-free while avoiding
	// rebuilding the tiny view struct on every property access.
	private readonly KeyCollection mKeys;
	private readonly ValueCollection mValues;

	public int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => mCount - mFreeCount;
	}

	public int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => mEntries == null ? 0 : mEntries.Length;
	}

	public KeyCollection Keys
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => mKeys;
	}

	public ValueCollection Values
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => mValues;
	}

	public IEqualityComparer<TKey> Comparer
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => mComparer ?? sDefaultComparer;
	}

	public TValue this[TKey key]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (!sKeyIsValueType && key is null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			int[] buckets = mBuckets;
			if (buckets != null)
			{
				IEqualityComparer<TKey> comparer = mComparer;
				int hashCode = (comparer == null ? key.GetHashCode() : comparer.GetHashCode(key)) & 0x7FFFFFFF;
				int bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
				int headPlusOne = buckets[bucket];
				if (headPlusOne != 0)
				{
					Entry[] entries = mEntries;
					for (int i = headPlusOne - 1; i >= 0; i = entries[i].mNext)
					{
						ref Entry entry = ref entries[i];
						if (entry.mHashCode == hashCode && (comparer == null ? sDefaultComparer.Equals(entry.mKey, key) : comparer.Equals(entry.mKey, key)))
						{
							return entry.mValue;
						}
					}
				}
			}

			throw new KeyNotFoundException();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if (!sKeyIsValueType && key is null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (mBuckets == null)
			{
				initialize(DEFAULT_CAPACITY);
			}

			IEqualityComparer<TKey> comparer = mComparer;
			int hashCode = (comparer == null ? key.GetHashCode() : comparer.GetHashCode(key)) & 0x7FFFFFFF;
			int bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
			int[] buckets = mBuckets;
			Entry[] entries = mEntries;

			for (int i = buckets[bucket] - 1; i >= 0; i = entries[i].mNext)
			{
				ref Entry existing = ref entries[i];
				if (existing.mHashCode == hashCode &&
					(comparer == null
						? sDefaultComparer.Equals(existing.mKey, key)
						: comparer.Equals(existing.mKey, key)))
				{
					// Updating an existing value is intentionally not structural.
					existing.mValue = value;
					return;
				}
			}

			int index;
			if (mFreeCount > 0)
			{
				index = mFreeList;
				mFreeList = entries[index].mNext;
				--mFreeCount;
			}
			else
			{
				if (mCount == entries.Length)
				{
					resize(getExpandedEntryCapacity(mCount));
					bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
					buckets = mBuckets;
					entries = mEntries;
				}

				index = mCount++;
			}

			ref Entry entry = ref entries[index];
			entry.mHashCode = hashCode;
			entry.mNext = buckets[bucket] - 1;
			entry.mKey = key;
			entry.mValue = value;
			buckets[bucket] = index + 1;
			++mStructureVersion;
		}
	}

	public FastDictionary()
		: this(0, null)
	{
	}

	public FastDictionary(int capacity)
		: this(capacity, null)
	{
	}

	public FastDictionary(IEqualityComparer<TKey> comparer)
		: this(0, comparer)
	{
	}

	public FastDictionary(int capacity, IEqualityComparer<TKey> comparer)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity));
		}

		mComparer = comparer == null || ReferenceEquals(comparer, sDefaultComparer)
			? null
			: comparer;
		mKeys = new KeyCollection(this);
		mValues = new ValueCollection(this);
		mFreeList = -1;

		if (capacity > 0)
		{
			initialize(capacity);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	/// <summary>
	/// Adds a new key. This hot path is intentionally expanded instead of routing
	/// through insert(key, value, add:true). Android IL2CPP showed a large, stable
	/// regression for pre-sized Add even though lookup/update were already competitive.
	/// Keeping duplicate probing and the common append path in one method removes the
	/// boolean add-mode branch plus the insertNewEntry call boundary from every Add.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(TKey key, TValue value)
	{
		if (!sKeyIsValueType && key is null)
		{
			throw new ArgumentNullException(nameof(key));
		}

		if (mBuckets == null)
		{
			initialize(DEFAULT_CAPACITY);
		}

		IEqualityComparer<TKey> comparer = mComparer;
		int hashCode = (comparer == null ? key.GetHashCode() : comparer.GetHashCode(key)) & 0x7FFFFFFF;
		int bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
		int[] buckets = mBuckets;
		Entry[] entries = mEntries;

		for (int i = buckets[bucket] - 1; i >= 0; i = entries[i].mNext)
		{
			ref Entry existing = ref entries[i];
			if (existing.mHashCode == hashCode && (comparer == null ? sDefaultComparer.Equals(existing.mKey, key) : comparer.Equals(existing.mKey, key)))
			{
				throw new ArgumentException(
					"An item with the same key has already been added.",
					nameof(key));
			}
		}

		int index;
		if (mFreeCount > 0)
		{
			index = mFreeList;
			mFreeList = entries[index].mNext;
			--mFreeCount;
		}
		else
		{
			if (mCount == entries.Length)
			{
				resize(getExpandedEntryCapacity(mCount));
				bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
				buckets = mBuckets;
				entries = mEntries;
			}

			index = mCount++;
		}

		ref Entry entry = ref entries[index];
		entry.mHashCode = hashCode;
		entry.mNext = buckets[bucket] - 1;
		entry.mKey = key;
		entry.mValue = value;
		buckets[bucket] = index + 1;
		++mStructureVersion;
	}

	/// <summary>
	/// TryAdd mirrors the expanded Add path so Android IL2CPP can optimize the
	/// successful insert path without crossing insertNewEntry.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryAdd(TKey key, TValue value)
	{
		if (!sKeyIsValueType && key is null)
		{
			throw new ArgumentNullException(nameof(key));
		}

		if (mBuckets == null)
		{
			initialize(DEFAULT_CAPACITY);
		}

		IEqualityComparer<TKey> comparer = mComparer;
		int hashCode = (comparer == null ? key.GetHashCode() : comparer.GetHashCode(key)) & 0x7FFFFFFF;
		int bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
		int[] buckets = mBuckets;
		Entry[] entries = mEntries;

		for (int i = buckets[bucket] - 1; i >= 0; i = entries[i].mNext)
		{
			ref Entry existing = ref entries[i];
			if (existing.mHashCode == hashCode && (comparer == null ? sDefaultComparer.Equals(existing.mKey, key) : comparer.Equals(existing.mKey, key)))
			{
				return false;
			}
		}

		int index;
		if (mFreeCount > 0)
		{
			index = mFreeList;
			mFreeList = entries[index].mNext;
			--mFreeCount;
		}
		else
		{
			if (mCount == entries.Length)
			{
				resize(getExpandedEntryCapacity(mCount));
				bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
				buckets = mBuckets;
				entries = mEntries;
			}

			index = mCount++;
		}

		ref Entry entry = ref entries[index];
		entry.mHashCode = hashCode;
		entry.mNext = buckets[bucket] - 1;
		entry.mKey = key;
		entry.mValue = value;
		buckets[bucket] = index + 1;
		++mStructureVersion;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool ContainsKey(TKey key)
	{
		if (!sKeyIsValueType && key is null)
		{
			throw new ArgumentNullException(nameof(key));
		}

		int[] buckets = mBuckets;
		if (buckets == null)
		{
			return false;
		}

		IEqualityComparer<TKey> comparer = mComparer;
		int hashCode = (comparer == null ? key.GetHashCode() : comparer.GetHashCode(key)) & 0x7FFFFFFF;
		int bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
		int headPlusOne = buckets[bucket];
		if (headPlusOne == 0)
		{
			return false;
		}

		Entry[] entries = mEntries;
		for (int i = headPlusOne - 1; i >= 0; i = entries[i].mNext)
		{
			ref Entry entry = ref entries[i];
			if (entry.mHashCode == hashCode && (comparer == null ? sDefaultComparer.Equals(entry.mKey, key) : comparer.Equals(entry.mKey, key)))
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Returns true when any active entry contains the supplied value. Value
	/// comparison intentionally follows System.Dictionary and always uses
	/// EqualityComparer&lt;TValue&gt;.Default; the key comparer does not participate.
	/// Direct array scanning keeps the method allocation-free.
	/// </summary>
	public bool ContainsValue(TValue value)
	{
		Entry[] entries = mEntries;
		if (entries == null)
		{
			return false;
		}

		EqualityComparer<TValue> comparer = sDefaultValueComparer;
		int count = mCount;
		for (int i = 0; i < count; ++i)
		{
			ref Entry entry = ref entries[i];
			if (entry.mHashCode >= 0 && comparer.Equals(entry.mValue, value))
			{
				return true;
			}
		}

		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue(TKey key, out TValue value)
	{
		if (!sKeyIsValueType && key is null)
		{
			throw new ArgumentNullException(nameof(key));
		}

		int[] buckets = mBuckets;
		if (buckets != null)
		{
			IEqualityComparer<TKey> comparer = mComparer;
			int hashCode = (comparer == null ? key.GetHashCode() : comparer.GetHashCode(key)) & 0x7FFFFFFF;
			int bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
			int headPlusOne = buckets[bucket];
			if (headPlusOne != 0)
			{
				Entry[] entries = mEntries;
				for (int i = headPlusOne - 1; i >= 0; i = entries[i].mNext)
				{
					ref Entry entry = ref entries[i];
					if (entry.mHashCode == hashCode && (comparer == null ? sDefaultComparer.Equals(entry.mKey, key) : comparer.Equals(entry.mKey, key)))
					{
						value = entry.mValue;
						return true;
					}
				}
			}
		}

		value = default;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TValue GetValueOrDefault(TKey key)
	{
		if (!sKeyIsValueType && key is null)
		{
			throw new ArgumentNullException(nameof(key));
		}

		int[] buckets = mBuckets;
		if (buckets != null)
		{
			IEqualityComparer<TKey> comparer = mComparer;
			int hashCode = (comparer == null ? key.GetHashCode() : comparer.GetHashCode(key)) & 0x7FFFFFFF;
			int bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
			int headPlusOne = buckets[bucket];
			if (headPlusOne != 0)
			{
				Entry[] entries = mEntries;
				for (int i = headPlusOne - 1; i >= 0; i = entries[i].mNext)
				{
					ref Entry entry = ref entries[i];
					if (entry.mHashCode == hashCode && (comparer == null ? sDefaultComparer.Equals(entry.mKey, key) : comparer.Equals(entry.mKey, key)))
					{
						return entry.mValue;
					}
				}
			}
		}

		return default;
	}

	/// <summary>
	/// Removes a key using a control-flow shape intentionally kept close to the
	/// Unity/.NET Framework Dictionary implementation. In Unity Mono, forcing
	/// this relatively large method inline made the miss path slower. Letting the
	/// JIT compile it as a standalone method and allowing an empty bucket to fail
	/// the for-loop condition directly produces a smaller miss path.
	/// </summary>
	public bool Remove(TKey key)
	{
		if (!sKeyIsValueType && key is null)
		{
			throw new ArgumentNullException(nameof(key));
		}

		if (mBuckets != null)
		{
			IEqualityComparer<TKey> comparer = mComparer;
			int hashCode = (comparer == null ? key.GetHashCode() : comparer.GetHashCode(key)) & 0x7FFFFFFF;
			int bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
			int previous = -1;

			// Buckets are 1-based, so an empty bucket is 0 and becomes -1 here.
			// There is deliberately no separate empty-bucket branch: the loop test
			// handles the common miss case directly.
			for (int i = mBuckets[bucket] - 1; i >= 0; i = mEntries[i].mNext)
			{
				if (mEntries[i].mHashCode == hashCode &&
					(comparer == null
						? sDefaultComparer.Equals(mEntries[i].mKey, key)
						: comparer.Equals(mEntries[i].mKey, key)))
				{
					int next = mEntries[i].mNext;

					if (previous < 0)
					{
						mBuckets[bucket] = next + 1;
					}
					else
					{
						mEntries[previous].mNext = next;
					}

					mEntries[i].mHashCode = -1;
					mEntries[i].mNext = mFreeList;
					mEntries[i].mKey = default;
					mEntries[i].mValue = default;

					mFreeList = i;
					++mFreeCount;
					++mStructureVersion;
					return true;
				}

				previous = i;
			}
		}

		return false;
	}

	/// <summary>
	/// Removes the specified key and returns its previous value in one lookup.
	/// This mirrors the modern Dictionary.Remove(TKey, out TValue) API while
	/// keeping the already benchmarked Remove(TKey) implementation unchanged.
	/// </summary>
	public bool Remove(TKey key, out TValue value)
	{
		if (!sKeyIsValueType && key is null)
		{
			throw new ArgumentNullException(nameof(key));
		}

		if (mBuckets != null)
		{
			IEqualityComparer<TKey> comparer = mComparer;
			int hashCode = (comparer == null ? key.GetHashCode() : comparer.GetHashCode(key)) & 0x7FFFFFFF;
			int bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & mBucketMask;
			int previous = -1;

			for (int i = mBuckets[bucket] - 1; i >= 0; i = mEntries[i].mNext)
			{
				if (mEntries[i].mHashCode == hashCode &&
					(comparer == null
						? sDefaultComparer.Equals(mEntries[i].mKey, key)
						: comparer.Equals(mEntries[i].mKey, key)))
				{
					int next = mEntries[i].mNext;
					value = mEntries[i].mValue;

					if (previous < 0)
					{
						mBuckets[bucket] = next + 1;
					}
					else
					{
						mEntries[previous].mNext = next;
					}

					mEntries[i].mHashCode = -1;
					mEntries[i].mNext = mFreeList;
					mEntries[i].mKey = default;
					mEntries[i].mValue = default;

					mFreeList = i;
					++mFreeCount;
					++mStructureVersion;
					return true;
				}

				previous = i;
			}
		}

		value = default;
		return false;
	}

	public void Clear()
	{
		if (mCount == 0)
		{
			return;
		}

		// 1-based buckets let zero mean empty, so this is substantially cheaper
		// than writing -1 in a managed loop.
		Array.Clear(mBuckets, 0, mBuckets.Length);
		Array.Clear(mEntries, 0, mCount);

		mCount = 0;
		mFreeList = -1;
		mFreeCount = 0;
		++mStructureVersion;
	}

	/// <summary>
	/// Ensures room for at least capacity entries. Returns the resulting entry
	/// capacity. It is a structural storage change, but because active Entry
	/// indexes and dictionary content stay unchanged, current enumerators remain
	/// valid only if they were created before this call and no entries are added.
	/// For simple, predictable semantics we invalidate them here.
	/// </summary>
	public int EnsureCapacity(int capacity)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(capacity));
		}

		if (mEntries == null)
		{
			if (capacity == 0)
			{
				return 0;
			}

			initialize(capacity);
			++mStructureVersion;
			return mEntries.Length;
		}

		if (capacity <= mEntries.Length)
		{
			return mEntries.Length;
		}

		resize(capacity);
		++mStructureVersion;
		return mEntries.Length;
	}

	private void initialize(int entryCapacity)
	{
		if (entryCapacity < DEFAULT_CAPACITY)
		{
			entryCapacity = DEFAULT_CAPACITY;
		}
		if (entryCapacity > MAX_CAPACITY)
		{
			throw new ArgumentOutOfRangeException(nameof(entryCapacity));
		}

		int bucketCapacity = getBucketCapacity(entryCapacity);
		mBuckets = new int[bucketCapacity];
		mEntries = new Entry[entryCapacity];
		mBucketMask = bucketCapacity - 1;
		mFreeList = -1;
	}

	private void resize(int newEntryCapacity)
	{
		if (newEntryCapacity <= mEntries.Length)
		{
			return;
		}
		if (newEntryCapacity > MAX_CAPACITY)
		{
			throw new InvalidOperationException("Dictionary capacity exceeded.");
		}

		int newBucketCapacity = getBucketCapacity(newEntryCapacity);
		int[] newBuckets = new int[newBucketCapacity];
		Entry[] newEntries = new Entry[newEntryCapacity];
		Array.Copy(mEntries, 0, newEntries, 0, mCount);

		int newMask = newBucketCapacity - 1;
		for (int i = 0; i < mCount; ++i)
		{
			if (newEntries[i].mHashCode < 0)
			{
				continue;
			}

			int hashCode = newEntries[i].mHashCode;
			int bucket = (hashCode ^ (int)((uint)hashCode >> 16)) & newMask;
			newEntries[i].mNext = newBuckets[bucket] - 1;
			newBuckets[bucket] = i + 1;
		}

		mBuckets = newBuckets;
		mEntries = newEntries;
		mBucketMask = newMask;
	}

	private static int getExpandedEntryCapacity(int currentCapacity)
	{
		if (currentCapacity < DEFAULT_CAPACITY)
		{
			return DEFAULT_CAPACITY;
		}
		if (currentCapacity >= MAX_CAPACITY)
		{
			throw new InvalidOperationException("Dictionary capacity exceeded.");
		}

		long doubled = (long)currentCapacity << 1;
		return doubled > MAX_CAPACITY ? MAX_CAPACITY : (int)doubled;
	}

	private static int getBucketCapacity(int entryCapacity)
	{
		int required = entryCapacity < DEFAULT_CAPACITY ? DEFAULT_CAPACITY : entryCapacity;
		if (required >= MAX_POWER_OF_TWO_BUCKETS)
		{
			return MAX_POWER_OF_TWO_BUCKETS;
		}

		--required;
		required |= required >> 1;
		required |= required >> 2;
		required |= required >> 4;
		required |= required >> 8;
		required |= required >> 16;
		++required;

		return required < DEFAULT_CAPACITY ? DEFAULT_CAPACITY : required;
	}
}
