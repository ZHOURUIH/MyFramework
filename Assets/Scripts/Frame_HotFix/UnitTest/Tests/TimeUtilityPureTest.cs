#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static TimeUtility;
using static MathUtility;

// TimeUtility 时间格式化/转换/比较测试
public static class TimeUtilityPureTest
{
	public static void Run()
	{
		testDateTimeToTimeStamp();
		testTimeStampToDateTime();
		testDateToTimeToTimeStamp();
		testIsSameDay();
		testDaysToSeconds();
		testGetTimeStringWithOutParams();
	}

	private static void testDateTimeToTimeStamp()
	{
		DateTime dt = new DateTime(2025, 6, 15, 0, 0, 0, DateTimeKind.Local);
		long ts = dateTimeToTimeStamp(dt);
		DateTime back = timeStampToDateTime(ts);
		Assert(isSameDay(dt, back));
	}

	private static void testTimeStampToDateTime()
	{
		DateTime dt = timeStampToDateTime(0);
		AssertEqual(1970, dt.Year);
	}

	private static void testDateToTimeToTimeStamp()
	{
		long ms = dateTimeToTimeStampMS(new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Local));
		Assert(ms > 0);
	}

	private static void testIsSameDay()
	{
		DateTime a = new DateTime(2025, 6, 15, 10, 0, 0);
		DateTime b = new DateTime(2025, 6, 15, 22, 0, 0);
		DateTime c = new DateTime(2025, 6, 16, 0, 0, 0);
		Assert(isSameDay(a, b));
		Assert(!isSameDay(a, c));
	}

	private static void testDaysToSeconds()
	{
		AssertEqual(0, daysToSeconds(0));
		AssertEqual(86400, daysToSeconds(1));
		AssertEqual(604800, daysToSeconds(7));
	}

	private static void testGetTimeStringWithOutParams()
	{
		int d, h, m, s;
		string result = getTimeString(90061, out d, out h, out m, out s, true);
		AssertEqual(1, d);
		AssertEqual(1, h);
		AssertEqual(1, m);
		AssertEqual(1, s);

		result = getTimeString(45, out d, out h, out m, out s, true);
		AssertEqual(0, d);
		AssertEqual(0, h);
		AssertEqual(0, m);
		AssertEqual(45, s);

		// 无秒版本
		result = getTimeString(3661, out d, out h, out m);
		AssertEqual(0, d);
		AssertEqual(1, h);
		AssertEqual(1, m);
	}

	private static void Assert(bool c) { if (!c) throw new Exception("Assert failed"); }
	private static void AssertEqual(int e, int a) { if (e != a) throw new Exception($"Expected [{e}] got [{a}]"); }
}
#endif
