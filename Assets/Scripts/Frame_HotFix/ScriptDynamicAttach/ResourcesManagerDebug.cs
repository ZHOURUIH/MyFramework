using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 资源管理器调试信息
public class ResourcesManagerDebug : MonoBehaviour
{
	public List<string> mLoadedAssetBundleListKeys = new();				// 已加载的AssetBundle列表Key
	public List<AssetBundleDebug> mLoadedAssetBundleListValues = new();	// 已加载的AssetBundle列表Value
	public void Update()
	{
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug)
		{
			return;
		}
		mLoadedAssetBundleListKeys.Clear();
		mLoadedAssetBundleListValues.Clear();
		foreach(var item in mResourceManager.getAssetBundleLoader().getAssetBundleInfoList())
		{
			if (item.Value.getLoadState() != LOAD_STATE.LOADED)
			{
				continue;
			}
			mLoadedAssetBundleListKeys.Add(item.Key);
			AssetBundleDebug bundleDebug = new(item.Value.getBundleName());
			bundleDebug.mAssetList.setRange(item.Value.getAssetList().Values);
			bundleDebug.mParentBundles.setRange(item.Value.getParents().Keys);
			bundleDebug.mChildBundles.setRange(item.Value.getChildren().Keys);
			mLoadedAssetBundleListValues.Add(bundleDebug);
		}
	}
}