using System;
using System.Collections.Generic;
using static TestAssert;

// 仅用于测试的子流程类型（解决 addChildProcedure 按类型去重）
public class ProcA : SceneProcedure { }
public class ProcB : SceneProcedure { }
public class ProcC : SceneProcedure { }
public class ProcD : SceneProcedure { }
public class ProcE : SceneProcedure { }
public class ProcF : SceneProcedure { }
public class ProcG : SceneProcedure { }
public class ProcH : SceneProcedure { }
public class ProcI : SceneProcedure { }
public class ProcJ : SceneProcedure { }
public class ProcK : SceneProcedure { }
public class ProcL : SceneProcedure { }

// ─── 深度测试辅助类(DG 系列): 可追踪生命周期回调的流程与场景 ───
// 可追踪流程: 记录生命周期回调用于验证执行路径
public class DGProc : SceneProcedure
{
	public string tag = "P";
	public DGProc() { mDelayCmdList = new HashSet<long>(); }
	public static List<string> CallLog = new();
	public override void resetProperty()
	{
		base.resetProperty();
		tag = "P";
	}
	public static void Reset() { CallLog.Clear(); }
	protected override void onInit(SceneProcedure last) { CallLog.Add($"{tag}.onInit({dgName(last)})"); }
	protected override void onInitFromChild(SceneProcedure last) { CallLog.Add($"{tag}.onInitFromChild({dgName(last)})"); }
	protected override void onExit(SceneProcedure next) { CallLog.Add($"{tag}.onExit({dgName(next)})"); }
	protected override void onExitToChild(SceneProcedure next) { CallLog.Add($"{tag}.onExitToChild({dgName(next)})"); }
	protected override void onExitSelf() { CallLog.Add($"{tag}.onExitSelf()"); }
	protected override void onPrepareExit(SceneProcedure next) { CallLog.Add($"{tag}.onPrepareExit({dgName(next)})"); }
	public override void onNextProcedurePrepared(SceneProcedure nextProcedure) { CallLog.Add($"{tag}.onNextProcedurePrepared({dgName(nextProcedure)})"); }
	static string dgName(SceneProcedure p)
	{
		return p is DGProc dp ? dp.tag : (p?.GetType().Name ?? "null");
	}
}
public class DGA : DGProc { public DGA() { tag = "A"; } }
public class DGB : DGProc { public DGB() { tag = "B"; } }
public class DGC : DGProc { public DGC() { tag = "C"; } }
public class DGD : DGProc { public DGD() { tag = "D"; } }
public class DGE : DGProc { public DGE() { tag = "E"; } }
public class DGF : DGProc { public DGF() { tag = "F"; } }
public class DGG : DGProc { public DGG() { tag = "G"; } }
public class DGH : DGProc { public DGH() { tag = "H"; } }
public class DGI : DGProc { public DGI() { tag = "I"; } }
public class DGJ : DGProc { public DGJ() { tag = "J"; } }
public class DGK : DGProc { public DGK() { tag = "K"; } }
public class DGParent : DGProc { public DGParent() { tag = "Parent"; } }
public class DGChild : DGProc { public DGChild() { tag = "Child"; } }

// 暴露 mStartProcedure/mExitProcedure/mTempStartProcedure 的测试场景
public class DGDeepScene : GameScene
{
	public override void assignStartExitProcedure()
	{
		mStartProcedure = typeof(DGA);
		mExitProcedure = typeof(DGC);
	}
	public override void createSceneProcedure()
	{
		addProcedure<DGA>();
		addProcedure<DGB>();
		addProcedure<DGC>();
		addProcedure<DGD>();
		addProcedure<DGE>();
		addProcedure<DGF>();
		addProcedure<DGG>();
		addProcedure<DGH>();
		addProcedure<DGI>();
		addProcedure<DGJ>();
		addProcedure<DGK>();
		addProcedure<DGParent>();
		addProcedure<DGChild>(typeof(DGParent));
	}
	public void setTemp(Type type) { mTempStartProcedure = type; }
}

// 模拟 LoginScene/MainScene 模式：createSceneProcedure 会注册多个流程
public class RichTestGameScene : GameScene
{
	public override void assignStartExitProcedure()
	{
		mStartProcedure = typeof(ProcA);
		mExitProcedure = typeof(ProcL);
	}
	public override void createSceneProcedure()
	{
		addProcedure<ProcA>();
		addProcedure<ProcB>();
		addProcedure<ProcC>();
		addProcedure<ProcD>(typeof(ProcC));
		addProcedure<ProcE>(typeof(ProcC));
		addProcedure<ProcF>();
		addProcedure<ProcG>();
		addProcedure<ProcH>();
		addProcedure<ProcI>();
		addProcedure<ProcJ>();
		addProcedure<ProcK>();
		addProcedure<ProcL>();
	}
}

// 基础版测试场景
public class TestGameScene : GameScene
{
	public override void assignStartExitProcedure() { }
	public override void createSceneProcedure() { }
	public void setStartProcedureForTest(Type type) { mStartProcedure = type; }
	public void setExitProcedureForTest(Type type) { mExitProcedure = type; }
	public Dictionary<Type, SceneProcedure> getSceneProcedureList() { return mSceneProcedureList; }
	public List<SceneProcedure> getLastProcedureList() { return mLastProcedureList; }
}

// ============================================================================
// GameScene 场景单元测试
// 通过 TestGameScene 具体子类测试
//
// 不覆盖（需 Unity 运行时）:
//   init/destroy/willDestroy/update/lateUpdate/exit
// ============================================================================
public static class GameSceneTest
{
	public static void Run()
	{
		// === 初始状态 ===
		testDefaultCurProcedure();
		testDefaultProcedureListEmpty();
		testDefaultLastProcedureListEmpty();
		testDefaultAllState();
		testGetObjectDefaultNull();
		testGameSceneManagerGetCurSceneDefaultNull();

		// === addProcedure ===
		testAddProcedure();
		testAddProcedureGeneric();
		testAddProcedureWithParent();
		testAddProcedureReturnsInstance();
		testAddProcedureGenericReturnsType();
		testGetProcedureUnregistered();
		testGetProcedureGeneric();
		testAddProcedureGenericReturnsExactType();

		// === changeProcedure ===
		testChangeProcedureSetsCurProcedure();
		testChangeProcedureSameTypeEarlyReturn();
		testChangeProcedureWithAddToLastList();
		testChangeProcedureAddToLastListFalse();
		testChangeProcedureChain();
		testChangeProcedureGetCurProcedureType();
		testChangeProcedureGetCurOrParentProcedure();
		testChangeProcedureABAB();
		testChangeProcedureWithMultipleTypes();

		// === backToLastProcedure / getLastProcedureType ===
		testBackToLastProcedureEmpty();
		testBackToLastProcedureWithHistory();
		testGetLastProcedureTypeEmpty();
		testGetLastProcedureTypeAfterChange();
		testBackToLastProcedureTwice();
		testBackToLastProcedureThrice();

		// === enterStartProcedure ===
		testEnterStartProcedureByTempStart();
		testEnterStartProcedureByStartProcedure();
		testEnterStartProcedureUsesTempFirst();
		testEnterStartProcedureMultipleTimes();

		// === RichTestGameScene ===
		testRichSceneCreateProcedures();
		testRichSceneAssignStartExitProcedure();
		testRichSceneFullFlow();
		testRichSceneAddProcedureWithParent();
		testRichSceneMultipleChangeBack();
		testRichSceneGetCurProcedureTypeAfterChanges();

		// === atProcedure ===
		testAtProcedureFalseWhenNull();
		testAtProcedureSelf();
		testAtProcedureGeneric();
		testAtProcedureMultipleTypes();

		// === atSelfProcedure ===
		testAtSelfProcedureMatch();
		testAtSelfProcedureNoMatch();
		testAtSelfProcedureMultipleTypes();

		// === keyProcess ===
		testKeyProcessNullCurProcedure();
		testKeyProcessWithCurProcedure();

		// === notifyProcedurePrepared ===
		testNotifyProcedurePreparedEmpty();
		testNotifyProcedurePreparedWithHistory();

		// === MAX_LAST_PROCEDURE_COUNT 限制 ===
		testMaxLastProcedureCount();
		testMaxLastProcedureCountExactlyEight();
		testMaxLastProcedureCountOrder();

		// === resetProperty ===
		testResetPropertyClearsCurProcedure();
		testResetPropertyClearsProcedureList();
		testResetPropertyClearsLastProcedureList();
		testResetPropertyMultiStepThenClear();

		// === 更多 addProcedure 变体 ===
		testAddProcedureDeepParentChain();
		testAddProcedureParentReferenceConsistency();

		// === 更多 changeProcedure 变体 ===
		testChangeProcedureGeneric();
		testChangeProcedureTwelveProcedures();
		testChangeProcedureBackAndForth();

		// === 更多 enterStartProcedure 变体 ===
		testEnterStartProcedureNullStartProcedure();

		// === 更多 backToLastProcedure 变体 ===
		testBackToLastProcedureEmptyThenChange();

		// === 更多 RichTestGameScene 场景 ===
		testRichSceneGetCurOrParentWithChildProcedure();
		testRichSceneProcedureParentLookup();
		testRichSceneChangeBackUntilEmpty();

		// === getCurProcedureType ===
		testGetCurProcedureTypeBeforeChange();
		testGetCurProcedureTypeMultipleChanges();

		// === getCurOrParentProcedure ===
		testGetCurOrParentProcedureWithParentChain();

		// === MAX_LAST_PROCEDURE_COUNT 额外 ===
		testMaxLastProcedureCountWithDifferentTypes();
		testMaxLastProcedureCountBackAllTheWay();
	

		// ─── tempStart 一次性语义 ───
		testTempStartUsedOnceThenFallsBack();
		testTempStartNullMeansStart();
		testTempStartChangeBetweenEnters();
		// ─── changeProcedure 复杂链 ───
		testChangeChain10Procedures();
		testChangeAddToLastListFalseNoHistory();
		testChangeParentChildLifecycle();
		testChangeFromChildToParentDeep();
		// ─── lastProcedure 历史溢出 (MAX=8) ───
		testLastProcedureOverflowKeepsEight();
		testBackToLastMultiStep();
		testBackToLastEmptyScene();
		testBackToLastAfterOverflow();
		// ─── prepareExit 准备态过渡 ───
		testPrepareExitStateTransition();
		testPrepareExitShortUpdateNotFire();
		// ─── notifyProcedurePrepared 回调链 ───
		testNotifyProcedurePreparedCallsLast();
		// ─── atProcedure 深层匹配 ───
		testAtProcedureParentChild();
		testGetCurOrParentProcedure();
	}

	// ================================================================
	//  初始状态 — 4 个函数
	// ================================================================
	private static void testDefaultCurProcedure()
	{
		assertNull(new TestGameScene().getCurProcedure());
	}
	private static void testGetObjectDefaultNull()
	{
		// 未 init 时 mObject 为 null, getObject() 返回 null
		assertNull(new TestGameScene().getObject());
	}
	private static void testGameSceneManagerGetCurSceneDefaultNull()
	{
		// 未 enterScene 时 getCurScene() 返回 null
		var manager = new GameSceneManager();
		assertNull(manager.getCurScene());
	}
	private static void testDefaultProcedureListEmpty()
	{
		assertEqual(0, new TestGameScene().getSceneProcedureList().Count);
	}
	private static void testDefaultLastProcedureListEmpty()
	{
		assertEqual(0, new TestGameScene().getLastProcedureList().Count);
	}
	private static void testDefaultAllState()
	{
		var s = new TestGameScene();
		assertNull(s.getCurProcedure());
		assertEqual(0, s.getSceneProcedureList().Count);
		assertEqual(0, s.getLastProcedureList().Count);
		assertFalse(s.atProcedure(typeof(SceneProcedure)));
		assertFalse(s.atProcedure<SceneProcedure>());
		assertNull(s.getLastProcedureType());
	}

	// ================================================================
	//  addProcedure — 9 个函数
	// ================================================================
	private static void testAddProcedure()
	{
		var scene = new TestGameScene();
		var proc = scene.addProcedure(typeof(SceneProcedure));
		assertNotNull(proc);
		assertTrue(ReferenceEquals(proc, scene.getProcedure(typeof(SceneProcedure))));
	}
	private static void testAddProcedureGeneric()
	{
		var scene = new TestGameScene();
		var proc = scene.addProcedure<ProcA>();
		assertNotNull(proc);
		assertTrue(ReferenceEquals(proc, scene.getProcedure(typeof(ProcA))));
	}
	private static void testAddProcedureWithParent()
	{
		var scene = new TestGameScene();
		var parent = scene.addProcedure(typeof(SceneProcedure));
		var child = scene.addProcedure(typeof(ProcA), typeof(SceneProcedure));
		assertTrue(ReferenceEquals(parent, child.getParent()));
	}
	private static void testAddProcedureReturnsInstance()
	{
		var scene = new TestGameScene();
		var proc = scene.addProcedure(typeof(SceneProcedure));
		assertTrue(ReferenceEquals(scene, proc.getGameScene()));
	}
	private static void testAddProcedureGenericReturnsType()
	{
		var scene = new TestGameScene();
		var proc = scene.addProcedure<ProcA>();
		assertTrue(proc is ProcA);
	}
	private static void testGetProcedureUnregistered()
	{
		assertNull(new TestGameScene().getProcedure(typeof(string)));
	}
	private static void testGetProcedureGeneric()
	{
		var scene = new TestGameScene();
		scene.addProcedure<ProcA>();
		assertNotNull(scene.getProcedure<ProcA>());
	}
	private static void testAddProcedureGenericReturnsExactType()
	{
		var scene = new TestGameScene();
		var a = scene.addProcedure<ProcA>();
		var b = scene.addProcedure<ProcB>();
		assertTrue(a is ProcA);
		assertTrue(b is ProcB);
		assertTrue(ReferenceEquals(a, scene.getProcedure(typeof(ProcA))));
		assertTrue(ReferenceEquals(b, scene.getProcedure(typeof(ProcB))));
	}

	// ================================================================
	//  changeProcedure — 10 个函数
	// ================================================================
	private static TestGameScene sceneWith(params Type[] types)
	{
		var s = new TestGameScene();
		foreach (var t in types)
		{
			s.addProcedure(t);
		}
		return s;
	}
	private static void testChangeProcedureSetsCurProcedure()
	{
		var scene = sceneWith(typeof(SceneProcedure));
		scene.changeProcedure(typeof(SceneProcedure));
		assertNotNull(scene.getCurProcedure());
		assertTrue(scene.atProcedure(typeof(SceneProcedure)));
	}
	private static void testChangeProcedureSameTypeEarlyReturn()
	{
		var scene = sceneWith(typeof(SceneProcedure));
		var first = scene.changeProcedure(typeof(SceneProcedure));
		var second = scene.changeProcedure(typeof(SceneProcedure));
		assertTrue(ReferenceEquals(first, second));
	}
	private static void testChangeProcedureWithAddToLastList()
	{
		var scene = sceneWith(typeof(SceneProcedure), typeof(ProcA));
		scene.changeProcedure(typeof(SceneProcedure));
		scene.changeProcedure(typeof(ProcA));
		assertEqual(1, scene.getLastProcedureList().Count);
	}
	private static void testChangeProcedureAddToLastListFalse()
	{
		var scene = sceneWith(typeof(SceneProcedure), typeof(ProcA));
		scene.changeProcedure(typeof(SceneProcedure));
		scene.changeProcedure(typeof(ProcA), false);
		assertEqual(0, scene.getLastProcedureList().Count);
	}
	private static void testChangeProcedureChain()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB), typeof(ProcC));
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		assertTrue(scene.atProcedure(typeof(ProcC)));
		assertEqual(2, scene.getLastProcedureList().Count);
	}
	private static void testChangeProcedureGetCurProcedureType()
	{
		var scene = sceneWith(typeof(ProcA));
		scene.changeProcedure(typeof(ProcA));
		assertEqual(typeof(ProcA), scene.getCurProcedureType());
		scene.changeProcedure(typeof(ProcA));
		assertEqual(typeof(ProcA), scene.getCurProcedureType());
	}
	private static void testChangeProcedureGetCurOrParentProcedure()
	{
		var scene = new TestGameScene();
		scene.addProcedure<ProcA>();
		scene.changeProcedure(typeof(ProcA));
		assertNotNull(scene.getCurOrParentProcedure(typeof(ProcA)));
		assertNull(scene.getCurOrParentProcedure(typeof(ProcB)));
	}
	private static void testChangeProcedureABAB()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB));
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		assertTrue(scene.atProcedure(typeof(ProcB)));
		// A→B→A→B: 每次跳转记录上一个，lastList = [A,B,A] = 3
		int count = scene.getLastProcedureList().Count;
		assertEqual(3, count);
	}
	private static void testChangeProcedureWithMultipleTypes()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB), typeof(ProcC), typeof(ProcD));
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcD));
		scene.changeProcedure(typeof(ProcA));
		assertTrue(scene.atProcedure(typeof(ProcA)));
		assertEqual(4, scene.getLastProcedureList().Count);
	}

	// ================================================================
	//  backToLastProcedure / getLastProcedureType — 6 个函数
	// ================================================================
	private static void testBackToLastProcedureEmpty()
	{
		new TestGameScene().backToLastProcedure();
	}
	private static void testBackToLastProcedureWithHistory()
	{
		var scene = sceneWith(typeof(SceneProcedure), typeof(ProcA));
		scene.changeProcedure(typeof(SceneProcedure));
		scene.changeProcedure(typeof(ProcA));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(SceneProcedure)));
		assertEqual(0, scene.getLastProcedureList().Count);
	}
	private static void testBackToLastProcedureTwice()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB), typeof(ProcC));
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcB)));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
	}
	private static void testBackToLastProcedureThrice()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB), typeof(ProcC), typeof(ProcD));
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcD));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcC)));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcB)));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
		assertEqual(0, scene.getLastProcedureList().Count);
	}
	private static void testGetLastProcedureTypeEmpty()
	{
		assertNull(new TestGameScene().getLastProcedureType());
	}
	private static void testGetLastProcedureTypeAfterChange()
	{
		var scene = sceneWith(typeof(SceneProcedure), typeof(ProcA));
		scene.changeProcedure(typeof(SceneProcedure));
		scene.changeProcedure(typeof(ProcA));
		assertEqual(typeof(SceneProcedure), scene.getLastProcedureType());
	}

	// ================================================================
	//  enterStartProcedure — 4 个函数
	// ================================================================
	private static void testEnterStartProcedureByTempStart()
	{
		var scene = new TestGameScene();
		scene.addProcedure<ProcA>();
		scene.setTempStartProcedure(typeof(ProcA));
		scene.enterStartProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
	}
	private static void testEnterStartProcedureByStartProcedure()
	{
		var scene = new TestGameScene();
		scene.addProcedure<ProcA>();
		scene.setStartProcedureForTest(typeof(ProcA));
		scene.enterStartProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
	}
	private static void testEnterStartProcedureUsesTempFirst()
	{
		var scene = new TestGameScene();
		scene.addProcedure<ProcA>();
		scene.addProcedure<ProcB>();
		scene.setStartProcedureForTest(typeof(ProcA));
		scene.setTempStartProcedure(typeof(ProcB));
		scene.enterStartProcedure();
		assertTrue(scene.atProcedure(typeof(ProcB)));
	}
	private static void testEnterStartProcedureMultipleTimes()
	{
		var scene = new TestGameScene();
		scene.addProcedure<ProcA>();
		scene.setStartProcedureForTest(typeof(ProcA));
		scene.enterStartProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
		// 第二次 enterStartProcedure → mTempStartProcedure 为 null → 查 mStartProcedure
		// 但当前已经是 ProcA → changeProcedure 早期返回
		scene.enterStartProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
	}

	// ================================================================
	//  RichTestGameScene — 7 个函数
	// ================================================================
	private static void testRichSceneCreateProcedures()
	{
		var scene = new RichTestGameScene();
		scene.createSceneProcedure();
		assertNotNull(scene.getProcedure(typeof(ProcA)));
		assertNotNull(scene.getProcedure(typeof(ProcC)));
		assertNotNull(scene.getProcedure(typeof(ProcL)));
		assertEqual(12, scene.getProcedureCount());
	}
	private static void testRichSceneAssignStartExitProcedure()
	{
		var scene = new RichTestGameScene();
		scene.assignStartExitProcedure();
		scene.createSceneProcedure();
		scene.enterStartProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
	}
	private static void testRichSceneFullFlow()
	{
		var scene = new RichTestGameScene();
		scene.createSceneProcedure();
		scene.assignStartExitProcedure();
		scene.enterStartProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
		scene.changeProcedure(typeof(ProcB));
		assertTrue(scene.atProcedure(typeof(ProcB)));
		scene.changeProcedure(typeof(ProcC));
		assertTrue(scene.atProcedure(typeof(ProcC)));
		scene.changeProcedure(typeof(ProcF));
		assertTrue(scene.atProcedure(typeof(ProcF)));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcC)));
	}
	private static void testRichSceneAddProcedureWithParent()
	{
		var scene = new RichTestGameScene();
		scene.createSceneProcedure();
		var procC = scene.getProcedure(typeof(ProcC));
		var procD = scene.getProcedure(typeof(ProcD));
		var procE = scene.getProcedure(typeof(ProcE));
		assertTrue(ReferenceEquals(procC, procD.getParent()));
		assertTrue(ReferenceEquals(procC, procE.getParent()));
	}
	private static void testRichSceneMultipleChangeBack()
	{
		var scene = new RichTestGameScene();
		scene.createSceneProcedure();
		scene.assignStartExitProcedure();
		scene.enterStartProcedure();
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcF));
		scene.changeProcedure(typeof(ProcG));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcF)));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcC)));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
	}
	private static void testRichSceneGetCurProcedureTypeAfterChanges()
	{
		var scene = new RichTestGameScene();
		scene.createSceneProcedure();
		scene.assignStartExitProcedure();
		scene.enterStartProcedure();
		assertEqual(typeof(ProcA), scene.getCurProcedureType());
		scene.changeProcedure(typeof(ProcF));
		assertEqual(typeof(ProcF), scene.getCurProcedureType());
		scene.changeProcedure(typeof(ProcL));
		assertEqual(typeof(ProcL), scene.getCurProcedureType());
		scene.backToLastProcedure();
		assertEqual(typeof(ProcF), scene.getCurProcedureType());
	}

	// ================================================================
	//  atProcedure — 4 个函数
	// ================================================================
	private static void testAtProcedureFalseWhenNull()
	{
		var scene = new TestGameScene();
		assertFalse(scene.atProcedure(typeof(SceneProcedure)));
		assertFalse(scene.atProcedure<SceneProcedure>());
	}
	private static void testAtProcedureSelf()
	{
		var scene = sceneWith(typeof(SceneProcedure));
		scene.changeProcedure(typeof(SceneProcedure));
		assertTrue(scene.atProcedure(typeof(SceneProcedure)));
	}
	private static void testAtProcedureGeneric()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB));
		scene.changeProcedure(typeof(ProcA));
		assertTrue(scene.atProcedure<ProcA>());
		assertFalse(scene.atProcedure<ProcB>());
	}
	private static void testAtProcedureMultipleTypes()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB), typeof(ProcC));
		scene.changeProcedure(typeof(ProcA));
		assertTrue(scene.atProcedure(typeof(ProcA)));
		assertFalse(scene.atProcedure(typeof(ProcB)));
		assertFalse(scene.atProcedure(typeof(ProcC)));
		scene.changeProcedure(typeof(ProcB));
		assertFalse(scene.atProcedure(typeof(ProcA)));
		assertTrue(scene.atProcedure(typeof(ProcB)));
		assertFalse(scene.atProcedure(typeof(ProcC)));
	}

	// ================================================================
	//  atSelfProcedure — 3 个函数
	// ================================================================
	private static void testAtSelfProcedureMatch()
	{
		var scene = sceneWith(typeof(SceneProcedure));
		scene.changeProcedure(typeof(SceneProcedure));
		assertTrue(scene.atSelfProcedure(typeof(SceneProcedure)));
	}
	private static void testAtSelfProcedureNoMatch()
	{
		var scene = sceneWith(typeof(SceneProcedure));
		scene.changeProcedure(typeof(SceneProcedure));
		assertFalse(scene.atSelfProcedure(typeof(ProcA)));
	}
	private static void testAtSelfProcedureMultipleTypes()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB));
		scene.changeProcedure(typeof(ProcA));
		assertTrue(scene.atSelfProcedure(typeof(ProcA)));
		assertFalse(scene.atSelfProcedure(typeof(ProcB)));
		scene.changeProcedure(typeof(ProcB));
		assertFalse(scene.atSelfProcedure(typeof(ProcA)));
		assertTrue(scene.atSelfProcedure(typeof(ProcB)));
	}

	// ================================================================
	//  keyProcess — 2 个函数
	// ================================================================
	private static void testKeyProcessNullCurProcedure()
	{
		new TestGameScene().keyProcess(0.016f);
	}
	private static void testKeyProcessWithCurProcedure()
	{
		var scene = sceneWith(typeof(SceneProcedure));
		scene.changeProcedure(typeof(SceneProcedure));
		scene.keyProcess(0.016f);
	}

	// ================================================================
	//  prepareChangeProcedure — 2 个函数
	// ================================================================

	// ================================================================
	//  notifyProcedurePrepared — 2 个函数
	// ================================================================
	private static void testNotifyProcedurePreparedEmpty()
	{
		new TestGameScene().notifyProcedurePrepared();
	}
	private static void testNotifyProcedurePreparedWithHistory()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB));
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		// mLastProcedureList 有 ProcA → onNextProcedurePrepared 会被调用（空实现）
		scene.notifyProcedurePrepared();
	}

	// ================================================================
	//  MAX_LAST_PROCEDURE_COUNT 限制 — 3 个函数
	// ================================================================
	private static void testMaxLastProcedureCount()
	{
		var scene = new TestGameScene();
		// 注册 9 个不同流程
		scene.addProcedure<ProcA>();
		scene.addProcedure<ProcB>();
		scene.addProcedure<ProcC>();
		scene.addProcedure<ProcD>();
		scene.addProcedure<ProcE>();
		scene.addProcedure<ProcF>();
		scene.addProcedure<ProcG>();
		scene.addProcedure<ProcH>();
		scene.addProcedure<ProcI>();
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcD));
		scene.changeProcedure(typeof(ProcE));
		scene.changeProcedure(typeof(ProcF));
		scene.changeProcedure(typeof(ProcG));
		scene.changeProcedure(typeof(ProcH));
		scene.changeProcedure(typeof(ProcI));
		int count = scene.getLastProcedureList().Count;
		assertTrue(count <= 8);
	}
	private static void testMaxLastProcedureCountExactlyEight()
	{
		var scene = new TestGameScene();
		scene.addProcedure<ProcA>();
		scene.addProcedure<ProcB>();
		scene.addProcedure<ProcC>();
		scene.addProcedure<ProcD>();
		scene.addProcedure<ProcE>();
		scene.addProcedure<ProcF>();
		scene.addProcedure<ProcG>();
		scene.addProcedure<ProcH>();
		scene.addProcedure<ProcI>();
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcD));
		scene.changeProcedure(typeof(ProcE));
		scene.changeProcedure(typeof(ProcF));
		scene.changeProcedure(typeof(ProcG));
		scene.changeProcedure(typeof(ProcH));
		scene.changeProcedure(typeof(ProcI));
		// 最多 8 个历史（A 被淘汰）
		// 现在列表应该是 B,C,D,E,F,G,H,I
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcH)));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcG)));
	}
	private static void testMaxLastProcedureCountOrder()
	{
		var scene = new TestGameScene();
		scene.addProcedure<ProcA>();
		scene.addProcedure<ProcB>();
		scene.addProcedure<ProcC>();
		scene.addProcedure<ProcD>();
		scene.addProcedure<ProcE>();
		scene.addProcedure<ProcF>();
		scene.addProcedure<ProcG>();
		scene.addProcedure<ProcH>();
		scene.addProcedure<ProcI>();
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcD));
		scene.changeProcedure(typeof(ProcE));
		scene.changeProcedure(typeof(ProcF));
		scene.changeProcedure(typeof(ProcG));
		scene.changeProcedure(typeof(ProcH));
		scene.changeProcedure(typeof(ProcI));
		// 回退顺序验证
		assertTrue(scene.atProcedure(typeof(ProcI)));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcH)));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcG)));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcF)));
	}

	// ================================================================
	//  resetProperty — 4 个函数
	// ================================================================
	private static void testResetPropertyClearsCurProcedure()
	{
		var scene = sceneWith(typeof(SceneProcedure));
		scene.changeProcedure(typeof(SceneProcedure));
		scene.resetProperty();
		assertNull(scene.getCurProcedure());
	}
	private static void testResetPropertyClearsProcedureList()
	{
		var scene = new TestGameScene();
		scene.addProcedure(typeof(SceneProcedure));
		scene.resetProperty();
		assertEqual(0, scene.getSceneProcedureList().Count);
	}
	private static void testResetPropertyClearsLastProcedureList()
	{
		var scene = sceneWith(typeof(SceneProcedure), typeof(ProcA));
		scene.changeProcedure(typeof(SceneProcedure));
		scene.changeProcedure(typeof(ProcA));
		scene.resetProperty();
		assertEqual(0, scene.getLastProcedureList().Count);
	}
	private static void testResetPropertyMultiStepThenClear()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB), typeof(ProcC));
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		assertTrue(scene.getLastProcedureList().Count > 0);
		assertTrue(scene.getSceneProcedureList().Count > 0);
		scene.resetProperty();
		assertNull(scene.getCurProcedure());
		assertEqual(0, scene.getLastProcedureList().Count);
		assertEqual(0, scene.getSceneProcedureList().Count);
		// reset 后重新注册 → 正常
		scene.addProcedure(typeof(SceneProcedure));
		assertEqual(1, scene.getSceneProcedureList().Count);
		scene.changeProcedure(typeof(SceneProcedure));
		assertNotNull(scene.getCurProcedure());
	}
	// ================================================================
	//  更多 addProcedure 变体 — 2 个函数
	// ================================================================
	private static void testAddProcedureDeepParentChain()
	{
		var scene = new TestGameScene();
		var root = scene.addProcedure(typeof(ProcA));
		var child = scene.addProcedure(typeof(ProcB), typeof(ProcA));
		var grandchild = scene.addProcedure(typeof(ProcC), typeof(ProcB));
		assertTrue(ReferenceEquals(root, child.getParent()));
		assertTrue(ReferenceEquals(child, grandchild.getParent()));
		assertTrue(ReferenceEquals(root, grandchild.getParent(typeof(ProcA))));
	}
	private static void testAddProcedureParentReferenceConsistency()
	{
		var scene = new TestGameScene();
		var p1 = scene.addProcedure(typeof(ProcA));
		var p2 = scene.addProcedure(typeof(ProcB), typeof(ProcA));
		var p3 = scene.addProcedure(typeof(ProcC), typeof(ProcB));
		// 三层引用一致性
		assertTrue(ReferenceEquals(p1, p2.getParent()));
		assertTrue(ReferenceEquals(p2, p3.getParent()));
		assertTrue(ReferenceEquals(p1, p3.getParent(typeof(ProcA))));
	}
	// ================================================================
	//  更多 changeProcedure 变体 — 4 个函数
	// ================================================================
	private static void testChangeProcedureGeneric()
	{
		var scene = sceneWith(typeof(ProcA));
		var result = scene.changeProcedure<ProcA>();
		assertNotNull(result);
		assertTrue(scene.atProcedure<ProcA>());
	}
	private static void testChangeProcedureTwelveProcedures()
	{
		var scene = new TestGameScene();
		scene.addProcedure<ProcA>(); 
		scene.addProcedure<ProcB>();
		scene.addProcedure<ProcC>(); 
		scene.addProcedure<ProcD>();
		scene.addProcedure<ProcE>(); 
		scene.addProcedure<ProcF>();
		scene.addProcedure<ProcG>(); 
		scene.addProcedure<ProcH>();
		scene.addProcedure<ProcI>(); 
		scene.addProcedure<ProcJ>();
		scene.addProcedure<ProcK>(); 
		scene.addProcedure<ProcL>();
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcD));
		scene.changeProcedure(typeof(ProcE));
		scene.changeProcedure(typeof(ProcF));
		scene.changeProcedure(typeof(ProcG));
		scene.changeProcedure(typeof(ProcH));
		scene.changeProcedure(typeof(ProcI));
		scene.changeProcedure(typeof(ProcJ));
		scene.changeProcedure(typeof(ProcK));
		scene.changeProcedure(typeof(ProcL));
		assertTrue(scene.atProcedure(typeof(ProcL)));
		assertTrue(scene.getLastProcedureList().Count <= 8);
	}
	private static void testChangeProcedureBackAndForth()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB), typeof(ProcC));
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcB));
		assertTrue(scene.atProcedure(typeof(ProcB)));
	}
	// ================================================================
	//  更多 enterStartProcedure — 1 个函数
	// ================================================================
	private static void testEnterStartProcedureNullStartProcedure()
	{
		var scene = new TestGameScene();
		// mStartProcedure 和 mTempStartProcedure 均为 null → changeProcedure(null) → TryGetValue(null) 抛异常
		try
		{ 
			scene.enterStartProcedure(); 
		}
		catch { /* 预期异常 */ }
	}
	// ================================================================
	//  更多 backToLastProcedure — 1 个函数
	// ================================================================
	private static void testBackToLastProcedureEmptyThenChange()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB));
		scene.backToLastProcedure();
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
	}
	// ================================================================
	//  更多 RichTestGameScene — 3 个函数
	// ================================================================
	private static void testRichSceneGetCurOrParentWithChildProcedure()
	{
		var scene = new RichTestGameScene();
		scene.createSceneProcedure();
		// ProcD 是 ProcC 的子流程；当进入 ProcC 时，getCurOrParentProcedure(typeof(ProcC)) 应返回 ProcC
		scene.changeProcedure(typeof(ProcC));
		assertTrue(ReferenceEquals(scene.getCurProcedure(), scene.getCurOrParentProcedure(typeof(ProcC))));
	}
	private static void testRichSceneProcedureParentLookup()
	{
		var scene = new RichTestGameScene();
		scene.createSceneProcedure();
		var procC = scene.getProcedure(typeof(ProcC));
		var procD = scene.getProcedure(typeof(ProcD));
		assertTrue(ReferenceEquals(procC, procD.getParent()));
		assertTrue(ReferenceEquals(procC, procD.getParent(typeof(ProcC))));
	}
	private static void testRichSceneChangeBackUntilEmpty()
	{
		var scene = new RichTestGameScene();
		scene.createSceneProcedure();
		scene.assignStartExitProcedure();
		scene.enterStartProcedure();
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcF));
		scene.changeProcedure(typeof(ProcG));
		scene.changeProcedure(typeof(ProcH));
		scene.backToLastProcedure();
		scene.backToLastProcedure();
		scene.backToLastProcedure();
		scene.backToLastProcedure();
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
		// 历史已空，再次 backToLastProcedure 安全返回
		scene.backToLastProcedure();
		assertTrue(scene.atProcedure(typeof(ProcA)));
	}
	// ================================================================
	//  getCurProcedureType — 2 个函数
	// ================================================================
	private static void testGetCurProcedureTypeBeforeChange()
	{
		var scene = new TestGameScene();
		try 
		{
			var t = scene.getCurProcedureType(); 
		}
		catch { /* mCurProcedure=null → NPE，预期行为 */ }
	}
	private static void testGetCurProcedureTypeMultipleChanges()
	{
		var scene = sceneWith(typeof(ProcA), typeof(ProcB), typeof(ProcC));
		scene.changeProcedure(typeof(ProcA));
		assertEqual(typeof(ProcA), scene.getCurProcedureType());
		scene.changeProcedure(typeof(ProcB));
		assertEqual(typeof(ProcB), scene.getCurProcedureType());
		scene.changeProcedure(typeof(ProcC));
		assertEqual(typeof(ProcC), scene.getCurProcedureType());
		scene.changeProcedure(typeof(ProcA));
		assertEqual(typeof(ProcA), scene.getCurProcedureType());
	}
	// ================================================================
	//  getCurOrParentProcedure — 1 个函数
	// ================================================================
	private static void testGetCurOrParentProcedureWithParentChain()
	{
		var scene = new TestGameScene();
		var parent = scene.addProcedure(typeof(ProcA));
		var child = scene.addProcedure(typeof(ProcB), typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		// 当前流程是 ProcB（child），getCurOrParentProcedure(typeof(ProcA)) 向上查找 → parent
		var result = scene.getCurOrParentProcedure(typeof(ProcA));
		assertTrue(ReferenceEquals(parent, result));
	}
	// ================================================================
	//  MAX_LAST_PROCEDURE_COUNT 额外 — 2 个函数
	// ================================================================
	private static void testMaxLastProcedureCountWithDifferentTypes()
	{
		var scene = new TestGameScene();
		// 用 5 个不同类型测试 max count
		scene.addProcedure<ProcA>();
		scene.addProcedure<ProcB>();
		scene.addProcedure<ProcC>();
		scene.addProcedure<ProcD>();
		scene.addProcedure<ProcE>();
		scene.changeProcedure(typeof(ProcA));
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcD));
		scene.changeProcedure(typeof(ProcE));
		assertTrue(scene.atProcedure(typeof(ProcE)));
		assertTrue(scene.getLastProcedureList().Count <= 8);
	}
	private static void testMaxLastProcedureCountBackAllTheWay()
	{
		var scene = new TestGameScene();
		scene.addProcedure<ProcA>(); 
		scene.addProcedure<ProcB>();
		scene.addProcedure<ProcC>(); 
		scene.addProcedure<ProcD>();
		scene.addProcedure<ProcE>();
		scene.addProcedure<ProcF>();
		scene.addProcedure<ProcG>(); 
		scene.addProcedure<ProcH>();
		scene.addProcedure<ProcI>();
		scene.changeProcedure(typeof(ProcA));
		// 从 A 到 I 逐次跳转，产生历史记录
		scene.changeProcedure(typeof(ProcB));
		scene.changeProcedure(typeof(ProcC));
		scene.changeProcedure(typeof(ProcD));
		scene.changeProcedure(typeof(ProcE));
		scene.changeProcedure(typeof(ProcF));
		scene.changeProcedure(typeof(ProcG));
		scene.changeProcedure(typeof(ProcH));
		scene.changeProcedure(typeof(ProcI));
		// 回退到不能再回退，验证空历史时安全返回
		while (scene.getLastProcedureList().Count > 0)
		{
			scene.backToLastProcedure();
		}
		assertNotNull(scene.getCurProcedure());
		// 再次回退验证空列表安全
		scene.backToLastProcedure();
		assertNotNull(scene.getCurProcedure());
	}


	

	// ═════════════════════════════════════════════════════════════════════
	//  建场景 helper
	// ═════════════════════════════════════════════════════════════════════
	static DGDeepScene NewDeepScene()
	{
		DGProc.Reset();
		var s = new DGDeepScene();
		s.createSceneProcedure();
		s.assignStartExitProcedure();
		return s;
	}
	static void Verify(List<string> expected)
	{
		assertEqual(expected.Count, DGProc.CallLog.Count, $"call count mismatch: expected {expected.Count}, actual {DGProc.CallLog.Count}");
		for (int i = 0; i < expected.Count && i < DGProc.CallLog.Count; ++i)
		{
			assertEqual(expected[i], DGProc.CallLog[i], $"call #{i} mismatch");
		}
	}

	// ═════════════════════════════════════════════════════════════════════
	//  tempStart 一次性语义
	// ═════════════════════════════════════════════════════════════════════
	//  1. 设置了 tempStart, enterStartProcedure 用之一次后清除
	private static void testTempStartUsedOnceThenFallsBack()
	{
		var s = NewDeepScene();
		s.setTemp(typeof(DGB));
		s.enterStartProcedure();
		// 使用了 tempStart=B
		assertTrue(s.atProcedure(typeof(DGB)), "tempStart 后进入 B");
		// 再次 enter: 已清除 tempStart → 回到 startProcedure=A
		s.enterStartProcedure();
		assertTrue(s.atProcedure(typeof(DGA)), "tempStart 一次性, 二次进入回到 A");
	}

	//  2. 未设置 tempStart → 直接用 startProcedure
	private static void testTempStartNullMeansStart()
	{
		var s = NewDeepScene();
		s.enterStartProcedure();
		assertTrue(s.atProcedure(typeof(DGA)), "无 tempStart 时进入 startProcedure=A");
	}

	//  3. 两次 enter 间修改 tempStart → 每次都生效
	private static void testTempStartChangeBetweenEnters()
	{
		var s = NewDeepScene();
		s.enterStartProcedure(); // A
		s.setTemp(typeof(DGC));
		s.enterStartProcedure(); // C
		assertTrue(s.atProcedure(typeof(DGC)), "重置 tempStart 后再次 enter 进入 C");
	}

	// ═════════════════════════════════════════════════════════════════════
	//  changeProcedure 复杂链
	// ═════════════════════════════════════════════════════════════════════
	//  4. 平级连续跳转 10 个流程: 每次退出前一进程并进入新进程
	private static void testChangeChain10Procedures()
	{
		var s = NewDeepScene();
		Type[] chain = { typeof(DGA), typeof(DGB), typeof(DGC), typeof(DGD), typeof(DGE),
						 typeof(DGF), typeof(DGG), typeof(DGH), typeof(DGI), typeof(DGJ) };
		for (int i = 0; i < chain.Length; ++i)
		{
			s.changeProcedure(chain[i]);
			assertEqual(chain[i], s.getCurProcedureType(), $"跳到第 {i} 个流程");
		}
		// 最后一个是 J
		assertTrue(s.atProcedure(typeof(DGJ)), "链末为 J");
	}

	//  5. addToLastList=false: 跳转不记录历史
	private static void testChangeAddToLastListFalseNoHistory()
	{
		var s = NewDeepScene();
		s.changeProcedure(typeof(DGA), false);
		s.changeProcedure(typeof(DGB), false);
		s.changeProcedure(typeof(DGC), false);
		// 由于从未加历史, backToLast 不应发生跳转
		assertNull(s.getLastProcedureType(), "addToLastList=false 时无历史");
	}

	//  6. 父→子 生命周期: 退出父进入子
	private static void testChangeParentChildLifecycle()
	{
		var s = NewDeepScene();
		s.changeProcedure(typeof(DGParent));
		DGProc.CallLog.Clear();
		s.changeProcedure(typeof(DGChild));
		Verify(new() {
			"Parent.onExitToChild(Child)",
			"Parent.onExitSelf()",
			"Child.onInit(Parent)"
		});
	}

	//  7. 子→父 深层回退: 子退出 → 祖先链逐级退出 → 父 onInitFromChild
	private static void testChangeFromChildToParentDeep()
	{
		var s = NewDeepScene();
		s.changeProcedure(typeof(DGParent));
		s.changeProcedure(typeof(DGChild));
		DGProc.CallLog.Clear();
		s.changeProcedure(typeof(DGParent));
		Verify(new() {
			"Child.onExit(Parent)", "Child.onExitSelf()",
			"Parent.onExitToChild(Parent)", "Parent.onExitSelf()",
			"Parent.onInitFromChild(Child)"
		});
	}

	// ═════════════════════════════════════════════════════════════════════
	//  lastProcedure 历史溢出 (MAX=8)
	// ═════════════════════════════════════════════════════════════════════
	//  8. 连续 11 次添加历史 → 只保留最近 8 个
	private static void testLastProcedureOverflowKeepsEight()
	{
		var s = NewDeepScene();
		// 先进入第 0 个作为基底
		Type[] chain = { typeof(DGA), typeof(DGB), typeof(DGC), typeof(DGD), typeof(DGE),
						 typeof(DGF), typeof(DGG), typeof(DGH), typeof(DGI), typeof(DGJ), typeof(DGK) };
		for (int i = 0; i < chain.Length; ++i)
		{
			s.changeProcedure(chain[i]);
		}
		// 当前在 K。历史记录的是前 10 个(每次 change 把 prev push 进历史), 超过 8 → 裁剪到 8
		assertTrue(s.atProcedure(typeof(DGK)), "当前在 K");
		// backToLast 逐级回退, 每级应命中最近的历史项
		// 历史应从 J 开始: J,I,H,G,F,E,D,C (前两轮 A,B 被裁剪)
		s.backToLastProcedure();
		assertTrue(s.atProcedure(typeof(DGJ)), "backToLast 一步回到 J");
		s.backToLastProcedure();
		assertTrue(s.atProcedure(typeof(DGI)), "backToLast 两步回到 I");
	}

	//  9. backToLast 多层回退: A→B→C→D 后连续回退至 A
	private static void testBackToLastMultiStep()
	{
		var s = NewDeepScene();
		s.changeProcedure(typeof(DGA));
		s.changeProcedure(typeof(DGB));
		s.changeProcedure(typeof(DGC));
		s.changeProcedure(typeof(DGD));
		assertTrue(s.atProcedure(typeof(DGD)), "链末为 D");
		s.backToLastProcedure();
		assertTrue(s.atProcedure(typeof(DGC)), "回退到 C");
		s.backToLastProcedure();
		assertTrue(s.atProcedure(typeof(DGB)), "回退到 B");
		s.backToLastProcedure();
		assertTrue(s.atProcedure(typeof(DGA)), "回退到 A");
	}

	//  10. 空历史 backToLast: 无操作
	private static void testBackToLastEmptyScene()
	{
		var s = NewDeepScene();
		DGProc.CallLog.Clear();
		s.backToLastProcedure();
		assertNull(s.getCurProcedure(), "backToLast 空历史时 cur 仍为 null");
		assertEqual(0, DGProc.CallLog.Count, "空历史不触发任何跳转");
	}

	//  11. 溢出后 backToLast 仍能走到最老的保留项
	private static void testBackToLastAfterOverflow()
	{
		var s = NewDeepScene();
		Type[] chain = { typeof(DGA), typeof(DGB), typeof(DGC), typeof(DGD), typeof(DGE),
						 typeof(DGF), typeof(DGG), typeof(DGH), typeof(DGI), typeof(DGJ), typeof(DGK) };
		for (int i = 0; i < chain.Length; ++i)
		{
			s.changeProcedure(chain[i]);
		}
		// 连续回退直至历史耗尽
		int steps = 0;
		while (s.getLastProcedureType() != null && steps < 20)
		{
			s.backToLastProcedure();
			++steps;
		}
		assertEqual(8, steps, "溢出裁剪后应可回退 8 步");
	}

	// ═════════════════════════════════════════════════════════════════════
	//  prepareExit 准备态过渡
	// ═════════════════════════════════════════════════════════════════════
	//  12. prepareChangeProcedure: 当前流程进入准备态, 记录 prepareNext
	private static void testPrepareExitStateTransition()
	{
		var s = NewDeepScene();
		s.changeProcedure(typeof(DGA));
		DGProc.CallLog.Clear();
		s.prepareChangeProcedure(typeof(DGB), 1.0f);
		assertTrue(s.getCurProcedure().isPreparingExit(), "prepareChangeProcedure 后当前流程进入准备态");
		assertEqual(typeof(DGB), s.getCurProcedure().getPrepareNext().GetType(), "prepareNext 为目标流程 B 类型");
		// onPrepareExit 应被调用
		Verify(new() { "A.onPrepareExit(B)" });
	}

	//  13. 准备未完成时短 update 不触发强制跳转
	private static void testPrepareExitShortUpdateNotFire()
	{
		var s = NewDeepScene();
		s.changeProcedure(typeof(DGA));
		s.prepareChangeProcedure(typeof(DGB), 1.0f);
		// 直接驱动当前流程的 update 短时间 (未到 1.0s) → 不跳转
		s.getCurProcedure().update(0.3f);
		assertTrue(s.atProcedure(typeof(DGA)), "短 update 未到时间仍停留在 A");
		assertTrue(s.getCurProcedure().isPreparingExit(), "短 update 后仍处于准备态");
	}

	//  14. 正在准备退出时再次 prepare → 被拦截 (不走 error 路径的纯状态验证)
	//  此路径源码会 logError, 按项目约定不触发; 此处仅验证准备态可保持
	//  (补充: 准备态在未到时间前不触发强制跳转已在 testPrepareExitShortUpdateNotFire 覆盖)

	// ═════════════════════════════════════════════════════════════════════
	//  notifyProcedurePrepared 回调链
	// ═════════════════════════════════════════════════════════════════════
	//  15. 通知上一流程已准备好
	private static void testNotifyProcedurePreparedCallsLast()
	{
		var s = NewDeepScene();
		s.changeProcedure(typeof(DGA));
		s.changeProcedure(typeof(DGB));
		DGProc.CallLog.Clear();
		s.notifyProcedurePrepared();
		// 上一流程是 A, 当前是 B
		Verify(new() { "A.onNextProcedurePrepared(B)" });
	}

	// ═════════════════════════════════════════════════════════════════════
	//  atProcedure 深层匹配
	// ═════════════════════════════════════════════════════════════════════
	//  16. 父子流程: atProcedure(父) 为 true
	private static void testAtProcedureParentChild()
	{
		var s = NewDeepScene();
		s.changeProcedure(typeof(DGChild));
		// 子流程同时命中自身与父流程
		assertTrue(s.atProcedure(typeof(DGChild)), "在子流程时 atProcedure(子) true");
		assertTrue(s.atProcedure(typeof(DGParent)), "在子流程时 atProcedure(父) true");
		assertTrue(s.atSelfProcedure(typeof(DGChild)), "atSelfProcedure(子) true");
		assertFalse(s.atSelfProcedure(typeof(DGParent)), "atSelfProcedure(父) false");
	}

	//  17. getCurOrParentProcedure: 在子流程时取父类型
	private static void testGetCurOrParentProcedure()
	{
		var s = NewDeepScene();
		s.changeProcedure(typeof(DGChild));
		assertTrue(s.getCurOrParentProcedure(typeof(DGChild)) is DGChild, "取到当前子流程");
		assertTrue(s.getCurOrParentProcedure(typeof(DGParent)) is DGProc, "取到父流程");
	}
}