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
	public static void Run()
	{
		testRegisteState();
		testPassRayToggle();
		testPassRayDefault();
		testDestroyAutoUnregiste();
		testUnregisteSafe();
		testNeedUpdateEnabled();
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
}
