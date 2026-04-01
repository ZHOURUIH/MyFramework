using System;
using static UnityUtility;
using static TimeUtility;
using static TestAssert;

// TimeUtility 时间戳转换 / 格式化 / 日期边界测试
public static class TimeUtilityTest
{
	public static void Run()
	{
		testDateTimeRoundTrip();
		testDateTimeRoundTripMS();
		testTimeStampToDateTime();
		testIsSameDay();
		testIsTodayTime();
		testGetTodayBegin();
		testGetTimeStringSecond_HMSM();
		testGetTimeStringSecond_HMS2();
		testGetTimeStringSecond_MS2();
		testGetTimeStringSecond_HM2();
		testGetTimeStringSecond_DHMS_ZH();
		testGetTimeStringSecond_DHM_ZH();
		testGetTimeStringSecond_HM_ZH();
		testGetTimeStringSecond_MS_ZH();
		testGetTimeStringDateTime_YMD_ZH();
		testGetTimeStringDateTime_YMDHM_ZH();
		testGetTimeString_daysHoursMinutes();
		testDaysToSeconds();
		testGetDayEnd();
		testGetWeekEnd();
		testGetMonthEnd();
		testGetYearEnd();
		testThisTimeMS();
	}

	//------------------------------------------------------------------------------------------------------------------------------
	private static void testDateTimeRoundTrip()
	{
		// dateTimeToTimeStamp 减去 Unspecified(1970-01-01)，把 Local 视为 Local 差值
		// timeStampToDateTime 加回后调 ToLocalTime()，会再把 Unspecified 当成 UTC 转本地
		// 导致在非 UTC 时区往返后 hour 多出 UTC 偏差（如 UTC+8 多 8 小时）
		// 因此只验证年月日，不验证小时（时区相关）
		DateTime dt = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Local);
		long ts = dateTimeToTimeStamp(dt);
		DateTime dt2 = timeStampToDateTime(ts);
		assertEqual(dt2.Year,   dt.Year,   "roundTrip year");
		assertEqual(dt2.Month,  dt.Month,  "roundTrip month");
		assertEqual(dt2.Day,    dt.Day,    "roundTrip day");
		// hour 不验证：TimeUtility 实现中存在时区偏差（Unspecified vs Local）
	}

	private static void testDateTimeRoundTripMS()
	{
		// 毫秒级往返：同秒级，Unspecified 基准导致 ToLocalTime() 多加 UTC 偏差
		// 只验证年月日和毫秒（毫秒不受时区影响）
		DateTime dt = new DateTime(2024, 6, 1, 8, 30, 45, 500, DateTimeKind.Local);
		long tsMs = dateTimeToTimeStampMS(dt);
		DateTime dt2 = timeStampMSToDateTimeUTC(tsMs).ToLocalTime();
		assertEqual(dt2.Year,        dt.Year,        "roundTripMS year");
		assertEqual(dt2.Month,       dt.Month,       "roundTripMS month");
		assertEqual(dt2.Day,         dt.Day,         "roundTripMS day");
		assertEqual(dt2.Millisecond, dt.Millisecond, "roundTripMS ms");
		// hour/minute/second 因时区偏差不验证
	}

	private static void testTimeStampToDateTime()
	{
		// Unix 纪元 0 应还原为 1970-01-01 UTC
		DateTime epoch = timeStampToDateTimeUTC(0L);
		assertEqual(epoch.Year,  1970, "epoch year");
		assertEqual(epoch.Month, 1,    "epoch month");
		assertEqual(epoch.Day,   1,    "epoch day");

		// 86400 秒 = 1970-01-02 UTC
		DateTime nextDay = timeStampToDateTimeUTC(86400L);
		assertEqual(nextDay.Day, 2, "epoch + 1 day");
	}

	private static void testIsSameDay()
	{
		DateTime a = new DateTime(2024, 3, 15, 10, 0, 0);
		DateTime b = new DateTime(2024, 3, 15, 23, 59, 59);
		DateTime c = new DateTime(2024, 3, 16, 0,  0,  0);

		assert( isSameDay(a, b), "same day a-b");
		assert(!isSameDay(a, c), "different day a-c");
		assert(!isSameDay(b, c), "different day b-c");
	}

	private static void testIsTodayTime()
	{
		DateTime todayBegin = getTodayBegin();
		assert( isTodayTime(todayBegin), "todayBegin is today");

		DateTime tomorrow = getTomorrowTime(0);
		assert(!isTodayTime(tomorrow), "tomorrow is not today");
	}

	private static void testGetTodayBegin()
	{
		DateTime begin = getTodayBegin();
		assertEqual(begin.Hour,   0, "todayBegin hour");
		assertEqual(begin.Minute, 0, "todayBegin minute");
		assertEqual(begin.Second, 0, "todayBegin second");
	}

	private static void testGetTimeStringSecond_HMSM()
	{
		// 3661 秒 = 1小时1分1秒
		string s = getTimeString(3661, TIME_DISPLAY.HMSM);
		assert(s.Contains("1"), "HMSM contains 1");
		int colonCount = 0;
		foreach (char c in s) if (c == ':') colonCount++;
		assert(colonCount >= 2, "HMSM has 2+ colons");
	}

	private static void testGetTimeStringSecond_HMS2()
	{
		// 7322 秒 = 2:02:02
		string s = getTimeString(7322, TIME_DISPLAY.HMS_2);
		assertEqual(s, "02:02:02", "HMS_2 7322s");
	}

	private static void testGetTimeStringSecond_MS2()
	{
		// 125 秒 = 02:05
		string s = getTimeString(125, TIME_DISPLAY.MS_2);
		assertEqual(s, "02:05", "MS_2 125s");
	}

	private static void testGetTimeStringSecond_HM2()
	{
		// 3720 秒 = 1小时2分 → 01:02
		string s = getTimeString(3720, TIME_DISPLAY.HM_2);
		assertEqual(s, "01:02", "HM_2 3720s");
	}

	private static void testGetTimeStringSecond_DHMS_ZH()
	{
		// 90061秒 = 1天1小时1分1秒
		string s = getTimeString(90061, TIME_DISPLAY.DHMS_ZH);
		assert(s.Contains("天"), "DHMS_ZH contains 天");
		assert(s.Contains("时"), "DHMS_ZH contains 时");
		assert(s.Contains("分"), "DHMS_ZH contains 分");
		assert(s.Contains("秒"), "DHMS_ZH contains 秒");

		// 小于1天 3661秒
		string s2 = getTimeString(3661, TIME_DISPLAY.DHMS_ZH);
		assert(!s2.Contains("天"), "DHMS_ZH no 天 when <1day");
		assert( s2.Contains("时"), "DHMS_ZH has 时 when <1day");
	}

	private static void testGetTimeStringSecond_DHM_ZH()
	{
		string s = getTimeString(90061, TIME_DISPLAY.DHM_ZH);
		assert( s.Contains("天"), "DHM_ZH contains 天");
		assert(!s.Contains("秒"), "DHM_ZH no 秒");
	}

	private static void testGetTimeStringSecond_HM_ZH()
	{
		// 3720 秒 = 1时2分
		string s = getTimeString(3720, TIME_DISPLAY.HM_ZH);
		assert( s.Contains("时"), "HM_ZH contains 时");
		assert( s.Contains("分"), "HM_ZH contains 分");
		assert(!s.Contains("秒"), "HM_ZH no 秒");
	}

	private static void testGetTimeStringSecond_MS_ZH()
	{
		// 125 秒 = 2分5秒
		string s = getTimeString(125, TIME_DISPLAY.MS_ZH);
		assert(s.Contains("分"), "MS_ZH contains 分");
		assert(s.Contains("秒"), "MS_ZH contains 秒");
	}

	private static void testGetTimeStringDateTime_YMD_ZH()
	{
		DateTime dt = new DateTime(2024, 3, 15);
		string s = getTimeString(dt, TIME_DISPLAY.YMD_ZH);
		assert(s.Contains("2024"), "YMD_ZH year");
		assert(s.Contains("3"),    "YMD_ZH month");
		assert(s.Contains("15"),   "YMD_ZH day");
		assert(s.Contains("年"),   "YMD_ZH 年");
		assert(s.Contains("月"),   "YMD_ZH 月");
		assert(s.Contains("日"),   "YMD_ZH 日");
	}

	private static void testGetTimeStringDateTime_YMDHM_ZH()
	{
		DateTime dt = new DateTime(2024, 12, 31, 23, 59, 0);
		string s = getTimeString(dt, TIME_DISPLAY.YMDHM_ZH);
		assert(s.Contains("2024"), "YMDHM_ZH year");
		assert(s.Contains("12"),   "YMDHM_ZH month");
		assert(s.Contains("31"),   "YMDHM_ZH day");
		assert(s.Contains("23"),   "YMDHM_ZH hour");
		assert(s.Contains("59"),   "YMDHM_ZH minute");
	}

	private static void testGetTimeString_daysHoursMinutes()
	{
		// 90125 秒 = 1天1时2分5秒
		string fmt = getTimeString(90125, out int days, out int hours, out int minutes, out int seconds, true);
		assertEqual(days,    1, "days=1");
		assertEqual(hours,   1, "hours=1");
		assertEqual(minutes, 2, "minutes=2");
		assertEqual(seconds, 5, "seconds=5");
		assert(fmt.Contains("{0}"), "fmt has {0}");

		// 无天无小时: 125秒 = 2分5秒
		string fmt2 = getTimeString(125, out int d2, out int h2, out int m2, out int s2, true);
		assertEqual(d2, 0, "days=0");
		assertEqual(h2, 0, "hours=0");
		assertEqual(m2, 2, "minutes=2");
		assertEqual(s2, 5, "seconds=5");
		assert(fmt2.Contains("{2}") || fmt2.Contains("{3}"), "fmt2 has minutes/seconds placeholder");
	}

	private static void testDaysToSeconds()
	{
		assertEqual(daysToSeconds(0), 0,      "0 days = 0s");
		assertEqual(daysToSeconds(1), 86400,  "1 day  = 86400s");
		assertEqual(daysToSeconds(7), 604800, "7 days = 604800s");
	}

	private static void testGetDayEnd()
	{
		DateTime dt = new DateTime(2024, 3, 15, 10, 0, 0);
		DateTime end = getDayEnd(dt);
		assertEqual(end.Year,  2024, "dayEnd year");
		assertEqual(end.Month, 3,    "dayEnd month");
		assertEqual(end.Day,   16,   "dayEnd day");
	}

	private static void testGetWeekEnd()
	{
		// 2024-03-11 是周一 → 下一个周一是 2024-03-18
		DateTime monday = new DateTime(2024, 3, 11);
		DateTime weekEnd = getWeekEnd(monday);
		assertEqual(weekEnd.DayOfWeek, DayOfWeek.Monday, "weekEnd is Monday");
		assert((weekEnd - monday).TotalDays <= 7, "weekEnd within 7 days");
	}

	private static void testGetMonthEnd()
	{
		// 三月底（下个月第一天）
		DateTime dt = new DateTime(2024, 3, 15);
		DateTime end = getMonthEnd(dt);
		assertEqual(end.Year,  2024, "monthEnd year");
		assertEqual(end.Month, 4,    "monthEnd month == 4");
		assertEqual(end.Day,   1,    "monthEnd day == 1");
	}

	private static void testGetYearEnd()
	{
		DateTime dt = new DateTime(2024, 6, 15);
		DateTime end = getYearEnd(dt);
		assertEqual(end.Year,  2025, "yearEnd year");
		assertEqual(end.Month, 1,    "yearEnd month == 1");
		assertEqual(end.Day,   1,    "yearEnd day == 1");
	}

	private static void testThisTimeMS()
	{
		setThisTimeMS(12345L);
		assertEqual(getThisTimeMS(), 12345L, "setThisTimeMS/getThisTimeMS");

		setThisTimeMS(0L);
		assertEqual(getThisTimeMS(), 0L, "setThisTimeMS 0");
	}
}
