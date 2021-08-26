using UnityEngine;
using System;

[Serializable]
public class ObjectInfo : FrameBase
{
	public GameObject mObject;
	public string mFileWithPath;
	public int mTag;				// 物体的标签,外部给物体添加标签后,方便统一对指定标签的物体进行销毁,从而不用指定具体的实例或名字
	public bool mUsing;
	public bool isUsing() { return mUsing; }
	public void setUsing(bool value) { mUsing = value; }
	public void setTag(int tag) { mTag = tag; }
	public int getTag() { return mTag; }
	// 创建物体,只能同步执行
	public void createObject(GameObject prefab, string fileWithPath)
	{
		mObject = instantiatePrefab(null, prefab, getFileName(prefab.name), true);
		mObject.name = getFileNameNoSuffix(fileWithPath, true);
		mFileWithPath = fileWithPath;
	}
	// 销毁物体
	public void destroyObject()
	{
		destroyGameObject(ref mObject);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mObject = null;
		mFileWithPath = null;
		mUsing = false;
		mTag = 0;
	}
}