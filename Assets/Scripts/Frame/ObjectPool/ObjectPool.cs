using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : FrameComponent
{
	private List<ObjectInfo> mTempList;
	private List<AsyncLoadGroup> mTempList1;
	protected Dictionary<string, Dictionary<GameObject, ObjectInfo>> mInstanceFileList;
	protected Dictionary<GameObject, ObjectInfo> mInstanceList;
	protected List<AsyncLoadGroup> mAsyncLoadGroup;
	public ObjectPool(string name)
		: base(name)
	{
		mInstanceFileList = new Dictionary<string, Dictionary<GameObject, ObjectInfo>>();
		mInstanceList = new Dictionary<GameObject, ObjectInfo>();
		mTempList = new List<ObjectInfo>();
		mAsyncLoadGroup = new List<AsyncLoadGroup>();
		mTempList1 = new List<AsyncLoadGroup>();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<ObjectPoolDebug>();
#endif
	}
	public override void destroy()
	{
		foreach (var item in mInstanceList)
		{
			item.Value.destroyObject();
		}
		mInstanceList.Clear();
		mInstanceFileList.Clear();
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (var item in mInstanceList)
		{
			if (item.Key == null)
			{
				logError("Object can not be destroy outside of ObjectManager!");
			}
		}
		// 遍历加载组,组中所有资源加载完毕时调用回调
		// 为避免在调用的回调中再次异步加载资源组而引起的迭代器失效,所以使用另外一个临时列表
		mTempList1.Clear();
		for (int i = 0; i < mAsyncLoadGroup.Count; ++i)
		{
			if (mAsyncLoadGroup[i].isAllLoaded())
			{
				mAsyncLoadGroup[i].activeAll();
				mTempList1.Add(mAsyncLoadGroup[i]);
				mAsyncLoadGroup.RemoveAt(i--);
			}
		}
		foreach (var item in mTempList1)
		{
			item.mCallback(item.mNameList, item.mUserData);
			mClassPool.destroyClass(item);
		}
		mTempList1.Clear();
	}
	public void createObjectAsync(List<string> fileWithPath, CreateObjectGroupCallback callback, string objectTag, object userData = null)
	{
		AsyncLoadGroup group = null;
		mClassPool.newClass(out group);
		group.mCallback = callback;
		group.mUserData = userData;
		mAsyncLoadGroup.Add(group);
		foreach (var item in fileWithPath)
		{
			group.mNameList.Add(item, null);
			// 物体如果已经有相同的,则直接返回
			ObjectInfo objInfo = getUnusedObject(item);
			if (objInfo != null)
			{
				objInfo.setTag(objectTag);
				objInfo.setUsing(true);
				activeObject(objInfo.mObject, false);
				group.mNameList[item] = objInfo.mObject;
			}
			else
			{
				// 预设未加载,异步加载预设
				mResourceManager.loadResourceAsync<GameObject>(item, onPrefabGroupLoaded, objectTag, true);
			}
		}
	}
	// 异步创建物体,实际上只是异步加载,实例化还是同步的
	public void createObjectAsync(string fileWithPath, CreateObjectCallback callback, string objectTag, object userData)
	{
		// 物体如果已经有相同的,则直接返回
		ObjectInfo objInfo = getUnusedObject(fileWithPath);
		if (objInfo != null)
		{
			objInfo.setUsing(true);
			onObjectLoaded(objInfo.mObject, callback, userData);
		}
		else
		{
			object[] tempUserData = new object[] { callback, objectTag, userData };
			// 预设未加载,异步加载预设
			mResourceManager.loadResourceAsync<GameObject>(fileWithPath, onPrefabLoaded, tempUserData, true);
		}
	}
	// 同步创建物体
	public GameObject createObject(string fileWithPath, string objectTag)
	{
		ObjectInfo objInfo = getUnusedObject(fileWithPath);
		if (objInfo == null)
		{
			mClassPool.newClass(out objInfo);
			GameObject prefab = mResourceManager.loadResource<GameObject>(fileWithPath, true);
			if (prefab == null)
			{
				return null;
			}
			objInfo.createObject(prefab, fileWithPath);
			addObject(objInfo);
		}
		objInfo.setTag(objectTag);
		objInfo.setUsing(true);
		// 返回前先确保物体是挂接到预设管理器下的
		setNormalProperty(objInfo.mObject, mObject);
		// 显示物体
		objInfo.mObject.SetActive(true);
		return objInfo.mObject;
	}
	public void destroyAllWithTag(string objectTag)
	{
		mTempList.Clear();
		foreach (var item in mInstanceList)
		{
			if (item.Value.getTag() == objectTag)
			{
				mTempList.Add(item.Value);
			}
		}
		foreach (var item in mTempList)
		{
			destroyObject(ref item.mObject, true);
		}
	}
	public void destroyObject(ref GameObject obj, bool destroyReally)
	{
		if (obj == null)
		{
			return;
		}
		// 隐藏物体,并且将物体重新挂接到预设管理器下,重置物体变换
		obj.SetActive(false);
		setNormalProperty(obj, mObject);
		if (!mInstanceList.ContainsKey(obj))
		{
			logError("can not find gameObject in ObjectPool! obj:" + obj.name);
			return;
		}
		mInstanceList[obj].setUsing(false);
		// 销毁物体
		if (destroyReally)
		{
			ObjectInfo objInfo = mInstanceList[obj];
			removeObject(mInstanceList[obj]);
			objInfo.destroyObject();
		}
		obj = null;
	}
	public bool isExistInPool(GameObject go) { return go != null && mInstanceList.ContainsKey(go); }
	public Dictionary<string, Dictionary<GameObject, ObjectInfo>> getInstanceFileList() {return mInstanceFileList;}
	public Dictionary<GameObject, ObjectInfo> getInstanceList() { return mInstanceList; }
	//------------------------------------------------------------------------------------------------
	protected void onObjectLoaded(GameObject go, object userData0, object userData1)
	{
		if (go != null)
		{
			setNormalProperty(go, mObject);
		}
		if (userData0 != null && (userData0 as CreateObjectCallback) != null)
		{
			(userData0 as CreateObjectCallback)(go, userData1);
		}
	}
	protected void onPrefabGroupLoaded(Object asset, Object[] subAssets, byte[] bytes, object[] userData, string loadPath)
	{
		ObjectInfo objInfo;
		mClassPool.newClass(out objInfo);
		objInfo.setTag(userData[0] as string);
		objInfo.setUsing(true);
		// 实例化,只能同步进行
		objInfo.createObject(asset as GameObject, loadPath);
		addObject(objInfo);
		foreach (var item in mAsyncLoadGroup)
		{
			if (item.mNameList.ContainsKey(objInfo.mFileWithPath))
			{
				item.mNameList[objInfo.mFileWithPath] = objInfo.mObject;
				// 资源组中的物体只有在全部加载完成后才激活
				activeObject(objInfo.mObject, false);
				onObjectLoaded(objInfo.mObject, null, null);
				break;
			}
		}
	}
	protected void onPrefabLoaded(Object asset, Object[] subAssets, byte[] bytes, object[] userData, string loadPath)
	{
		if (asset != null)
		{
			ObjectInfo objInfo;
			mClassPool.newClass(out objInfo);
			objInfo.setTag(userData[1] as string);
			objInfo.setUsing(true);
			// 实例化,只能同步进行
			objInfo.createObject(asset as GameObject, loadPath);
			addObject(objInfo);
			onObjectLoaded(objInfo.mObject, userData[0], userData[2]);
		}
	}
	protected ObjectInfo getUnusedObject(string fileWithPath)
	{
		if (mInstanceFileList.ContainsKey(fileWithPath))
		{
			foreach (var item in mInstanceFileList[fileWithPath])
			{
				if (!item.Value.isUsing())
				{
					return item.Value;
				}
			}
		}
		return null;
	}
	protected void addObject(ObjectInfo objInfo)
	{
		if (!mInstanceFileList.ContainsKey(objInfo.mFileWithPath))
		{
			mInstanceFileList.Add(objInfo.mFileWithPath, new Dictionary<GameObject, ObjectInfo>());
		}
		mInstanceFileList[objInfo.mFileWithPath].Add(objInfo.mObject, objInfo);
		if (!mInstanceList.ContainsKey(objInfo.mObject))
		{
			mInstanceList.Add(objInfo.mObject, objInfo);
		}
	}
	protected void removeObject(ObjectInfo objInfo)
	{
		if (mInstanceFileList.ContainsKey(objInfo.mFileWithPath))
		{
			mInstanceFileList[objInfo.mFileWithPath].Remove(objInfo.mObject);
		}
		mInstanceList.Remove(objInfo.mObject);
		mClassPool.destroyClass(objInfo);
	}
}