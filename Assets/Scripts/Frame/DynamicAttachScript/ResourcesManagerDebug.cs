using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct AssetBundleDebug
{
	public string mBundleName;
	public List<AssetInfo> mAssetList;
	public List<string> mParentBundles;
	public List<string> mChildBundles;
	public AssetBundleDebug(string name)
	{
		mBundleName = name;
		mAssetList = new List<AssetInfo>();
		mParentBundles = new List<string>();
		mChildBundles = new List<string>();
	}
}

public class ResourcesManagerDebug : MonoBehaviour
{
	// 已加载的AssetBundle列表
	public List<string> mLoadedAssetBundleListKeys = new List<string>();
	public List<AssetBundleDebug> mLoadedAssetBundleListValues = new List<AssetBundleDebug>();
	public void Update()
	{
		mLoadedAssetBundleListKeys.Clear();
		mLoadedAssetBundleListValues.Clear();
		AssetBundleLoader assetBundleLoader = FrameBase.mResourceManager.getAssetBundleLoader();
		var bundleList = assetBundleLoader.getAssetBundleInfoList();
		foreach(var item in bundleList)
		{
			if(item.Value.getAssetBundle() != null)
			{
				mLoadedAssetBundleListKeys.Add(item.Key);
				AssetBundleDebug bundleDebug = new AssetBundleDebug(item.Value.getBundleName());
				bundleDebug.mAssetList.AddRange(item.Value.getAssetList().Values);
				bundleDebug.mParentBundles.AddRange(item.Value.getParents().Keys);
				bundleDebug.mChildBundles.AddRange(item.Value.getChildren().Keys);
				mLoadedAssetBundleListValues.Add(bundleDebug);
			}
		}
	}
}