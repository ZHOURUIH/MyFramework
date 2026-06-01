#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// DistanceSortHelper 距离排序辅助类测试
public static class DistanceSortHelperTest
{
	public static void Run()
	{
		testConstructor();
		testEquals();
		testCompareAscend();
	}

	static void testConstructor()
	{
		var objA = new MockMouseEventCollect();
		var helper = new DistanceSortHelper(10.5f, objA);
		assertEqual(10.5f, helper.mDistance, "Distance should match constructor argument");
		assertNotNull(helper.mObject, "mObject should not be null");
	}

	static void testEquals()
	{
		var objA = new MockMouseEventCollect();
		var objB = new MockMouseEventCollect();
		var a = new DistanceSortHelper(5.0f, objA);
		var b = new DistanceSortHelper(5.0f, objA);
		var c = new DistanceSortHelper(5.0f, objB);
		var d = new DistanceSortHelper(10.0f, objA);

		assertTrue(a.Equals(b), "Same distance and same object should be equal");
		assertFalse(a.Equals(c), "Different object should not be equal even with same distance");
		assertFalse(a.Equals(d), "Different distance should not be equal");
	}

	static void testCompareAscend()
	{
		var obj = new MockMouseEventCollect();
		var a = new DistanceSortHelper(10.0f, obj);
		var b = new DistanceSortHelper(5.0f, obj);
		var c = new DistanceSortHelper(15.0f, obj);

		var list = new List<DistanceSortHelper> { a, b, c };
		list.Sort(DistanceSortHelper.mCompareAscend);

		assertTrue(list[0].mDistance <= list[1].mDistance, "First item should have smallest distance");
		assertTrue(list[1].mDistance <= list[2].mDistance, "Second item should have middle distance");
		assertEqual(5.0f, list[0].mDistance, "Smallest distance should be first");
		assertEqual(15.0f, list[2].mDistance, "Largest distance should be last");
	}

	// 最小的 IMouseEventCollect 模拟实现
	class MockMouseEventCollect : IMouseEventCollect
	{
		public string getName() { return "Mock"; }
		public string getDescription() { return "Mock for test"; }
		public bool isDestroy() { return false; }
		public bool isActive() { return true; }
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
#endif
