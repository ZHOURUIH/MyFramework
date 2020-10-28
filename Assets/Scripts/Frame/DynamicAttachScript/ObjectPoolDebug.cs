using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectPoolDebug : MonoBehaviour
{
	public List<string> InstanceFileListKeys = new List<string>();
	public List<GameObject> InstanceListKeys = new List<GameObject>();
	public List<ObjectInfo> InstanceListValues = new List<ObjectInfo>();
	private void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
		{
			return;
		}
		if (FrameBase.mObjectPool == null)
		{
			return;
		}
		InstanceFileListKeys.Clear();
		InstanceListKeys.Clear();
		InstanceListValues.Clear();
		var instanceFileList = FrameBase.mObjectPool.getInstanceFileList();
		InstanceFileListKeys.AddRange(instanceFileList.Keys);
		var instanceList = FrameBase.mObjectPool.getInstanceList();
		InstanceListKeys.AddRange(instanceList.Keys);
		InstanceListValues.AddRange(instanceList.Values);
	}
}