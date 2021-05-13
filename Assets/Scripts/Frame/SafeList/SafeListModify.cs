using System.Collections.Generic;

public struct SafeListModify<T>
{
	public T mValue;
	public bool mAdd;
	public SafeListModify(T value, bool add)
	{
		mValue = value;
		mAdd = add;
	}
}