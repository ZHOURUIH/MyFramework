using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBase;
using static FrameUtility;

// 单个prefab的实例化池
public class PrefabPool
{
	protected HashSet<ObjectInfo> mInuseList;   // 正在使用的实例化列表,第一个key是文件名,第二个列表中的key是实例化出的物体,value是物品信息,为了提高运行时效率,仅在编辑器下使用
	protected List<ObjectInfo> mUnuseList;		// 未使用的实例化列表,第一个key是文件名,第二个列表中的key是实例化出的物体,value是物品信息
	protected GameObject mPrefab;				// 从资源管理器中加载的预设
	protected string mFileName;                 // 此实例物体的预设文件名,相对于GameResources的路径,带后缀
	public PrefabPool(GameObject prefab, string filePath)
	{
		mInuseList = new HashSet<ObjectInfo>();
		mUnuseList = new List<ObjectInfo>();
		mPrefab = prefab;
		mFileName = filePath;
	}
	public void destroy()
	{
		mResourceManager.unload(ref mPrefab);
	}
	public GameObject getPrefab() { return mPrefab; }
	public string getFileName() { return mFileName; }
	public bool isEmpty()
	{
		return mInuseList.Count == 0 && mUnuseList.Count == 0;
	}
	public ObjectInfo getOneUnused()
	{
		if (mUnuseList.Count == 0)
		{
			return null;
		}
		ObjectInfo obj = mUnuseList[mUnuseList.Count - 1];
		mUnuseList.RemoveAt(mUnuseList.Count - 1);
		mInuseList.Add(obj);
		obj.setUsing(true);
		return obj;
	}
	public ObjectInfo createObject(int tag)
	{
		CLASS(out ObjectInfo objInfo);
		objInfo.setTag(tag);
		// 实例化,只能同步进行
		objInfo.createObject(mPrefab, mFileName);
		objInfo.setUsing(true);
		mInuseList.Add(objInfo);
		return objInfo;
	}
	public void destroyObject(ObjectInfo obj, bool destroyReally)
	{
		if (!mInuseList.Remove(obj))
		{
			logError("从使用列表中移除失败:" + obj.mObject.name);
		}
		if (!destroyReally)
		{
			// 隐藏物体,并且将物体重新挂接到预设管理器下,重置物体变换
			obj.mObject.SetActive(false);
			setNormalProperty(obj.mObject, mPrefabPoolManager.getObject());
			obj.setUsing(false);
			mUnuseList.Add(obj);
		}
		else
		{
			obj.destroyObject();
			UN_CLASS(ref obj);
		}
	}
}