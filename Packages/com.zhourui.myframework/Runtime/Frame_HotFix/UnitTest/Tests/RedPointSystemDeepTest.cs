using System;
using UnityEngine;
using static TestAssert;
using static FrameBaseHotFix;
using static FrameUtility;

// RedPointSystem 深度测试 — 复杂红点树结构下的状态级联传播
// 覆盖 多层树/多叶聚合/子树销毁/动态重挂 等复杂调用链下的状态正确性
//   OR 聚合: 根下多个叶节点, 任一 enable 则根 enable
//   深层树: 3 层结构, 叶变化逐层向上传播 enable
//   叶销毁: 销毁一个叶后父节点重新计算状态
//   子树销毁: 销毁根节点级联销毁所有子孙
//   update 驱动: 事件触发 dirty → update 只刷新该叶 → 级联向上
//   count 边界: setCount(0) 使自身及父链 disable
//   动态重挂: setParent 移动节点后两棵树状态都重新计算
// 使用局部 new RedPointSystem() 实例, 不污染全局 mRedPointSystem
public static class RedPointSystemDeepTest
{
	// 测试用叶节点: 必须调用 base.init()(此规范强制要求)
	// 因未 override initEventType, mEventTypeList 为空, base.init() 不实际注册监听
	// refresh 保持自身 enable 不变(由外部 setEnable 控制)
	private class TestLeafRedPoint : RedPoint
	{
		public override void init()
		{
			base.init();
		}
		public override void refresh()
		{
			// 叶节点保持当前 enable 状态
		}
	}

	// 测试用真实事件叶节点: 监听事件触发 dirty, 用于 update 驱动刷新测试
	private class TestEventRedPoint : RedPoint
	{
		public int mEventTriggerCount;
		public override void init()
		{
			base.init(); // 注册事件监听
		}
		public override void refresh()
		{
			// 叶节点保持当前 enable 状态
		}
		protected override void initEventType()
		{
			addEvent<TestEvent>();
		}
		public override void resetProperty()
		{
			base.resetProperty();
			mEventTriggerCount = 0;
		}
		public void triggerEvent()
		{
			++mEventTriggerCount;
			onEventTrigger(); // 设置 dirty
		}
	}

	private class TestEvent : GameEvent { }

	public static void Run()
	{
		testMultiLeafOrAggregation();
		testDeepTreePropagation();
		testDestroyLeafRecomputesParent();
		testDestroySubtreeCascades();
		testUpdateDrivenDirtyRefresh();
		testCountZeroDisablesParentChain();
		testReparentRecomputesState();
		testFullRefreshRecomputesTree();
		testCountUISync();
	}

	// 创建局部 RedPointSystem
	private static RedPointSystem CreateSystem()
	{
		return new RedPointSystem();
	}

	// 创建纯 RedPoint 父节点 (setParent 要求 parent 类型为 RedPoint/RedPointCount)
	private static RedPoint CreateParent(RedPointSystem sys)
	{
		return sys.createRedPoint();
	}

	// 创建叶节点
	private static TestLeafRedPoint CreateLeaf(RedPointSystem sys)
	{
		return sys.createRedPoint<TestLeafRedPoint>();
	}

	// 创建带 UI 节点的对象
	private static myUGUIObject CreateUI()
	{
		var go = new GameObject("RedPointDeepUI");
		go.AddComponent<RectTransform>();
		var ui = new myUGUIObject();
		ui.setObject(go);
		return ui;
	}

	private static void DestroyUI(myUGUIObject ui)
	{
		if (ui != null && ui.getGameObject() != null)
		{
			UnityEngine.Object.DestroyImmediate(ui.getGameObject());
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// OR 聚合: 根下有多个叶节点, 任一叶 enable 则根 enable
	// ═════════════════════════════════════════════════════════════════
	private static void testMultiLeafOrAggregation()
	{
		var sys = CreateSystem();
		// 根: 纯 RedPoint
		RedPoint root = CreateParent(sys);
		// 三个叶节点挂到根下
		var leaf1 = CreateLeaf(sys);
		var leaf2 = CreateLeaf(sys);
		var leaf3 = CreateLeaf(sys);
		leaf1.setParent(root);
		leaf2.setParent(root);
		leaf3.setParent(root);

		// 初始全 disable → 根 disable
		sys.refresh();
		assertFalse(root.isEnable(), "根初始 disable");

		// 仅 leaf1 enable → 根 enable
		leaf1.setEnable(true);
		sys.refresh();
		assertTrue(root.isEnable(), "任一时 enable 则根 enable");

		// leaf1 disable, leaf2 enable → 根仍 enable
		leaf1.setEnable(false);
		leaf2.setEnable(true);
		sys.refresh();
		assertTrue(root.isEnable(), "leaf2 enable 根仍 enable");

		// 全 disable → 根 disable
		leaf2.setEnable(false);
		sys.refresh();
		assertFalse(root.isEnable(), "全部 disable 根 disable");

		// 清理
		sys.destroyRedPoint(root);
	}

	// ═════════════════════════════════════════════════════════════════
	// 深层树: 根→中→叶 3 层, 叶变化逐层向上传播
	// ═════════════════════════════════════════════════════════════════
	private static void testDeepTreePropagation()
	{
		var sys = CreateSystem();
		RedPoint root = CreateParent(sys);
		RedPoint mid = CreateParent(sys);
		var leaf = CreateLeaf(sys);
		mid.setParent(root);
		leaf.setParent(mid);

		// 叶 enable → mid enable → root enable
		leaf.setEnable(true);
		sys.refresh();
		assertTrue(mid.isEnable(), "中间节点 enable");
		assertTrue(root.isEnable(), "根节点 enable");

		// 叶 disable → mid disable → root disable
		leaf.setEnable(false);
		sys.refresh();
		assertFalse(mid.isEnable(), "中间节点 disable");
		assertFalse(root.isEnable(), "根节点 disable");

		sys.destroyRedPoint(root);
	}

	// ═════════════════════════════════════════════════════════════════
	// 叶销毁后父节点重新计算: 销毁 enable 的叶, 若其余叶都 disable 则父 disable
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyLeafRecomputesParent()
	{
		var sys = CreateSystem();
		RedPoint root = CreateParent(sys);
		var leafA = CreateLeaf(sys);
		var leafB = CreateLeaf(sys);
		leafA.setParent(root);
		leafB.setParent(root);

		leafA.setEnable(true);
		leafB.setEnable(false);
		sys.refresh();
		assertTrue(root.isEnable(), "leafA enable 根 enable");

		// 销毁 leafA(唯一 enable 的叶) → 根 refresh 后 disable
		sys.destroyRedPoint(leafA);
		assertFalse(root.isEnable(), "销毁唯一 enable 叶后根 disable");

		sys.destroyRedPoint(root);
	}

	// ═════════════════════════════════════════════════════════════════
	// 子树销毁: 销毁根节点级联销毁所有子孙, 列表清空
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroySubtreeCascades()
	{
		var sys = CreateSystem();
		RedPoint root = CreateParent(sys);
		RedPoint mid = CreateParent(sys);
		var leaf1 = CreateLeaf(sys);
		var leaf2 = CreateLeaf(sys);
		mid.setParent(root);
		leaf1.setParent(mid);
		leaf2.setParent(mid);

		// 销毁整棵子树(根)
		sys.destroyRedPoint(root);

		// 验证子节点已被级联销毁: 从父节点列表移除 + 从系统列表移除
		assertEqual(0, root.getChildCount(), "根的子节点已清空");
		// 重新挂载验证对象已失效(父节点已被销毁)
		assertNull(root.getParent(), "根父节点为 null");
	}

	// ═════════════════════════════════════════════════════════════════
	// update 驱动: 事件触发 dirty → update 只刷新 dirty 叶 → 级联向上刷新父链
	// ═════════════════════════════════════════════════════════════════
	private static void testUpdateDrivenDirtyRefresh()
	{
		var sys = CreateSystem();
		RedPoint root = CreateParent(sys);
		var leaf = CreateLeaf(sys);
		var eventLeaf = sys.createRedPoint<TestEventRedPoint>();
		leaf.setParent(root);
		eventLeaf.setParent(root);

		// 初始: 事件叶 enable, 普通叶 disable → 根 enable
		eventLeaf.setEnable(true);
		sys.refresh();
		assertTrue(root.isEnable(), "根 enable");

		// 触发事件 → 事件叶 dirty
		eventLeaf.triggerEvent();
		assertTrue(eventLeaf.isDirty(), "事件触发后叶 dirty");

		// 手动将事件叶改为 disable, 模拟数据变化, 但尚未 refresh
		eventLeaf.setEnable(false);

		// update 驱动刷新: 只刷新 dirty 叶, 并级联向上刷新父链
		sys.update(0f);
		assertFalse(eventLeaf.isDirty(), "update 后 dirty 清除");
		// 事件叶 disable + 普通叶 disable → 根 disable
		assertFalse(root.isEnable(), "update 级联刷新后根 disable");

		sys.destroyRedPoint(root);
	}

	// ═════════════════════════════════════════════════════════════════
	// count 边界: RedPointCount setCount 直接驱动自身 enable
	// 父节点 refresh 只读子 enable, 不递归刷新子 → 传播正确
	// (注意: RedPointCount 未 override refresh, 若调用 sys.refresh() 会把它按"无子节点"强制 disable)
	// ═════════════════════════════════════════════════════════════════
	private static void testCountZeroDisablesParentChain()
	{
		var sys = CreateSystem();
		RedPoint root = CreateParent(sys);
		RedPointCount count = sys.createRedPoint<RedPointCount>();
		count.setParent(root);

		// count>0 → 自身 enable, 父节点 refresh(只读子 enable) → 根 enable
		count.setCount(3);
		assertTrue(count.isEnable(), "count>0 叶 enable");
		root.refresh();
		assertTrue(root.isEnable(), "根 enable(父只读子 enable)");

		// count=0 → 自身 disable, 父 refresh → 根 disable
		count.setCount(0);
		assertFalse(count.isEnable(), "count=0 叶 disable");
		root.refresh();
		assertFalse(root.isEnable(), "根 disable");

		sys.destroyRedPoint(root);
	}

	// ═════════════════════════════════════════════════════════════════
	// 动态重挂: 把叶节点从树A移到树B, 两棵树状态都重新计算
	// ═════════════════════════════════════════════════════════════════
	private static void testReparentRecomputesState()
	{
		var sys = CreateSystem();
		RedPoint rootA = CreateParent(sys);
		RedPoint rootB = CreateParent(sys);
		var leaf = CreateLeaf(sys);
		leaf.setParent(rootA);

		// leaf enable → rootA enable, rootB disable
		leaf.setEnable(true);
		sys.refresh();
		assertTrue(rootA.isEnable(), "rootA enable");
		assertFalse(rootB.isEnable(), "rootB disable");

		// 把 leaf 从 rootA 移到 rootB
		leaf.setParent(rootB);
		sys.refresh();
		// rootA 失去 leaf → disable
		assertFalse(rootA.isEnable(), "rootA 失去 leaf 后 disable");
		// rootB 获得 leaf → enable
		assertTrue(rootB.isEnable(), "rootB 获得 leaf 后 enable");

		sys.destroyRedPoint(rootA);
		sys.destroyRedPoint(rootB);
	}

	// ═════════════════════════════════════════════════════════════════
	// 全量刷新: RedPointSystem.refresh() 递归刷新整棵树, 状态一致
	// ═════════════════════════════════════════════════════════════════
	private static void testFullRefreshRecomputesTree()
	{
		var sys = CreateSystem();
		RedPoint root = CreateParent(sys);
		RedPoint mid1 = CreateParent(sys);
		RedPoint mid2 = CreateParent(sys);
		var leafA = CreateLeaf(sys);
		var leafB = CreateLeaf(sys);
		mid1.setParent(root);
		mid2.setParent(root);
		leafA.setParent(mid1);
		leafB.setParent(mid2);

		// 设置部分叶 enable, 全量刷新后状态正确
		leafA.setEnable(true);
		leafB.setEnable(false);
		sys.refresh();
		assertTrue(mid1.isEnable(), "mid1 enable(因 leafA)");
		assertFalse(mid2.isEnable(), "mid2 disable(leafB disable)");
		assertTrue(root.isEnable(), "root enable(因 mid1)");

		// 反转: leafA disable, leafB enable
		leafA.setEnable(false);
		leafB.setEnable(true);
		sys.refresh();
		assertFalse(mid1.isEnable(), "mid1 disable");
		assertTrue(mid2.isEnable(), "mid2 enable");
		assertTrue(root.isEnable(), "root enable(因 mid2)");

		sys.destroyRedPoint(root);
	}

	// 测试用 IUGUIText mock: 记录 setText 收到的内容
	private class TestUGUIText : IUGUIText
	{
		public string mLastText;
		public void setText(string text) { mLastText = text; }
		public void setText(int text) { mLastText = text.ToString(); }
		public void setText(long text) { mLastText = text.ToString(); }
		public T tryGetUnityComponent<T>() where T : Component { return null; }
		public string getName() { return "RedPointDeepText"; }
	}

	// ═════════════════════════════════════════════════════════════════
	// count 与 UI 同步: setCount 更新绑定的数字 UI 文本 + 激活状态
	// ═════════════════════════════════════════════════════════════════
	private static void testCountUISync()
	{
		var sys = CreateSystem();
		RedPointCount count = sys.createRedPoint<RedPointCount>();
		var ui = CreateUI();
		var text = new TestUGUIText();

		// 绑定 UI 和文本
		count.setPointUI(ui, text);

		// 绑定后 UI 激活状态跟随 count(默认0 → disable), 文本为0
		assertFalse(ui.isActive(), "绑定后 count=0 UI 不激活");
		assertEqual("0", text.mLastText, "绑定后文本显示0");

		// setCount(5) → UI 激活 + 文本更新
		count.setCount(5);
		assertTrue(ui.isActive(), "count>0 UI 激活");
		assertEqual("5", text.mLastText, "文本显示5");

		// setCount(0) → UI 不激活 + 文本更新
		count.setCount(0);
		assertFalse(ui.isActive(), "count=0 UI 不激活");
		assertEqual("0", text.mLastText, "文本显示0");

		sys.destroyRedPoint(count);
		DestroyUI(ui);
	}
}
