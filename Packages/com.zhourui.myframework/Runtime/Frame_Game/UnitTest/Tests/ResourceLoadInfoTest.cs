using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using static TestAssert;

// Frame_Game 精简层 ResourceLoadInfo 测试
// 实例类直接 new; 测试子类暴露 protected 字段验证
public static class ResourceLoadInfoTest
{
	public static void Run()
	{
		testDefaultState();
		testAddCallbackNullSafe();
		testAddCallbackCount();
		testCallbackAllTriggers();
		testCallbackAllClears();
		testSetGetRoundTrip();
	}

	// 测试子类暴露 protected 字段
	private class TestResourceLoadInfo : ResourceLoadInfo
	{
		public List<AssetLoadDoneCallback> peekCallbacks() { return mCallback; }
		public List<string> peekLoadPaths() { return mLoadPath; }
	}

	// 默认状态
	static void testDefaultState()
	{
		TestResourceLoadInfo info = new();
		assertEqual(0, info.peekCallbacks().Count, "默认无回调");
		assertEqual(0, info.peekLoadPaths().Count, "默认无路径");
		assertEqual(LOAD_STATE.NONE, info.getState(), "默认 LOAD_STATE.NONE");
		assertNull(info.getObject(), "默认对象 null");
		assertNull(info.getResourceName(), "默认资源名 null");
	}

	// addCallback(null) 安全
	static void testAddCallbackNullSafe()
	{
		TestResourceLoadInfo info = new();
		info.addCallback(null, "path");
		assertEqual(0, info.peekCallbacks().Count, "null 回调不加入");
	}

	// addCallback 计数与路径
	static void testAddCallbackCount()
	{
		TestResourceLoadInfo info = new();
		AssetLoadDoneCallback cb = (asset, assets, bytes, path) => { };
		info.addCallback(cb, "a");
		info.addCallback(cb, "b");
		assertEqual(2, info.peekCallbacks().Count, "2 个回调");
		assertEqual(2, info.peekLoadPaths().Count, "2 个路径");
	}

	// callbackAll 触发回调并传入 loadPath
	static void testCallbackAllTriggers()
	{
		TestResourceLoadInfo info = new();
		string gotPath = null;
		AssetLoadDoneCallback cb = (asset, assets, bytes, path) => { gotPath = path; };
		info.addCallback(cb, "hello");
		info.setObject(new GameObject("RLO_Obj"));
		info.callbackAll();
		assertEqual("hello", gotPath, "回调收到 loadPath");
	}

	// callbackAll 后清空列表
	static void testCallbackAllClears()
	{
		TestResourceLoadInfo info = new();
		int count = 0;
		AssetLoadDoneCallback cb = (asset, assets, bytes, path) => { ++count; };
		info.addCallback(cb, "x");
		info.addCallback(cb, "y");
		info.callbackAll();
		assertEqual(2, count, "两次回调触发");
		assertEqual(0, info.peekCallbacks().Count, "回调后列表清空");
	}

	// setter/getter 往返
	static void testSetGetRoundTrip()
	{
		TestResourceLoadInfo info = new();
		var go = new GameObject("RLO_SetGet");
		try
		{
			info.setPath("path/x");
			info.setResourceName("x.prefab");
			info.setObject(go);
			info.setSubObjects(new UObject[] { go });
			info.setState(LOAD_STATE.LOADED);
			assertEqual("path/x", info.getPath(), "path 往返");
			assertEqual("x.prefab", info.getResourceName(), "资源名往返");
			assertEqual(go, info.getObject(), "对象往返");
			assertEqual(1, info.getSubObjects().Length, "子物体数组长度");
			assertEqual(LOAD_STATE.LOADED, info.getState(), "状态往返");
		}
		finally
		{
			Object.DestroyImmediate(go);
		}
	}
}
