using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using static FrameBaseUtility;
using static StringUtility;

// 从Resources中加载资源
public class ResourcesLoader
{
	protected Dictionary<string, Dictionary<string, ResourceLoadInfo>> mLoadedPath = new(); // 所有已加载的文件夹
	protected Dictionary<UObject, ResourceLoadInfo> mLoadedObjects = new();                 // 所有的已加载的资源
	public void destroy() { }
	public bool unloadResource<T>(ref T obj, bool showError) where T : UObject
	{
		if (obj == null)
		{
			return false;
		}
		// 资源已经加载完
		if (!mLoadedObjects.Remove(obj, out ResourceLoadInfo info))
		{
			if (showError)
			{
				logWarningBase("无法卸载资源:" + obj.name + ", 可能未加载,或者已经卸载,或者该资源是子资源,或者正在异步加载");
			}
			return false;
		}
		if (obj is not GameObject)
		{
			Resources.UnloadAsset(obj);
		}
		mLoadedPath.get(info.getPath()).Remove(info.getResourceName());
		obj = null;
		return true;
	}
	// 同步加载资源,name为Resources下的相对路径,不带后缀
	public T loadResource<T>(string name) where T : UObject
	{
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		var resList = mLoadedPath.getOrAddNew(path);
		// 资源未加载,则使用Resources.Load加载资源
		if (!resList.TryGetValue(name, out ResourceLoadInfo info))
		{
			load<T>(path, name);
			// 加载后需要重新获取一次
			info = resList.get(name);
			return info.getObject() as T;
		}
		if (info.getState() == LOAD_STATE.LOADED)
		{
			return info.getObject() as T;
		}
		else if (info.getState() == LOAD_STATE.DOWNLOADING)
		{
			logErrorBase("Resources资源无法下载,不能同步加载!" + name);
		}
		else if (info.getState() == LOAD_STATE.LOADING)
		{
			logWarningBase("资源正在后台加载,不能同步加载!" + name);
		}
		else if (info.getState() == LOAD_STATE.NONE)
		{
			logWarningBase("资源已加入列表,但是未加载" + name);
		}
		return null;
	}
	// 异步加载资源,name为Resources下的相对路径,不带后缀
	public void loadResourcesAsync<T>(string name, AssetLoadDoneCallback doneCallback) where T : UObject
	{
		string path = getFilePath(name);
		// 如果文件夹还未加载,则添加文件夹
		var resList = mLoadedPath.getOrAddNew(path);
		// 已经加载,则返回true
		if (resList.TryGetValue(name, out ResourceLoadInfo info))
		{
			if (info.getState() == LOAD_STATE.DOWNLOADING)
			{
				logErrorBase("Resources资源无法下载!" + name);
			}
			// 资源正在下载,将回调添加到回调列表中,等待下载完毕
			if (info.getState() == LOAD_STATE.LOADING)
			{
				info.addCallback(doneCallback, name);
			}
			// 资源已经下载完毕,直接调用回调
			else if (info.getState() == LOAD_STATE.LOADED)
			{
				ResourceLoadInfo loadInfo = mLoadedPath.get(path).get(name);
				doneCallback?.Invoke(loadInfo.getObject(), loadInfo.getSubObjects(), null, name);
			}
		}
		// 还没有加载则开始异步加载
		else
		{
			info = resList.add(name, new ResourceLoadInfo());
			info.setPath(path);
			info.setResourceName(name);
			info.setState(LOAD_STATE.LOADING);
			info.addCallback(doneCallback, name);
			GameEntry.getInstance().StartCoroutine(loadResourceCoroutine<T>(info));
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void load<T>(string path, string name) where T : UObject
	{
		var resList = mLoadedPath.get(path);
		if (resList.ContainsKey(name))
		{
			return;
		}
		var info = resList.add(name, new ResourceLoadInfo());
		info.setPath(path);
		info.setResourceName(name);
		info.setState(LOAD_STATE.LOADING);
		info.setObject(Resources.Load(name, typeof(T)));
		info.setSubObjects(Resources.LoadAll(name));
		info.setState(LOAD_STATE.LOADED);
		if (info.getObject() != null)
		{
			mLoadedObjects.Add(info.getObject(), info);
		}
		else
		{
			resList.Remove(name);
		}
	}
	protected IEnumerator loadResourceCoroutine<T>(ResourceLoadInfo info) where T : UObject
	{
		ResourceRequest request = Resources.LoadAsync<T>(info.getResourceName());
		yield return request;
		info.setObject(request.asset);
		info.setState(LOAD_STATE.LOADED);
		if (info.getObject() != null)
		{
			mLoadedObjects.Add(info.getObject(), info);
		}
		try
		{
			info.callbackAll();
		}
		catch (Exception e)
		{
			logExceptionBase(e);
		}
	}
}