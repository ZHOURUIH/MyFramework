using static TestAssert;

// CharacterState 基类单元测试 — 覆盖角色状态基类的字段存取 / 计时更新 / 离开回调 / 移出回调
public static class CharacterStateTest
{
	public static void Run()
	{
		testDefaultValues();
		testSetGetID();
		testSetGetStateTime();
		testSetGetStateMaxTime();
		testSetGetMutexID();
		testSetGetActive();
		testSetGetIgnoreTimeScale();
		testSetGetJustEnter();
		testCanEnterDefault();
		testGetPriorityDefault();
		testSetGetCharacter();
		testSetGetParam();
		testLeaveCallback();
		testUpdateUnlimitedTime();
		testUpdateCountDown();
		testUpdateExpire();
		testResetProperty();
		testWillRemoveCallback();
		testWillRemoveCallbackRemove();
		testDestroy();
	

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

	// 内部测试用监听器
	private class TestEventListener : IEventListener { }

	// ─── 构造默认值 ────────────────────────────────────────────────────
	private static void testDefaultValues()
	{
		var state = new CharacterState();
		assertEqual(STATE_MUTEX.COEXIST, state.getMutexType(), "构造后互斥类型默认 COEXIST");
		assertEqual(BUFF_STATE_TYPE.NONE, state.getBuffStateType(), "构造后 buff 类型默认 NONE");
		assertTrue(state.isActive(), "构造后状态默认激活");
		assertEqual(0L, state.getID(), "构造后 ID 默认 0");
		assertEqual(-1.0f, state.getStateMaxTime(), "构造后最大持续时间默认 -1(无限制)");
		assertEqual(-1.0f, state.getStateTime(), "构造后当前持续时间默认 -1(无限制)");
		assertFalse(state.isJustEnter(), "构造后 justEnter 默认 false");
		assertFalse(state.isIgnoreTimeScale(), "构造后 ignoreTimeScale 默认 false");
		assertEqual(0, state.getMutexID(), "构造后互斥ID默认 0");
	}

	// ─── setID / getID ────────────────────────────────────────────────
	private static void testSetGetID()
	{
		var state = new CharacterState();
		state.setID(12345L);
		assertEqual(12345L, state.getID(), "setID 后 getID 返回该值");
		state.setID(-7L);
		assertEqual(-7L, state.getID(), "setID 支持负数");
	}

	// ─── setStateTime / getStateTime ──────────────────────────────────
	private static void testSetGetStateTime()
	{
		var state = new CharacterState();
		state.setStateTime(3.5f);
		assertEqual(3.5f, state.getStateTime(), 0.0001f, "setStateTime 后 getStateTime 返回该值");
		state.setStateTime(-1.0f);
		assertEqual(-1.0f, state.getStateTime(), 0.0001f, "setStateTime 可设置无限制");
	}

	// ─── setStateMaxTime / getStateMaxTime ────────────────────────────
	private static void testSetGetStateMaxTime()
	{
		var state = new CharacterState();
		state.setStateMaxTime(10.0f);
		assertEqual(10.0f, state.getStateMaxTime(), 0.0001f, "setStateMaxTime 后 getStateMaxTime 返回该值");
		state.setStateMaxTime(-1.0f);
		assertEqual(-1.0f, state.getStateMaxTime(), 0.0001f, "setStateMaxTime 可设置无限制");
	}

	// ─── setMutexID / getMutexID ─────────────────────────────────────
	private static void testSetGetMutexID()
	{
		var state = new CharacterState();
		state.setMutexID(99);
		assertEqual(99, state.getMutexID(), "setMutexID 后 getMutexID 返回该值");
		state.setMutexID(0);
		assertEqual(0, state.getMutexID(), "setMutexID 可设置为 0");
	}

	// ─── setActive / isActive ─────────────────────────────────────────
	private static void testSetGetActive()
	{
		var state = new CharacterState();
		state.setActive(false);
		assertFalse(state.isActive(), "setActive(false) 后 isActive 为 false");
		state.setActive(true);
		assertTrue(state.isActive(), "setActive(true) 后 isActive 为 true");
	}

	// ─── setIgnoreTimeScale / isIgnoreTimeScale ───────────────────────
	private static void testSetGetIgnoreTimeScale()
	{
		var state = new CharacterState();
		state.setIgnoreTimeScale(true);
		assertTrue(state.isIgnoreTimeScale(), "setIgnoreTimeScale(true) 后 isIgnoreTimeScale 为 true");
		state.setIgnoreTimeScale(false);
		assertFalse(state.isIgnoreTimeScale(), "setIgnoreTimeScale(false) 后 isIgnoreTimeScale 为 false");
	}

	// ─── setJustEnter / isJustEnter ───────────────────────────────────
	private static void testSetGetJustEnter()
	{
		var state = new CharacterState();
		state.setJustEnter(true);
		assertTrue(state.isJustEnter(), "setJustEnter(true) 后 isJustEnter 为 true");
		state.setJustEnter(false);
		assertFalse(state.isJustEnter(), "setJustEnter(false) 后 isJustEnter 为 false");
	}

	// ─── canEnter 默认 ────────────────────────────────────────────────
	private static void testCanEnterDefault()
	{
		var state = new CharacterState();
		assertTrue(state.canEnter(), "基类 canEnter 默认返回 true");
	}

	// ─── getPriority 默认 ─────────────────────────────────────────────
	private static void testGetPriorityDefault()
	{
		var state = new CharacterState();
		assertEqual(0, state.getPriority(), "基类 getPriority 默认返回 0");
	}

	// ─── setCharacter / getCharacter ──────────────────────────────────
	private static void testSetGetCharacter()
	{
		var state = new CharacterState();
		assertNull(state.getCharacter(), "未设置角色时 getCharacter 返回 null");
		state.setCharacter(null);
		assertNull(state.getCharacter(), "setCharacter(null) 后 getCharacter 返回 null");
	}

	// ─── setParam / getParam ──────────────────────────────────────────
	private static void testSetGetParam()
	{
		var state = new CharacterState();
		assertNull(state.getParam(), "未设置参数时 getParam 返回 null");
		var param = new StateParam();
		state.setParam(param);
		assertEqual(param, state.getParam(), "setParam 后 getParam 返回该参数");
		state.setParam(null);
		assertNull(state.getParam(), "setParam(null) 后 getParam 返回 null");
	}

	// ─── leave 触发离开回调 ───────────────────────────────────────────
	private static void testLeaveCallback()
	{
		var state = new CharacterState();
		bool called = false;
		bool cbBreak = false;
		bool cbWillDestroy = false;
		string cbParam = null;
		state.setLeaveCallback((s, isBreak, willDestroy, param) =>
		{
			called = true;
			cbBreak = isBreak;
			cbWillDestroy = willDestroy;
			cbParam = param;
		});
		state.leave(true, false, "testParam");
		assertTrue(called, "leave 应触发离开回调");
		assertTrue(cbBreak, "离开回调收到 isBreak=true");
		assertFalse(cbWillDestroy, "离开回调收到 willDestroy=false");
		assertEqual("testParam", cbParam, "离开回调收到 param 参数");
	}

	// ─── update: 无限制时间不递减 ─────────────────────────────────────
	private static void testUpdateUnlimitedTime()
	{
		var state = new CharacterState();
		state.setStateTime(-1.0f);
		state.update(2.0f);
		assertEqual(-1.0f, state.getStateTime(), 0.0001f, "无限制时间下 update 不改变时间");
	}

	// ─── update: 有限时间递减 ─────────────────────────────────────────
	private static void testUpdateCountDown()
	{
		var state = new CharacterState();
		state.setStateTime(5.0f);
		state.update(2.0f);
		assertEqual(3.0f, state.getStateTime(), 0.0001f, "update 递减剩余时间");
	}

	// ─── update: 时间耗尽触发移出,时间归 -1 ───────────────────────────
	private static void testUpdateExpire()
	{
		var state = new CharacterState();
		state.setStateTime(3.0f);
		state.update(3.0f);
		assertEqual(-1.0f, state.getStateTime(), 0.0001f, "时间耗尽后 update 将时间归 -1 并触发移出");
	}

	// ─── resetProperty 恢复默认 ───────────────────────────────────────
	private static void testResetProperty()
	{
		var state = new CharacterState();
		state.setID(5L);
		state.setStateTime(3.0f);
		state.setStateMaxTime(8.0f);
		state.setMutexID(2);
		state.setActive(false);
		state.setJustEnter(true);
		state.setIgnoreTimeScale(true);
		state.setCharacter(null);
		state.resetProperty();
		assertEqual(0L, state.getID(), "reset 后 ID 恢复 0");
		assertEqual(-1.0f, state.getStateTime(), 0.0001f, "reset 后当前时间恢复 -1");
		assertEqual(-1.0f, state.getStateMaxTime(), 0.0001f, "reset 后最大时间恢复 -1");
		assertEqual(0, state.getMutexID(), "reset 后互斥ID恢复 0");
		assertTrue(state.isActive(), "reset 后状态恢复激活");
		assertFalse(state.isJustEnter(), "reset 后 justEnter 恢复 false");
		assertFalse(state.isIgnoreTimeScale(), "reset 后 ignoreTimeScale 恢复 false");
		assertNull(state.getCharacter(), "reset 后角色清空");
		assertNull(state.getParam(), "reset 后参数清空");
		// 互斥类型在 reset 中不应重置(子类构造时指定后不可变)
		assertEqual(STATE_MUTEX.COEXIST, state.getMutexType(), "reset 后互斥类型保持不变");
	}

	// ─── addWillRemoveCallback + callWillRemoveCallback ───────────────
	private static void testWillRemoveCallback()
	{
		var state = new CharacterState();
		var listener = new TestEventListener();
		int callCount = 0;
		CharacterState cbState = null;
		state.addWillRemoveCallback(listener, (s) =>
		{
			callCount++;
			cbState = s;
		});
		state.callWillRemoveCallback();
		assertEqual(1, callCount, "callWillRemoveCallback 应触发已注册的回调");
		assertEqual(state, cbState, "回调参数为当前状态");
	}

	// ─── removeWillRemoveCallback 后再 call 不再触发 ──────────────────
	private static void testWillRemoveCallbackRemove()
	{
		var state = new CharacterState();
		var listener = new TestEventListener();
		int callCount = 0;
		state.addWillRemoveCallback(listener, (s) =>
		{
			callCount++;
		});
		state.removeWillRemoveCallback(listener);
		state.callWillRemoveCallback();
		assertEqual(0, callCount, "移除监听后 call 不再触发回调");
	}

	// ─── destroy 安全 ─────────────────────────────────────────────────
	private static void testDestroy()
	{
		var state = new CharacterState();
		var listener = new TestEventListener();
		int callCount = 0;
		state.addWillRemoveCallback(listener, (s) =>
		{
			callCount++;
		});
		state.destroy();
		// destroy 后移出回调列表被清空,再次 call 不触发
		state.callWillRemoveCallback();
		assertEqual(0, callCount, "destroy 清空回调列表后 call 不触发");
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
