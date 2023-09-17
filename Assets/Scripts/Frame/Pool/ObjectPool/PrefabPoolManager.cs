using System;
using UnityEngine;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using static UnityUtility;
using static FrameUtility;
using static FrameBase;

// 从prefab实例化的物体对象池
public class PrefabPoolManager : FrameSystem
{
	protected Dictionary<GameObject, ObjectInfo> mInstanceList;		// 根据实例化的物体查找的列表
	protected Dictionary<string, PrefabPool> mPrefabPoolList;		// 已实例化对象的实例池列表
	protected List<AsyncLoadGroup> mAsyncLoadGroup;                 // 物体组的加载信息,物体组是用于同时加载多个物体,并且都加载完毕后才进行通知
	protected HashSet<string> mDontUnloadPrefab;					// 即使已经没有实例化对象了,也不会卸载的prefab
	public PrefabPoolManager()
	{
		mInstanceList = new Dictionary<GameObject, ObjectInfo>();
		mPrefabPoolList = new Dictionary<string, PrefabPool>();
		mAsyncLoadGroup = new List<AsyncLoadGroup>();
		mDontUnloadPrefab = new HashSet<string>();
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
		mPrefabPoolList = null;
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
#if UNITY_EDITOR
		foreach (var item in mInstanceList)
		{
			if (item.Value.mObject == null)
			{
				logError("Object can not be destroy outside of ObjectManager! filePath:" + item.Value.mFileWithPath);
			}
		}
#endif
		// 遍历加载组,组中所有资源加载完毕时调用回调
		// 为避免在调用的回调中再次异步加载资源组而引起的迭代器失效,所以使用另外一个临时列表
		int groupCount = mAsyncLoadGroup.Count;
		if(groupCount > 0)
		{
			using (new ListScope<AsyncLoadGroup>(out var tempList))
			{
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
				for (int i = 0; i < count; ++i)
				{
					AsyncLoadGroup item = tempList[i];
					item.mCallback(item.mNameList, item.mUserData);
					UN_CLASS(ref item);
				}
			}
		}
	}
	public void addDontUnloadPrefab(string name) { mDontUnloadPrefab.Add(name); }
	// 异步加载物体列表,当列表中的所有物体加载完成时才会调用回调
	public void createObjectGroupAsync(List<string> fileWithPath, CreateObjectGroupCallback callback, int objectTag, object userData = null)
	{
		CLASS(out AsyncLoadGroup group);
		group.mCallback = callback;
		group.mUserData = userData;
		mAsyncLoadGroup.Add(group);
		int count = fileWithPath.Count;
		for(int i = 0; i < count; ++i)
		{
			string item = fileWithPath[i];
			group.mNameList.Add(item, null);
			// 物体如果已经有相同的,则直接返回
			ObjectInfo objInfo = getUnusedObject(item);
			if (objInfo != null)
			{
				objInfo.setTag(objectTag);
				objInfo.mObject.SetActive(false);
				group.mNameList[item] = objInfo.mObject;
				continue;
			}
			if (mPrefabPoolList.TryGetValue(item, out PrefabPool prefabPool))
			{
				onPrefabGroupLoaded(prefabPool.getPrefab(), objectTag, item);
				continue;
			}
			// 预设未加载,异步加载预设
			mResourceManager.loadResourceAsync<GameObject>(item, (UObject asset, UObject[] subAssets, byte[] bytes, object userData, string loadPath) =>
			{
				onPrefabGroupLoaded(asset as GameObject, objectTag, loadPath);
			});
		}
	}
	// 异步创建物体,实际上只是异步加载,实例化还是同步的
	public void createObjectAsync(string fileWithPath, int objectTag, CreateObjectCallback callback)
	{
		// 物体如果已经有相同的,则直接返回
		ObjectInfo objInfo = getUnusedObject(fileWithPath);
		if (objInfo != null)
		{
			objInfo.setTag(objectTag);
			objectLoaded(objInfo.mObject, callback);
			return;
		}

		if (mPrefabPoolList.TryGetValue(fileWithPath, out PrefabPool prefabPool))
		{
			ObjectInfo newObjInfo = createNewInstance(objectTag, prefabPool.getPrefab(), fileWithPath);
			objectLoaded(newObjInfo.mObject, callback);
			return;
		}

		// 预设未加载,异步加载预设
		mResourceManager.loadResourceAsync<GameObject>(fileWithPath, (UObject asset, UObject[] subAssets, byte[] bytes, object loadUserData, string loadPath) =>
		{
			if (asset == null)
			{
				return;
			}
			ObjectInfo objInfo = createNewInstance(objectTag, asset as GameObject, loadPath);
			objectLoaded(objInfo.mObject, callback);
		});
	}
	// 同步创建物体
	public GameObject createObject(string fileWithPath, int objectTag, GameObject parent = null)
	{
		ObjectInfo objInfo = getUnusedObject(fileWithPath);
		if (objInfo == null)
		{
			if (mPrefabPoolList.TryGetValue(fileWithPath, out PrefabPool prefabPool))
			{
				objInfo = createNewInstance(objectTag, prefabPool.getPrefab(), fileWithPath);
			}
			else
			{
				objInfo = createNewInstance(objectTag, mResourceManager.loadResource<GameObject>(fileWithPath), fileWithPath);
			}
		}
		objInfo.setTag(objectTag);
		// 返回前先确保物体是挂接到预设管理器下的
		if (parent == null)
		{
			parent = mObject;
		}
		setNormalProperty(objInfo.mObject, parent);
		// 显示物体
		objInfo.mObject.SetActive(true);
		return objInfo.mObject;
	}
	public void destroyAllWithTag(int objectTag)
	{
		using (new ListScope<ObjectInfo>(out var tempList))
		{
			foreach (var item in mInstanceList)
			{
				if (item.Value.getTag() == objectTag)
				{
					tempList.Add(item.Value);
				}
			}
			int count = tempList.Count;
			for (int i = 0; i < count; ++i)
			{
				destroyObject(ref tempList[i].mObject, true);
			}
		}
	}
	public void destroyObject(ref GameObject obj, bool destroyReally)
	{
		if (obj == null)
		{
			return;
		}
		if (!mInstanceList.TryGetValue(obj, out ObjectInfo info))
		{
			logError("can not find gameObject in ObjectPool! obj:" + obj.name + ", HashCode:" + obj.GetHashCode());
			return;
		}

		string path = info.mFileWithPath;
		if (!mPrefabPoolList.TryGetValue(path, out PrefabPool prefabPool))
		{
			logError("找不到此预设的对象池:" + path);
			return;
		}
		if (destroyReally)
		{
			mInstanceList.Remove(obj);
		}
		prefabPool.destroyObject(info, destroyReally);
		// 如果已经没有任何实例化对象,则销毁此对象池
		if (prefabPool.isEmpty() && !mDontUnloadPrefab.Contains(prefabPool.getFileName()))
		{
			prefabPool.destroy();
			mPrefabPoolList.Remove(path);
		}
		obj = null;
	}
	public bool isExistInPool(GameObject go) { return go != null && mInstanceList.ContainsKey(go); }
	public Dictionary<GameObject, ObjectInfo> getInstanceList() { return mInstanceList; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void objectLoaded(GameObject go, CreateObjectCallback callback)
	{
		if (go != null)
		{
			setNormalProperty(go, mObject);
		}
		callback?.Invoke(go, null);
	}
	protected void onPrefabGroupLoaded(GameObject prefab, int tag, string loadPath)
	{
		ObjectInfo objInfo = createNewInstance(tag, prefab, loadPath);
		int count = mAsyncLoadGroup.Count;
		for(int i = 0; i < count; ++i)
		{
			var item = mAsyncLoadGroup[i];
			if (item.mNameList.ContainsKey(objInfo.mFileWithPath))
			{
				item.mNameList[objInfo.mFileWithPath] = objInfo.mObject;
				// 资源组中的物体只有在全部加载完成后才激活
				objInfo.mObject.SetActive(false);
				setNormalProperty(objInfo.mObject, mObject);
				break;
			}
		}
	}
	protected ObjectInfo getUnusedObject(string fileWithPath)
	{
		if (!mPrefabPoolList.TryGetValue(fileWithPath, out PrefabPool instance))
		{
			return null;
		}
		return instance.getOneUnused();
	}
	protected ObjectInfo createNewInstance(int tag, GameObject prefab, string filePath)
	{
		if (!mPrefabPoolList.TryGetValue(filePath, out PrefabPool instance))
		{
			instance = new PrefabPool(prefab, filePath);
			mPrefabPoolList.Add(filePath, instance);
		}
		ObjectInfo objInfo = instance.createObject(tag);
		if (!mInstanceList.ContainsKey(objInfo.mObject))
		{
			mInstanceList.Add(objInfo.mObject, objInfo);
		}
		return objInfo;
	}
}