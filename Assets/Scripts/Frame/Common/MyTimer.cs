using System;

// 自定义的计时器,需要手动调用tickTimer进行更新,返回值为是否到达指定时间
// 因为使用的是每帧的经过的时间来计时,可能会出现比预计时间短的情况,所以适用于时间不需要特别精确的地方
public class MyTimer
{
	public float mTimeInterval;		// 计时间隔,小于等于0表示任意时刻都已经完成计时
	public float mCurTime;			// 当前计时时间,大于等于0表示正在计时,小于0表示未开始计时
	public bool mLoop;				// 是否循环计时,循环计时时到达计时后将减去一个间隔再开始计时,不循环时则停止计时
	public MyTimer()
	{
		mCurTime = -1.0f;
	}
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
		if (mCurTime < 0.0f || mTimeInterval < 0.0f)
		{
			return false;
		}
		mCurTime += elapsedTime;
		if (mCurTime < mTimeInterval)
		{
			return false;
		}
		
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
	public void stop(bool resetInterval = true)
	{
		if(resetInterval)
		{
			mTimeInterval = -1.0f;
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