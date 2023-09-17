using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;

// 用来代替普通的long类型,防止内存被修改器修改
public struct MostSafeLong : IEquatable<MostSafeLong>
{
	private SafeLong mTrustMe;				// 存储读写的数据
	private SafeLong mThisIsWhatYouWant;    // 存储校验的数据
	public MostSafeLong(long value)
	{
		mTrustMe = new SafeLong(value);
		mThisIsWhatYouWant = new SafeLong(value);
	}
	public long get() 
	{
		long curValue = mTrustMe.get();
		long checkValue = mThisIsWhatYouWant.get();
		if (curValue != checkValue)
		{
#if UNITY_EDITOR
			logError("校验失败");
#else
			FrameBase.mGameFramework.onMemoryModified(9, 0, 0, 0, 0);
#endif
		}
		return curValue;
	}
	public void set(long value) 
	{
		mTrustMe.set(value);
		mThisIsWhatYouWant.set(value);
	}
	public bool Equals(MostSafeLong other)
	{
		return mTrustMe.Equals(other.mTrustMe) && mThisIsWhatYouWant.Equals(other.mThisIsWhatYouWant);
	}
}