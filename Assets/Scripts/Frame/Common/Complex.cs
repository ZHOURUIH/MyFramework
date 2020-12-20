using System;
using System.Collections.Generic;

// 结构体如果要存列表并且要进行Contains或ContainsKey查询时,最好是要继承Equatable
// 这样在列表中查找时会判断该类型是否继承了Equatable
// 如果继承了就会在比较时调用已经重写过的Equals函数
// 从而避免默认调用Equals(object),可以避免装箱带来的GC
// 复数
public struct Complex : IEquatable<Complex>
{
	public float mReal;
	public float mImg;
	public bool Equals(Complex value) { return value.mReal == mReal && value.mImg == mImg; }
	public Complex(float realPart, float imgPart)
	{
		mReal = realPart;
		mImg = imgPart;
	}
	public static Complex operator+(Complex c0, Complex c1)
	{
		return new Complex(c1.mReal + c0.mReal, c1.mImg + c0.mImg);
	}
	public static Complex operator -(Complex c0, Complex c1)
	{
		return new Complex(c0.mReal - c1.mReal, c0.mImg - c1.mImg);
	}
}