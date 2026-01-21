using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameUtility;

// 单个prefab的实例化池
public class PrefabPool : ClassObject
{
	protected HashSet<GameObjectInfo> mInuseList = new();   // 正在使用的实例化列表,第一个key是文件名,第二个列表中的key是实例化出的物体,value是物品信息,为了提高运行时效率,仅在编辑器下使用
	protected List<GameObjectInfo> mUnuseList = new();		// 未使用的实例化列表,第一个key是文件名,第二个列表中的key是实例化出的物体,value是物品信息
	protected GameObject mPrefab;							// 从资源管理器中加载的预设
	protected string mFileName;								// 此实例物体的预设文件名,相对于GameResources的路径,带后缀
	protected int mAsyncLoadingCount;                       // 正在异步加载的数量
	protected int mAsyncInstantiateCount;					// 正在异步实例化的数量
	public override void resetProperty()
	{
		base.resetProperty();
		mInuseList.Clear();
		mUnuseList.Clear();
		mPrefab = null;
		mFileName = null;
		mAsyncLoadingCount = 0;
		mAsyncInstantiateCount = 0;
	}
	public override void destroy()
	{
		base.destroy();
		destroyAllInstance();
		mResourceManager?.unload(ref mPrefab);
	}
	public void destroyAllInstance()
	{
		foreach (GameObjectInfo item in mInuseList)
		{
			item.destroyObject();
		}
		mInuseList.Clear();
		foreach (GameObjectInfo item in mUnuseList)
		{
			item.destroyObject();
		}
		mUnuseList.Clear();
	}
	public void setFileName(string fileName) { mFileName = fileName; }
	public void setPrefab(GameObject prefab) { mPrefab = prefab; }
	public GameObject getPrefab() { return mPrefab; }
	public string getFileName() { return mFileName; }
	public List<GameObjectInfo> getUnuseList() { return mUnuseList; }
	public HashSet<GameObjectInfo> getInuseList() { return mInuseList; }
	public int getInuseCount() { return mInuseList.Count; }
	public int getUnuseCount() { return mUnuseList.Count; }
	public bool isEmpty() { return mInuseList.Count == 0 && mUnuseList.Count == 0 && mAsyncLoadingCount == 0 && mAsyncInstantiateCount == 0; }
	public bool isEmptyInUse() { return mInuseList.Count == 0 && mAsyncLoadingCount == 0 && mAsyncInstantiateCount == 0; }
	// 向池中异步初始化一定数量的对象
	public CustomAsyncOperation initToPoolAsync(int tag, int count, bool moveToHide, Action callback)
	{
		if (mPrefab != null)
		{
			doInitToPool(tag, count, moveToHide);
			callback?.Invoke();
			return new CustomAsyncOperation().setFinish();
		}
		// 预设未加载,异步加载预设
		++mAsyncLoadingCount;
		long assignID = getAssignID();
		return mResourceManager.loadGameResourceAsync(mFileName, (GameObject asset) =>
		{
			--mAsyncLoadingCount;
			if (asset == null)
			{
				callback?.Invoke();
				return;
			}
			if (assignID != getAssignID())
			{
				mResourceManager.unload(ref asset);
				callback?.Invoke();
				return;
			}
			setPrefab(asset);
			doInitToPool(tag, count, moveToHide);
			callback?.Invoke();
		});
	}
	// 向池中同步初始化一定数量的对象
	public void initToPool(int tag, int count, bool moveToHide)
	{
		if (mPrefab == null)
		{
			// 预设未加载,同步加载预设
			var go = mResourceManager.loadGameResource<GameObject>(mFileName);
			if (go == null)
			{
				return;
			}
			setPrefab(go);
		}
		doInitToPool(tag, count, moveToHide);
	}
	// 从池中异步获取一个对象
	public CustomAsyncOperation getOneUnusedAsync(int tag, Action<GameObjectInfo, bool> callback)
	{
		if (mPrefab != null)
		{
			getOneUnusedAsyncInternal(tag, (GameObjectInfo info) => { callback?.Invoke(info, false); });
			return new CustomAsyncOperation().setFinish();
		}
		// 预设未加载,异步加载预设
		++mAsyncLoadingCount;
		long assignID = getAssignID();
		return mResourceManager.loadGameResourceAsync(mFileName, (GameObject asset) =>
		{
			--mAsyncLoadingCount;
			if (asset == null)
			{
				callback?.Invoke(null, false);
				return;
			}
			if (assignID != getAssignID())
			{
				callback?.Invoke(null, true);
				mResourceManager.unload(ref asset);
				return;
			}
			setPrefab(asset);
			getOneUnusedAsyncInternal(tag, (GameObjectInfo info)=> { callback?.Invoke(info, false); });
		});
	}
	// 从对象池中同步获取或者创建一个物体
	public GameObjectInfo getOneUnused(int tag)
	{
		if (mPrefab == null)
		{
			mPrefab = mResourceManager.loadGameResource<GameObject>(mFileName);
		}
		GameObjectInfo objInfo;
		// 未使用列表中有就从未使用列表中获取
		if (mUnuseList.Count > 0)
		{
			objInfo = mUnuseList.popBack();
			if (objInfo.getTag() != tag)
			{
				logError("不能为同一个物体设置不同的tag, file:" + objInfo.getFileWithPath() + ", 旧tag:" + objInfo.getTag() + ", 新tag:" + tag);
			}
		}
		// 没有就创建一个新的
		else
		{
			// 实例化
			CLASS(out objInfo).createObject(mPrefab, mFileName);
			objInfo.setTag(tag);
		}
		objInfo.setUsing(true);
		return mInuseList.add(objInfo);
	}
	// 销毁物体,destroyReally为true表示将对象直接从内存中销毁,false表示只是放到未使用列表中
	// moveToHide为true则表示回收时不会改变GameObject的显示,只是将位置设置到很远的地方
	public void destroyObject(GameObjectInfo obj, bool destroyReally)
	{
		if (obj.getPool() != this)
		{
			logError("要销毁的物体不属于当前对象池");
			return;
		}
		GameObject go = obj.getObject();
		if (!mInuseList.Remove(obj))
		{
			logError("从使用列表中移除失败:" + go.name + ", " + obj.GetHashCode() + ", pool hash:" + GetHashCode());
		}
		if (destroyReally)
		{
			mUnuseList.Remove(obj);
			UN_CLASS(ref obj);
			return;
		}

		bool moveToHide = obj.isMoveToHide();
		if (go.transform.parent == null || go.transform.parent.gameObject != mPrefabPoolManager.getObject())
		{
			// 只有在PrefabPoolManager节点下的物体才可以在回收时只改变位置
			moveToHide = false;
		}
		// 隐藏物体,并且将物体重新挂接到预设管理器下,重置物体变换
		if (moveToHide)
		{
			go.transform.localPosition = FAR_POSITION;
		}
		else
		{
			if (go.activeSelf)
			{
				go.SetActive(false);
			}
			setNormalProperty(go, mPrefabPoolManager.getObject());
		}
		obj.setUsing(false);
		mUnuseList.add(obj);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void doInitToPool(int tag, int count, bool moveToHide)
	{
		if (mPrefab == null)
		{
			return;
		}
		int needCreate = clampMin(count - mInuseList.Count - mUnuseList.Count);
		int needCapacity = mUnuseList.count() + needCreate;
		if (mUnuseList.Capacity < needCapacity)
		{
			mUnuseList.Capacity = needCapacity;
		}
		for (int i = 0; i < needCreate; ++i)
		{
			GameObjectInfo objInfo = mUnuseList.addClass();
			objInfo.setTag(tag);
			// 实例化,同步进行
			objInfo.createObject(mPrefab, mFileName);
			GameObject go = objInfo.getObject();
			if (go != null)
			{
				// 隐藏物体,并且将物体重新挂接到预设管理器下,重置物体变换
				setNormalProperty(go, mPrefabPoolManager.getObject());
				if (moveToHide)
				{
					go.transform.localPosition = FAR_POSITION;
				}
				else
				{
					if (go.activeSelf)
					{
						go.SetActive(false);
					}
				}
			}
			objInfo.setUsing(false);
		}
	}
	// 从池中异步获取一个可用的对象
	protected void getOneUnusedAsyncInternal(int tag, Action<GameObjectInfo> callback)
	{
		// 未使用列表中有就从未使用列表中获取
		if (mUnuseList.Count > 0)
		{
			GameObjectInfo objInfo = mUnuseList.popBack();
			if (objInfo.getTag() != tag)
			{
				logError("不能为同一个物体设置不同的tag, file:" + objInfo.getFileWithPath());
			}
			objInfo.setUsing(true);
			callback?.Invoke(mInuseList.add(objInfo));
		}
		// 没有就创建一个新的
		else
		{
			// 实例化
			++mAsyncInstantiateCount;
			CLASS<GameObjectInfo>().createObjectAsync(mPrefab, mFileName, (GameObjectInfo info) =>
			{
				--mAsyncInstantiateCount;
				info.setTag(tag);
				info.setUsing(true);
				callback?.Invoke(mInuseList.add(info));
			});
		}
	}
}