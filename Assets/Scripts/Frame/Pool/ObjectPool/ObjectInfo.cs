using UnityEngine;
using System;

// 已经从资源加载的物体的信息
[Serializable]
public class ObjectInfo : FrameBase
{
	public GameObject mObject;		// 物体实例
	public string mFileWithPath;	// 带GameResources下相对路径的文件名,不带后缀
	public int mTag;				// 物体的标签,外部给物体添加标签后,方便统一对指定标签的物体进行销毁,从而不用指定具体的实例或名字
	public bool mUsing;				// 是否正在使用
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