using UnityEngine;
using static TestAssert;

// ComplexPoint 结构体单元测试(Frame_Base 层, 纯逻辑, 无依赖)
// setAbsolute(float) 语义: mAbsolute = (int)(absolute + 0.5f * Mathf.Sign(absolute))
//   - 正数: 四舍五入(0.5 进位)
//   - 负数: 远离零取整(-0.5 进位到 -1)
//   - 0: 保持 0(Mathf.Sign(0)=0)
public static class ComplexPointTest
{
	public static void Run()
	{
		testSetRelative();
		testSetAbsolutePositive();
		testSetAbsoluteNegative();
		testSetAbsoluteZero();
		testSetAbsoluteRoundHalfAwayFromZero();
		testDefaultValue();
	}

	// ═════════════════════════════════════════════════════════════════
	// setRelative — 直接存 float, 不改写
	// ═════════════════════════════════════════════════════════════════
	private static void testSetRelative()
	{
		ComplexPoint point = new ComplexPoint();
		point.setRelative(3.5f);
		assertEqual(3.5f, point.mRelative, "setRelative 应原样保存");
		point.setRelative(-2.25f);
		assertEqual(-2.25f, point.mRelative, "setRelative 负数原样保存");
	}

	// ═════════════════════════════════════════════════════════════════
	// setAbsolute — 正数四舍五入
	// ═════════════════════════════════════════════════════════════════
	private static void testSetAbsolutePositive()
	{
		ComplexPoint point = new ComplexPoint();
		point.setAbsolute(0.4f);
		assertEqual(0, point.mAbsolute, "0.4 四舍五入为 0");
		point.setAbsolute(0.6f);
		assertEqual(1, point.mAbsolute, "0.6 四舍五入为 1");
		point.setAbsolute(1.9f);
		assertEqual(2, point.mAbsolute, "1.9 四舍五入为 2");
		point.setAbsolute(2.1f);
		assertEqual(2, point.mAbsolute, "2.1 四舍五入为 2");
		point.setAbsolute(100.3f);
		assertEqual(100, point.mAbsolute, "100.3 四舍五入为 100");
	}

	// ═════════════════════════════════════════════════════════════════
	// setAbsolute — 负数远离零取整
	// ═════════════════════════════════════════════════════════════════
	private static void testSetAbsoluteNegative()
	{
		ComplexPoint point = new ComplexPoint();
		point.setAbsolute(-0.4f);
		assertEqual(0, point.mAbsolute, "-0.4 取整为 0");
		point.setAbsolute(-0.6f);
		assertEqual(-1, point.mAbsolute, "-0.6 远离零取整为 -1");
		point.setAbsolute(-1.9f);
		assertEqual(-2, point.mAbsolute, "-1.9 远离零取整为 -2");
		point.setAbsolute(-2.1f);
		assertEqual(-2, point.mAbsolute, "-2.1 远离零取整为 -2");
	}

	// ═════════════════════════════════════════════════════════════════
	// setAbsolute — 零值保持 0
	// ═════════════════════════════════════════════════════════════════
	private static void testSetAbsoluteZero()
	{
		ComplexPoint point = new ComplexPoint();
		point.setAbsolute(0f);
		assertEqual(0, point.mAbsolute, "0 保持 0");
		point.setAbsolute(-0f);
		assertEqual(0, point.mAbsolute, "-0 保持 0");
	}

	// ═════════════════════════════════════════════════════════════════
	// setAbsolute — 半值边界(0.5/-0.5): 远离零进位
	// ═════════════════════════════════════════════════════════════════
	private static void testSetAbsoluteRoundHalfAwayFromZero()
	{
		ComplexPoint point = new ComplexPoint();
		point.setAbsolute(0.5f);
		assertEqual(1, point.mAbsolute, "0.5 进位到 1");
		point.setAbsolute(-0.5f);
		assertEqual(-1, point.mAbsolute, "-0.5 进位到 -1");
		point.setAbsolute(1.5f);
		assertEqual(2, point.mAbsolute, "1.5 进位到 2");
		point.setAbsolute(-1.5f);
		assertEqual(-2, point.mAbsolute, "-1.5 进位到 -2");
	}

	// ═════════════════════════════════════════════════════════════════
	// 默认值 — struct new 后 mAbsolute=0
	// ═════════════════════════════════════════════════════════════════
	private static void testDefaultValue()
	{
		ComplexPoint point = new ComplexPoint();
		assertEqual(0, point.mAbsolute, "默认 mAbsolute 为 0");
		assertEqual(0f, point.mRelative, "默认 mRelative 为 0");
	}
}
