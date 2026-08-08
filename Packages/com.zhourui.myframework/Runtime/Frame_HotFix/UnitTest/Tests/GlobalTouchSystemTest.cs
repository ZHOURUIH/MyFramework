using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// GlobalTouchSystem 纯逻辑单测(统一多摄像机触摸事件系统)
//
// 设计要点:
//   - GlobalTouchSystem 继承 FrameSystem(mCreateObject 默认 false), 可 new 局部实例安全测试。
//   - 只测"空状态 + 非类型对象"下的安全分支, 避免触发:
//       * registeCollider(需真实碰撞体/makeUGUIObject/MovableObject, 否则 logError)
//       * bindPassOnlyArea / bindPassOnlyParent(未注册对象 → logError)
//       * update() 默认开启时依赖 mInputSystem 单例; 若 mUseGlobalTouch=false 则提前返回不碰单例
//   - 每用例 finally 中 destroy() 局部实例(mObject 为 null 时 destroy 空安全)。
public static class GlobalTouchSystemTest
{
	public static void Run()
	{
		testEmptyStateHoverReturnsNull();
		testEmptyStateHoverSetEmpty();
		testEmptyStateHoverListEmpty();
		testSetUseGlobalTouchDisablesUpdate();
		testIsColliderRegistedUnregistered();
		testUnregisteColliderNotRegisteredNoOp();
		testSetActiveOnlyObjectNonType();
		testAddActiveOnlyObjectNonType();
		testHasActiveOnlyObjectDefaultFalse();
		testDestroySafe();
	}

	// ─── 空状态下 hover 查询返回 null/空(无摄像机/无碰撞体依赖) ───────
	private static void testEmptyStateHoverReturnsNull()
	{
		GlobalTouchSystem sys = new GlobalTouchSystem();
		try
		{
			IMouseEventCollect result = sys.getHoverObject(Vector3.zero);
			assertNull(result, "空状态下 getHoverObject 应返回 null");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testEmptyStateHoverSetEmpty()
	{
		GlobalTouchSystem sys = new GlobalTouchSystem();
		try
		{
			HashSet<IMouseEventCollect> set = new();
			sys.getAllHoverObject(set, Vector3.zero);
			assertEqual(0, set.Count, "空状态下 getAllHoverObject(HashSet) 应返回空集合");
		}
		finally
		{
			sys.destroy();
		}
	}

	private static void testEmptyStateHoverListEmpty()
	{
		GlobalTouchSystem sys = new GlobalTouchSystem();
		try
		{
			List<IMouseEventCollect> list = new();
			sys.getAllHoverObject(list, new Vector3(10, 20, 0));
			assertEqual(0, list.Count, "空状态下 getAllHoverObject(List) 应返回空列表");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── setUseGlobalTouch(false) 后 update() 提前返回, 不碰 mInputSystem 单例 ──
	private static void testSetUseGlobalTouchDisablesUpdate()
	{
		GlobalTouchSystem sys = new GlobalTouchSystem();
		try
		{
			sys.setUseGlobalTouch(false);
			// mUseGlobalTouch=false 时 update 第一行判断后即 return, 不依赖输入系统单例
			sys.update(0.016f);
			// 再次切换为 true(默认真值), 验证字段可来回切换
			sys.setUseGlobalTouch(true);
			sys.setUseGlobalTouch(false);
			sys.update(0.1f);
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── isColliderRegisted: 未注册对象 → false ─────────────────────
	private static void testIsColliderRegistedUnregistered()
	{
		GlobalTouchSystem sys = new GlobalTouchSystem();
		try
		{
			MockMouseEventCollect obj = new();
			assertFalse(sys.isColliderRegisted(obj), "未注册对象 isColliderRegisted 应为 false");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── unregisteCollider: 未注册对象 → 提前返回(无异常/无 logError) ──
	private static void testUnregisteColliderNotRegisteredNoOp()
	{
		GlobalTouchSystem sys = new GlobalTouchSystem();
		try
		{
			MockMouseEventCollect obj = new();
			// mAllObjectSet.Remove 返回 false → 直接 return, 不进入移除逻辑
			sys.unregisteCollider(obj);
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── setActiveOnlyObject: 非 myUGUIObject/MovableObject 类型 → 仅清空列表 ──
	private static void testSetActiveOnlyObjectNonType()
	{
		GlobalTouchSystem sys = new GlobalTouchSystem();
		try
		{
			MockMouseEventCollect obj = new();
			sys.setActiveOnlyObject(obj);
			// 对象既不是 myUGUIObject 也不是 MovableObject → 两个激活列表都被清空, 不添加任何项
			assertFalse(sys.hasActiveOnlyObject(), "非类型对象 setActiveOnlyObject 后不应有激活对象");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── addActiveOnlyObject: 非类型对象 → 不添加, 无副作用 ─────────
	private static void testAddActiveOnlyObjectNonType()
	{
		GlobalTouchSystem sys = new GlobalTouchSystem();
		try
		{
			MockMouseEventCollect obj = new();
			sys.addActiveOnlyObject(obj);
			assertFalse(sys.hasActiveOnlyObject(), "非类型对象 addActiveOnlyObject 后不应有激活对象");
			// 再次添加仍无副作用
			sys.addActiveOnlyObject(obj);
			assertFalse(sys.hasActiveOnlyObject(), "重复添加非类型对象后仍无激活对象");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── hasActiveOnlyObject: 默认无激活对象 → false ────────────────
	private static void testHasActiveOnlyObjectDefaultFalse()
	{
		GlobalTouchSystem sys = new GlobalTouchSystem();
		try
		{
			assertFalse(sys.hasActiveOnlyObject(), "默认无激活对象时 hasActiveOnlyObject 应为 false");
		}
		finally
		{
			sys.destroy();
		}
	}

	// ─── destroy: 空状态清理安全 ────────────────────────────────────
	private static void testDestroySafe()
	{
		GlobalTouchSystem sys = new GlobalTouchSystem();
		// 仅验证 destroy 在空状态不抛异常(mObject=null, destroyUnityObject 空安全)
		sys.destroy();
	}

	// ─── 最小的 IMouseEventCollect 模拟实现(非 myUGUIObject/MovableObject) ──
	private class MockMouseEventCollect : IMouseEventCollect
	{
		public string getName() { return "Mock"; }
		public string getDescription() { return "Mock for GlobalTouchSystem test"; }
		public bool isDestroy() { return false; }
		public bool isActiveInHierarchy() { return true; }
		public bool isHandleInput() { return false; }
		public void onTouchLeave(Vector3 touchPos, int touchID) { }
		public void onTouchEnter(Vector3 touchPos, int touchID) { }
		public void onTouchMove(Vector3 touchPos, Vector3 moveDelta, float moveTime, int touchID) { }
		public void onTouchStay(Vector3 touchPos, int touchID) { }
		public Collider getCollider(bool addIfNotExist = false) { return null; }
		public UIDepth getDepth() { return null; }
		public bool isReceiveScreenTouch() { return false; }
		public void onScreenTouchDown(Vector3 touchPos, int touchID) { }
		public void onScreenTouchUp(Vector3 touchPos, int touchID) { }
		public void onTouchDown(Vector3 touchPos, int touchID) { }
		public void onTouchUp(Vector3 touchPos, int touchID) { }
		public bool isPassRay() { return false; }
		public bool isPassDragEvent() { return false; }
		public void onReceiveDrag(IMouseEventCollect dragObj, Vector3 touchPos, ref bool continueEvent) { }
		public bool isDraggable() { return false; }
		public bool isChildOf(IMouseEventCollect parent) { return false; }
	}
}
