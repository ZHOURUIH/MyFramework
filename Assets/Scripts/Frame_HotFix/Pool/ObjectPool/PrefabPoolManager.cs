using System;
using UnityEngine;
using System.Collections.Generic;
using UObject = UnityEngine.Object;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static FrameBaseUtility;

// 从prefab实例化的物体对象池
public class PrefabPoolManager : FrameSystem
{
	protected Dictionary<GameObject, ObjectInfo> mInstanceList = new();     // 根据实例化的物体查找的列表
	protected SafeDictionary<string, PrefabPool> mPrefabPoolList = new();	// 已实例化对象的实例池列表
	protected HashSet<string> mDontUnloadPrefab = new();                    // 即使已经没有实例化对象了,也不会卸载的prefab
	protected float mTimerInterval = 3.0f;									// 扫描间隔,默认3秒
	protected float mDestroyTimer;                                          // 扫描是否有需要卸载的资源的计时器
	public PrefabPoolManager()
	{
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
		if (isEditor())
		{
			mObject.AddComponent<ObjectPoolDebug>();
		}
		mResourceManager.addUnloadPathCallback((string path)=>
		{
			// 找到此路径中所有的Prefab,将PrefabPool销毁
			using var a = new SafeDictionaryReader<string, PrefabPool>(mPrefabPoolList);
			foreach (var item in a.mReadList)
			{
				if (!item.Key.startWith(path))
				{
					continue;
				}
				mPrefabPoolList.remove(item.Key);
				// 需要将实例化列表中的属于此对象池的所有对象也一起销毁
				PrefabPool pool = item.Value;
				foreach (ObjectInfo obj in pool.getInuseList())
				{
					mInstanceList.Remove(obj.getObject());
				}
				foreach (ObjectInfo obj in pool.getUnuseList())
				{
					mInstanceList.Remove(obj.getObject());
				}
				UN_CLASS(ref pool);
			}
		});
		mResourceManager.addUnloadObjectCallback((UObject obj) =>
		{
			if (obj is not GameObject)
			{
				return;
			}
			// 找到对应的PrefabPool将其销毁
			foreach (var item in mPrefabPoolList.getMainList())
			{
				PrefabPool pool = item.Value;
				if (pool.getPrefab() == obj)
				{
					UN_CLASS(ref pool);
					mPrefabPoolList.remove(item.Key);
					break;
				}
			}
		});
	}
	public override void destroy()
	{
		mInstanceList.Clear();
		UN_CLASS_LIST(mPrefabPoolList.getMainList());
		mPrefabPoolList.clear();
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 每隔一定时间销毁不再使用的对象池
		if (tickTimerLoop(ref mDestroyTimer, elapsedTime, mTimerInterval))
		{
			using var a = new SafeDictionaryReader<string, PrefabPool>(mPrefabPoolList);
			foreach (PrefabPool pool in a.mReadList.Values)
			{
				if (!pool.isEmptyInUse())
				{
					continue;
				}
				foreach (ObjectInfo obj in pool.getUnuseList())
				{
					if (obj?.getObject() == null)
					{
						logWarning("object is null:" + obj?.getFileWithPath());
						continue;
					}
					mInstanceList.Remove(obj.getObject());
				}
				destroyPool(pool);
			}
		}
		if (isEditor())
		{
			foreach (ObjectInfo item in mInstanceList.Values)
			{
				if (item.getObject() == null)
				{
					logError("Object can not be destroy outside of PrefabPoolManager! filePath:" + item.getFileWithPath());
				}
			}
		}
	}
	public float getTimerInterval() { return mTimerInterval; }
	public void setTimerInterval(float interval) { mTimerInterval = interval; }
	public void addDontUnloadPrefab(string name) { mDontUnloadPrefab.Add(name); }
	// 异步预加载prefab,加载prefab文件,并实例化对象,fileWithPath是GameResource下的相对路径
	public CustomAsyncOperation initObjectToPoolAsync(string fileWithPath, int objectTag, int count, bool moveToHide, Action callback)
	{
		return getPrefabPool(fileWithPath).initToPoolAsync(objectTag, count, moveToHide, callback);
	}
	// 同步预加载prefab,加载prefab文件,并实例化对象,fileWithPath是GameResource下的相对路径
	public void initObjectToPool(string fileWithPath, int objectTag, int count, bool moveToHide)
	{
		getPrefabPool(fileWithPath).initToPool(objectTag, count, moveToHide);
	}
	// 异步加载物体列表,当列表中的所有物体加载完成时才会调用回调,fileWithPath是GameResource下的相对路径
	public void createObjectGroupAsync(List<string> fileWithPath, CreateObjectGroupCallback callback, int objectTag, bool moveToHide)
	{
		DIC_PERSIST<string, GameObject>(out var list);
		AsyncTaskGroup group = mAsyncTaskGroupManager.createGroup(()=> 
		{
			callback?.Invoke(list);
			UN_DIC(ref list);
		});

		// 遍历所有需要创建的对象
		int count = fileWithPath.Count;
		for(int i = 0; i < count; ++i)
		{
			string fileName = fileWithPath[i];
			group.addTask(createObjectAsync(fileName, objectTag, moveToHide, false, (GameObject go) => { list.Add(fileName, go); }));
		}
	}
	// 异步创建物体,实际上只是异步加载,实例化还是同步的
	// fileWithPath是GameResource下的相对路径
	// failCallback的参数表示是否为资源加载失败而失败
	public CustomAsyncOperation createObjectAsyncSafe(ClassObject relatedObj, string fileWithPath, int objectTag, bool moveToHide, bool active, GameObjectCallback callback, BoolCallback failCallback = null)
	{
		if (fileWithPath.isEmpty())
		{
			return null;
		}
		using var a = new ProfilerScope(0);
		long assignID = relatedObj?.getAssignID() ?? 0;
		return getPrefabPool(fileWithPath).getOneUnusedAsync(objectTag, (ObjectInfo objInfo, bool poolDestroy) =>
		{
			using var a = new ProfilerScope(0);
			if (objInfo == null)
			{
				if (!poolDestroy)
				{
					logError("prefab加载失败:" + fileWithPath + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (fileWithPath.Contains(' ') || fileWithPath.Contains('　') ? "注意此文件名中带有空格" : ""));
				}
				// 资源加载失败而失败
				failCallback?.Invoke(true);
				return;
			}
			PrefabPool pool = getPrefabPool(fileWithPath);
			objInfo.setPool(pool);
			if (assignID != 0 && assignID != relatedObj.getAssignID())
			{
				pool.destroyObject(objInfo, false);
				// 因为关联对象被销毁而失败
				failCallback?.Invoke(false);
				return;
			}
			postCreateObject(pool, objInfo, moveToHide, null, active);
			callback?.Invoke(objInfo.getObject());
		});
	}
	// 异步创建物体,实际上只是异步加载,实例化还是同步的,fileWithPath是GameResource下的相对路径
	public CustomAsyncOperation createObjectAsync(string fileWithPath, int objectTag, bool moveToHide, bool active, GameObjectCallback callback)
	{
		return createObjectAsyncSafe(null, fileWithPath, objectTag, moveToHide, active, callback, null);
	}
	// 同步创建物体,fileWithPath是GameResource下的相对路径
	public GameObject createObject(string fileWithPath, int objectTag, bool moveToHide, bool active, GameObject parent = null)
	{
		using var a = new ProfilerScope(0);
		PrefabPool pool = getPrefabPool(fileWithPath);
		ObjectInfo objInfo = pool.getOneUnused(objectTag);
		if (objInfo == null)
		{
			logError("prefab加载失败:" + fileWithPath + ",请确认文件存在,且带后缀名,且不能使用反斜杠\\," + (fileWithPath.Contains(' ') || fileWithPath.Contains('　') ? "注意此文件名中带有空格" : ""));
			return null;
		}
		postCreateObject(pool, objInfo, moveToHide, parent, active);
		return objInfo.getObject();
	}
	// 销毁指定tag的所有物体,会从内存中真正销毁,不会放回到池中
	public void destroyAllWithTag(int objectTag)
	{
		using var a = new ListScope<ObjectInfo>(out var tempList);
		foreach (ObjectInfo item in mInstanceList.Values)
		{
			if (item.getTag() == objectTag)
			{
				tempList.Add(item);
			}
		}
		foreach (ObjectInfo item in tempList)
		{
			destroyObject(item.getObject(), true);
		}
	}
	public void destroyObject(GameObject obj, bool destroyReally)
	{
		destroyObject(ref obj, destroyReally);
	}
	// 销毁一个物体,destroyReally为true表示真正从内存中销毁,false表示仅仅只是放回到池中
	public void destroyObject(ref GameObject obj, bool destroyReally)
	{
		if (obj == null)
		{
			return;
		}
		if (!mInstanceList.Remove(obj, out ObjectInfo info))
		{
			logError("can not find gameObject in ObjectPool! obj:" + obj.name + ", HashCode:" + obj.GetHashCode());
			return;
		}

		if (!mPrefabPoolList.tryGetValue(info.getFileWithPath(), out PrefabPool prefabPool))
		{
			logError("找不到此预设的对象池:" + info.getFileWithPath());
			return;
		}
		prefabPool.destroyObject(info, destroyReally);
		// 如果已经没有任何实例化对象,则销毁此对象池
		if (prefabPool.isEmpty())
		{
			destroyPool(prefabPool);
		}
		obj = null;
	}
	public bool isExistInPool(GameObject go) { return go != null && mInstanceList.ContainsKey(go); }
	public Dictionary<GameObject, ObjectInfo> getInstanceList() { return mInstanceList; }
	public SafeDictionary<string, PrefabPool> getPrefabPoolList() { return mPrefabPoolList; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void destroyPool(PrefabPool pool)
	{
		if (!mDontUnloadPrefab.Contains(pool.getFileName()))
		{
			mPrefabPoolList.remove(pool.getFileName());
			UN_CLASS(ref pool);
		}
	}
	protected void postCreateObject(PrefabPool pool, ObjectInfo objInfo, bool moveToHide, GameObject parent, bool active)
	{
		objInfo.setPool(pool);
		objInfo.setMoveToHide(moveToHide);
		GameObject go = objInfo.getObject();
		if (go == null)
		{
			return;
		}
		mInstanceList.TryAdd(go, objInfo);
		// 返回前先确保物体是挂接到预设管理器下的
		if (parent == null)
		{
			parent = mObject;
		}
		setNormalProperty(go, parent);
		if (go.activeSelf != active)
		{
			go.SetActive(active);
		}
	}
	// 根据名字获取一个对象池
	protected PrefabPool getPrefabPool(string fileWithPath)
	{
		if (fileWithPath.isEmpty())
		{
			logError("fileWithPath is empty");
		}
		if (!mPrefabPoolList.tryGetValue(fileWithPath, out PrefabPool prefabPool))
		{
			prefabPool = mPrefabPoolList.addClass(fileWithPath);
			prefabPool.setFileName(fileWithPath);
		}
		return prefabPool;
	}
}