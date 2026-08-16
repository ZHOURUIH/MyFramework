using UnityEngine;
using static TestAssert;

// CurveInfo: 关键帧曲线信息数据类(纯数据, 带参构造, 无外部依赖)
public static class CurveInfoTest
{
	public static void Run()
	{
		testConstructorFields();
		testCurveReference();
		testNullCurve();
		testMultipleInstances();
		testEmptyName();
		testThreeCurvesIndependent();
		testLargeID();
		testNegativeID();
		testNameWithSpecialChars();
		testSharedCurveReference();
		testNameNull();
		testSameIDDifferentName();
		testLongName();
		testSameNameDifferentID();
		testDifferentCurvesIndependent();
	}

	// 构造字段读回
	private static void testConstructorFields()
	{
		AnimationCurve curve = new AnimationCurve();
		CurveInfo info = new CurveInfo(101, "curve101", curve);
		assertEqual(101, info.mID, "构造 mID 读回");
		assertEqual("curve101", info.mName, "构造 mName 读回");
		assertTrue(info.mCurve != null, "构造 mCurve 非空");
	}

	// mCurve 与传入引用一致
	private static void testCurveReference()
	{
		AnimationCurve curve = new AnimationCurve();
		CurveInfo info = new CurveInfo(1, "c", curve);
		assertTrue(ReferenceEquals(curve, info.mCurve), "mCurve 与传入同一引用");
	}

	// mCurve 可传 null
	private static void testNullCurve()
	{
		CurveInfo info = new CurveInfo(2, "c", null);
		assertTrue(info.mCurve == null, "mCurve 传 null 可构造");
		assertEqual(2, info.mID, "null 曲线不影响 ID");
	}

	// 多实例独立
	private static void testMultipleInstances()
	{
		CurveInfo info1 = new CurveInfo(101, "a", new AnimationCurve());
		CurveInfo info2 = new CurveInfo(102, "b", new AnimationCurve());
		assertEqual(101, info1.mID, "实例1 ID");
		assertEqual(102, info2.mID, "实例2 ID");
		assertFalse(ReferenceEquals(info1.mCurve, info2.mCurve), "两个实例曲线独立");
	}

	// 空名字可构造
	private static void testEmptyName()
	{
		CurveInfo info = new CurveInfo(3, "", new AnimationCurve());
		assertEqual("", info.mName, "空名字读回");
	}

	// 三个实例互不干扰
	private static void testThreeCurvesIndependent()
	{
		CurveInfo a = new CurveInfo(101, "a", new AnimationCurve());
		CurveInfo b = new CurveInfo(102, "b", new AnimationCurve());
		CurveInfo c = new CurveInfo(103, "c", null);
		assertEqual(101, a.mID, "a ID");
		assertEqual(102, b.mID, "b ID");
		assertEqual(103, c.mID, "c ID");
		assertFalse(ReferenceEquals(a.mCurve, b.mCurve), "a/b 曲线独立");
		assertTrue(c.mCurve == null, "c 曲线 null");
	}

	// 大 ID 读回
	private static void testLargeID()
	{
		CurveInfo info = new CurveInfo(999999, "large", new AnimationCurve());
		assertEqual(999999, info.mID, "大 ID 读回");
	}

	// 负 ID 读回(纯数据类不校验)
	private static void testNegativeID()
	{
		CurveInfo info = new CurveInfo(-5, "neg", new AnimationCurve());
		assertEqual(-5, info.mID, "负 ID 读回");
	}

	// 特殊字符名字读回
	private static void testNameWithSpecialChars()
	{
		CurveInfo info = new CurveInfo(7, "curve_带中文.特殊!符号", new AnimationCurve());
		assertEqual("curve_带中文.特殊!符号", info.mName, "特殊字符名字读回");
	}

	// 两 info 共享同一 curve 引用
	private static void testSharedCurveReference()
	{
		AnimationCurve shared = new AnimationCurve();
		CurveInfo a = new CurveInfo(1, "a", shared);
		CurveInfo b = new CurveInfo(2, "b", shared);
		assertTrue(ReferenceEquals(a.mCurve, shared), "a 与 shared 同引用");
		assertTrue(ReferenceEquals(b.mCurve, shared), "b 与 shared 同引用");
	}

	// 名字为 null 读回(纯数据不校验)
	private static void testNameNull()
	{
		CurveInfo info = new CurveInfo(9, null, new AnimationCurve());
		assertTrue(info.mName == null, "null 名字读回 null");
	}

	// 同 ID 不同名字互不影响
	private static void testSameIDDifferentName()
	{
		CurveInfo a = new CurveInfo(5, "first", new AnimationCurve());
		CurveInfo b = new CurveInfo(5, "second", new AnimationCurve());
		assertEqual(5, a.mID, "a ID 5");
		assertEqual(5, b.mID, "b ID 5");
		assertEqual("first", a.mName, "a 名字 first");
		assertEqual("second", b.mName, "b 名字 second");
		assertFalse(ReferenceEquals(a, b), "两实例不同");
	}

	// 长名字读回
	private static void testLongName()
	{
		string longName = "CurveWithAVeryLongName_That_Is_Designed_To_Test_Buffer_Boundaries_0123456789_ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		CurveInfo info = new CurveInfo(11, longName, new AnimationCurve());
		assertEqual(longName, info.mName, "长名字读回");
	}

	// 同名字不同 ID 互不影响
	private static void testSameNameDifferentID()
	{
		CurveInfo a = new CurveInfo(1, "shared", new AnimationCurve());
		CurveInfo b = new CurveInfo(2, "shared", new AnimationCurve());
		assertEqual(1, a.mID, "a ID 1");
		assertEqual(2, b.mID, "b ID 2");
		assertEqual("shared", a.mName, "a 名字");
		assertEqual("shared", b.mName, "b 名字");
	}

	// 不同 curve 实例引用隔离
	private static void testDifferentCurvesIndependent()
	{
		AnimationCurve ca = new AnimationCurve();
		AnimationCurve cb = new AnimationCurve();
		CurveInfo a = new CurveInfo(3, "ca", ca);
		CurveInfo b = new CurveInfo(4, "cb", cb);
		assertTrue(ReferenceEquals(ca, a.mCurve), "a 持有 ca");
		assertTrue(ReferenceEquals(cb, b.mCurve), "b 持有 cb");
		assertFalse(ReferenceEquals(a.mCurve, b.mCurve), "两实例曲线不同");
	}
}
