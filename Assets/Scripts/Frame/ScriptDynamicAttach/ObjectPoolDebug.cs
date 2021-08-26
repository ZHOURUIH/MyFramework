using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolDebug : MonoBehaviour
{
	public List<string> mInstanceFileListKeys = new List<string>();
	public List<GameObject> mInstanceListKeys = new List<GameObject>();
	public List<ObjectInfo> mInstanceListValues = new List<ObjectInfo>();
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