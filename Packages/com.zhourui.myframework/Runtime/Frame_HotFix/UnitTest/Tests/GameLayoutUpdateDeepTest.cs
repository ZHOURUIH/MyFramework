using System.IO;
using UnityEngine;
using static FrameBaseHotFix;
using static TestAssert;

// GameLayout.update 驱动链深度测试(完整 init 的 layout):
//   - update → mScript.update 驱动(mNeedUpdate 默认 true)
//   - update → mNeedUpdateList 遍历驱动 uiObj.update(canUpdate 过滤)
//   - setVisible(false) → update 直接 return, 不驱动
//   - setDefaultUpdateWindow(false) + mNeedUpdate=false → register 不进更新列表, 不被驱动
//   - 遍历中自移除(SafeList 快照遍历安全, 不抛异常)
//   - myUGUICanvas setSortingOrder/setSortingLayer(getCanvas 断言)
//
// 复用 GameLayoutLifecycleTest 的完整 init 突破(运行时创建 prefab 文件 + 动态根节点 + 预加 Canvas)
public static class GameLayoutUpdateDeepTest
{
	private const string TEST_LAYOUT_PATH = "TestLayoutDeepUpdate.prefab";
	private const string TEST_LAYOUT_NAME = "TestLayoutDeepUpdate";
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
				mLayoutManager.registeLayout(typeof(TestLayoutScriptDeep), TEST_LAYOUT_PATH, LAYOUT_LIFE_CYCLE.PERSIST, null);
				sRegistered = true;
			}
			testUpdateDrivesScriptUpdate();
			testUpdateDrivesUIObjectUpdate();
			testUpdateSkipsInvisible();
			testUpdateSkipsNotInList();
			testUpdateRemoveSelfInIteration();
			testCanvasSortingOrderAndLayer();
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
	// 辅助: 完整 init 的 layout(文件已注册 + 动态根节点 + 预加 Canvas)
	// ═════════════════════════════════════════════════════════════════
	private static GameLayout createInitedLayout()
	{
		GameObject rootGo = new GameObject(TEST_LAYOUT_NAME);
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		GameLayout layout = new GameLayout();
		layout.setName(TEST_LAYOUT_NAME);
		layout.setType(typeof(TestLayoutScriptDeep));
		layout.setParent(null);
		layout.init();
		return layout;
	}

	// 创建 UI 对象(mNeedUpdate 默认 false, 需要驱动时测试里显式 setNeedUpdate(true))
	private static TestUpdateUIObject createUpdateUI()
	{
		GameObject go = new GameObject("TestUpdateUI");
		go.AddComponent<RectTransform>();
		TestUpdateUIObject ui = new TestUpdateUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}

	// destroyTestUI: ref 参数需精确类型, 子类先转基类再销毁并置空
	private static void destroyTestUI(ref TestUpdateUIObject ui)
	{
		if (ui != null)
		{
			myUGUIObject obj = ui;
			LayoutScript.destroyObject(ref obj, true);
			ui = null;
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// update → mScript.update 驱动(mNeedUpdate 默认 true)
	// ═════════════════════════════════════════════════════════════════
	private static void testUpdateDrivesScriptUpdate()
	{
		GameLayout layout = createInitedLayout();
		try
		{
			TestLayoutScriptDeep script = (TestLayoutScriptDeep)layout.getScript();
			assertNotNull(script, "init 后 mScript 是 TestLayoutScriptDeep");
			// init() 末尾 setVisibleForce(false) 强制隐藏, 需先显示才能驱动 update
			layout.setVisible(true);
			layout.update(0.1f);
			assertEqual(1, script.updateCount, "script.update 被驱动(mNeedUpdate 默认 true)");
			layout.update(0.2f);
			assertEqual(2, script.updateCount, "再次 update 再次驱动");
		}
		finally
		{
			// layout.destroy 已销毁 rootGo(mDestroyImmediately=true), 不手动销毁
			layout.destroy();
		}
	}

	// update → mNeedUpdateList 遍历驱动 uiObj.update(canUpdate 过滤)
	private static void testUpdateDrivesUIObjectUpdate()
	{
		GameLayout layout = createInitedLayout();
		TestUpdateUIObject ui = createUpdateUI();
		try
		{
			ui.setNeedUpdate(true);
			layout.registerUIObject(ui);   // mDefaultUpdateWindow=true 自动进更新列表
			layout.setVisible(true);   // init 后强制隐藏, 需先显示
			layout.update(0.1f);
			assertEqual(1, ui.updateCount, "mNeedUpdateList 中 canUpdate 对象被驱动");
			layout.update(0.2f);
			assertEqual(2, ui.updateCount, "再次 update 再次驱动");
		}
		finally
		{
			destroyTestUI(ref ui);
			layout.destroy();
		}
	}

	// setVisible(false) → update 直接 return, 不驱动
	private static void testUpdateSkipsInvisible()
	{
		GameLayout layout = createInitedLayout();
		try
		{
			TestLayoutScriptDeep script = (TestLayoutScriptDeep)layout.getScript();
			layout.setVisible(false);
			layout.update(0.1f);
			assertEqual(0, script.updateCount, "不可见时 update 直接 return");
			layout.setVisible(true);
			layout.update(0.1f);
			assertEqual(1, script.updateCount, "重新可见后 update 恢复驱动");
		}
		finally
		{
			layout.destroy();
		}
	}

	// setDefaultUpdateWindow(false) + mNeedUpdate=false → register 不进更新列表, 不被驱动
	private static void testUpdateSkipsNotInList()
	{
		GameLayout layout = createInitedLayout();
		TestUpdateUIObject ui = createUpdateUI();
		try
		{
			layout.setDefaultUpdateWindow(false);
			layout.registerUIObject(ui);   // mDefaultUpdateWindow=false && isNeedUpdate=false → 不进列表
			layout.setVisible(true);   // init 后强制隐藏, 需先显示
			layout.update(0.1f);
			assertEqual(0, ui.updateCount, "不在更新列表中的对象不被驱动");
			assertFalse(layout.canUIObjectUpdate(ui), "未 notify 时 canUIObjectUpdate=false");
		}
		finally
		{
			destroyTestUI(ref ui);
			layout.destroy();
		}
	}

	// 遍历中自移除: ui.update 内部 notify(false) 移除自己 → SafeList 快照遍历安全
	private static void testUpdateRemoveSelfInIteration()
	{
		GameLayout layout = createInitedLayout();
		TestUpdateUIObject ui = createUpdateUI();
		ui.layoutForRemove = layout;
		ui.removeSelfInUpdate = true;
		try
		{
			ui.setNeedUpdate(true);
			layout.registerUIObject(ui);
			layout.setVisible(true);   // init 后强制隐藏, 需先显示
			layout.update(0.1f);   // 遍历时 ui.update 内部移除自己
			assertEqual(1, ui.updateCount, "遍历中自移除后 update 仍被调用一次(快照)");
			assertFalse(layout.canUIObjectUpdate(ui), "自移除后不在 mNeedUpdateList");
		}
		finally
		{
			destroyTestUI(ref ui);
			layout.destroy();
		}
	}

	// myUGUICanvas: setSortingOrder/setSortingLayer → getCanvas 断言
	private static void testCanvasSortingOrderAndLayer()
	{
		GameLayout layout = createInitedLayout();
		try
		{
			myUGUICanvas root = layout.getRoot();
			assertNotNull(root, "init 后 mRoot 是 myUGUICanvas");
			root.setSortingOrder(5);
			assertEqual(5, root.getCanvas().sortingOrder, "setSortingOrder 写入 mCanvas");
			// 项目仅定义 Default 一个排序层: 给不存在的层赋值被忽略(回退 Default, 文档化真实行为)
			root.setSortingLayer("UI");
			assertEqual("Default", root.getCanvas().sortingLayerName, "不存在的排序层被忽略(项目仅 Default)");
			assertEqual(0, SortingLayer.NameToID("UI"), "UI 排序层不存在(NameToID=0, 文档化)");
		}
		finally
		{
			layout.destroy();
		}
	}
}

// 可被驱动计数的 myUGUIObject 子类: override update 计数, 支持遍历中自移除
public class TestUpdateUIObject : myUGUIObject
{
	public int updateCount;
	public GameLayout layoutForRemove;
	public bool removeSelfInUpdate;

	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		updateCount++;
		if (removeSelfInUpdate && layoutForRemove != null)
		{
			layoutForRemove.notifyUIObjectNeedUpdate(this, false);
		}
	}

	public new void resetProperty()
	{
		base.resetProperty();
		updateCount = 0;
		layoutForRemove = null;
		removeSelfInUpdate = false;
	}
}
