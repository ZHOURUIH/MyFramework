using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FileUtility;
using static UnityUtility;
using static FrameUtility;
using static StringUtility;
using static FrameBase;
using static CSharpUtility;
using UObject = UnityEngine.Object;

// 从Resources中加载资源
public class ResourcesLoader : ClassObject
{
	protected Dictionary<string, Dictionary<string, ResourceLoadInfo>> mLoadedPath;	// 所有已加载的文件夹
	protected Dictionary<UObject, ResourceLoadInfo> mLoadedObjects;					// 所有的已加载的资源
	public ResourcesLoader()
	{
		mLoadedPath = new Dictionary<string, Dictionary<string, ResourceLoadInfo>>();
		mLoadedObjects = new Dictionary<UObject, ResourceLoadInfo>();
	}
	public void init(){}
	public void destroy(){}
	public override void resetProperty()
	{
		base.resetProperty();
		mLoadedPath.Clear();
		mLoadedObjects.Clear();
	}
	public void getFileList(string path, List<string> list)
	{
		List<string> fileList = findResourcesFilesNonAlloc(path);
		// 去除meta文件
		list.Clear();
		int count = fileList.Count;
		for(int i = 0; i < count; ++i)
		{
			string item = fileList[i];
			if (!item.EndsWith(".meta"))
			{
				list.Add(getFileNameNoSuffix(item));
			}
		}
	}
	public bool unloadResource<T>(ref T obj, bool showError) where T : UObject
	{
		if(obj == null)
		{
			return false;
		}
		// 资源已经加载完
		if(!mLoadedObjects.TryGetValue(obj, out ResourceLoadInfo info))
		{
			if (showError)
			{
				logWarning("无法卸载资源:" + obj.name + ", 可能未加载,或者已经卸载,或者该资源是子资源,或者正在异步加载");
			}
			return false;
		}
		if (!(obj is GameObject))
		{
			Resources.UnloadAsset(obj);
		}
		mLoadedPath[info.mPath].Remove(info.mResouceName);
		UN_CLASS(ref info);
		mLoadedObjects.Remove(obj);
		obj = null;
		return true;
	}
	// 卸载指定路径中的所有资源
	public void unloadPath(string path)
	{
		using (new ListScope<string>(out var tempList))
		{
			tempList.AddRange(mLoadedPath.Keys);
			int count = tempList.Count;
			for (int i = 0; i < count; ++i)
			{
				string item0 = tempList[i];
				if (!startWith(item0, path))
				{
					continue;
				}
#if UNITY_EDITOR || DEVELOPMENT_BUILD
				log("unload path: " + item0);
#endif
				var list = mLoadedPath[item0];
				foreach (var item in list)
				{
					ResourceLoadInfo info = item.Value;
					if (info.mObject != null)
					{
						mLoadedObjects.Remove(info.mObject);
						if (!(info.mObject is GameObject))
						{
							Resources.UnloadAsset(info.mObject);
						}
						info.mObject = null;
					}
					UN_CLASS(ref info);
				}
				mLoadedPath.Remove(item0);
			}
		}
	}
	public bool isResourceLoaded(string name)
	{
		string path = getFilePath(name);
		if (mLoadedPath.TryGetValue(path, out Dictionary<string, ResourceLoadInfo> resList))
		{
			return resList.ContainsKey(name);
		}
		return false;
	}
	public UObject getResource(string name)
	{
		string path = getFilePath(name);
		if (mLoadedPath.TryGetValue(path, out Dictionary<string, ResourceLoadInfo> resList) && 
			resList.TryGetValue(name, out ResourceLoadInfo info))
		{
			return info.mObject;
		}
		return null;
	}
	// 同步加载资源,name为Resources下的相对路径,不带后缀
	public UObject[] loadSubResource<T>(string name, out UObject mainAsset) where T : UObject
	{
		mainAsset = null;
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		if (!mLoadedPath.TryGetValue(path, out Dictionary<string, ResourceLoadInfo> resList))
		{
			resList = new Dictionary<string, ResourceLoadInfo>();
			mLoadedPath.Add(path, resList);
		}
		// 资源未加载,则使用Resources.Load加载资源
		if (!resList.TryGetValue(name, out ResourceLoadInfo info))
		{
			load<T>(path, name);
			// 加载后需要重新获取一次
			info = resList[name];
		}
		if (info.mState == LOAD_STATE.LOADED)
		{
			mainAsset = info.mObject;
			return info.mSubObjects;
		}
		else if (info.mState == LOAD_STATE.LOADING)
		{
			logForce("资源正在后台加载,不能同步加载!" + name);
		}
		else if (info.mState == LOAD_STATE.UNLOAD)
		{
			logForce("资源已加入列表,但是未加载" + name);
		}
		return null;
	}
	// 同步加载资源,name为Resources下的相对路径,不带后缀
	public T loadResource<T>(string name) where T : UObject
	{
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		if (!mLoadedPath.TryGetValue(path, out Dictionary<string, ResourceLoadInfo> resList))
		{
			resList = new Dictionary<string, ResourceLoadInfo>();
			mLoadedPath.Add(path, resList);
		}
		// 资源未加载,则使用Resources.Load加载资源
		if (!resList.TryGetValue(name, out ResourceLoadInfo info))
		{
			load<T>(path, name);
			// 加载后需要重新获取一次
			info = resList[name];
			return info.mObject as T;
		}
		if(info.mState == LOAD_STATE.LOADED)
		{
			return info.mObject as T;
		}
		else if(info.mState == LOAD_STATE.LOADING)
		{
			logWarning("资源正在后台加载,不能同步加载!" + name);
		}
		else if(info.mState == LOAD_STATE.UNLOAD)
		{
			logWarning("资源已加入列表,但是未加载" + name);
		}
		return null;
	}
	// 异步加载资源,name为Resources下的相对路径,不带后缀
	public bool loadResourcesAsync<T>(string name, AssetLoadDoneCallback doneCallback, object userData = null) where T : UObject
	{
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		if (!mLoadedPath.TryGetValue(path, out Dictionary<string, ResourceLoadInfo> resList))
		{
			resList = new Dictionary<string, ResourceLoadInfo>();
			mLoadedPath.Add(path, resList);
		}
		// 已经加载,则返回true
		if (resList.TryGetValue(name, out ResourceLoadInfo info))
		{
			// 资源正在下载,将回调添加到回调列表中,等待下载完毕
			if (info.mState == LOAD_STATE.LOADING)
			{	
				info.addCallback(doneCallback, userData, name);
			}
			// 资源已经下载完毕,直接调用回调
			else if(info.mState == LOAD_STATE.LOADED)
			{
				doneCallback(mLoadedPath[path][name].mObject, mLoadedPath[path][name].mSubObjects, null, userData, name);
			}
		}
		// 还没有加载则开始异步加载
		else
		{
			CLASS(out info);
			info.mPath = path;
			info.mResouceName = name;
			info.mState = LOAD_STATE.LOADING;
			info.addCallback(doneCallback, userData, name);
			resList.Add(name, info);
			mGameFramework.StartCoroutine(loadResourceCoroutine<T>(info));
		}
		return true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void load<T>(string path, string name) where T : UObject
	{
		var resList = mLoadedPath[path];
		if (resList.ContainsKey(name))
		{
			return;
		}
		CLASS(out ResourceLoadInfo info);
		info.mPath = path;
		info.mResouceName = name;
		info.mState = LOAD_STATE.LOADING;
		resList.Add(info.mResouceName, info);
		info.mObject = Resources.Load(name, Typeof<T>());
		info.mSubObjects = Resources.LoadAll(name);
		info.mState = LOAD_STATE.LOADED;
		if(info.mObject != null)
		{
			mLoadedObjects.Add(info.mObject, info);
		}
	}
	protected IEnumerator loadResourceCoroutine<T>(ResourceLoadInfo info) where T : UObject
	{
		ResourceRequest request = Resources.LoadAsync<T>(info.mResouceName);
		yield return request;
		info.mObject = request.asset;
		info.mState = LOAD_STATE.LOADED;
		if(info.mObject != null)
		{
			mLoadedObjects.Add(info.mObject, info);
		}
		info.callbackAll(info.mObject, info.mSubObjects, null);
	}
}