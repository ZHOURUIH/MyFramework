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
		testEqualsSelf();
		testCompareAscendEqualDistance();
		testCompareAscendNegativeDistance();
		testCompareAscendManyElements();
		testCompareAscendSameObjectDifferentDistance();
		testCompareAscendZeroDistance();
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

	// ═════════════════════════════════════════════════════════════════
	// 深度组合
	// ═════════════════════════════════════════════════════════════════

	// 自身相等
	static void testEqualsSelf()
	{
		var obj = new MockMouseEventCollect();
		var a = new DistanceSortHelper(3.0f, obj);
		assertTrue(a.Equals(a), "对象与自身相等");
	}

	// 同距离排序(比较器返回 0, 顺序稳定)
	static void testCompareAscendEqualDistance()
	{
		var obj = new MockMouseEventCollect();
		var list = new List<DistanceSortHelper>
		{
			new DistanceSortHelper(7.0f, obj),
			new DistanceSortHelper(7.0f, obj),
			new DistanceSortHelper(7.0f, obj)
		};
		list.Sort(DistanceSortHelper.mCompareAscend);
		assertEqual(7.0f, list[0].mDistance, "同距离排序后距离不变");
		assertEqual(3, list.Count, "同距离 3 个元素");
	}

	// 负距离排序
	static void testCompareAscendNegativeDistance()
	{
		var obj = new MockMouseEventCollect();
		var list = new List<DistanceSortHelper>
		{
			new DistanceSortHelper(-3.0f, obj),
			new DistanceSortHelper(-10.0f, obj),
			new DistanceSortHelper(0.0f, obj)
		};
		list.Sort(DistanceSortHelper.mCompareAscend);
		assertEqual(-10.0f, list[0].mDistance, "最小负距离在前");
		assertEqual(0.0f, list[2].mDistance, "0 在最后");
	}

	// 多元素随机排序 → 升序
	static void testCompareAscendManyElements()
	{
		var obj = new MockMouseEventCollect();
		var list = new List<DistanceSortHelper>();
		for (int i = 10; i >= 1; --i)
		{
			list.Add(new DistanceSortHelper(i, obj));
		}
		list.Sort(DistanceSortHelper.mCompareAscend);
		for (int i = 1; i < list.Count; ++i)
		{
			assertTrue(list[i - 1].mDistance <= list[i].mDistance, "升序, index " + i);
		}
		assertEqual(1.0f, list[0].mDistance, "最小 1 在前");
		assertEqual(10.0f, list[9].mDistance, "最大 10 在后");
	}

	// 同对象不同距离排序
	static void testCompareAscendSameObjectDifferentDistance()
	{
		var obj = new MockMouseEventCollect();
		var list = new List<DistanceSortHelper>
		{
			new DistanceSortHelper(9.0f, obj),
			new DistanceSortHelper(2.0f, obj),
			new DistanceSortHelper(5.0f, obj)
		};
		list.Sort(DistanceSortHelper.mCompareAscend);
		assertEqual(2.0f, list[0].mDistance, "2 在前");
		assertEqual(9.0f, list[2].mDistance, "9 在后");
	}

	// 零距离排序
	static void testCompareAscendZeroDistance()
	{
		var obj = new MockMouseEventCollect();
		var list = new List<DistanceSortHelper>
		{
			new DistanceSortHelper(0.0f, obj),
			new DistanceSortHelper(1.0f, obj)
		};
		list.Sort(DistanceSortHelper.mCompareAscend);
		assertEqual(0.0f, list[0].mDistance, "0 在前");
	}

	// 注: 框架无无参构造(只有 (float, IMouseEventCollect)), 不测默认构造

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