using UnityEngine;
using static TestAssert;

// Frame_Game 精简层 MathUtility 测试(纯数学)
public static class MathUtilityTest
{
	public static void Run()
	{
		testRoundWhole();
		testRoundHalfUp();
		testRoundNegative();
		testMultiVector2();
		testMultiVector3();
		testMultiVector3Negative();
	}

	// 整数不变化
	static void testRoundWhole()
	{
		Vector3 r = MathUtility.round(new Vector3(1f, 2f, 3f));
		assertEqual(new Vector3(1f, 2f, 3f), r, "整数不变");
	}

	// 四舍五入
	static void testRoundHalfUp()
	{
		Vector3 r = MathUtility.round(new Vector3(1.6f, 2.4f, 3.5f));
		assertEqual(new Vector3(2f, 2f, 4f), r, "四舍五入");
	}

	// 负数四舍五入(Mathf.RoundToInt 银行家舍入: 2.5->2, -2.5->-2)
	static void testRoundNegative()
	{
		Vector3 r = MathUtility.round(new Vector3(-1.4f, -1.6f, 0f));
		assertEqual(new Vector3(-1f, -2f, 0f), r, "负数四舍五入");
	}

	// Vector2 逐分量乘法
	static void testMultiVector2()
	{
		Vector2 r = MathUtility.multiVector2(new Vector2(2f, 3f), new Vector2(4f, 5f));
		assertEqual(new Vector2(8f, 15f), r, "逐分量乘");
	}

	// Vector3 逐分量乘法
	static void testMultiVector3()
	{
		Vector3 r = MathUtility.multiVector3(new Vector3(2f, 3f, 4f), new Vector3(5f, 6f, 7f));
		assertEqual(new Vector3(10f, 18f, 28f), r, "逐分量乘");
	}

	// 含负数/零
	static void testMultiVector3Negative()
	{
		Vector3 r = MathUtility.multiVector3(new Vector3(-2f, 0f, 1f), new Vector3(3f, 5f, -4f));
		assertEqual(new Vector3(-6f, 0f, -4f), r, "负/零分量");
	}
}
