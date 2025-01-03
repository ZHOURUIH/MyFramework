using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameBase;

[Serializable]
public class PrefabPoolDebugInfo
{
	public int InuseCount;
	public int UnuseCount;
	public string PrefabName;
	public string FileName;
}

// 从资源加载的物体池的调试信息
public class ObjectPoolDebug : MonoBehaviour
{
	public List<ObjectInfo> mInstanceListValues = new();   // 物体信息列表
	public List<PrefabPoolDebugInfo> mPrefabPoolInfo = new();	// 预设列表
	private void Update()
	{
		if (mGameFramework == null || !mGameFramework.mParam.mEnableScriptDebug || mPrefabPoolManager == null)
		{
			return;
		}

		mInstanceListValues.setRange(mPrefabPoolManager.getInstanceList().Values);

		mPrefabPoolInfo.Clear();
		foreach (var item in mPrefabPoolManager.getPrefabPoolList().getMainList())
		{
			PrefabPoolDebugInfo info = new();
			info.InuseCount = item.Value.getInuseCount();
			info.UnuseCount = item.Value.getUnuseCount();
			info.PrefabName = item.Value.getPrefab() != null ? item.Value.getPrefab().name : "null";
			info.FileName = item.Value.getFileName();
			mPrefabPoolInfo.Add(info);
		}
	}
}