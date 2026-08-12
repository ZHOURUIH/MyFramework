using UnityEngine;
using static TestAssert;

// LayoutAutoGrid 深度测试
// 网格排列组件: 横向排列, 满了换行, 确保不超出父节点横向范围
// doAutoGrid → RectTransformExtension.autoGrid(gridSize, interval, keepTopSide, horizontal)
// 测试用 mIntervalX/mIntervalY=0(避开 adjustByScreenScaleAuto 的屏幕缩放调整), 数值可精确断言
//
// 环境: 父 200x100 + 4 子 20x20 + LayoutAutoGrid 组件
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class LayoutAutoGridTest
{
	public static void Run()
	{
		testGridLeft();
		testGridCenter();
		testGridRight();
		testGridMultiRow();
		testGridActiveOnly();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建父 200x100 + count 个 20x20 子节点 + LayoutAutoGrid
	// ═════════════════════════════════════════════════════════════════
	private static LayoutAutoGrid createGrid(out GameObject parentGO, out RectTransform[] children, int count)
	{
		parentGO = new GameObject("GridParent");
		RectTransform parentRT = parentGO.AddComponent<RectTransform>();
		parentRT.sizeDelta = new Vector2(200.0f, 100.0f);
		children = new RectTransform[count];
		for (int i = 0; i < count; ++i)
		{
			GameObject childGO = new GameObject("Cell" + i);
			RectTransform childRT = childGO.AddComponent<RectTransform>();
			childRT.SetParent(parentGO.transform, false);
			childRT.sizeDelta = new Vector2(20.0f, 20.0f);
			children[i] = childRT;
		}
		LayoutAutoGrid grid = parentGO.AddComponent<LayoutAutoGrid>();
		grid.mIntervalX = 0.0f;
		grid.mIntervalY = 0.0f;
		return grid;
	}

	// 基本网格: LEFT 停靠, 4 子 20x20, interval 0
	//   maxColumn = (200-20)/(0+20)+1 = 10; 1 行; contentSize = (80,20)
	//   startPos.x = -200*0.5+10 = -90; startPos.y = 20*0.5-10 = 0
	//   子位置 = (-90,0),(-70,0),(-50,0),(-30,0); 父高度变 20
	private static void testGridLeft()
	{
		LayoutAutoGrid grid = createGrid(out GameObject parentGO, out RectTransform[] children, 4);
		try
		{
			grid.mHorizontal = HORIZONTAL_DIRECTION.LEFT;
			grid.doAutoGrid();
			assertEqual(-90.0f, children[0].localPosition.x, 0.001f, "子0 x=-90");
			assertEqual(-70.0f, children[1].localPosition.x, 0.001f, "子1 x=-70");
			assertEqual(-50.0f, children[2].localPosition.x, 0.001f, "子2 x=-50");
			assertEqual(-30.0f, children[3].localPosition.x, 0.001f, "子3 x=-30");
			assertEqual(0.0f, children[0].localPosition.y, 0.001f, "子0 y=0");
			assertEqual(20.0f, parentGO.GetComponent<RectTransform>().rect.height, 0.001f, "父高度调整为内容高度 20");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// CENTER 停靠: startPos.x = -100+100-40 = -40, 再 +gridSize.x*0.5 = 10 → -30 → 子 x = -30,-10,10,30
	private static void testGridCenter()
	{
		LayoutAutoGrid grid = createGrid(out GameObject parentGO, out RectTransform[] children, 4);
		try
		{
			grid.mHorizontal = HORIZONTAL_DIRECTION.CENTER;
			grid.doAutoGrid();
			assertEqual(-30.0f, children[0].localPosition.x, 0.001f, "CENTER 子0 x=-30");
			assertEqual(-10.0f, children[1].localPosition.x, 0.001f, "CENTER 子1 x=-10");
			assertEqual(10.0f, children[2].localPosition.x, 0.001f, "CENTER 子2 x=10");
			assertEqual(30.0f, children[3].localPosition.x, 0.001f, "CENTER 子3 x=30");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// RIGHT 停靠: startPos.x = -100+200-80 = 20, 再 +gridSize.x*0.5 = 10 → 30 → 子 x = 30,50,70,90
	private static void testGridRight()
	{
		LayoutAutoGrid grid = createGrid(out GameObject parentGO, out RectTransform[] children, 4);
		try
		{
			grid.mHorizontal = HORIZONTAL_DIRECTION.RIGHT;
			grid.doAutoGrid();
			assertEqual(30.0f, children[0].localPosition.x, 0.001f, "RIGHT 子0 x=30");
			assertEqual(50.0f, children[1].localPosition.x, 0.001f, "RIGHT 子1 x=50");
			assertEqual(90.0f, children[3].localPosition.x, 0.001f, "RIGHT 子3 x=90");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// 多行: 父 100x100 + 8 子 30x30, maxColumn = (100-30)/30+1 = 3 → 3 行
	private static void testGridMultiRow()
	{
		GameObject parentGO = new GameObject("GridMulti");
		RectTransform parentRT = parentGO.AddComponent<RectTransform>();
		parentRT.sizeDelta = new Vector2(100.0f, 100.0f);
		RectTransform[] children = new RectTransform[8];
		for (int i = 0; i < 8; ++i)
		{
			GameObject childGO = new GameObject("Cell" + i);
			RectTransform childRT = childGO.AddComponent<RectTransform>();
			childRT.SetParent(parentGO.transform, false);
			childRT.sizeDelta = new Vector2(30.0f, 30.0f);
			children[i] = childRT;
		}
		try
		{
			LayoutAutoGrid grid = parentGO.AddComponent<LayoutAutoGrid>();
			grid.mIntervalX = 0.0f;
			grid.mIntervalY = 0.0f;
			grid.mHorizontal = HORIZONTAL_DIRECTION.LEFT;
			grid.doAutoGrid();
			// 3 列 × 3 行(8 个): contentSize = (90, 90); startPos.x = -50+15 = -35; startPos.y = 90*0.5-15 = 30
			assertEqual(-35.0f, children[0].localPosition.x, 0.001f, "多行 子0 x=-35");
			assertEqual(30.0f, children[0].localPosition.y, 0.001f, "多行 子0 y=30(第1行)");
			assertEqual(-35.0f, children[3].localPosition.x, 0.001f, "多行 子3 x=-35(第2行第1列)");
			assertEqual(0.0f, children[3].localPosition.y, 0.001f, "多行 子3 y=0(第2行)");
			assertEqual(90.0f, parentRT.rect.height, 0.001f, "父高度调整为 3 行内容高 90");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// 只排列激活的子节点: 4 子中 1 个 inactive → 只排 3 个
	private static void testGridActiveOnly()
	{
		LayoutAutoGrid grid = createGrid(out GameObject parentGO, out RectTransform[] children, 4);
		try
		{
			children[2].gameObject.SetActive(false);   // 第 3 个不激活
			grid.mHorizontal = HORIZONTAL_DIRECTION.LEFT;
			grid.doAutoGrid();
			// 只排激活的 3 个: 子0(-90), 子1(-70), 子3(-50); 子2 位置不变(未激活)
			assertEqual(-90.0f, children[0].localPosition.x, 0.001f, "激活子0 x=-90");
			assertEqual(-70.0f, children[1].localPosition.x, 0.001f, "激活子1 x=-70");
			assertEqual(-50.0f, children[3].localPosition.x, 0.001f, "激活子3 x=-50(跳过未激活子2)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}
}
