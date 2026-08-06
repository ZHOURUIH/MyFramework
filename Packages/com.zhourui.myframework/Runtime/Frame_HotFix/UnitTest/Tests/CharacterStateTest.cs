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
}
