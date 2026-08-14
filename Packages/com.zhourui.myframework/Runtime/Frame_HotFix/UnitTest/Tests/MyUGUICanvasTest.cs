using static TestAssert;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;

// myUGUICanvas: UGUI Canvas 封装——init 自动补 Canvas/GraphicRaycaster + sorting 设置
public static class MyUGUICanvasTest
{
	public static void Run()
	{
		testInitAddsCanvas();
		testInitAddsGraphicRaycaster();
		testSetSortingOrder();
		testSetSortingLayer();
		testGetCanvas();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// init: 无 Canvas 组件时 setIsNewObject(true) 自动 AddComponent, getCanvas 非 null
	private static void testInitAddsCanvas()
	{
		myUGUICanvas canvas = createCanvas(out GameObject go);
		try
		{
			assertNotNull(canvas.getCanvas(), "init 后 getCanvas 非 null");
			assertNotNull(go.GetComponent<Canvas>(), "GameObject 上已添加 Canvas 组件");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// init: 自动添加 GraphicRaycaster(射线接收)
	private static void testInitAddsGraphicRaycaster()
	{
		myUGUICanvas canvas = createCanvas(out GameObject go);
		try
		{
			assertNotNull(go.GetComponent<GraphicRaycaster>(), "init 后已添加 GraphicRaycaster");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setSortingOrder: 写入 Canvas.sortingOrder
	private static void testSetSortingOrder()
	{
		myUGUICanvas canvas = createCanvas(out GameObject go);
		try
		{
			canvas.setSortingOrder(100);
			assertEqual(100, canvas.getCanvas().sortingOrder, "setSortingOrder(100) 读回");
			canvas.setSortingOrder(-5);
			assertEqual(-5, canvas.getCanvas().sortingOrder, "setSortingOrder(-5) 读回");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// setSortingLayer: 写入 Canvas.sortingLayerName(只用 Unity 内置的 Default 层, 避免项目自定义层名差异)
	private static void testSetSortingLayer()
	{
		myUGUICanvas canvas = createCanvas(out GameObject go);
		try
		{
			canvas.setSortingLayer("Default");
			assertEqual("Default", canvas.getCanvas().sortingLayerName, "setSortingLayer(Default) 读回");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// getCanvas: 返回同一 Canvas 引用
	private static void testGetCanvas()
	{
		myUGUICanvas canvas = createCanvas(out GameObject go);
		try
		{
			Canvas c = go.GetComponent<Canvas>();
			assertTrue(ReferenceEquals(c, canvas.getCanvas()), "getCanvas 返回 GameObject 上的 Canvas");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static myUGUICanvas createCanvas(out GameObject go)
	{
		go = new GameObject("Canvas");
		myUGUICanvas canvas = new myUGUICanvas();
		// 无 Canvas 组件时自动补组件, 避免 init 的 logError 分支(getLayout().getName() NRE 陷阱)
		canvas.setIsNewObject(true);
		canvas.setObject(go);
		canvas.init();
		return canvas;
	}
}
