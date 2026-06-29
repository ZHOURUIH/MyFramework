using System;

// 表示一个Key-Value对的类型
public struct DictionaryType : IEquatable<DictionaryType>
{
	private int mValueTypeHash;	// Value类型的Hash
	private int mKeyTypeHash;	// Key类型的Hash
	private Type mValueType;	// Value的类型
	private Type mKeyType;		// Key的类型
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