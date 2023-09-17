using System;
using System.Collections.Generic;
using static UnityUtility;

// 用来代替普通的float类型,防止内存被修改器修改
public struct MostSafeFloat : IEquatable<MostSafeFloat>
{
	private SafeFloat mWhyDoYouWantToFindThis;	// 存储读写的数据
	private SafeFloat mFuckYouWithThis;			// 存储校验数据
	public MostSafeFloat(float value)
	{
		mWhyDoYouWantToFindThis = new SafeFloat(value);
		mFuckYouWithThis = new SafeFloat(value);
	}
	public float get()
	{
		float curValue = mWhyDoYouWantToFindThis.get();
		float checkValue = mFuckYouWithThis.get();
		if (curValue != checkValue)
		{
#if UNITY_EDITOR
			logError("校验失败");
#else
			FrameBase.mGameFramework.onMemoryModified(7, 0, 0, 0, 0);
#endif
		}
		return curValue;
	}
	public void set(float value)
	{
		mWhyDoYouWantToFindThis.set(value);
		mFuckYouWithThis.set(value);
	}
	public bool Equals(MostSafeFloat other)
	{
		return mWhyDoYouWantToFindThis.Equals(other.mWhyDoYouWantToFindThis) && mFuckYouWithThis.Equals(other.mFuckYouWithThis);
	}
}