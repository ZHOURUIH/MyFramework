using System.Collections.Generic;
using UnityEngine;

// 从资源加载的物体池的调试信息
public class ObjectPoolDebug : MonoBehaviour
{
	public List<ObjectInfo> mInstanceListValues = new List<ObjectInfo>();	// 物体信息列表
	private void Update()
	{
		if (FrameBase.mGameFramework == null || !FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		if (FrameBase.mPrefabPoolManager == null)
		{
			return;
		}
		mInstanceListValues.Clear();
		var instanceList = FrameBase.mPrefabPoolManager.getInstanceList();
		mInstanceListValues.AddRange(instanceList.Values);
	}
}