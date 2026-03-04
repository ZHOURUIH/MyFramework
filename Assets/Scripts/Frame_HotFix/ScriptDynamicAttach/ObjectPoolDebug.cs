using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

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
	public List<GameObjectInfo> mInstanceListValues = new();	// 物体信息列表
	public List<PrefabPoolDebugInfo> mPrefabPoolInfo = new();	// 预设列表
	private void Update()
	{
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug || mPrefabPoolManager == null)
		{
			return;
		}

		mInstanceListValues.setRangeValues(mPrefabPoolManager.getInstanceList());

		mPrefabPoolInfo.Clear();
		foreach (var item in mPrefabPoolManager.getPrefabPoolList().getMainList())
		{
			PrefabPool pool = item.Value;
			PrefabPoolDebugInfo info = new();
			info.InuseCount = pool.getInuseCount();
			info.UnuseCount = pool.getUnuseCount();
			info.PrefabName = pool.getPrefab() != null ? pool.getPrefab().name : "null";
			info.FileName = pool.getFileName();
			mPrefabPoolInfo.Add(info);
		}
	}
}