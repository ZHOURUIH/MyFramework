using System;
using UnityEngine;
using static TestAssert;
using UObject = UnityEngine.Object;

// QuickEffect 单元测试
// 框架环境已完全初始化, 可覆盖:
//   构造 (继承 GameEffect)
//   setObject(无拖尾对象) 不触发拖尾 logError
//   playQuick(pos) 设置位置并记录 mLastPlayTime
//   getLastPlayTime 默认 / playQuick 后 刷新
//   resetProperty 重置 mLastPlayTime 为 MinValue
public static class QuickEffectTest
{
	public static void Run()
	{
		testConstruct();
		testSetObjectNoTrailNoError();
		testPlayQuickMovesAndRecords();
		testGetLastPlayTimeDefault();
		testResetPropertyClearsLastPlayTime();
	}

	// ═════════════════════════════════════════════════════════════════
	// 构造
	// ═════════════════════════════════════════════════════════════════
	private static void testConstruct()
	{
		QuickEffect effect = new();
		assertNotNull(effect, "QuickEffect 可构造");
		effect.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// setObject
	// ═════════════════════════════════════════════════════════════════
	private static void testSetObjectNoTrailNoError()
	{
		QuickEffect effect = new();
		var go = new GameObject("QESet");
		effect.setExistObject(true);
		try
		{
			// 无拖尾对象: setObject 不会触发 QuickEffect 的拖尾 logError 分支
			effect.setObject(go);
			assertEqual(go, effect.getGameObject(), "setObject 应绑定 GameObject");
		}
		finally
		{
			UObject.DestroyImmediate(go);
			effect.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// playQuick
	// ═════════════════════════════════════════════════════════════════
	private static void testPlayQuickMovesAndRecords()
	{
		QuickEffect effect = new();
		var go = new GameObject("QEPlay");
		effect.setExistObject(true);
		try
		{
			effect.setObject(go);
			var pos = new Vector3(5, 6, 7);
			effect.playQuick(pos);
			// playQuick 设置位置
			assertEqual(pos, effect.getPosition(), "playQuick 应设置特效位置");
			// playQuick 记录 mLastPlayTime 为当前时间 (非 MinValue)
			assertTrue(effect.getLastPlayTime() > DateTime.MinValue, "playQuick 后 getLastPlayTime 应为当前时间");
		}
		finally
		{
			UObject.DestroyImmediate(go);
			effect.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// getLastPlayTime
	// ═════════════════════════════════════════════════════════════════
	private static void testGetLastPlayTimeDefault()
	{
		QuickEffect effect = new();
		assertEqual(DateTime.MinValue, effect.getLastPlayTime(), "默认 mLastPlayTime 为 MinValue");
		effect.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty
	// ═════════════════════════════════════════════════════════════════
	private static void testResetPropertyClearsLastPlayTime()
	{
		QuickEffect effect = new();
		var go = new GameObject("QEReset");
		effect.setExistObject(true);
		try
		{
			effect.setObject(go);
			effect.playQuick(Vector3.zero);
			assertTrue(effect.getLastPlayTime() > DateTime.MinValue, "playQuick 后记录时间");
			effect.resetProperty();
			assertEqual(DateTime.MinValue, effect.getLastPlayTime(), "resetProperty 重置 mLastPlayTime 为 MinValue");
		}
		finally
		{
			UObject.DestroyImmediate(go);
			effect.destroy();
		}
	}
}
