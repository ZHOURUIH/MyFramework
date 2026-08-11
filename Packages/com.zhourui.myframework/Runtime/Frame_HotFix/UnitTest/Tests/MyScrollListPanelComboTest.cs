using System.IO;
using UnityEngine;
using UnityEngine.UI;
using static FrameBaseHotFix;
using static TestAssert;

// 滚动列表面板组合深度测试(模仿真实业务: 列表面板动态添加 item + 自动布局 + 帧循环驱动):
//   TestScrollListPanel : LayoutScript —— 模拟"背包/聊天列表"类滚动面板
//   assignWindow: newObject 绑定 ScrollView(RectTransform+Image+ScrollRect)/Viewport/Content/Title
//                 + initScrollRect(视口内容绑定+pivot)
//   addItem: newObject 在 Content 下动态创建 item + 设置尺寸(业务入口)
//   autoAdjustContent: 内容变化后重排对齐(content > viewport → 对齐 1-pivot.y)
//   update 帧循环: layout.update 驱动 script.update
//
// 节点树(场景根): rootGo(Canvas) / ScrollView(RectTransform+Image+ScrollRect)
//                   ├ Viewport(RectTransform+Image) └ Content └ Title(Text)
public static class MyScrollListPanelComboTest
{
	private const string TEST_LAYOUT_PATH = "TestScrollListPanel.prefab";
	private const string TEST_LAYOUT_NAME = "TestScrollListPanel";
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
				mLayoutManager.registeLayout(typeof(TestScrollListPanel), TEST_LAYOUT_PATH, LAYOUT_LIFE_CYCLE.PERSIST, null);
				sRegistered = true;
			}
			testAssignWindowBindsScrollTree();
			testAddItemsContentGrows();
			testAutoAdjustAfterAdd();
			testAddMoreAndReAlign();
			testFrameLoopUpdate();
			testScrollTopPosKeep();
			testNormalizedScrollAndReAlign();
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
	// 辅助: 完整 init 滚动列表面板(文件已注册 + 动态节点树 + 预加组件)
	// ═════════════════════════════════════════════════════════════════
	private static GameLayout createPanel(out GameObject rootGo, out GameObject scrollGo, out GameObject viewportGo, out GameObject contentGo, out GameObject titleGo)
	{
		rootGo = new GameObject(TEST_LAYOUT_NAME);
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		scrollGo = new GameObject("ScrollView");
		scrollGo.AddComponent<RectTransform>();
		scrollGo.AddComponent<Image>();
		scrollGo.AddComponent<ScrollRect>();
		scrollGo.transform.SetParent(rootGo.transform);
		viewportGo = new GameObject("Viewport");
		viewportGo.AddComponent<RectTransform>();
		viewportGo.AddComponent<Image>();
		viewportGo.transform.SetParent(scrollGo.transform);
		contentGo = new GameObject("Content");
		contentGo.AddComponent<RectTransform>();
		contentGo.transform.SetParent(viewportGo.transform);
		titleGo = new GameObject("Title");
		titleGo.AddComponent<RectTransform>();
		titleGo.AddComponent<Text>();
		titleGo.transform.SetParent(scrollGo.transform);

		GameLayout layout = new GameLayout();
		layout.setName(TEST_LAYOUT_NAME);
		layout.setType(typeof(TestScrollListPanel));
		layout.setParent(null);
		layout.init();
		return layout;
	}

	// assignWindow: newObject 绑定 4 节点 + initScrollRect 初始化滚动结构
	private static void testAssignWindowBindsScrollTree()
	{
		GameLayout layout = createPanel(out GameObject rootGo, out GameObject scrollGo, out GameObject viewportGo, out GameObject contentGo, out GameObject titleGo);
		try
		{
			TestScrollListPanel panel = (TestScrollListPanel)layout.getScript();
			assertEqual(1, panel.assignWindowCount, "init 链路调用了 assignWindow");
			assertTrue(ReferenceEquals(scrollGo, panel.mScrollView.getGameObject()), "mScrollView 绑定 ScrollView 节点");
			assertTrue(ReferenceEquals(viewportGo, panel.mViewport.getGameObject()), "mViewport 绑定 Viewport 节点");
			assertTrue(ReferenceEquals(contentGo, panel.mContent.getGameObject()), "mContent 绑定 Content 节点");
			assertTrue(ReferenceEquals(titleGo, panel.mTitle.getGameObject()), "mTitle 绑定 Title 节点");
			// initScrollRect 绑定
			assertTrue(ReferenceEquals(panel.mContent.getRectTransform(), panel.mScrollView.getScrollRect().content), "ScrollRect.content 绑定");
			assertTrue(ReferenceEquals(panel.mViewport.getRectTransform(), panel.mScrollView.getScrollRect().viewport), "ScrollRect.viewport 绑定");
			// initScrollRect 内 setContentPivotVertical(0.3)
			assertEqual(0.3f, panel.mContent.getPivot().y, 0.001f, "pivot.y=0.3");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 动态添加 item: 挂到 Content 下 + content 尺寸随业务增长
	private static void testAddItemsContentGrows()
	{
		GameLayout layout = createPanel(out GameObject rootGo, out GameObject scrollGo, out GameObject viewportGo, out GameObject contentGo, out GameObject titleGo);
		try
		{
			TestScrollListPanel panel = (TestScrollListPanel)layout.getScript();
			panel.mViewport.setSize(new Vector2(100.0f, 60.0f));
			myUGUIObject item0 = panel.addItem("Item0");
			myUGUIObject item1 = panel.addItem("Item1");
			myUGUIObject item2 = panel.addItem("Item2");
			// item 挂到 Content 下(组合: newObject 指定父)
			assertTrue(ReferenceEquals(contentGo.transform, item0.getGameObject().transform.parent), "Item0 挂到 Content 下");
			assertTrue(ReferenceEquals(contentGo.transform, item1.getGameObject().transform.parent), "Item1 挂到 Content 下");
			assertTrue(ReferenceEquals(contentGo.transform, item2.getGameObject().transform.parent), "Item2 挂到 Content 下");
			// item 尺寸生效(autoGridVertical 用 rect.height 计算)
			assertEqual(30.0f, item0.getSize().y, 0.001f, "Item 高 30");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 组合链路: addItem x3 → autoAdjustContent → content(90) > viewport(60) → 对齐 1-pivot.y
	private static void testAutoAdjustAfterAdd()
	{
		GameLayout layout = createPanel(out GameObject rootGo, out GameObject scrollGo, out GameObject viewportGo, out GameObject contentGo, out GameObject titleGo);
		try
		{
			TestScrollListPanel panel = (TestScrollListPanel)layout.getScript();
			panel.mViewport.setSize(new Vector2(100.0f, 60.0f));
			panel.addItem("Item0");
			panel.addItem("Item1");
			panel.addItem("Item2");
			// 业务组合: 添加完调用 autoAdjustContent 自动排列+对齐
			panel.mScrollView.autoAdjustContent(CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE);
			// content 高 = 3*30 = 90 > viewport 60 → alignContentY(1-0.3)
			assertEqual(90.0f, panel.mContent.getSize().y, 0.001f, "content 高度=3 item 高度");
			assertEqual(0.7f, panel.mScrollView.getScrollRect().verticalNormalizedPosition, 0.01f, "content>viewport → vnp=1-pivot.y");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 继续添加 item 超过 viewport 后仍保持对齐
	private static void testAddMoreAndReAlign()
	{
		GameLayout layout = createPanel(out GameObject rootGo, out GameObject scrollGo, out GameObject viewportGo, out GameObject contentGo, out GameObject titleGo);
		try
		{
			TestScrollListPanel panel = (TestScrollListPanel)layout.getScript();
			panel.mViewport.setSize(new Vector2(100.0f, 60.0f));
			for (int i = 0; i < 5; ++i)
			{
				panel.addItem("Item" + i);
			}
			panel.mScrollView.autoAdjustContent(CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE);
			assertEqual(150.0f, panel.mContent.getSize().y, 0.001f, "5 item → content 高 150");
			assertEqual(0.7f, panel.mScrollView.getScrollRect().verticalNormalizedPosition, 0.01f, "content 更大仍对齐 0.7");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 帧循环: layout.update 驱动 script.update 多帧
	private static void testFrameLoopUpdate()
	{
		GameLayout layout = createPanel(out GameObject rootGo, out GameObject scrollGo, out GameObject viewportGo, out GameObject contentGo, out GameObject titleGo);
		try
		{
			TestScrollListPanel panel = (TestScrollListPanel)layout.getScript();
			// setVisible 触发 onGameState 检查: 滚动列表必须在注册列表里(模拟 registeScrollRect 的注册)
			panel.registeScrollViewForTest();
			// init 后强制隐藏, 先显示才能驱动 update
			layout.setVisible(true);
			layout.update(0.1f);
			layout.update(0.2f);
			layout.update(0.3f);
			assertEqual(3, panel.updateCount, "3 帧 update 全部驱动 script.update");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setContentTopPos/getContentTopPos 组合: 内容变化时保持顶部
	private static void testScrollTopPosKeep()
	{
		GameLayout layout = createPanel(out GameObject rootGo, out GameObject scrollGo, out GameObject viewportGo, out GameObject contentGo, out GameObject titleGo);
		try
		{
			TestScrollListPanel panel = (TestScrollListPanel)layout.getScript();
			panel.mViewport.setSize(new Vector2(100.0f, 60.0f));
			panel.addItem("Item0");
			panel.addItem("Item1");
			panel.mScrollView.setContentTopPos(40.0f);
			assertEqual(40.0f, panel.mScrollView.getContentTopPos(), 0.001f, "setContentTopPos(40) 读回");
			// 再添加 item 后顶部位置仍保持(业务组合: 先设顶部再增内容)
			panel.addItem("Item2");
			assertEqual(40.0f, panel.mScrollView.getContentTopPos(), 0.001f, "添加 item 不改变顶部位置");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 滚动后新增内容重排: normalizedPosition 被 autoAdjustContent 重新对齐
	private static void testNormalizedScrollAndReAlign()
	{
		GameLayout layout = createPanel(out GameObject rootGo, out GameObject scrollGo, out GameObject viewportGo, out GameObject contentGo, out GameObject titleGo);
		try
		{
			TestScrollListPanel panel = (TestScrollListPanel)layout.getScript();
			panel.mViewport.setSize(new Vector2(100.0f, 60.0f));
			// 3 个 item → content 90 > viewport 60(可滚动), 2 个时 60==60 会归零
			panel.addItem("Item0");
			panel.addItem("Item1");
			panel.addItem("Item2");
			panel.mScrollView.autoAdjustContent(CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE);
			assertEqual(0.7f, panel.mScrollView.getScrollRect().verticalNormalizedPosition, 0.01f, "初始对齐 0.7");
			// 用户滚动到中间
			panel.mScrollView.setNormalizedPositionY(0.5f);
			assertEqual(0.5f, panel.mScrollView.getScrollRect().verticalNormalizedPosition, 0.01f, "手动滚动到 0.5");
			// 新增内容后重新对齐(业务: 列表顶部插新数据 → 回顶部)
			panel.addItem("ItemNew");
			panel.mScrollView.autoAdjustContent(CONTENT_ADJUST.SINGLE_COLUMN_OR_LINE);
			assertEqual(0.7f, panel.mScrollView.getScrollRect().verticalNormalizedPosition, 0.01f, "重新对齐回 0.7");
		}
		finally
		{
			layout.destroy();
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}

// 滚动列表面板模拟(背包/聊天列表类业务)
public class TestScrollListPanel : LayoutScript
{
	public myUGUIScrollRect mScrollView;
	public myUGUIObject mViewport;
	public myUGUIObject mContent;
	public myUGUIText mTitle;
	public int assignWindowCount;
	public int updateCount;

	public override void assignWindow()
	{
		assignWindowCount++;
		newObject(out mScrollView, "ScrollView");
		newObject(out mViewport, mScrollView, "Viewport");
		newObject(out mContent, mViewport, "Content");
		newObject(out mTitle, mScrollView, "Title");
		// 布局内初始化滚动结构(不用 registeScrollRect: 其 bindPassOnlyParent 要求 viewport 已注册, 会 logError)
		mScrollView.initScrollRect(mViewport, mContent, 0.3f, 0.5f);
	}

	// 模拟 registeScrollRect 的滚动列表注册效果(避开 bindPassOnlyParent 的 viewport 注册检查)
	// 用于通过 onGameState 的"滑动列表未注册"编辑器检查
	public void registeScrollViewForTest()
	{
		mScrollViewRegisteList ??= new();
		mScrollViewRegisteList.Add(mScrollView);
	}

	// 业务入口: 在 Content 下动态创建 item
	// 注意: newObject 是"查找已有节点"(对应 prefab 内节点), 不能动态创建 → 运行时 item 用直接创建
	public myUGUIObject addItem(string name)
	{
		GameObject go = new GameObject(name);
		go.AddComponent<RectTransform>();
		go.transform.SetParent(mContent.getGameObject().transform);
		myUGUIObject item = new myUGUIObject();
		item.setObject(go);
		item.init();
		item.setSize(new Vector2(100.0f, 30.0f));
		item.setPosition(Vector3.zero);
		return item;
	}

	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		updateCount++;
	}

	public new void resetProperty()
	{
		base.resetProperty();
		mScrollView = null;
		mViewport = null;
		mContent = null;
		mTitle = null;
		assignWindowCount = 0;
		updateCount = 0;
	}
}
