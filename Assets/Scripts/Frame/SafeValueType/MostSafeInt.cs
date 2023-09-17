using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBase;

// 用来代替普通的float类型,防止内存被修改器修改
public struct MostSafeInt : IEquatable<MostSafeInt>
{
	private SafeInt mFindAnyThing;		// 存储校验的数据
	private SafeInt mJustFuck;			// 存储读写的数据
	public MostSafeInt(int value)
	{
		mFindAnyThing = new SafeInt(value);
		mJustFuck = new SafeInt(value);
	}
	public int get()
	{
		int curValue = mJustFuck.get();
		int checkValue = mFindAnyThing.get();
		if (curValue != checkValue)
		{
#if UNITY_EDITOR
			logError("校验失败");
#else
			mGameFramework.onMemoryModified(8, 0, 0, 0, 0);
#endif
		}
		return curValue;
	}
	public void set(int value)
	{
		mJustFuck.set(value);
		mFindAnyThing.set(value);
	}
	public bool Equals(MostSafeInt other)
	{
		return mJustFuck.Equals(other.mJustFuck) && mFindAnyThing.Equals(other.mFindAnyThing);
	}
}