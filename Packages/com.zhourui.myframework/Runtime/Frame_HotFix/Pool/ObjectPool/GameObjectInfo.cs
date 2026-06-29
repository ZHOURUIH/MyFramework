using UnityEngine;
using System;
using static StringUtility;
using static UnityUtility;
using static FrameBaseUtility;

// 已经从资源加载的物体的信息
[Serializable]
public class GameObjectInfo : ClassObject
{
	protected PrefabPool mPool;			// 所属的对象池
	protected GameObject mObject;		// 物体实例
	protected string mFileWithPath;		// 带GameResources下相对路径的文件名,不带后缀
	protected int mTag;					// 物体的标签,外部给物体添加标签后,方便统一对指定标签的物体进行销毁,从而不用指定具体的实例或名字
	protected bool mUsing;				// 是否正在使用
	protected bool mMoveToHide;         // 是否通过移动到远处来隐藏
	public override void destroy()
	{
		base.destroy();
		destroyObject();
	}
	public PrefabPool getPool()					{ return mPool; }
	public GameObject getObject()				{ return mObject; }
	public string getFileWithPath()				{ return mFileWithPath; }
	public int getTag()							{ return mTag; }
	public bool isUsing()						{ return mUsing; }
	public bool isMoveToHide()					{ return mMoveToHide; }
	public void setPool(PrefabPool pool)		{ mPool = pool; }
	public void setObject(GameObject obj)		{ mObject = obj; }
	public void setTag(int tag)					{ mTag = tag; }
	public void setUsing(bool value)			{ mUsing = value; }
	public void setMoveToHide(bool moveToHide)	{ mMoveToHide = moveToHide; }
	// 同步创建物体
	public void createObject(GameObject prefab, string fileWithPath)
	{
		if (prefab == null)
		{
			return;
		}
		mFileWithPath = fileWithPath;
		mObject = instantiatePrefab(null, prefab, getFileNameWithSuffix(prefab.name), true);
	}
	// 异步创建物体
	public void createObjectAsync(GameObject prefab, string fileWithPath, Action<GameObjectInfo> callback)
	{
		if (prefab == null)
		{
			callback?.Invoke(this);
			return;
		}
		mFileWithPath = fileWithPath;
#if UNITY_6000_0_OR_NEWER
		long curAssignID = mAssignID;
		instantiatePrefabAsync(prefab, getFileNameWithSuffix(prefab.name), true, (GameObject go)=> 
		{
			mObject = go;
			callback?.Invoke(curAssignID == mAssignID ? this : null);
		});
#else
		mObject = instantiatePrefab(null, prefab, getFileNameWithSuffix(prefab.name), true);
		callback?.Invoke(this);
#endif
	}
	// 销毁物体
	public void destroyObject()
	{
		destroyUnityObject(ref mObject);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mPool = null;
		mObject = null;
		mFileWithPath = null;
		mTag = 0;
		mUsing = false;
		mMoveToHide = false;
	}
}