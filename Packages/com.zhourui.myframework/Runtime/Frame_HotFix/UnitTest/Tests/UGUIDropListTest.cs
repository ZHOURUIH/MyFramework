using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// UGUIDropListBase 深度测试
// 覆盖下拉列表控件的核心逻辑:
//   assignWindow(节点绑定) + init(注册碰撞/隐藏下拉/Options 需要 Canvas)
//   setOptions: 创建选项 + 自动选中第 0 项 + 触发回调
//   setSelect: 选中切换(回调/文本/越界保护/triggerEvent 开关)
//   dropItemClick: 点击选项 → 选中 + 关闭下拉
//   showOptions: 展开/收起下拉(mOptions/mMask 激活状态)
//   onClick / onMaskClick: 点击标签打开 / 点击遮罩关闭
//   clearOptions: 重置选中下标
//
// 环境: TestLayoutScriptDeep + GameLayout + myUGUICanvas
// 节点: 测试子类重写 assignWindowInternal 用 createUGUIObject 绑定 5 节点
//       (绕开基类 newObject 的场景查找依赖, 只测基类逻辑)
// 清理: 节点用 destroyObject 销毁, rootGo 手动 DestroyImmediate
public static class UGUIDropListTest
{
	public static void Run()
	{
		testDropListInit();
		testDropListSetOptions();
		testDropListSetOptionsCustomValue();
		testDropListSetSelect();
		testDropListSetSelectCallback();
		testDropListDropItemClick();
		testDropListShowOptions();
		testDropListOnClickAndMask();
		testDropListClearOptions();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 script + 根节点 + 已 init 的下拉列表
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScript(out GameObject rootGo, out myUGUIObject dropRoot)
	{
		rootGo = new GameObject("TestDropListRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		GameLayout layout = new GameLayout();
		layout.setScript(script);   // myUGUIDragView.init 依赖 mLayout.getScript(), 需注入脚本
		script.setLayout(layout);
		script.setRoot(root);
		dropRoot = script.createUGUIObject<myUGUIObject>(null, "DropListRoot", true);
		return script;
	}

	private static void destroyAllUI(List<myUGUIObject> allUI)
	{
		if (allUI == null)
		{
			return;
		}
		for (int i = allUI.Count - 1; i >= 0; --i)
		{
			myUGUIObject ui = allUI[i];
			if (ui != null)
			{
				LayoutScript.destroyObject(ref ui, true);
			}
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// init: 节点绑定 + 下拉/遮罩初始隐藏 + Options 拥有 Canvas
	// ═════════════════════════════════════════════════════════════════
	private static void testDropListInit()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject dropRoot);
		TestDropList drop = null;
		try
		{
			drop = new TestDropList(script);
			drop.initForTest(dropRoot);
			assertFalse(drop.isOptionsActive(), "init 后下拉框隐藏");
			assertFalse(drop.isMaskActive(), "init 后遮罩隐藏");
			assertEqual(0, drop.getSelect(), "init 后选中下标 0");
			assertEqual("", drop.getSelectedText(), "init 后无选中文本");
		}
		finally
		{
			drop?.destroy();
			LayoutScript.destroyObject(ref dropRoot, true);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setOptions: 创建选项 + 自动选中第 0 项 + 触发回调
	private static void testDropListSetOptions()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject dropRoot);
		TestDropList drop = null;
		try
		{
			drop = new TestDropList(script);
			drop.initForTest(dropRoot);
			int callbackCount = 0;
			drop.setSelectCallback(() => callbackCount++);
			drop.setOptions(new List<string> { "A", "B", "C" });
			assertEqual(3, drop.getItems().Count, "setOptions 创建 3 个选项");
			assertEqual(0, drop.getSelect(), "setOptions 后自动选中第 0 项");
			assertEqual("A", drop.getSelectedText(), "选中第 0 项文本 A");
			assertEqual(1, callbackCount, "setOptions 默认触发回调(setSelect(0))");
			// 选项的文本与父引用
			assertEqual("B", drop.getItems()[1].getText(), "选项 1 文本 B");
		}
		finally
		{
			drop?.destroy();
			LayoutScript.destroyObject(ref dropRoot, true);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setOptions 带 customValue: 附加数据传递到选项
	private static void testDropListSetOptionsCustomValue()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject dropRoot);
		TestDropList drop = null;
		try
		{
			drop = new TestDropList(script);
			drop.initForTest(dropRoot);
			drop.setOptions(new List<string> { "A", "B" }, new List<int> { 10, 20 });
			assertEqual(10, drop.getItems()[0].getCustomValue(), "选项 0 附加数据 10");
			assertEqual(20, drop.getItems()[1].getCustomValue(), "选项 1 附加数据 20");
			assertEqual(10, drop.getSelectedCustomValue(), "选中第 0 项附加数据 10");
			// 数量不一致: logError 分支不测
		}
		finally
		{
			drop?.destroy();
			LayoutScript.destroyObject(ref dropRoot, true);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setSelect: 选中切换(回调/文本/越界保护/triggerEvent 开关)
	private static void testDropListSetSelect()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject dropRoot);
		TestDropList drop = null;
		try
		{
			drop = new TestDropList(script);
			drop.initForTest(dropRoot);
			drop.setOptions(new List<string> { "A", "B", "C" });
			int callbackCount = 1;   // setOptions 已触发一次
			drop.setSelectCallback(() => callbackCount++);
			drop.setSelect(1);
			assertEqual(1, drop.getSelect(), "setSelect(1) 后选中下标 1");
			assertEqual("B", drop.getSelectedText(), "选中文本 B");
			assertEqual(2, callbackCount, "setSelect(1) 触发回调");
			// triggerEvent=false: 不触发回调
			drop.setSelect(2, false);
			assertEqual(2, drop.getSelect(), "setSelect(2,false) 仍切换选中");
			assertEqual("C", drop.getSelectedText(), "选中文本 C");
			assertEqual(2, callbackCount, "triggerEvent=false 不触发回调");
			// 越界: getSelectByIndex 返回 null → 状态不变
			drop.setSelect(99);
			assertEqual(2, drop.getSelect(), "setSelect(99) 越界状态不变");
			assertEqual("C", drop.getSelectedText(), "越界后文本不变");
			drop.setSelect(-1);
			assertEqual(2, drop.getSelect(), "setSelect(-1) 越界状态不变");
		}
		finally
		{
			drop?.destroy();
			LayoutScript.destroyObject(ref dropRoot, true);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setSelectCallback: 回调设置/替换/null 安全
	private static void testDropListSetSelectCallback()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject dropRoot);
		TestDropList drop = null;
		try
		{
			drop = new TestDropList(script);
			drop.initForTest(dropRoot);
			drop.setOptions(new List<string> { "A" });
			// null 回调: setSelect 不崩溃
			drop.setSelectCallback(null);
			drop.setSelect(0);
			// 替换回调
			int count = 0;
			drop.setSelectCallback(() => count++);
			drop.setSelect(0);
			assertEqual(1, count, "替换后的回调触发");
		}
		finally
		{
			drop?.destroy();
			LayoutScript.destroyObject(ref dropRoot, true);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// dropItemClick: 点击选项 → 选中 + 关闭下拉
	private static void testDropListDropItemClick()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject dropRoot);
		TestDropList drop = null;
		try
		{
			drop = new TestDropList(script);
			drop.initForTest(dropRoot);
			drop.setOptions(new List<string> { "A", "B", "C" });
			drop.showOptions(true);
			assertTrue(drop.isOptionsActive(), "展开下拉");
			// 点击第 1 个选项
			drop.dropItemClick(drop.getItems()[1]);
			assertEqual(1, drop.getSelect(), "dropItemClick 选中对应项");
			assertEqual("B", drop.getSelectedText(), "dropItemClick 后选中文本 B");
			assertFalse(drop.isOptionsActive(), "dropItemClick 后下拉关闭");
			assertFalse(drop.isMaskActive(), "dropItemClick 后遮罩关闭");
		}
		finally
		{
			drop?.destroy();
			LayoutScript.destroyObject(ref dropRoot, true);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// showOptions: 展开/收起下拉(mOptions/mMask 激活状态)
	private static void testDropListShowOptions()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject dropRoot);
		TestDropList drop = null;
		try
		{
			drop = new TestDropList(script);
			drop.initForTest(dropRoot);
			drop.setOptions(new List<string> { "A" });
			drop.showOptions(true);
			assertTrue(drop.isOptionsActive(), "showOptions(true) 展开下拉");
			assertTrue(drop.isMaskActive(), "showOptions(true) 显示遮罩");
			drop.showOptions(false);
			assertFalse(drop.isOptionsActive(), "showOptions(false) 收起下拉");
			assertFalse(drop.isMaskActive(), "showOptions(false) 隐藏遮罩");
			// 重复收起: 幂等
			drop.showOptions(false);
			assertFalse(drop.isOptionsActive(), "重复收起保持隐藏");
		}
		finally
		{
			drop?.destroy();
			LayoutScript.destroyObject(ref dropRoot, true);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onClick / onMaskClick: 点击标签打开 / 点击遮罩关闭
	private static void testDropListOnClickAndMask()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject dropRoot);
		TestDropList drop = null;
		try
		{
			drop = new TestDropList(script);
			drop.initForTest(dropRoot);
			drop.setOptions(new List<string> { "A", "B" });
			assertFalse(drop.isOptionsActive(), "初始下拉隐藏");
			// onClick: 计算下拉框位置(不依赖 Text 组件, getSize/getPositionNoPivot 走 RectTransform) + 展开
			drop.clickLabel();
			assertTrue(drop.isOptionsActive(), "onClick 展开下拉");
			assertTrue(drop.isMaskActive(), "onClick 显示遮罩");
			// onMaskClick: 关闭
			drop.clickMask();
			assertFalse(drop.isOptionsActive(), "onMaskClick 收起下拉");
			assertFalse(drop.isMaskActive(), "onMaskClick 隐藏遮罩");
		}
		finally
		{
			drop?.destroy();
			LayoutScript.destroyObject(ref dropRoot, true);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// clearOptions: 重置选中下标
	private static void testDropListClearOptions()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject dropRoot);
		TestDropList drop = null;
		try
		{
			drop = new TestDropList(script);
			drop.initForTest(dropRoot);
			drop.setOptions(new List<string> { "A", "B" });
			drop.setSelect(1);
			assertEqual(1, drop.getSelect(), "选中第 1 项");
			drop.clearOptions();
			assertEqual(0, drop.getSelect(), "clearOptions 重置选中下标 0");
		}
		finally
		{
			drop?.destroy();
			LayoutScript.destroyObject(ref dropRoot, true);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: UGUIDropListBase 子类(绕开 newObject 场景查找, 绑定真实节点)
// ═════════════════════════════════════════════════════════════════
public class TestDropList : UGUIDropListBase
{
	private TestLayoutScriptDeep mScript;
	private List<TestDropItem> mItems = new();
	public TestDropList(TestLayoutScriptDeep script) : base(script)
	{
		mScript = script;
	}
	// 绕开基类 assignWindow/assignWindowInternal 流程:
	// BASE001 分析器要求 override assignWindowInternal 必须调 base, 但 base 依赖场景节点查找(newObject)无法在测试环境满足;
	// 手动搭建节点: 设置 mRoot + 创建 5 节点 + 补 Canvas + init, 不触发 assignWindowInternal
	public void initForTest(myUGUIObject dropRoot)
	{
		mRoot = dropRoot;
		mLabel = mScript.createUGUIObject<myUGUITextAuto>(null, "Label", true);
		mOptions = mScript.createUGUIObject<myUGUIObject>(null, "Options", true);
		mMask = mScript.createUGUIObject<myUGUIObject>(mOptions, "Mask", false);
		mViewport = mScript.createUGUIObject<myUGUIObject>(mOptions, "Viewport", true);
		mContent = mScript.createUGUIObject<myUGUIDragView>(mViewport, "Content", true);
		// init 要求 Options 节点拥有 Canvas 组件, 先补上再 init
		mOptions.getGameObject().AddComponent<Canvas>();
		init();
	}
	protected override void createAllItem(List<string> options, List<int> customValue)
	{
		mItems.Clear();
		int count = options.Count;
		for (int i = 0; i < count; ++i)
		{
			TestDropItem item = new TestDropItem();
			item.setText(options[i]);
			if (customValue != null)
			{
				item.setCustomValue(customValue[i]);
			}
			item.setParent(this);
			mItems.Add(item);
		}
	}
	protected override IDropItem getSelectByIndex(int index)
	{
		if (index < 0 || index >= mItems.Count)
		{
			return null;
		}
		return mItems[index];
	}
	protected override int getIndexOfItem(IDropItem item) { return mItems.IndexOf((TestDropItem)item); }
	// 暴露 protected 状态与点击链
	public bool isOptionsActive() { return mOptions.isActive(); }
	public bool isMaskActive() { return mMask.isActive(); }
	public void clickLabel() { onClick(); }
	public void clickMask() { onMaskClick(); }
	public List<TestDropItem> getItems() { return mItems; }
}
// ═════════════════════════════════════════════════════════════════
// 测试辅助: IDropItem 实现(纯逻辑项, 不绑定 UI 节点)
// ═════════════════════════════════════════════════════════════════
public class TestDropItem : IDropItem
{
	private string mText;
	private int mCustomValue;
	public string getText() { return mText; }
	public int getCustomValue() { return mCustomValue; }
	public void setText(string text) { mText = text; }
	public void setCustomValue(int value) { mCustomValue = value; }
	public void setParent(UGUIDropListBase parent) { }
}
