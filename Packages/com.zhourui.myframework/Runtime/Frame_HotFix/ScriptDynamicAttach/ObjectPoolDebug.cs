using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

[Serializable]
// 预制体池调试信息,用于在编辑器中显示对象池状态
public class PrefabPoolDebugInfo
{
	public int InuseCount;
	public int UnuseCount;
	public string PrefabName;
	public string FileName;
}

[Serializable]
// 预制体池调试信息,用于在编辑器中显示对象池状态
public class GameObjectDebugInfo
{
	public GameObject Object;
	public string FileWithPath;
	public int Tag;
}

// 从资源加载的物体池的调试信息
public class ObjectPoolDebug : MonoBehaviour
{
	public List<GameObjectDebugInfo> mInstanceListValues = new();	// 物体信息列表
	public List<PrefabPoolDebugInfo> mPrefabPoolInfo = new();	// 预设列表
	private void Update()
	{
		if (GameEntryBase.getInstance() == null || !GameEntryBase.getInstance().mFrameworkParam.mEnableScriptDebug || mPrefabPoolManager == null)
		{
			return;
		}

		mInstanceListValues.Clear();
		foreach (var item in mPrefabPoolManager.getInstanceList())
		{
			GameObjectDebugInfo info = new();
			info.Object = item.Value.getObject();
			info.FileWithPath = item.Value.getFileWithPath();
			info.Tag = item.Value.getTag();
			mInstanceListValues.add(info);
		}

		mPrefabPoolInfo.Clear();
		foreach (var item in mPrefabPoolManager.getPrefabPoolList())
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