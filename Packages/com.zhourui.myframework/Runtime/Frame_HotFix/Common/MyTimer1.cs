using System;
using static MathUtility;

// 自定义的计时器,需要手动调用tickTimer进行更新,返回值为是否到达指定时间
// 相比于MyTimer,会更加精确,不会出现比预计时间短的情况,但是效率比MyTimer慢10倍
public class MyTimer1
{
	public DateTime mLastTime;      // 上一次获取的时间
	public float mTimeInterval;     // 计时间隔,小于等于0表示任意时刻都已经完成计时
	public float mCurTime;          // 当前计时时间,大于等于0表示正在计时,小于0表示未开始计时
	public bool mLoop;              // 是否循环计时,循环计时时到达计时后将减去一个间隔再开始计时,不循环时则停止计时
	public MyTimer1()
	{
		mCurTime = -1.0f;
	}
	public void init(float defaultTime, float interval, bool loop = true)
	{
		mTimeInterval = interval;
		mCurTime = defaultTime;
		mLoop = loop;
		mLastTime = DateTime.Now;
	}
	public bool isCounting() { return mCurTime >= 0.0f; }
	public float getTimePercent()
	{
		if (mTimeInterval <= 0.0f)
		{
			return 0.0f;
		}
		return divide(mCurTime, mTimeInterval);
	}
	public bool tickTimer()
	{
		if (mCurTime < 0.0f || mTimeInterval < 0.0f)
		{
			return false;
		}
		DateTime now = DateTime.Now;
		mCurTime += (float)(now - mLastTime).TotalSeconds;
		mLastTime = now;
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
		if (resetInterval)
		{
			mTimeInterval = -1.0f;
		}
		mCurTime = -1.0f;
	}
	public void start()
	{
		mCurTime = 0.0f;
		mLastTime = DateTime.Now;
	}
	public void resetToInterval()
	{
		mCurTime = mTimeInterval;
		mLastTime = DateTime.Now;
	}
	public void setInterval(float interval)
	{
		mTimeInterval = interval;
	}
}