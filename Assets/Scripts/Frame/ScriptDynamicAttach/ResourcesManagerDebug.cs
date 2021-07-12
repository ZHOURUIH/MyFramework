using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManagerDebug : MonoBehaviour
{
	// 已加载的AssetBundle列表
	public List<string> mLoadedAssetBundleListKeys = new List<string>();
	public List<AssetBundleDebug> mLoadedAssetBundleListValues = new List<AssetBundleDebug>();
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		mLoadedAssetBundleListKeys.Clear();
		mLoadedAssetBundleListValues.Clear();
		AssetBundleLoader assetBundleLoader = FrameBase.mResourceManager.getAssetBundleLoader();
		var bundleList = assetBundleLoader.getAssetBundleInfoList();
		foreach(var item in bundleList)
		{
			if(item.Value.getAssetBundle() == null)
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