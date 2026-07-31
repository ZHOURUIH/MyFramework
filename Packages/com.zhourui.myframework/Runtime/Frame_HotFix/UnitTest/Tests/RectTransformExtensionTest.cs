using UnityEngine;
using static RectTransformExtension;
using static TestAssert;

// RectTransformExtension 中不依赖复杂布局场景的纯计算函数测试
// 注: 在默认 anchor(0.5,0.5) 且无拉伸的场景下 sizeDelta 等于 rect.size
public static class RectTransformExtensionTest
{
	public static void Run()
	{
		testSetRectSize();
		testSetRectWidthAndHeight();
		testGetWindowSelfBounds();
		testPositionNoPivotRoundTrip();
		testPositionXyz();
		testWindowInParentCenter();
		testWindowBoundsInParent();
		testAlignToParent();
		testAlignToParentCenter();
		testAlignToOther();
		testPositionNoPivotInParent();
		testGetParentSize();
		testSetRectSizeWithFontSize();
		testAutoGridNull();
	}

	private static RectTransform createRect(GameObject parent, float width, float height)
	{
		GameObject go = new GameObject();
		RectTransform rect = go.AddComponent<RectTransform>();
		if (parent != null)
		{
			rect.SetParent(parent.transform, false);
		}
		// 默认 anchor (0.5,0.5) 且无拉伸, sizeDelta 直接对应 rect.size
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = new Vector2(width, height);
		return rect;
	}

	private static void testSetRectSize()
	{
		GameObject parentGo = new GameObject();
		RectTransform parent = parentGo.AddComponent<RectTransform>();
		parent.sizeDelta = new Vector2(100f, 100f);

		RectTransform child = createRect(parentGo, 10f, 10f);
		// 父节点大小 100x100
		child.setRectSize(new Vector2(50f, 30f));
		assertEqual(50f, child.rect.width, 0.001f, "setRectSize 宽");
		assertEqual(30f, child.rect.height, 0.001f, "setRectSize 高");
		Object.DestroyImmediate(child.gameObject);
		Object.DestroyImmediate(parentGo);
	}

	private static void testSetRectWidthAndHeight()
	{
		GameObject parentGo = new GameObject();
		RectTransform parent = parentGo.AddComponent<RectTransform>();
		parent.sizeDelta = new Vector2(100f, 100f);

		RectTransform child = createRect(parentGo, 20f, 20f);
		child.setRectWidth(60f);
		assertEqual(60f, child.rect.width, 0.001f, "setRectWidth");
		child.setRectHeight(40f);
		assertEqual(40f, child.rect.height, 0.001f, "setRectHeight");
		Object.DestroyImmediate(child.gameObject);
		Object.DestroyImmediate(parentGo);
	}

	private static void testGetWindowSelfBounds()
	{
		GameObject parentGo = new GameObject();
		RectTransform rect = createRect(parentGo, 100f, 50f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		// 左-50 右+50 上+25 下-25 (相对pivot)
		assertEqual(-50f, rect.getWindowLeftInSelf(), 0.001f, "左边界");
		assertEqual(50f, rect.getWindowRightInSelf(), 0.001f, "右边界");
		assertEqual(25f, rect.getWindowTopInSelf(), 0.001f, "上边界");
		assertEqual(-25f, rect.getWindowBottomInSelf(), 0.001f, "下边界");
		Object.DestroyImmediate(rect.gameObject);
		Object.DestroyImmediate(parentGo);
	}

	private static void testPositionNoPivotRoundTrip()
	{
		GameObject parentGo = new GameObject();
		RectTransform rect = createRect(parentGo, 100f, 50f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.localPosition = new Vector3(0f, 0f, 0f);
		// pivot 在中心时 getPositionNoPivot 应等于 localPosition
		Vector3 noPivot = rect.getPositionNoPivot(false);
		assertEqual(0f, noPivot.x, 0.001f, "无 pivot 偏移 X");
		assertEqual(0f, noPivot.y, 0.001f, "无 pivot 偏移 Y");

		// pivot 改为左上角 (0,1), 无 pivot 坐标应偏移
		rect.pivot = new Vector2(0f, 1f);
		rect.localPosition = Vector3.zero;
		Vector3 shifted = rect.getPositionNoPivot(false);
		assertEqual(50f, shifted.x, 0.001f, "pivot 左上角时 X 偏移 +width/2");
		assertEqual(-25f, shifted.y, 0.001f, "pivot 左上角时 Y 偏移 -height/2");
		Object.DestroyImmediate(rect.gameObject);
		Object.DestroyImmediate(parentGo);
	}

	// ─── setPositionX/Y/Z ───────────────────────────────────────────
	private static void testPositionXyz()
	{
		GameObject go = new GameObject();
		RectTransform rect = go.AddComponent<RectTransform>();
		rect.localPosition = Vector3.zero;

		rect.setPositionX(100f);
		assertEqual(100f, rect.localPosition.x, 0.001f, "setPositionX");
		rect.setPositionY(200f);
		assertEqual(200f, rect.localPosition.y, 0.001f, "setPositionY");
		rect.setPositionZ(50f);
		assertEqual(50f, rect.localPosition.z, 0.001f, "setPositionZ");

		// X和Z不应受Y修改影响
		assertEqual(100f, rect.localPosition.x, 0.001f, "setPositionY keeps X");
		assertEqual(50f, rect.localPosition.z, 0.001f, "setPositionY keeps Z");

		Object.DestroyImmediate(go);
	}

	// ─── setWindowInParentCenterX/Y ─────────────────────────────────
	private static void testWindowInParentCenter()
	{
		GameObject parentGo = new GameObject();
		RectTransform rect = createRect(parentGo, 100f, 50f);
		rect.localPosition = new Vector3(50f, 30f, 0f);

		rect.setWindowInParentCenterX();
		assertEqual(0f, rect.localPosition.x, 0.001f, "center X");
		assertEqual(30f, rect.localPosition.y, 0.001f, "center X keeps Y");

		rect.setWindowInParentCenterY();
		assertEqual(0f, rect.localPosition.y, 0.001f, "center Y");

		Object.DestroyImmediate(rect.gameObject);
		Object.DestroyImmediate(parentGo);
	}

	// ─── getWindow*InParent / setWindow*InParent ────────────────────
	private static void testWindowBoundsInParent()
	{
		GameObject parentGo = new GameObject();
		RectTransform rect = createRect(parentGo, 100f, 50f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.localPosition = new Vector3(10f, 20f, 0f);

		// 窗口边界在父节点坐标
		float left = rect.getWindowLeftInParent();
		float right = rect.getWindowRightInParent();
		float top = rect.getWindowTopInParent();
		float bottom = rect.getWindowBottomInParent();

		// localPosition(10,20) + 窗口自身边界(-50,50,25,-25) = (-40,60,45,-5)
		assertEqual(-40f, left, 0.001f, "get left in parent");
		assertEqual(60f, right, 0.001f, "get right in parent");
		assertEqual(45f, top, 0.001f, "get top in parent");
		assertEqual(-5f, bottom, 0.001f, "get bottom in parent");

		// 设置窗口边界
		rect.setWindowLeftInParent(0f);
		assertEqual(50f, rect.localPosition.x, 0.001f, "set left=0 → x=50");
		rect.setWindowRightInParent(100f);
		assertEqual(50f, rect.localPosition.x, 0.001f, "set right=100 → x=50");
		rect.setWindowTopInParent(0f);
		assertEqual(-25f, rect.localPosition.y, 0.001f, "set top=0 → y=-25");
		rect.setWindowBottomInParent(0f);
		assertEqual(25f, rect.localPosition.y, 0.001f, "set bottom=0 → y=25");

		Object.DestroyImmediate(rect.gameObject);
		Object.DestroyImmediate(parentGo);
	}

	// ─── set*ToParent* 对齐父节点边界 ──────────────────────────────
	private static void testAlignToParent()
	{
		GameObject parentGo = new GameObject();
		RectTransform rect = createRect(parentGo, 100f, 50f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.localPosition = new Vector3(100f, 100f, 0f);

		// setTopToParentTop: 内部调用 setWindowTopInParent(rect, getWindowTopInSelf(rect))
		// getWindowTopInSelf(rect) = pivot(0.5,0.5)时 = h/2 = 25
		// setWindowTopInParent(top=25) = setPositionY(25-25) = 0
		rect.setTopToParentTop();
		assertEqual(0f, rect.localPosition.y, 0.001f, "top to parent top Y=0");

		// setBottomToParentBottom
		rect.setBottomToParentBottom();
		assertEqual(0f, rect.localPosition.y, 0.001f, "bottom to parent bottom Y=0");

		// setLeftToParentLeft
		rect.setLeftToParentLeft();
		assertEqual(0f, rect.localPosition.x, 0.001f, "left to parent left X=0");

		// setRightToParentRight
		rect.setRightToParentRight();
		assertEqual(0f, rect.localPosition.x, 0.001f, "right to parent right X=0");

		Object.DestroyImmediate(rect.gameObject);
		Object.DestroyImmediate(parentGo);
	}

	// ─── set*CenterToParent*Center 居中对齐 ────────────────────────
	private static void testAlignToParentCenter()
	{
		GameObject parentGo = new GameObject();
		RectTransform rect = createRect(parentGo, 100f, 50f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.localPosition = new Vector3(100f, 100f, 0f);

        // setTopCenterToParentTopCenter: 顶部边缘对齐父节点顶部(Y=0), 水平居中(X=0)
        // 实现: setWindowTopInParent(getWindowTopInSelf) → Y = topSelf - topSelf = 0
        //       setWindowInParentCenterX → X = 0
        rect.setTopCenterToParentTopCenter();
        assertEqual(0f, rect.localPosition.y, 0.001f, "top center Y=0");
        assertEqual(0f, rect.localPosition.x, 0.001f, "top center X=0");

        // setBottomCenterToParentBottomCenter: 底部边缘对齐父节点底部(Y=0), 水平居中(X=0)
        rect.localPosition = new Vector3(100f, 100f, 0f);
        rect.setBottomCenterToParentBottomCenter();
        assertEqual(0f, rect.localPosition.y, 0.001f, "bottom center Y=0");
        assertEqual(0f, rect.localPosition.x, 0.001f, "bottom center X=0");

        // setLeftCenterToParentLeftCenter: 左边缘对齐父节点左边(X=0), 纵向居中(Y=0)
        rect.localPosition = new Vector3(100f, 100f, 0f);
        rect.setLeftCenterToParentLeftCenter();
        assertEqual(0f, rect.localPosition.x, 0.001f, "left center X=0");
        assertEqual(0f, rect.localPosition.y, 0.001f, "left center Y=0");

        // setRightCenterToParentRightCenter: 右边缘对齐父节点右边(X=0), 纵向居中(Y=0)
        rect.localPosition = new Vector3(100f, 100f, 0f);
        rect.setRightCenterToParentRightCenter();
        assertEqual(0f, rect.localPosition.x, 0.001f, "right center X=0");
		assertEqual(0f, rect.localPosition.y, 0.001f, "right center Y=0");

		Object.DestroyImmediate(rect.gameObject);
		Object.DestroyImmediate(parentGo);
	}

	// ─── set*ToOther* 对齐其他节点 ──────────────────────────────────
	private static void testAlignToOther()
	{
		GameObject parentGo = new GameObject();

		// 当前节点: 100x50, pivot(0.5,0.5), localPos(0,0)
		RectTransform rect = createRect(parentGo, 100f, 50f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.localPosition = Vector3.zero;

		// 其他节点: 200x100, pivot(0.5,0.5), localPos(150f, 75f)
		GameObject otherGo = new GameObject();
		otherGo.transform.SetParent(parentGo.transform, false);
		RectTransform other = otherGo.AddComponent<RectTransform>();
		other.pivot = new Vector2(0.5f, 0.5f);
		other.localPosition = new Vector3(150f, 75f, 0f);
		other.sizeDelta = new Vector2(200f, 100f);

		// setLeftToOtherLeft: rect.left = other.left = 150 - 100 = 50
		rect.setLeftToOtherLeft(other);
		assertEqual(100f, rect.localPosition.x, 0.001f, "left to other left");

		// setLeftToOtherRight: rect.left = other.right = 150 + 100 = 250
		rect.setLeftToOtherRight(other);
		assertEqual(300f, rect.localPosition.x, 0.001f, "left to other right");

        // setRightToOtherLeft: X = other.x - other.w/2 - rect.w/2 = 150 - 100 - 50 = 0
        rect.setRightToOtherLeft(other);
        assertEqual(0f, rect.localPosition.x, 0.001f, "right to other left");

		// setRightToOtherRight: rect.right = other.right = 250
		rect.setRightToOtherRight(other);
		assertEqual(200f, rect.localPosition.x, 0.001f, "right to other right");

		// setTopToOtherTop: rect.top = other.top = 75 + 50 = 125
		rect.setTopToOtherTop(other);
		assertEqual(100f, rect.localPosition.y, 0.001f, "top to other top");

		// setTopToOtherBottom: rect.top = other.bottom = 75 - 50 = 25
		rect.setTopToOtherBottom(other);
		assertEqual(0f, rect.localPosition.y, 0.001f, "top to other bottom");

		// setBottomToOtherTop: rect.bottom = other.top = 125
		rect.setBottomToOtherTop(other);
		assertEqual(150f, rect.localPosition.y, 0.001f, "bottom to other top");

		// setBottomToOtherBottom: rect.bottom = other.bottom = 25
		rect.setBottomToOtherBottom(other);
		assertEqual(50f, rect.localPosition.y, 0.001f, "bottom to other bottom");

		// 不同父节点: 源码内部 logError 后不修改, 跳过此测试避免 error log
		// (setLeftToOtherLeft 等函数在父节点不同时无条件 logError)

		Object.DestroyImmediate(rect.gameObject);
		Object.DestroyImmediate(otherGo);
		Object.DestroyImmediate(parentGo);
	}

	// ─── setPositionNoPivot / getPositionNoPivotInParent ────────────
	private static void testPositionNoPivotInParent()
	{
		GameObject parentGo = new GameObject();
		RectTransform parentRect = parentGo.AddComponent<RectTransform>();
		RectTransform rect = createRect(parentGo, 100f, 50f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.localPosition = new Vector3(10f, 20f, 0f);

		// getPositionNoPivotInParent: 父节点存在时等于 localPosition（pivot居中）
		Vector3 p = rect.getPositionNoPivotInParent(false);
		assertEqual(10f, p.x, 0.001f, "noPivotInParent X");
		assertEqual(20f, p.y, 0.001f, "noPivotInParent Y");

		// setPositionNoPivotInParent: pivot居中时相当于直接设localPosition
		rect.setPositionNoPivotInParent(new Vector3(50f, 30f, 0f), false);
		assertEqual(50f, rect.localPosition.x, 0.001f, "set noPivotInParent X");
		assertEqual(30f, rect.localPosition.y, 0.001f, "set noPivotInParent Y");

		// 没有父节点时: getPositionNoPivotInParent = getPositionNoPivot
		GameObject orphanGo = new GameObject();
		RectTransform orphan = orphanGo.AddComponent<RectTransform>();
		orphan.pivot = new Vector2(0.5f, 0.5f);
		orphan.sizeDelta = new Vector2(100f, 50f);
		orphan.localPosition = Vector3.zero;
		Vector3 pOrphan = orphan.getPositionNoPivotInParent(false);
		assertEqual(0f, pOrphan.x, 0.001f, "orphan noPivotInParent X");

		// 直接调用 setPositionNoPivot: pivot 居中时 = localPosition
		orphan.pivot = new Vector2(0.5f, 0.5f);
		orphan.setPositionNoPivot(new Vector3(30f, 40f, 0f), false);
		assertEqual(30f, orphan.localPosition.x, 0.001f, "setPositionNoPivot X");
		assertEqual(40f, orphan.localPosition.y, 0.001f, "setPositionNoPivot Y");

		// pivot 非中心时偏移
		orphan.pivot = new Vector2(0f, 1f); // 左上角
		orphan.setPositionNoPivot(Vector3.zero, false);
		assertEqual(-50f, orphan.localPosition.x, 0.001f, "pivot(0,1) X = -w/2");
		assertEqual(25f, orphan.localPosition.y, 0.001f, "pivot(0,1) Y = +h/2");

		Object.DestroyImmediate(orphan.gameObject);
		Object.DestroyImmediate(orphanGo);
		Object.DestroyImmediate(rect.gameObject);
		Object.DestroyImmediate(parentGo);
	}

	// ─── getParentSize ──────────────────────────────────────────────
	private static void testGetParentSize()
	{
		GameObject parentGo = new GameObject();
		RectTransform parent = parentGo.AddComponent<RectTransform>();
		parent.sizeDelta = new Vector2(200f, 100f);

		RectTransform child = createRect(parentGo, 50f, 50f);
		Vector2 pSize = child.getParentSize();
		assertEqual(200f, pSize.x, 0.001f, "parent size X");
		assertEqual(100f, pSize.y, 0.001f, "parent size Y");

		// 无父节点
		GameObject orphanGo = new GameObject();
		RectTransform orphan = orphanGo.AddComponent<RectTransform>();
		Vector2 noParent = orphan.getParentSize();
		assertEqual(0f, noParent.x, 0.001f, "no parent size X=0");
		assertEqual(0f, noParent.y, 0.001f, "no parent size Y=0");

		Object.DestroyImmediate(orphan.gameObject);
		Object.DestroyImmediate(orphanGo);
		Object.DestroyImmediate(child.gameObject);
		Object.DestroyImmediate(parentGo);
	}

	// ─── setRectSizeWithFontSize ────────────────────────────────────
	private static void testSetRectSizeWithFontSize()
	{
		// null 保护
		RectTransform rect0 = null;
		rect0.setRectSizeWithFontSize(new Vector2(100f, 50f), 14);
		// 正常调用
		GameObject go = new GameObject();
		RectTransform rect = go.AddComponent<RectTransform>();
		rect.sizeDelta = new Vector2(10f, 10f);
		// 不崩溃即通过（内部依赖 TMPro 组件，但 null 分支应先返回）
		// 实际调用需要 TextMeshProUGUI 组件，此处只测 null 分支
		Object.DestroyImmediate(go);
	}

	// ─── autoGrid / adjustRectTransformToContainsAllChildRect null 分支 ─
	private static void testAutoGridNull()
	{
		// 所有 autoGrid 传入 null 应安全返回
		RectTransform rect0 = null;
		rect0.autoGrid(Vector2.zero, Vector2.zero, false);
		rect0.autoGridHorizontal(0f);
		rect0.autoGridVertical(0f);
		rect0.autoGridHorizontalCenter(0f);
		rect0.adjustRectTransformToContainsAllChildRect();

		// 空子节点列表也应安全
		GameObject go = new GameObject();
		RectTransform rect = go.AddComponent<RectTransform>();
		rect.sizeDelta = new Vector2(100f, 100f);

		// 有父节点但无子节点
		rect.autoGrid(new Vector2(10f, 10f), Vector2.zero, false);
		rect.autoGridHorizontal(0);
		rect.autoGridVertical(0);
		rect.autoGridHorizontalCenter(0);
		rect.adjustRectTransformToContainsAllChildRect();

		Object.DestroyImmediate(go);
	}
}
