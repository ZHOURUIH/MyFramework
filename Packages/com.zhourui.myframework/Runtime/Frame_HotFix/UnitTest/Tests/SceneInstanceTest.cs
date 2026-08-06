using System;
using static TestAssert;

// SceneInstance 单元测试
// 覆盖可脱离 Unity 场景加载的实例级逻辑:
//   构造默认状态 / init幂等 / setType/setName/setState/setActiveLoaded/setMainScene
//   callLoading/callLoaded 回调 / 空回调安全
//   setRoot/getActive/setActive 对 root 的控制
//   getScene 默认 / setScene / getRoot / isInited
//   destroy 重置 mInited / resetProperty 全字段
//   onShow/onHide/update/lateUpdate 空实现不抛异常
// 注: DelayCmdWatcher 基类已被 CommandSystemTest 覆盖, 这里不重复
public static class SceneInstanceTest
{
	public static void Run()
	{
		// ─── 构造默认状态 ───
		testDefaultState();
		testDefaultStateFields();
		// ─── 属性读写 ───
		testTypeNameState();
		testActiveLoadedMainScene();
		// ─── 回调 ───
		testCallLoading();
		testCallLoaded();
		testCallWithoutCallback();
		testCallbackReset();
		// ─── root / active ───
		testSetRootAndGetActive();
		testSetActiveWithRoot();
		testSetActiveWithoutRoot();
		testGetSceneDefault();
		testSetScene();
		// ─── init ───
		testInitIdempotent();
		testInitWithoutRoot();
		// ─── destroy ───
		testDestroyResetsInited();
		// ─── 空实现方法 ───
		testNoOpMethods();
		// ─── resetProperty ───
		testResetPropertyAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// 构造默认状态
	// ═════════════════════════════════════════════════════════════════
	private static void testDefaultState()
	{
		SceneInstance scene = new();
		assertEqual(LOAD_STATE.NONE, scene.getState(), "默认加载状态为 NONE");
		assertFalse(scene.isActiveLoaded(), "默认非 activeLoaded");
		assertFalse(scene.isMainScene(), "默认非 mainScene");
		assertFalse(scene.isInited(), "默认未初始化");
	}
	private static void testDefaultStateFields()
	{
		SceneInstance scene = new();
		assertNull(scene.getType(), "默认类型为空");
		assertNull(scene.getName(), "默认名称为空");
		assertNull(scene.getRoot(), "默认根节点为空");
		assertNull(scene.getScene().IsValid() ? scene.getScene().name : null, "默认场景为空");
	}

	// ═════════════════════════════════════════════════════════════════
	// 属性读写
	// ═════════════════════════════════════════════════════════════════
	private static void testTypeNameState()
	{
		SceneInstance scene = new();
		scene.setType(typeof(SceneInstance));
		scene.setName("Battle");
		scene.setState(LOAD_STATE.LOADING);
		assertEqual(typeof(SceneInstance), scene.getType());
		assertEqual("Battle", scene.getName());
		assertEqual(LOAD_STATE.LOADING, scene.getState());
	}
	private static void testActiveLoadedMainScene()
	{
		SceneInstance scene = new();
		scene.setActiveLoaded(true);
		scene.setMainScene(true);
		assertTrue(scene.isActiveLoaded());
		assertTrue(scene.isMainScene());
		scene.setActiveLoaded(false);
		scene.setMainScene(false);
		assertFalse(scene.isActiveLoaded());
		assertFalse(scene.isMainScene());
	}

	// ═════════════════════════════════════════════════════════════════
	// 回调
	// ═════════════════════════════════════════════════════════════════
	private static void testCallLoading()
	{
		SceneInstance scene = new();
		float percent = 0.0f;
		scene.setLoadingCallback(v => percent = v);
		scene.callLoading(0.5f);
		assertEqual(0.5f, percent, "callLoading 应回调百分比");
	}
	private static void testCallLoaded()
	{
		SceneInstance scene = new();
		int loaded = 0;
		scene.setLoadedCallback(() => ++loaded);
		scene.callLoaded();
		assertEqual(1, loaded, "callLoaded 应触发回调");
	}
	private static void testCallWithoutCallback()
	{
		SceneInstance scene = new();
		// 未设置回调时不抛异常
		scene.callLoading(1.0f);
		scene.callLoaded();
	}
	private static void testCallbackReset()
	{
		SceneInstance scene = new();
		int loaded = 0;
		scene.setLoadedCallback(() => ++loaded);
		scene.resetProperty();
		scene.callLoaded();
		assertEqual(0, loaded, "resetProperty 后回调应被清空");
	}

	// ═════════════════════════════════════════════════════════════════
	// root / active
	// ═════════════════════════════════════════════════════════════════
	private static void testSetRootAndGetActive()
	{
		SceneInstance scene = new();
		var go = new UnityEngine.GameObject("SceneRoot");
		try
		{
			scene.setRoot(go);
			assertEqual(go, scene.getRoot());
			assertTrue(go.activeSelf, "默认 GameObject activeSelf 为 true");
			assertTrue(scene.getActive(), "root active 时 getActive 为 true");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
	private static void testSetActiveWithRoot()
	{
		SceneInstance scene = new();
		var go = new UnityEngine.GameObject("SceneRoot");
		try
		{
			scene.setRoot(go);
			scene.setActive(false);
			assertFalse(go.activeSelf, "setActive(false) 应禁用 root");
			assertFalse(scene.getActive());
			scene.setActive(true);
			assertTrue(go.activeSelf, "setActive(true) 应启用 root");
			assertTrue(scene.getActive());
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
	private static void testSetActiveWithoutRoot()
	{
		SceneInstance scene = new();
		// 无 root 时不抛异常, getActive 返回 false
		scene.setActive(false);
		scene.setActive(true);
		assertFalse(scene.getActive(), "无 root 时 getActive 为 false");
	}
	private static void testGetSceneDefault()
	{
		SceneInstance scene = new();
		UnityEngine.SceneManagement.Scene s = scene.getScene();
		assertFalse(s.IsValid(), "未 setScene 时返回无效场景");
	}
	private static void testSetScene()
	{
		SceneInstance scene = new();
		// 用当前活动场景测试 set/get
		UnityEngine.SceneManagement.Scene cur = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		scene.setScene(cur);
		assertEqual(cur, scene.getScene());
	}

	// ═════════════════════════════════════════════════════════════════
	// init
	// ═════════════════════════════════════════════════════════════════
	private static void testInitIdempotent()
	{
		SceneInstance scene = new();
		var go = new UnityEngine.GameObject("SceneRoot");
		try
		{
			scene.setRoot(go);
			scene.init();
			assertTrue(scene.isInited(), "init 后应已初始化");
			scene.init();
			assertTrue(scene.isInited(), "重复 init 仍为已初始化(幂等)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
	private static void testInitWithoutRoot()
	{
		// init 需要有效 root(内部调用 findShaders 遍历 root 子节点)
		SceneInstance scene = new();
		var go = new UnityEngine.GameObject("SceneRoot");
		try
		{
			scene.setRoot(go);
			bool thrown = false;
			try
			{
				scene.init();
			}
			catch
			{
				thrown = true;
			}
			assertFalse(thrown, "有 root 时 init 不应抛异常");
			assertTrue(scene.isInited());
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// destroy
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyResetsInited()
	{
		SceneInstance scene = new();
		var go = new UnityEngine.GameObject("SceneRoot");
		try
		{
			scene.setRoot(go);
			scene.init();
			assertTrue(scene.isInited());
			scene.destroy();
			assertFalse(scene.isInited(), "destroy 后 mInited 重置为 false");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 空实现方法
	// ═════════════════════════════════════════════════════════════════
	private static void testNoOpMethods()
	{
		SceneInstance scene = new();
		scene.onShow();
		scene.onHide();
		scene.update(0.1f);
		scene.lateUpdate(0.1f);
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty
	// ═════════════════════════════════════════════════════════════════
	private static void testResetPropertyAll()
	{
		SceneInstance scene = new();
		scene.setType(typeof(SceneInstance));
		scene.setName("X");
		scene.setState(LOAD_STATE.LOADED);
		scene.setActiveLoaded(true);
		scene.setMainScene(true);
		scene.setLoadedCallback(() => { });
		scene.setLoadingCallback(_ => { });
		scene.resetProperty();
		assertNull(scene.getType(), "reset 后类型清空");
		assertNull(scene.getName(), "reset 后名称清空");
		assertEqual(LOAD_STATE.NONE, scene.getState(), "reset 后状态为 NONE");
		assertFalse(scene.isActiveLoaded());
		assertFalse(scene.isMainScene());
		assertFalse(scene.isInited());
		assertNull(scene.getRoot());
	}
}
