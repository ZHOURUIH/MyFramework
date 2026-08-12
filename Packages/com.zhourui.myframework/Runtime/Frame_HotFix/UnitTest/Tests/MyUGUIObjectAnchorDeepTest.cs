using UnityEngine;
using static TestAssert;

// myUGUIObject 锚点定位族深度测试(纯数学, 父停靠/边界值/相对定位)
//   setTopToParentTop 等: setXInParent(parent.getXInSelf()) → setPositionY(top - getTopInSelf())
//   getLeftInParent 等: getPosition() + getLeftInSelf()
//   getLeftInSelf: -size.x*pivot.x; getTopInSelf: size.y*(1-pivot.y)
// 环境: parent(100x100, pivot 0.5) + child(50x50, pivot 0.5), 裸 GO + setParent(child, false)
// 数学基准(pivot 0.5): child 边界 ±25; parent 边界 ±50; getPositionNoPivot == localPosition
public static class MyUGUIObjectAnchorDeepTest
{
	public static void Run()
	{
		testSetTopToParentTop();
		testSetBottomToParentBottom();
		testSetLeftToParentLeft();
		testSetRightToParentRight();
		testSetTopCenterToParentTopCenter();
		testSetBottomCenterToParentBottomCenter();
		testSetLeftCenterToParentLeftCenter();
		testSetRightCenterToParentRightCenter();
		testSetInParentCenter();
		testGetInParent();
		testGetInSelf();
		testGetPositionNoPivot();
		testSetXInParentDirect();
		testRelativeOther();
		testRelativeSameSide();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createWindow(GameObject go, string name, Vector2 size)
	{
		go.name = name;
		go.AddComponent<RectTransform>();
		myUGUIObject obj = new myUGUIObject();
		obj.setIsNewObject(true);
		obj.setObject(go);
		obj.init();
		obj.setSize(size);
		return obj;
	}

	// parent(100x100) + child(50x50) 树
	private static void createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child)
	{
		parentGo = new GameObject("AnchorParent");
		parent = createWindow(parentGo, "AnchorParent", new Vector2(100.0f, 100.0f));
		childGo = new GameObject("AnchorChild");
		child = createWindow(childGo, "AnchorChild", new Vector2(50.0f, 50.0f));
		child.setParent(parent, false);
	}

	// 顶停靠: y = parentTop(50) - childTopInSelf(25) = 25
	private static void testSetTopToParentTop()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setTopToParentTop();
			assertEqual(25.0f, child.getPosition().y, 0.001f, "child 顶 = parent 顶 → y=25");
			assertEqual(50.0f, child.getTopInParent(), 0.001f, "顶边界 = parent 顶 50");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 底停靠: y = parentBottom(-50) - childBottomInSelf(-25) = -25
	private static void testSetBottomToParentBottom()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setBottomToParentBottom();
			assertEqual(-25.0f, child.getPosition().y, 0.001f, "child 底 = parent 底 → y=-25");
			assertEqual(-50.0f, child.getBottomInParent(), 0.001f, "底边界 = parent 底 -50");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 左停靠: x = parentLeft(-50) - childLeftInSelf(-25) = -25
	private static void testSetLeftToParentLeft()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setLeftToParentLeft();
			assertEqual(-25.0f, child.getPosition().x, 0.001f, "child 左 = parent 左 → x=-25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 右停靠: x = parentRight(50) - childRightInSelf(25) = 25
	private static void testSetRightToParentRight()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setRightToParentRight();
			assertEqual(25.0f, child.getPosition().x, 0.001f, "child 右 = parent 右 → x=25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 顶中: y=25, x=0
	private static void testSetTopCenterToParentTopCenter()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setTopCenterToParentTopCenter();
			assertEqual(25.0f, child.getPosition().y, 0.001f, "顶中 y=25");
			assertEqual(0.0f, child.getPosition().x, 0.001f, "顶中 x=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 底中: y=-25, x=0
	private static void testSetBottomCenterToParentBottomCenter()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setBottomCenterToParentBottomCenter();
			assertEqual(-25.0f, child.getPosition().y, 0.001f, "底中 y=-25");
			assertEqual(0.0f, child.getPosition().x, 0.001f, "底中 x=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 左中: x=-25, y=0
	private static void testSetLeftCenterToParentLeftCenter()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setLeftCenterToParentLeftCenter();
			assertEqual(-25.0f, child.getPosition().x, 0.001f, "左中 x=-25");
			assertEqual(0.0f, child.getPosition().y, 0.001f, "左中 y=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 右中: x=25, y=0
	private static void testSetRightCenterToParentRightCenter()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setRightCenterToParentRightCenter();
			assertEqual(25.0f, child.getPosition().x, 0.001f, "右中 x=25");
			assertEqual(0.0f, child.getPosition().y, 0.001f, "右中 y=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 父中心: setInParentCenterX/Y → 0
	private static void testSetInParentCenter()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setPosition(new Vector3(30.0f, 40.0f, 0.0f));
			child.setInParentCenterX();
			assertEqual(0.0f, child.getPosition().x, 0.001f, "setInParentCenterX → x=0");
			assertEqual(40.0f, child.getPosition().y, 0.001f, "y 不受影响");
			child.setInParentCenterY();
			assertEqual(0.0f, child.getPosition().y, 0.001f, "setInParentCenterY → y=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// getLeftInParent 等: 位置 0 时 = ±25
	private static void testGetInParent()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setPosition(Vector3.zero);
			assertEqual(-25.0f, child.getLeftInParent(), 0.001f, "左边界 -25");
			assertEqual(25.0f, child.getRightInParent(), 0.001f, "右边界 25");
			assertEqual(25.0f, child.getTopInParent(), 0.001f, "顶边界 25");
			assertEqual(-25.0f, child.getBottomInParent(), 0.001f, "底边界 -25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// getXInSelf: 与 pivot/size 的纯数学
	private static void testGetInSelf()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			assertEqual(-25.0f, child.getLeftInSelf(), 0.001f, "leftInSelf = -50*0.5 = -25");
			assertEqual(25.0f, child.getRightInSelf(), 0.001f, "rightInSelf = 50*0.5 = 25");
			assertEqual(25.0f, child.getTopInSelf(), 0.001f, "topInSelf = 50*0.5 = 25");
			assertEqual(-25.0f, child.getBottomInSelf(), 0.001f, "bottomInSelf = -50*0.5 = -25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// getPositionNoPivot: pivot 0.5 时 = localPosition
	private static void testGetPositionNoPivot()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setPosition(new Vector3(10.0f, 20.0f, 0.0f));
			Vector3 noPivot = child.getPositionNoPivot();
			assertEqual(10.0f, noPivot.x, 0.001f, "pivot 0.5 → x 无偏移");
			assertEqual(20.0f, noPivot.y, 0.001f, "pivot 0.5 → y 无偏移");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// setLeftInParent(0): x = 0 - leftInSelf(-25) = 25
	private static void testSetXInParentDirect()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setLeftInParent(0.0f);
			assertEqual(25.0f, child.getPosition().x, 0.001f, "setLeftInParent(0) → x=25");
			child.setRightInParent(0.0f);
			assertEqual(-25.0f, child.getPosition().x, 0.001f, "setRightInParent(0) → x=-25");
			child.setTopInParent(0.0f);
			assertEqual(-25.0f, child.getPosition().y, 0.001f, "setTopInParent(0) → y=-25");
			child.setBottomInParent(0.0f);
			assertEqual(25.0f, child.getPosition().y, 0.001f, "setBottomInParent(0) → y=25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 相对定位(异侧): child2 相对 child1 的四边 + interval
	private static void testRelativeOther()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		GameObject otherGo = new GameObject("AnchorOther");
		myUGUIObject other = createWindow(otherGo, "AnchorOther", new Vector2(30.0f, 30.0f));
		other.setParent(parent, false);
		try
		{
			child.setPosition(new Vector3(10.0f, 20.0f, 0.0f));
			// child1: left=-15, right=35, top=45, bottom=-5
			// other2(30x30, 边界±15): setRightToOtherLeft → 右边 = child1左-5 = -20 → x = -20-15 = -35
			other.setRightToOtherLeft(child, 5.0f);
			assertEqual(-35.0f, other.getPosition().x, 0.001f, "other 右 = child 左 -5 → x=-35");
			// setLeftToOtherRight → 左边 = child1右+5 = 40 → x = 40-(-15) = 55
			other.setLeftToOtherRight(child, 5.0f);
			assertEqual(55.0f, other.getPosition().x, 0.001f, "other 左 = child 右 +5 → x=55");
			// setBottomToOtherTop → 底 = child1顶+5 = 50 → y = 50-(-15) = 65
			other.setBottomToOtherTop(child, 5.0f);
			assertEqual(65.0f, other.getPosition().y, 0.001f, "other 底 = child 顶 +5 → y=65");
			// setTopToOtherBottom → 顶 = child1底-5 = -10 → y = -10-15 = -25
			other.setTopToOtherBottom(child, 5.0f);
			assertEqual(-25.0f, other.getPosition().y, 0.001f, "other 顶 = child 底 -5 → y=-25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(otherGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

		// 相对定位(同侧): 直接公式(非 setXInParent 组合, interval 符号各不同)
		//   setLeftToOtherLeft:  x = other.x - other.size.x*0.5 + this.size.x*0.5 + interval
		//   setRightToOtherRight: x = other.x + other.size.x*0.5 - this.size.x*0.5 - interval
		//   setTopToOtherTop:     y = other.y + other.size.y*0.5 - this.size.y*0.5 - interval
		private static void testRelativeSameSide()
		{
			createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
			GameObject otherGo = new GameObject("AnchorOther2");
			myUGUIObject other = createWindow(otherGo, "AnchorOther2", new Vector2(30.0f, 30.0f));
			other.setParent(parent, false);
			try
			{
				child.setPosition(new Vector3(10.0f, 20.0f, 0.0f));
				// child(50x50) @(10,20), other(30x30)
				// setLeftToOtherLeft: x = 10-25+15+5 = 5
				other.setLeftToOtherLeft(child, 5.0f);
				assertEqual(5.0f, other.getPosition().x, 0.001f, "other 左对齐 child 左 +5 → x=5");
				// setRightToOtherRight: x = 10+25-15-5 = 15
				other.setRightToOtherRight(child, 5.0f);
				assertEqual(15.0f, other.getPosition().x, 0.001f, "other 右对齐 child 右 -5 → x=15");
				// setTopToOtherTop: y = 20+25-15-5 = 25
				other.setTopToOtherTop(child, 5.0f);
				assertEqual(25.0f, other.getPosition().y, 0.001f, "other 顶对齐 child 顶 -5 → y=25");
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(otherGo);
				UnityEngine.Object.DestroyImmediate(parentGo);
			}
		}
}
