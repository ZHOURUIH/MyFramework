using System;
using System.Collections.Generic;

// 复数
public struct Complex
{
	public float mReal;
	public float mImg;
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