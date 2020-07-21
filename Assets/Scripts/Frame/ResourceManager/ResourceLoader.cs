using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class ResourceLoader : GameBase
{
	protected Dictionary<string, Dictionary<string, ResoucesLoadInfo>> mLoadedPath;
	protected Dictionary<Object, ResoucesLoadInfo> mLoadedObjects;
	public ResourceLoader()
	{
		mLoadedPath = new Dictionary<string, Dictionary<string, ResoucesLoadInfo>>();
		mLoadedObjects = new Dictionary<Object, ResoucesLoadInfo>();
	}
	public void init(){}
	public void update(float elapsedTime){}
	public void destroy(){}
	public void getFileList(string path, List<string> list)
	{
		List<string> fileList = findResourcesFilesNonAlloc(path);
		// 去除meta文件
		list.Clear();
		foreach (var item in fileList)
		{
			if(!item.EndsWith(".meta"))
			{
				list.Add(getFileNameNoSuffix(item));
			}
		}
	}
	public bool unloadResource<T>(ref T obj) where T : Object
	{
		if(obj == null)
		{
			return false;
		}
		// 资源已经加载完
		if(mLoadedObjects.ContainsKey(obj))
		{
			if(!(obj is GameObject))
			{
				Resources.UnloadAsset(obj);
			}
			mLoadedPath[mLoadedObjects[obj].mPath].Remove(mLoadedObjects[obj].mResouceName);
			mClassPool.destroyClass(mLoadedObjects[obj]);
			mLoadedObjects.Remove(obj);
			obj = null;
			return true;
		}
		else
		{
			logWarning("无法卸载资源:" + obj.name + ", 可能未加载,或者已经卸载,或者该资源是子资源,或者正在异步加载");
		}
		return false;
	}
	// 卸载指定路径中的所有资源
	public void unloadPath(string path)
	{
		List<string> tempList = mListPool.newList(out tempList);
		tempList.AddRange(mLoadedPath.Keys);
		foreach(var item0 in tempList)
		{
			if (!startWith(item0, path))
			{
				continue;
			}
			logInfo("unload path: " + item0);
			var list = mLoadedPath[item0];
			foreach (var item in list)
			{
				if (item.Value.mObject != null)
				{
					mLoadedObjects.Remove(item.Value.mObject);
					if (!(item.Value.mObject is GameObject))
					{
						Resources.UnloadAsset(item.Value.mObject);
					}
					item.Value.mObject = null;
				}
				mClassPool.destroyClass(item.Value);
			}
			mLoadedPath.Remove(item0);
		}
		mListPool.destroyList(tempList);
	}
	public bool isResourceLoaded(string name)
	{
		string path = getFilePath(name);
		if (!mLoadedPath.ContainsKey(path))
		{
			return mLoadedPath[path].ContainsKey(name);
		}
		return false;
	}
	public Object getResource(string name)
	{
		string path = getFilePath(name);
		if (mLoadedPath.ContainsKey(path) && mLoadedPath[path].ContainsKey(name))
		{
			return mLoadedPath[path][name].mObject;
		}
		return null;
	}
	// 同步加载资源,name为Resources下的相对路径,不带后缀
	public Object[] loadSubResource<T>(string name) where T : Object
	{
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		if (!mLoadedPath.ContainsKey(path))
		{
			mLoadedPath.Add(path, new Dictionary<string, ResoucesLoadInfo>());
		}
		// 资源未加载,则使用Resources.Load加载资源
		if (!mLoadedPath[path].ContainsKey(name))
		{
			load<T>(path, name);
			return mLoadedPath[path][name].mSubObjects;
		}
		else
		{
			ResoucesLoadInfo info = mLoadedPath[path][name];
			if (info.mState == LOAD_STATE.LS_LOADED)
			{
				return info.mSubObjects;
			}
			else if (info.mState == LOAD_STATE.LS_LOADING)
			{
				logInfo("资源正在后台加载,不能同步加载!" + name, LOG_LEVEL.LL_FORCE);
			}
			else if (info.mState == LOAD_STATE.LS_UNLOAD)
			{
				logInfo("资源已加入列表,但是未加载" + name, LOG_LEVEL.LL_FORCE);
			}
		}
		return null;
	}
	// 同步加载资源,name为Resources下的相对路径,不带后缀
	public T loadResource<T>(string name) where T : Object
	{
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		if (!mLoadedPath.ContainsKey(path))
		{
			mLoadedPath.Add(path, new Dictionary<string, ResoucesLoadInfo>());
		}
		// 资源未加载,则使用Resources.Load加载资源
		if (!mLoadedPath[path].ContainsKey(name))
		{
			load<T>(path, name);
			return mLoadedPath[path][name].mObject as T;
		}
		else
		{
			ResoucesLoadInfo info = mLoadedPath[path][name];
			if(info.mState == LOAD_STATE.LS_LOADED)
			{
				return info.mObject as T;
			}
			else if(info.mState == LOAD_STATE.LS_LOADING)
			{
				logWarning("资源正在后台加载,不能同步加载!" + name, LOG_LEVEL.LL_FORCE);
			}
			else if(info.mState == LOAD_STATE.LS_UNLOAD)
			{
				logWarning("资源已加入列表,但是未加载" + name, LOG_LEVEL.LL_FORCE);
			}
		}
		return null;
	}
	// 异步加载资源,name为Resources下的相对路径,不带后缀
	public bool loadResourcesAsync<T>(string name, AssetLoadDoneCallback doneCallback, object[] userData) where T : Object
	{
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		if (!mLoadedPath.ContainsKey(path))
		{
			mLoadedPath.Add(path, new Dictionary<string, ResoucesLoadInfo>());
		}
		// 已经加载,则返回true
		if (mLoadedPath[path].ContainsKey(name))
		{
			ResoucesLoadInfo info = mLoadedPath[path][name];
			// 资源正在下载,将回调添加到回调列表中,等待下载完毕
			if (info.mState == LOAD_STATE.LS_LOADING)
			{	
				info.addCallback(doneCallback, userData, name);
			}
			// 资源已经下载完毕,直接调用回调
			else if(info.mState == LOAD_STATE.LS_LOADED)
			{
				doneCallback(mLoadedPath[path][name].mObject, mLoadedPath[path][name].mSubObjects, null, userData, name);
			}
		}
		// 还没有加载则开始异步加载
		else
		{
			ResoucesLoadInfo info;
			mClassPool.newClass(out info);
			info.mPath = path;
			info.mResouceName = name;
			info.mState = LOAD_STATE.LS_LOADING;
			info.addCallback(doneCallback, userData, name);
			mLoadedPath[path].Add(name, info);
			mGameFramework.StartCoroutine(loadResourceCoroutine<T>(info));
		}
		return true;
	}
	//---------------------------------------------------------------------------------------------------------------------------------------
	protected void load<T>(string path, string name) where T : Object
	{
		if (!mLoadedPath[path].ContainsKey(name))
		{
			ResoucesLoadInfo info;
			mClassPool.newClass(out info);
			info.mPath = path;
			info.mResouceName = name;
			info.mState = LOAD_STATE.LS_LOADING;
			mLoadedPath[path].Add(info.mResouceName, info);
#if UNITY_EDITOR
			List<string> fileNameList = mListPool.newList(out fileNameList);
			ResourceManager.adjustResourceName<T>(name, fileNameList, false);
			int fileCount = fileNameList.Count;
			for(int i = 0; i < fileCount; ++i)
			{
				string filePath = CommonDefine.P_GAME_RESOURCES_PATH + fileNameList[i];
				if (isFileExist(filePath))
				{
					info.mObject = AssetDatabase.LoadAssetAtPath<T>(filePath);
					info.mSubObjects = AssetDatabase.LoadAllAssetsAtPath(filePath);
					break;
				}
			}
			mListPool.destroyList(fileNameList);
#else
			info.mObject = Resources.Load(name, typeof(T));
			info.mSubObjects = Resources.LoadAll(name);
#endif
			info.mState = LOAD_STATE.LS_LOADED;
			if(info.mObject != null)
			{
				mLoadedObjects.Add(info.mObject, info);
			}
		}
	}
	protected IEnumerator loadResourceCoroutine<T>(ResoucesLoadInfo info) where T : Object
	{
#if UNITY_EDITOR
		List<string> fileNameList = mListPool.newList(out fileNameList);
		ResourceManager.adjustResourceName<T>(info.mResouceName, fileNameList, false);
		int fileCount = fileNameList.Count;
		for (int i = 0; i < fileCount; ++i)
		{
			string filePath = CommonDefine.P_GAME_RESOURCES_PATH + fileNameList[i];
			if (isFileExist(filePath))
			{
				info.mObject = AssetDatabase.LoadAssetAtPath<T>(filePath);
				info.mSubObjects = AssetDatabase.LoadAllAssetsAtPath(filePath);
				break;
			}
		}
		mListPool.destroyList(fileNameList);
		yield return null;
#else
		ResourceRequest request = Resources.LoadAsync<T>(info.mResouceName);
		yield return request;
		info.mObject = request.asset;
#endif
		info.mState = LOAD_STATE.LS_LOADED;
		if(info.mObject != null)
		{
			mLoadedObjects.Add(info.mObject, info);
		}
		info.callbackAll(info.mObject, info.mSubObjects, null);
	}
}