using UnityEngine;
using static TestAssert;

// UGUIEventThroughArea 深度测试
// 允许部分区域穿透 UGUI 鼠标事件的组件:
//   setPassOnlyArea(Rect): 存储穿透区域(protected mPassOnlyRect, 子类读回)
//   setPassOnlyArea(RectTransform): 将子节点的 rect 转换到父节点空间(世界坐标转换)
//   OnPointerDown/Up/Click: passEvent 依赖 EventSystem(设备依赖), 合法跳过
//
// 环境: 裸 GameObject + RectTransform
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class UGUIEventThroughAreaTest
{
	public static void Run()
	{
		testSetPassOnlyRect();
		testSetPassOnlyRectTransform();
		testSetPassOnlyRectTransformNoParent();
		testSetPassOnlyRectRoundTrip();
	}

	// setPassOnlyArea(Rect): 存储穿透区域, 可读回
	private static void testSetPassOnlyRect()
	{
		GameObject go = new GameObject("EventArea");
		TestEventThroughArea area = go.AddComponent<TestEventThroughArea>();
		try
		{
			area.setPassOnlyArea(new Rect(10.0f, 20.0f, 30.0f, 40.0f));
			Rect rect = area.getPassOnlyRectForTest();
			assertEqual(10.0f, rect.x, 0.001f, "穿透区域 x=10");
			assertEqual(20.0f, rect.y, 0.001f, "穿透区域 y=20");
			assertEqual(30.0f, rect.width, 0.001f, "穿透区域宽 30");
			assertEqual(40.0f, rect.height, 0.001f, "穿透区域高 40");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setPassOnlyArea(RectTransform): 子 rect 转换到父节点空间
	// 父 localPosition(0,0) 时, 子 rect(100x100, 中心 0) 的 min/max 在父空间 = (-50,-50)/(50,50)
	private static void testSetPassOnlyRectTransform()
	{
		GameObject parentGO = new GameObject("EventParent");
		RectTransform parentRT = parentGO.AddComponent<RectTransform>();
		parentRT.sizeDelta = new Vector2(200.0f, 200.0f);
		GameObject childGO = new GameObject("EventChild");
		RectTransform childRT = childGO.AddComponent<RectTransform>();
		childRT.SetParent(parentGO.transform, false);
		childRT.sizeDelta = new Vector2(100.0f, 100.0f);
		TestEventThroughArea area = childGO.AddComponent<TestEventThroughArea>();
		try
		{
			area.setPassOnlyArea(childRT);
			Rect rect = area.getPassOnlyRectForTest();
			assertEqual(-50.0f, rect.min.x, 0.001f, "转换后 min.x=-50(父空间)");
			assertEqual(-50.0f, rect.min.y, 0.001f, "转换后 min.y=-50");
			assertEqual(50.0f, rect.max.x, 0.001f, "转换后 max.x=50");
			assertEqual(50.0f, rect.max.y, 0.001f, "转换后 max.y=50");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGO);
		}
	}

	// setPassOnlyArea(RectTransform) 无父节点: 使用 rect 原值
	private static void testSetPassOnlyRectTransformNoParent()
	{
		GameObject go = new GameObject("EventNoParent");
		RectTransform rt = go.AddComponent<RectTransform>();
		rt.sizeDelta = new Vector2(100.0f, 50.0f);
		TestEventThroughArea area = go.AddComponent<TestEventThroughArea>();
		try
		{
			area.setPassOnlyArea(rt);
			Rect rect = area.getPassOnlyRectForTest();
			assertEqual(-50.0f, rect.min.x, 0.001f, "无父节点 min.x=-50(rect 原值)");
			assertEqual(-25.0f, rect.min.y, 0.001f, "无父节点 min.y=-25");
			assertEqual(50.0f, rect.max.x, 0.001f, "无父节点 max.x=50");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setPassOnlyArea(Rect) 重复设置: 覆盖旧值
	private static void testSetPassOnlyRectRoundTrip()
	{
		GameObject go = new GameObject("EventRoundTrip");
		TestEventThroughArea area = go.AddComponent<TestEventThroughArea>();
		try
		{
			area.setPassOnlyArea(new Rect(0.0f, 0.0f, 10.0f, 10.0f));
			area.setPassOnlyArea(new Rect(100.0f, 200.0f, 300.0f, 400.0f));
			Rect rect = area.getPassOnlyRectForTest();
			assertEqual(100.0f, rect.x, 0.001f, "覆盖后 x=100");
			assertEqual(200.0f, rect.y, 0.001f, "覆盖后 y=200");
			assertEqual(300.0f, rect.width, 0.001f, "覆盖后宽 300");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 UGUIEventThroughArea 的 protected 穿透区域
// ═════════════════════════════════════════════════════════════════
public class TestEventThroughArea : UGUIEventThroughArea
{
	public Rect getPassOnlyRectForTest() { return mPassOnlyRect; }
}
