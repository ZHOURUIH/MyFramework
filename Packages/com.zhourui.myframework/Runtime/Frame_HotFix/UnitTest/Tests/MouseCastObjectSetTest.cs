using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// MouseCastObjectSet 正常使用入口守卫式单测(GlobalTouchSystem 射线检测的物体集合)
//
// 设计要点:
//   - MouseCastObjectSet 继承 ClassObject, 是"存储触点检测时物体"的纯数据结构,
//     所有方法只操作公开字段 mObjectOrderList(List<IMouseEventCollect>) 与 mCamera, 无全局依赖。
//   - 可局部 new 测试(字段初始化器已建好空 List), 无需 resetProperty, 无需全局单例/真实场景。
//   - 守卫式: 空集合操作安全; 用 Mock 对象走 add/remove/isEmpty 正常入口; setCamera 纯赋值。
public static class MouseCastObjectSetTest
{
	public static void Run()
	{
		testDefaultStateEmpty();
		testAddObjectIncreasesCount();
		testAddDuplicateObjectCountOnce();
		testRemoveObjectExisting();
		testRemoveObjectNotExisting();
		testAddThenRemoveReturnsEmpty();
		testSetCameraThenGetCamera();
		testIsEmptyWithObjects();
		testResetPropertyClears();
	}

	// ─── 默认状态: 空列表 ────────────────────────────────────────
	private static void testDefaultStateEmpty()
	{
		MouseCastObjectSet set = new MouseCastObjectSet();
		assertTrue(set.isEmpty(), "新建 MouseCastObjectSet 应为空");
		assertEqual(0, set.mObjectOrderList.Count, "默认 mObjectOrderList 应为空");
	}

	// ─── addObject: 正常入口, 增加列表项 ─────────────────────────
	private static void testAddObjectIncreasesCount()
	{
		MouseCastObjectSet set = new MouseCastObjectSet();
		MockMouseEventCollect obj = new MockMouseEventCollect();
		set.addObject(obj);
		assertEqual(1, set.mObjectOrderList.Count, "addObject 后列表应有 1 项");
		assertFalse(set.isEmpty(), "addObject 后 isEmpty 应为 false");
		// 直接验证列表内容为同一实例
		assertTrue(set.mObjectOrderList.Contains(obj), "列表应包含 addObject 的对象");
	}

	// ─── addObject 重复: List.Add 允许重复, 计数累加 ─────────────
	private static void testAddDuplicateObjectCountOnce()
	{
		MouseCastObjectSet set = new MouseCastObjectSet();
		MockMouseEventCollect obj = new MockMouseEventCollect();
		set.addObject(obj);
		set.addObject(obj);
		assertEqual(2, set.mObjectOrderList.Count, "addObject 重复添加同一对象(原生 List.Add)应累加到 2");
	}

	// ─── removeObject 已存在: 移除成功返回 true ──────────────────
	private static void testRemoveObjectExisting()
	{
		MouseCastObjectSet set = new MouseCastObjectSet();
		MockMouseEventCollect obj = new MockMouseEventCollect();
		set.addObject(obj);
		bool removed = set.removeObject(obj);
		assertTrue(removed, "removeObject 已存在对象应返回 true");
		assertTrue(set.isEmpty(), "移除后集合应为空");
	}

	// ─── removeObject 不存在: 返回 false ─────────────────────────
	private static void testRemoveObjectNotExisting()
	{
		MouseCastObjectSet set = new MouseCastObjectSet();
		MockMouseEventCollect obj = new MockMouseEventCollect();
		MockMouseEventCollect other = new MockMouseEventCollect();
		set.addObject(obj);
		bool removed = set.removeObject(other);
		assertFalse(removed, "removeObject 不存在对象应返回 false");
		assertEqual(1, set.mObjectOrderList.Count, "移除不存在的对象后列表仍应有 1 项");
	}

	// ─── add 后 remove: 回到空态 ─────────────────────────────────
	private static void testAddThenRemoveReturnsEmpty()
	{
		MouseCastObjectSet set = new MouseCastObjectSet();
		MockMouseEventCollect obj1 = new MockMouseEventCollect();
		MockMouseEventCollect obj2 = new MockMouseEventCollect();
		set.addObject(obj1);
		set.addObject(obj2);
		set.removeObject(obj1);
		assertFalse(set.isEmpty(), "移除一个后不应为空");
		set.removeObject(obj2);
		assertTrue(set.isEmpty(), "移除全部后应为空");
	}

	// ─── setCamera: 纯赋值入口, 不读取内部状态 ───────────────────
	private static void testSetCameraThenGetCamera()
	{
		MouseCastObjectSet set = new MouseCastObjectSet();
		// 未设置时为 null
		assertNull(set.mCamera, "默认 mCamera 应为 null");
		// 直接 new GameCamera(不调 setObject, 避免 isEditor 下 CameraDebug 组件初始化等全局依赖);
		// setCamera 仅做引用赋值, 不读取 camera 内部 Camera 组件, 安全。
		GameCamera camera = new GameCamera();
		set.setCamera(camera);
		assertTrue(ReferenceEquals(camera, set.mCamera), "setCamera 后 mCamera 应为同一实例");
	}

	// ─── isEmpty: 有对象时返回 false ─────────────────────────────
	private static void testIsEmptyWithObjects()
	{
		MouseCastObjectSet set = new MouseCastObjectSet();
		MockMouseEventCollect obj = new MockMouseEventCollect();
		set.addObject(obj);
		assertFalse(set.isEmpty(), "有对象时 isEmpty 应为 false");
	}

	// ─── resetProperty: 清空列表与相机 ───────────────────────────
	private static void testResetPropertyClears()
	{
		MouseCastObjectSet set = new MouseCastObjectSet();
		MockMouseEventCollect obj = new MockMouseEventCollect();
		set.addObject(obj);
		set.resetProperty();
		assertTrue(set.isEmpty(), "resetProperty 后列表应为空");
		assertNull(set.mCamera, "resetProperty 后 mCamera 应为 null");
	}

	// ─── 最小的 IMouseEventCollect 模拟实现 ──────────────────────
	private class MockMouseEventCollect : IMouseEventCollect
	{
		public string getName() { return "Mock"; }
		public string getDescription() { return "Mock for MouseCastObjectSet test"; }
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
