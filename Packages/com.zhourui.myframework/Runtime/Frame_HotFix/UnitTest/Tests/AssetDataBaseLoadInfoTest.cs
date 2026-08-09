using UnityEngine;
using static TestAssert;

// AssetDataBaseLoadInfo 单元测试
// 纯数据类(继承 ClassObject), 可覆盖:
//   默认 getter (getObject null / getState NONE / getPath null / getResourceName null / getSubObjects null)
//   setter 链路 (setObject/setPath/setResourceName/setSubObjects/setState)
//   addCallback / callbackAll (回调触发 + 传参正确)
//   addCallback(null) 空安全
//   resetProperty 清空所有字段 + 回调列表
public static class AssetDataBaseLoadInfoTest
{
	public static void Run()
	{
		testConstruct();
		testDefaultGetters();
		testSetters();
		testCallbackAll();
		testAddCallbackNull();
		testResetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// 构造
	// ═════════════════════════════════════════════════════════════════
	private static void testConstruct()
	{
		AssetDataBaseLoadInfo info = new();
		assertNotNull(info, "AssetDataBaseLoadInfo 可构造");
		info.resetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// 默认 getter
	// ═════════════════════════════════════════════════════════════════
	private static void testDefaultGetters()
	{
		AssetDataBaseLoadInfo info = new();
		assertNull(info.getObject(), "默认 getObject 为 null");
		assertEqual(LOAD_STATE.NONE, info.getState(), "默认 getState 为 NONE");
		assertNull(info.getPath(), "默认 getPath 为 null");
		assertNull(info.getResourceName(), "默认 getResourceName 为 null");
		assertNull(info.getSubObjects(), "默认 getSubObjects 为 null");
		info.resetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// setter 链路
	// ═════════════════════════════════════════════════════════════════
	private static void testSetters()
	{
		AssetDataBaseLoadInfo info = new();
		info.setPath("effect/abc");
		info.setResourceName("abc.prefab");
		info.setState(LOAD_STATE.LOADED);
		Object[] subs = new Object[1];
		info.setSubObjects(subs);
		assertEqual("effect/abc", info.getPath(), "setPath 生效");
		assertEqual("abc.prefab", info.getResourceName(), "setResourceName 生效");
		assertEqual(LOAD_STATE.LOADED, info.getState(), "setState 生效");
		assertEqual(subs, info.getSubObjects(), "setSubObjects 生效");
		info.resetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// addCallback / callbackAll
	// ═════════════════════════════════════════════════════════════════
	private static void testCallbackAll()
	{
		AssetDataBaseLoadInfo info = new();
		Object obj = new GameObject("ResObj");
		Object[] subs = new Object[1] { obj };
		info.setObject(obj);
		info.setSubObjects(subs);
		int calls = 0;
		Object receivedAsset = null;
		string receivedPath = null;
		// AssetLoadCallback 签名: void(UObject asset, UObject[] assets, byte[] bytes, string loadPath)
		info.addCallback((asset, assets, bytes, loadPath) =>
		{
			++calls;
			receivedAsset = asset;
			receivedPath = loadPath;
		}, "load/path");
		info.callbackAll();
		assertEqual(1, calls, "callbackAll 应触发回调");
		assertEqual(obj, receivedAsset, "回调应收到 setObject 的 asset");
		assertEqual("load/path", receivedPath, "回调应收到 addCallback 的 loadPath");
		Object.DestroyImmediate(obj);
		info.resetProperty();
	}
	private static void testAddCallbackNull()
	{
		AssetDataBaseLoadInfo info = new();
		// addCallback(null, ...) 直接 return, 不抛异常
		info.addCallback(null, "path");
		info.callbackAll();
		info.resetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty
	// ═════════════════════════════════════════════════════════════════
	private static void testResetProperty()
	{
		AssetDataBaseLoadInfo info = new();
		Object obj = new GameObject("ResObj2");
		info.setObject(obj);
		info.setPath("p");
		info.setResourceName("n");
		info.setState(LOAD_STATE.LOADING);
		info.setSubObjects(new Object[1]);
		info.addCallback((a, b, c, d) => { }, "lp");
		info.resetProperty();
		assertNull(info.getObject(), "resetProperty 清空 mObject");
		assertNull(info.getPath(), "resetProperty 清空 mPath");
		assertNull(info.getResourceName(), "resetProperty 清空 mResourceName");
		assertEqual(LOAD_STATE.NONE, info.getState(), "resetProperty 重置 mState 为 NONE");
		assertNull(info.getSubObjects(), "resetProperty 清空 mSubObjects");
		Object.DestroyImmediate(obj);
	}
}
