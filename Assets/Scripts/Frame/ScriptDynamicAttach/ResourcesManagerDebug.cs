using System;
using System.Collections.Generic;
using UnityEngine;

// 资源管理器调试信息
public class ResourcesManagerDebug : MonoBehaviour
{
	public List<string> mLoadedAssetBundleListKeys = new List<string>();						// 已加载的AssetBundle列表Key
	public List<AssetBundleDebug> mLoadedAssetBundleListValues = new List<AssetBundleDebug>();	// 已加载的AssetBundle列表Value
	public void Update()
	{
		if (FrameBase.mGameFramework == null || !FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		mLoadedAssetBundleListKeys.Clear();
		mLoadedAssetBundleListValues.Clear();
		AssetBundleLoader assetBundleLoader = FrameBase.mResourceManager.getAssetBundleLoader();
		var bundleList = assetBundleLoader.getAssetBundleInfoList();
		foreach(var item in bundleList)
		{
			if (item.Value.getLoadState() != LOAD_STATE.LOADED)
			{
				continue;
			}
			mLoadedAssetBundleListKeys.Add(item.Key);
			AssetBundleDebug bundleDebug = new AssetBundleDebug(item.Value.getBundleName());
			bundleDebug.mAssetList.Clear();
			bundleDebug.mParentBundles.Clear();
			bundleDebug.mChildBundles.Clear();
			bundleDebug.mAssetList.AddRange(item.Value.getAssetList().Values);
			bundleDebug.mParentBundles.AddRange(item.Value.getParents().Keys);
			bundleDebug.mChildBundles.AddRange(item.Value.getChildren().Keys);
			mLoadedAssetBundleListValues.Add(bundleDebug);
		}
	}
}