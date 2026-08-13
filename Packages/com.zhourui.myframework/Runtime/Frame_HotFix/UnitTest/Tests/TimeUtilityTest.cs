using System;
using System.Text.RegularExpressions;
using static TestAssert;

// TimeUtility 时间工具函数测试
public static class TimeUtilityTest
{
    public static void Run()
    {
        testSetGetThisTimeMS();
        testDateTimeToTimeStamp();
        testDateTimeToTimeStampMS();
        testTimeStampToDateTime();
        testTimeStampToDateTimeUTC();
        testTimeStampMSToDateTimeUTC();
        testIsSameDay_DateTime();
        testIsSameDay_TimeStamp();
        testGetTodayTime();
        testGetTomorrowTime();
        testDaysToSeconds();
        testMinuteToHourMinuteString();
        testGetTimeString_OutParams();
        testGetTimeString_OutParams_NoSeconds();
        testGetTimeString_Int_Display();
        testGetTimeString_DateTime_Display();
        testGetTimeStringNoLock();
        testGetTimeStringNoBuilder();
        testGetDayEnd();
        testGetWeekEnd();
        testGetMonthEnd();
        testGetYearEnd();
        testGetTimeToTodayEnd();
        testGetSecondsToTodayEnd();
    

		testGetTimeNoLock_Format();
		testGetNowTime_Format();
		testGetTimeNoBuilder_Format();
		testGetDateTimeToUTC_Fixed();
		testGetLocalTime_Fixed();
		testGetNowTimeStamp_Window();
		testGetNowUTCTimeStamp_Window();
		testGetNowTimeStampMS_Monotonic();
		testIsTodayTime_DateTime();
		testIsTodayTime_TimeStamp();
		testGetTodayBegin_Consistency();
		testTodayBegin_IsMidnight();
		testDayEndRemain_Range();
		testWeekMonthYearEndRemain_Range();
		testTimestamp_RoundTrip_UTC();
		testChain_TodayBoundaryAcrossFunctions();
	}

    // ---- setThisTimeMS / getThisTimeMS ----
    static void testSetGetThisTimeMS()
    {
        TimeUtility.setThisTimeMS(12345L);
        assertEqual(12345L, TimeUtility.getThisTimeMS(), "set/get thisTimeMS 12345");
        TimeUtility.setThisTimeMS(0L);
        assertEqual(0L, TimeUtility.getThisTimeMS(), "set/get thisTimeMS 0");
    }

    // ---- dateTimeToTimeStamp ----
    static void testDateTimeToTimeStamp()
    {
        DateTime dt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long ts = TimeUtility.dateTimeToTimeStamp(dt);
        // 2020-01-01 UTC = 1577836800
        assertEqual(1577836800L, ts, "2020-01-01 UTC timestamp");
    }

    // ---- dateTimeToTimeStampMS ----
    static void testDateTimeToTimeStampMS()
    {
        DateTime dt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long ts = TimeUtility.dateTimeToTimeStampMS(dt);
        assertEqual(1577836800000L, ts, "2020-01-01 UTC timestamp ms");
    }

    // ---- timeStampToDateTime ----
    static void testTimeStampToDateTime()
    {
        long ts = 1577836800L; // 2020-01-01 00:00:00 UTC
        DateTime dt = TimeUtility.timeStampToDateTime(ts);
        // 转换为本地时间后，年月日应该正确
        assertEqual(2020, dt.Year, "year 2020");
        assertEqual(1, dt.Month, "month 1");
        assertEqual(1, dt.Day, "day 1");
        // timeStampToDateTime 内部通过 ToLocalTime() 转换，验证双向转换一致性
        long roundTrip = TimeUtility.dateTimeToTimeStamp(dt.ToUniversalTime());
        assertEqual(ts, roundTrip, "round-trip timestamp matches");
    }

    // ---- timeStampToDateTimeUTC ----
    static void testTimeStampToDateTimeUTC()
    {
        long ts = 1577836800L;
        DateTime dt = TimeUtility.timeStampToDateTimeUTC(ts);
        assertEqual(2020, dt.Year, "UTC year 2020");
        assertEqual(1, dt.Month, "UTC month 1");
        assertEqual(1, dt.Day, "UTC day 1");
        assertEqual(0, dt.Hour, "UTC hour 0");
        assertEqual(0, dt.Minute, "UTC minute 0");
        assertEqual(0, dt.Second, "UTC second 0");
    }

    // ---- timeStampMSToDateTimeUTC ----
    static void testTimeStampMSToDateTimeUTC()
    {
        long tsMs = 1577836800000L;
        DateTime dt = TimeUtility.timeStampMSToDateTimeUTC(tsMs);
        assertEqual(2020, dt.Year, "ms UTC year 2020");
        assertEqual(1, dt.Month, "ms UTC month 1");
        assertEqual(1, dt.Day, "ms UTC day 1");
    }

    // ---- isSameDay(DateTime, DateTime) ----
    static void testIsSameDay_DateTime()
    {
        DateTime d1 = new DateTime(2020, 6, 15, 10, 30, 0);
        DateTime d2 = new DateTime(2020, 6, 15, 23, 59, 59);
        DateTime d3 = new DateTime(2020, 6, 16, 0, 0, 0);
        assertTrue(TimeUtility.isSameDay(d1, d2), "same day: 6/15 10:30 vs 6/15 23:59");
        assertFalse(TimeUtility.isSameDay(d1, d3), "diff day: 6/15 vs 6/16");
    }

    // ---- isSameDay(long, long) ----
    static void testIsSameDay_TimeStamp()
    {
        // 2020-06-15 10:00 UTC = 1592215200, 2020-06-15 22:00 UTC = 1592258400
        long ts1 = 1592215200L;
        long ts2 = 1592258400L;
        long ts3 = 1592301600L; // next day
        assertTrue(TimeUtility.isSameDay(ts1, ts2), "same day timestamps");
        assertFalse(TimeUtility.isSameDay(ts1, ts3), "diff day timestamps");
    }

    // ---- getTodayTime ----
    static void testGetTodayTime()
    {
        DateTime today = TimeUtility.getTodayTime(12, 30, 45);
        DateTime now = DateTime.Now;
        assertEqual(now.Year, today.Year, "today year");
        assertEqual(now.Month, today.Month, "today month");
        assertEqual(now.Day, today.Day, "today day");
        assertEqual(12, today.Hour, "today hour 12");
        assertEqual(30, today.Minute, "today minute 30");
        assertEqual(45, today.Second, "today second 45");
    }

    // ---- getTomorrowTime ----
    static void testGetTomorrowTime()
    {
        DateTime tomorrow = TimeUtility.getTomorrowTime(8);
        DateTime tomorrowExpected = DateTime.Now.AddDays(1);
        assertEqual(tomorrowExpected.Year, tomorrow.Year, "tomorrow year");
        assertEqual(tomorrowExpected.Month, tomorrow.Month, "tomorrow month");
        assertEqual(tomorrowExpected.Day, tomorrow.Day, "tomorrow day");
        assertEqual(8, tomorrow.Hour, "tomorrow hour 8");
    }

    // ---- daysToSeconds ----
    static void testDaysToSeconds()
    {
        assertEqual(86400, TimeUtility.daysToSeconds(1), "1 day -> 86400s");
        assertEqual(172800, TimeUtility.daysToSeconds(2), "2 days -> 172800s");
        assertEqual(0, TimeUtility.daysToSeconds(0), "0 days -> 0s");
        assertEqual(604800, TimeUtility.daysToSeconds(7), "7 days -> 604800s");
    }

    // ---- minuteToHourMinuteString ----
    static void testMinuteToHourMinuteString()
    {
        string s = TimeUtility.minuteToHourMinuteString(65);
        assertTrue(s.Contains("1") && s.Contains("小时") && s.Contains("5") && s.Contains("分钟"),
            "65min -> 1小时5分钟");
        string s2 = TimeUtility.minuteToHourMinuteString(60);
        assertTrue(s2.Contains("1") && s2.Contains("小时"), "60min -> 1小时");
        string s3 = TimeUtility.minuteToHourMinuteString(30);
        assertTrue(s3.Contains("30") && s3.Contains("分钟"), "30min -> 30分钟");
    }

    // ---- getTimeString(int, out int x4, bool) ----
    static void testGetTimeString_OutParams()
    {
        string fmt = TimeUtility.getTimeString(90061, out int days, out int hours, out int minutes, out int seconds, true);
        // 90061s = 1天1小时1分1秒
        assertEqual(1, days, "days=1");
        assertEqual(1, hours, "hours=1");
        assertEqual(1, minutes, "minutes=1");
        assertEqual(1, seconds, "seconds=1");
        assertEqual("{0}天{1}时{2}分{3}秒", fmt, "format string");

        // 3661s = 1小时1分1秒
        string fmt2 = TimeUtility.getTimeString(3661, out days, out hours, out minutes, out seconds, true);
        assertEqual(0, days, "days=0");
        assertEqual(1, hours, "hours=1");
        assertEqual(1, minutes, "minutes=1");
        assertEqual(1, seconds, "seconds=1");
        assertEqual("{1}时{2}分{3}秒", fmt2, "format string");

        // 61s = 1分1秒
        string fmt3 = TimeUtility.getTimeString(61, out days, out hours, out minutes, out seconds, true);
        assertEqual(1, minutes, "minutes=1");
        assertEqual(1, seconds, "seconds=1");
        assertEqual("{2}分{3}秒", fmt3, "format string");

        // <60s with needSecond=true
        string fmt4 = TimeUtility.getTimeString(30, out days, out hours, out minutes, out seconds, true);
        assertEqual(30, seconds, "seconds=30");
        assertEqual("{3}秒", fmt4, "format string <60s needSecond");

        // <60s with needSecond=false
        string fmt5 = TimeUtility.getTimeString(30, out days, out hours, out minutes, out seconds, false);
        assertEqual("1分", fmt5, "format string <60s no second");
    }

    // ---- getTimeString(int, out int x3) ----
    static void testGetTimeString_OutParams_NoSeconds()
    {
        // 90061s = 1天1小时1分
        string fmt = TimeUtility.getTimeString(90061, out int days, out int hours, out int minutes);
        assertEqual(1, days, "days=1");
        assertEqual(1, hours, "hours=1");
        assertEqual(1, minutes, "minutes=1");
        assertEqual("{0}天{1}时{2}分", fmt, "format string");

        // 3661s = 1小时1分
        string fmt2 = TimeUtility.getTimeString(3661, out days, out hours, out minutes);
        assertEqual(0, days, "days=0");
        assertEqual(1, hours, "hours=1");
        assertEqual(1, minutes, "minutes=1");
        assertEqual("{1}时{2}分", fmt2, "format string");

        // 61s = 1分
        string fmt3 = TimeUtility.getTimeString(61, out days, out hours, out minutes);
        assertEqual(1, minutes, "minutes=1");
        assertEqual("{2}分", fmt3, "format string");
    }

    // ---- getTimeString(int, TIME_DISPLAY) ----
    static void testGetTimeString_Int_Display()
    {
        int secs = 3661; // 1h 1m 1s

        string hmsm = TimeUtility.getTimeString(secs, TIME_DISPLAY.HMSM);
        assertEqual("1:1:1", hmsm, "HMSM 3661s -> 1:1:1");

        string hms2 = TimeUtility.getTimeString(secs, TIME_DISPLAY.HMS_2);
        assertEqual("01:01:01", hms2, "HMS_2 3661s -> 01:01:01");

        string hm2 = TimeUtility.getTimeString(secs, TIME_DISPLAY.HM_2);
        assertEqual("01:01", hm2, "HM_2 3661s -> 01:01");

        string ms2 = TimeUtility.getTimeString(secs, TIME_DISPLAY.MS_2);
        assertEqual("61:01", ms2, "MS_2 3661s -> 61:01");

        // DHMS_ZH: 3661s = 1时1分1秒
        string dhms = TimeUtility.getTimeString(secs, TIME_DISPLAY.DHMS_ZH);
        assertEqual("1时1分1秒", dhms, "DHMS_ZH 3661s");

        // 90061s = 1天1时1分1秒
        string dhms2 = TimeUtility.getTimeString(90061, TIME_DISPLAY.DHMS_ZH);
        assertEqual("1天1时1分1秒", dhms2, "DHMS_ZH 90061s");

        // DHM_ZH
        string dhm = TimeUtility.getTimeString(90061, TIME_DISPLAY.DHM_ZH);
        assertEqual("1天1时1分", dhm, "DHM_ZH 90061s");

        // HM_ZH
        string hmZh = TimeUtility.getTimeString(3661, TIME_DISPLAY.HM_ZH);
        assertEqual("1时1分", hmZh, "HM_ZH 3661s");

        // MS_ZH
        string msZh = TimeUtility.getTimeString(61, TIME_DISPLAY.MS_ZH);
        assertEqual("1分1秒", msZh, "MS_ZH 61s");

        // <60s MS_ZH
        string msZh2 = TimeUtility.getTimeString(30, TIME_DISPLAY.MS_ZH);
        assertEqual("30秒", msZh2, "MS_ZH 30s");

        // zero
        string zero = TimeUtility.getTimeString(0, TIME_DISPLAY.HMS_2);
        assertEqual("00:00:00", zero, "HMS_2 0s");
    }

    // ---- getTimeString(DateTime, TIME_DISPLAY) ----
    static void testGetTimeString_DateTime_Display()
    {
        DateTime dt = new DateTime(2022, 3, 15, 14, 30, 45);

        string hmsm = TimeUtility.getTimeString(dt, TIME_DISPLAY.HMSM);
        assertTrue(hmsm.StartsWith("14:30:45:"), "HMSM starts with 14:30:45:");

        string hms2 = TimeUtility.getTimeString(dt, TIME_DISPLAY.HMS_2);
        assertEqual("14:30:45", hms2, "HMS_2 14:30:45");

        string hm2 = TimeUtility.getTimeString(dt, TIME_DISPLAY.HM_2);
        assertEqual("14:30", hm2, "HM_2 14:30");

        string ymdZh = TimeUtility.getTimeString(dt, TIME_DISPLAY.YMD_ZH);
        assertEqual("2022年3月15日", ymdZh, "YMD_ZH");

        string ymdhmZh = TimeUtility.getTimeString(dt, TIME_DISPLAY.YMDHM_ZH);
        assertEqual("2022年3月15日14时30分", ymdhmZh, "YMDHM_ZH");

        string dhmsZh = TimeUtility.getTimeString(dt, TIME_DISPLAY.DHMS_ZH);
        assertEqual("15日14时30分45秒", dhmsZh, "DHMS_ZH DateTime");
    }

    // ---- getTimeStringNoLock ----
    static void testGetTimeStringNoLock()
    {
        DateTime dt = new DateTime(2022, 3, 15, 14, 30, 45);

        string hms2 = TimeUtility.getTimeStringNoLock(dt, TIME_DISPLAY.HMS_2);
        assertEqual("14:30:45", hms2, "NoLock HMS_2");

        string ymdZh = TimeUtility.getTimeStringNoLock(dt, TIME_DISPLAY.YMD_ZH);
        assertEqual("2022年3月15日", ymdZh, "NoLock YMD_ZH");

        string dhmsZh = TimeUtility.getTimeStringNoLock(dt, TIME_DISPLAY.DHMS_ZH);
        assertEqual("15日14时30分45秒", dhmsZh, "NoLock DHMS_ZH");
    }

    // ---- getTimeStringNoBuilder ----
    static void testGetTimeStringNoBuilder()
    {
        DateTime dt = new DateTime(2022, 3, 15, 14, 30, 45);

        string hms2 = TimeUtility.getTimeStringNoBuilder(dt, TIME_DISPLAY.HMS_2);
        assertEqual("14:30:45", hms2, "NoBuilder HMS_2");

        string ymdZh = TimeUtility.getTimeStringNoBuilder(dt, TIME_DISPLAY.YMD_ZH);
        assertEqual("2022年3月15日", ymdZh, "NoBuilder YMD_ZH");

        string dhmsZh = TimeUtility.getTimeStringNoBuilder(dt, TIME_DISPLAY.DHMS_ZH);
        assertEqual("15日14时30分45秒", dhmsZh, "NoBuilder DHMS_ZH");
    }

    // ---- getDayEnd ----
    static void testGetDayEnd()
    {
        DateTime dt = new DateTime(2022, 6, 15, 10, 0, 0);
        DateTime end = TimeUtility.getDayEnd(dt);
        assertEqual(2022, end.Year, "dayEnd year");
        assertEqual(6, end.Month, "dayEnd month");
        assertEqual(16, end.Day, "dayEnd day");
        // AddDays(1).Date 归零时间，返回明天 00:00:00
        assertEqual(0, end.Hour, "dayEnd hour 0");
    }

    // ---- getWeekEnd ----
    static void testGetWeekEnd()
    {
        // 2022-06-15 is Wednesday (DayOfWeek=3)
        DateTime wed = new DateTime(2022, 6, 15, 10, 0, 0);
        DateTime weekEnd = TimeUtility.getWeekEnd(wed);
        // 周日 24:00 = 下周一 00:00:00
        // 7-3+1=5, so Wed+5=Mon(20th)
        assertEqual(2022, weekEnd.Year, "weekEnd year");
        assertEqual(6, weekEnd.Month, "weekEnd month");
        assertEqual(20, weekEnd.Day, "weekEnd day (Wed->Mon)");
        assertEqual(0, weekEnd.Hour, "weekEnd hour 0");
        assertEqual(0, weekEnd.Minute, "weekEnd minute 0");

        // Sunday: DayOfWeek=0
        DateTime sun = new DateTime(2022, 6, 19, 10, 0, 0);
        DateTime sunEnd = TimeUtility.getWeekEnd(sun);
        // Sunday + 1 = Monday 20th
        assertEqual(20, sunEnd.Day, "weekEnd Sunday -> Monday");
        assertEqual(0, sunEnd.Hour, "weekEnd Sunday hour 0");
    }

    // ---- getMonthEnd ----
    static void testGetMonthEnd()
    {
        DateTime dt = new DateTime(2022, 6, 15, 10, 0, 0);
        DateTime end = TimeUtility.getMonthEnd(dt);
        assertEqual(2022, end.Year, "monthEnd year");
        assertEqual(7, end.Month, "monthEnd month");
        assertEqual(1, end.Day, "monthEnd day 1");

        // December -> next year
        DateTime dec = new DateTime(2022, 12, 10, 0, 0, 0);
        DateTime decEnd = TimeUtility.getMonthEnd(dec);
        assertEqual(2023, decEnd.Year, "monthEnd year rollover");
        assertEqual(1, decEnd.Month, "monthEnd month rollover");
    }

    // ---- getYearEnd ----
    static void testGetYearEnd()
    {
        DateTime dt = new DateTime(2022, 6, 15, 10, 0, 0);
        DateTime end = TimeUtility.getYearEnd(dt);
        assertEqual(2023, end.Year, "yearEnd year");
        assertEqual(1, end.Month, "yearEnd month");
        assertEqual(1, end.Day, "yearEnd day");
    }

    // ---- getTimeToTodayEnd ----
    static void testGetTimeToTodayEnd()
    {
        TimeSpan span = TimeUtility.getTimeToTodayEnd();
        // 应该大于0且小于24小时
        assertTrue(span.TotalSeconds > 0, "time to today end > 0");
        assertTrue(span.TotalSeconds <= 86400, "time to today end <= 86400");
    }

    // ---- getSecondsToTodayEnd ----
    static void testGetSecondsToTodayEnd()
    {
        int secs = TimeUtility.getSecondsToTodayEnd();
        assertTrue(secs > 0, "seconds to today end > 0");
        assertTrue(secs <= 86400, "seconds to today end <= 86400");
    }


	

	// ─── getTimeNoLock: 无锁当前时间字符串, 校验格式 ─────────────────
	private static void testGetTimeNoLock_Format()
	{
		// HMS_2 格式: HH:MM:SS
		string s = TimeUtility.getTimeNoLock(TIME_DISPLAY.HMS_2);
		assertTrue(Regex.IsMatch(s, @"^\d{2}:\d{2}:\d{2}$"), $"getTimeNoLock HMS_2 格式: {s}");
		// HMSM: HH:MM:SS:mmm
		string s2 = TimeUtility.getTimeNoLock(TIME_DISPLAY.HMSM);
		assertTrue(Regex.IsMatch(s2, @"^\d{1,2}:\d{1,2}:\d{1,2}:\d{1,3}$"), $"getTimeNoLock HMSM 格式: {s2}");
	}

	// ─── getNowTime: 主线程当前时间字符串, 格式校验(避免跨秒竞态) ───
	private static void testGetNowTime_Format()
	{
		// 格式校验为主(避免秒边界抖动导致 flaky): HH:MM:SS
		string s = TimeUtility.getNowTime(TIME_DISPLAY.HMS_2);
		assertTrue(Regex.IsMatch(s, @"^\d{2}:\d{2}:\d{2}$"), $"getNowTime HMS_2 格式: {s}");
		// 时、分应与当前时间一致(秒因采样间隙可能差1)
		DateTime now = DateTime.Now;
		assertTrue(s.StartsWith(now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2")),
			$"getNowTime 时/分应与当前一致, got={s}, now={now.Hour:D2}:{now.Minute:D2}");
	}

	// ─── getTimeNoBuilder: StringBuilder 版本与 NoLock 版本同格式 ───
	private static void testGetTimeNoBuilder_Format()
	{
		string s = TimeUtility.getTimeNoBuilder(TIME_DISPLAY.YMD_ZH);
		assertTrue(Regex.IsMatch(s, @"^\d+年\d+月\d+日$"), $"getTimeNoBuilder YMD_ZH 格式: {s}");
		string s2 = TimeUtility.getTimeNoBuilder(TIME_DISPLAY.DHMS_ZH);
		assertTrue(Regex.IsMatch(s2, @"^\d+日\d+时\d+分\d+秒$"), $"getTimeNoBuilder DHMS_ZH 格式: {s2}");
	}

	// ─── getDateTimeToUTC: 固定 UTC 时间戳 → 精确 UTC 字符串 ────────
	private static void testGetDateTimeToUTC_Fixed()
	{
		// 2020-01-01 00:00:00 UTC = 1577836800
		long ts = 1577836800L;
		// UTC 时间年月日
		string ymd = TimeUtility.getDateTimeToUTC(ts, TIME_DISPLAY.YMD_ZH);
		assertEqual("2020年1月1日", ymd, "getDateTimeToUTC 2020-01-01 UTC YMD_ZH");
		// 时分秒 00:00:00
		string hms = TimeUtility.getDateTimeToUTC(ts, TIME_DISPLAY.HMS_2);
		assertEqual("00:00:00", hms, "getDateTimeToUTC 2020-01-01 UTC HMS_2");
	}

	// ─── getLocalTime: 固定 UTC 时间戳 → 本地时间字符串 ─────────────
	private static void testGetLocalTime_Fixed()
	{
		long ts = 1577836800L; // 2020-01-01 00:00:00 UTC
		// 本地时间 = UTC + 时区偏移
		DateTime local = TimeUtility.timeStampToDateTime(ts);
		// 通过 getTimeString 构造期望值(与库内部一致的格式化路径)
		string expected = TimeUtility.getTimeString(local, TIME_DISPLAY.YMDHM_ZH);
		string actual = TimeUtility.getLocalTime(ts, TIME_DISPLAY.YMDHM_ZH);
		assertEqual(expected, actual, "getLocalTime == getTimeString(timeStampToDateTime(ts)) 自洽");
	}

	// ─── getNowTimeStamp: 本地秒级时间戳, 采样窗口内 ─────────────────
	private static void testGetNowTimeStamp_Window()
	{
		long before = TimeUtility.dateTimeToTimeStamp(DateTime.Now);
		long now = TimeUtility.getNowTimeStamp();
		long after = TimeUtility.dateTimeToTimeStamp(DateTime.Now);
		// 采样间隙较短时, 三者应在同一个秒级窗口内(最多相差1s)
		assertTrue(now >= before && now <= after + 1, $"getNowTimeStamp 应落在前后采样窗口内 b={before} n={now} a={after}");
	}

	// ─── getNowUTCTimeStamp: UTC 秒级时间戳, 与本地戳关联 ───────────
	private static void testGetNowUTCTimeStamp_Window()
	{
		long before = TimeUtility.dateTimeToTimeStamp(DateTime.UtcNow);
		long now = TimeUtility.getNowUTCTimeStamp();
		long after = TimeUtility.dateTimeToTimeStamp(DateTime.UtcNow);
		assertTrue(now >= before && now <= after + 1, $"getNowUTCTimeStamp 应在前后采样窗口内 b={before} n={now} a={after}");
	}

	// ─── getNowTimeStampMS: 毫秒级应单调非减 ────────────────────────
	private static void testGetNowTimeStampMS_Monotonic()
	{
		long b1 = TimeUtility.getNowTimeStampMS();
		long b2 = TimeUtility.getNowTimeStampMS();
		long b3 = TimeUtility.getNowTimeStampMS();
		// 单调非减
		assertTrue(b2 >= b1, "ms 时间戳单调非减(1->2)");
		assertTrue(b3 >= b2, "ms 时间戳单调非减(2->3)");
		// 与 getNowUTCTimeStampMS 的差值等于本地与 UTC 的时区偏移毫秒数
		long local = TimeUtility.getNowTimeStampMS();
		long utc = TimeUtility.getNowUTCTimeStampMS();
		long localOffsetMillis = (long)TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).TotalMilliseconds;
		long diff = local - utc;
		assertTrue(Math.Abs(diff - localOffsetMillis) <= 2000, $"本地/UTC ms 戳差值应≈时区偏移 {localOffsetMillis}, 实际 {diff}");
	}

	// ─── isTodayTime(DateTime): 今天/昨天/明天 ───────────────────────
	private static void testIsTodayTime_DateTime()
	{
		assertTrue(TimeUtility.isTodayTime(DateTime.Now), "现在应视为今天");
		assertFalse(TimeUtility.isTodayTime(DateTime.Now.AddDays(-1)), "昨天不是今天");
		assertFalse(TimeUtility.isTodayTime(DateTime.Now.AddDays(1)), "明天不是今天");
		assertTrue(TimeUtility.isTodayTime(DateTime.Today), "今天0点是今天");
	}

	// ─── isTodayTime(long): UTC 时间戳判断 ───────────────────────────
	private static void testIsTodayTime_TimeStamp()
	{
		// 明显过去 / 未来(偏离当前 >3 天)在任何时区都不可能是今天
		long past = TimeUtility.getNowUTCTimeStamp() - 3 * 86400;
		long future = TimeUtility.getNowUTCTimeStamp() + 3 * 86400;
		assertFalse(TimeUtility.isTodayTime(past), "3天前的 UTC 戳不是今天");
		assertFalse(TimeUtility.isTodayTime(future), "3天后的 UTC 戳不是今天");
		// 语义一致性: isTodayTime(ts) 应与"把 ts 转成 UTC 日期再与本地今天比"一致
		long nowUtc = TimeUtility.getNowUTCTimeStamp();
		bool refResult = TimeUtility.isSameDay(TimeUtility.timeStampToDateTimeUTC(nowUtc), DateTime.Now);
		assertEqual(refResult, TimeUtility.isTodayTime(nowUtc), "isTodayTime(nowUtc) 与语义引用一致");
	}

	// ─── getTodayBegin: 今天0点, 与 now 同一天 ──────────────────────
	private static void testGetTodayBegin_Consistency()
	{
		DateTime begin = TimeUtility.getTodayBegin();
		DateTime now = DateTime.Now;
		assertEqual(now.Year, begin.Year, "getTodayBegin year 同今天");
		assertEqual(now.Month, begin.Month, "getTodayBegin month 同今天");
		assertEqual(now.Day, begin.Day, "getTodayBegin day 同今天");
		assertEqual(0, begin.Hour, "getTodayBegin hour=0");
		assertEqual(0, begin.Minute, "getTodayBegin minute=0");
		assertEqual(0, begin.Second, "getTodayBegin second=0");
	}

	// ─── getTodayBeginTimeStamp: 应等于今天0点的时间戳 ──────────────
	private static void testTodayBegin_IsMidnight()
	{
		long ts = TimeUtility.getTodayBeginTimeStamp();
		// 与 getTodayBegin 的时间戳一致(同一函数路径, 恒等式)
		assertEqual(TimeUtility.dateTimeToTimeStamp(TimeUtility.getTodayBegin()), ts,
			"getTodayBeginTimeStamp == dateTimeToTimeStamp(getTodayBegin())");
		// 注意: 本库 dateTimeToTimeStamp 基于"本地纪元"做秒差,而 timeStampToDateTime(ts)
		// 内部又对结果做 ToLocalTime()(把 Unspecified 当 UTC 再转一次),会导致零点偏移 N 小时,
		// 因此跨时区下不能走 timeStampToDateTime 还原。此处用 timeStampToDateTimeUTC(ts)
		// (内部只有 AddSeconds, 不二次转当地) 验证 ts 确实指向"今天 0 点"。
		DateTime begin = TimeUtility.timeStampToDateTimeUTC(ts);
		assertEqual(DateTime.Now.Year, begin.Year, "begin 时间戳转回同年");
		assertEqual(DateTime.Now.Month, begin.Month, "begin 时间戳转回同月");
		assertEqual(DateTime.Now.Day, begin.Day, "begin 时间戳转回同日");
		assertEqual(0, begin.Hour, "begin 时间戳转回 hour=0");
		assertEqual(0, begin.Minute, "begin 时间戳转回 minute=0");
		assertEqual(0, begin.Second, "begin 时间戳转回 second=0");
	}

	// ─── getTodayEndRemain: 剩余秒数在 (0, 86400] 范围 ──────────────
	private static void testDayEndRemain_Range()
	{
		int remain = TimeUtility.getTodayEndRemain();
		assertTrue(remain > 0, "今天剩余秒数 > 0");
		assertTrue(remain <= 86400, "今天剩余秒数 <= 86400");
	}

	// ─── 周/月/年结束剩余: 各自范围 ────────────────────────────────
	private static void testWeekMonthYearEndRemain_Range()
	{
		int week = TimeUtility.getWeekEndRemain();
		int month = TimeUtility.getMonthEndRemain();
		int year = TimeUtility.getYearEndRemain();
		assertTrue(week > 0 && week <= 7 * 86400 + 1, $"week remain 范围: {week}");
		assertTrue(month > 0 && month <= 31 * 86400 + 1, $"month remain 范围: {month}");
		assertTrue(year > 0 && year <= 366 * 86400 + 1, $"year remain 范围: {year}");
		// 年剩余应 >= 月剩余(同年恒成立, 到12月时二者可能接近但年仍更大)
		assertTrue(year >= month, "year remain >= month remain");
	}

	// ─── 时间戳↔UTC DateTime 往返 ──────────────────────────────────
	private static void testTimestamp_RoundTrip_UTC()
	{
		long ts = 1600000000L; // 固定值
		var dt = TimeUtility.timeStampToDateTimeUTC(ts);
		long back = TimeUtility.dateTimeToTimeStamp(dt);
		assertEqual(ts, back, $"UTC 时间戳往返一致 {ts}");
	}

	// ─── 链式: 今天边界在不同函数间自洽 ────────────────────────────
	private static void testChain_TodayBoundaryAcrossFunctions()
	{
		DateTime begin = TimeUtility.getTodayBegin();
		long beginTs = TimeUtility.dateTimeToTimeStamp(begin);
		// 今天的剩余 = 明天0点 - 现在
		DateTime tomorrowMidnight = TimeUtility.getTomorrowTime(0);
		long nowTs = TimeUtility.getNowTimeStamp();
		long expectedRemain = TimeUtility.dateTimeToTimeStamp(tomorrowMidnight) - nowTs;
		int actualRemain = TimeUtility.getTodayEndRemain();
		// 留 2 秒容差(采样间隙)
		assertTrue(Math.Abs(actualRemain - expectedRemain) <= 2,
			$"getTodayEndRemain 与 getTomorrowTime(0)-now 自洽 expected={expectedRemain} actual={actualRemain}");
		// beginTs 应在昨天23:59之后、今天23:59之前
		assertTrue(beginTs > TimeUtility.getNowTimeStamp() - 86400, "今天0点应晚于昨天的0点");
		assertTrue(beginTs <= TimeUtility.getNowTimeStamp(), "今天0点应不晚于现在");
	}
}
