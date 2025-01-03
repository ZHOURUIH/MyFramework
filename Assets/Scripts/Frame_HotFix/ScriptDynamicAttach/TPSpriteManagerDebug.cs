using System.Collections.Generic;
using UnityEngine;
using static FrameBase;

// 资源管理器调试信息
public class TPSpriteManagerDebug : MonoBehaviour
{
	public List<UGUIAtlasDebug> mAtlasList = new();				// 已加载的AssetBundle列表Value
	public List<UGUIAtlasDebug> mResourcesAtlasList = new();	// 已加载的AssetBundle列表Value
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mParam.mEnableScriptDebug)
		{
			return;
		}
		mAtlasList.Clear();
		mResourcesAtlasList.Clear();
		foreach (var item in mTPSpriteManager.getAtlasList().getMainList())
		{
			UGUIAtlasDebug info = new();
			info.mAtlasName = item.Key;
			info.mRefCount = item.Value.getReferenceCount();
			mAtlasList.Add(info);
		}
		foreach (var item in mTPSpriteManager.getAtlasListInResources().getMainList())
		{
			UGUIAtlasDebug info = new();
			info.mAtlasName = item.Key;
			info.mRefCount = item.Value.getReferenceCount();
			mResourcesAtlasList.Add(info);
		}
	}
}