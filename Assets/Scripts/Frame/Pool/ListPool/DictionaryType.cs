using System;

// 表示一个Key-Value对的类型
public struct DictionaryType : IEquatable<DictionaryType>
{
	public int mValueTypeHash;	// Value类型的Hash
	public int mKeyTypeHash;	// Key类型的Hash
	public Type mValueType;		// Value的类型
	public Type mKeyType;		// Key的类型
	public DictionaryType(Type keyType, Type valueType)
	{
		mKeyType = keyType;
		mValueType = valueType;
		mKeyTypeHash = mKeyType.GetHashCode();
		mValueTypeHash = mValueType.GetHashCode();
	}
	public bool Equals(DictionaryType other) { return mKeyType == other.mKeyType && mValueType == other.mValueType; }
	public override int GetHashCode() { return mKeyTypeHash + mValueTypeHash; }
	public override string ToString() { return mKeyType + "," + mValueType; }
}