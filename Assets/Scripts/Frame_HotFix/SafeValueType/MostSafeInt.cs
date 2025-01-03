﻿using System;
using static UnityUtility;
using static FrameBase;
using static FrameEditorUtility;

// 用来代替普通的float类型,防止内存被修改器修改
public struct MostSafeInt : IEquatable<MostSafeInt>
{
	private SafeInt erhgre;			// 存储校验的数据
	private SafeInt gwegihweg;		// 存储读写的数据
	public MostSafeInt(int value)
	{
		erhgre = new(value);
		gwegihweg = new(value);
	}
	public int get()
	{
		int curValue = gwegihweg.get();
		int checkValue = erhgre.get();
		if (curValue != checkValue)
		{
			if (isEditor())
			{
				logError("校验失败");
			}
			mGameFramework.onMemoryModified(8, 0, 0, 0, 0);
		}
		return curValue;
	}
	public void set(int value)
	{
		gwegihweg.set(value);
		erhgre.set(value);
	}
	public bool Equals(MostSafeInt other)
	{
		return gwegihweg.Equals(other.gwegihweg) && erhgre.Equals(other.erhgre);
	}
}