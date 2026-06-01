#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static TestAssert;

// GameEvent 单元测试
// 创建 / GetType / Equals
public static class GameEventTest
{
	public static void Run()
	{
		testCreate();
		testGetType();
		testIsEqual();
	}

	// ─── 创建 ────────────────────────────────────────────────────────────

	private static void testCreate()
	{
		var e = new GameEvent();
		assertNotNull(e, "GameEvent 实例不应为空");
	}

	// ─── GetType ─────────────────────────────────────────────────────────

	private static void testGetType()
	{
		var e = new GameEvent();
		assertEqual(typeof(GameEvent), e.GetType(), "GetType 应返回 GameEvent");
	}

	// ─── Equals (isEqual) ───────────────────────────────────────────────

	private static void testIsEqual()
	{
		var a = new GameEvent();
		var b = new GameEvent();
		assertTrue(a.Equals(a), "同一实例 Equals 应相等");
		assertFalse(a.Equals(b), "不同实例 Equals 应不相等");
	}
}
#endif
