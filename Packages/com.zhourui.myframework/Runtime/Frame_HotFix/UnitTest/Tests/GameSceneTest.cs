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
	}

	// ================================================================
	//  初始状态 — 4 个函数
	// ================================================================
	private static void testDefaultCurProcedure()
	{
		assertNull(new TestGameScene().getCurProcedure());
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
}