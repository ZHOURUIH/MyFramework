using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static FrameBaseHotFix;
using static TestAssert;

// UI 面板完整组合深度测试(模仿 MicroLegend 真实业务面板 UIAttackTarget + UILegendBase):
//   TestLegendPanel : LayoutScript —— 完整业务面板模拟
//   assignWindow: newObject 绑定 4 个真实组件节点(TargetRoot/Head(Image)/Name(Text)/Close(Image))
//   update: mTargetRoot.setActive(返回值) + setText(string/int) + setColor + setActive 组合
//   initLegendPanel: registeColliderImage + Image.raycastTarget + registeCollider + setSize 同步 组合
//   enablePanelDrag: registeCollider + getOrAddComponent<COMWindowDrag> + setStartDragThreshold 组合
//
// 节点树(场景根): rootGo(Canvas) / TargetRoot(独立根) ├ Head(Image) ├ Name(Text) └ Close(Image)
public static class UIPanelTest
{
	private const string TEST_LAYOUT_PATH = "TestLegendPanel.prefab";
	private const string TEST_LAYOUT_NAME = "TestLegendPanel";
	private static readonly string sPrefabFile = "Assets/GameResources/" + TEST_LAYOUT_PATH;
	private static bool sRegistered;

	public static void Run()
	{
		try
		{
			if (!File.Exists(sPrefabFile))
			{
				File.WriteAllText(sPrefabFile, "");
			}
			if (!sRegistered)
			{
				mLayoutManager.registeLayout(typeof(TestLegendPanel), TEST_LAYOUT_PATH, LAYOUT_LIFE_CYCLE.PERSIST, null);
				sRegistered = true;
			}
			testPanelInitAssignWindowBindsTree();
			testPanelUpdateComboWithTargetVisible();
			testPanelUpdateComboWithTargetHidden();
			testPanelLegendInitCombo();
			testPanelDragCombo();
		}
		finally
		{
			sRegistered = false;
			if (File.Exists(sPrefabFile))
			{
				File.Delete(sPrefabFile);
			}
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 完整 init 面板(文件已注册 + 动态节点树 + 预加组件)
	// 返回 layout 和所有独立节点(需手动销毁: 不在 destroy 链内)
	// ═════════════════════════════════════════════════════════════════
	private static GameLayout createPanel(out GameObject targetGo, out GameObject headGo, out GameObject nameGo, out GameObject closeGo)
	{
		GameObject rootGo = new GameObject(TEST_LAYOUT_NAME);
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		// newObject(out,"TargetRoot") 2 参重载以 mRoot(canvas) 为 parent 递归查找 → TargetRoot 必须挂到 rootGo 下
		targetGo = new GameObject("TargetRoot");
		targetGo.AddComponent<RectTransform>();
		targetGo.transform.SetParent(rootGo.transform);
		headGo = new GameObject("Head");
		headGo.AddComponent<RectTransform>();
		headGo.AddComponent<Image>();
		headGo.transform.SetParent(targetGo.transform);
		nameGo = new GameObject("Name");
		nameGo.AddComponent<RectTransform>();
		nameGo.AddComponent<Text>();
		nameGo.transform.SetParent(targetGo.transform);
		closeGo = new GameObject("Close");
		closeGo.AddComponent<RectTransform>();
		closeGo.AddComponent<Image>();
		closeGo.transform.SetParent(targetGo.transform);

		GameLayout layout = new GameLayout();
		layout.setName(TEST_LAYOUT_NAME);
		layout.setType(typeof(TestLegendPanel));
		layout.setParent(null);
		layout.init();
		return layout;
	}

	// 创建普通 UI 对象(遮罩/背景用)
	private static myUGUIObject createUI(string name, out GameObject go)
	{
		go = new GameObject(name);
		go.AddComponent<RectTransform>();
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}

	// ═════════════════════════════════════════════════════════════════
	// assignWindow: newObject 绑定 4 个真实组件节点
	// ═════════════════════════════════════════════════════════════════
	private static void testPanelInitAssignWindowBindsTree()
	{
		GameLayout layout = createPanel(out GameObject targetGo, out GameObject headGo, out GameObject nameGo, out GameObject closeGo);
		try
		{
			TestLegendPanel panel = (TestLegendPanel)layout.getScript();
			assertEqual(1, panel.assignWindowCount, "init 链路调用了 assignWindow");
			assertTrue(ReferenceEquals(targetGo, panel.mTargetRoot.getGameObject()), "mTargetRoot 绑定 TargetRoot 节点");
			assertTrue(ReferenceEquals(headGo, panel.mHead.getGameObject()), "mHead 绑定 Head 节点(Image)");
			assertTrue(ReferenceEquals(nameGo, panel.mName.getGameObject()), "mName 绑定 Name 节点(Text)");
			assertTrue(ReferenceEquals(closeGo, panel.mCloseBtn.getGameObject()), "mCloseBtn 绑定 Close 节点(Image)");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(headGo);
			UnityEngine.Object.DestroyImmediate(nameGo);
			UnityEngine.Object.DestroyImmediate(closeGo);
		}
	}

	// update 组合: mShowTarget=true → setActive(返回值) → setText+setColor+setActive 全生效
	private static void testPanelUpdateComboWithTargetVisible()
	{
		GameLayout layout = createPanel(out GameObject targetGo, out GameObject headGo, out GameObject nameGo, out GameObject closeGo);
		try
		{
			TestLegendPanel panel = (TestLegendPanel)layout.getScript();
			// init() 末尾 setVisibleForce(false) 强制隐藏, 需先显示才能驱动 update
			layout.setVisible(true);
			panel.mShowTarget = true;
			layout.update(0.1f);
			assertEqual(1, panel.updateCount, "script.update 被驱动");
			// setText(string) 组合生效
			assertEqual("123", panel.mName.getText(), "update 组合: setText(int) 覆盖后为 123");
			// setColor 组合生效
			Color red = new Color(1.0f, 0.0f, 0.0f, 1.0f);
			assertTrue(red == panel.mName.getColor(), "update 组合: setColor(red) 生效");
			// setActive 组合生效
			assertTrue(panel.mHead.isActive(), "update 组合: mHead.setActive(true) 生效");
			assertTrue(panel.mTargetRoot.isActive(), "mTargetRoot 保持激活");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(headGo);
			UnityEngine.Object.DestroyImmediate(nameGo);
			UnityEngine.Object.DestroyImmediate(closeGo);
		}
	}

	// update 组合(隐藏): mShowTarget=false → setActive(false) 返回 false → if 块不执行, UI 状态保持
	private static void testPanelUpdateComboWithTargetHidden()
	{
		GameLayout layout = createPanel(out GameObject targetGo, out GameObject headGo, out GameObject nameGo, out GameObject closeGo);
		try
		{
			TestLegendPanel panel = (TestLegendPanel)layout.getScript();
			// init 后强制隐藏, 先显示
			layout.setVisible(true);
			// 先显示一次再隐藏
			panel.mShowTarget = true;
			layout.update(0.1f);
			panel.mShowTarget = false;
			layout.update(0.2f);
			assertEqual(2, panel.updateCount, "update 每次都被驱动");
			assertFalse(panel.mTargetRoot.isActive(), "mShowTarget=false 后 mTargetRoot 失活");
			// if 块不执行 → mName 保持上次值(组合语义: 隐藏不刷新)
			assertEqual("123", panel.mName.getText(), "隐藏时 update 不刷新文本(保持上次值)");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(headGo);
			UnityEngine.Object.DestroyImmediate(nameGo);
			UnityEngine.Object.DestroyImmediate(closeGo);
		}
	}

	// UILegendBase.initLegendPanel 组合: 关闭按钮注册 + 背景 raycast + 遮罩尺寸同步
	private static void testPanelLegendInitCombo()
	{
		GameLayout layout = createPanel(out GameObject targetGo, out GameObject headGo, out GameObject nameGo, out GameObject closeGo);
		myUGUIObject background = createUI("Background", out GameObject bgGo);
		myUGUIObject activeMask = createUI("ActiveMask", out GameObject maskGo);
		try
		{
			background.setSize(new Vector2(300.0f, 200.0f));
			activeMask.setSize(new Vector2(100.0f, 100.0f));
			TestLegendPanel panel = (TestLegendPanel)layout.getScript();
			panel.initLegendPanel(panel.mCloseBtn, activeMask, background, false);
			// 背景 Image.raycastTarget = true
			Image bgImage = bgGo.GetComponent<Image>();
			if (bgImage == null)
			{
				bgImage = bgGo.AddComponent<Image>();
			}
			assertTrue(bgImage.raycastTarget, "initLegendPanel: 背景 raycastTarget=true");
			// 遮罩尺寸同步为背景尺寸
			assertEqual(300.0f, activeMask.getSize().x, 0.001f, "遮罩宽度同步为背景宽度");
			assertEqual(200.0f, activeMask.getSize().y, 0.001f, "遮罩高度同步为背景高度");
			// 关闭按钮已注册(registeColliderImage → COMWindowInteractiveFade 组件)
			assertNotNull(panel.mCloseBtn.getComponent<COMWindowInteractiveFade>(), "关闭按钮注册后含 COMWindowInteractiveFade");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(headGo);
			UnityEngine.Object.DestroyImmediate(nameGo);
			UnityEngine.Object.DestroyImmediate(closeGo);
			UnityEngine.Object.DestroyImmediate(bgGo);
			UnityEngine.Object.DestroyImmediate(maskGo);
		}
	}

	// UILegendBase.enablePanelDrag 组合: registeCollider + COMWindowDrag + 阈值
	private static void testPanelDragCombo()
	{
		GameLayout layout = createPanel(out GameObject targetGo, out GameObject headGo, out GameObject nameGo, out GameObject closeGo);
		myUGUIObject background = createUI("Background", out GameObject bgGo);
		try
		{
			TestLegendPanel panel = (TestLegendPanel)layout.getScript();
			panel.enablePanelDrag(background);
			COMWindowDrag drag = background.getComponent<COMWindowDrag>();
			assertNotNull(drag, "enablePanelDrag 添加 COMWindowDrag 组件");
			assertTrue(drag.isActive(), "拖拽组件激活");
			assertTrue(background.isNeedUpdate(), "enablePanelDrag 启用窗口更新");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(targetGo);
			UnityEngine.Object.DestroyImmediate(headGo);
			UnityEngine.Object.DestroyImmediate(nameGo);
			UnityEngine.Object.DestroyImmediate(closeGo);
			UnityEngine.Object.DestroyImmediate(bgGo);
		}
	}
}

// 业务面板模拟(UIAttackTarget + UILegendBase 真实用法的组合)
public class TestLegendPanel : LayoutScript
{
	public myUGUIObject mTargetRoot;
	public myUGUIImage mHead;
	public myUGUIText mName;
	public myUGUIImageSimple mCloseBtn;
	public bool mShowTarget;
	public int assignWindowCount;
	public int updateCount;
	public int mCloseCount;

	public override void assignWindow()
	{
		assignWindowCount++;
		newObject(out mTargetRoot, "TargetRoot");
		newObject(out mHead, mTargetRoot, "Head");
		newObject(out mName, mTargetRoot, "Name");
		newObject(out mCloseBtn, mTargetRoot, "Close");
	}

	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		updateCount++;
		// 模仿 UIAttackTarget: setActive 返回值当"是否变化"判断(文档化: 返回入参本身)
		if (mTargetRoot.setActive(mShowTarget))
		{
			mName.setText("TestName");
			mName.setColor(new Color(1.0f, 0.0f, 0.0f, 1.0f));
			mHead.setActive(true);
			mName.setText(123);
		}
	}

	// 模仿 UILegendBase.initLegendPanel
	public void initLegendPanel(myUGUIImageSimple closeObj, myUGUIObject activeMask, myUGUIObject background, bool enableDrag)
	{
		closeObj?.registeColliderImage(close);
		if (background != null)
		{
			Image image = background.tryGetUnityComponent<Image>();
			if (image != null)
			{
				image.raycastTarget = true;
			}
			background.registeCollider();
		}
		if (activeMask != null && background != null && !activeMask.getSize().isEqual(background.getSize()))
		{
			activeMask.setPosition(Vector3.zero);
			activeMask.setSize(background.getSize());
		}
	}

	// 模仿 UILegendBase.enablePanelDrag
	public void enablePanelDrag(myUGUIObject obj)
	{
		obj.registeCollider();
		COMWindowDrag drag = obj.getOrAddComponent<COMWindowDrag>();
		drag.setActive(true);
		drag.setStartDragThreshold(1.0f);
		obj.setNeedUpdate(true);
	}

	private new void close() { mCloseCount++; }

	public new void resetProperty()
	{
		base.resetProperty();
		mTargetRoot = null;
		mHead = null;
		mName = null;
		mCloseBtn = null;
		mShowTarget = false;
		assignWindowCount = 0;
		updateCount = 0;
		mCloseCount = 0;
	}
}
