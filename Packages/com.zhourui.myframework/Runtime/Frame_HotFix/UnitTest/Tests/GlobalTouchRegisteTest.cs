using UnityEngine;
using static FrameBaseHotFix;
using static TestAssert;

// GlobalTouchSystem 交互注册状态机深度测试
// (MicroLegend 最高频: registeCollider 605 次调用, 本测试验证注册的完整生命周期)
//   registeCollider() → isColliderRegisted 状态 / passRay 切换 / needUpdate 副作用
//   unregisteCollider 手动注销 / 未注册重复注销安全
//   destroy 链自动注销(真实交互链路前置)
public static class GlobalTouchRegisteTest
{
	private class TestUGUIObject : myUGUIObject
	{
		public int getMouseCastWindowCountInTree() { return mMouseCastWindowCountInTree; }
	}
	public static void Run()
	{
		testRegisteState();
		testPassRayToggle();
		testPassRayDefault();
		testDestroyAutoUnregiste();
		testUnregisteSafe();
		testNeedUpdateEnabled();
		testMouseCastTreeCount();
		testMouseCastTreeCountAfterReparent();
		testRaycastVersionOnUIChange();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 myUGUIObject
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createUI()
	{
		GameObject go = new GameObject("TestTouchUI");
		go.AddComponent<RectTransform>();
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}
	private static TestUGUIObject createTestUI(TestUGUIObject parent = null)
	{
		GameObject go = new GameObject("TestTouchTreeUI");
		go.AddComponent<RectTransform>();
		TestUGUIObject ui = new();
		ui.setObject(go);
		ui.setParent(parent, false);
		ui.init();
		return ui;
	}

	// 注册状态机: 未注册 → 注册 → 注销
	private static void testRegisteState()
	{
		myUGUIObject ui = createUI();
		try
		{
			assertFalse(mGlobalTouchSystem.isColliderRegisted(ui), "注册前不在碰撞列表");
			ui.registeCollider();
			assertTrue(mGlobalTouchSystem.isColliderRegisted(ui), "registeCollider() 后已注册");
			ui.unregisteCollider();
			assertFalse(mGlobalTouchSystem.isColliderRegisted(ui), "unregisteCollider() 后已注销");
		}
		finally
		{
			// destroyObject → destroyWindow → 销毁 go + 自动 unregiste
			LayoutScript.destroyObject(ref ui, true);
		}
	}

	// passRay 切换: registeCollider() 无参设 false, bool 版设 true
	private static void testPassRayToggle()
	{
		myUGUIObject ui = createUI();
		try
		{
			ui.registeCollider();
			assertFalse(ui.isPassRay(), "registeCollider() 无参版 passRay=false");
			ui.registeCollider(true);
			assertTrue(ui.isPassRay(), "registeCollider(true) 后 passRay=true");
			ui.registeCollider(false);
			assertFalse(ui.isPassRay(), "registeCollider(false) 后 passRay=false");
		}
		finally
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}

	// 默认 passRay: 无交互组件时穿透
	private static void testPassRayDefault()
	{
		myUGUIObject ui = createUI();
		try
		{
			assertTrue(ui.isPassRay(), "无交互组件时 isPassRay 默认 true(穿透)");
		}
		finally
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}

	// destroy 链自动注销(真实交互链路: 面板关闭后碰撞注册自动清理)
	private static void testDestroyAutoUnregiste()
	{
		myUGUIObject ui = createUI();
		ui.registeCollider();
		assertTrue(mGlobalTouchSystem.isColliderRegisted(ui), "注册成功");
		LayoutScript.destroyObject(ref ui, true);
		assertFalse(mGlobalTouchSystem.isColliderRegisted(ui), "destroy 后自动注销(无残留)");
	}

	// 未注册对象注销: 安全无副作用
	private static void testUnregisteSafe()
	{
		myUGUIObject ui = createUI();
		try
		{
			ui.unregisteCollider();
			assertFalse(mGlobalTouchSystem.isColliderRegisted(ui), "未注册注销后仍不在列表(无副作用)");
			// 多次注销安全
			ui.registeCollider();
			ui.unregisteCollider();
			ui.unregisteCollider();
			assertFalse(mGlobalTouchSystem.isColliderRegisted(ui), "重复注销安全");
		}
		finally
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}

	// registeCollider 副作用: setNeedUpdate(true)
	private static void testNeedUpdateEnabled()
	{
		myUGUIObject ui = createUI();
		try
		{
			assertFalse(ui.isNeedUpdate(), "初始 mNeedUpdate=false");
			ui.registeCollider();
			assertTrue(ui.isNeedUpdate(), "registeCollider 后启用窗口更新(保证碰撞体尺寸同步)");
		}
		finally
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}

	// 注册一个子节点后,自己以及所有父节点都应该记录这棵子树中存在1个MouseCast窗口
	private static void testMouseCastTreeCount()
	{
		TestUGUIObject parent = createTestUI();
		TestUGUIObject child = createTestUI(parent);
		try
		{
			assertEqual(0, parent.getMouseCastWindowCountInTree(), "注册前父节点MouseCast子树计数应为0");
			assertEqual(0, child.getMouseCastWindowCountInTree(), "注册前子节点MouseCast子树计数应为0");
			child.registeCollider();
			assertEqual(1, child.getMouseCastWindowCountInTree(), "注册后子节点MouseCast子树计数应为1");
			assertEqual(1, parent.getMouseCastWindowCountInTree(), "注册后父节点应统计到已注册子节点");
			child.unregisteCollider();
			assertEqual(0, child.getMouseCastWindowCountInTree(), "注销后子节点MouseCast子树计数应恢复为0");
			assertEqual(0, parent.getMouseCastWindowCountInTree(), "注销后父节点MouseCast子树计数应恢复为0");
		}
		finally
		{
			LayoutScript.destroyObject(child, true);
			LayoutScript.destroyObject(parent, true);
		}
	}
	// 已注册子树换父节点时,MouseCast子树计数必须从旧父链迁移到新父链
	private static void testMouseCastTreeCountAfterReparent()
	{
		TestUGUIObject parentA = createTestUI();
		TestUGUIObject parentB = createTestUI();
		TestUGUIObject child = createTestUI(parentA);
		try
		{
			child.registeCollider();
			assertEqual(1, parentA.getMouseCastWindowCountInTree(), "换父节点前旧父节点计数应为1");
			assertEqual(0, parentB.getMouseCastWindowCountInTree(), "换父节点前新父节点计数应为0");
			child.setParent(parentB, false);
			assertEqual(0, parentA.getMouseCastWindowCountInTree(), "换父节点后旧父节点计数应恢复为0");
			assertEqual(1, parentB.getMouseCastWindowCountInTree(), "换父节点后新父节点计数应变为1");
			assertEqual(1, child.getMouseCastWindowCountInTree(), "换父节点不应改变子树自身计数");
		}
		finally
		{
			LayoutScript.destroyObject(child, true);
			LayoutScript.destroyObject(parentA, true);
			LayoutScript.destroyObject(parentB, true);
		}
	}
	// 已注册UI真正影响Raycast结果的状态发生变化时,必须推进版本,否则静止鼠标会错误复用旧Hover
	private static void testRaycastVersionOnUIChange()
	{
		myUGUIObject ui = createUI();
		try
		{
			int version0 = mGlobalTouchSystem.getRaycastVersion();
			ui.registeCollider();
			int version1 = mGlobalTouchSystem.getRaycastVersion();
			assertTrue(version1 != version0, "注册Collider后Raycast版本必须变化");
			ui.setPositionX(10.0f);
			int version2 = mGlobalTouchSystem.getRaycastVersion();
			assertTrue(version2 != version1, "已注册UI移动后Raycast版本必须变化");
			ui.setPassRay(!ui.isPassRay());
			int version3 = mGlobalTouchSystem.getRaycastVersion();
			assertTrue(version3 != version2, "已注册UI修改PassRay后Raycast版本必须变化");
			ui.setHandleInput(false);
			int version4 = mGlobalTouchSystem.getRaycastVersion();
			assertTrue(version4 != version3, "已注册UI修改HandleInput后Raycast版本必须变化");
		}
		finally
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}

}
