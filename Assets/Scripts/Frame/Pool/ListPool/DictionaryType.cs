using System;

public struct DictionaryType : IEquatable<DictionaryType>
{
	public Type mKeyType;
	public Type mValueType;
	public DictionaryType(Type keyType, Type valueType)
	{
		mKeyType = keyType;
		mValueType = valueType;
	}
	public bool Equals(DictionaryType other)
	{
		return mKeyType == other.mKeyType && mValueType == other.mValueType;
	}
	public override int GetHashCode()
	{
		return mKeyType.GetHashCode() + mValueType.GetHashCode();
	}
	public override string ToString()
	{
		return mKeyType + "," + mValueType;
	}
}