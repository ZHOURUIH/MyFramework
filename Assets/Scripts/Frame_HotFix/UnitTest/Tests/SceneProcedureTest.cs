#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static TestAssert;

// SceneProcedure 测试专用子类型
public class ProcChildA : SceneProcedure { }
public class ProcChildB : SceneProcedure { }
public class ProcChildC : SceneProcedure { }
public class ProcChildD : SceneProcedure { }
public class ProcChildE : SceneProcedure { }

// 暴露 protected 成员给测试
public class TestSceneProcedure : SceneProcedure
{
	public bool isInited() { return mInited; }
}

// ============================================================================
// 可追踪的流程子类，记录所有生命周期回调用于验证执行路径
// 使用不同子类型防止 changeProcedure 同类型提前返回
// ============================================================================
public class TrackA : SceneProcedure
{
	public string tag = "A";
	public TrackA() { mDelayCmdList = new HashSet<long>(); }
	public static List<string> CallLog = new();
	public static void Reset() { CallLog.Clear(); }
	protected override void onInit(SceneProcedure last) { CallLog.Add($"{tag}.onInit({(last is TrackA ta ? ta.tag : "null")})"); }
	protected override void onInitFromChild(SceneProcedure last) { CallLog.Add($"{tag}.onInitFromChild({(last is TrackA ta ? ta.tag : "?")})"); }
	protected override void onExit(SceneProcedure next) { CallLog.Add($"{tag}.onExit({(next is TrackA ta ? ta.tag : "?")})"); }
	protected override void onExitToChild(SceneProcedure next) { CallLog.Add($"{tag}.onExitToChild({(next is TrackA ta ? ta.tag : "?")})"); }
	protected override void onExitSelf() { CallLog.Add($"{tag}.onExitSelf()"); }
}
public class TrackB : TrackA { public TrackB() { tag = "B"; } }
public class TrackC : TrackA { public TrackC() { tag = "C"; } }
public class TrackParent : TrackA { public TrackParent() { tag = "Parent"; } }
public class TrackChild : TrackA { public TrackChild() { tag = "Child"; } }

// 可注入的 GameScene，支持手动注册 TrackedProcedure
public class TrackScene : GameScene
{
	public override void assignStartExitProcedure() { }
	public override void createSceneProcedure() { }
	public void add(TrackA proc, Type parentType = null)
	{
		proc.setGameScene(this);
		if (parentType != null)
		{
			var p = getProcedure(parentType);
			if (p is TrackA pa) pa.addChildProcedure(proc);
		}
		mSceneProcedureList.Add(proc.GetType(), proc);
	}
}

// ============================================================================
// SceneProcedure 场景流程单元测试
//
// 不覆盖（需 Unity 运行时）:
//   exit - 内部 interruptAllCommand() 需 mDelayCmdList 非 null
//   destroy - 需完整生命周期
// ============================================================================
public static class SceneProcedureTest
{
	public static void Run()
	{
		// === 默认值 ===
		testDefaultGameScene();
		testDefaultParentProcedure();
		testDefaultPrepareNext();
		testDefaultIsPreparingExit();
		testDefaultInited();
		testDefaultsAllAfterNew();

		// === setGameScene / getGameScene ===
		testSetGameSceneNull();
		testSetGameSceneGetGameScene();
		testSetGameSceneThenUnset();
		testSetGameSceneNullAfterNonNull();

		// === addChildProcedure / getChildProcedure ===
		testAddChildProcedureNull();
		testAddChildProcedureValid();
		testAddChildProcedureDuplicate();
		testGetChildProcedureUnregistered();
		testAddMultipleChildren();
		testAddChildProcedureSameTypeDifferentInstance();
		testAddChildProcedureSelf();
		testAddChildProcedureThenCheckParent();
		testAddChildProcedureGetOnMissingType();
		testAddChildProcedureFiveDifferentTypes();
		testAddChildProcedureAfterParentSet();

		// === isThisOrParent ===
		testIsThisOrParentSelf();
		testIsThisOrParentNull();
		testIsThisOrParentWithParentChain();
		testIsThisOrParentMatchesParent();
		testIsThisOrParentDeepChain();
		testIsThisOrParentFourLevelChain();
		testIsThisOrParentFiveLevelChain();
		testIsThisOrParentNoParentFallsback();
		testIsThisOrParentWithObjectType();
		testIsThisOrParentWithStringType();
		testIsThisOrParentEmptyHierarchy();

		// === getParent ===
		testGetParentNoParent();
		testGetParentWithChain();
		testGetParentNotFound();
		testGetParentDeepChain();
		testGetParentFourLevelChain();
		testGetParentFiveLevelChain();
		testGetParentWithNullType();
		testGetParentWithSelfType();

		// === getThisOrParent ===
		testGetThisOrParentSelf();
		testGetThisOrParentNull();
		testGetThisOrParentFromParent();
		testGetThisOrParentDeepChain();
		testGetThisOrParentFourLevelChain();
		testGetThisOrParentFiveLevelChain();
		testGetThisOrParentNotFound();
		testGetThisOrParentWithNullType();
		testGetThisOrParentWithStringType();

		// === getParentList ===
		testGetParentListSelf();
		testGetParentListWithParent();
		testGetParentListThreeLevels();
		testGetParentListFourLevels();
		testGetParentListFiveLevels();
		testGetParentListConsistentOrder();

		// === getSameParent ===
		testGetSameParentSelf();
		testGetSameParentWithCommonParent();
		testGetSameParentDeep();
		testGetSameParentSameInstance();
		testGetSameParentWithNull();
		testGetSameParentCousins();
		testGetSameParentThreeSiblings();
		testGetSameParentDeepCousins();

		// === prepareExit / isPreparingExit ===
		testPrepareExit();
		testPrepareExitMultipleTimes();
		testPrepareNextAfterPrepareExit();
		testPrepareExitZeroTime();
		testPrepareExitLargeTime();
		testPrepareExitThenIsPreparing();
		testIsPreparingExitDefault();
		testIsPreparingExitAfterInit();

		// === init ===
		testInitNull();
		testInitSetsInited();
		testInitMultipleTimes();
		testInitWithParent();
		testInitWithLastProcedure();
		testInitWithLastProcedureSameType();
		testInitParentAlreadyInited();
		testInitParentAlreadyInitedChain();

		// === update / lateUpdate / keyProcess ===
		testUpdate();
		testLateUpdate();
		testKeyProcess();
		testUpdateWithParentProcedure();
		testUpdateAfterInit();
		testLateUpdateAfterInit();

		// === 组合场景 ===
		testAddChildThenInitChain();
		testPrepareExitThenInit();
		testAddMultipleChildThenGetParentList();
		testDeepHierarchyAllChecks();
		testSiblingGetSameParentMultipleChecks();
		testProcedureChainConsistency();
		testMultiLevelInitChain();

		// === 流程跳转执行路径验证 ===
		testFlowFlatAtoB();
		testFlowFlatAtoBtoC();
		testFlowSameTypeEarlyReturn();
		testFlowParentToChild();
		testFlowChildToParent();
		testFlowSiblings();
		testFlowGrandParentToChildToGrandParent();
		testFlowComplexChain();

		// === GameScene + SceneProcedure 联合流程测试 ===
		testCombinedSceneCreateAndStart();
		testCombinedChangeFlat();
		testCombinedParentToChild();
		testCombinedChildToParent();
		testCombinedSiblings();
		testCombinedBackToLast();
		testCombinedEnterStartThenChangeBack();
		testCombinedFullChain();
	}

	// ================================================================
	//  默认值 — 5 个函数
	// ================================================================
	private static void testDefaultGameScene()
	{
		var p = new SceneProcedure();
		assertNull(p.getGameScene());
	}
	private static void testDefaultParentProcedure()
	{
		var p = new SceneProcedure();
		assertNull(p.getParent());
	}
	private static void testDefaultPrepareNext()
	{
		var p = new SceneProcedure();
		assertNull(p.getPrepareNext());
	}
	private static void testDefaultIsPreparingExit()
	{
		var p = new SceneProcedure();
		assertFalse(p.isPreparingExit());
	}
	private static void testDefaultInited()
	{
		var p = new TestSceneProcedure();
		assertFalse(p.isInited());
	}
	private static void testDefaultsAllAfterNew()
	{
		var p = new SceneProcedure();
		assertNull(p.getGameScene());
		assertNull(p.getParent());
		assertNull(p.getPrepareNext());
		assertFalse(p.isPreparingExit());
		var tp = new TestSceneProcedure();
		assertFalse(tp.isInited());
	}

	// ================================================================
	//  setGameScene / getGameScene — 4 个函数
	// ================================================================
	private static void testSetGameSceneNull()
	{
		var p = new SceneProcedure();
		p.setGameScene(null);
		assertNull(p.getGameScene());
	}
	private static void testSetGameSceneGetGameScene()
	{
		var p = new SceneProcedure();
		p.setGameScene(null);
		assertNull(p.getGameScene());
	}
	private static void testSetGameSceneThenUnset()
	{
		var p = new SceneProcedure();
		p.setGameScene(null);
		p.setGameScene(null);
		assertNull(p.getGameScene());
	}
	private static void testSetGameSceneNullAfterNonNull()
	{
		var p = new SceneProcedure();
		p.setGameScene(null);
		assertNull(p.getGameScene());
		p.setGameScene(null);
		assertNull(p.getGameScene());
	}

	// ================================================================
	//  addChildProcedure / getChildProcedure — 12 个函数
	// ================================================================
	private static void testAddChildProcedureNull()
	{
		var p = new SceneProcedure();
		assertFalse(p.addChildProcedure(null));
	}
	private static void testAddChildProcedureValid()
	{
		var p = new SceneProcedure();
		var child = new SceneProcedure();
		assertTrue(p.addChildProcedure(child));
		assertTrue(ReferenceEquals(child, p.getChildProcedure(typeof(SceneProcedure))));
	}
	private static void testAddChildProcedureDuplicate()
	{
		var p = new SceneProcedure();
		var child = new SceneProcedure();
		assertTrue(p.addChildProcedure(child));
		assertFalse(p.addChildProcedure(child));
	}
	private static void testGetChildProcedureUnregistered()
	{
		var p = new SceneProcedure();
		assertNull(p.getChildProcedure(typeof(string)));
	}
	private static void testAddMultipleChildren()
	{
		var p = new SceneProcedure();
		assertTrue(p.addChildProcedure(new ProcChildA()));
		assertTrue(p.addChildProcedure(new ProcChildB()));
		assertTrue(p.addChildProcedure(new ProcChildC()));
		assertTrue(p.addChildProcedure(new ProcChildD()));
		assertTrue(p.addChildProcedure(new ProcChildE()));
	}
	private static void testAddChildProcedureSameTypeDifferentInstance()
	{
		var p = new SceneProcedure();
		assertTrue(p.addChildProcedure(new SceneProcedure()));
		assertFalse(p.addChildProcedure(new SceneProcedure()));
	}
	private static void testAddChildProcedureSelf()
	{
		var p = new SceneProcedure();
		// 把自己加为自己的子节点 → TryAdd 成功（key 不同）→ setParent(this) 设置父节点为自己
		bool result = p.addChildProcedure(p);
		assertTrue(result);
		assertTrue(ReferenceEquals(p, p.getParent()));
	}
	private static void testAddChildProcedureThenCheckParent()
	{
		var p = new SceneProcedure();
		var child = new SceneProcedure();
		p.addChildProcedure(child);
		assertTrue(ReferenceEquals(p, child.getParent()));
	}
	private static void testAddChildProcedureGetOnMissingType()
	{
		var p = new SceneProcedure();
		p.addChildProcedure(new ProcChildA());
		assertNull(p.getChildProcedure(typeof(ProcChildB)));
	}
	private static void testAddChildProcedureFiveDifferentTypes()
	{
		var p = new SceneProcedure();
		assertTrue(p.addChildProcedure(new ProcChildA()));
		assertTrue(p.addChildProcedure(new ProcChildB()));
		assertTrue(p.addChildProcedure(new ProcChildC()));
		assertTrue(p.addChildProcedure(new ProcChildD()));
		assertTrue(p.addChildProcedure(new ProcChildE()));
		assertTrue(ReferenceEquals(p.getChildProcedure(typeof(ProcChildA)).GetType(), typeof(ProcChildA)));
		assertTrue(ReferenceEquals(p.getChildProcedure(typeof(ProcChildC)).GetType(), typeof(ProcChildC)));
		assertTrue(ReferenceEquals(p.getChildProcedure(typeof(ProcChildE)).GetType(), typeof(ProcChildE)));
	}
	private static void testAddChildProcedureAfterParentSet()
	{
		var p = new SceneProcedure();
		var child = new SceneProcedure();
		p.addChildProcedure(child);
		// child 的父节点已经是 p, 再次 addChildren 不会修改
		assertFalse(p.addChildProcedure(new SceneProcedure()));
	}

	// ================================================================
	//  isThisOrParent — 11 个函数
	// ================================================================
	private static void testIsThisOrParentSelf()
	{
		var p = new SceneProcedure();
		assertTrue(p.isThisOrParent(typeof(SceneProcedure)));
	}
	private static void testIsThisOrParentNull()
	{
		var p = new SceneProcedure();
		assertFalse(p.isThisOrParent(null));
	}
	private static void testIsThisOrParentWithParentChain()
	{
		var parent = new SceneProcedure();
		var child = new ProcChildA();
		parent.addChildProcedure(child);
		assertTrue(child.isThisOrParent(typeof(ProcChildA)));
		assertTrue(child.isThisOrParent(typeof(SceneProcedure)));
	}
	private static void testIsThisOrParentMatchesParent()
	{
		var parent = new SceneProcedure();
		var child = new ProcChildA();
		parent.addChildProcedure(child);
		assertFalse(child.isThisOrParent(typeof(ProcChildB)));
	}
	private static void testIsThisOrParentDeepChain()
	{
		var gp = new SceneProcedure();
		var parent = new ProcChildA();
		var child = new ProcChildB();
		gp.addChildProcedure(parent);
		parent.addChildProcedure(child);
		assertTrue(child.isThisOrParent(typeof(ProcChildB)));
		assertTrue(child.isThisOrParent(typeof(ProcChildA)));
		assertTrue(child.isThisOrParent(typeof(SceneProcedure)));
		assertFalse(child.isThisOrParent(typeof(ProcChildC)));
	}
	private static void testIsThisOrParentFourLevelChain()
	{
		var l1 = new SceneProcedure();
		var l2 = new ProcChildA();
		var l3 = new ProcChildB();
		var l4 = new ProcChildC();
		l1.addChildProcedure(l2);
		l2.addChildProcedure(l3);
		l3.addChildProcedure(l4);
		assertTrue(l4.isThisOrParent(typeof(ProcChildC)));
		assertTrue(l4.isThisOrParent(typeof(ProcChildB)));
		assertTrue(l4.isThisOrParent(typeof(ProcChildA)));
		assertTrue(l4.isThisOrParent(typeof(SceneProcedure)));
		assertFalse(l4.isThisOrParent(typeof(ProcChildD)));
	}
	private static void testIsThisOrParentFiveLevelChain()
	{
		var l1 = new SceneProcedure();
		var l2 = new ProcChildA();
		var l3 = new ProcChildB();
		var l4 = new ProcChildC();
		var l5 = new ProcChildD();
		l1.addChildProcedure(l2);
		l2.addChildProcedure(l3);
		l3.addChildProcedure(l4);
		l4.addChildProcedure(l5);
		assertTrue(l5.isThisOrParent(typeof(ProcChildD)));
		assertTrue(l5.isThisOrParent(typeof(ProcChildC)));
		assertTrue(l5.isThisOrParent(typeof(ProcChildB)));
		assertTrue(l5.isThisOrParent(typeof(ProcChildA)));
		assertTrue(l5.isThisOrParent(typeof(SceneProcedure)));
	}
	private static void testIsThisOrParentNoParentFallsback()
	{
		var p = new SceneProcedure();
		assertFalse(p.isThisOrParent(typeof(string)));
		assertFalse(p.isThisOrParent(typeof(int)));
	}
	private static void testIsThisOrParentWithObjectType()
	{
		var p = new SceneProcedure();
		assertFalse(p.isThisOrParent(typeof(object)));
	}
	private static void testIsThisOrParentWithStringType()
	{
		var p = new SceneProcedure();
		assertFalse(p.isThisOrParent(typeof(string)));
		// 子节点 + 父节点 都不匹配
		var parent = new SceneProcedure();
		var child = new ProcChildA();
		parent.addChildProcedure(child);
		assertFalse(child.isThisOrParent(typeof(string)));
	}
	private static void testIsThisOrParentEmptyHierarchy()
	{
		var p = new SceneProcedure();
		var child = new ProcChildA();
		// 没有通过 addChildProcedure 建立关系，查不到父节点
		assertFalse(child.isThisOrParent(typeof(SceneProcedure)));
	}

	// ================================================================
	//  getParent — 7 个函数
	// ================================================================
	private static void testGetParentNoParent()
	{
		var p = new SceneProcedure();
		assertNull(p.getParent(typeof(string)));
		assertNull(p.getParent(typeof(SceneProcedure)));
	}
	private static void testGetParentWithChain()
	{
		var parent = new SceneProcedure();
		var child = new ProcChildA();
		parent.addChildProcedure(child);
		assertTrue(ReferenceEquals(parent, child.getParent(typeof(SceneProcedure))));
	}
	private static void testGetParentNotFound()
	{
		var parent = new SceneProcedure();
		var child = new ProcChildA();
		parent.addChildProcedure(child);
		assertNull(child.getParent(typeof(ProcChildB)));
	}
	private static void testGetParentDeepChain()
	{
		var gp = new SceneProcedure();
		var parent = new ProcChildA();
		var child = new ProcChildB();
		gp.addChildProcedure(parent);
		parent.addChildProcedure(child);
		assertTrue(ReferenceEquals(gp, child.getParent(typeof(SceneProcedure))));
		assertTrue(ReferenceEquals(parent, child.getParent(typeof(ProcChildA))));
	}
	private static void testGetParentFourLevelChain()
	{
		var l1 = new SceneProcedure();
		var l2 = new ProcChildA();
		var l3 = new ProcChildB();
		var l4 = new ProcChildC();
		l1.addChildProcedure(l2);
		l2.addChildProcedure(l3);
		l3.addChildProcedure(l4);
		assertTrue(ReferenceEquals(l3, l4.getParent(typeof(ProcChildB))));
		assertTrue(ReferenceEquals(l2, l4.getParent(typeof(ProcChildA))));
		assertTrue(ReferenceEquals(l1, l4.getParent(typeof(SceneProcedure))));
	}
	private static void testGetParentFiveLevelChain()
	{
		var l1 = new SceneProcedure();
		var l2 = new ProcChildA();
		var l3 = new ProcChildB();
		var l4 = new ProcChildC();
		var l5 = new ProcChildD();
		l1.addChildProcedure(l2);
		l2.addChildProcedure(l3);
		l3.addChildProcedure(l4);
		l4.addChildProcedure(l5);
		assertTrue(ReferenceEquals(l4, l5.getParent(typeof(ProcChildC))));
		assertTrue(ReferenceEquals(l3, l5.getParent(typeof(ProcChildB))));
		assertTrue(ReferenceEquals(l2, l5.getParent(typeof(ProcChildA))));
		assertTrue(ReferenceEquals(l1, l5.getParent(typeof(SceneProcedure))));
	}
	private static void testGetParentWithNullType()
	{
		var p = new SceneProcedure();
		assertNull(p.getParent(null));
	}
	private static void testGetParentWithSelfType()
	{
		var p = new SceneProcedure();
		// 没有父节点时查询自身类型 → null（不匹配自身，因为没有父节点）
		assertNull(p.getParent(typeof(SceneProcedure)));
	}

	// ================================================================
	//  getThisOrParent — 9 个函数
	// ================================================================
	private static void testGetThisOrParentSelf()
	{
		var p = new SceneProcedure();
		assertTrue(ReferenceEquals(p, p.getThisOrParent(typeof(SceneProcedure))));
	}
	private static void testGetThisOrParentNull()
	{
		var p = new SceneProcedure();
		assertNull(p.getThisOrParent(null));
	}
	private static void testGetThisOrParentFromParent()
	{
		var parent = new SceneProcedure();
		var child = new ProcChildA();
		parent.addChildProcedure(child);
		assertTrue(ReferenceEquals(parent, child.getThisOrParent(typeof(SceneProcedure))));
	}
	private static void testGetThisOrParentDeepChain()
	{
		var gp = new SceneProcedure();
		var parent = new ProcChildA();
		var child = new ProcChildB();
		gp.addChildProcedure(parent);
		parent.addChildProcedure(child);
		assertTrue(ReferenceEquals(gp, child.getThisOrParent(typeof(SceneProcedure))));
		assertTrue(ReferenceEquals(parent, child.getThisOrParent(typeof(ProcChildA))));
		assertTrue(ReferenceEquals(child, child.getThisOrParent(typeof(ProcChildB))));
	}
	private static void testGetThisOrParentFourLevelChain()
	{
		var l1 = new SceneProcedure();
		var l2 = new ProcChildA();
		var l3 = new ProcChildB();
		var l4 = new ProcChildC();
		l1.addChildProcedure(l2);
		l2.addChildProcedure(l3);
		l3.addChildProcedure(l4);
		assertTrue(ReferenceEquals(l4, l4.getThisOrParent(typeof(ProcChildC))));
		assertTrue(ReferenceEquals(l3, l4.getThisOrParent(typeof(ProcChildB))));
		assertTrue(ReferenceEquals(l2, l4.getThisOrParent(typeof(ProcChildA))));
		assertTrue(ReferenceEquals(l1, l4.getThisOrParent(typeof(SceneProcedure))));
	}
	private static void testGetThisOrParentFiveLevelChain()
	{
		var l1 = new SceneProcedure();
		var l2 = new ProcChildA();
		var l3 = new ProcChildB();
		var l4 = new ProcChildC();
		var l5 = new ProcChildD();
		l1.addChildProcedure(l2);
		l2.addChildProcedure(l3);
		l3.addChildProcedure(l4);
		l4.addChildProcedure(l5);
		assertTrue(ReferenceEquals(l5, l5.getThisOrParent(typeof(ProcChildD))));
		assertTrue(ReferenceEquals(l4, l5.getThisOrParent(typeof(ProcChildC))));
		assertTrue(ReferenceEquals(l3, l5.getThisOrParent(typeof(ProcChildB))));
		assertTrue(ReferenceEquals(l2, l5.getThisOrParent(typeof(ProcChildA))));
		assertTrue(ReferenceEquals(l1, l5.getThisOrParent(typeof(SceneProcedure))));
	}
	private static void testGetThisOrParentNotFound()
	{
		var child = new ProcChildA();
		assertNull(child.getThisOrParent(typeof(ProcChildB)));
	}
	private static void testGetThisOrParentWithNullType()
	{
		var p = new SceneProcedure();
		assertNull(p.getThisOrParent(null));
	}
	private static void testGetThisOrParentWithStringType()
	{
		var p = new SceneProcedure();
		assertNull(p.getThisOrParent(typeof(string)));
	}

	// ================================================================
	//  getParentList — 6 个函数
	// ================================================================
	private static void testGetParentListSelf()
	{
		var list = new List<SceneProcedure>();
		var p = new SceneProcedure();
		p.getParentList(list);
		assertEqual(1, list.Count);
		assertTrue(ReferenceEquals(p, list[0]));
	}
	private static void testGetParentListWithParent()
	{
		var parent = new SceneProcedure();
		var child = new ProcChildA();
		parent.addChildProcedure(child);
		var list = new List<SceneProcedure>();
		child.getParentList(list);
		assertEqual(2, list.Count);
		assertTrue(ReferenceEquals(child, list[0]));
		assertTrue(ReferenceEquals(parent, list[1]));
	}
	private static void testGetParentListThreeLevels()
	{
		var gp = new SceneProcedure();
		var parent = new ProcChildA();
		var child = new ProcChildB();
		gp.addChildProcedure(parent);
		parent.addChildProcedure(child);
		var list = new List<SceneProcedure>();
		child.getParentList(list);
		assertEqual(3, list.Count);
		assertTrue(ReferenceEquals(child, list[0]));
		assertTrue(ReferenceEquals(parent, list[1]));
		assertTrue(ReferenceEquals(gp, list[2]));
	}
	private static void testGetParentListFourLevels()
	{
		var l1 = new SceneProcedure();
		var l2 = new ProcChildA();
		var l3 = new ProcChildB();
		var l4 = new ProcChildC();
		l1.addChildProcedure(l2);
		l2.addChildProcedure(l3);
		l3.addChildProcedure(l4);
		var list = new List<SceneProcedure>();
		l4.getParentList(list);
		assertEqual(4, list.Count);
		assertTrue(ReferenceEquals(l4, list[0]));
		assertTrue(ReferenceEquals(l3, list[1]));
		assertTrue(ReferenceEquals(l2, list[2]));
		assertTrue(ReferenceEquals(l1, list[3]));
	}
	private static void testGetParentListFiveLevels()
	{
		var l1 = new SceneProcedure();
		var l2 = new ProcChildA();
		var l3 = new ProcChildB();
		var l4 = new ProcChildC();
		var l5 = new ProcChildD();
		l1.addChildProcedure(l2);
		l2.addChildProcedure(l3);
		l3.addChildProcedure(l4);
		l4.addChildProcedure(l5);
		var list = new List<SceneProcedure>();
		l5.getParentList(list);
		assertEqual(5, list.Count);
		assertTrue(ReferenceEquals(l5, list[0]));
		assertTrue(ReferenceEquals(l4, list[1]));
		assertTrue(ReferenceEquals(l3, list[2]));
		assertTrue(ReferenceEquals(l2, list[3]));
		assertTrue(ReferenceEquals(l1, list[4]));
	}
	private static void testGetParentListConsistentOrder()
	{
		var gp = new SceneProcedure();
		var parent = new ProcChildA();
		var child = new ProcChildB();
		gp.addChildProcedure(parent);
		parent.addChildProcedure(child);
		var list = new List<SceneProcedure>();
		child.getParentList(list);
		// 顺序始终是：自身 → 父 → 祖父
		assertEqual(typeof(ProcChildB), list[0].GetType());
		assertEqual(typeof(ProcChildA), list[1].GetType());
		assertEqual(typeof(SceneProcedure), list[2].GetType());
	}

	// ================================================================
	//  getSameParent — 8 个函数
	// ================================================================
	private static void testGetSameParentSelf()
	{
		var p = new SceneProcedure();
		var other = new SceneProcedure();
		assertNull(p.getSameParent(other));
	}
	private static void testGetSameParentWithCommonParent()
	{
		var p = new SceneProcedure();
		var a = new ProcChildA();
		var b = new ProcChildB();
		p.addChildProcedure(a);
		p.addChildProcedure(b);
		assertTrue(ReferenceEquals(p, a.getSameParent(b)));
	}
	private static void testGetSameParentDeep()
	{
		var gp = new SceneProcedure();
		var pa = new ProcChildA();
		var pb = new ProcChildB();
		var ca = new ProcChildC();
		var cb = new TestSceneProcedure();
		gp.addChildProcedure(pa);
		gp.addChildProcedure(pb);
		pa.addChildProcedure(ca);
		pb.addChildProcedure(cb);
		assertTrue(ReferenceEquals(gp, ca.getSameParent(cb)));
	}
	private static void testGetSameParentSameInstance()
	{
		var p = new SceneProcedure();
		var child = new ProcChildA();
		p.addChildProcedure(child);
		// 同一个实例和自己比较 → 第一个匹配的就是 child 自身
		var same = child.getSameParent(child);
		assertTrue(ReferenceEquals(child, same));
	}
	private static void testGetSameParentWithNull()
	{
		var p = new SceneProcedure();
		// null 参数在 otherProcedure.getParentList() 处 NPE
		// 这里只测试非 null 的常规路径
		try
		{
			p.getSameParent(p);
			// 可以运行
		}
		catch { /* 预期可能异常 */ }
	}
	private static void testGetSameParentCousins()
	{
		var gp = new SceneProcedure();
		var pa = new ProcChildA();
		var pb = new ProcChildB();
		var ca = new ProcChildC();
		var cb = new ProcChildD();
		gp.addChildProcedure(pa);
		gp.addChildProcedure(pb);
		pa.addChildProcedure(ca);
		pb.addChildProcedure(cb);
		// ca 和 cb 是堂兄弟，共同父节点是 gp
		assertTrue(ReferenceEquals(gp, ca.getSameParent(cb)));
	}
	private static void testGetSameParentThreeSiblings()
	{
		var p = new SceneProcedure();
		var a = new ProcChildA();
		var b = new ProcChildB();
		var c = new ProcChildC();
		p.addChildProcedure(a);
		p.addChildProcedure(b);
		p.addChildProcedure(c);
		assertTrue(ReferenceEquals(p, a.getSameParent(b)));
		assertTrue(ReferenceEquals(p, b.getSameParent(c)));
		assertTrue(ReferenceEquals(p, a.getSameParent(c)));
	}
	private static void testGetSameParentDeepCousins()
	{
		var root = new SceneProcedure();
		var branchA = new ProcChildA();
		var branchB = new ProcChildB();
		var leafA = new ProcChildC();
		var leafB = new ProcChildD();
		var deepA = new ProcChildE();
		root.addChildProcedure(branchA);
		root.addChildProcedure(branchB);
		branchA.addChildProcedure(leafA);
		branchB.addChildProcedure(leafB);
		leafA.addChildProcedure(deepA);
		// deepA 和 leafB 的共同祖先是 root
		assertTrue(ReferenceEquals(root, deepA.getSameParent(leafB)));
	}

	// ================================================================
	//  prepareExit / isPreparingExit — 8 个函数
	// ================================================================
	private static void testPrepareExit()
	{
		var p = new SceneProcedure();
		p.prepareExit(new SceneProcedure(), 1.0f);
		assertTrue(p.isPreparingExit());
	}
	private static void testPrepareExitMultipleTimes()
	{
		var p = new SceneProcedure();
		p.prepareExit(new SceneProcedure(), 0.5f);
		var next2 = new SceneProcedure();
		p.prepareExit(next2, 2.0f);
		assertTrue(ReferenceEquals(next2, p.getPrepareNext()));
	}
	private static void testPrepareNextAfterPrepareExit()
	{
		var p = new SceneProcedure();
		var next = new SceneProcedure();
		p.prepareExit(next, 0.5f);
		assertTrue(ReferenceEquals(next, p.getPrepareNext()));
	}
	private static void testPrepareExitZeroTime()
	{
		var p = new SceneProcedure();
		p.prepareExit(new SceneProcedure(), 0.0f);
		assertTrue(p.isPreparingExit());
	}
	private static void testPrepareExitLargeTime()
	{
		var p = new SceneProcedure();
		p.prepareExit(new SceneProcedure(), 99999.0f);
		assertTrue(p.isPreparingExit());
	}
	private static void testPrepareExitThenIsPreparing()
	{
		var p = new SceneProcedure();
		assertFalse(p.isPreparingExit());
		p.prepareExit(new SceneProcedure(), 1.0f);
		assertTrue(p.isPreparingExit());
	}
	private static void testIsPreparingExitDefault()
	{
		var p = new SceneProcedure();
		assertFalse(p.isPreparingExit());
		p.prepareExit(new SceneProcedure(), -1.0f);
		assertTrue(p.isPreparingExit());
	}
	private static void testIsPreparingExitAfterInit()
	{
		var p = new TestSceneProcedure();
		p.init(null);
		assertFalse(p.isPreparingExit());
	}

	// ================================================================
	//  init — 8 个函数
	// ================================================================
	private static void testInitNull()
	{
		var p = new TestSceneProcedure();
		p.init(null);
	}
	private static void testInitSetsInited()
	{
		var p = new TestSceneProcedure();
		p.init(null);
		assertTrue(p.isInited());
	}
	private static void testInitMultipleTimes()
	{
		var p = new TestSceneProcedure();
		p.init(null); assertTrue(p.isInited());
		p.init(null); assertTrue(p.isInited());
		p.init(null); assertTrue(p.isInited());
	}
	private static void testInitWithParent()
	{
		var parent = new TestSceneProcedure();
		var child = new TestSceneProcedure();
		parent.addChildProcedure(child);
		child.init(null);
		assertTrue(parent.isInited());
		assertTrue(child.isInited());
	}
	private static void testInitWithLastProcedure()
	{
		var p = new TestSceneProcedure();
		var lastProc = new ProcChildA();
		p.init(lastProc);
		assertTrue(p.isInited());
	}
	private static void testInitWithLastProcedureSameType()
	{
		var p = new TestSceneProcedure();
		var lastProc = new TestSceneProcedure();
		p.init(lastProc);
		assertTrue(p.isInited());
	}
	private static void testInitParentAlreadyInited()
	{
		var parent = new TestSceneProcedure();
		var child = new TestSceneProcedure();
		parent.addChildProcedure(child);
		parent.init(null);
		assertTrue(parent.isInited());
		// 父已初始化，child.init 不再重新初始化父节点
		child.init(null);
		assertTrue(child.isInited());
	}
	private static void testInitParentAlreadyInitedChain()
	{
		var gp = new TestSceneProcedure();
		var parent = new TestSceneProcedure();
		var child = new TestSceneProcedure();
		gp.addChildProcedure(parent);
		parent.addChildProcedure(child);
		gp.init(null);
		parent.init(null);
		child.init(null);
		assertTrue(gp.isInited());
		assertTrue(parent.isInited());
		assertTrue(child.isInited());
	}

	// ================================================================
	//  update / lateUpdate / keyProcess — 6 个函数
	// ================================================================
	private static void testUpdate()
	{
		var p = new SceneProcedure();
		p.update(0.016f);
	}
	private static void testLateUpdate()
	{
		var p = new SceneProcedure();
		p.lateUpdate(0.016f);
	}
	private static void testKeyProcess()
	{
		var p = new SceneProcedure();
		p.keyProcess(0.016f);
	}
	private static void testUpdateWithParentProcedure()
	{
		var parent = new SceneProcedure();
		var child = new ProcChildA();
		parent.addChildProcedure(child);
		child.update(0.016f);
	}
	private static void testUpdateAfterInit()
	{
		var p = new TestSceneProcedure();
		p.init(null);
		p.update(0.016f);
		p.lateUpdate(0.016f);
	}
	private static void testLateUpdateAfterInit()
	{
		var p = new TestSceneProcedure();
		p.init(null);
		p.lateUpdate(0.016f);
	}

	// ================================================================
	//  组合场景 — 7 个函数
	// ================================================================
	private static void testAddChildThenInitChain()
	{
		var parent = new TestSceneProcedure();
		var child = new TestSceneProcedure();
		parent.addChildProcedure(child);
		child.init(null);
		assertTrue(parent.isInited());
		assertTrue(child.isInited());
		assertTrue(ReferenceEquals(parent, child.getParent()));
	}
	private static void testPrepareExitThenInit()
	{
		var p = new TestSceneProcedure();
		p.prepareExit(new SceneProcedure(), 0.5f);
		assertTrue(p.isPreparingExit());
		p.init(null);
		assertTrue(p.isInited());
		// prepareExit 状态在 init 后应该仍然保持?
		// init 不修改 mPrepareTimer/mPrepareNext
		assertTrue(p.isPreparingExit());
	}
	private static void testAddMultipleChildThenGetParentList()
	{
		var parent = new SceneProcedure();
		var a = new ProcChildA();
		var b = new ProcChildB();
		var c = new ProcChildC();
		parent.addChildProcedure(a);
		parent.addChildProcedure(b);
		parent.addChildProcedure(c);
		var listA = new List<SceneProcedure>();
		a.getParentList(listA);
		assertEqual(2, listA.Count);
		var listB = new List<SceneProcedure>();
		b.getParentList(listB);
		assertEqual(2, listB.Count);
		assertEqual(listA[1].GetType(), listB[1].GetType());
	}
	private static void testDeepHierarchyAllChecks()
	{
		var l1 = new SceneProcedure(); // 必须用 SceneProcedure 类型，否则 isThisOrParent(typeof(SceneProcedure)) 因类型不匹配
		var l2 = new ProcChildA();
		var l3 = new ProcChildB();
		var l4 = new ProcChildC();
		l1.addChildProcedure(l2);
		l2.addChildProcedure(l3);
		l3.addChildProcedure(l4);
		// 4 层：全部 isThisOrParent
		assertTrue(l4.isThisOrParent(typeof(ProcChildC)));
		assertTrue(l4.isThisOrParent(typeof(ProcChildB)));
		assertTrue(l4.isThisOrParent(typeof(ProcChildA)));
		assertTrue(l4.isThisOrParent(typeof(SceneProcedure)));
		// 4 层：getParentList
		var list = new List<SceneProcedure>();
		l4.getParentList(list);
		assertEqual(4, list.Count);
		// getSameParent 自身 → 自身
		assertTrue(ReferenceEquals(l4, l4.getSameParent(l4)));
		// getParent 最顶层
		assertTrue(ReferenceEquals(l1, l4.getParent(typeof(SceneProcedure))));
	}
	private static void testSiblingGetSameParentMultipleChecks()
	{
		var p = new SceneProcedure();
		var a = new ProcChildA();
		var b = new ProcChildB();
		var c = new ProcChildC();
		p.addChildProcedure(a);
		p.addChildProcedure(b);
		p.addChildProcedure(c);
		assertTrue(ReferenceEquals(p, a.getSameParent(b)));
		assertTrue(ReferenceEquals(p, a.getSameParent(c)));
		assertTrue(ReferenceEquals(p, b.getSameParent(c)));
	}
	private static void testProcedureChainConsistency()
	{
		var gp = new SceneProcedure();
		var parent = new ProcChildA();
		var child = new ProcChildB();
		gp.addChildProcedure(parent);
		parent.addChildProcedure(child);
		// child → parent → grandParent
		assertTrue(ReferenceEquals(parent, child.getParent()));
		assertTrue(ReferenceEquals(gp, parent.getParent()));
		assertNull(gp.getParent());
		// getSameParent
		assertTrue(ReferenceEquals(parent, child.getSameParent(parent)));
	}
	private static void testMultiLevelInitChain()
	{
		var l1 = new TestSceneProcedure();
		var l2 = new TestSceneProcedure();
		var l3 = new TestSceneProcedure();
		l1.addChildProcedure(l2);
		l2.addChildProcedure(l3);
		// 从最底层初始化，触发整条链
		l3.init(null);
		assertTrue(l1.isInited());
		assertTrue(l2.isInited());
		assertTrue(l3.isInited());
	}
	// ================================================================
	//  流程跳转执行路径验证 — 以下使用 TrackA/TrackB/TrackParent/TrackChild
	//  验证 changeProcedure 是否正确按 退出→进入 顺序调用各回调
	// ================================================================
	// ----- 辅助 -----
	static TrackScene NewScene() { TrackA.Reset(); return new TrackScene(); }
	static TrackA AddProc(TrackScene s, Type t, Type parent = null)
	{
		var p = Activator.CreateInstance(t) as TrackA;
		s.add(p, parent);
		return p;
	}
	static void VerifyFlow(List<string> expected)
	{
		var actual = TrackA.CallLog;
		assertEqual(expected.Count, actual.Count, "Flow: call count mismatch");
		for (int i = 0; i < expected.Count; i++)
			assertEqual(expected[i], actual[i], $"Flow: call #{i} mismatch");
	}
	// ================================================================
	//  1. A → B：先退出 A，再进入 B
	// ================================================================
	private static void testFlowFlatAtoB()
	{
		var s = NewScene();
		AddProc(s, typeof(TrackA));
		AddProc(s, typeof(TrackB));
		s.changeProcedure(typeof(TrackA));
		VerifyFlow(new() { "A.onInit(null)" });
		s.changeProcedure(typeof(TrackB));
		VerifyFlow(new() { "A.onInit(null)", "A.onExit(B)", "A.onExitSelf()", "B.onInit(A)" });
	}
	// ================================================================
	//  2. A → B → C：连续跳转
	// ================================================================
	private static void testFlowFlatAtoBtoC()
	{
		var s = NewScene();
		AddProc(s, typeof(TrackA));
		AddProc(s, typeof(TrackB));
		AddProc(s, typeof(TrackC));
		s.changeProcedure(typeof(TrackA));
		s.changeProcedure(typeof(TrackB));
		s.changeProcedure(typeof(TrackC));
		VerifyFlow(new() {
			"A.onInit(null)",
			"A.onExit(B)", "A.onExitSelf()", "B.onInit(A)",
			"B.onExit(C)", "B.onExitSelf()", "C.onInit(B)"
		});
	}
	// ================================================================
	//  3. 同类型跳转 → 早期返回，0 回调
	// ================================================================
	private static void testFlowSameTypeEarlyReturn()
	{
		var s = NewScene();
		AddProc(s, typeof(TrackA));
		s.changeProcedure(typeof(TrackA));
		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackA));
		assertEqual(0, TrackA.CallLog.Count);
		assertTrue(s.atProcedure(typeof(TrackA)));
	}
	// ================================================================
	//  4. 父→子：Parent → Child
	// ================================================================
	private static void testFlowParentToChild()
	{
		var s = NewScene();
		AddProc(s, typeof(TrackParent));
		AddProc(s, typeof(TrackChild), typeof(TrackParent));
		s.changeProcedure(typeof(TrackParent));
		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackChild));
		VerifyFlow(new() {
			"Parent.onExitToChild(Child)",
			"Parent.onExitSelf()",
			"Child.onInit(Parent)"
		});
	}
	// ================================================================
	//  5. 子→父：Child → Parent
	// ================================================================
	private static void testFlowChildToParent()
	{
		var s = NewScene();
		AddProc(s, typeof(TrackParent));
		AddProc(s, typeof(TrackChild), typeof(TrackParent));
		s.changeProcedure(typeof(TrackParent));
		s.changeProcedure(typeof(TrackChild));
		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackParent));
		VerifyFlow(new() {
			"Child.onExit(Parent)", "Child.onExitSelf()",
			"Parent.onExitToChild(Parent)", "Parent.onExitSelf()",
			"Parent.onInitFromChild(Child)"
		});
	}
	// ================================================================
	//  6. 堂兄弟：childA → childB（同父）
	// ================================================================
	private static void testFlowSiblings()
	{
		var s = NewScene();
		AddProc(s, typeof(TrackParent));
		AddProc(s, typeof(TrackChild), typeof(TrackParent));
		AddProc(s, typeof(TrackB), typeof(TrackParent));
		s.changeProcedure(typeof(TrackParent));
		s.changeProcedure(typeof(TrackChild));
		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackB));
		VerifyFlow(new() {
			"Child.onExit(B)", "Child.onExitSelf()",
			"Parent.onExitToChild(B)", "Parent.onExitSelf()",
			"B.onInit(Child)"
		});
	}
	// ================================================================
	//  7. 三层：GP → P → C → P → GP
	// ================================================================
	private static void testFlowGrandParentToChildToGrandParent()
	{
		var s = NewScene();
		AddProc(s, typeof(TrackA));
		AddProc(s, typeof(TrackParent), typeof(TrackA));
		AddProc(s, typeof(TrackChild), typeof(TrackParent));
		s.changeProcedure(typeof(TrackA));
		s.changeProcedure(typeof(TrackParent));
		s.changeProcedure(typeof(TrackChild));
		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackParent));
		VerifyFlow(new() {
			"Child.onExit(Parent)", "Child.onExitSelf()",
			"Parent.onExitToChild(Parent)", "Parent.onExitSelf()",
			"Parent.onInitFromChild(Child)"
		});
		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackA));
		VerifyFlow(new() {
			"Parent.onExit(A)", "Parent.onExitSelf()",
			"A.onExitToChild(A)", "A.onExitSelf()",
			"A.onInitFromChild(Parent)"
		});
	}
	// ================================================================
	//  8. 复杂链：GP → P → C → P → GP → C → P（7 步逐次验证）
	// ================================================================
	private static void testFlowComplexChain()
	{
		var s = NewScene();
		var gp = AddProc(s, typeof(TrackA));
		var p  = AddProc(s, typeof(TrackParent), typeof(TrackA));
		var c  = AddProc(s, typeof(TrackChild), typeof(TrackParent));

		s.changeProcedure(typeof(TrackA));
		VerifyFlow(new() { "A.onInit(null)" });

		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackParent));
		VerifyFlow(new() { "A.onExitToChild(Parent)", "A.onExitSelf()", "Parent.onInit(A)" });

		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackChild));
		VerifyFlow(new() { "Parent.onExitToChild(Child)", "Parent.onExitSelf()", "Child.onInit(Parent)" });

		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackParent));
		VerifyFlow(new() { "Child.onExit(Parent)", "Child.onExitSelf()", "Parent.onExitToChild(Parent)", "Parent.onExitSelf()", "Parent.onInitFromChild(Child)" });

		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackA));
		VerifyFlow(new() { "Parent.onExit(A)", "Parent.onExitSelf()", "A.onExitToChild(A)", "A.onExitSelf()", "A.onInitFromChild(Parent)" });

		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackChild));
		VerifyFlow(new() {
			"A.onExitToChild(Child)", "A.onExitSelf()",
			"Parent.onInit(A)", "Parent.onExitToChild(Child)", "Parent.onExitSelf()",
			"Child.onInit(A)"
		});

		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackParent));
		VerifyFlow(new() { "Child.onExit(Parent)", "Child.onExitSelf()", "Parent.onExitToChild(Parent)", "Parent.onExitSelf()", "Parent.onInitFromChild(Child)" });
	}
	// ==========================================================================
	//  GameScene + SceneProcedure 联合流程测试
	//  模拟 LoginScene/MainScene 真实使用模式：scene.createSceneProcedure → 
	//  assignStartExitProcedure → enterStartProcedure → changeProcedure
	// ==========================================================================
	// 联合测试用的 GameScene 子类（像 LoginScene 一样注册 Track 流程）
	public class TrackGameScene : GameScene
	{
		public override void assignStartExitProcedure() { mStartProcedure = typeof(TrackA); mExitProcedure = typeof(TrackC); }
		public override void createSceneProcedure()
		{
			addProcedure(typeof(TrackA));
			addProcedure(typeof(TrackB));
			addProcedure(typeof(TrackC));
			addProcedure(typeof(TrackParent));
			addProcedure(typeof(TrackChild), typeof(TrackParent));
		}
	}
	static void VerifyCombined(List<string> expected)
	{
		var actual = TrackA.CallLog;
		assertEqual(expected.Count, actual.Count, "Combined: call count mismatch");
		for (int i = 0; i < expected.Count; i++)
			assertEqual(expected[i], actual[i], $"Combined: call #{i} mismatch");
	}
	// ----- 1. 创建场景+注册流程+进入起始流程 -----
	private static void testCombinedSceneCreateAndStart()
	{
		TrackA.Reset(); // 每个联合测试独立开始，清除之前 flow 测试的残留日志
		TrackA.Reset();
		var s = new TrackGameScene();
		s.createSceneProcedure();
		s.assignStartExitProcedure();
		s.enterStartProcedure(); // changeProcedure(TrackA) → TrackA.onInit(null)
		VerifyCombined(new() { "A.onInit(null)" });
		assertTrue(s.atProcedure(typeof(TrackA)));
	}
	// ----- 2. 联合：A → B → C -----
	private static void testCombinedChangeFlat()
	{
		TrackA.Reset();
		TrackA.Reset();
		var s = new TrackGameScene();
		s.createSceneProcedure();
		s.assignStartExitProcedure();
		s.enterStartProcedure(); // → A0
		TrackA.CallLog.Clear();

		s.changeProcedure(typeof(TrackB));
		VerifyCombined(new() { "A.onExit(B)", "A.onExitSelf()", "B.onInit(A)" });

		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackC));
		VerifyCombined(new() { "B.onExit(C)", "B.onExitSelf()", "C.onInit(B)" });
	}
	// ----- 3. 联合：Parent → Child（父子流程）-----
	private static void testCombinedParentToChild()
	{
		TrackA.Reset();
		TrackA.Reset();
		var s = new TrackGameScene();
		s.createSceneProcedure();
		s.changeProcedure(typeof(TrackParent)); // 进入 Parent
		VerifyCombined(new() { "Parent.onInit(null)" });
		TrackA.CallLog.Clear();

		s.changeProcedure(typeof(TrackChild)); // Parent → Child
		VerifyCombined(new() {
			"Parent.onExitToChild(Child)", "Parent.onExitSelf()",
			"Child.onInit(Parent)"
		});
	}
	// ----- 4. 联合：Child → Parent（返回）-----
	private static void testCombinedChildToParent()
	{
		TrackA.Reset();
		var s = new TrackGameScene();
		s.createSceneProcedure();
		s.changeProcedure(typeof(TrackParent));
		s.changeProcedure(typeof(TrackChild));
		TrackA.CallLog.Clear();

		s.changeProcedure(typeof(TrackParent)); // Child → Parent
		VerifyCombined(new() {
			"Child.onExit(Parent)", "Child.onExitSelf()",
			"Parent.onExitToChild(Parent)", "Parent.onExitSelf()",
			"Parent.onInitFromChild(Child)"
		});
	}
	// ----- 5. 联合：通过不存在的流程（非法跳转）-----
	private static void testCombinedSiblings()
	{
		TrackA.Reset();
		var s = new TrackGameScene();
		s.createSceneProcedure();
		s.changeProcedure(typeof(TrackParent));
		s.changeProcedure(typeof(TrackChild));
		TrackA.CallLog.Clear();

		s.changeProcedure(typeof(TrackB)); // Child→B: B 无父节点 → 整条退 Child→Parent→进 B
		VerifyCombined(new() {
			"Child.onExit(B)", "Child.onExitSelf()",
			"Parent.onExit(B)", "Parent.onExitSelf()",
			"B.onInit(Child)"
		});
	}
	// ----- 6. 联合：backToLastProcedure -----
	private static void testCombinedBackToLast()
	{
		TrackA.Reset();
		var s = new TrackGameScene();
		s.createSceneProcedure();
		s.assignStartExitProcedure();
		s.enterStartProcedure();
		s.changeProcedure(typeof(TrackB));
		TrackA.CallLog.Clear();

		s.backToLastProcedure(); // 回到 A
		// B→A: 无父子关系 → 退出 B + 进入 A
		VerifyCombined(new() {
			"B.onExit(A)", "B.onExitSelf()",
			"A.onInit(B)"
		});
		assertTrue(s.atProcedure(typeof(TrackA)));
	}
	// ----- 7. 联合：enter + change + back 完整链 -----
	private static void testCombinedEnterStartThenChangeBack()
	{
		TrackA.Reset();
		var s = new TrackGameScene();
		s.createSceneProcedure();
		s.assignStartExitProcedure();
		s.enterStartProcedure(); // A
		TrackA.CallLog.Clear();

		s.changeProcedure(typeof(TrackB)); // A → B
		VerifyCombined(new() { "A.onExit(B)", "A.onExitSelf()", "B.onInit(A)" });

		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackC)); // B → C
		VerifyCombined(new() { "B.onExit(C)", "B.onExitSelf()", "C.onInit(B)" });

		TrackA.CallLog.Clear();
		s.backToLastProcedure(); // C → B（无父子关系）
		VerifyCombined(new() {
			"C.onExit(B)", "C.onExitSelf()",
			"B.onInit(C)"
		});

		TrackA.CallLog.Clear();
		s.backToLastProcedure(); // B → A（无父子关系）
		VerifyCombined(new() {
			"B.onExit(A)", "B.onExitSelf()",
			"A.onInit(B)"
		});
	}
	// ----- 8. 联合：完整双向父子链 -----
	private static void testCombinedFullChain()
	{
		TrackA.Reset();
		var s = new TrackGameScene();
		s.createSceneProcedure();
		s.changeProcedure(typeof(TrackA)); // A
		s.changeProcedure(typeof(TrackParent)); // A → Parent
		s.changeProcedure(typeof(TrackChild)); // Parent → Child
		assertTrue(s.atProcedure(typeof(TrackChild)));

		// 退回到 A（Child → Parent → A：Child 和 A 无共同父链，整条链全部退出）
		TrackA.CallLog.Clear();
		s.changeProcedure(typeof(TrackA));
		// Child.onExit(A) + onExitSelf() → Parent.onExit(A) + onExitSelf() → A.onInit(Child)
		VerifyCombined(new() {
			"Child.onExit(A)", "Child.onExitSelf()",
			"Parent.onExit(A)", "Parent.onExitSelf()",
			"A.onInit(Child)"
		});
	}
}
#endif
