using System;

// 时间工具函数,由于与时间的相关操作比较多,所以单独写到时间工具类中
public class TimeUtility : FileUtility
{
	protected static DateTime mTime19700101 = new DateTime(1970, 1, 1);
	protected static long mRemoteTimeStamp;		// 远端的时间戳
	protected static long mRemoteTimeSpan;		// 本地时间与远端时间的差值,毫秒数
	// 设置远端的时间戳,计算出本地时间与远端时间的差值
	public static void setRemoteTime(long remoteTime) { mRemoteTimeSpan = timeGetTimeUTC() - remoteTime; }
	// 获取从1970年1月1日到现在所经过的毫秒数
	public static long timeGetTime() { return (long)(DateTime.Now - mTime19700101).TotalMilliseconds; }
	public static long timeGetTimeUTC() { return (long)(DateTime.UtcNow - mTime19700101).TotalMilliseconds; }
	public static void generateRemoteTimeStamp() { mRemoteTimeStamp = timeGetTimeUTC() - mRemoteTimeSpan; }
	public static string getTime(TIME_DISPLAY display) { return getTime(DateTime.Now, display); }
	public static string getTimeThread(TIME_DISPLAY display) { return getTimeThread(DateTime.Now, display); }
	public static string getTimeNoBuilder(TIME_DISPLAY display) { return getTimeNoBuilder(DateTime.Now, display); }
	public static string getTime(long timeStamp, TIME_DISPLAY display) { return getTime(timeStampToDateTimeUTC(timeStamp), display); }
	// 一般用于倒计时显示的字符串
	public static string getRemainTime(int timeSecond, TIME_DISPLAY display)
	{
		int min = timeSecond / 60;
		int second = timeSecond % 60;
		int hour = min / 60;
		if (display == TIME_DISPLAY.HMSM)
		{
			return strcat(IToS(hour), ":", IToS(min), ":", IToS(second));
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			return strcat(IToS(hour, 2), ":", IToS(min, 2), ":", IToS(second, 2));
		}
		else if (display == TIME_DISPLAY.DHMS_ZH)
		{
			int totalMin = timeSecond / 60;
			int totalHour = totalMin / 60;
			int totalDay = totalHour / 24;
			int curHour = totalHour % 24;
			int curMin = totalMin % 60;
			int curSecond = timeSecond % 60;
			// 大于等于1天
			if (totalDay > 0)
			{
				return strcat(IToS(totalDay), "天", IToS(curHour), "时", IToS(curMin), "分", IToS(curSecond) + "秒");
			}
			// 小于1天,并且大于等于1小时
			else if (totalHour > 0)
			{
				return strcat(IToS(totalHour), "时", IToS(curMin), "分", IToS(curSecond) + "秒");
			}
			// 小于1小时,并且大于等于1分钟
			else if (totalMin > 0)
			{
				return IToS(totalMin) + "分" + IToS(curSecond) + "秒";
			}
			return timeSecond + "秒";
		}
		return EMPTY;
	}
	// 可以在多线程中调用的获取当前时间字符串
	public static string getTimeThread(DateTime time, TIME_DISPLAY display)
	{
		if (display == TIME_DISPLAY.HMSM)
		{
			return strcat_thread(IToS(time.Hour), ":", IToS(time.Minute), ":", IToS(time.Second), ":", IToS(time.Millisecond));
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			return strcat_thread(IToS(time.Hour, 2), ":", IToS(time.Minute, 2), ":", IToS(time.Second, 2));
		}
		else if (display == TIME_DISPLAY.DHMS_ZH)
		{
			return strcat_thread(IToS(time.Hour), "时", IToS(time.Minute), "分", IToS(time.Second), "秒");
		}
		else if (display == TIME_DISPLAY.YMD_ZH)
		{
			return strcat_thread(IToS(time.Year), "年", IToS(time.Month), "月", IToS(time.Day), "日");
		}
		return EMPTY;
	}
	// 只能在主线程中调用的获取当前时间字符串
	public static string getTime(DateTime time, TIME_DISPLAY display)
	{
		if (display == TIME_DISPLAY.HMSM)
		{
			return strcat(IToS(time.Hour), ":", IToS(time.Minute), ":", IToS(time.Second), ":", IToS(time.Millisecond));
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			return strcat(IToS(time.Hour, 2), ":", IToS(time.Minute, 2), ":", IToS(time.Second, 2));
		}
		else if (display == TIME_DISPLAY.DHMS_ZH)
		{
			return strcat(IToS(time.Hour), "时", IToS(time.Minute), "分", IToS(time.Second), "秒");
		}
		else if (display == TIME_DISPLAY.YMD_ZH)
		{
			return strcat_thread(IToS(time.Year), "年", IToS(time.Month), "月", IToS(time.Day), "日");
		}
		return EMPTY;
	}
	public static string getTimeNoBuilder(DateTime time, TIME_DISPLAY display)
	{
		if (display == TIME_DISPLAY.HMSM)
		{
			return IToS(time.Hour) + ":" + IToS(time.Minute) + ":" + IToS(time.Second) + ":" + IToS(time.Millisecond);
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			return IToS(time.Hour, 2) + ":" + IToS(time.Minute, 2) + ":" + IToS(time.Second, 2);
		}
		else if (display == TIME_DISPLAY.DHMS_ZH)
		{
			return IToS(time.Hour) + "时" + IToS(time.Minute) + "分" + IToS(time.Second) + "秒";
		}
		return EMPTY;
	}
	// 将时间转化成时间戳,dateTime是本地时间
	public static long dateTimeToTimeStamp(DateTime dateTime)
	{
		return (long)(dateTime - mTime19700101).TotalSeconds;
	}
	// 将时间戳转化成时间,转换后是utc时间
	public static DateTime timeStampToDateTimeUTC(long unixTimeStamp)
	{
		return mTime19700101.AddSeconds(unixTimeStamp);
	}
}