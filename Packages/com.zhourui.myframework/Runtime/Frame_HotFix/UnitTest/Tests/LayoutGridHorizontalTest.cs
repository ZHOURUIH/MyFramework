using UnityEngine;
using static TestAssert;

// LayoutGridHorizontal 深度测试
// 横向排列组件: 从左往右排列所有激活子节点, 不换行, 可自动调整父节点宽度
// doAutoGrid → RectTransformExtension.autoGridHorizontal(interval, changeRootPosSize)
// 测试用 mInterval=0(避开 adjustByScreenScaleAuto 的屏幕缩放调整), 数值可精确断言
//
// 环境: 父 100x100 + 3 子 20x20 + LayoutGridHorizontal 组件
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class LayoutGridHorizontalTest
{
	public static void Run()
	{
		testGridHorizontalBasic();
		testGridHorizontalInterval();
		testGridHorizontalNoChangeRoot();
		testGridHorizontalActiveOnly();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建父 100x100 + count 个 20x20 子节点 + LayoutGridHorizontal
	// ═════════════════════════════════════════════════════════════════
	private static LayoutGridHorizontal createGrid(out GameObject parentGO, out RectTransform[] children, int count)
	{
		parentGO = new GameObject("GridHParent");
		RectTransform parentRT = parentGO.AddComponent<RectTransform>();
		parentRT.sizeDelta = new Vector2(100.0f, 100.0f);
		children = new RectTransform[count];
		for (int i = 0; i < count; ++i)
		{
			GameObject childGO = new GameObject("HCell" + i);
			RectTransform childRT = childGO.AddComponent<RectTransform>();
			childRT.SetParent(parentGO.transform, false);
			childRT.sizeDelta = new Vector2(20.0f, 20.0f);
			children[i] = childRT;
		}
		LayoutGridHorizontal grid = parentGO.AddComponent<LayoutGridHorizontal>();
		grid.mInterval = 0.0f;
		return grid;
	}

	// 基本横向排列: 3 子 20x20, interval 0, changeRootPosSize=true
	//   width = 60; rootWidth = 60(偶数); 父宽变 60
	//   keepLeftSide: 父 localPosition.x += (60-100)*0.5 = -20
	//   currentLeft = -60*0.5 = -30 → 子 x = -30+10=-20, -30+30=0, -30+50=20
	private static void testGridHorizontalBasic()
	{
		LayoutGridHorizontal grid = createGrid(out GameObject parentGO, out RectTransform[] children, 3);
		try
		{
			grid.mChangeRootSize = true;   // 默认 false, 必须显式开启才会调整父宽度
			grid.doAutoGrid();
			assertEqual(60.0f, parentGO.GetComponent<RectTransform>().rect.width, 0.001f, "父宽度调整为 60");
			assertEqual(-20.0f, parentGO.transform.localPosition.x, 0.001f, "keepLeftSide 父位置补偿 -20");
			assertEqual(-20.0f, children[0].localPosition.x, 0.001f, "子0 x=-20(最左)");
			assertEqual(0.0f, children[1].localPosition.x, 0.001f, "子1 x=0");
			assertEqual(20.0f, children[2].localPosition.x, 0.001f, "子2 x=20(最右)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// interval=5: 3 子 20x20 → width = 60+5*2 = 70(偶数)
	private static void testGridHorizontalInterval()
	{
		LayoutGridHorizontal grid = createGrid(out GameObject parentGO, out RectTransform[] children, 3);
		try
		{
			grid.mInterval = 5.0f;
			grid.mChangeRootSize = true;   // 默认 false, 必须显式开启才会调整父宽度
			grid.doAutoGrid();
			assertEqual(70.0f, parentGO.GetComponent<RectTransform>().rect.width, 0.001f, "带间隔父宽度 = 60+5*2 = 70");
			// 子位置: left=-35, 子0=-35+10=-25, 子1=-25-20-5=0, 子2=0-20-5=25
			assertEqual(-25.0f, children[0].localPosition.x, 0.001f, "间隔 5 子0 x=-25");
			assertEqual(0.0f, children[1].localPosition.x, 0.001f, "间隔 5 子1 x=0");
			assertEqual(25.0f, children[2].localPosition.x, 0.001f, "间隔 5 子2 x=25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// changeRootPosSize=false: 不改父节点大小, 只排列子节点
	private static void testGridHorizontalNoChangeRoot()
	{
		LayoutGridHorizontal grid = createGrid(out GameObject parentGO, out RectTransform[] children, 3);
		try
		{
			grid.mChangeRootSize = false;
			grid.doAutoGrid();
			assertEqual(100.0f, parentGO.GetComponent<RectTransform>().rect.width, 0.001f, "不改父宽度");
			assertEqual(0.0f, parentGO.transform.localPosition.x, 0.001f, "父位置不补偿");
			// 子节点仍按父原始宽度排列: left=-50 → 子 x = -40,-20,0
			assertEqual(-40.0f, children[0].localPosition.x, 0.001f, "子0 x=-40");
			assertEqual(-20.0f, children[1].localPosition.x, 0.001f, "子1 x=-20");
			assertEqual(0.0f, children[2].localPosition.x, 0.001f, "子2 x=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// 只排列激活的子节点: 3 子中 1 个 inactive → 只排 2 个
	private static void testGridHorizontalActiveOnly()
	{
		LayoutGridHorizontal grid = createGrid(out GameObject parentGO, out RectTransform[] children, 3);
		try
		{
			children[1].gameObject.SetActive(false);   // 中间不激活
			grid.mChangeRootSize = true;   // 默认 false, 必须显式开启才会调整父宽度
			grid.doAutoGrid();
			// 只排激活的 2 个: width = 40 → 父宽 40; keepLeftSide → 父 x = (40-100)*0.5 = -30
			// currentLeft = -20 → 子0 = -20+10 = -10; 子2 = -20+20+10 = 10
			assertEqual(40.0f, parentGO.GetComponent<RectTransform>().rect.width, 0.001f, "只排 2 个 → 父宽 40");
			assertEqual(-10.0f, children[0].localPosition.x, 0.001f, "子0 x=-10");
			assertEqual(10.0f, children[2].localPosition.x, 0.001f, "激活子2 x=10(跳过未激活子1)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}
}
