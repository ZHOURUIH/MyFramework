using UnityEngine;
using static TestAssert;

// myUGUIObject 父子层级深度测试(MyUGUIObjectGeometryTest 只测无父情况, 本文件补有父链路):
//   setParent(建立层级) / 重复 setParent 去重 / setParent(null) 解绑
//   removeChild(仅操作父列表, 不改 child.mParent)
//   有父对齐: setLeftToParentLeft / setRightToParentRight / setLeftCenterToParentLeftCenter / setInParentCenterX/Y
//
// 手算前提: parent size(200,100) child size(60,30), pivot=0.5
//   parent.getLeftInSelf = -100, getRightInSelf = +100
//   child.getLeftInSelf = -30, getRightInSelf = +30
//   setLeftToParentLeft → x = -100-(-30) = -70; setRightToParentRight → x = 100-30 = 70
//
// 清理: 先销毁 child 的 GameObject 再销毁 parent 的(先子后父)
public static class MyUGUIObjectParentTest
{
	public static void Run()
	{
		testSetParentEstablishesHierarchy();
		testSetParentSameParentNoDuplicates();
		testSetParentNullDetach();
		testRemoveChild();
		testSetLeftToParentLeft();
		testSetRightToParentRight();
		testSetLeftCenterToParentLeftCenter();
		testSetInParentCenter();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 UI 对象(setObject+init, 无父时 setSize → sizeDelta=size 确定)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createUI(string name, Vector2 size, out GameObject go)
	{
		go = new GameObject(name);
		go.AddComponent<RectTransform>();
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		ui.setSize(size);
		return ui;
	}

	// ═════════════════════════════════════════════════════════════════
	// setParent: 建立父子层级(Transform 父子 + 父列表)
	// ═════════════════════════════════════════════════════════════════
	private static void testSetParentEstablishesHierarchy()
	{
		myUGUIObject parent = createUI("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			assertTrue(ReferenceEquals(parent, child.getParent()), "child.getParent()==parent");
			assertTrue(ReferenceEquals(goP.transform, goC.transform.parent), "Transform 父子层级建立");
			assertEqual(1, parent.getChildList().Count, "parent 子列表含 1 个");
			assertTrue(parent.getChildList().Contains(child), "子列表包含 child");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// 重复 setParent 相同 parent: 直接 return, 不重复添加
	private static void testSetParentSameParentNoDuplicates()
	{
		myUGUIObject parent = createUI("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setParent(parent, false);
			assertEqual(1, parent.getChildList().Count, "重复 setParent 不重复添加");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// setParent(null): 解绑(从父列表移除 + mParent 置空)
	private static void testSetParentNullDetach()
	{
		myUGUIObject parent = createUI("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setParent(null, false);
			assertNull(child.getParent(), "setParent(null) 后 getParent 为 null");
			assertEqual(0, parent.getChildList().Count, "从父列表移除");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// removeChild: 只操作父列表, 不改 child.mParent(真实语义)
	private static void testRemoveChild()
	{
		myUGUIObject parent = createUI("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			parent.removeChild(child);
			assertEqual(0, parent.getChildList().Count, "removeChild 后父列表空");
			assertTrue(ReferenceEquals(parent, child.getParent()), "removeChild 不改 child.mParent(真实语义)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 有父对齐: 左边界对齐父左边界
	// parent left = -100, child left = -30 → x = -70
	// ═════════════════════════════════════════════════════════════════
	private static void testSetLeftToParentLeft()
	{
		myUGUIObject parent = createUI("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setLeftToParentLeft();
			Vector3 pos = child.getPosition();
			assertEqual(-70.0f, pos.x, 0.001f, "左边界对齐 parent 左(-100): x=-70");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// 右边界对齐父右边界
	// parent right = +100, child right = +30 → x = 70
	private static void testSetRightToParentRight()
	{
		myUGUIObject parent = createUI("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setRightToParentRight();
			Vector3 pos = child.getPosition();
			assertEqual(70.0f, pos.x, 0.001f, "右边界对齐 parent 右(+100): x=70");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// 左边界中心对齐: X 对齐左边界 + Y 居中
	private static void testSetLeftCenterToParentLeftCenter()
	{
		myUGUIObject parent = createUI("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setPosition(new Vector3(10.0f, 20.0f, 0.0f));
			child.setLeftCenterToParentLeftCenter();
			Vector3 pos = child.getPosition();
			assertEqual(-70.0f, pos.x, 0.001f, "左中心对齐: x=-70");
			assertEqual(0.0f, pos.y, 0.001f, "左中心对齐: y=0(居中)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// 居中: setInParentCenterX/Y → 位置归零
	private static void testSetInParentCenter()
	{
		myUGUIObject parent = createUI("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setPosition(new Vector3(50.0f, 60.0f, 0.0f));
			child.setInParentCenterX();
			assertEqual(0.0f, child.getPosition().x, 0.001f, "setInParentCenterX 后 x=0");
			assertEqual(60.0f, child.getPosition().y, 0.001f, "setInParentCenterX 不影响 y");
			child.setInParentCenterY();
			assertEqual(0.0f, child.getPosition().y, 0.001f, "setInParentCenterY 后 y=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}
}
