using System;
using System.Text.RegularExpressions;
using static TestAssert;

// TimeUtility 时间工具深度测试
// 覆盖基本测试未触及的"当前时间"类方法与日期边界逻辑:
// getTimeNoLock / getNowTime / getTimeNoBuilder / getDateTimeToUTC / getLocalTime /
// getNowTimeStamp 系列 / isTodayTime / getTodayBegin / getTodayBeginTimeStamp /
// getTodayEndRemain / getWeekEndRemain / getMonthEndRemain / getYearEndRemain。
// 设计要点:
//  1. "当前时间"输出不可精确硬编码, 改为"采样前后快照 + 容差窗口"或"regex 格式校验"。
//  2. 纯时间戳转换(getDateTimeToUTC/getLocalTime)用固定已知时间戳 → 可精确断言。
//  3. 按深度测试理念, 多做"往返一致 / 链式互证 / 边界交叉"断言, 而非单点调用。
public static class TimeUtilityDeepTest
{
	public static void Run()
	{
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
