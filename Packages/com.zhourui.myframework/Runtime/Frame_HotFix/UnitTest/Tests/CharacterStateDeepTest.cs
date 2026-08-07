using static TestAssert;

// CharacterState 深度测试
// 聚焦复杂交互链：多个监听者回调注册/触发的时序、回调移除后不触发、
// update 跨多个时间片的分段累计、时间耗尽精确触发、子类重写派生行为、
// callback 的可选参数链路。
public static class CharacterStateDeepTest
{
	public static void Run()
	{
		testMultipleListeners_AllFired();
		testMultipleCallbacks_OneListener_Order();
		testRemoveOneListener_OtherStillFired();
		testCallTwice_CallbacksFiredOnce();
		testUpdate_FractionalAccumulation();
		testUpdate_ExactExpire();
		testUpdate_JustOverExpire();
		testUpdate_RespawnTime_NoExpire();
		testLeaveCallback_OptionalParams();
		testLeave_RepeatedCalls();
		testSubclassPriorityOverride();
		testSubclassCanEnterOverride();
		testBuffStateType_Consistent();
		testMutexID_And_Type_Independent();
		testStateTimeResetsToMinusOne_AfterExpire();
	}

	// 内部测试监听器（不同实例）
	private class ListenerA : IEventListener { }
	private class ListenerB : IEventListener { }

	// ─── 多个监听者：全部触发 ───────────────────────────────────────
	private static void testMultipleListeners_AllFired()
	{
		var state = new CharacterState();
		var a = new ListenerA();
		var b = new ListenerB();
		int aCount = 0;
		int bCount = 0;
		state.addWillRemoveCallback(a, (s) => ++aCount);
		state.addWillRemoveCallback(b, (s) => ++bCount);
		state.callWillRemoveCallback();
		assertEqual(1, aCount, "监听者 A 触发");
		assertEqual(1, bCount, "监听者 B 触发");
		// call 之后列表被清空，再次调用不应再触发
		state.callWillRemoveCallback();
		assertEqual(1, aCount, "二次 call A 不再触发");
		assertEqual(1, bCount, "二次 call B 不再触发");
	}

	// ─── 同一监听者多个回调：按注册顺序触发 ─────────────────────────
	private static void testMultipleCallbacks_OneListener_Order()
	{
		var state = new CharacterState();
		var listener = new ListenerA();
		var order = new System.Collections.Generic.List<int>();
		state.addWillRemoveCallback(listener, (s) => order.Add(1));
		state.addWillRemoveCallback(listener, (s) => order.Add(2));
		state.callWillRemoveCallback();
		assertEqual(2, order.Count, "两个回调均触发");
		assertEqual(1, order[0], "第一个注册的回调先触发");
		assertEqual(2, order[1], "第二个注册的回调后触发");
	}

	// ─── 移除一个监听者，另一个仍触发 ───────────────────────────────
	private static void testRemoveOneListener_OtherStillFired()
	{
		var state = new CharacterState();
		var a = new ListenerA();
		var b = new ListenerB();
		int aCount = 0;
		int bCount = 0;
		state.addWillRemoveCallback(a, (s) => ++aCount);
		state.addWillRemoveCallback(b, (s) => ++bCount);
		state.removeWillRemoveCallback(a);
		state.callWillRemoveCallback();
		assertEqual(0, aCount, "已移除的 A 不触发");
		assertEqual(1, bCount, "未移除的 B 触发");
	}

	// ─── call 两次，回调只触发一次（列表清空） ─────────────────────
	private static void testCallTwice_CallbacksFiredOnce()
	{
		var state = new CharacterState();
		var listener = new ListenerA();
		int count = 0;
		state.addWillRemoveCallback(listener, (s) => ++count);
		state.callWillRemoveCallback();
		state.callWillRemoveCallback();
		// 首次 call 触发并清空，第二次不再触发
		assertEqual(1, count, "二次 call 只触发一次");
	}

	// ─── update 分段累计：分多次小步长累加不提前超时 ───────────────
	private static void testUpdate_FractionalAccumulation()
	{
		var state = new CharacterState();
		state.setStateTime(10.0f);
		// 5 次 1.5 秒 = 7.5 秒，未耗尽
		for (int i = 0; i < 5; ++i)
		{
			state.update(1.5f);
		}
		assertEqual(2.5f, state.getStateTime(), 0.0001f, "分段累计 10-7.5=2.5");
		// 再 2 次 1.5 = 3 秒，耗尽并归 -1
		state.update(1.5f);
		state.update(1.5f);
		assertEqual(-1.0f, state.getStateTime(), 0.0001f, "累计耗尽后归 -1");
	}

	// ─── update 恰好精确耗尽 ───────────────────────────────────────
	private static void testUpdate_ExactExpire()
	{
		var state = new CharacterState();
		state.setStateTime(4.0f);
		state.update(4.0f);
		assertEqual(-1.0f, state.getStateTime(), 0.0001f, "精确耗尽即触发归 -1");
	}

	// ─── update 一步略超耗尽 ─────────────────────────────────────────
	private static void testUpdate_JustOverExpire()
	{
		var state = new CharacterState();
		state.setStateTime(2.0f);
		state.update(2.5f);
		assertEqual(-1.0f, state.getStateTime(), 0.0001f, "略超即耗尽归 -1");
	}

	// ─── 重新设置时间后继续运行不触发过期 ───────────────────────────
	private static void testUpdate_RespawnTime_NoExpire()
	{
		var state = new CharacterState();
		state.setStateTime(5.0f);
		state.update(3.0f); // 剩 2.0
		assertEqual(2.0f, state.getStateTime(), 0.0001f, "第一次递减后 2.0");
		state.setStateTime(8.0f); // 重新设定
		state.update(1.0f);
		assertEqual(7.0f, state.getStateTime(), 0.0001f, "重新设定后重新递减");
	}

	// ─── leave 可选参数回传 ─────────────────────────────────────────
	private static void testLeaveCallback_OptionalParams()
	{
		var state = new CharacterState();
		bool breakFlag = false;
		bool destroyFlag = false;
		string p = "sentinel";
		state.setLeaveCallback((s, isBreak, willDestroy, param) =>
		{
			breakFlag = isBreak;
			destroyFlag = willDestroy;
			p = param;
		});
		state.leave(true, true, null);
		assertTrue(breakFlag, "leave(isBreak=true) 回传");
		assertTrue(destroyFlag, "leave(willDestroy=true) 回传");
		assertNull(p, "leave(param=null) 回传 null");
	}

	// ─── leave 重复调用（回调仍可重复触发，属于外部显式调用） ─────
	private static void testLeave_RepeatedCalls()
	{
		var state = new CharacterState();
		int count = 0;
		state.setLeaveCallback((s, b, d, p) => ++count);
		state.leave(false, false, null);
		state.leave(false, false, null);
		assertEqual(2, count, "leave 每次调用均触发回调");
	}

	// ─── 子类重写 getPriority ───────────────────────────────────────
	private static void testSubclassPriorityOverride()
	{
		var state = new HighPriorityState();
		assertEqual(50, state.getPriority(), "子类重写 getPriority 返回 50");
		var baseState = new CharacterState();
		assertEqual(0, baseState.getPriority(), "基类 getPriority 返回 0");
	}

	// ─── 子类重写 canEnter ──────────────────────────────────────────
	private static void testSubclassCanEnterOverride()
	{
		var state = new ConditionedCanEnter();
		assertFalse(state.canEnter(), "子类重写 canEnter 返回 false");
	}

	// ─── buffStateType 在实例生命周期一致性 ────────────────────────
	private static void testBuffStateType_Consistent()
	{
		var state = new CharacterState();
		assertEqual(BUFF_STATE_TYPE.NONE, state.getBuffStateType(), "基类 buff 类型默认 NONE");
		// getter 在 set 后仍稳定
		state.resetProperty();
		assertEqual(BUFF_STATE_TYPE.NONE, state.getBuffStateType(), "reset 后 buff 类型仍默认");
	}

	// ─── mutexType 与 mutexID 独立 ──────────────────────────────────
	private static void testMutexID_And_Type_Independent()
	{
		var state = new CharacterState();
		state.setMutexID(7);
		assertEqual(7, state.getMutexID(), "mutexID=7");
		// mutexType 是构造时设定，与 mutexID 无关
		assertEqual(STATE_MUTEX.COEXIST, state.getMutexType(), "mutexType 保持 COEXIST");
		state.resetProperty();
		assertEqual(0, state.getMutexID(), "reset 后 mutexID=0");
		assertEqual(STATE_MUTEX.COEXIST, state.getMutexType(), "reset 后 mutexType 不变");
	}

	// ─── 状态时间耗尽后归 -1（不再残留旧计时值） ──────────────────
	private static void testStateTimeResetsToMinusOne_AfterExpire()
	{
		var state = new CharacterState();
		state.setStateTime(1.0f);
		state.update(1.0f);
		assertEqual(-1.0f, state.getStateTime(), 0.0001f, "耗尽后 stateTime=-1");
		state.setStateTime(3.0f);
		state.update(1.0f);
		assertEqual(2.0f, state.getStateTime(), 0.0001f, "重新设定后正常递减");
	}

	// ─── 内部测试子类 ───────────────────────────────────────────────
	private class HighPriorityState : CharacterState
	{
		public override int getPriority() { return 50; }
	}
	private class ConditionedCanEnter : CharacterState
	{
		public override bool canEnter() { base.canEnter(); return false; }
	}
}
