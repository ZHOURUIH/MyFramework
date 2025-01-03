using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using static FileUtility;
using static StringUtility;
using static FrameUtility;
using static UnityUtility;
using static FrameBase;
using static FrameDefine;
using static FrameEditorUtility;

// 从AssetDataBase中加载资源
public class AssetDataBaseLoader
{
	protected Dictionary<string, Dictionary<string, ResourceLoadInfo>> mLoadedPath = new();		// 所有已加载的文件夹
	protected Dictionary<UObject, ResourceLoadInfo> mLoadedObjects = new();						// 所有的已加载的资源
	public void destroy(){}
	public void getFileList(string path, List<string> list)
	{
		List<string> fileList = findResourcesFilesNonAlloc(path);
		// 去除meta文件
		list.Clear();
		foreach (string item in fileList)
		{
			if (!item.EndsWith(".meta"))
			{
				list.Add(removeSuffix(item));
			}
		}
	}
	public bool unloadResource<T>(ref T obj, bool showError) where T : UObject
	{
		if (obj == null)
		{
			return false;
		}
		// 资源已经加载完
		if(!mLoadedObjects.Remove(obj, out ResourceLoadInfo info))
		{
			if (showError)
			{
				logWarning("无法卸载资源:" + obj.name + ", 可能未加载,或者已经卸载,或者该资源是子资源,或者正在异步加载");
			}
			return false;
		}
		if (obj is not GameObject)
		{
			Resources.UnloadAsset(obj);
		}
		mLoadedPath.get(info.getPath()).Remove(info.getResourceName());
		UN_CLASS(ref info);
		obj = null;
		return true;
	}
	// 卸载指定路径中的所有资源
	public void unloadPath(string path)
	{
		using var a = new ListScope<string>(out var tempList);
		foreach (string item0 in tempList.addRange(mLoadedPath.Keys))
		{
			if (!startWith(item0, path))
			{
				continue;
			}
			if (isEditor() || isDevelopment())
			{
				log("unload path: " + item0);
			}
			foreach (ResourceLoadInfo info in mLoadedPath.get(item0).Values)
			{
				if (info.getObject() != null)
				{
					mLoadedObjects.Remove(info.getObject());
					if (info.getObject() is not GameObject)
					{
						Resources.UnloadAsset(info.getObject());
					}
					info.setObject(null);
				}
				UN_CLASS(info);
			}
			mLoadedPath.Remove(item0);
		}
	}
	public bool isResourceLoaded(string name)
	{
		return mLoadedPath.TryGetValue(getFilePath(name), out var resList) && resList.ContainsKey(name);
	}
	public UObject getResource(string name)
	{
		return mLoadedPath.get(getFilePath(name))?.get(name)?.getObject();
	}
	// 同步加载资源,name为GameResources下的相对路径,带后缀
	public UObject[] loadSubResource<T>(string name, out UObject mainAsset) where T : UObject
	{
		mainAsset = null;
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		var resList = mLoadedPath.tryGetOrAddNew(path);
		// 资源未加载,则使用Resources.Load加载资源
		if (!resList.TryGetValue(name, out ResourceLoadInfo info))
		{
			if (!load<T>(path, name))
			{
				return null;
			}
			// 加载后需要重新获取一次
			info = resList.get(name);
		}
		if (info.getState() == LOAD_STATE.LOADED)
		{
			mainAsset = info.getObject();
			return info.getSubObjects();
		}
		else if (info.getState() == LOAD_STATE.DOWNLOADING)
		{
			logWarning("资源正在后台下载,不能同步加载!" + name);
		}
		else if (info.getState() == LOAD_STATE.LOADING)
		{
			logWarning("资源正在后台加载,不能同步加载!" + name);
		}
		else if (info.getState() == LOAD_STATE.UNLOAD)
		{
			logWarning("资源已加入列表,但是未加载" + name);
		}
		return null;
	}
	// 同步加载资源,name为GameResources下的相对路径,带后缀
	public T loadResource<T>(string name) where T : UObject
	{
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		var resList = mLoadedPath.tryGetOrAddNew(path);
		// 资源未加载,则使用Resources.Load加载资源
		if (!resList.TryGetValue(name, out ResourceLoadInfo info))
		{
			if (!load<T>(path, name))
			{
				return null;
			}
			// 加载后需要重新获取一次
			info = resList.get(name);
			return info.getObject() as T;
		}
		if(info.getState() == LOAD_STATE.LOADED)
		{
			return info.getObject() as T;
		}
		else if (info.getState() == LOAD_STATE.DOWNLOADING)
		{
			logWarning("资源正在后台下载,不能同步加载!" + name);
		}
		else if(info.getState() == LOAD_STATE.LOADING)
		{
			logWarning("资源正在后台加载,不能同步加载!" + name);
		}
		else if(info.getState() == LOAD_STATE.UNLOAD)
		{
			logWarning("资源已加入列表,但是未加载" + name);
		}
		return null;
	}
	// 异步加载资源,name为GameResources下的相对路径,带后缀
	public CustomAsyncOperation loadResourcesAsync<T>(string name, AssetLoadDoneCallback doneCallback) where T : UObject
	{
		CustomAsyncOperation op = new();
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		var resList = mLoadedPath.tryGetOrAddNew(path);
		// 已经加载,则返回true
		if (resList.TryGetValue(name, out ResourceLoadInfo info))
		{
			// 资源正在下载或加载,将回调添加到回调列表中,等待加载完毕
			if (info.getState() == LOAD_STATE.DOWNLOADING || info.getState() == LOAD_STATE.LOADING)
			{	
				info.addCallback((UObject asset, UObject[] assets, byte[] bytes, string loadPath) =>
				{
					op.mFinish = true;
					doneCallback?.Invoke(asset, assets, bytes, loadPath);
				}, name);
			}
			// 资源已经加载完毕,直接调用回调
			else if(info.getState() == LOAD_STATE.LOADED)
			{
				doneCallback?.Invoke(info.getObject(), info.getSubObjects(), null, name);
				op.mFinish = true;
			}
		}
		// 还没有加载则开始异步加载
		else
		{
			resList.Add(name, CLASS(out info));
			info.setPath(path);
			info.setResourceName(name);
			info.setState(LOAD_STATE.LOADING);
			info.addCallback((UObject asset, UObject[] assets, byte[] bytes, string loadPath) =>
			{
				op.mFinish = true;
				doneCallback?.Invoke(asset, assets, bytes, loadPath);
			}, name);
			mGameFramework.StartCoroutine(loadResourceCoroutine<T>(info));
		}
		return op;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected bool load<T>(string path, string name) where T : UObject
	{
		var resList = mLoadedPath.get(path);
		if (resList.ContainsKey(name))
		{
			return true;
		}
		resList.Add(name, CLASS(out ResourceLoadInfo info));
		info.setPath(path);
		info.setResourceName(name);
		info.setState(LOAD_STATE.LOADING);
		if (isEditor())
		{
			string filePath = P_GAME_RESOURCES_PATH + name;
			if (isFileExist(filePath))
			{
				info.setObject(loadAssetAtPath<T>(filePath));
				info.setSubObjects(loadAllAssetsAtPath(filePath));
			}
		}
		else
		{
			string filePath = removeSuffix(name);
			info.setObject(Resources.Load(filePath, typeof(T)));
			info.setSubObjects(Resources.LoadAll(filePath));
		}
		info.setState(LOAD_STATE.LOADED);
		if (info.getObject() == null)
		{
			// 加载失败则从列表中移除
			resList.Remove(info.getResourceName());
			UN_CLASS(ref info);
			return false;
		}
		mLoadedObjects.Add(info.getObject(), info);
		return true;
	}
	protected IEnumerator loadResourceCoroutine<T>(ResourceLoadInfo info) where T : UObject
	{
		if (isEditor())
		{
			string filePath = P_GAME_RESOURCES_PATH + info.getResourceName();
			if (isFileExist(filePath))
			{
				info.setObject(loadAssetAtPath<T>(filePath));
				info.setSubObjects(loadAllAssetsAtPath(filePath));
			}
			else
			{
				logError("文件不存在:" + filePath);
			}
			yield return new WaitForEndOfFrame();
		}
		else
		{
			ResourceRequest request = Resources.LoadAsync<T>(removeSuffix(info.getResourceName()));
			yield return request;
			info.setObject(request.asset);
			if (request.asset == null)
			{
				logError("文件不存在:" + info.getResourceName());
			}
		}
		info.setState(LOAD_STATE.LOADED);
		try
		{
			if (info.getObject() != null)
			{
				mLoadedObjects.Add(info.getObject(), info);
				info.callbackAll();
			}
			else
			{
				// 加载失败则从列表中移除
				logWarning("资源加载失败:" + info.getResourceName());
				mLoadedPath.get(info.getPath()).Remove(info.getResourceName());
				info.callbackAll();
				UN_CLASS(ref info);
			}
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
}