using System.Collections.Generic;
using UnityEngine;

// 从资源加载的物体池的调试信息
public class ObjectPoolDebug : MonoBehaviour
{
	public List<string> mInstanceFileListKeys = new List<string>();			// 文件列表
	public List<GameObject> mInstanceListKeys = new List<GameObject>();		// 物体对象列表
	public List<ObjectInfo> mInstanceListValues = new List<ObjectInfo>();	// 物体信息列表
	private void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		if (FrameBase.mObjectPool == null)
		{
			return;
		}
		mInstanceFileListKeys.Clear();
		mInstanceListKeys.Clear();
		mInstanceListValues.Clear();
		var instanceFileList = FrameBase.mObjectPool.getInstanceFileList();
		mInstanceFileListKeys.AddRange(instanceFileList.Keys);
		var instanceList = FrameBase.mObjectPool.getInstanceList();
		mInstanceListKeys.AddRange(instanceList.Keys);
		mInstanceListValues.AddRange(instanceList.Values);
	}
}