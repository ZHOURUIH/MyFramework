using UnityEngine;
using static TestAssert;

// PaddingAnchor 深度测试
// 停靠/拉伸锚点组件, updateRect 是纯 RectTransform 数学:
//   PADDING_PARENT_SIDE: 停靠到父节点某条边(LEFT_IN/RIGHT_OUT/CENTER 等), 位置 = 边距 + 尺寸补偿
//   STRETCH_TO_PARENT_SIDE: 按锚点拉伸, 尺寸 = right-left, 位置 = 中点
// 测试用 mRelative=0(绝对边距) + CENTER 模式, 数值不依赖父节点边坐标(mParentSides 乘 0), 可精确断言
//
// 环境: 父节点 100x100 + 子节点 20x10(挂父下) + PaddingAnchor(子组件)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class PaddingAnchorTest
{
	public static void Run()
	{
		testPaddingLeftInTopIn();
		testPaddingRightOutBottomOut();
		testPaddingCenter();
		testPaddingCenterWithAbsolute();
		testStretchToParentSide();
		testStretchZero();
		testUpdateRectDirty();
		testSetAnchorMode();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建父 100x100 + 子 20x10 + PaddingAnchor
	// ═════════════════════════════════════════════════════════════════
	private static PaddingAnchor createAnchor(out GameObject parentGO, out RectTransform childRT)
	{
		parentGO = new GameObject("AnchorParent");
		RectTransform parentRT = parentGO.AddComponent<RectTransform>();
		parentRT.sizeDelta = new Vector2(100.0f, 100.0f);
		GameObject childGO = new GameObject("AnchorChild");
		childRT = childGO.AddComponent<RectTransform>();
		childRT.SetParent(parentGO.transform, false);
		childRT.sizeDelta = new Vector2(20.0f, 10.0f);
		return childGO.AddComponent<PaddingAnchor>();
	}

	// PADDING_PARENT_SIDE: LEFT_IN + TOP_IN + 绝对边距(10,10)
	//   pos.x = 0*S0.x + 10 + 20*0.5 = 20; pos.y = 0*S1.y + 10 - 10*0.5 = 5
	private static void testPaddingLeftInTopIn()
	{
		PaddingAnchor anchor = createAnchor(out GameObject parentGO, out RectTransform childRT);
		try
		{
			anchor.mHorizontalNearSide = HORIZONTAL_PADDING.LEFT_IN;
			anchor.mVerticalNearSide = VERTICAL_PADDING.TOP_IN;
			anchor.setAnchorMode(ANCHOR_MODE.PADDING_PARENT_SIDE);
			anchor.mDistanceToBoard[0].mRelative = 0.0f;
			anchor.mDistanceToBoard[0].setAbsolute(10);   // 左边距 10
			anchor.mDistanceToBoard[1].mRelative = 0.0f;
			anchor.mDistanceToBoard[1].setAbsolute(10);   // 上边距 10
			anchor.updateRect();
			Vector3 pos = childRT.localPosition;
			assertEqual(20.0f, pos.x, 0.001f, "LEFT_IN 左边距10 → x=10+20/2=20");
			assertEqual(5.0f, pos.y, 0.001f, "TOP_IN 上边距10 → y=10-10/2=5");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// PADDING_PARENT_SIDE: RIGHT_OUT + BOTTOM_OUT + 绝对边距(10,10)
	//   pos.x = 0*S2.x + 10 + 20*0.5 = 20; pos.y = 0*S3.y + 10 - 10*0.5 = 5
	private static void testPaddingRightOutBottomOut()
	{
		PaddingAnchor anchor = createAnchor(out GameObject parentGO, out RectTransform childRT);
		try
		{
			anchor.mHorizontalNearSide = HORIZONTAL_PADDING.RIGHT_OUT;
			anchor.mVerticalNearSide = VERTICAL_PADDING.BOTTOM_OUT;
			anchor.setAnchorMode(ANCHOR_MODE.PADDING_PARENT_SIDE);
			anchor.mDistanceToBoard[2].mRelative = 0.0f;
			anchor.mDistanceToBoard[2].setAbsolute(10);   // 右边距 10
			anchor.mDistanceToBoard[3].mRelative = 0.0f;
			anchor.mDistanceToBoard[3].setAbsolute(10);   // 下边距 10
			anchor.updateRect();
			Vector3 pos = childRT.localPosition;
			assertEqual(20.0f, pos.x, 0.001f, "RIGHT_OUT 右边距10 → x=10+20/2=20");
			assertEqual(5.0f, pos.y, 0.001f, "BOTTOM_OUT 下边距10 → y=10-10/2=5");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// PADDING_PARENT_SIDE: CENTER + CENTER + relative/absolute 全 0 → 居中 (0,0)
	// 注意: 必须先 setAnchorMode 再设字段——setAnchorMode 内部会调 setToPaddingParentSide 覆盖 relative/absolute 字段
	private static void testPaddingCenter()
	{
		PaddingAnchor anchor = createAnchor(out GameObject parentGO, out RectTransform childRT);
		try
		{
			anchor.setAnchorMode(ANCHOR_MODE.PADDING_PARENT_SIDE);
			anchor.mHorizontalNearSide = HORIZONTAL_PADDING.CENTER;
			anchor.mVerticalNearSide = VERTICAL_PADDING.CENTER;
			anchor.mHorizontalPositionRelative = 0.0f;
			anchor.mHorizontalPositionAbsolute = 0;
			anchor.mVerticalPositionRelative = 0.0f;
			anchor.mVerticalPositionAbsolute = 0;
			anchor.updateRect();
			Vector3 pos = childRT.localPosition;
			assertEqual(0.0f, pos.x, 0.001f, "CENTER 居中 → x=0");
			assertEqual(0.0f, pos.y, 0.001f, "CENTER 居中 → y=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// PADDING_PARENT_SIDE: CENTER 横 + absolute=10 → x=10 (只依赖 parentSize*0.5=50 的 relative=0)
	// 注意: 必须先 setAnchorMode 再设字段——setAnchorMode 内部会调 setToPaddingParentSide 覆盖 relative/absolute 字段
	private static void testPaddingCenterWithAbsolute()
	{
		PaddingAnchor anchor = createAnchor(out GameObject parentGO, out RectTransform childRT);
		try
		{
			anchor.setAnchorMode(ANCHOR_MODE.PADDING_PARENT_SIDE);
			anchor.mHorizontalNearSide = HORIZONTAL_PADDING.CENTER;
			anchor.mVerticalNearSide = VERTICAL_PADDING.CENTER;
			anchor.mHorizontalPositionRelative = 0.0f;
			anchor.mHorizontalPositionAbsolute = 10;
			anchor.mVerticalPositionRelative = 0.0f;
			anchor.mVerticalPositionAbsolute = -5;
			anchor.updateRect();
			Vector3 pos = childRT.localPosition;
			assertEqual(10.0f, pos.x, 0.001f, "CENTER + absolute=10 → x=10");
			assertEqual(-5.0f, pos.y, 0.001f, "CENTER + absolute=-5 → y=-5");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// STRETCH_TO_PARENT_SIDE: 锚点 left=-20, top=30, right=80, bottom=0 (relative 全 0)
	//   newSize.x = 80-(-20) = 100; newSize.y = 30-0 = 30
	//   pos.x = (80-20)/2 = 30; pos.y = (30+0)/2 = 15
	private static void testStretchToParentSide()
	{
		PaddingAnchor anchor = createAnchor(out GameObject parentGO, out RectTransform childRT);
		try
		{
			anchor.mAnchorMode = ANCHOR_MODE.STRETCH_TO_PARENT_SIDE;
			anchor.setAnchorMode(ANCHOR_MODE.STRETCH_TO_PARENT_SIDE);
			anchor.mAnchorPoint[0].mRelative = 0.0f;
			anchor.mAnchorPoint[0].setAbsolute(-20);   // 左
			anchor.mAnchorPoint[1].mRelative = 0.0f;
			anchor.mAnchorPoint[1].setAbsolute(30);    // 上
			anchor.mAnchorPoint[2].mRelative = 0.0f;
			anchor.mAnchorPoint[2].setAbsolute(80);    // 右
			anchor.mAnchorPoint[3].mRelative = 0.0f;
			anchor.mAnchorPoint[3].setAbsolute(0);     // 下
			anchor.updateRect();
			Vector2 size = childRT.sizeDelta;
			Vector3 pos = childRT.localPosition;
			assertEqual(100.0f, size.x, 0.001f, "拉伸宽 = 80-(-20) = 100");
			assertEqual(30.0f, size.y, 0.001f, "拉伸高 = 30-0 = 30");
			assertEqual(30.0f, pos.x, 0.001f, "位置 x = (80-20)/2 = 30");
			assertEqual(15.0f, pos.y, 0.001f, "位置 y = (30+0)/2 = 15");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// STRETCH_TO_PARENT_SIDE: 全 0 锚点 → size=(0,0), pos=(0,0) (0 不是负数, 不触发 logError)
	private static void testStretchZero()
	{
		PaddingAnchor anchor = createAnchor(out GameObject parentGO, out RectTransform childRT);
		try
		{
			anchor.setAnchorMode(ANCHOR_MODE.STRETCH_TO_PARENT_SIDE);
			anchor.mAnchorPoint[0].setAbsolute(0);
			anchor.mAnchorPoint[1].setAbsolute(0);
			anchor.mAnchorPoint[2].setAbsolute(0);
			anchor.mAnchorPoint[3].setAbsolute(0);
			anchor.updateRect();
			Vector2 size = childRT.sizeDelta;
			assertEqual(0.0f, size.x, 0.001f, "全 0 锚点 → 宽 0");
			assertEqual(0.0f, size.y, 0.001f, "全 0 锚点 → 高 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// updateRect: mDirty 语义(初始 true 执行, 之后跳过, force 强制)
	private static void testUpdateRectDirty()
	{
		PaddingAnchor anchor = createAnchor(out GameObject parentGO, out RectTransform childRT);
		try
		{
			anchor.mHorizontalNearSide = HORIZONTAL_PADDING.CENTER;
			anchor.mVerticalNearSide = VERTICAL_PADDING.CENTER;
			anchor.setAnchorMode(ANCHOR_MODE.PADDING_PARENT_SIDE);
			// 第一次: mDirty 初始 true → 执行
			anchor.updateRect();
			Vector3 firstPos = childRT.localPosition;
			// 第二次: mDirty 已清除 → 不执行, 位置不变
			anchor.mHorizontalPositionAbsolute = 50;
			anchor.updateRect();
			Vector3 secondPos = childRT.localPosition;
			assertEqual(firstPos.x, secondPos.x, 0.001f, "mDirty=false 时 updateRect 不重算");
			// force=true → 强制重算
			anchor.updateRect(true);
			Vector3 thirdPos = childRT.localPosition;
			assertEqual(50.0f, thirdPos.x, 0.001f, "updateRect(true) 强制按新配置重算");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// setAnchorMode: 模式切换 + getAnchorMode 读回
	private static void testSetAnchorMode()
	{
		PaddingAnchor anchor = createAnchor(out GameObject parentGO, out RectTransform childRT);
		try
		{
			anchor.setAnchorMode(ANCHOR_MODE.PADDING_PARENT_SIDE);
			assertTrue(ANCHOR_MODE.PADDING_PARENT_SIDE == anchor.getAnchorMode(), "PADDING_PARENT_SIDE 读回");
			anchor.setAnchorMode(ANCHOR_MODE.STRETCH_TO_PARENT_SIDE);
			assertTrue(ANCHOR_MODE.STRETCH_TO_PARENT_SIDE == anchor.getAnchorMode(), "STRETCH_TO_PARENT_SIDE 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}
}
