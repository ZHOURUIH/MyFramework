using System;
using static TestAssert;

// StateManager 单元测试 — 覆盖状态/参数类型的注册与查询、互斥组类型注册
// 注: 只测不依赖全局 mStateManager 单例的纯注册/查询部分
//     更深入的复杂调用链测试(allowKeepStateByGroup / allowAddStateByGroup /
//     registeGroup / assignGroup 的跨组互斥判定)见 StateManagerDeepTest.cs
//     (修正: 这些方法只依赖实例字段, 不依赖全局单例, 用局部 new StateManager() 即可测)
public static class StateManagerTest
{
	public static void Run()
	{
		testDefaultGroupMutex();
		testRegisteState();
		testGetStateTypeUnknown();
		testGetParamTypeUnknown();
		testRegisteGroupMutexMapping();
		testGetGroupList();
		testGetStateGroup();
		testGetGroupNull();
	}

	// 测试用类型
	private class TestStateType { }
	private class TestParamType { }

	// ─── 用于验证状态组注册/查询的局部组类型(必须继承 StateGroup) ──
	private class GroupA : StateGroup { }
	private class GroupB : StateGroup { }
	private class GroupC : StateGroup { }
	private class MStateA { }
	private class MStateB { }

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

	// ─── registeGroupMutex 经 getGroupMutex 的映射契约 ─────────────────
	// 说明: 6 个 GROUP_MUTEX 枚举值全部在构造时经 registeAllGroupMutex
	//   → registeGroupMutex 预注册(StateManager.cs 118-131), 各自映射到对应的
	//   StateGroupMutexXxx 类型。mGroupMutexList 用 Dictionary.Add 填充,
	//   枚举恰有 6 个取值且全部已注册, 故不能在未释放的实例上再次按已有枚举值
	//   registeGroupMutex(会因重复 key 抛 ArgumentException, 且触发框架 error 日志)。
	//   这里用 registeGroup(type, mutex) 里的 getGroupMutex 读取链路来正向验证
	//   该映射被正确应用到新创建的状态组上(registeGroup 内部即 setMutex(getGroupMutex(mutex))
	//   → StateGroup.setMutex 会按返回的互斥类型实例化对应策略)。
	private static void testRegisteGroupMutexMapping()
	{
		var mgr = new StateManager();
		try
		{
			// 构造预注册的映射一致: 各枚举应映射到对应的具体互斥策略类型
			assertEqual(typeof(StateGroupMutexCoexist), mgr.getGroupMutex(GROUP_MUTEX.COEXIST), "COEXIST 映射");
			assertEqual(typeof(StateGroupMutexRemoveOthers), mgr.getGroupMutex(GROUP_MUTEX.REMOVE_OTHERS), "REMOVE_OTHERS 映射");
			assertEqual(typeof(StateGroupMutexNoNew), mgr.getGroupMutex(GROUP_MUTEX.NO_NEW), "NO_NEW 映射");
			assertEqual(typeof(StateGroupMutexMutexWithMain), mgr.getGroupMutex(GROUP_MUTEX.MUTEX_WITH_MAIN), "MUTEX_WITH_MAIN 映射");
			assertEqual(typeof(StateGroupMutexMutexWithMainOnly), mgr.getGroupMutex(GROUP_MUTEX.MUTEX_WITH_MAIN_ONLY), "MUTEX_WITH_MAIN_ONLY 映射");
			assertEqual(typeof(StateGroupMutexMutexInverseMain), mgr.getGroupMutex(GROUP_MUTEX.MUTEX_INVERSE_MAIN), "MUTEX_INVERSE_MAIN 映射");

			// registeGroup(type, mutex) 经 getGroupMutex 读到的映射会落到组实例上:
			// 用 NO_NEW 注册组后, 组互斥类型应为 NO_NEW 的策略类型
			mgr.registeGroup(typeof(GroupA), GROUP_MUTEX.NO_NEW);
			StateGroup group = mgr.getStateGroup(typeof(GroupA));
			assertNotNull(group, "registeGroup 后 getStateGroup 应返回组实例");
			assertEqual(GROUP_MUTEX.NO_NEW, group.mMutex.getMutexType(), "组互斥类型取自 getGroupMutex 映射");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── getGroupList: 返回状态所属的所有组类型列表 ─────────────────
	private static void testGetGroupList()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(GroupA), GROUP_MUTEX.COEXIST);
			mgr.registeGroup(typeof(GroupB), GROUP_MUTEX.COEXIST);
			mgr.registeGroup(typeof(GroupC), GROUP_MUTEX.COEXIST);
			// MStateA 属于 GroupA + GroupB 两个组
			mgr.assignGroup(typeof(GroupA), typeof(MStateA));
			mgr.assignGroup(typeof(GroupB), typeof(MStateA));

			var groups = mgr.getGroupList(typeof(MStateA));
			assertNotNull(groups, "已 assign 组的状态 getGroupList 不为 null");
			assertEqual(2, groups.Count, "MStateA 应属于 2 个组");
			assertTrue(groups.Contains(typeof(GroupA)), "包含组 A");
			assertTrue(groups.Contains(typeof(GroupB)), "包含组 B");
			assertFalse(groups.Contains(typeof(GroupC)), "不包含组 C");

			// MStateB 只属于一个组
			mgr.assignGroup(typeof(GroupC), typeof(MStateB));
			var groupsB = mgr.getGroupList(typeof(MStateB));
			assertEqual(1, groupsB.Count, "MStateB 应属于 1 个组");
			assertEqual(typeof(GroupC), groupsB[0], "MStateB 所属组为 C");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── getStateGroup: 返回已注册状态组的实例 ──────────────────────
	private static void testGetStateGroup()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(GroupA), GROUP_MUTEX.REMOVE_OTHERS);
			StateGroup group = mgr.getStateGroup(typeof(GroupA));
			assertNotNull(group, "注册后 getStateGroup 返回组实例");
			assertEqual(GROUP_MUTEX.REMOVE_OTHERS, group.mMutex.getMutexType(), "组互斥类型为 REMOVE_OTHERS");

			// assignGroup 指定主状态后, 组内状态列表应被填充
			mgr.assignGroup(typeof(GroupA), typeof(MStateA), true);
			assertTrue(group.mStateList.Count > 0, "assignGroup 后组内状态列表非空");
			assertTrue(group.hasState(typeof(MStateA)), "组内包含 MStateA");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ─── 未注册查询返回 null ────────────────────────────────────────
	private static void testGetGroupNull()
	{
		var mgr = new StateManager();
		try
		{
			assertNull(mgr.getGroupList(typeof(MStateA)), "未 assign 组的状态 getGroupList 返回 null");
			assertNull(mgr.getStateGroup(typeof(GroupC)), "未注册的状态组 getStateGroup 返回 null");
		}
		finally
		{
			mgr.destroy();
		}
	}
}
