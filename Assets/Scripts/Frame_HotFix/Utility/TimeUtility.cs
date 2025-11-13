using System;
using System.Text;
using static StringUtility;
using static MathUtility;

// 时间工具函数,由于与时间的相关操作比较多,所以单独写到时间工具类中
public class TimeUtility
{
	private static DateTime mTime19700101 = new(1970, 1, 1);	// 时间的起始
	private static long mThisTimeMS;							// 这一帧的时间戳,每一帧设置一次,方便与elapsedTime搭配使用
	// 获取从1970年1月1日到现在所经过的毫秒数
	public static long timeGetTime() { return (long)(DateTime.Now - mTime19700101).TotalMilliseconds; }
	// 获取从1970年1月1日到现在所经过的秒数
	public static long getTimeSecond() { return (long)(DateTime.Now - mTime19700101).TotalSeconds; }
	// 获取从1970年1月1日到当前UTC时间(世界标准时间)所经过的毫秒数
	public static long timeGetTimeUTC() { return (long)(DateTime.UtcNow - mTime19700101).TotalMilliseconds; }
	// 获取从1970年1月1日到当前UTC时间(世界标准时间)所经过的秒数
	public static long getTimeSecondUTC() { return (long)(DateTime.UtcNow - mTime19700101).TotalSeconds; }
	// GameFramework每一帧设置一次
	public static void setThisTimeMS(long time) { mThisTimeMS = time; }
	// 获取这一帧的时间戳,时间戳在这一帧内都不变,比getNowTimeStampMS效率高一些
	public static long getThisTimeMS() { return mThisTimeMS; }
	public static string getTimeNoLock(TIME_DISPLAY display) { return getTimeStringNoLock(DateTime.Now, display); }
	// 获得当前时间的字符串,display表示显示的格式,仅可在主线程中调用
	public static string getNowTime(TIME_DISPLAY display) { return getTimeString(DateTime.Now, display); }
	// 获得当前时间的字符串,display表示显示的格式
	public static string getTimeNoBuilder(TIME_DISPLAY display) { return getTimeStringNoBuilder(DateTime.Now, display); }
	// timeStamp是UTC时间戳,显示为UTC时间
	public static string getDateTimeToUTC(long utcTimeStamp, TIME_DISPLAY display) { return getTimeString(timeStampToDateTimeUTC(utcTimeStamp), display); }
	// timeStamp是UTC时间戳,会转换为本地时间来显示
	public static string getLocalTime(long utcTimeStamp, TIME_DISPLAY display) { return getTimeString(timeStampToDateTime(utcTimeStamp), display); }
	// 将时间转化成时间戳,dateTime是本地时间
	public static long dateTimeToTimeStamp(DateTime dateTime) { return (long)(dateTime - mTime19700101).TotalSeconds; }
	// 将时间转化成时间戳,dateTime是本地时间
	public static long dateTimeToTimeStampMS(DateTime dateTime) { return (long)(dateTime - mTime19700101).TotalMilliseconds; }
	// 将时间戳转化成时间,转换后是本地时间
	public static DateTime timeStampToDateTime(long utcTimeStamp) { return mTime19700101.AddSeconds(utcTimeStamp).ToLocalTime(); }
	// 将时间戳转化成时间,转换后是utc时间
	public static DateTime timeStampToDateTimeUTC(long utcTimeStamp) { return mTime19700101.AddSeconds(utcTimeStamp); }
	// 将时间戳转化成时间,转换后是utc时间
	public static DateTime timeStampMSToDateTimeUTC(long utcTimeStampMS) { return mTime19700101.AddMilliseconds(utcTimeStampMS); }
	// 获得当前的本地时间戳,以秒为单位
	public static long getNowTimeStamp() { return dateTimeToTimeStamp(DateTime.Now); }
	// 获得当前的本地时间戳,以毫秒为单位
	public static long getNowTimeStampMS() { return dateTimeToTimeStampMS(DateTime.Now); }
	// 获得当前的UTC时间戳,以秒为单位
	public static long getNowUTCTimeStamp() { return dateTimeToTimeStamp(DateTime.UtcNow); }
	// 获得当前的UTC时间戳,以毫秒为单位
	public static long getNowUTCTimeStampMS() { return dateTimeToTimeStampMS(DateTime.UtcNow); }
	// 判断两个时间戳是否在同一天,time为localtime
	public static bool isSameDay(long utcTimeStamp0, long utcTimeStamp1)
	{
		return isSameDay(timeStampToDateTime(utcTimeStamp0), timeStampToDateTime(utcTimeStamp1));
	}
	// 判断两个时间是否在同一天
	public static bool isSameDay(DateTime date0, DateTime date1)
	{
		return date0.Year == date1.Year &&
			   date0.Month == date1.Month &&
			   date0.Day == date1.Day;
	}
	// 判断指定时间是否在今天,time为localtime
	public static bool isTodayTime(long utcTimeStamp) { return isSameDay(timeStampToDateTime(utcTimeStamp), DateTime.Now); }
	// 判断指定时间是否在今天
	public static bool isTodayTime(DateTime date) { return isSameDay(date, DateTime.Now); }
	// 获取今天的时间,如果hour为0,就是今天的凌晨0点
	public static DateTime getTodayTime(int hour, int minute = 0, int second = 0)
	{
		DateTime now = DateTime.Now;
		return new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
	}
	// 获取今天开始的时间
	public static DateTime getTodayBegin()
	{
		return getTodayTime(0);
	}
	// 获取今天开始时间的时间戳
	public static long getTodayBeginTimeStamp()
	{
		return dateTimeToTimeStamp(getTodayBegin());
	}
	// 获得明天的时间,如果hour为0,就是今天的晚上12点
	public static DateTime getTomorrowTime(int hour, int minute = 0, int second = 0)
	{
		DateTime now = DateTime.Now.AddDays(1);
		return new DateTime(now.Year, now.Month, now.Day, hour, minute, second);
	}
	// 获取从现在到晚上12点的时间差
	public static TimeSpan getTimeToTodayEnd()  { return getTomorrowTime(0) - DateTime.Now; }
	// 获取从现在到晚上12点的时间差秒数
	public static int getSecondsToTodayEnd()  { return (int)getTimeToTodayEnd().TotalSeconds;  }
	// 获取一定天数的秒数
	public static int daysToSeconds(int days) { return 24 * 60 * 60 * days; }
	public static string minuteToHourMinuteString(int totalMinute)
	{
		minuteToHourMinute(totalMinute, out int hour, out int minute);
		using var a = new MyStringBuilderScope(out var timeStr);
		if (hour > 0)
		{
			timeStr.append(IToS(hour), "小时");
		}
		if (minute > 0)
		{
			timeStr.append(IToS(minute), "分钟");
		}
		return timeStr.ToString();
	}
	// 一般用于倒计时显示的字符串,只获取数字,自己拼接需要显示的字符串,适用于需要切换多语言的文本
	public static string getTimeString(int timeSecond, out int days, out int hours, out int minutes, out int seconds, bool needSecond)
	{
		int totalMin = timeSecond / 60;
		int totalHour = totalMin / 60;
		int totalDay = totalHour / 24;
		int curHour = totalHour % 24;
		int curMin = totalMin % 60;
		int curSecond = timeSecond % 60;
		days = totalDay;
		hours = curHour;
		minutes = curMin;
		seconds = curSecond;
		string returnStr;
		if (totalDay > 0)
		{
			returnStr = "{0}天{1}时{2}分";
		}
		// 小于1天,并且大于等于1小时
		else if (totalHour > 0)
		{
			returnStr = "{1}时{2}分";
		}
		// 小于1小时,并且大于等于1分钟
		else if (totalMin > 0)
		{
			returnStr = "{2}分";
		}
		else
		{
			// 小于一分钟时，假如不显示秒，则固定返回1分
			returnStr = needSecond ? "{3}秒" : "1分";
			return returnStr;
		}
		return returnStr + (needSecond ? "{3}秒" : EMPTY);
	}
	// 一般用于倒计时显示的字符串,只获取数字,自己拼接需要显示的字符串,适用于需要切换多语言的文本
	public static string getTimeString(int timeSecond, out int days, out int hours, out int minutes)
	{
		int totalMin = timeSecond / 60;
		int totalHour = totalMin / 60;
		int totalDay = totalHour / 24;
		int curHour = totalHour % 24;
		int curMin = totalMin % 60;
		days = totalDay;
		hours = curHour;
		minutes = curMin;
		if (totalDay > 0)
		{
			return "{0}天{1}时{2}分";
		}
		// 小于1天,并且大于等于1小时
		else if (totalHour > 0)
		{
			return "{1}时{2}分";
		}
		return "{2}分";
	}
	// 一般用于倒计时显示的字符串
	public static string getTimeString(int timeSecond, TIME_DISPLAY display)
	{
		int hour = timeSecond / 3600;
        int min = (timeSecond - hour * 3600) / 60;
		int second = timeSecond - hour * 3600 - min * 60;
		if (display == TIME_DISPLAY.HMSM)
		{
			return strcat(IToS(hour), ":", IToS(min), ":", IToS(second));
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			return strcat(IToS(hour, 2), ":", IToS(min, 2), ":", IToS(second, 2));
		}
		else if (display == TIME_DISPLAY.MS_2)
		{
			return IToS(min + hour * 60, 2) + ":" + IToS(second, 2);
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
				return strcat(IToS(totalDay), "天", IToS(curHour), "时", IToS(curMin), "分", IToS(curSecond), "秒");
			}
			// 小于1天,并且大于等于1小时
			else if (totalHour > 0)
			{
				return strcat(IToS(totalHour), "时", IToS(curMin), "分", IToS(curSecond), "秒");
			}
			// 小于1小时,并且大于等于1分钟
			else if (totalMin > 0)
			{
				return IToS(totalMin) + "分" + IToS(curSecond) + "秒";
			}
			return timeSecond + "秒";
		}
		else if (display == TIME_DISPLAY.DHM_ZH)
		{
			int totalMin = timeSecond / 60;
			int totalHour = totalMin / 60;
			int totalDay = totalHour / 24;
			int curHour = totalHour % 24;
			int curMin = totalMin % 60;
			// 大于等于1天
			if (totalDay > 0)
			{
				return strcat(IToS(totalDay), "天", IToS(curHour), "时", IToS(curMin), "分");
			}
			// 小于1天,并且大于等于1小时
			else if (totalHour > 0)
			{
				return IToS(totalHour) + "时" + IToS(curMin) + "分";
			}
			return IToS(totalMin) + "分";
		}
		else if (display == TIME_DISPLAY.HM_ZH)
		{
			int totalMin = timeSecond / 60;
			int totalHour = totalMin / 60;
			int curMin = totalMin % 60;
			// 小于1天,并且大于等于1小时
			if (totalHour > 0)
			{
				return IToS(totalHour) + "时" + IToS(curMin) + "分";
			}
			return IToS(totalMin) + "分";
		}
		return EMPTY;
	}
	// 只能在主线程中调用的获取当前时间字符串
	public static string getTimeString(DateTime time, TIME_DISPLAY display)
	{
		if (display == TIME_DISPLAY.HMSM)
		{
			return strcat(IToS(time.Hour), ":", IToS(time.Minute), ":", IToS(time.Second), ":", IToS(time.Millisecond));
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			return strcat(IToS(time.Hour, 2), ":", IToS(time.Minute, 2), ":", IToS(time.Second, 2));
		}
		else if (display == TIME_DISPLAY.MS_2)
		{
			return IToS(time.Minute, 2) + ":" + IToS(time.Second, 2);
		}
		else if (display == TIME_DISPLAY.DHMS_ZH)
		{
			return strcat(IToS(time.Day), "日", IToS(time.Hour), "时", IToS(time.Minute), "分", IToS(time.Second), "秒");
		}
		else if (display == TIME_DISPLAY.DHM_ZH)
		{
			return strcat(IToS(time.Day), "日", IToS(time.Hour), "时", IToS(time.Minute), "分");
		}
		else if (display == TIME_DISPLAY.HM_ZH)
		{
			return IToS(time.Hour) + "时" + IToS(time.Minute) + "分";
		}
		else if (display == TIME_DISPLAY.YMD_ZH)
		{
			return strcat(IToS(time.Year), "年", IToS(time.Month), "月", IToS(time.Day), "日");
		}
		else if(display == TIME_DISPLAY.YMDHM_ZH)
		{
			return strcat(IToS(time.Year), "年", IToS(time.Month), "月", IToS(time.Day), "日", IToS(time.Hour), "时", IToS(time.Minute), "分");
		}
		return EMPTY;
	}
	public static string getTimeStringNoLock(DateTime time, TIME_DISPLAY display)
	{
		if (display == TIME_DISPLAY.HMSM)
		{
			return IToS(time.Hour) + ":" + IToS(time.Minute) + ":" + IToS(time.Second) + ":" + IToS(time.Millisecond);
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			return IToS(time.Hour, 2) + ":" + IToS(time.Minute, 2) + ":" + IToS(time.Second, 2);
		}
		else if (display == TIME_DISPLAY.MS_2)
		{
			return IToS(time.Minute, 2) + ":" + IToS(time.Second, 2);
		}
		else if (display == TIME_DISPLAY.DHMS_ZH)
		{
			return IToS(time.Day) + "日" + IToS(time.Hour) + "时" + IToS(time.Minute) + "分" + IToS(time.Second) + "秒";
		}
		else if (display == TIME_DISPLAY.DHM_ZH)
		{
			return IToS(time.Day) + "日" + IToS(time.Hour) + "时" + IToS(time.Minute) + "分";
		}
		else if (display == TIME_DISPLAY.HM_ZH)
		{
			return IToS(time.Hour) + "时" + IToS(time.Minute) + "分";
		}
		else if (display == TIME_DISPLAY.YMD_ZH)
		{
			return IToS(time.Year) + "年" + IToS(time.Month) + "月" + IToS(time.Day) + "日";
		}
		else if (display == TIME_DISPLAY.YMDHM_ZH)
		{
			return IToS(time.Year) + "年" + IToS(time.Month) + "月" + IToS(time.Day) + "日" + IToS(time.Hour) + "时" + IToS(time.Minute) + "分";
		}
		return EMPTY;
	}
	// 不使用自定义的字符串拼接器,使用内置的StringBuilder进行拼接
	public static string getTimeStringNoBuilder(DateTime time, TIME_DISPLAY display)
	{
		StringBuilder builder = new(256);
		if (display == TIME_DISPLAY.HMSM)
		{
			builder.Append(IToS(time.Hour)).
					Append(":").Append(IToS(time.Minute)).
					Append(":").Append(IToS(time.Second)).
					Append(":").Append(IToS(time.Millisecond));
			return builder.ToString();
		}
		else if (display == TIME_DISPLAY.HMS_2)
		{
			builder.Append(IToS(time.Hour, 2)).
					Append(":").Append(IToS(time.Minute, 2)).
					Append(":").Append(IToS(time.Second, 2));
			return builder.ToString();
		}
		else if (display == TIME_DISPLAY.MS_2)
		{
			builder.Append(":").Append(IToS(time.Minute, 2)).
					Append(":").Append(IToS(time.Second, 2));
			return builder.ToString();
		}
		else if (display == TIME_DISPLAY.DHMS_ZH)
		{
			builder.Append(IToS(time.Day)).Append("日").
					Append(IToS(time.Hour)).Append("时").
					Append(IToS(time.Minute)).Append("分").
					Append(IToS(time.Second)).Append("秒");
			return builder.ToString();
		}
		else if (display == TIME_DISPLAY.DHM_ZH)
		{
			builder.Append(IToS(time.Day)).Append("日").
					Append(IToS(time.Hour)).Append("时").
					Append(IToS(time.Minute)).Append("分");
			return builder.ToString();
		}
		else if (display == TIME_DISPLAY.HM_ZH)
		{
			builder.Append(IToS(time.Hour)).Append("时").
					Append(IToS(time.Minute)).Append("分");
			return builder.ToString();
		}
		else if (display == TIME_DISPLAY.YMD_ZH)
		{
			builder.Append(IToS(time.Year)).Append("年").
					Append(IToS(time.Month)).Append("月").
					Append(IToS(time.Day)).Append("日");
			return builder.ToString();
		}
		else if (display == TIME_DISPLAY.YMDHM_ZH)
		{
			builder.Append(IToS(time.Year)).Append("年").
					Append(IToS(time.Month)).Append("月").
					Append(IToS(time.Day)).Append("日").
					Append(IToS(time.Hour)).Append("时").
					Append(IToS(time.Minute)).Append("分");
			return builder.ToString();
		}
		return EMPTY;
	}
	public static DateTime getDayEnd(long utcTimeStamp) { return timeStampToDateTime(utcTimeStamp).AddDays(1); }
	public static DateTime getDayEnd() { return getDayEnd(DateTime.Today); }
	public static DateTime getDayEnd(DateTime dateTime) { return dateTime.AddDays(1); }
	public static DateTime getWeekEnd() { return getWeekEnd(DateTime.Today); }
	public static DateTime getWeekEnd(long utcTimeStamp) { return getWeekEnd(timeStampToDateTime(utcTimeStamp)); }
	public static DateTime getWeekEnd(DateTime dateTime)
	{
		if (dateTime.DayOfWeek == 0)
		{
			return dateTime.AddDays(1);
		}
		return dateTime.AddDays(7 - (int)dateTime.DayOfWeek + 1);
	}
	public static DateTime getMonthEnd() { return getMonthEnd(DateTime.Today); }
	public static DateTime getMonthEnd(long utcTimeStamp) { return getMonthEnd(timeStampToDateTime(utcTimeStamp)); }
	public static DateTime getMonthEnd(DateTime dateTime) { return new DateTime(dateTime.Year, dateTime.Month, 1).AddMonths(1); }
	public static DateTime getYearEnd() { return getYearEnd(DateTime.Today); }
	public static DateTime getYearEnd(long utcTimeStamp) { return getYearEnd(timeStampToDateTime(utcTimeStamp)); }
	public static DateTime getYearEnd(DateTime dateTime) { return new DateTime(dateTime.Year + 1, 1, 1); }
	public static int getTodayEndRemain() { return (int)(getDayEnd() - DateTime.Now).TotalSeconds; }
	public static int getWeekEndRemain() { return (int)(getWeekEnd() - DateTime.Now).TotalSeconds; }
	public static int getMonthEndRemain() { return (int)(getMonthEnd() - DateTime.Now).TotalSeconds; }
	public static int getYearEndRemain() { return (int)(getYearEnd() - DateTime.Now).TotalSeconds; }
}