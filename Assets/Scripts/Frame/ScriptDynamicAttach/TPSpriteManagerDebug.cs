using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameBase;

// 资源管理器调试信息
public class TPSpriteManagerDebug : MonoBehaviour
{
	public List<UGUIAtlasDebug> mAtlasList = new List<UGUIAtlasDebug>();	// 已加载的AssetBundle列表Value
	public List<UGUIAtlasDebug> mResourcesAtlasList = new List<UGUIAtlasDebug>();	// 已加载的AssetBundle列表Value
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		mAtlasList.Clear();
		mResourcesAtlasList.Clear();
#if UNITY_EDITOR
		var assetBundleAtlasList = mTPSpriteManager.getAtlasList();
		var resourcesAtlasList = mTPSpriteManager.getAtlasListInResources();
		foreach (var item in assetBundleAtlasList)
		{
			UGUIAtlasDebug info = new UGUIAtlasDebug();
			info.mAtlasName = item.Key;
			info.mRefCount = item.Value.getReferenceCount();
			mAtlasList.Add(info);
		}
		foreach (var item in resourcesAtlasList)
		{
			UGUIAtlasDebug info = new UGUIAtlasDebug();
			info.mAtlasName = item.Key;
			info.mRefCount = item.Value.getReferenceCount();
			mResourcesAtlasList.Add(info);
		}
#endif
	}
}