using UnityEngine;
using static TestAssert;

// CustomLine 深度测试(MaskableGraphic 自定义线条)
//   setWidth/getWidth: 线条宽度
//   setPointList: 设置点列表(去除连续重复点, 刷新顶点数据)
//   AddComponent 无 Canvas 也安全
// 环境: 裸 GameObject + CustomLine(AddComponent)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class CustomLineTest
{
	public static void Run()
	{
		testAddComponentSafe();
		testWidthRoundTrip();
		testSetPointList();
		testSetPointListNullSafe();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static CustomLine createLine(out GameObject go)
	{
		go = new GameObject("CustomLineGO");
		return go.AddComponent<CustomLine>();
	}

	// AddComponent 无 Canvas 安全
	private static void testAddComponentSafe()
	{
		CustomLine line = createLine(out GameObject go);
		try
		{
			assertTrue(line != null, "AddComponent<CustomLine> 成功");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setWidth/getWidth
	private static void testWidthRoundTrip()
	{
		CustomLine line = createLine(out GameObject go);
		try
		{
			line.setWidth(3.0f);
			assertEqual(3.0f, line.getWidth(), 0.001f, "setWidth(3) 读回");
			line.setWidth(8.0f);
			assertEqual(8.0f, line.getWidth(), 0.001f, "setWidth(8) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setPointList: 设置点列表不崩溃
	private static void testSetPointList()
	{
		CustomLine line = createLine(out GameObject go);
		try
		{
			Vector3[] points = new Vector3[] { new Vector3(0.0f, 0.0f, 0.0f), new Vector3(10.0f, 10.0f, 0.0f), new Vector3(20.0f, 0.0f, 0.0f) };
			line.setPointList(points);
			line.setPointList((System.Collections.Generic.List<Vector3>)null);   // null 安全
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setPointList null 安全(List 版本)
	private static void testSetPointListNullSafe()
	{
		CustomLine line = createLine(out GameObject go);
		try
		{
			System.Collections.Generic.List<Vector3> list = null;
			line.setPointList(list);   // list.safe() 空安全
			line.setPointList((Vector3[])null);   // 数组版本 safe()
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
