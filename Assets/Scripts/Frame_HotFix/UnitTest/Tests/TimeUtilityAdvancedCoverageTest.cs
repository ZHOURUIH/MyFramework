#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static TestAssert;
using static TimeUtility;

public static class TimeUtilityAdvancedCoverageTest
{
	public static void Run()
	{
		testDisplayParity();
		testTimestampRoundTrips();
		testMinuteFormatting();
		testBoundaryHelpers();
		testFrameTimeSetter();
	}

	private static void testDisplayParity()
	{
		DateTime time = new DateTime(2024, 3, 5, 6, 7, 8, 9, DateTimeKind.Unspecified);
		assertEqual(getTimeString(time, TIME_DISPLAY.HMSM), getTimeStringNoLock(time, TIME_DISPLAY.HMSM), "HMSM outputs should match");
		assertEqual(getTimeString(time, TIME_DISPLAY.HMS_2), getTimeStringNoLock(time, TIME_DISPLAY.HMS_2), "HMS_2 outputs should match");
		assertEqual(getTimeString(time, TIME_DISPLAY.HM_2), getTimeStringNoLock(time, TIME_DISPLAY.HM_2), "HM_2 outputs should match");
		assertEqual(getTimeString(time, TIME_DISPLAY.MS_2), getTimeStringNoLock(time, TIME_DISPLAY.MS_2), "MS_2 outputs should match");
		assertEqual(getTimeString(time, TIME_DISPLAY.YMD_ZH), getTimeStringNoBuilder(time, TIME_DISPLAY.YMD_ZH), "YMD_ZH outputs should match");
		assertEqual(getTimeString(time, TIME_DISPLAY.YMDHM_ZH), getTimeStringNoBuilder(time, TIME_DISPLAY.YMDHM_ZH), "YMDHM_ZH outputs should match");
	}

	private static void testTimestampRoundTrips()
	{
		DateTime baseline = new DateTime(1970, 1, 1, 0, 0, 1, 500, DateTimeKind.Unspecified);
		long secondStamp = dateTimeToTimeStamp(baseline);
		long milliStamp = dateTimeToTimeStampMS(baseline);
		assertEqual(1L, secondStamp, "dateTimeToTimeStamp should map to unix seconds");
		assertEqual(1500L, milliStamp, "dateTimeToTimeStampMS should map to unix milliseconds");

		DateTime utcBack = timeStampToDateTimeUTC(secondStamp);
		assertEqual(1970, utcBack.Year, "timeStampToDateTimeUTC year");
		assertEqual(1, utcBack.Month, "timeStampToDateTimeUTC month");
		assertEqual(1, utcBack.Day, "timeStampToDateTimeUTC day");
		assertEqual(1, utcBack.Second, "timeStampToDateTimeUTC second");

		DateTime utcBackMs = timeStampMSToDateTimeUTC(milliStamp);
		assertEqual(500, utcBackMs.Millisecond, "timeStampMSToDateTimeUTC millisecond");
	}

	private static void testMinuteFormatting()
	{
		assertEqual("1小时30分钟", minuteToHourMinuteString(90), "minuteToHourMinuteString should format hour+minute");
		assertEqual("45分钟", minuteToHourMinuteString(45), "minuteToHourMinuteString should format minutes only");
		assertEqual("1小时", minuteToHourMinuteString(60), "minuteToHourMinuteString should omit zero minutes");
	}

	private static void testBoundaryHelpers()
	{
		DateTime dt = new DateTime(2024, 12, 31, 23, 59, 59);
		assertEqual(new DateTime(2025, 1, 1, 23, 59, 59), getDayEnd(dt), "getDayEnd should add one day");
		assertEqual(new DateTime(2025, 1, 1, 0, 0, 0), getYearEnd(dt), "getYearEnd should advance to next year");

		DateTime monthEnd = getMonthEnd(new DateTime(2024, 2, 15, 12, 0, 0));
		assertEqual(new DateTime(2024, 3, 1, 0, 0, 0), monthEnd, "getMonthEnd should advance to first day of next month");

		DateTime monday = new DateTime(2024, 3, 11);
		DateTime nextMonday = getWeekEnd(monday);
		assertEqual(new DateTime(2024, 3, 18), nextMonday, "getWeekEnd should advance to the next Monday");
	}

	private static void testFrameTimeSetter()
	{
		setThisTimeMS(123456789L);
		assertEqual(123456789L, getThisTimeMS(), "getThisTimeMS should return the last set value");
		setThisTimeMS(0L);
		assertEqual(0L, getThisTimeMS(), "getThisTimeMS should support zero");
	}
}
#endif
