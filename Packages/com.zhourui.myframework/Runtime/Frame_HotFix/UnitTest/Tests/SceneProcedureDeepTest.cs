using System;
using System.Collections.Generic;
using static TestAssert;

// SceneProcedure 深度测试
// ============================================================================
// 与 SceneProcedureTest(单接口查询: getParentList/getSameParent/isThisOrParent/init)
// 的区别:
//   本测试聚焦 SceneProcedure 的复杂生命周期——exit() 多级回调传播、
//   prepareExit 准备态 + exit 清除、多级父子初始化级联、深层 getSameParent(LCA)、
//   深层 isThisOrParent 匹配。其中 exit() 因需 mDelayCmdList 非 null 而在
//   本测试通过构造初始化解锁 (源码直接单测测不到)。
// ============================================================================

// 可追踪流程: 记录生命周期回调, 构造初始化 mDelayCmdList 解锁 exit()
public class SDPProc : SceneProcedure
{
	public string tag = "S";
	public SDPProc() { mDelayCmdList = new HashSet<long>(); }
	public static List<string> CallLog = new();
	public override void resetProperty()
	{
		base.resetProperty();
		tag = "S";
	}
	public static void Reset() { CallLog.Clear(); }
	protected override void onInit(SceneProcedure last) { CallLog.Add($"{tag}.onInit({Name(last)})"); }
	protected override void onInitFromChild(SceneProcedure last) { CallLog.Add($"{tag}.onInitFromChild({Name(last)})"); }
	protected override void onExit(SceneProcedure next) { CallLog.Add($"{tag}.onExit({Name(next)})"); }
	protected override void onExitToChild(SceneProcedure next) { CallLog.Add($"{tag}.onExitToChild({Name(next)})"); }
	protected override void onExitSelf() { CallLog.Add($"{tag}.onExitSelf()"); }
	protected override void onPrepareExit(SceneProcedure next) { CallLog.Add($"{tag}.onPrepareExit({Name(next)})"); }
	static string Name(SceneProcedure p)
	{
		return p is SDPProc sp ? sp.tag : (p?.GetType().Name ?? "null");
	}
}
public class SDPA : SDPProc { public SDPA() { tag = "A"; } }
public class SDPB : SDPProc { public SDPB() { tag = "B"; } }
public class SDPC : SDPProc { public SDPC() { tag = "C"; } }
public class SDPDeepA : SDPProc { public SDPDeepA() { tag = "DeepA"; } }
public class SDPDeepB : SDPProc { public SDPDeepB() { tag = "DeepB"; } }
public class SDPRoot : SDPProc { public SDPRoot() { tag = "Root"; } }

public static class SceneProcedureDeepTest
{
	// 校验 SDPProc.CallLog 与期望序列完全一致
	private static void Verify(List<string> expected)
	{
		List<string> actual = SDPProc.CallLog;
		assertEqual(expected.Count, actual.Count, $"CallLog 条数不符 -> 期望[{string.Join(",", expected)}] 实际[{string.Join(",", actual)}]");
		for (int i = 0; i < expected.Count; ++i)
		{
			assertEqual(expected[i], actual[i], $"CallLog 第{i}条不符");
		}
	}

	public static void Run()
	{
		// ─── exit() 多级回调传播 ───
		testExitToChildNoParentExit();
		testExitFlatToOther();
		testExitClearsPrepareState();
		testExitPropagatesToParent();
		testExitSameParentChild();
		// ─── 多级父子生命周期 (通过 init 级联) ───
		testInitCascadeChildFirst();
		testInitCascadeFromLeaf();
		// ─── getSameParent 深层 LCA ───
		testGetSameParentDeepBranches();
		testGetSameParentSameLeaf();
		// ─── isThisOrParent 深层匹配 ───
		testIsThisOrParentDeepChain();
		testIsThisOrParentSiblingFalse();
		// ─── prepareExit 状态 + 清理 ───
		testPrepareExitSetsState();
		testExitAfterPrepareStopsTimer();
	}

	// ═════════════════════════════════════════════════════════════════════
	//  exit() 多级回调传播
	// ═════════════════════════════════════════════════════════════════════
	//  1. exitTo==this 且 nextPro 是子流程 → onExitToChild + onExitSelf
	private static void testExitToChildNoParentExit()
	{
		SDPProc.Reset();
		SDPRoot root = new();
		SDPDeepA child = new();
		root.addChildProcedure(child);
		// 当 this==exitTo, 且 next 是子流程 → 只退到子, 不再退自身之外
		root.exit(root, child);
		Verify(new() {
			"Root.onExitToChild(DeepA)",
			"Root.onExitSelf()"
		});
	}

	//  2. 平级退出: exitTo!=this → onExit + onExitSelf
	private static void testExitFlatToOther()
	{
		SDPProc.Reset();
		SDPA a = new();
		SDPB b = new();
		a.exit(b, b);
		Verify(new() {
			"A.onExit(B)",
			"A.onExitSelf()"
		});
	}

	//  3. exit 清除准备态: 结束后 isPreparingExit false, prepareNext null
	private static void testExitClearsPrepareState()
	{
		SDPProc.Reset();
		SDPA a = new();
		SDPB b = new();
		a.prepareExit(b, 1.0f);
		assertTrue(a.isPreparingExit(), "prepareExit 后进入准备态");
		assertTrue(ReferenceEquals(b, a.getPrepareNext()), "prepareNext 指向 B");
		a.exit(b, b);
		assertFalse(a.isPreparingExit(), "exit 后清除准备态");
		assertNull(a.getPrepareNext(), "exit 后 prepareNext 清空");
	}

	//  4. exit 传播到父节点: 子退出会向上传递
	private static void testExitPropagatesToParent()
	{
		SDPProc.Reset();
		SDPRoot root = new();
		SDPDeepA parent = new();
		SDPDeepB child = new();
		root.addChildProcedure(parent);
		parent.addChildProcedure(child);
		// 子流程退出到 root (跳出父子链)
		child.exit(root, root);
		// child 退出 → 父(parent) 退出 → 祖父(root) 退出 直到命中 exitTo
		Verify(new() {
			"DeepB.onExit(Root)", "DeepB.onExitSelf()",
			"DeepA.onExit(Root)", "DeepA.onExitSelf()",
			"Root.onExitToChild(Root)", "Root.onExitSelf()"
		});
	}

	//  5. exitSelf 命中 exitTo: 只退出自身及以上的链
	private static void testExitSameParentChild()
	{
		SDPProc.Reset();
		SDPRoot root = new();
		SDPDeepA parent = new();
		SDPDeepB child = new();
		root.addChildProcedure(parent);
		parent.addChildProcedure(child);
		// child 退出到父 parent: child.onExit + onExitSelf → 父==exitTo 且 next 是子流程 → onExitToChild+onExitSelf
		child.exit(parent, parent);
		Verify(new() {
			"DeepB.onExit(DeepA)", "DeepB.onExitSelf()",
			"DeepA.onExitToChild(DeepA)", "DeepA.onExitSelf()"
		});
	}

	// ═════════════════════════════════════════════════════════════════════
	//  多级父子生命周期 (通过 init 级联)
	// ═════════════════════════════════════════════════════════════════════
	//  6. 子先 init: 父未初始化 → 先级联初始化父
	private static void testInitCascadeChildFirst()
	{
		SDPProc.Reset();
		SDPRoot root = new();
		SDPDeepA child = new();
		root.addChildProcedure(child);
		child.init(null);
		// child.init(null) → 父(root) 未初始化 → root.init(null) → root.onExitToChild(child)+onExitSelf → child.onInit(last)
		// 注意: init 只把传入的 lastProcedure(此处为 null)透传给所有层级的 onInit,
		// 父流程 root 不会作为 last 传给子流程的 onInit(只有从子流程返回时 last!=null 才走 onInitFromChild)。
		Verify(new() {
			"Root.onInit(null)",
			"Root.onExitToChild(DeepA)",
			"Root.onExitSelf()",
			"DeepA.onInit(null)"
		});
	}

	//  7. 从最深层叶子 init: 整条祖先链级联
	private static void testInitCascadeFromLeaf()
	{
		SDPProc.Reset();
		SDPRoot root = new();
		SDPDeepA parent = new();
		SDPDeepB leaf = new();
		root.addChildProcedure(parent);
		parent.addChildProcedure(leaf);
		leaf.init(null);
		// leaf.init(null) → parent.init(null) → root.init(null) → root.onExitToChild(parent)+onExitSelf
		//              → parent.onInit(null) → parent.onExitToChild(leaf)+onExitSelf → leaf.onInit(null)
		// 同上: init 透传 lastProcedure=null, 各层 onInit 均收到 null(父不会作为 last 传给子)。
		Verify(new() {
			"Root.onInit(null)",
			"Root.onExitToChild(DeepA)",
			"Root.onExitSelf()",
			"DeepA.onInit(null)",
			"DeepA.onExitToChild(DeepB)",
			"DeepA.onExitSelf()",
			"DeepB.onInit(null)"
		});
	}

	// ═════════════════════════════════════════════════════════════════════
	//  getSameParent 深层 LCA
	// ═════════════════════════════════════════════════════════════════════
	//  8. 两深叶节点找共同祖先
	private static void testGetSameParentDeepBranches()
	{
		SDPRoot root = new();
		SDPDeepA branchA = new();
		SDPDeepB branchB = new();
		SDPDeepA leafA = new();
		SDPDeepB leafB = new();
		root.addChildProcedure(branchA);
		root.addChildProcedure(branchB);
		branchA.addChildProcedure(leafA);
		branchB.addChildProcedure(leafB);
		var same = leafA.getSameParent(leafB);
		assertTrue(ReferenceEquals(root, same), "两深叶共同祖先为 root");
	}

	//  9. 同一叶子与自身: 返回自身
	private static void testGetSameParentSameLeaf()
	{
		SDPRoot root = new();
		SDPDeepA child = new();
		root.addChildProcedure(child);
		var same = child.getSameParent(child);
		assertTrue(ReferenceEquals(child, same), "叶子与自身共同祖先为自身");
	}

	// ═════════════════════════════════════════════════════════════════════
	//  isThisOrParent 深层匹配
	// ═════════════════════════════════════════════════════════════════════
	//  10. 深叶匹配整条祖先链
	private static void testIsThisOrParentDeepChain()
	{
		SDPRoot root = new();
		SDPDeepA parent = new();
		SDPDeepB leaf = new();
		root.addChildProcedure(parent);
		parent.addChildProcedure(leaf);
		assertTrue(leaf.isThisOrParent(typeof(SDPRoot)), "叶匹配根类型");
		assertTrue(leaf.isThisOrParent(typeof(SDPDeepA)), "叶匹配父类型");
		assertTrue(leaf.isThisOrParent(typeof(SDPDeepB)), "叶匹配自身类型");
	}

	//  11. 兄弟之间互不匹配
	private static void testIsThisOrParentSiblingFalse()
	{
		SDPRoot root = new();
		SDPDeepA a = new();
		SDPDeepB b = new();
		root.addChildProcedure(a);
		root.addChildProcedure(b);
		assertFalse(a.isThisOrParent(typeof(SDPDeepB)), "兄弟 A 不匹配 B");
		assertFalse(b.isThisOrParent(typeof(SDPDeepA)), "兄弟 B 不匹配 A");
	}

	// ═════════════════════════════════════════════════════════════════════
	//  prepareExit 状态 + 清理
	// ═════════════════════════════════════════════════════════════════════
	//  12. prepareExit 设置状态与回调
	private static void testPrepareExitSetsState()
	{
		SDPProc.Reset();
		SDPA a = new();
		SDPB b = new();
		a.prepareExit(b, 2.0f);
		assertTrue(a.isPreparingExit(), "prepareExit 后处于准备态");
		assertTrue(ReferenceEquals(b, a.getPrepareNext()), "prepareNext 指向 B");
		Verify(new() { "A.onPrepareExit(B)" });
	}

	//  13. exit 后计时器停止 (借助 getPrepareNext 清空验证)
	private static void testExitAfterPrepareStopsTimer()
	{
		SDPProc.Reset();
		SDPA a = new();
		SDPB b = new();
		a.prepareExit(b, 1.0f);
		a.exit(b, b);
		assertFalse(a.isPreparingExit(), "exit 后不再准备退出");
	}
}
