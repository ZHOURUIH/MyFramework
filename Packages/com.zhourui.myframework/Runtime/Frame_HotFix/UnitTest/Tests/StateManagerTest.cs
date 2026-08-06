using System;
using static TestAssert;

// StateManager 单元测试 — 覆盖状态/参数类型的注册与查询、互斥组类型注册
// 注: 只测不依赖全局 mStateManager 单例的纯注册/查询部分
//     allowKeepStateByGroup / allowAddStateByGroup 依赖全局单例, 不在本测试范围
public static class StateManagerTest
{
	public static void Run()
	{
		testDefaultGroupMutex();
		testRegisteState();
		testGetStateTypeUnknown();
		testGetParamTypeUnknown();
	}

	// 测试用类型
	private class TestStateType { }
	private class TestParamType { }

	// ─── 构造时预注册的互斥组类型 ─────────────────────────────────────
	private static void testDefaultGroupMutex()
	{
		var mgr = new StateManager();
		assertEqual(typeof(StateGroupMutexCoexist), mgr.getGroupMutex(GROUP_MUTEX.COEXIST), "构造预注册 COEXIST 互斥类型");
		assertEqual(typeof(StateGroupMutexRemoveOthers), mgr.getGroupMutex(GROUP_MUTEX.REMOVE_OTHERS), "构造预注册 REMOVE_OTHERS 互斥类型");
		assertEqual(typeof(StateGroupMutexNoNew), mgr.getGroupMutex(GROUP_MUTEX.NO_NEW), "构造预注册 NO_NEW 互斥类型");
		assertEqual(typeof(StateGroupMutexMutexWithMain), mgr.getGroupMutex(GROUP_MUTEX.MUTEX_WITH_MAIN), "构造预注册 MUTEX_WITH_MAIN 互斥类型");
		assertEqual(typeof(StateGroupMutexMutexWithMainOnly), mgr.getGroupMutex(GROUP_MUTEX.MUTEX_WITH_MAIN_ONLY), "构造预注册 MUTEX_WITH_MAIN_ONLY 互斥类型");
		assertEqual(typeof(StateGroupMutexMutexInverseMain), mgr.getGroupMutex(GROUP_MUTEX.MUTEX_INVERSE_MAIN), "构造预注册 MUTEX_INVERSE_MAIN 互斥类型");
	}

	// ─── registeState / getStateType / getParamType ──────────────────
	private static void testRegisteState()
	{
		var mgr = new StateManager();
		mgr.registeState(1, typeof(TestStateType), typeof(TestParamType));
		assertEqual(typeof(TestStateType), mgr.getStateType(1), "registeState 后 getStateType 返回状态类型");
		assertEqual(typeof(TestParamType), mgr.getParamType(1), "registeState 后 getParamType 返回参数类型");
	}

	// ─── 未注册 id 查询返回 null ──────────────────────────────────────
	private static void testGetStateTypeUnknown()
	{
		var mgr = new StateManager();
		assertNull(mgr.getStateType(999), "未注册 id 查询状态类型返回 null");
	}

	private static void testGetParamTypeUnknown()
	{
		var mgr = new StateManager();
		assertNull(mgr.getParamType(999), "未注册 id 查询参数类型返回 null");
	}
}
