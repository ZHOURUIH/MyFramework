using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// GameKeyframe 单元测试 — 关键帧曲线列表编辑的纯逻辑
// 覆盖 createKeyframe/destroyKeyframe
// GameKeyframe 是 MonoBehaviour, 用 new GameObject().AddComponent 构造,
// 但 createKeyframe/destroyKeyframe 只操作 public 字段 mCurveList, 无任何外部依赖。
// GameObject 统一 try-finally 清理。
public static class GameKeyframeTest
{
	public static void Run()
	{
		testCreateKeyframe_EmptyList_FirstID101();
		testCreateKeyframe_Sequential_IncrementID();
		testCreateKeyframe_ReturnedCurve_MatchList();
		testDestroyKeyframe_RemoveOne();
		testDestroyKeyframe_NullList_NoThrow();
		testDestroyKeyframe_NullInfo_NoThrow();
	}

	// ═════════════════════════════════════════════════════════════════
	// 空列表首次 createKeyframe: ID 从 101 开始, 列表 1 个元素
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateKeyframe_EmptyList_FirstID101()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			AnimationCurve curve = gk.createKeyframe();
			assertNotNull(curve, "createKeyframe 返回非空 AnimationCurve");
			assertNotNull(gk.mCurveList, "mCurveList 被初始化");
			assertEqual(1, gk.mCurveList.Count, "空列表首次创建后有 1 个元素");
			assertEqual(101, gk.mCurveList[0].mID, "首个曲线 ID 为 101");
			assertTrue(gk.mCurveList[0].mName.StartsWith("curve"), "曲线名字以 curve 开头");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 连续创建: ID 递增(101,102,...), 列表保持按 ID 排序
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateKeyframe_Sequential_IncrementID()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			gk.createKeyframe();
			gk.createKeyframe();
			gk.createKeyframe();
			assertEqual(3, gk.mCurveList.Count, "三次创建后有 3 个元素");
			assertEqual(101, gk.mCurveList[0].mID, "第一个 ID 101");
			assertEqual(102, gk.mCurveList[1].mID, "第二个 ID 102");
			assertEqual(103, gk.mCurveList[2].mID, "第三个 ID 103");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 返回的曲线对象与列表内存储的是同一引用
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateKeyframe_ReturnedCurve_MatchList()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			AnimationCurve curve = gk.createKeyframe();
			assertTrue(ReferenceEquals(curve, gk.mCurveList[0].mCurve), "返回曲线与列表存储同一引用");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// destroyKeyframe 移除指定曲线信息
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyKeyframe_RemoveOne()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			AnimationCurve c1 = gk.createKeyframe();
			gk.createKeyframe();
			assertEqual(2, gk.mCurveList.Count, "创建两条曲线");
			// 移除第一条(ID 101 对应的 info)
			CurveInfo first = gk.mCurveList[0];
			gk.destroyKeyframe(first);
			assertEqual(1, gk.mCurveList.Count, "移除一条后剩 1 条");
			assertEqual(102, gk.mCurveList[0].mID, "剩的是 ID 102 那条");
			assertTrue(!ReferenceEquals(c1, gk.mCurveList[0].mCurve), "移除的不是残留的其它曲线");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// mCurveList 为 null(从未 create)时 destroyKeyframe 空安全
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyKeyframe_NullList_NoThrow()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			// 不调 createKeyframe, mCurveList 保持 null
			gk.destroyKeyframe(null);
			assertNull(gk.mCurveList, "未创建时 mCurveList 仍为 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// destroyKeyframe(null) 空安全(列表非空时传 null 不抛异常)
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyKeyframe_NullInfo_NoThrow()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			gk.createKeyframe();
			gk.destroyKeyframe(null);
			assertEqual(1, gk.mCurveList.Count, "传 null 不移除任何元素");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
