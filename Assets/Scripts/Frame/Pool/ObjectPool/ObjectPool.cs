using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : FrameSystem
{
	protected Dictionary<string, Dictionary<GameObject, ObjectInfo>> mInstanceFileList;
	protected Dictionary<GameObject, ObjectInfo> mInstanceList;
	protected List<AsyncLoadGroup> mAsyncLoadGroup;
	protected AssetLoadDoneCallback mPrefabGroupCallback;       // 预先保存下函数的委托,避免传参时产生GC
	protected AssetLoadDoneCallback mPrefabCallback;			// 预先保存下函数的委托,避免传参时产生GC
	public ObjectPool()
	{
		mInstanceFileList = new Dictionary<string, Dictionary<GameObject, ObjectInfo>>();
		mInstanceList = new Dictionary<GameObject, ObjectInfo>();
		mAsyncLoadGroup = new List<AsyncLoadGroup>();
		mCreateObject = true;
		mPrefabGroupCallback = onPrefabGroupLoaded;
		mPrefabCallback = onPrefabLoaded;
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
		int groupCount = mAsyncLoadGroup.Count;
		if(groupCount > 0)
		{
			List<AsyncLoadGroup> tempList = newList(out tempList);
			for (int i = 0; i < groupCount; ++i)
			{
				if (mAsyncLoadGroup[i].isAllLoaded())
				{
					mAsyncLoadGroup[i].activeAll();
					tempList.Add(mAsyncLoadGroup[i]);
					mAsyncLoadGroup.RemoveAt(i--);
				}
			}
			int count = tempList.Count;
			for(int i = 0; i < count; ++i)
			{
				var item = tempList[i];
				item.mCallback(item.mNameList, item.mUserData);
				destroyClass(item);
			}
			destroyList(tempList);
		}
	}
	public void createObjectAsync(List<string> fileWithPath, CreateObjectGroupCallback callback, int objectTag, object userData = null)
	{
		var group = newClass(Typeof<AsyncLoadGroup>()) as AsyncLoadGroup;
		group.mCallback = callback;
		group.mUserData = userData;
		mAsyncLoadGroup.Add(group);
		int count = fileWithPath.Count;
		for(int i = 0; i < count; ++i)
		{
			var item = fileWithPath[i];
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
				mResourceManager.loadResourceAsync<GameObject>(item, mPrefabGroupCallback, objectTag);
			}
		}
	}
	// 异步创建物体,实际上只是异步加载,实例化还是同步的
	public void createObjectAsync(string fileWithPath, CreateObjectCallback callback, int objectTag, object userData = null)
	{
		// 物体如果已经有相同的,则直接返回
		ObjectInfo objInfo = getUnusedObject(fileWithPath);
		if (objInfo != null)
		{
			objInfo.setUsing(true);
			objectLoaded(objInfo.mObject, callback, userData);
		}
		else
		{
			var param = newClass(Typeof<PrefabLoadParam>()) as PrefabLoadParam;
			param.mCallback = callback;
			param.mTag = objectTag;
			param.mUserData = userData;
			// 预设未加载,异步加载预设
			mResourceManager.loadResourceAsync<GameObject>(fileWithPath, mPrefabCallback, param);
		}
	}
	// 同步创建物体
	public GameObject createObject(string fileWithPath, int objectTag)
	{
		ObjectInfo objInfo = getUnusedObject(fileWithPath);
		if (objInfo == null)
		{
			objInfo = newClass(Typeof<ObjectInfo>()) as ObjectInfo;
			GameObject prefab = mResourceManager.loadResource<GameObject>(fileWithPath);
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
	public void destroyAllWithTag(int objectTag)
	{
		List<ObjectInfo> tempList = newList(out tempList);
		foreach (var item in mInstanceList)
		{
			if (item.Value.getTag() == objectTag)
			{
				tempList.Add(item.Value);
			}
		}
		int count = tempList.Count;
		for(int i = 0; i < count; ++i)
		{
			destroyObject(ref tempList[i].mObject, true);
		}
		destroyList(tempList);
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
		if (!mInstanceList.TryGetValue(obj, out ObjectInfo info))
		{
			logError("can not find gameObject in ObjectPool! obj:" + obj.name);
			return;
		}
		info.setUsing(false);
		// 销毁物体
		if (destroyReally)
		{
			removeObject(info);
			info.destroyObject();
		}
		obj = null;
	}
	public bool isExistInPool(GameObject go) { return go != null && mInstanceList.ContainsKey(go); }
	public Dictionary<string, Dictionary<GameObject, ObjectInfo>> getInstanceFileList() {return mInstanceFileList;}
	public Dictionary<GameObject, ObjectInfo> getInstanceList() { return mInstanceList; }
	//------------------------------------------------------------------------------------------------------------------------
	protected void objectLoaded(GameObject go, CreateObjectCallback callback, object userData)
	{
		if (go != null)
		{
			setNormalProperty(go, mObject);
		}
		callback?.Invoke(go, userData);
	}
	protected void onPrefabGroupLoaded(Object asset, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		var objInfo = newClass(Typeof<ObjectInfo>()) as ObjectInfo;
		objInfo.setTag((int)userData);
		objInfo.setUsing(true);
		// 实例化,只能同步进行
		objInfo.createObject(asset as GameObject, loadPath);
		addObject(objInfo);
		int count = mAsyncLoadGroup.Count;
		for(int i = 0; i < count; ++i)
		{
			var item = mAsyncLoadGroup[i];
			if (item.mNameList.ContainsKey(objInfo.mFileWithPath))
			{
				item.mNameList[objInfo.mFileWithPath] = objInfo.mObject;
				// 资源组中的物体只有在全部加载完成后才激活
				activeObject(objInfo.mObject, false);
				objectLoaded(objInfo.mObject, null, null);
				break;
			}
		}
	}
	protected void onPrefabLoaded(Object asset, Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		PrefabLoadParam param = userData as PrefabLoadParam;
		if (asset == null)
		{
			destroyClass(param);
			return;
		}
		var objInfo = newClass(Typeof<ObjectInfo>()) as ObjectInfo;
		objInfo.setTag(param.mTag);
		objInfo.setUsing(true);
		// 实例化,只能同步进行
		objInfo.createObject(asset as GameObject, loadPath);
		addObject(objInfo);
		objectLoaded(objInfo.mObject, param.mCallback, param.mUserData);
		destroyClass(param);
	}
	protected ObjectInfo getUnusedObject(string fileWithPath)
	{
		if (!mInstanceFileList.TryGetValue(fileWithPath, out Dictionary<GameObject, ObjectInfo> list))
		{
			return null;
		}
		foreach (var item in list)
		{
			if (!item.Value.isUsing())
			{
				return item.Value;
			}
		}
		return null;
	}
	protected void addObject(ObjectInfo objInfo)
	{
		if (!mInstanceFileList.TryGetValue(objInfo.mFileWithPath, out Dictionary<GameObject, ObjectInfo> infoList))
		{
			infoList = new Dictionary<GameObject, ObjectInfo>();
			mInstanceFileList.Add(objInfo.mFileWithPath, infoList);
		}
		infoList.Add(objInfo.mObject, objInfo);
		if (!mInstanceList.ContainsKey(objInfo.mObject))
		{
			mInstanceList.Add(objInfo.mObject, objInfo);
		}
	}
	protected void removeObject(ObjectInfo objInfo)
	{
		if (mInstanceFileList.TryGetValue(objInfo.mFileWithPath, out Dictionary<GameObject, ObjectInfo> infoList))
		{
			infoList.Remove(objInfo.mObject);
		}
		mInstanceList.Remove(objInfo.mObject);
		destroyClass(objInfo);
	}
}