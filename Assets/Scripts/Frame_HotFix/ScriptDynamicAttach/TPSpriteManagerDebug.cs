using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 资源管理器调试信息
public class TPSpriteManagerDebug : MonoBehaviour
{
	public List<UGUIAtlasDebug> mAtlasList = new();				// 已加载的AssetBundle列表Value
	public List<UGUIAtlasDebug> mResourcesAtlasList = new();	// 已加载的AssetBundle列表Value
	public void Update()
	{
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug)
		{
			return;
		}
		mAtlasList.Clear();
		mResourcesAtlasList.Clear();
		foreach (var item in mAtlasManager.getAtlasList().getMainList())
		{
			UGUIAtlasDebug info = new();
			info.mAtlasName = item.Key;
			info.mRefCount = item.Value.getReferenceCount();
			mAtlasList.Add(info);
		}
		foreach (var item in mAtlasManager.getAtlasListInResources().getMainList())
		{
			UGUIAtlasDebug info = new();
			info.mAtlasName = item.Key;
			info.mRefCount = item.Value.getReferenceCount();
			mResourcesAtlasList.Add(info);
		}
	}
}