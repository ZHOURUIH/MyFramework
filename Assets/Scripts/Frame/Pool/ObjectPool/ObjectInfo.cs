using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class ObjectInfo : FrameBase, IClassObject
{
	public GameObject mObject;
	public string mFileWithPath;
	public string mTag;
	public bool mUsing;
	public bool isUsing() { return mUsing; }
	public void setUsing(bool value) { mUsing = value; }
	public void setTag(string tag) { mTag = tag; }
	public string getTag() { return mTag; }
	// 创建物体,只能同步执行
	public void createObject(GameObject prefab, string fileWithPath)
	{
		mObject = instantiatePrefab(null, prefab, true);
		mObject.name = getFileNameNoSuffix(fileWithPath, true);
		mFileWithPath = fileWithPath;
	}
	// 销毁物体
	public void destroyObject()
	{
		destroyGameObject(ref mObject);
	}
	public void resetProperty()
	{
		mObject = null;
		mFileWithPath = null;
		mUsing = false;
		mTag = null;
	}
}