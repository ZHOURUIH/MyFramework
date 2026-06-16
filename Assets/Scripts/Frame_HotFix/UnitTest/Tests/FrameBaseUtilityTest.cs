using System;
using static FrameBaseUtility;

// FrameBaseUtility compareVersion3 / checkValidVersion 测试
public static class FrameBaseUtilityTest
{
	public static void Run()
	{
		testCompareEqual();
		testCompareRemoteHigher();
		testCompareLocalHigher();
		testCompareRemoteEmpty();
		testCompareLocalEmpty();
		testCheckValidNormal();
		testCheckValidEmpty();
	}

	private static void testCompareEqual()
	{
		var result = compareVersion3("1.2.3", "1.2.3", out var l, out var h);
		AssertEqual(0, (int)result);
		AssertEqual(0, (int)l);
		AssertEqual(0, (int)h);
	}

	private static void testCompareRemoteHigher()
	{
		VERSION_COMPARE result = compareVersion3("2.0.0", "1.0.0", out var l, out var h);
		AssertEqual((int)VERSION_COMPARE.LOCAL_LOWER, (int)result);
	}

	private static void testCompareLocalHigher()
	{
		VERSION_COMPARE result = compareVersion3("1.0.0", "2.0.0", out var l, out var h);
		AssertEqual((int)VERSION_COMPARE.REMOTE_LOWER, (int)result);
	}

	private static void testCompareRemoteEmpty()
	{
		VERSION_COMPARE result = compareVersion3("", "1.0.0", out var l, out var h);
		AssertEqual((int)VERSION_COMPARE.REMOTE_LOWER, (int)result);
	}

	private static void testCompareLocalEmpty()
	{
		VERSION_COMPARE result = compareVersion3("1.0.0", "", out var l, out var h);
		AssertEqual((int)VERSION_COMPARE.LOCAL_LOWER, (int)result);
	}

	private static void testCheckValidNormal()
	{
		string v = "1.2.3";
		bool ok = checkValidVersion(ref v);
		Assert(ok);
		AssertEqual("1.2.3", v);
	}

	private static void testCheckValidEmpty()
	{
		string v = "";
		bool ok = checkValidVersion(ref v);
		Assert(!ok);
		AssertEqual("0.0.0", v);
	}

	private static void Assert(bool c) 
	{
		if (!c)
		{
			throw new Exception("Assert failed");
		}
	}
	private static void AssertEqual(int e, int a) 
	{
		if (e != a)
		{
			throw new Exception($"Expected [{e}] got [{a}]");
		}
	}
	private static void AssertEqual(string e, string a) 
	{
		if (e != a)
		{
			throw new Exception($"Expected [{e}] got [{a}]");
		}
	}
}