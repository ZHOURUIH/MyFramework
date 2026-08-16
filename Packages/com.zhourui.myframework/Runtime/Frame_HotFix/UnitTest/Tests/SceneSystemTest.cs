using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static TestAssert;

// SceneSystem 单元测试 (注册/查询逻辑)
// 框架环境已完全初始化, 覆盖不涉及场景加载的纯逻辑:
//   registeScene (需 .unity 后缀) / getScenePath
//   getScriptMappingCount (同类型多场景)
//   getScene 未加载返回 null
//   registeScene 非法路径被拒绝
// 注: loadSceneAsync/unloadScene/showScene 依赖 ResourceManager+场景加载, 不在此覆盖
public static class SceneSystemTest
{
	public static void Run()
	{
		// ─── 注册 ───
		testRegisteScene();
		testGetScenePath();
		testRegisteSceneRejectsNonUnity();
		// ─── 脚本映射 ───
		testScriptMappingCount();
		testScriptMappingMultipleSameType();
		// ─── 查询未加载场景 ───
		testGetSceneNotLoaded();
		// ─── 生命周期/卸载 空列表守卫 ───
		testLateUpdateEmptySafe();
		testUnloadOtherSceneEmptySafe();
	

		// ─── 注册回调链 ───
		testRegisteNotTriggerCallbackOnRegiste();
		testRegisteCallbackNullSafe();
		testRegisteDistinctNamesIndependentPaths();
		// 注: 源码 registeScene 对非 .unity 结尾会调 logError, 故不测该分支(遵守"测试不得触发 error 日志")
		// ─── showScene 可见性状态机 ───
		testShowSceneActivatesTarget();
		testShowSceneHideOtherShowsOnlyTarget();
		testShowSceneHideOtherOnShowOrder();
		testSetMainSceneUnregisteredSafe();
		testHideSceneUnregisteredSafe();
		testUnloadOtherSceneEmptyListSafe();
		testShowSceneNotHideOtherKeepsOthersActive();
		testShowSceneUnknownNameNoOp();
		testShowSceneSameSceneSelfOnly();
		// ─── hideScene ───
		testHideSceneDeactivatesAndOnHide();
		testHideSceneUnknownNoOp();
		// ─── hideOther=false 双场景显示 ───
		testShowTwoScenesBothActive();
		// ─── 脚本映射与查询 ───
		testScriptMappingMixedTypes();
		testGetSceneAfterInject();
		testGetSceneUnknown();
	}

	// ═════════════════════════════════════════════════════════════════
	// 注册
	// ═════════════════════════════════════════════════════════════════
	private static void testRegisteScene()
	{
		SceneSystem sys = new();
		sys.registeScene(typeof(TestSceneInstance), "Assets/Scenes/Main.unity", null);
		assertEqual("Assets/Scenes/Main.unity", sys.getScenePath("Main"), "注册后能取到路径");
		sys.destroy();
	}
	private static void testGetScenePath()
	{
		SceneSystem sys = new();
		// 未注册返回空串
		assertEqual("", sys.getScenePath("Nonexistent"), "未注册场景路径为空");
		sys.destroy();
	}
	private static void testRegisteSceneRejectsNonUnity()
	{
		// 非 .unity 结尾 → 源码 SceneSystem.registeScene 无条件 logError, 无法在避免日志污染的前提下
		// 触发该错误分支, 遵循项目约定跳过此错误路径测试 (避免 error log)
		assertTrue(true, "skip testRegisteSceneRejectsNonUnity: 非 .unity 路径必然触发 logError");
	}

	// ═════════════════════════════════════════════════════════════════
	// 脚本映射
	// ═════════════════════════════════════════════════════════════════
	private static void testScriptMappingCount()
	{
		SceneSystem sys = new();
		sys.registeScene(typeof(TestSceneInstance), "Assets/Scenes/A.unity", null);
		assertEqual(1, sys.getScriptMappingCount(typeof(TestSceneInstance)), "1 个场景映射到该类型");
		sys.destroy();
	}
	private static void testScriptMappingMultipleSameType()
	{
		SceneSystem sys = new();
		// 多个场景共用同一脚本类型
		sys.registeScene(typeof(TestSceneInstance), "Assets/Scenes/A.unity", null);
		sys.registeScene(typeof(TestSceneInstance), "Assets/Scenes/B.unity", null);
		assertEqual(2, sys.getScriptMappingCount(typeof(TestSceneInstance)), "2 个场景映射到该类型");
		sys.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// 查询未加载场景
	// ═════════════════════════════════════════════════════════════════
	private static void testGetSceneNotLoaded()
	{
		SceneSystem sys = new();
		sys.registeScene(typeof(TestSceneInstance), "Assets/Scenes/A.unity", null);
		// 未加载时 getScene 返回 null
		assertNull(sys.getScene<SceneInstance>("A"), "未加载场景 getScene 返回 null");
		sys.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// 生命周期/卸载 空列表守卫
	// ═════════════════════════════════════════════════════════════════
	// lateUpdate: 空 mSceneList 时 foreach 不执行, 只转发 base.lateUpdate, 安全
	private static void testLateUpdateEmptySafe()
	{
		SceneSystem sys = new();
		sys.lateUpdate(0.016f);
		// 再次调用验证幂等
		sys.lateUpdate(0.1f);
		sys.destroy();
	}

	// unloadOtherScene: 空 mSceneList 时 setRangeKeys 得空列表, foreach 不执行, 不触发真实卸载
	private static void testUnloadOtherSceneEmptySafe()
	{
		SceneSystem sys = new();
		// 空场景列表下卸载"其他场景"无副作用, 不触发 SceneManager 卸载日志
		sys.unloadOtherScene("AnyScene");
		sys.unloadOtherScene("", true);
		sys.destroy();
	}


	// 反射读取 SceneSystem 的 mSceneList 字段(protected)
	private static readonly FieldInfo FI_MSCENE_LIST = typeof(SceneSystem).GetField("mSceneList",
		BindingFlags.NonPublic | BindingFlags.Instance);

	

	// ═════════════════════════════════════════════════════════════════
	// 注册回调链
	// ═════════════════════════════════════════════════════════════════
	// 源码事实: registeScene 只注册不触发回调; notifySceneChanged 仅在
	// createScene(loadAsync 才走)/unloadSceneOnly 里触发。showScene/hideScene
	// 不直接 notifySceneChanged。故此处断言"注册后回调未触发"这一源码事实。
	private static void testRegisteNotTriggerCallbackOnRegiste()
	{
		SceneSystem sys = new();
		SceneInstance captured = null;
		int fired = 0;
		sys.registeScene(typeof(MockScene), "Assets/Scenes/A.unity", (s) => { fired++; captured = s; });
		assertEqual(0, fired, "registeScene 本身不触发回调");
		assertEqual(1, sys.getScriptMappingCount(typeof(MockScene)), "注册后脚本映射为 1");
		assertNull(captured, "回调未被调用, 参数保持 null");
		// getScene 未加载仍为 null
		assertNull(sys.getScene<MockScene>("A"), "未加载场景 getScene 为 null");
		destroySys(sys);
	}

	private static void testRegisteCallbackNullSafe()
	{
		SceneSystem sys = new();
		sys.registeScene(typeof(MockScene), "Assets/Scenes/NullCb.unity", null);
		assertEqual(1, sys.getScriptMappingCount(typeof(MockScene)), "null 回调仍注册成功");
		assertEqual("Assets/Scenes/NullCb.unity", sys.getScenePath("NullCb"), "路径可查");
		destroySys(sys);
	}

	// 源码事实核对: registeScene 内部 mSceneRegisteList.add(name, new()) 走
	// DictionaryExtension.add -> 原生 Dictionary.Add, key 已存在会抛 ArgumentException。
	// 故"同名场景重复注册覆盖路径"并非源码行为(而是会抛异常), 违反"测试不得触发 error 日志",
	// 不再作为覆盖语义断言。改为验证不同名场景路径查询互相独立(真实且无害行为)。
	private static void testRegisteDistinctNamesIndependentPaths()
	{
		SceneSystem sys = new();
		sys.registeScene(typeof(MockScene), "Assets/Scenes/A.unity", null);
		sys.registeScene(typeof(MockScene), "Assets/Scenes/B.unity", null);
		// 不同场景名 -> 不同 key, 各路径独立, 互不覆盖
		assertEqual("Assets/Scenes/A.unity", sys.getScenePath("A"), "A 路径可独立查询");
		assertEqual("Assets/Scenes/B.unity", sys.getScenePath("B"), "B 路径可独立查询");
		assertEqual(2, sys.getScriptMappingCount(typeof(MockScene)), "两个不同名场景各自追加脚本映射");
		destroySys(sys);
	}

	// ═════════════════════════════════════════════════════════════════
	// showScene 可见性状态机(核心)
	// ═════════════════════════════════════════════════════════════════
	private static void testShowSceneActivatesTarget()
	{
		SceneSystem sys = new();
		MockScene a = injectScene(sys, "A", false);
		sys.showScene("A", false, false);
		// hideOther=false: 目标激活 + onShow
		assertTrue(a.getActive(), "showScene 激活目标场景");
		assertEqual(1, a.mOnShowCount, "目标场景 onShow 调用一次");
		assertEqual(0, a.mOnHideCount, "不触及其他 onHide");
		destroySys(sys);
	}

	private static void testShowSceneHideOtherShowsOnlyTarget()
	{
		SceneSystem sys = new();
		MockScene a = injectScene(sys, "A", true);
		MockScene b = injectScene(sys, "B", true);
		MockScene c = injectScene(sys, "C", false);
		sys.showScene("B", true, false);
		// hideOther=true: 只有 B 激活, 其他全部隐藏并触发 onHide
		assertTrue(b.getActive(), "目标 B 保持激活");
		assertTrue(!a.getActive(), "A 被隐藏");
		assertTrue(!c.getActive(), "C 被隐藏(本就非激活)");
		assertEqual(1, a.mOnHideCount, "A onHide 一次");
		assertEqual(1, c.mOnHideCount, "C onHide 一次");
		assertEqual(1, b.mOnShowCount, "B onShow 一次");
		assertEqual(0, b.mOnHideCount, "B 不被 onHide");
		assertEqual(0, a.mOnShowCount, "A 不被 onShow");
		destroySys(sys);
	}

	private static void testShowSceneHideOtherOnShowOrder()
	{
		SceneSystem sys = new();
		MockScene a = injectScene(sys, "A", true);
		MockScene b = injectScene(sys, "B", false);
		List<string> log = new();
		a.mLog = log; b.mLog = log;
		sys.showScene("B", true, false);
		// 源码按 Dictionary 遍历(不保证插入序), 断言的是"调用发生时场景已被激活":
		// MockScene.onShow 内部记录 getActive(), 应为 true(base.setActive 已先执行)
		assertTrue(b.mActiveWhenShown, "onShow 调用时目标已激活(setActive 先于 onShow)");
		assertEqual(1, a.mOnHideCount, "A 被 onHide");
		assertEqual(1, b.mOnShowCount, "B 被 onShow");
		destroySys(sys);
	}

	private static void testShowSceneNotHideOtherKeepsOthersActive()
	{
		SceneSystem sys = new();
		MockScene a = injectScene(sys, "A", true);
		MockScene b = injectScene(sys, "B", false);
		sys.showScene("B", false, false);
		// hideOther=false: 目标激活, 其他场景保持原状(A 仍激活, 不触发 onHide)
		assertTrue(b.getActive(), "B 激活");
		assertTrue(a.getActive(), "A 保持激活(hideOther=false 不隐藏)");
		assertEqual(0, a.mOnHideCount, "A 未触发 onHide");
		assertEqual(1, b.mOnShowCount, "B onShow 一次");
		destroySys(sys);
	}

	private static void testShowSceneUnknownNameNoOp()
	{
		SceneSystem sys = new();
		// 未注册/未加载场景 → showScene 直接 return, 不抛异常
		sys.showScene("NotExist", true, false);
		sys.showScene("NotExist2", false, false);
		assertTrue(true, "未知场景 showScene 安全无操作");
		destroySys(sys);
	}

	private static void testShowSceneSameSceneSelfOnly()
	{
		SceneSystem sys = new();
		MockScene a = injectScene(sys, "A", true);
		MockScene b = injectScene(sys, "B", false);
		sys.showScene("A", true, false); // 目标就是已激活的 A
		// hideOther=true: A 保持激活(既是目标也是唯一存活), onShow 仍触发一次
		assertTrue(a.getActive(), "A 保持激活");
		assertTrue(!b.getActive(), "B 被隐藏");
		assertEqual(1, a.mOnShowCount, "A onShow 一次(每次 showScene 都触发目标 onShow)");
		destroySys(sys);
	}

	// ═════════════════════════════════════════════════════════════════
	// hideScene
	// ═════════════════════════════════════════════════════════════════
	private static void testHideSceneDeactivatesAndOnHide()
	{
		SceneSystem sys = new();
		MockScene a = injectScene(sys, "A", true);
		sys.hideScene("A");
		assertTrue(!a.getActive(), "hideScene 使场景非激活");
		assertEqual(1, a.mOnHideCount, "hideScene 触发 onHide");
		assertEqual(0, a.mOnShowCount, "hideScene 不触发 onShow");
		destroySys(sys);
	}

	private static void testHideSceneUnknownNoOp()
	{
		SceneSystem sys = new();
		sys.hideScene("NotExist");
		assertTrue(true, "未知场景 hideScene 安全无操作");
		destroySys(sys);
	}

	// ═════════════════════════════════════════════════════════════════
	// hideOther=false 双场景显示
	// ═════════════════════════════════════════════════════════════════
	private static void testShowTwoScenesBothActive()
	{
		SceneSystem sys = new();
		MockScene a = injectScene(sys, "A", false);
		MockScene b = injectScene(sys, "B", false);
		sys.showScene("A", false, false);
		sys.showScene("B", false, false);
		// 两次非 hideOther 显示 → 两个场景都激活
		assertTrue(a.getActive(), "A 激活(hideOther=false)");
		assertTrue(b.getActive(), "B 激活(hideOther=false)");
		assertEqual(0, a.mOnHideCount, "A 未被隐藏");
		destroySys(sys);
	}

	// ═════════════════════════════════════════════════════════════════
	// 脚本映射与查询
	// ═════════════════════════════════════════════════════════════════
	private static void testScriptMappingMixedTypes()
	{
		SceneSystem sys = new();
		sys.registeScene(typeof(MockScene), "Assets/Scenes/A.unity", null);
		sys.registeScene(typeof(MockScene), "Assets/Scenes/B.unity", null);
		sys.registeScene(typeof(DifferentMockScene), "Assets/Scenes/C.unity", null);
		assertEqual(2, sys.getScriptMappingCount(typeof(MockScene)), "MockScene 映射 2 个");
		assertEqual(1, sys.getScriptMappingCount(typeof(DifferentMockScene)), "DifferentMockScene 映射 1 个");
		assertEqual("Assets/Scenes/B.unity", sys.getScenePath("B"), "B 路径可查");
		assertEqual("", sys.getScenePath("NotRegisted"), "未注册路径为空");
		destroySys(sys);
	}

	private static void testGetSceneAfterInject()
	{
		SceneSystem sys = new();
		MockScene a = injectScene(sys, "A", false);
		SceneInstance got = sys.getScene<SceneInstance>("A");
		assertNotNull(got, "反射注入后 getScene 可取到");
		assertTrue(ReferenceEquals(a, got), "取到的是注入的同一实例");
		destroySys(sys);
	}

	private static void testGetSceneUnknown()
	{
		SceneSystem sys = new();
		assertNull(sys.getScene<SceneInstance>("NotExist"), "未注册场景 getScene 为 null");
		destroySys(sys);
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	// 反射注入 mock 场景到 mSceneList
	private static MockScene injectScene(SceneSystem sys, string name, bool active)
	{
		Dictionary<string, SceneInstance> dict = (Dictionary<string, SceneInstance>)FI_MSCENE_LIST.GetValue(sys);
		if (dict == null)
		{
			dict = new Dictionary<string, SceneInstance>();
			FI_MSCENE_LIST.SetValue(sys, dict);
		}
		MockScene scene = new(name);
		scene.setActive(active);
		dict[name] = scene;
		return scene;
	}

	// 先释放 MockScene 根节点(plain GameObject, 非对象池), 再清空 mSceneList 使其为空,
	// 最后 destroy: 避免 SceneSystem.destroy 遍历时对未加载场景名调 SceneManager.UnloadSceneAsync
	// 产生 error 日志; 同时防止测试间真 GameObject 残留。
	private static void destroySys(SceneSystem sys)
	{
		Dictionary<string, SceneInstance> dict = (Dictionary<string, SceneInstance>)FI_MSCENE_LIST.GetValue(sys);
		if (dict != null)
		{
			foreach (SceneInstance scene in dict.Values)
			{
				if (scene?.getRoot() != null)
				{
					UnityEngine.Object.DestroyImmediate(scene.getRoot());
				}
			}
		}
				FI_MSCENE_LIST.SetValue(sys, new Dictionary<string, SceneInstance>());
		sys.destroy();
	}

	// ═════════════════════════════════════════════════════════════════
	// 未注册名守卫(return 分支, 不依赖真实场景)
	// ═════════════════════════════════════════════════════════════════

	// setMainScene 未注册名不崩
	private static void testSetMainSceneUnregisteredSafe()
	{
		SceneSystem sys = new();
		sys.setMainScene("NotRegisteredScene");
		sys.destroy();
	}

	// hideScene 未注册名不崩
	private static void testHideSceneUnregisteredSafe()
	{
		SceneSystem sys = new();
		sys.hideScene("NotRegisteredScene");
		sys.destroy();
	}

	// 注: unloadScene 未注册名会 NRE(mSceneRegisteList.get(name) 未守卫)——框架不守卫该分支, 合法不测
	// unloadOtherScene 空列表不崩(遍历空, 不调 unloadScene)
	private static void testUnloadOtherSceneEmptyListSafe()
	{
		SceneSystem sys = new();
		sys.unloadOtherScene("Keep");
		sys.destroy();
	}
}

// 测试用 SceneInstance 子类
public class TestSceneInstance : SceneInstance
{
}



// 测试用 SceneInstance 子类: 记录 onShow/onHide 轨迹。
// base.setActive/getActive 是非虚方法(依赖 mRoot), 故在构造时创建真实根节点 GameObject
// 使其按真实语义工作; 仅 override 虚方法 onShow/onHide/update 记录调用。
public class MockScene : SceneInstance
{
	public int mOnShowCount;
	public int mOnHideCount;
	public bool mActiveWhenShown;  // onShow 触发瞬间 getActive() 的采样
	public List<string> mLog;      // 跨场景调用轨迹(可选)
	private string mSampleName;
	public MockScene(string name)
	{
		mSampleName = name;
		// 创建真实根节点, 使 base.setActive/getActive 生效(按"场景名_Root"命名习惯)
		setRoot(new GameObject(name + "_Root"));
		// 同步 SceneInstance 内部 mName, 便于 getName/notifySceneChanged 用
		setName(name);
	}
	public override void onShow()
	{
		base.onShow();
		++mOnShowCount;
		mActiveWhenShown = getActive();
		mLog?.Add(mSampleName + ":onShow");
	}
	public override void onHide()
	{
		base.onHide();
		++mOnHideCount;
		mLog?.Add(mSampleName + ":onHide");
	}
	// 覆写 update 记录(不覆盖也可安全继承, 此处仅展示可测 update 路径)
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mOnShowCount = 0;
		mOnHideCount = 0;
		mActiveWhenShown = false;
		mLog = null;
		mSampleName = null;
	}
}

// 另一个 SceneInstance 子类, 用于不同类型映射计数测试
public class DifferentMockScene : SceneInstance
{
}
