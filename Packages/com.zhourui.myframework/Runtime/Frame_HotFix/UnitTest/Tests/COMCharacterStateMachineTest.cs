using System;
using static TestAssert;

// COMCharacterStateMachine 深度测试 — 状态机同类型互斥判定(mutexWithExistState)
// 这是状态机最核心的纯逻辑: 处理 5 种 STATE_MUTEX 策略(添加新状态时与已存在同类型状态的关系)
//   NO_NEW: 不可添加相同新状态
//   REMOVE_OLD: 添加新状态, 移除互斥旧状态
//   COEXIST: 新旧共存
//   KEEP_HIGH_PRIORITY: 保留优先级最高的
//   OVERLAP_LAYER: 叠层通知
// 方法为 protected 纯逻辑, 不依赖 Character/init, 通过测试子类暴露后可直接测
public static class COMCharacterStateMachineTest
{
	// 测试用状态机子类, 暴露 protected mutexWithExistState
	class TestStateMachine : COMCharacterStateMachine
	{
		public bool TestMutex(CharacterState state, CharacterState exist, out CharacterState needRemove)
		{
			return mutexWithExistState(state, exist, out needRemove);
		}
	}

	// 测试用状态, 暴露 protected mMutexType 并支持自定义优先级
	class TestState : CharacterState
	{
		private int mPriority;
		public void SetMutexType(STATE_MUTEX type) { mMutexType = type; }
		public void SetPriority(int priority) { mPriority = priority; }
		public override int getPriority() { return mPriority; }
		public override void resetProperty()
		{
			base.resetProperty();
			mPriority = 0;
		}
	}

	public static void Run()
	{
		testNoNew();
		testRemoveOld();
		testCoexist();
		testKeepHighPriority_NewHigher();
		testKeepHighPriority_ExistHigher();
		testKeepHighPriority_EqualPriority();
		testOverlapLayer();
		testNullExistState();
	}

	// ═════════════════════════════════════════════════════════════════
	// NO_NEW — 存在同类型状态时, 不允许添加新状态
	// ═════════════════════════════════════════════════════════════════
	private static void testNoNew()
	{
		var sm = new TestStateMachine();
		var existState = makeState(STATE_MUTEX.NO_NEW, 0);
		var newState = makeState(STATE_MUTEX.NO_NEW, 0);
		bool result = sm.TestMutex(newState, existState, out CharacterState needRemove);
		assertFalse(result, "NO_NEW: 不允许添加新状态");
		assertNull(needRemove, "NO_NEW: 不移除任何状态");
	}

	// ═════════════════════════════════════════════════════════════════
	// REMOVE_OLD — 添加新状态时移除旧状态
	// ═════════════════════════════════════════════════════════════════
	private static void testRemoveOld()
	{
		var sm = new TestStateMachine();
		var existState = makeState(STATE_MUTEX.REMOVE_OLD, 0);
		var newState = makeState(STATE_MUTEX.REMOVE_OLD, 0);
		bool result = sm.TestMutex(newState, existState, out CharacterState needRemove);
		assertTrue(result, "REMOVE_OLD: 允许添加新状态");
		assertEqual(existState, needRemove, "REMOVE_OLD: 需要移除旧状态");
	}

	// ═════════════════════════════════════════════════════════════════
	// COEXIST — 新旧状态共存
	// ═════════════════════════════════════════════════════════════════
	private static void testCoexist()
	{
		var sm = new TestStateMachine();
		var existState = makeState(STATE_MUTEX.COEXIST, 0);
		var newState = makeState(STATE_MUTEX.COEXIST, 0);
		bool result = sm.TestMutex(newState, existState, out CharacterState needRemove);
		assertTrue(result, "COEXIST: 允许添加新状态");
		assertNull(needRemove, "COEXIST: 不移除任何状态");
	}

	// ═════════════════════════════════════════════════════════════════
	// KEEP_HIGH_PRIORITY — 新状态优先级更高, 移除旧状态
	// ═════════════════════════════════════════════════════════════════
	private static void testKeepHighPriority_NewHigher()
	{
		var sm = new TestStateMachine();
		var existState = makeState(STATE_MUTEX.KEEP_HIGH_PRIORITY, 5);
		var newState = makeState(STATE_MUTEX.KEEP_HIGH_PRIORITY, 10);
		bool result = sm.TestMutex(newState, existState, out CharacterState needRemove);
		assertTrue(result, "KEEP_HIGH_PRIORITY: 新状态优先级更高, 允许添加");
		assertEqual(existState, needRemove, "KEEP_HIGH_PRIORITY: 移除低优先级旧状态");
	}

	// ═════════════════════════════════════════════════════════════════
	// KEEP_HIGH_PRIORITY — 旧状态优先级更高, 不允许添加新状态
	// ═════════════════════════════════════════════════════════════════
	private static void testKeepHighPriority_ExistHigher()
	{
		var sm = new TestStateMachine();
		var existState = makeState(STATE_MUTEX.KEEP_HIGH_PRIORITY, 10);
		var newState = makeState(STATE_MUTEX.KEEP_HIGH_PRIORITY, 5);
		bool result = sm.TestMutex(newState, existState, out CharacterState needRemove);
		assertFalse(result, "KEEP_HIGH_PRIORITY: 旧状态优先级更高, 不允许添加");
		assertNull(needRemove, "KEEP_HIGH_PRIORITY: 不移除任何状态");
	}

	// ═════════════════════════════════════════════════════════════════
	// KEEP_HIGH_PRIORITY — 优先级相等(旧状态不严格更低), 不允许添加
	// ═════════════════════════════════════════════════════════════════
	private static void testKeepHighPriority_EqualPriority()
	{
		var sm = new TestStateMachine();
		var existState = makeState(STATE_MUTEX.KEEP_HIGH_PRIORITY, 7);
		var newState = makeState(STATE_MUTEX.KEEP_HIGH_PRIORITY, 7);
		// 条件 existState.getPriority() < state.getPriority() → 7<7 false → 不允许
		bool result = sm.TestMutex(newState, existState, out CharacterState needRemove);
		assertFalse(result, "KEEP_HIGH_PRIORITY: 优先级相等不允许添加");
		assertNull(needRemove, "KEEP_HIGH_PRIORITY: 不移除任何状态");
	}

	// ═════════════════════════════════════════════════════════════════
	// OVERLAP_LAYER — 叠层通知, 允许添加(后续在 enter 前销毁)
	// ═════════════════════════════════════════════════════════════════
	private static void testOverlapLayer()
	{
		var sm = new TestStateMachine();
		var existState = makeState(STATE_MUTEX.OVERLAP_LAYER, 0);
		var newState = makeState(STATE_MUTEX.OVERLAP_LAYER, 0);
		bool result = sm.TestMutex(newState, existState, out CharacterState needRemove);
		assertTrue(result, "OVERLAP_LAYER: 允许添加(叠层通知)");
		assertNull(needRemove, "OVERLAP_LAYER: 不移除任何状态(进入前才销毁)");
	}

	// ═════════════════════════════════════════════════════════════════
	// 无已存在状态 — 直接允许添加
	// ═════════════════════════════════════════════════════════════════
	private static void testNullExistState()
	{
		var sm = new TestStateMachine();
		var newState = makeState(STATE_MUTEX.COEXIST, 0);
		bool result = sm.TestMutex(newState, null, out CharacterState needRemove);
		assertTrue(result, "无已存在状态时允许添加");
		assertNull(needRemove, "无需移除任何状态");
	}

	// ─── 构造测试状态 ────────────────────────────────────────────────
	private static TestState makeState(STATE_MUTEX mutexType, int priority)
	{
		TestState state = new TestState();
		state.SetMutexType(mutexType);
		state.SetPriority(priority);
		return state;
	}
}
