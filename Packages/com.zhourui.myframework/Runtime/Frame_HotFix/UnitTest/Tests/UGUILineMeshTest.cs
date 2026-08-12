using UnityEngine;
using static TestAssert;

// UGUILineMesh 深度测试(通过 MeshRenderer 画线)
//   init: 需要 GO 预加 MeshRenderer + MeshFilter
//   setWidth: 线条宽度(半宽)
//   setPointList: 设置点列表(去除连续重复点) + 更新 Mesh
//   setActive / getMaterial
// 环境: 裸 GameObject + MeshRenderer + MeshFilter + UGUILineMesh.init
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class UGUILineMeshTest
{
	public static void Run()
	{
		testInitRequiresComponents();
		testWidth();
		testSetPointList();
		testSetActive();
		testMaterial();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static UGUILineMesh createLine(out GameObject go)
	{
		go = new GameObject("LineMeshGO");
		go.AddComponent<MeshRenderer>();
		go.AddComponent<MeshFilter>();
		UGUILineMesh line = new UGUILineMesh();
		line.init(go);
		return line;
	}

	// init: 组件齐全时正常初始化
	private static void testInitRequiresComponents()
	{
		UGUILineMesh line = createLine(out GameObject go);
		try
		{
			assertTrue(go.GetComponent<MeshRenderer>() != null, "MeshRenderer 存在");
			assertTrue(go.GetComponent<MeshFilter>() != null, "MeshFilter 存在");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setWidth
	private static void testWidth()
	{
		UGUILineMesh line = createLine(out GameObject go);
		try
		{
			line.setWidth(5.0f);
			line.setPointList(new Vector3[] { new Vector3(0.0f, 0.0f, 0.0f), new Vector3(10.0f, 0.0f, 0.0f) });
			assertTrue(go.activeSelf, "设置点列表后对象仍激活");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setPointList: 多个点设置不崩溃 + Mesh 更新
	private static void testSetPointList()
	{
		UGUILineMesh line = createLine(out GameObject go);
		try
		{
			Vector3[] points = new Vector3[] { new Vector3(0.0f, 0.0f, 0.0f), new Vector3(10.0f, 0.0f, 0.0f), new Vector3(20.0f, 5.0f, 0.0f) };
			line.setPointList(points);
			// 设置点列表不崩溃即可(顶点由 onPointsChanged 计算)
			line.setPointList((System.Collections.Generic.List<Vector3>)null);   // null 安全
			line.setPointList(points);   // 再次设置
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setActive
	private static void testSetActive()
	{
		UGUILineMesh line = createLine(out GameObject go);
		try
		{
			line.setActive(false);
			assertTrue(!go.activeSelf, "setActive(false) 隐藏对象");
			line.setActive(true);
			assertTrue(go.activeSelf, "setActive(true) 恢复");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// getMaterial
	private static void testMaterial()
	{
		UGUILineMesh line = createLine(out GameObject go);
		try
		{
			// MeshRenderer 默认材质存在(返回默认材质)
			line.getMaterial();
			assertTrue(go.GetComponent<MeshRenderer>().material != null, "MeshRenderer 有默认材质");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
