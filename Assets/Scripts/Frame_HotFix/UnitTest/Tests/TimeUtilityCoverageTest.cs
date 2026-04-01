#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static TimeUtility;
using static TestAssert;

public static class TimeUtilityCoverageTest
{
    public static void Run()
    {
        testTimestampRoundTrips();
        testFormattingAndBoundaries();
        testTodayHelpers();
    }

    private static void testTimestampRoundTrips()
    {
        DateTime utc = new DateTime(2024, 3, 5, 6, 7, 8, 9, DateTimeKind.Utc);
        long sec = dateTimeToTimeStamp(utc);
        long ms = dateTimeToTimeStampMS(utc);

        assertEqual(sec, dateTimeToTimeStamp(timeStampToDateTimeUTC(sec)), "timestamp seconds round trip");
        assertEqual(ms, dateTimeToTimeStampMS(timeStampMSToDateTimeUTC(ms)), "timestamp milliseconds round trip");
        assertTrue(timeStampToDateTimeUTC(sec).Year == 2024, "utc year");
        assertTrue(timeStampMSToDateTimeUTC(ms).Millisecond == 9, "utc millisecond");
    }

    private static void testFormattingAndBoundaries()
    {
        DateTime sample = new DateTime(2024, 12, 31, 23, 59, 58, 123, DateTimeKind.Unspecified);
        string hms = getTimeString(sample, TIME_DISPLAY.HMS_2);
        string ymd = getTimeString(sample, TIME_DISPLAY.YMD_ZH);
        string zh = getTimeStringNoBuilder(sample, TIME_DISPLAY.YMDHM_ZH);

        assertTrue(hms.Length > 0, "hms format");
        assertTrue(ymd.Contains("2024"), "ymd format");
        assertTrue(zh.Length > 0, "zh format");
        assertEqual(new DateTime(2025, 1, 1, 23, 59, 58, 123, DateTimeKind.Unspecified), getDayEnd(sample), "day end");
        assertEqual(new DateTime(2025, 1, 1), getYearEnd(sample), "year end");
        assertEqual(new DateTime(2025, 1, 1), getMonthEnd(sample), "month end");
        assertTrue(minuteToHourMinuteString(90).Contains("1"), "minute format");
        assertEqual(86400 * 3, daysToSeconds(3), "days to seconds");
    }

    private static void testTodayHelpers()
    {
        DateTime now = DateTime.Now;
        long todayStamp = getTodayBeginTimeStamp();
        DateTime todayBegin = getTodayBegin();
        DateTime todayAtEight = getTodayTime(8, 0, 0);
        DateTime tomorrowAtEight = getTomorrowTime(8, 0, 0);

        assertTrue(isSameDay(now, timeStampToDateTime(todayStamp)), "today stamp is same day");
        assertTrue(todayBegin.Hour == 0 && todayBegin.Minute == 0, "today begin at midnight");
        assertTrue(tomorrowAtEight > todayAtEight, "tomorrow after today");
        assertTrue(getTimeToTodayEnd().TotalSeconds > 0, "time to end positive");
    }
}
#endif
