using System;
using System.Text;

// 时间工具函数,由于与时间的相关操作比较多,所以单独写到时间工具类中
public class TimeUtility : FileUtility
{
	protected static DateTime mTime19700101 = new DateTime(1970, 1, 1);
	protected static long mRemoteTimeStampMS;	// 远端的时间戳
	protected static long mRemoteTimeSpan;      // 本地时间与远端时间的差值,毫秒数
	// 设置远端的时间戳,计算出本地时间与远端时间的差值
	public static void setRemoteTimeMS(long remoteTime) { mRemoteTimeSpan = timeGetTimeUTC() - remoteTime; }
	// 获取从1970年1月1日到现在所经过的毫秒数
	public static long timeGetTime() { return (long)(DateTime.Now - mTime19700101).TotalMilliseconds; }
	// 获取从1970年1月1日到当前UTC时间(世界标准时间)所经过的毫秒数
	public static long timeGetTimeUTC() { return (long)(DateTime.UtcNow - mTime19700101).TotalMilliseconds; }
	// 利用与远端的时间差,刷新计算出远端的时间,一般每帧刷新一次
	public static void generateRemoteTimeStampMS() { mRemoteTimeStampMS = timeGetTimeUTC() - mRemoteTimeSpan; }
	// 获得时间的字符串,display表示显示的格式,仅可在主线程中调用
	public static string getTime(TIME_DISPLAY display) { return getTime(DateTime.Now, display); }
	// 获得时间的字符串,display表示显示的格式,可在子线程中调用,效率较低
	public static string getTimeThread(TIME_DISPLAY display) { return getTimeThread(DateTime.Now, display); }
	// 获得时间的字符串,display表示显示的格式
	public static string getTimeNoBuilder(TIME_DISPLAY display) { return getTimeNoBuilder(DateTime.Now, display); }
	public static string getTime(long timeStamp, TIME_DISPLAY display) { return getTime(timeStampToDateTimeUTC(timeStamp), display); }
	// 将时间转化成时间戳,dateTime是本地时间
	public static long dateTimeToTimeStamp(DateTime dateTime) { return (long)(dateTime - mTime19700101).TotalSeconds; }
	// 将时间转化成时间戳,dateTime是本地时间
	public static long dateTimeToTimeStampMS(DateTime dateTime) { return (long)(dateTime - mTime19700101).TotalMilliseconds; }
	// 将时间戳转化成时间,转换后是utc时间
	public static DateTime timeStampToDateTimeUTC(long timeStamp) { return mTime19700101.AddSeconds(timeStamp); }
	// 将时间戳转化成时间,转换后是utc时间
	public static DateTime timeStampMSToDateTimeUTC(long timeStamp) { return mTime19700101.AddMilliseconds(timeStamp); }
	// 获得当前的本地时间戳,以秒为单位
	public static long getNowTimeStamp() { return dateTimeToTimeStamp(DateTime.Now); }
	// 获得当前的本地时间戳,以毫秒为单位
	public static long getNowTimeStampMS() { return dateTimeToTimeStampMS(DateTime.Now); }
	// 获得当前的UTC时间戳,以秒为单位
	public static long getNowUTCTimeStamp() { return dateTimeToTimeStamp(DateTime.UtcNow); }
	// 获得当前的UTC时间戳,以毫秒为单位
	public static long getNowUTCTimeStampMS() { return dateTimeToTimeStampMS(DateTime.UtcNow); }
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
	// 不使用自定义的字符串拼接器,使用内置的StringBuilder进行拼接
	public static string getTimeNoBuilder(DateTime time, TIME_DISPLAY display)
	{
		var builder = new StringBuilder(256);
		if (display == TIME_DISPLAY.HMSM)
		{
			builder.Append(IToS(time.Hour)).
					Append(":").
					Append(IToS(time.Minute)).
					Append(":").
					Append(IToS(time.Second)).
					Append(":").
					Append(IToS(time.Millisecond));
			return builder.ToString();
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			builder.Append(IToS(time.Hour, 2)).
					Append(":").
					Append(IToS(time.Minute, 2)).
					Append(":").
					Append(IToS(time.Second, 2));
			return builder.ToString();
		}
		else if (display == TIME_DISPLAY.DHMS_ZH)
		{
			builder.Append(IToS(time.Hour)).
					Append("时").
					Append(IToS(time.Minute)).
					Append("分").
					Append(IToS(time.Second)).
					Append("秒");
			return builder.ToString();
		}
		else if(display == TIME_DISPLAY.YMD_ZH)
		{
			builder.Append(IToS(time.Year)).
					Append("年").
					Append(IToS(time.Month)).
					Append("月").
					Append(IToS(time.Day)).Append("日");
			return builder.ToString();
		}
		return EMPTY;
	}
}