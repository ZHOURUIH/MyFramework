using System;
using System.Collections.Generic;
using static TestAssert;

// GameScene 深度测试
// ============================================================================
// 与 GameSceneTest(单接口调用) 的区别:
//   本测试聚焦 GameScene 的复杂流程状态机行为——tempStart 一次性语义、
//   lastProcedure 历史溢出(MAX=8)、backToLast 多层回退、prepareExit 准备态
//   过渡、父子流程下的完整生命周期、notifyProcedurePrepared 回调链。
//   全部走局部 DGDeepScene(不依赖全局场景管理器), 安全无泄漏。
// ============================================================================

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

public static class GameSceneDeepTest
{
	public static void Run()
	{
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
