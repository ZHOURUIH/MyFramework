using UnityEngine;
using static TestAssert;

// LayoutGridVertical 深度测试
// 纵向排列组件: 从上往下排列所有激活子节点, 自动调整父节点高度
// doAutoGrid → RectTransformExtension.autoGridVertical(interval, keepTopSide)
// 测试用 mInterval=0(避开 adjustByScreenScaleAuto 的屏幕缩放调整), 数值可精确断言
//
// 环境: 父 100x100 + 3 子 20x20 + LayoutGridVertical 组件
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class LayoutGridVerticalTest
{
	public static void Run()
	{
		testGridVerticalBasic();
		testGridVerticalInterval();
		testGridVerticalFromBottom();
		testGridVerticalKeepTopSide();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建父 100x100 + count 个 20x20 子节点 + TestLayoutGridVertical
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutGridVertical createGrid(out GameObject parentGO, out RectTransform[] children, int count)
	{
		parentGO = new GameObject("GridVParent");
		RectTransform parentRT = parentGO.AddComponent<RectTransform>();
		parentRT.sizeDelta = new Vector2(100.0f, 100.0f);
		children = new RectTransform[count];
		for (int i = 0; i < count; ++i)
		{
			GameObject childGO = new GameObject("VCell" + i);
			RectTransform childRT = childGO.AddComponent<RectTransform>();
			childRT.SetParent(parentGO.transform, false);
			childRT.sizeDelta = new Vector2(20.0f, 20.0f);
			children[i] = childRT;
		}
		TestLayoutGridVertical grid = parentGO.AddComponent<TestLayoutGridVertical>();
		grid.mInterval = 0.0f;
		return grid;
	}

	// 基本纵向排列: 3 子 20x20, interval 0, keepTopSide 默认
	//   height = 60; rootHeight = 60(偶数); 父高变 60
	//   keepTopSide: 父 localPosition.y += (100-60)*0.5 = 20
	//   currentTop = 60*0.5 = 30 → 子 y = 30-10=20, 30-30=0, 30-50=-20
	private static void testGridVerticalBasic()
	{
		LayoutGridVertical grid = createGrid(out GameObject parentGO, out RectTransform[] children, 3);
		try
		{
			grid.doAutoGrid();
			assertEqual(60.0f, parentGO.GetComponent<RectTransform>().rect.height, 0.001f, "父高度调整为 60");
			assertEqual(20.0f, parentGO.transform.localPosition.y, 0.001f, "keepTopSide 父位置补偿 +20");
			assertEqual(20.0f, children[0].localPosition.y, 0.001f, "子0 y=20(最上)");
			assertEqual(0.0f, children[1].localPosition.y, 0.001f, "子1 y=0");
			assertEqual(-20.0f, children[2].localPosition.y, 0.001f, "子2 y=-20(最下)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// interval=5: 3 子 20x20 → height = 60+5*2 = 70(偶数)
	private static void testGridVerticalInterval()
	{
		LayoutGridVertical grid = createGrid(out GameObject parentGO, out RectTransform[] children, 3);
		try
		{
			grid.mInterval = 5.0f;
			grid.doAutoGrid();
			assertEqual(70.0f, parentGO.GetComponent<RectTransform>().rect.height, 0.001f, "带间隔父高度 = 60+5*2 = 70");
			// 子位置: top=35, 子0=35-10=25, 子1=25-20-5=0, 子2=0-20-5=-25
			assertEqual(25.0f, children[0].localPosition.y, 0.001f, "间隔 5 子0 y=25");
			assertEqual(0.0f, children[1].localPosition.y, 0.001f, "间隔 5 子1 y=0");
			assertEqual(-25.0f, children[2].localPosition.y, 0.001f, "间隔 5 子2 y=-25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// fromTopToBottom=false: 从下往上排列
	private static void testGridVerticalFromBottom()
	{
		TestLayoutGridVertical grid = createGrid(out GameObject parentGO, out RectTransform[] children, 3);
		try
		{
			grid.doAutoGridFromBottomForTest();
			// 从下往上: currentBottom = -60*0.5 = -30
			//   子0 = -30+10 = -20(最下), 子1 = -30+20+10 = 0, 子2 = -30+40+10 = 20(最上)
			assertEqual(-20.0f, children[0].localPosition.y, 0.001f, "从下往上 子0 y=-20(最下)");
			assertEqual(0.0f, children[1].localPosition.y, 0.001f, "从下往上 子1 y=0");
			assertEqual(20.0f, children[2].localPosition.y, 0.001f, "从下往上 子2 y=20(最上)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// keepTopSide 语义: 父高度不变时(子节点总高 == 父高)不补偿位置
	private static void testGridVerticalKeepTopSide()
	{
		GameObject parentGO = new GameObject("GridVKeep");
		RectTransform parentRT = parentGO.AddComponent<RectTransform>();
		parentRT.sizeDelta = new Vector2(100.0f, 60.0f);   // 父高恰好 = 3 子总高
		RectTransform[] children = new RectTransform[3];
		for (int i = 0; i < 3; ++i)
		{
			GameObject childGO = new GameObject("VCell" + i);
			RectTransform childRT = childGO.AddComponent<RectTransform>();
			childRT.SetParent(parentGO.transform, false);
			childRT.sizeDelta = new Vector2(20.0f, 20.0f);
			children[i] = childRT;
		}
		try
		{
			LayoutGridVertical grid = parentGO.AddComponent<LayoutGridVertical>();
			grid.mInterval = 0.0f;
			grid.doAutoGrid();
			// 父高 60 == 内容高 60 → 高度不变 → 位置不补偿
			assertEqual(0.0f, parentGO.transform.localPosition.y, 0.001f, "父高不变时位置不补偿");
			assertEqual(60.0f, parentRT.rect.height, 0.001f, "父高度保持 60");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 LayoutGridVertical 的 fromTopToBottom=false 路径
// ═════════════════════════════════════════════════════════════════
public class TestLayoutGridVertical : LayoutGridVertical
{
	public void doAutoGridFromBottomForTest()
	{
		(transform as RectTransform).autoGridVertical(mInterval, 0.0f, 0.0f, 0.0f, true, false);
	}
}
