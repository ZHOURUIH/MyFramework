using System.IO;
using UnityEngine;
using UObject = UnityEngine.Object;
using static FrameBaseHotFix;
using static TestAssert;

// GameLayout 完整生命周期深度测试(运行时创建真实文件 + 动态场景节点)
// 突破点:
//   - registeLayout 的 isFileExist 检查 → 测试运行时创建真实文件 Assets/GameResources/TestLayout.prefab
//   - mScript.newObject 的 findRootGameObject → 测试动态创建同名 GameObject("TestLayout")
//   - myUGUICanvas.init 缺 Canvas 时自动 AddComponent(自包含)
// 流程: 创建文件 → registeLayout → 创建根节点 → new GameLayout+setName+setType → init()
//       → 验证生命周期 → destroy() → 清理文件/节点
public static class GameLayoutLifecycleTest
{
	private const string TEST_LAYOUT_PATH = "TestLayout.prefab";
	private const string TEST_LAYOUT_NAME = "TestLayout";
	private static readonly string sPrefabFile = "Assets/GameResources/" + TEST_LAYOUT_PATH;

	public static void Run()
	{
		// 创建真实 prefab 文件(空文件满足 File.Exists)
		if (!File.Exists(sPrefabFile))
		{
			File.WriteAllText(sPrefabFile, "");
		}
		try
		{
			mLayoutManager.registeLayout(typeof(TestLayoutScript), TEST_LAYOUT_PATH, LAYOUT_LIFE_CYCLE.PERSIST, null);
			testInitFullLifecycle();
			testSetVisibleToggle();
			testUpdateAfterInit();
			testRegisterUIObjectAfterInit();
		}
		finally
		{
			if (File.Exists(sPrefabFile))
			{
				File.Delete(sPrefabFile);
			}
			// 残留检查: GameObject.Find("TestLayout") 应返回 null(destroyWindow 已销毁)
			GameObject leftover = GameObject.Find(TEST_LAYOUT_NAME);
			if (leftover != null)
			{
				UnityEngine.Debug.LogError("[GameLayoutLifecycleTest] 残留节点未清理: " + TEST_LAYOUT_NAME);
				UObject.DestroyImmediate(leftover);
			}
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建布局 + 根节点(返回 layout 和根 GameObject)
	// ═════════════════════════════════════════════════════════════════
	private static GameLayout createLayoutWithRoot(out GameObject rootGo)
	{
		rootGo = new GameObject(TEST_LAYOUT_NAME);
		rootGo.AddComponent<RectTransform>();
		// 预加 Canvas: myUGUICanvas.init 的 TryGetComponent 命中后跳过 logError 分支
		rootGo.AddComponent<Canvas>();
		GameLayout layout = new GameLayout();
		layout.setName(TEST_LAYOUT_NAME);
		layout.setType(typeof(TestLayoutScript));
		return layout;
	}

	// ═════════════════════════════════════════════════════════════════
	// init 完整生命周期: createScript → newObject → assignWindow → init
	// → postInit → setRenderOrder → setVisibleForce(false)
	// ═════════════════════════════════════════════════════════════════
	private static void testInitFullLifecycle()
	{
		GameLayout layout = createLayoutWithRoot(out GameObject rootGo);
		try
		{
			layout.init();
			// 脚本已创建
			assertNotNull(layout.getScript(), "init 后 getScript 非 null");
			assertTrue(layout.getScript() is TestLayoutScript, "脚本类型为 TestLayoutScript");
			// 根节点已创建且是 myUGUICanvas
			assertNotNull(layout.getRoot(), "init 后 mRoot 非 null");
			// init 末尾强制隐藏
			assertFalse(layout.isVisible(), "init 后强制隐藏 isVisible=false");
			// 渲染顺序默认 0
			assertEqual(0, layout.getRenderOrder(), "默认 renderOrder=0");
			// name 保留
			assertEqual(TEST_LAYOUT_NAME, layout.getName(), "name 保留");
		}
		finally
		{
			// layout.destroy() 内部 destroyWindow(mRoot, true) 已销毁 rootGo(mDestroyImmediately=true)
			layout.destroy();
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setVisible(true) → isVisible=true; setVisibleForce(false) → false
	// ═════════════════════════════════════════════════════════════════
	private static void testSetVisibleToggle()
	{
		GameLayout layout = createLayoutWithRoot(out GameObject rootGo);
		try
		{
			layout.init();
			assertFalse(layout.isVisible(), "init 后隐藏");
			layout.setVisible(true);
			assertTrue(layout.isVisible(), "setVisible(true) 后 isVisible=true");
			layout.setVisibleForce(false);
			assertFalse(layout.isVisible(), "setVisibleForce(false) 后 isVisible=false");
			layout.setVisible(true);
			assertTrue(layout.isVisible(), "再次 setVisible(true) 后 isVisible=true");
		}
		finally
		{
			layout.destroy();
			// rootGo 已由 destroyWindow(mRoot, true) 销毁, 不二次销毁
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// update/lateUpdate — 可见时走脚本更新, 不崩
	// ═════════════════════════════════════════════════════════════════
	private static void testUpdateAfterInit()
	{
		GameLayout layout = createLayoutWithRoot(out GameObject rootGo);
		try
		{
			layout.init();
			layout.setVisible(true);
			layout.update(0.016f);      // 可见 → mNeedUpdateList 空 → 只走脚本
			layout.lateUpdate(0.016f);  // 可见 → mScript.lateUpdate
			layout.setVisibleForce(false);
			layout.update(0.016f);      // 隐藏 → 直接 return
			layout.lateUpdate(0.016f);
		}
		finally
		{
			layout.destroy();
			// rootGo 已由 destroyWindow(mRoot, true) 销毁, 不二次销毁
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// init 后 registerUIObject — 布局容器与 UI 对象联动
	// ═════════════════════════════════════════════════════════════════
	private static void testRegisterUIObjectAfterInit()
	{
		GameLayout layout = createLayoutWithRoot(out GameObject rootGo);
		GameObject uiGo = new GameObject("TestLayoutChild");
		try
		{
			layout.init();
			// init 时 mRoot 已被注册到 mGameObjectSearchList(1 个)
			int initCount = layout.getUIObjectList().Count;
			assertEqual(1, initCount, "init 后 mRoot 已注册, 列表含 1 个");
			uiGo.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(uiGo);
			ui.init();
			layout.registerUIObject(ui);
			assertEqual(initCount + 1, layout.getUIObjectList().Count, "register 后列表 +1");
			assertTrue(ReferenceEquals(ui, layout.getUIObject(uiGo)), "getUIObject 反查命中");
			layout.unregisterUIObject(ui);
			assertEqual(initCount, layout.getUIObjectList().Count, "unregister 后恢复 init 数量");
		}
		finally
		{
			layout.destroy();
			// rootGo 已由 destroyWindow(mRoot, true) 销毁, 不二次销毁
			UObject.DestroyImmediate(uiGo);
		}
	}
}
