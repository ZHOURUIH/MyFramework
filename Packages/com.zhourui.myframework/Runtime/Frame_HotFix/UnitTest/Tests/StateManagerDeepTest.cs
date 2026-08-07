using System;
using static TestAssert;

// StateManager 深度测试 — 状态互斥系统的复杂调用链
// 覆盖 allowKeepStateByGroup / allowAddStateByGroup / registeGroup / assignGroup 的核心判定逻辑
// 这些是本项目状态机系统最复杂的逻辑: 跨组互斥判定、多状态归属、主状态互斥策略
//
// 关键点(与既有 StateManagerTest 不同): 
//   allowKeepStateByGroup / allowAddStateByGroup 只依赖实例字段 mStateGroupList / mGroupStateList,
//   完全可以在局部 new StateManager() 上测, 不依赖全局 mStateManager 单例!
//   (既有 StateManagerTest 注释声称依赖全局单例是不准确的)
// 注: registeGroup 内部 setMutex 会读全局 mStateManager.getGroupMutex 获取互斥类型,
//   但创建的是局部 group/mutex 对象, 全局单例已初始化可用, 对象池残留可忽略。
public static class StateManagerDeepTest
{
	public static void Run()
	{
		// ─── 单组互斥基础 ───
		testSingleGroupRemoveOthers();
		testSingleGroupNoNew();
		testSingleGroupCoexist();
		// ─── 一个状态属于多个组(核心复杂场景) ───
		testStateInMultipleGroups();
		testMultipleGroupsNoShared();
		testSharedGroupDecides();
		// ─── 未分配组的状态共存 ───
		testUngroupedStateCoexist();
		// ─── 同类型状态短路 ───
		testSameStateShortCircuit();
		// ─── 主状态互斥策略 ───
		testMutexWithMain();
		testMutexWithMainOnly();
		testMutexInverseMain();
		// ─── 主状态与多组交叉 ───
		testMainStateAcrossGroups();
	}

	// ═════════════════════════════════════════════════════════════════
	// 单组: REMOVE_OTHERS — 添加新状态时移除组内其他所有状态
	// ═════════════════════════════════════════════════════════════════
	private static void testSingleGroupRemoveOthers()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(RemoveOthersGroup), GROUP_MUTEX.REMOVE_OTHERS);
			mgr.assignGroup(typeof(RemoveOthersGroup), typeof(StateA));
			mgr.assignGroup(typeof(RemoveOthersGroup), typeof(StateB));

			// B 与 A 同属 REMOVE_OTHERS 组: 添加 B 后不保留 A
			assertFalse(mgr.allowKeepStateByGroup(typeof(StateB), typeof(StateA)), "REMOVE_OTHERS: 添加B后不应保留A");
			assertTrue(mgr.allowAddStateByGroup(typeof(StateB), typeof(StateA)), "REMOVE_OTHERS: 允许添加B");
			// 反向一致
			assertFalse(mgr.allowKeepStateByGroup(typeof(StateA), typeof(StateB)), "REMOVE_OTHERS: 添加A后不应保留B");
			assertTrue(mgr.allowAddStateByGroup(typeof(StateA), typeof(StateB)), "REMOVE_OTHERS: 允许添加A");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 单组: NO_NEW — 组内有状态时不允许添加新状态
	// ═════════════════════════════════════════════════════════════════
	private static void testSingleGroupNoNew()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(NoNewGroup), GROUP_MUTEX.NO_NEW);
			mgr.assignGroup(typeof(NoNewGroup), typeof(StateC));
			mgr.assignGroup(typeof(NoNewGroup), typeof(StateD));

			// D 与 C 同属 NO_NEW 组: 存在 C 时不允许添加 D
			assertTrue(mgr.allowKeepStateByGroup(typeof(StateD), typeof(StateC)), "NO_NEW: 已存在的C可保留");
			assertFalse(mgr.allowAddStateByGroup(typeof(StateD), typeof(StateC)), "NO_NEW: 存在C时不允许添加D");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 单组: COEXIST — 组内状态完全共存
	// ═════════════════════════════════════════════════════════════════
	private static void testSingleGroupCoexist()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(CoexistGroup), GROUP_MUTEX.COEXIST);
			mgr.assignGroup(typeof(CoexistGroup), typeof(StateE));
			mgr.assignGroup(typeof(CoexistGroup), typeof(StateF));

			assertTrue(mgr.allowKeepStateByGroup(typeof(StateF), typeof(StateE)), "COEXIST: 保留E");
			assertTrue(mgr.allowAddStateByGroup(typeof(StateF), typeof(StateE)), "COEXIST: 允许添加F");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 一个状态属于多个组 — 核心复杂场景
	// 状态 A 属于 [G1=COEXIST, G2=REMOVE_OTHERS], 状态 B 属于 [G2=REMOVE_OTHERS]
	// 共同组只有 G2 → 判定走 G2 的 REMOVE_OTHERS
	// ═════════════════════════════════════════════════════════════════
	private static void testStateInMultipleGroups()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(MG1), GROUP_MUTEX.COEXIST);
			mgr.registeGroup(typeof(MG2), GROUP_MUTEX.REMOVE_OTHERS);
			// A 属于两个组
			mgr.assignGroup(typeof(MG1), typeof(StateX));
			mgr.assignGroup(typeof(MG2), typeof(StateX));
			// B 只属于 G2
			mgr.assignGroup(typeof(MG2), typeof(StateY));

			// X 与 Y 共同组是 G2(REMOVE_OTHERS) → 添加 X 后不保留 Y
			assertFalse(mgr.allowKeepStateByGroup(typeof(StateX), typeof(StateY)), "共同组G2为REMOVE_OTHERS: 不保留Y");
			// X 与 Y 共同组是 G2 → 允许添加
			assertTrue(mgr.allowAddStateByGroup(typeof(StateX), typeof(StateY)), "共同组G2: 允许添加X");

			// 反向: Y 只有 G2, X 有 G1+G2, 共同组仍 G2
			assertFalse(mgr.allowKeepStateByGroup(typeof(StateY), typeof(StateX)), "共同组G2: 不保留X");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 两个状态在不同组且无共同组 — 可共存
	// A 属于 [G1], B 属于 [G2], G1 与 G2 都 COEXIST, 无交集组
	// ═════════════════════════════════════════════════════════════════
	private static void testMultipleGroupsNoShared()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(NS1), GROUP_MUTEX.REMOVE_OTHERS);
			mgr.registeGroup(typeof(NS2), GROUP_MUTEX.REMOVE_OTHERS);
			mgr.assignGroup(typeof(NS1), typeof(StateA2));
			mgr.assignGroup(typeof(NS2), typeof(StateB2));

			// A2 在 G1, B2 在 G2, 无共同组 → 即便都是 REMOVE_OTHERS 也互不影响
			assertTrue(mgr.allowKeepStateByGroup(typeof(StateA2), typeof(StateB2)), "无共同组: 保留B2");
			assertTrue(mgr.allowKeepStateByGroup(typeof(StateB2), typeof(StateA2)), "无共同组: 保留A2");
			assertTrue(mgr.allowAddStateByGroup(typeof(StateA2), typeof(StateB2)), "无共同组: 允许添加");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 共同组决定判定 — 一个状态在 COEXIST 组, 另一状态在 COEXIST+NO_NEW
	// A 属于 [G1=COEXIST], B 属于 [G1=COEXIST, G2=NO_NEW]
	// 共同组 G1=COEXIST → 允许共存
	// ═════════════════════════════════════════════════════════════════
	private static void testSharedGroupDecides()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(SD1), GROUP_MUTEX.COEXIST);
			mgr.registeGroup(typeof(SD2), GROUP_MUTEX.NO_NEW);
			mgr.assignGroup(typeof(SD1), typeof(StateP));
			// Q 属于 COEXIST + NO_NEW 两组
			mgr.assignGroup(typeof(SD1), typeof(StateQ));
			mgr.assignGroup(typeof(SD2), typeof(StateQ));

			// P 与 Q 共同组是 G1(COEXIST) → 可共存
			assertTrue(mgr.allowKeepStateByGroup(typeof(StateP), typeof(StateQ)), "共同组G1=COEXIST: 保留Q");
			assertTrue(mgr.allowAddStateByGroup(typeof(StateP), typeof(StateQ)), "共同组G1=COEXIST: 允许添加P");

			// Q 自身 vs Q(同类型短路 true)
			assertTrue(mgr.allowKeepStateByGroup(typeof(StateQ), typeof(StateQ)), "同类型短路");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 未分配任何组的状态与任意状态共存
	// A 未 assign 组, B 属于 REMOVE_OTHERS 组
	// ═════════════════════════════════════════════════════════════════
	private static void testUngroupedStateCoexist()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(UnGroup), GROUP_MUTEX.REMOVE_OTHERS);
			mgr.assignGroup(typeof(UnGroup), typeof(StateG));

			// StateH 未分配任何组 → 与 G 可共存
			assertTrue(mgr.allowKeepStateByGroup(typeof(StateH), typeof(StateG)), "未分组状态H与G共存");
			assertTrue(mgr.allowAddStateByGroup(typeof(StateH), typeof(StateG)), "未分组状态H可添加");
			assertTrue(mgr.allowKeepStateByGroup(typeof(StateG), typeof(StateH)), "G与未分组H共存");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 同类型状态短路 — 直接返回 true 不查组
	// ═════════════════════════════════════════════════════════════════
	private static void testSameStateShortCircuit()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(ShortGroup), GROUP_MUTEX.REMOVE_OTHERS);
			mgr.assignGroup(typeof(ShortGroup), typeof(StateI));

			// 即使 REMOVE_OTHERS 组, 同类型状态仍 true
			assertTrue(mgr.allowKeepStateByGroup(typeof(StateI), typeof(StateI)), "同类型 keep 短路");
			assertTrue(mgr.allowAddStateByGroup(typeof(StateI), typeof(StateI)), "同类型 add 短路");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// MUTEX_WITH_MAIN — 仅与主状态互斥
	// 有主状态时不可添加其他状态; 没有主状态时可任意添加其他状态
	// ═════════════════════════════════════════════════════════════════
	private static void testMutexWithMain()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(MainGroup), GROUP_MUTEX.MUTEX_WITH_MAIN);
			mgr.assignGroup(typeof(MainGroup), typeof(MainS), true);   // MainS 为主状态
			mgr.assignGroup(typeof(MainGroup), typeof(OtherS1));
			mgr.assignGroup(typeof(MainGroup), typeof(OtherS2));

			// 有主状态 MainS 时, 不可添加其他状态
			assertFalse(mgr.allowAddStateByGroup(typeof(OtherS1), typeof(MainS)), "有主状态时不可添加其他状态");
			assertFalse(mgr.allowAddStateByGroup(typeof(OtherS2), typeof(MainS)), "有主状态时不可添加其他状态");
			// 添加主状态时, 移除其他所有状态
			assertFalse(mgr.allowKeepStateByGroup(typeof(MainS), typeof(OtherS1)), "添加主状态移除其他状态");
			// 没有主状态参与时(OtherS1 vs OtherS2), 可添加
			assertTrue(mgr.allowAddStateByGroup(typeof(OtherS2), typeof(OtherS1)), "无主状态参与时可添加");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// MUTEX_WITH_MAIN_ONLY — 仅与主状态互斥, 但允许添加其他状态
	// ═════════════════════════════════════════════════════════════════
	private static void testMutexWithMainOnly()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(MainOnlyGroup), GROUP_MUTEX.MUTEX_WITH_MAIN_ONLY);
			mgr.assignGroup(typeof(MainOnlyGroup), typeof(MainS2), true); // 主状态
			mgr.assignGroup(typeof(MainOnlyGroup), typeof(OtherS3));

			// 即使有主状态, 也允许添加其他状态(与 MUTEX_WITH_MAIN 的区别)
			assertTrue(mgr.allowAddStateByGroup(typeof(OtherS3), typeof(MainS2)), "MAIN_ONLY: 有主状态仍可添加其他");
			// 添加主状态时移除其他状态
			assertFalse(mgr.allowKeepStateByGroup(typeof(MainS2), typeof(OtherS3)), "MAIN_ONLY: 添加主状态移除其他");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// MUTEX_INVERSE_MAIN — 主状态反向互斥
	// 有其他状态时不可添加主状态; 添加其他状态时移除主状态
	// ═════════════════════════════════════════════════════════════════
	private static void testMutexInverseMain()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(InverseGroup), GROUP_MUTEX.MUTEX_INVERSE_MAIN);
			mgr.assignGroup(typeof(InverseGroup), typeof(MainS3), true); // 主状态
			mgr.assignGroup(typeof(InverseGroup), typeof(OtherS4));

			// 有其他状态时不可添加主状态
			assertFalse(mgr.allowAddStateByGroup(typeof(MainS3), typeof(OtherS4)), "INVERSE: 有其他状态时不可添加主状态");
			// 添加其他状态时移除主状态
			assertFalse(mgr.allowKeepStateByGroup(typeof(OtherS4), typeof(MainS3)), "INVERSE: 添加其他状态移除主状态");
			// 允许添加其他状态
			assertTrue(mgr.allowAddStateByGroup(typeof(OtherS4), typeof(MainS3)), "INVERSE: 允许添加其他状态");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 主状态与多组交叉 — 主状态状态同时属于两个互斥组
	// ═════════════════════════════════════════════════════════════════
	private static void testMainStateAcrossGroups()
	{
		var mgr = new StateManager();
		try
		{
			mgr.registeGroup(typeof(XG1), GROUP_MUTEX.MUTEX_WITH_MAIN);
			mgr.registeGroup(typeof(XG2), GROUP_MUTEX.REMOVE_OTHERS);
			// 主状态同时属于两个组
			mgr.assignGroup(typeof(XG1), typeof(StateM), true);
			mgr.assignGroup(typeof(XG2), typeof(StateM));
			mgr.assignGroup(typeof(XG1), typeof(StateN));

			// StateM 与 StateN 共同组是 XG1(MUTEX_WITH_MAIN)
			// 添加 StateM 时移除 StateN
			assertFalse(mgr.allowKeepStateByGroup(typeof(StateM), typeof(StateN)), "跨组主状态: 添加M移除N");
			// StateN 与 StateM: StateN 只属于 XG1, 共同组 XG1, MUTEX_WITH_MAIN 有主状态 M → 不可添加 N
			assertFalse(mgr.allowAddStateByGroup(typeof(StateN), typeof(StateM)), "跨组主状态: 有主状态M时不可添加N");
		}
		finally
		{
			mgr.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 私有类型(组类型 + 状态类型)
	// 注意: 组类型必须继承 StateGroup! registeGroup 内部会 CLASS<StateGroup>(type)
	//   用组类型从对象池创建 StateGroup 实例(createInstance<ClassObject>(type)),
	//   若组类型不继承 StateGroup/非 ClassObject 会创建失败返回 null → NPE
	// ═════════════════════════════════════════════════════════════════
	private class RemoveOthersGroup : StateGroup { }
	private class NoNewGroup : StateGroup { }
	private class CoexistGroup : StateGroup { }
	private class MG1 : StateGroup { }
	private class MG2 : StateGroup { }
	private class NS1 : StateGroup { }
	private class NS2 : StateGroup { }
	private class SD1 : StateGroup { }
	private class SD2 : StateGroup { }
	private class UnGroup : StateGroup { }
	private class ShortGroup : StateGroup { }
	private class MainGroup : StateGroup { }
	private class MainOnlyGroup : StateGroup { }
	private class InverseGroup : StateGroup { }
	private class XG1 : StateGroup { }
	private class XG2 : StateGroup { }
	private class StateA { }
	private class StateB { }
	private class StateC { }
	private class StateD { }
	private class StateE { }
	private class StateF { }
	private class StateX { }
	private class StateY { }
	private class StateA2 { }
	private class StateB2 { }
	private class StateP { }
	private class StateQ { }
	private class StateG { }
	private class StateH { }
	private class StateI { }
	private class MainS { }
	private class OtherS1 { }
	private class OtherS2 { }
	private class MainS2 { }
	private class OtherS3 { }
	private class MainS3 { }
	private class OtherS4 { }
	private class StateM { }
	private class StateN { }
}
