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
		testDestroyMiddleReusesID();
		testCreateDestroyCreateChain();
		testDestroyAllThenCreateFirst();
		testMultiDestroyCreateLoop();
		testCurveNamesUnique();
		testDestroyTwiceSafe();
		testCurveListSortedAfterCreate();
		testDestroyAllThenCreateSequence();
		testCreateInterleavedNames();
		testCreateManyCount();
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

	// ═════════════════════════════════════════════════════════════════
	// 组合: 销毁中间曲线后, 再创建会复用被释放的 ID(Find 找最小空闲)
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyMiddleReusesID()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			gk.createKeyframe();   // 101
			gk.createKeyframe();   // 102
			gk.createKeyframe();   // 103
			// 销毁中间 102
			gk.destroyKeyframe(gk.mCurveList[1]);
			assertEqual(2, gk.mCurveList.Count, "销毁后剩 2 条");
			// 再创建 → 101 存在、102 空闲 → 复用 102
			gk.createKeyframe();
			assertEqual(3, gk.mCurveList.Count, "再创建后 3 条");
			assertTrue(gk.mCurveList.Exists((CurveInfo info) => info.mID == 102), "新曲线复用 ID 102");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合: 创建 → 销毁首条 → 再创建, ID 复用回 101
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateDestroyCreateChain()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			gk.createKeyframe();   // 101
			gk.createKeyframe();   // 102
			gk.destroyKeyframe(gk.mCurveList[0]);   // 销毁 101
			gk.createKeyframe();   // 101 空闲 → 复用
			assertEqual(2, gk.mCurveList.Count, "创建-销毁-创建后 2 条");
			assertTrue(gk.mCurveList.Exists((CurveInfo info) => info.mID == 101), "销毁首条后再创建复用 101");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合: 全部销毁后列表为空, 再创建从 101 重新开始
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyAllThenCreateFirst()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			gk.createKeyframe();
			gk.createKeyframe();
			gk.destroyKeyframe(gk.mCurveList[0]);
			gk.destroyKeyframe(gk.mCurveList[0]);
			assertEqual(0, gk.mCurveList.Count, "全部销毁后列表空");
			AnimationCurve curve = gk.createKeyframe();
			assertEqual(101, gk.mCurveList[0].mID, "空列表再创建 ID 回到 101");
			assertTrue(ReferenceEquals(curve, gk.mCurveList[0].mCurve), "返回曲线与列表一致");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 组合: 多轮创建/全销毁循环, 每轮都从 101 开始
	// ═════════════════════════════════════════════════════════════════
	private static void testMultiDestroyCreateLoop()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			for (int round = 0; round < 3; ++round)
			{
				gk.createKeyframe();
				gk.createKeyframe();
				assertEqual(2, gk.mCurveList.Count, "第 " + (round + 1) + " 轮创建 2 条");
				assertEqual(101, gk.mCurveList[0].mID, "第 " + (round + 1) + " 轮首条 ID 101");
				assertEqual(102, gk.mCurveList[1].mID, "第 " + (round + 1) + " 轮次条 ID 102");
				// 全销毁
				gk.destroyKeyframe(gk.mCurveList[0]);
				gk.destroyKeyframe(gk.mCurveList[0]);
				assertEqual(0, gk.mCurveList.Count, "第 " + (round + 1) + " 轮销毁后空");
			}
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 深度组合
	// ═════════════════════════════════════════════════════════════════

	// 连续创建名字唯一
	private static void testCurveNamesUnique()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			gk.createKeyframe();
			gk.createKeyframe();
			gk.createKeyframe();
			assertEqual("curve101", gk.mCurveList[0].mName, "第 0 个名字 curve101");
			assertEqual("curve102", gk.mCurveList[1].mName, "第 1 个名字 curve102");
			assertEqual("curve103", gk.mCurveList[2].mName, "第 2 个名字 curve103");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 同一 info 销毁两次安全
	private static void testDestroyTwiceSafe()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			gk.createKeyframe();   // 创建 101(返回 AnimationCurve, 用列表取 CurveInfo)
			CurveInfo info = gk.mCurveList[0];
			gk.destroyKeyframe(info);
			gk.destroyKeyframe(info);
			assertEqual(0, gk.mCurveList.Count, "两次销毁后列表空");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 创建后列表按 ID 升序
	private static void testCurveListSortedAfterCreate()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			gk.createKeyframe();
			gk.createKeyframe();
			gk.createKeyframe();
			assertEqual(101, gk.mCurveList[0].mID, "列表[0] ID 101");
			assertEqual(102, gk.mCurveList[1].mID, "列表[1] ID 102");
			assertEqual(103, gk.mCurveList[2].mID, "列表[2] ID 103");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 全销毁后重建: ID 从 101 重新开始
	private static void testDestroyAllThenCreateSequence()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			gk.createKeyframe();
			gk.createKeyframe();
			gk.destroyKeyframe(gk.mCurveList[0]);
			gk.destroyKeyframe(gk.mCurveList[0]);
			gk.createKeyframe();
			gk.createKeyframe();
			assertEqual(101, gk.mCurveList[0].mID, "重建后首个 ID 101");
			assertEqual(102, gk.mCurveList[1].mID, "重建后第二个 ID 102");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 销毁中间重建 → 名字随 ID 复用
	private static void testCreateInterleavedNames()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			gk.createKeyframe();   // 101
			gk.createKeyframe();   // 102
			gk.destroyKeyframe(gk.mCurveList[1]);   // 销毁 102
			gk.createKeyframe();   // 复用 102
			assertEqual("curve102", gk.mCurveList[1].mName, "复用 ID 名字 curve102");
			assertEqual(2, gk.mCurveList.Count, "销毁重建后 2 条");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 连续创建 5 个: 计数与末尾 ID
	private static void testCreateManyCount()
	{
		GameObject go = new GameObject();
		try
		{
			GameKeyframe gk = go.AddComponent<GameKeyframe>();
			for (int i = 0; i < 5; ++i)
			{
				gk.createKeyframe();
			}
			assertEqual(5, gk.mCurveList.Count, "创建 5 个后计数 5");
			assertEqual(105, gk.mCurveList[4].mID, "末尾 ID 105");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
