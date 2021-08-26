using System;
using System.Collections.Generic;

// 因为即使继承了IEquatable,也会因为本身是带T模板的,无法在重写的Equals中完全避免装箱和拆箱,所以不继承IEquatable
// 而且实际使用时也不会调用此类型的比较函数
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