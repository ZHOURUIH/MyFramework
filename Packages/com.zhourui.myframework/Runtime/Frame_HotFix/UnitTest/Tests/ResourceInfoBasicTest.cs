using UnityEngine;
using static TestAssert;

public static class ResourceInfoBasicTest
{
	public static void Run()
	{
		testAssetInfoStateAndSubAssets();
		testAssetDataBaseLoadInfoStateAndCallbacks();
		testAssetBundleInfoBasicRelations();
	}
	private static void testAssetInfoStateAndSubAssets()
	{
		AssetBundleInfo bundle = new("ui/common");
		AssetInfo info = new();
		info.setAssetBundleInfo(bundle);
		info.setAssetName("icon.prefab");
		assertEqual(bundle, info.getAssetBundle());
		assertEqual("icon.prefab", info.getAssetName());
		assertFalse(info.isLoaded());
		info.setLoadState(LOAD_STATE.WAIT_FOR_LOAD);
		assertEqual(LOAD_STATE.WAIT_FOR_LOAD, info.getLoadState());
		var obj = ScriptableObject.CreateInstance<TestAsset>();
		try
		{
			info.setSubAssets(new Object[]{ obj });
			assertTrue(info.isLoaded());
			assertEqual(LOAD_STATE.LOADED, info.getLoadState());
			assertEqual(obj, info.getAsset());
			info.clear();
			assertFalse(info.isLoaded());
			assertEqual(LOAD_STATE.NONE, info.getLoadState());
			info.setSubAssets(new Object[]{ null });
			assertFalse(info.isLoaded(), "包含 null 的资源数组应视为加载失败");
			assertEqual(LOAD_STATE.NONE, info.getLoadState());
		}
		finally
		{
			Object.DestroyImmediate(obj);
		}
		info.resetProperty();
		assertNull(info.getAssetBundle());
		assertNull(info.getAssetName());
		assertEqual(LOAD_STATE.NONE, info.getLoadState());
	}
	private static void testAssetDataBaseLoadInfoStateAndCallbacks()
	{
		AssetDataBaseLoadInfo info = new();
		info.setPath("UI/Icon");
		info.setResourceName("UI/Icon/a.png");
		info.setState(LOAD_STATE.LOADED);
		var obj = ScriptableObject.CreateInstance<TestAsset>();
		try
		{
			info.setObject(obj);
			info.setSubObjects(new Object[]{ obj });
			int callbackCount = 0;
			string callbackPath = null;
			info.addCallback((asset, assets, bytes, loadPath) =>
			{
				++callbackCount;
				assertEqual(obj, asset);
				assertEqual(obj, assets[0]);
				callbackPath = loadPath;
			}, "load/path");
			info.addCallback(null, "null callback ignored");
			info.callbackAll();
			assertEqual(1, callbackCount);
			assertEqual("load/path", callbackPath);
			assertEqual(0, info.mCallback.Count, "callbackAll 后应移空回调列表");
			assertEqual(0, info.mLoadPath.Count, "callbackAll 后应移空路径列表");
		}
		finally
		{
			Object.DestroyImmediate(obj);
		}
		assertEqual("UI/Icon", info.getPath());
		assertEqual("UI/Icon/a.png", info.getResourceName());
		assertEqual(LOAD_STATE.LOADED, info.getState());
		info.resetProperty();
		assertNull(info.getObject());
		assertNull(info.getSubObjects());
		assertNull(info.getPath());
		assertNull(info.getResourceName());
		assertEqual(LOAD_STATE.NONE, info.getState());
	}
	private static void testAssetBundleInfoBasicRelations()
	{
		AssetBundleInfo bundle = new("ui/common");
		assertEqual("ui/common", bundle.getBundleName());
		assertTrue(bundle.getBundleFileName().Contains("ui/common"), "BundleFileName 应包含 bundleName");
		bundle.addAssetName("a.prefab");
		AssetInfo asset = bundle.getAssetInfo("a.prefab");
		assertNotNull(asset);
		assertEqual(bundle, asset.getAssetBundle());
		assertEqual("a.prefab", asset.getAssetName());
		bundle.addParent("dep1");
		assertTrue(bundle.getParents().ContainsKey("dep1"));
		AssetBundleInfo child = new("child");
		bundle.addChild(child);
		assertTrue(bundle.getChildren().ContainsKey("child"));
		bundle.setLoadState(LOAD_STATE.LOADED);
		assertEqual(LOAD_STATE.LOADED, bundle.getLoadState());
		bundle.resetProperty();
		assertEqual(0, bundle.getParents().Count);
		assertEqual(0, bundle.getChildren().Count);
		assertEqual(0, bundle.getAssetList().Count);
		assertEqual(LOAD_STATE.NONE, bundle.getLoadState());
		assertEqual("ui/common", bundle.getBundleName(), "resetProperty 不应重置 bundleName");
	}
	private class TestAsset : ScriptableObject {}
}