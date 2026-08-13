using UnityEngine;
using static TestAssert;
using static FrameDefine;

// WindowObjectBase / WindowObjectT / WindowObjectRecyclableT 生命周期深度测试
// 覆盖 LayoutScriptTest / LayoutScriptDeepTest 未测的窗口对象体系:
//   init/postInit/reset 调用链, onShow/onHide 级联(父子窗口), setActive 三态切换
//   close() / destroy() 级联销毁, reassignParent 三种 parent 类型(LayoutScript/WindowObjectBase/WindowStructPoolBase)
//   isRootWindowObject / getScript 归属, WindowObjectT.setActive 的 changePositionAsInvisible 移动隐藏模式
//   WindowObjectT.assignWindow 三种重载 / isValid / isActive / isActiveSelf / setSibling / setParent
//   WindowObjectRecyclableT.recycle / setAssignID / getAssignID
//   addLocalizationObject / addWindowStructPool / addWindowPool 注册
//
// 环境: TestLayoutScriptDeep + setLayout(裸 GameLayout) + setRoot(myUGUICanvas, 预加 Canvas)
// 清理: window.destroy() 不销毁 mRoot(非 clone), 手动 destroyObject(ref root, true) + DestroyImmediate(rootGo)
public static class WindowObjectTest
{
	public static void Run()
	{
		testInitPostInit();
		testResetFlags();
		testShowHideChain();
		testCloseChain();
		testDestroyChain();
		testChildOnShowOnHideCascade();
		testReassignParentToLayout();
		testReassignParentToWindowObject();
		testReassignParentToStructPool();
		testIsRootWindowObject();
		testChangePositionAsInvisible();
		testAssignWindowByItemRoot();
		testIsActiveAndVisible();
		testSiblingOperation();
		testSetParent();
		testRecyclableAssignID();
		testLocalizationObject();
		testAddStructPoolAndWindowPool();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 TestLayoutScriptDeep + 根节点(myUGUICanvas)
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScript(out GameObject rootGo)
	{
		rootGo = new GameObject("TestWindowObjectRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		return script;
	}

	// 销毁独立 UI 对象(非池内, 可立即销毁)
	private static void destroyUI(ref myUGUIObject ui)
	{
		if (ui != null)
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// init / postInit: 只调用一次(重复 init 触发 logError 不测), 之后 mInited=true
	// ═════════════════════════════════════════════════════════════════
	private static void testInitPostInit()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			assertFalse(win.getInited(), "init 前 mInited=false");
			win.init();
			assertTrue(win.getInited(), "init 后 mInited=true");
			assertTrue(win.isValid(), "assignWindow 后 isValid");
			// postInit: 无池对象时 mUnuseAllWhenHide 保持默认 true
			win.postInit();
			assertTrue(win.getUnuseAllWhenHide(), "无使用中的池时 mUnuseAllWhenHide 保持 true");
			assertEqual(1, win.initCount, "init 只调用一次");
		}
		finally
		{
			destroyUI(ref itemRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// reset: 重置 mCalledOnHide/mCalledOnShow 标志, 使 onShow/onHide 可再次调用
	private static void testResetFlags()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			win.init();
			// 先走一轮 show/hide(root 初始 active, 首次 setActive(false) 触发 onHide)
			win.setActive(false);
			assertEqual(1, win.hideCount, "首次 setActive(false) 触发 onHide");
			win.setActive(true);
			assertEqual(1, win.showCount, "setActive(true) 触发 onShow");
			// reset 后标志清除, 可再次 show/hide(不触发 logError)
			win.reset();
			assertEqual(1, win.resetCount, "reset 被调用");
			win.setActive(false);
			assertEqual(2, win.hideCount, "reset 后 onHide 可再次触发");
			win.setActive(true);
			assertEqual(2, win.showCount, "reset 后 onShow 可再次触发");
		}
		finally
		{
			destroyUI(ref itemRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setActive 三态切换: true→false→true→true 幂等
	private static void testShowHideChain()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			win.init();
			// 首次 setActive(true): root 本就 active, 状态无变化不触发回调
			win.setActive(true);
			assertEqual(0, win.showCount, "root 已 active 时 setActive(true) 不触发 onShow");
			win.setActive(false);
			assertEqual(1, win.hideCount, "setActive(false) 触发 onHide");
			assertFalse(win.isActiveSelf(), "setActive(false) 后 isActiveSelf=false");
			win.setActive(true);
			assertEqual(1, win.showCount, "setActive(true) 触发 onShow");
			assertTrue(win.isActiveSelf(), "setActive(true) 后 isActiveSelf=true");
			// 幂等: 再次 setActive(true) 无变化
			win.setActive(true);
			assertEqual(1, win.showCount, "重复 setActive(true) 不重复触发 onShow");
		}
		finally
		{
			destroyUI(ref itemRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// close(): 等价于 setActive(false)
	private static void testCloseChain()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			win.init();
			win.close();
			assertEqual(1, win.hideCount, "close() 触发 onHide");
			assertFalse(win.isActiveSelf(), "close() 后 isActiveSelf=false");
		}
		finally
		{
			destroyUI(ref itemRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// destroy(): 自身 + 所有子节点级联销毁(遍历 mChildList)
	private static void testDestroyChain()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject parentRoot = null;
		myUGUIObject childRoot = null;
		try
		{
			parentRoot = script.createUGUIObject<myUGUIObject>(null, "ParentRoot", true);
			TestWindowObjectUGUI parent = new TestWindowObjectUGUI(script);
			parent.assignWindow(parentRoot);
			parent.init();
			childRoot = script.createUGUIObject<myUGUIObject>(null, "ChildRoot", true);
			TestWindowObjectUGUI child = new TestWindowObjectUGUI(parent);
			child.assignWindow(childRoot);
			child.init();
			assertEqual(1, parent.getChildCount(), "父窗口持有 1 个子节点");
			assertTrue(parent.isRootWindowObject(), "父窗口是根窗口");
			assertFalse(child.isRootWindowObject(), "子窗口不是根窗口");
			parent.destroy();
			assertTrue(parent.getHasDestroy(), "父窗口 destroy 后 mHasDestroy=true");
			assertTrue(child.getHasDestroy(), "子窗口被父窗口级联 destroy");
			assertEqual(1, parent.destroyCount, "父窗口 destroy 回调");
			assertEqual(1, child.destroyCount, "子窗口 destroy 回调");
		}
		finally
		{
			destroyUI(ref parentRoot);
			destroyUI(ref childRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onShow/onHide 级联: 父窗口 show/hide 时 active 的子窗口同步回调
	private static void testChildOnShowOnHideCascade()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject parentRoot = null;
		myUGUIObject childRoot = null;
		try
		{
			parentRoot = script.createUGUIObject<myUGUIObject>(null, "ParentRoot", true);
			TestWindowObjectUGUI parent = new TestWindowObjectUGUI(script);
			parent.assignWindow(parentRoot);
			parent.init();
			childRoot = script.createUGUIObject<myUGUIObject>(null, "ChildRoot", true);
			TestWindowObjectUGUI child = new TestWindowObjectUGUI(parent);
			child.assignWindow(childRoot);
			child.init();
			// 父隐藏: 子窗口 active → 级联 onHide
			parent.setActive(false);
			assertEqual(1, parent.hideCount, "父窗口 onHide");
			assertEqual(1, child.hideCount, "子窗口级联 onHide");
			// 父显示: 子窗口级联 onShow
			parent.setActive(true);
			assertEqual(1, parent.showCount, "父窗口 onShow");
			assertEqual(1, child.showCount, "子窗口级联 onShow");
		}
		finally
		{
			destroyUI(ref parentRoot);
			destroyUI(ref childRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// reassignParent(LayoutScript): 构造时传 script, mScript 绑定 + 注册到 script
	private static void testReassignParentToLayout()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			assertEqual(script, win.getScript(), "reassignParent(LayoutScript) 绑定 mScript");
			assertNull(win.getParentObject(), "reassignParent(LayoutScript) 不设置 mParent");
		}
		finally
		{
			destroyUI(ref itemRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// reassignParent(WindowObjectBase): 嵌套窗口, mScript 继承父 + 加入父的 mChildList
	private static void testReassignParentToWindowObject()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject parentRoot = null;
		myUGUIObject childRoot = null;
		try
		{
			parentRoot = script.createUGUIObject<myUGUIObject>(null, "ParentRoot", true);
			TestWindowObjectUGUI parent = new TestWindowObjectUGUI(script);
			parent.assignWindow(parentRoot);
			parent.init();
			childRoot = script.createUGUIObject<myUGUIObject>(null, "ChildRoot", true);
			TestWindowObjectUGUI child = new TestWindowObjectUGUI(parent);
			child.assignWindow(childRoot);
			assertEqual(script, child.getScript(), "子窗口 mScript 继承父窗口");
			assertEqual(parent, child.getParentObject(), "子窗口 mParent=父窗口");
			assertEqual(1, parent.getChildCount(), "父窗口 mChildList 加入子窗口");
		}
		finally
		{
			destroyUI(ref parentRoot);
			destroyUI(ref childRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// reassignParent(WindowStructPoolBase): mParentPool 绑定, mScript 从池获取
	private static void testReassignParentToStructPool()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject templateRoot = null;
		try
		{
			templateRoot = script.createUGUIObject<myUGUIObject>(null, "TemplateRoot", true);
			WindowStructPool<TestRecyclableWindow> pool = new WindowStructPool<TestRecyclableWindow>(script);
			pool.assignTemplate(templateRoot);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(pool);
			assertEqual(script, win.getScript(), "reassignParent(WindowStructPoolBase) 从池获取 mScript");
			assertEqual(pool, win.getParentPool(), "reassignParent(WindowStructPoolBase) 绑定 mParentPool");
			assertNull(win.getParentObject(), "池的 owner 为 script 时 mParent=null");
		}
		finally
		{
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// isRootWindowObject: parent 为 null 才是根窗口
	private static void testIsRootWindowObject()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			assertTrue(win.isRootWindowObject(), "parent 为 null 时是根窗口");
		}
		finally
		{
			destroyUI(ref itemRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// changePositionAsInvisible: setActive(false) 用移动 FAR_POSITION 代替隐藏
	private static void testChangePositionAsInvisible()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			win.init();
			win.setChangePositionAsInvisible(true);
			win.setActive(false);
			// 移动隐藏: root 位置变为 FAR_POSITION, 且 onHide 触发
			assertTrue(itemRoot.getPosition().isEqual(FAR_POSITION), "移动隐藏后位置=FAR_POSITION");
			assertEqual(1, win.hideCount, "移动隐藏也触发 onHide");
			assertTrue(itemRoot.isActive(), "移动隐藏不改变 GameObject 的 active 状态");
			// 再显示: 位置不自动恢复(文档化真实行为), 仅触发 onShow
			win.setActive(true);
			assertEqual(1, win.showCount, "移动隐藏模式下 setActive(true) 触发 onShow");
		}
		finally
		{
			destroyUI(ref itemRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// assignWindow(itemRoot): 直接用现有 UI 对象作为 root
	private static void testAssignWindowByItemRoot()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			assertTrue(win.isValid(), "assignWindow(itemRoot) 后 isValid");
			assertEqual(itemRoot, win.getRoot(), "mRoot=传入的 itemRoot");
		}
		finally
		{
			destroyUI(ref itemRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// isActive / isActiveSelf / isVisible: 依赖 root 的 active 状态
	private static void testIsActiveAndVisible()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			win.init();
			assertTrue(win.isActive(), "root active 时 isActive=true");
			assertTrue(win.isActiveSelf(), "root active 时 isActiveSelf=true");
			assertTrue(win.isVisible(), "root active 时 isVisible=true");
			win.setActive(false);
			assertFalse(win.isActive(), "root inactive 时 isActive=false");
			assertFalse(win.isActiveSelf(), "root inactive 时 isActiveSelf=false");
			assertFalse(win.isVisible(), "root inactive 时 isVisible=false");
		}
		finally
		{
			destroyUI(ref itemRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// getSibling / setSibling / setAsFirstSibling / setAsLastSibling: 依赖真实 Transform
	private static void testSiblingOperation()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		myUGUIObject itemRoot2 = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			itemRoot2 = script.createUGUIObject<myUGUIObject>(null, "ItemRoot2", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			// 初始: itemRoot 是 rootGo 下第一个子节点
			assertEqual(0, win.getSibling(), "第一个子节点 sibling=0");
			assertTrue(win.setSibling(1, false), "setSibling(1) 返回 true 表示位置变化");
			assertEqual(1, win.getSibling(), "setSibling(1) 后 sibling=1");
			assertFalse(win.setSibling(1, false), "setSibling(相同位置) 返回 false");
			win.setAsFirstSibling(false);
			assertEqual(0, win.getSibling(), "setAsFirstSibling 后 sibling=0");
			win.setAsLastSibling(false);
			assertEqual(1, win.getSibling(), "2 个子节点时 setAsLastSibling 后 sibling=1");
		}
		finally
		{
			destroyUI(ref itemRoot);
			destroyUI(ref itemRoot2);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setParent: 将 root 移到指定父节点下
	private static void testSetParent()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		myUGUIObject parentRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			parentRoot = script.createUGUIObject<myUGUIObject>(null, "ParentRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			win.setParent(parentRoot, false);
			assertEqual(parentRoot.getGameObject().transform, itemRoot.getGameObject().transform.parent, "setParent 后 root 的 Transform 父节点变化");
		}
		finally
		{
			destroyUI(ref itemRoot);
			destroyUI(ref parentRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// WindowObjectRecyclableT: recycle 重置 assignID, setAssignID/getAssignID
	private static void testRecyclableAssignID()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			TestRecyclableWindow win = new TestRecyclableWindow(script);
			win.assignWindow(itemRoot);
			win.setAssignID(10);
			assertEqual(10L, win.getAssignID(), "setAssignID(10) 后 getAssignID=10");
			win.recycle();
			assertEqual(-1L, win.getAssignID(), "recycle 后 assignID 重置为 -1");
		}
		finally
		{
			destroyUI(ref itemRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// addLocalizationObject: 注册本地化对象, destroy 时注销
	// 注意: IUGUIObject 由 myUGUIText/myUGUIImage 等实现, myUGUIObject 本身不实现
	private static void testLocalizationObject()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUITextAuto textRoot = null;
		try
		{
			textRoot = script.createUGUIObject<myUGUITextAuto>(null, "TextRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(textRoot);
			win.init();
			win.addLocalizationObject(textRoot);
			assertEqual(1, win.getLocalizationCount(), "addLocalizationObject 后计数=1");
			win.destroy();
			assertEqual(0, win.getLocalizationCount(), "destroy 后本地化列表清空");
		}
		finally
		{
			myUGUIObject temp = textRoot;
			destroyUI(ref temp);
			textRoot = null;
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// addWindowStructPool / addWindowPool: 重复添加触发 logError 不测, 只测首次添加成功
	private static void testAddStructPoolAndWindowPool()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject itemRoot = null;
		myUGUIObject templateRoot = null;
		try
		{
			itemRoot = script.createUGUIObject<myUGUIObject>(null, "ItemRoot", true);
			templateRoot = script.createUGUIObject<myUGUIObject>(null, "TemplateRoot", true);
			TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
			win.assignWindow(itemRoot);
			win.init();
			WindowStructPool<TestRecyclableWindow> structPool = new WindowStructPool<TestRecyclableWindow>(win);
			WindowPool<myUGUIObject> windowPool = new WindowPool<myUGUIObject>(win);
			// 池的 owner 是 win, 池的 mScript 继承自 win
			assertEqual(script, structPool.getLayoutScript(), "结构池 mScript 继承自 owner 窗口");
			assertEqual(1, win.getStructPoolCount(), "win 持有 1 个结构池");
			assertEqual(1, win.getWindowPoolCount(), "win 持有 1 个窗口池");
			assertFalse(structPool.isRootPool(), "结构池 owner 是窗口, 非根池");
			assertFalse(windowPool.isRootPool(), "窗口池 owner 是窗口, 非根池");
		}
		finally
		{
			destroyUI(ref itemRoot);
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 可记录生命周期回调的 WindowObjectUGUI 子类
// ═════════════════════════════════════════════════════════════════
public class TestWindowObjectUGUI : WindowObjectUGUI
{
	public int initCount;
	public int resetCount;
	public int showCount;
	public int hideCount;
	public int destroyCount;
	public TestWindowObjectUGUI(IWindowObjectOwner parent) : base(parent) { }
	protected override void assignWindowInternal() { }
	public override void init()
	{
		base.init();
		initCount++;
	}
	public override void reset()
	{
		base.reset();
		resetCount++;
	}
	public override void onShow()
	{
		base.onShow();
		showCount++;
	}
	public override void onHide()
	{
		base.onHide();
		hideCount++;
	}
	public override void destroy()
	{
		base.destroy();
		destroyCount++;
	}
	// getter 暴露 protected 状态, 便于断言
	public bool getInited() { return mInited; }
	public bool getHasDestroy() { return mHasDestroy; }
	public bool getUnuseAllWhenHide() { return mUnuseAllWhenHide; }
	public int getChildCount() { return mChildList.count(); }
	public int getLocalizationCount() { return mLocalizationObjectList.count(); }
	public int getStructPoolCount() { return mPoolList.count(); }
	public int getWindowPoolCount() { return mWindowPoolList.count(); }
	public WindowObjectBase getParentObject() { return mParent; }
	public WindowStructPoolBase getParentPool() { return mParentPool; }
	public void setChangePositionAsInvisible(bool value) { mChangePositionAsInvisible = value; }
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 可回收窗口对象(用于 WindowStructPool 泛型参数)
// ═════════════════════════════════════════════════════════════════
public class TestRecyclableWindow : WindowRecyclableUGUI
{
	public TestRecyclableWindow(IWindowObjectOwner parent) : base(parent) { }
	protected override void assignWindowInternal() { }
}
