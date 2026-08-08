using UnityEngine;
using static TestAssert;

// TouchInfo 正常使用入口守卫式单测(GlobalTouchSystem 触点悬停/按下信息)
//
// 设计要点:
//   - TouchInfo 继承 ClassObject, 存放一个触点的悬停和按下物体列表, 可局部 new(字段初始化器建好空容器)。
//   - 守卫式要点:
//       * update()/touchPress() 依赖 mGlobalTouchSystem 全局单例(getAllHoverObject), 非纯逻辑, 跳过。
//       * init(TouchPoint)/getTouch 只存取引用, 用 new TouchPoint() 即可(不触发其内部逻辑)。
//       * clearPressList/getPressList/removeObject/resetProperty 只操作本实例空 SafeList/HashSet, 安全。
//   - 全部断言确定性强、不触发 error、不依赖全局单例/真实场景。
public static class TouchInfoTest
{
	public static void Run()
	{
		testInitThenGetTouch();
		testGetTouchDefaultNull();
		testClearPressListEmptySafe();
		testGetPressListNotNull();
		testRemoveObjectEmptySafe();
		testRemoveObjectAfterInit();
		testResetPropertyClears();
	}

	// ─── init(TouchPoint) + getTouch: 存取引用 ────────────────────
	private static void testInitThenGetTouch()
	{
		TouchInfo info = new TouchInfo();
		TouchPoint point = new TouchPoint();
		info.init(point);
		assertTrue(ReferenceEquals(point, info.getTouch()), "init 后 getTouch 应返回同一 TouchPoint 实例");
	}

	// ─── 未 init 时 getTouch 返回 null ────────────────────────────
	private static void testGetTouchDefaultNull()
	{
		TouchInfo info = new TouchInfo();
		assertNull(info.getTouch(), "未 init 时 getTouch 应为 null");
	}

	// ─── clearPressList: 空列表安全 ───────────────────────────────
	private static void testClearPressListEmptySafe()
	{
		TouchInfo info = new TouchInfo();
		info.clearPressList();
		assertTrue(info.getPressList() != null, "clearPressList 后 getPressList 应非 null");
	}

	// ─── getPressList: 返回非 null 空 SafeList ────────────────────
	private static void testGetPressListNotNull()
	{
		TouchInfo info = new TouchInfo();
		var list = info.getPressList();
		assertNotNull(list, "getPressList 应返回非 null SafeList");
	}

	// ─── removeObject: 空列表移除安全, 无异常 ─────────────────────
	private static void testRemoveObjectEmptySafe()
	{
		TouchInfo info = new TouchInfo();
		MockMouseEventCollect obj = new MockMouseEventCollect();
		// mPressList.remove(不存在) 与 mHoverList.Remove(不存在) 均安全
		info.removeObject(obj);
	}

	// ─── removeObject: init 后移除安全 ────────────────────────────
	private static void testRemoveObjectAfterInit()
	{
		TouchInfo info = new TouchInfo();
		TouchPoint point = new TouchPoint();
		info.init(point);
		MockMouseEventCollect obj = new MockMouseEventCollect();
		info.removeObject(obj);
		// 未被添加的对象移除后, 悬停/按下列表仍为空
		assertTrue(info.getPressList() != null && info.getPressList().count() == 0, "移除未添加对象后按下列表应为空");
	}

	// ─── resetProperty: 清空所有状态 ──────────────────────────────
	private static void testResetPropertyClears()
	{
		TouchInfo info = new TouchInfo();
		TouchPoint point = new TouchPoint();
		info.init(point);
		info.resetProperty();
		assertNull(info.getTouch(), "resetProperty 后 getTouch 应为 null");
		assertTrue(info.getPressList().count() == 0, "resetProperty 后按下列表应为空");
	}

	// ─── 最小的 IMouseEventCollect 模拟实现 ──────────────────────
	private class MockMouseEventCollect : IMouseEventCollect
	{
		public string getName() { return "Mock"; }
		public string getDescription() { return "Mock for TouchInfo test"; }
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
