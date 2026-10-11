using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameUtility;
using static FrameBaseUtility;
using static StringUtility;

// 单个prefab的实例化池
public class PrefabPool : ClassObject
{
	protected HashSet<GameObjectInfo> mInuseList = new();   // 正在使用的实例化列表,第一个key是文件名,第二个列表中的key是实例化出的物体,value是物品信息,为了提高运行时效率,仅在编辑器下使用
	protected List<GameObjectInfo> mUnuseList = new();		// 未使用的实例化列表,第一个key是文件名,第二个列表中的key是实例化出的物体,value是物品信息
	protected ResourceRef<GameObject> mPrefab;				// 从资源管理器中加载的预设
	protected string mFileName;								// 此实例物体的预设文件名,相对于GameResources的路径,带后缀
	protected int mAsyncLoadingCount;                       // 正在异步加载的数量
	protected int mAsyncInstantiateCount;					// 正在异步实例化的数量
	protected struct AsyncRequest
	{
		public int mTag;
		public int mPreloadCount; // 0表示普通请求,大于0表示对象池的预热目标总数
		public bool mMoveToHide;
		public Action<GameObjectInfo, bool> mCallback;
		public CustomAsyncOperation mOperation;
	}
	protected Queue<AsyncRequest> mAsyncRequests = new(); // 等待交付的请求,包含当前批次
	protected int mAsyncFailedRequestCount;
	protected bool mAsyncPreloadFailed;
	protected const int ASYNC_INSTANTIATE_BATCH_COUNT = 8;
#if UNITY_6000_0_OR_NEWER
	protected AsyncInstantiateOperation<GameObject> mAsyncOperation;
	protected GameObject[] mCreatedObjects;
	protected int mCreatedObjectIndex;
#endif
	public override void resetProperty()
	{
		base.resetProperty();
		mInuseList.Clear();
		mUnuseList.Clear();
		mPrefab = null;
		mFileName = null;
		mAsyncLoadingCount = 0;
		mAsyncInstantiateCount = 0;
		mAsyncRequests.Clear();
		mAsyncFailedRequestCount = 0;
		mAsyncPreloadFailed = false;
#if UNITY_6000_0_OR_NEWER
		mAsyncOperation = null;
		mCreatedObjects = null;
		mCreatedObjectIndex = 0;
#endif
	}
	public override void destroy()
	{
		base.destroy();
		cancelAsyncCreate();
		destroyAllInstance();
		mResourceManager.unload(ref mPrefab);
	}
	public void destroyAllInstance()
	{
		UN_CLASS_LIST(mInuseList);
		UN_CLASS_LIST(mUnuseList);
	}
	public void setFileName(string fileName)				{ mFileName = fileName; }
	public void setPrefab(ResourceRef<GameObject> prefab)	{ mPrefab = prefab; }
	public GameObject getPrefab()							{ return mPrefab != null ? mPrefab.get() : null; }
	public string getFileName()								{ return mFileName; }
	public List<GameObjectInfo> getUnuseList()				{ return mUnuseList; }
	public HashSet<GameObjectInfo> getInuseList()			{ return mInuseList; }
	public int getInuseCount()								{ return mInuseList.Count; }
	public int getUnuseCount()								{ return mUnuseList.Count; }
	public bool isEmpty()									{ return mInuseList.Count == 0 && mUnuseList.Count == 0 && mAsyncLoadingCount == 0 && mAsyncInstantiateCount == 0 && mAsyncRequests.Count == 0; }
	public bool isEmptyInUse()								{ return mInuseList.Count == 0 && mAsyncLoadingCount == 0 && mAsyncInstantiateCount == 0 && mAsyncRequests.Count == 0; }
	// 向池中异步初始化一定数量的对象
	public CustomAsyncOperation initToPoolAsync(int tag, int count, bool moveToHide, Action callback)
	{
		CustomAsyncOperation op = new();
		AsyncRequest request = new()
		{
			mTag = tag,
			mPreloadCount = Math.Max(count, 1),
			mMoveToHide = moveToHide,
			mCallback = (_, _) => callback?.Invoke(),
			mOperation = op,
		};
		if (mHasDestroy || count <= 0)
		{
			finishAsyncRequest(request, null, mHasDestroy);
			return op;
		}
		bool completeImmediately = mAsyncRequests.Count == 0 && count <= mInuseList.Count + mUnuseList.Count;
		long assignID = getAssignID();
		mAsyncRequests.Enqueue(request);
		if (completeImmediately)
		{
			mPrefabPoolManager.processAsyncCreateImmediately(this);
		}
		if (!mHasDestroy && assignID == getAssignID())
		{
			mPrefabPoolManager.scheduleAsyncCreate(this);
		}
		return op;
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
	// 从池中异步获取一个对象,同一预制体的请求由管理器按帧预算集中调度
	public CustomAsyncOperation getOneUnusedAsync(int tag, Action<GameObjectInfo, bool> callback)
	{
		CustomAsyncOperation op = new();
		if (mHasDestroy)
		{
			finishAsyncRequest(new() { mTag = tag, mCallback = callback, mOperation = op }, null, true);
			return op;
		}
		bool reuseImmediately = mAsyncRequests.Count == 0 && mUnuseList.Count > 0;
		long assignID = getAssignID();
		mAsyncRequests.Enqueue(new() { mTag = tag, mCallback = callback, mOperation = op });
		if (reuseImmediately)
		{
			mPrefabPoolManager.processAsyncCreateImmediately(this);
		}
		if (!mHasDestroy && assignID == getAssignID())
		{
			mPrefabPoolManager.scheduleAsyncCreate(this);
		}
		return op;
	}
	public bool hasAsyncCreateRequest() { return !mHasDestroy && mAsyncRequests.Count > 0; }
	public bool isAsyncCreateReady()
	{
		if (!hasAsyncCreateRequest() || (mPrefab == null && mAsyncLoadingCount > 0 && mAsyncFailedRequestCount == 0))
		{
			return false;
		}
#if UNITY_6000_0_OR_NEWER
		return mAsyncOperation == null || mAsyncOperation.isDone;
#else
		return true;
#endif
	}
	// 一次只推进一个步骤或交付一个实例,初始化及业务回调都计入管理器预算。
	public void processAsyncCreate()
	{
		if (mAsyncFailedRequestCount > 0)
		{
			--mAsyncFailedRequestCount;
			finishAsyncRequest(mAsyncRequests.Dequeue(), null, false);
			return;
		}
		AsyncRequest request = mAsyncRequests.Peek();
		// 重复预热只等待目标数量满足,不会重复创建同一批实例。
		if (request.mPreloadCount > 0 && mAsyncInstantiateCount == 0 &&
			request.mPreloadCount <= mInuseList.Count + mUnuseList.Count)
		{
			finishAsyncRequest(mAsyncRequests.Dequeue(), null, false);
			return;
		}
		if (mPrefab == null)
		{
			++mAsyncLoadingCount;
			long assignID = getAssignID();
			try
			{
				mResourceManager.loadGameResourceAsync<GameObject>(mFileName, asset =>
				{
					if (mHasDestroy || assignID != getAssignID())
					{
						mResourceManager.unload(ref asset);
						return;
					}
					--mAsyncLoadingCount;
					if (asset == null)
					{
						mAsyncFailedRequestCount = mAsyncRequests.Count;
					}
					else
					{
						adoptAsyncLoadedPrefab(ref asset);
					}
					mPrefabPoolManager.scheduleAsyncCreate(this);
				});
			}
			catch (Exception error)
			{
				logException(error);
				if (!mHasDestroy && assignID == getAssignID())
				{
					mAsyncLoadingCount = 0;
					mAsyncFailedRequestCount = mAsyncRequests.Count;
				}
			}
			return;
		}
#if UNITY_6000_0_OR_NEWER
		if (mAsyncOperation != null)
		{
			mCreatedObjects = mAsyncOperation.Result;
			mCreatedObjectIndex = 0;
			mAsyncOperation = null;
		}
		if (mAsyncInstantiateCount > 0)
		{
			GameObject go = mCreatedObjects != null && mCreatedObjectIndex < mCreatedObjects.Length ? mCreatedObjects[mCreatedObjectIndex] : null;
			++mCreatedObjectIndex;
			if (--mAsyncInstantiateCount == 0)
			{
				mCreatedObjects = null;
				mCreatedObjectIndex = 0;
			}
			GameObjectInfo info = null;
			try
			{
				if (go != null)
				{
					// 批次先在隐藏节点下创建,轮到本实例时才初始化和交付。
					CLASS(out info).setObject(go, mFileName);
					go.SetActive(false);
					setNormalProperty(go, mPrefabPoolManager.getGameObject(), getFileNameWithSuffix(mPrefab.get().name));
					findShaders(go);
					info.setTag(request.mTag);
				}
			}
			catch (Exception error)
			{
				logException(error);
				if (info != null) UN_CLASS(ref info);
				else destroyUnityObject(ref go);
			}
			deliverAsyncObject(request, info);
			return;
		}
		if (request.mPreloadCount == 0 && mUnuseList.Count > 0)
		{
			mAsyncRequests.Dequeue();
			GameObjectInfo info = null;
			try { info = getOneUnused(request.mTag); }
			catch (Exception error) { logException(error); }
			finally { finishAsyncRequest(request, info, false); }
			return;
		}
		if (request.mPreloadCount > 0)
		{
			mAsyncInstantiateCount = Math.Min(ASYNC_INSTANTIATE_BATCH_COUNT, request.mPreloadCount - mInuseList.Count - mUnuseList.Count);
		}
		else
		{
			// 预热任务按目标数量创建,不能混入逐个交付的普通请求批次。
			foreach (AsyncRequest item in mAsyncRequests)
			{
				if (item.mPreloadCount > 0 || mAsyncInstantiateCount >= ASYNC_INSTANTIATE_BATCH_COUNT) break;
				++mAsyncInstantiateCount;
			}
		}
		try
		{
			mAsyncOperation = UnityEngine.Object.InstantiateAsync(mPrefab.get(), mAsyncInstantiateCount, mPrefabPoolManager.getAsyncInstantiateRoot());
		}
		catch (Exception error)
		{
			logException(error);
			// 后续按预算逐个返回失败,不在异常路径一次执行全部回调。
			mCreatedObjects = null;
		}
#else
		GameObjectInfo info = null;
		try
		{
			if (request.mPreloadCount > 0)
			{
				CLASS(out info).createObject(mPrefab.get(), mFileName);
				info.setTag(request.mTag);
			}
			else
			{
				info = getOneUnused(request.mTag);
			}
		}
		catch (Exception error)
		{
			logException(error);
			if (info != null) UN_CLASS(ref info);
		}
		deliverAsyncObject(request, info);
#endif
	}
	protected void deliverAsyncObject(AsyncRequest request, GameObjectInfo info)
	{
		if (request.mPreloadCount == 0)
		{
			mAsyncRequests.Dequeue();
			if (info != null)
			{
				info.setUsing(true);
				mInuseList.Add(info);
			}
			finishAsyncRequest(request, info, false);
			return;
		}
		long assignID = getAssignID();
		try
		{
			if (info != null && info.getObject() != null)
			{
				info.setUsing(false);
				mUnuseList.Add(info);
				GameObject go = info.getObject();
				setNormalProperty(go, mPrefabPoolManager.getGameObject());
				if (request.mMoveToHide)
				{
					go.transform.localPosition = FAR_POSITION;
					go.SetActive(true);
				}
				else if (go.activeSelf)
				{
					go.SetActive(false);
				}
			}
			else
			{
				if (info != null) UN_CLASS(ref info);
				mAsyncPreloadFailed = true;
			}
		}
		catch (Exception error)
		{
			logException(error);
			if (!mHasDestroy && assignID == getAssignID())
			{
				mUnuseList.Remove(info);
				if (info != null) UN_CLASS(ref info);
				mAsyncPreloadFailed = true;
			}
		}
		// 激活预热物体的组件可能触发场景退出,此时取消流程已经结束请求。
		if (!mHasDestroy && assignID == getAssignID() && mAsyncInstantiateCount == 0 &&
			(mAsyncPreloadFailed || request.mPreloadCount <= mInuseList.Count + mUnuseList.Count))
		{
			mAsyncPreloadFailed = false;
			finishAsyncRequest(mAsyncRequests.Dequeue(), null, false);
		}
	}
	protected static void finishAsyncRequest(AsyncRequest request, GameObjectInfo info, bool poolDestroy)
	{
		try { request.mCallback?.Invoke(info, poolDestroy); }
		catch (Exception error) { logException(error); }
		finally { request.mOperation.setFinish(); }
	}
	protected void failAsyncRequests(bool poolDestroy)
	{
		int count = mAsyncRequests.Count;
		for (int i = 0; i < count; ++i)
		{
			finishAsyncRequest(mAsyncRequests.Dequeue(), null, poolDestroy);
		}
	}
	protected void cancelAsyncCreate()
	{
		mPrefabPoolManager?.unscheduleAsyncCreate(this);
#if UNITY_6000_0_OR_NEWER
		if (mAsyncOperation != null)
		{
			if (mAsyncOperation.isDone)
			{
				mCreatedObjects = mAsyncOperation.Result;
				mCreatedObjectIndex = 0;
			}
			else
			{
				mAsyncOperation.Cancel();
			}
			mAsyncOperation = null;
		}
		if (mCreatedObjects != null)
		{
			for (int i = mCreatedObjectIndex; i < mCreatedObjects.Length; ++i)
			{
				destroyUnityObject(mCreatedObjects[i]);
			}
		}
		mCreatedObjects = null;
		mCreatedObjectIndex = 0;
#endif
		mAsyncInstantiateCount = 0;
		mAsyncFailedRequestCount = 0;
		mAsyncPreloadFailed = false;
		failAsyncRequests(true);
	}
	// 从对象池中同步获取或者创建一个物体
	public GameObjectInfo getOneUnused(int tag)
	{
		mPrefab ??= mResourceManager.loadGameResource<GameObject>(mFileName);
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
			CLASS(out objInfo).createObject(mPrefab.get(), mFileName);
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
		if (go.transform.parent == null || go.transform.parent.gameObject != mPrefabPoolManager.getGameObject())
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
			setNormalProperty(go, mPrefabPoolManager.getGameObject());
		}
		obj.setUsing(false);
		mUnuseList.add(obj);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 同一个Prefab在首次异步加载完成前,可能同时收到多次createObjectAsync请求。
	// 每次ResourceManager异步回调都会返回一个独立的ResourceRef,即使底层UnityEngine.Object是同一个。
	// mPrefab只能持有其中一个ResourceRef,其余ResourceRef必须显式unload,
	// 否则直接mPrefab = asset会丢失旧ResourceRef,对应的ResourceManager token将永远无法释放。
	protected void adoptAsyncLoadedPrefab(ref ResourceRef<GameObject> asset)
	{
		if (asset == null)
		{
			return;
		}
		if (mPrefab == null)
		{
			mPrefab = asset;
			asset = null;
			return;
		}
		// 已经有其他并发请求先完成并持有Prefab引用。
		// 当前asset只是同一Prefab的额外ResourceRef,释放它的token即可。
		mResourceManager.unload(ref asset);
	}
	protected void doInitToPool(int tag, int count, bool moveToHide)
	{
		if (mPrefab == null)
		{
			return;
		}
		int needCreate = (count - mInuseList.Count - mUnuseList.Count).clampMin();
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
			objInfo.createObject(mPrefab.get(), mFileName);
			GameObject go = objInfo.getObject();
			if (go != null)
			{
				// 隐藏物体,并且将物体重新挂接到预设管理器下,重置物体变换
				setNormalProperty(go, mPrefabPoolManager.getGameObject());
				if (moveToHide)
				{
					go.transform.localPosition = FAR_POSITION;
				}
				else if (go.activeSelf)
				{
					go.SetActive(false);
				}
			}
			objInfo.setUsing(false);
		}
	}
}