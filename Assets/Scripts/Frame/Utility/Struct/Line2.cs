using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;

public struct Line2
{
	public Vector2 mStart;
	public Vector2 mEnd;
	public float mK;		// 直线的斜率
	public float mB;        // 直线与Y轴的交点
	public bool mHasK;
	public Line2(Vector2 start, Vector2 end)
	{
		mStart = start;
		mEnd = end;
		mHasK = !isFloatZero(mEnd.x - mStart.x);
		if (mHasK)
		{
			mK = (mEnd.y - mStart.y) / (mEnd.x - mStart.x);
			mB = mStart.y - mK * mStart.x;
		}
		else 
		{
			mK = 0;
			mB = 0;
		}
	}
	public Line3 toLine3()
	{
		return new Line3(mStart, mEnd);
	}
	// 获取直线上指定x坐标对应的y坐标
	public bool getPointYOnLine(float x, out float y)
	{
		y = mHasK ? mK * x + mB : 0.0f;
		return mHasK;
	}
	// 获取直线上指定y坐标对应的x坐标
	public bool getPointXOnLine(float y, out float x)
	{
		// 没有斜率,是一条平行于Y轴的直线,所有X都是一样的
		if (!mHasK)
		{
			x = mStart.x;
			return true;
		}
		// 斜率为0,是一条平行于X轴的执行,获取不到x坐标
		if (isFloatZero(mK))
		{
			x = 0;
			return false;
		}
		x = (y - mB) / mK;
		return true;
	}
}