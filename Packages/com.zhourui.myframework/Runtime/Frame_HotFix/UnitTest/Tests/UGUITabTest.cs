using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// UGUITab / LegendButton 深度测试
// 覆盖 UGUIControlDeepTest 未测的窗口对象型控件:
//   UGUITab: assignWindow 绑定 Normal/Selected/NormalText/SelectedText 子节点
//            init(注册 mButton 碰撞 + 默认未选中), setSelected 切换选中态
//            setInteractable 拦截点击, onClick 触发回调(不响应时跳过)
//   LegendButton: assignWindow 绑定 Gray/Text, init(记录原始位置/颜色)
//                 setGray 显示/隐藏灰色遮罩, setText 设置文本, reset 恢复原始状态
//                 setHandleInput / registeCollider / unregisteCollider
//
// 环境: TestLayoutScriptDeep + setLayout(裸 GameLayout) + setRoot(myUGUICanvas, 预加 Canvas)
// 节点树: TabRoot(独立, createUGUIObject 注册) + 裸 GameObject 子节点
//         (Normal/Selected/NormalText/SelectedText/Gray/Text, 由控件内部 newObject 绑定)
// 注意: 子节点必须用裸 GameObject 创建(不预先注册 layout),
//       否则 UGUITab/LegendButton 内部 newObject 会命中"已创建相同GameObject"的 logError
// 清理: tab.destroy() 释放 C# 窗口对象, destroyAllUI 销毁根节点(注销碰撞), rootGo DestroyImmediate
public static class UGUITabTest
{
	public static void Run()
	{
		testTabAssignWindowAndInit();
		testTabSetSelected();
		testTabSetInteractable();
		testTabClickCallback();
		testLegendAssignWindowAndInit();
		testLegendSetGray();
		testLegendSetText();
		testLegendReset();
		testLegendRegisteCollider();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 script + 节点树(TabRoot 用 createUGUIObject 注册, 子节点用裸 GameObject)
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScriptAndTree(out GameObject rootGo, out myUGUIObject tabRoot)
	{
		rootGo = new GameObject("TestTabRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		tabRoot = script.createUGUIObject<myUGUIObject>(null, "TabRoot", true);
		// 子节点: 裸 GameObject(不注册 layout), 由控件内部 newObject 绑定
		createBareChild(tabRoot, "Normal", true);
		createBareChild(tabRoot, "Selected", false);
		createBareChild(tabRoot, "NormalText", true);
		createBareChild(tabRoot, "SelectedText", false);
		createBareChild(tabRoot, "Gray", false);
		// Text 节点需要 Text 组件, myUGUITextAuto 才能 setText 生效
		createBareChild(tabRoot, "Text", false, true);
		return script;
	}

	private static void createBareChild(myUGUIObject parent, string name, bool active, bool withTextComponent = false)
	{
		GameObject go = new GameObject(name);
		go.AddComponent<RectTransform>();
		if (withTextComponent)
		{
			go.AddComponent<Text>();
		}
		go.transform.SetParent(parent.getGameObject().transform, false);
		go.SetActive(active);
	}

	// ═════════════════════════════════════════════════════════════════
	// UGUITab: assignWindow 绑定子节点 + init(默认未选中 + 注册碰撞)
	// ═════════════════════════════════════════════════════════════════
	private static void testTabAssignWindowAndInit()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject tabRoot);
		TestTabUGUI tab = null;
		try
		{
			tab = new TestTabUGUI(script);
			tab.assignWindow(tabRoot);
			tab.init();
			assertFalse(tab.isSelected(), "init 后默认未选中");
			// 默认选中态: Normal 显示, Selected 隐藏
			assertTrue(tab.getNormalActive(), "默认 Normal 激活");
			assertFalse(tab.getSelectedActive(), "默认 Selected 隐藏");
			// mButton 已绑定 mRoot
			assertNotNull(tab.getButton(), "LegendButton 已创建");
		}
		finally
		{
			tab?.destroy();
			destroyUI(ref tabRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setSelected: 切换 Normal/Selected 节点激活状态
	private static void testTabSetSelected()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject tabRoot);
		TestTabUGUI tab = null;
		try
		{
			tab = new TestTabUGUI(script);
			tab.assignWindow(tabRoot);
			tab.init();
			tab.setSelected(true);
			assertTrue(tab.isSelected(), "setSelected(true) 后 isSelected=true");
			assertFalse(tab.getNormalActive(), "选中后 Normal 隐藏");
			assertTrue(tab.getSelectedActive(), "选中后 Selected 激活");
			tab.setSelected(false);
			assertFalse(tab.isSelected(), "setSelected(false) 后 isSelected=false");
			assertTrue(tab.getNormalActive(), "取消选中后 Normal 激活");
			assertFalse(tab.getSelectedActive(), "取消选中后 Selected 隐藏");
		}
		finally
		{
			tab?.destroy();
			destroyUI(ref tabRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setInteractable: 不响应点击时 onClick 不触发回调
	private static void testTabSetInteractable()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject tabRoot);
		TestTabUGUI tab = null;
		try
		{
			tab = new TestTabUGUI(script);
			tab.assignWindow(tabRoot);
			tab.init();
			int callbackCount = 0;
			tab.setCallback(() => callbackCount++);
			tab.setInteractable(false);
			tab.click();
			assertEqual(0, callbackCount, "不可交互时 onClick 不触发回调");
			tab.setInteractable(true);
			tab.click();
			assertEqual(1, callbackCount, "可交互时 onClick 触发回调");
			assertTrue(tab.isInteractable(), "isInteractable=true");
		}
		finally
		{
			tab?.destroy();
			destroyUI(ref tabRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onClick 直接触发回调(不经过碰撞系统)
	private static void testTabClickCallback()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject tabRoot);
		TestTabUGUI tab = null;
		try
		{
			tab = new TestTabUGUI(script);
			tab.assignWindow(tabRoot);
			tab.init();
			bool callbackCalled = false;
			tab.setCallback(() => callbackCalled = true);
			tab.click();
			assertTrue(callbackCalled, "onClick 触发 setCallback 注册的回调");
		}
		finally
		{
			tab?.destroy();
			destroyUI(ref tabRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// LegendButton: assignWindow 绑定 Gray/Text + init 记录原始属性
	// ═════════════════════════════════════════════════════════════════
	private static void testLegendAssignWindowAndInit()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject tabRoot);
		LegendButton button = null;
		try
		{
			button = new LegendButton(script);
			button.assignWindow(tabRoot);
			button.init();
			assertNotNull(button.getTextObject(), "LegendButton 绑定 Text 节点");
			// init 记录原始位置/颜色
			button.initOriginProperty();
			button.reset();
			// setText 后可读回
			button.setText("Legend");
			assertEqual("Legend", button.getTextObject().getText(), "setText 后文本正确");
		}
		finally
		{
			button?.destroy();
			destroyUI(ref tabRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setGray: 显示/隐藏灰色遮罩
	private static void testLegendSetGray()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject tabRoot);
		LegendButton button = null;
		try
		{
			button = new LegendButton(script);
			button.assignWindow(tabRoot);
			button.init();
			button.setGray(true);
			// Gray 节点激活
			assertTrue(findChildByName(script, tabRoot, "Gray").isActive(), "setGray(true) 后 Gray 激活");
			button.setGray(false);
			assertFalse(findChildByName(script, tabRoot, "Gray").isActive(), "setGray(false) 后 Gray 隐藏");
		}
		finally
		{
			button?.destroy();
			destroyUI(ref tabRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setText: 设置文本(字符串与整数)
	private static void testLegendSetText()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject tabRoot);
		LegendButton button = null;
		try
		{
			button = new LegendButton(script);
			button.assignWindow(tabRoot);
			button.init();
			button.setText("Hello");
			assertEqual("Hello", button.getTextObject().getText(), "setText(string) 文本正确");
			button.setText(123);
			assertEqual("123", button.getTextObject().getText(), "setText(int) 文本正确");
		}
		finally
		{
			button?.destroy();
			destroyUI(ref tabRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// reset: 恢复文本原始位置/颜色
	private static void testLegendReset()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject tabRoot);
		LegendButton button = null;
		try
		{
			button = new LegendButton(script);
			button.assignWindow(tabRoot);
			button.init();
			button.initOriginProperty();
			// 修改文本位置后 reset 应恢复
			button.getTextObject().setPosition(new Vector3(10.0f, 10.0f, 0.0f));
			button.reset();
			Vector3 pos = button.getTextObject().getPosition();
			assertEqual(0.0f, pos.x, 0.001f, "reset 后文本位置 x 恢复为 0");
			assertEqual(0.0f, pos.y, 0.001f, "reset 后文本位置 y 恢复为 0");
		}
		finally
		{
			button?.destroy();
			destroyUI(ref tabRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// registeCollider / unregisteCollider / setHandleInput
	private static void testLegendRegisteCollider()
	{
		TestLayoutScriptDeep script = createScriptAndTree(out GameObject rootGo, out myUGUIObject tabRoot);
		LegendButton button = null;
		try
		{
			button = new LegendButton(script);
			button.assignWindow(tabRoot);
			button.init();
			int clickCount = 0;
			button.registeCollider(() => clickCount++);
			button.unregisteCollider();
			button.setHandleInput(false);
			button.setHandleInput(true);
			button.setText("test");
			assertEqual("test", button.getTextObject().getText(), "registeCollider 后 setText 正常");
		}
		finally
		{
			button?.destroy();
			destroyUI(ref tabRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 查找子节点(通过 GameLayout 的注册表)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject findChildByName(TestLayoutScriptDeep script, myUGUIObject parent, string name)
	{
		Transform child = parent.getGameObject().transform.Find(name);
		if (child == null)
		{
			return null;
		}
		return script.getLayout().getUIObject(child.gameObject);
	}

	private static void destroyUI(ref myUGUIObject ui)
	{
		if (ui != null)
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 UGUITab 的 protected onClick(碰撞回调) 与子节点状态
// ═════════════════════════════════════════════════════════════════
public class TestTabUGUI : UGUITab
{
	public TestTabUGUI(IWindowObjectOwner parent) : base(parent) { }
	// 模拟点击(等价于碰撞系统回调 onClick)
	public void click() { onClick(); }
	public bool getNormalActive() { return mNormal.isActive(); }
	public bool getSelectedActive() { return mSelected.isActive(); }
	public LegendButton getButton() { return mButton; }
}
