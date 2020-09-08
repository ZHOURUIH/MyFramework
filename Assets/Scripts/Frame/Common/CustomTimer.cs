using System;
using System.Collections.Generic;

public class CustomTimer : GameBase
{
	public float mTimeInterval = 0.0f;	// 计时间隔,小于等于0表示任意时刻都已经完成计时
	public float mCurTime = -1.0f;		// 当前计时时间,大于等于0表示正在计时,小于0表示未开始计时
	public bool mLoop;					// 是否循环计时,循环计时时到达计时后将减去一个间隔再开始计时,不循环时则停止计时
	public void init(float defaultTime, float interval, bool loop = true)
	{
		mTimeInterval = interval;
		mCurTime = defaultTime;
		mLoop = loop;
	}
	public bool isCounting() { return mCurTime >= 0.0f; }
	public float getTimePercent() 
	{
		if(mTimeInterval <= 0.0f)
		{
			return 0.0f;
		}
		return mCurTime / mTimeInterval;
	}
	public bool tickTimer(float elapsedTime)
	{
		if (mCurTime < 0.0f)
		{
			return false;
		}
		if (mTimeInterval <= 0.0f)
		{
			return true;
		}
		mCurTime += elapsedTime;
		if (mCurTime >= mTimeInterval)
		{
			// 循环计时,则再重新开始计时
			if (mLoop)
			{
				mCurTime -= mTimeInterval;
				if (mTimeInterval <= 0.0f)
				{
					mCurTime = 0.0f;
				}
			}
			// 不循环计时,将mCurTime设置为-1
			else
			{
				mCurTime = -1.0f;
			}
			return true;
		}
		return false;
	}
	public void stop(bool resetInterval = true)
	{
		if(resetInterval)
		{
			mTimeInterval = 0.0f;
		}
		mCurTime = -1.0f;
	}
	public void start()
	{
		mCurTime = 0.0f;
	}
	public void resetToInterval()
	{
		mCurTime = mTimeInterval;
	}
	public void setInterval(float interval)
	{
		mTimeInterval = interval;
	}
}