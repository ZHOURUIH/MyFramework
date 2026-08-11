using System.Collections.Generic;
using UnityEngine;
using static TestAssert;
using static FrameUtility;

// myUGUITileImage 深度测试(TileImageRenderer 封装, 大量 Sprite 显示):
//   init(预加 TileImageRenderer) / setTileList+getTileCount / setTileMap+getTileCount
//   clearTile(池化回收 TileRenderData) / setSortingLayerName/setSortingOrder 守卫式
public static class MyUGUITileImageTest
{
	public static void Run()
	{
		testInitTileCountZero();
		testSetTileList();
		testSetTileMap();
		testClearTile();
		testSortingGuard();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static GameObject createTileImage(out myUGUITileImage tile)
	{
		GameObject go = new GameObject("TileImage");
		go.AddComponent<RectTransform>();
		go.AddComponent<TileImageRenderer>();
		tile = new myUGUITileImage();
		tile.setObject(go);
		tile.init();
		return go;
	}

	// init 后 tile 数量 0
	private static void testInitTileCountZero()
	{
		GameObject go = createTileImage(out myUGUITileImage tile);
		try
		{
			assertEqual(0, tile.getTileCount(), "init 后 tile 数量 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setTileList → getTileCount 对应
	private static void testSetTileList()
	{
		GameObject go = createTileImage(out myUGUITileImage tile);
		try
		{
			// TileRenderData 必须 CLASS 池化创建: clearTile 的 UN_CLASS_LIST 会回收池对象
			List<TileRenderData> list = new();
			CLASS(out TileRenderData t0);
			list.Add(t0);
			CLASS(out TileRenderData t1);
			list.Add(t1);
			CLASS(out TileRenderData t2);
			list.Add(t2);
			tile.setTileList(list);
			assertEqual(3, tile.getTileCount(), "setTileList 3 个 → count 3");
			tile.clearTile();   // 回收池化 TileRenderData
			assertEqual(0, tile.getTileCount(), "clearTile 后 count 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setTileMap → getTileCount 对应
	private static void testSetTileMap()
	{
		GameObject go = createTileImage(out myUGUITileImage tile);
		try
		{
			Dictionary<object, TileRenderData> map = new();
			CLASS(out TileRenderData tA);
			map.Add("a", tA);
			CLASS(out TileRenderData tB);
			map.Add("b", tB);
			tile.setTileMap(map);
			assertEqual(2, tile.getTileCount(), "setTileMap 2 个 → count 2");
			tile.clearTile();
			assertEqual(0, tile.getTileCount(), "clearTile 后 count 0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// clearTile 幂等(重复调用安全)
	private static void testClearTile()
	{
		GameObject go = createTileImage(out myUGUITileImage tile);
		try
		{
			CLASS(out TileRenderData t0);
			tile.setTileList(new() { t0 });
			tile.clearTile();
			tile.clearTile();   // 重复清空安全
			assertEqual(0, tile.getTileCount(), "重复 clearTile 安全");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSortingLayerName/setSortingOrder 守卫式(无 getter)
	private static void testSortingGuard()
	{
		GameObject go = createTileImage(out myUGUITileImage tile);
		try
		{
			tile.setSortingLayerName("UI");
			tile.setSortingOrder(5);
			// 守卫式调用不崩
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
