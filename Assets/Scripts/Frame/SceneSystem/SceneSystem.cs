using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public struct SceneRegisteInfo
{
	public string mName;
	public string mScenePath;
	public Type mSceneType;
}

public class SceneSystem : FrameComponent
{
	protected Dictionary<string, SceneInstance> mSceneList;
	protected Dictionary<string, SceneRegisteInfo> mSceneRegisteList;
	protected List<SceneLoadCallback> mDirectLoadCallback;  // 请求加载已经加载的场景时的回调,因为这些回调需要延迟一帧调用,所以放入列表中再调用
	protected List<SceneInstance> mLoadList;
	public SceneSystem(string name)
		:base(name)
	{
		mSceneList = new Dictionary<string, SceneInstance>();
		mSceneRegisteList = new Dictionary<string, SceneRegisteInfo>();
		mLoadList = new List<SceneInstance>();
		mDirectLoadCallback = new List<SceneLoadCallback>();
	}
	public override void destroy()
	{
		base.destroy();
		foreach (var item in mSceneList)
		{
			unloadSceneOnly(item.Key);
		}
		mSceneList.Clear();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if(mDirectLoadCallback.Count > 0)
		{
			// 只调用列表中第一个回调,避免回调中再次对该列表进行修改,且需要在列表中移除后再调用
			SceneLoadCallback callback = mDirectLoadCallback[0];
			mDirectLoadCallback.RemoveAt(0);
			callback(1.0f, true);
		}
		// 场景AssetBundle加载完毕时才开始加载场景
		for(int i = 0; i < mLoadList.Count; ++i)
		{
			if (mLoadList[i].mState == LOAD_STATE.LS_UNLOAD)
			{
				mGameFramework.StartCoroutine(loadSceneCoroutine(mLoadList[i]));
			}
			else if (mLoadList[i].mState == LOAD_STATE.LS_LOADED)
			{
				mLoadList.RemoveAt(i--);
			}
		}
		foreach(var item in mSceneList)
		{
			if(item.Value.getActive())
			{
				item.Value.update(elapsedTime);
			}
		}
	}
	// filePath是场景文件所在目录,不含场景名,最好该目录下包含所有只在这个场景使用的资源
	public void registeScene(Type type, string name, string filePath)
	{
		// 路径需要以/结尾
		if (!endWith(filePath, "/"))
		{
			filePath += "/";
		}
		SceneRegisteInfo info = new SceneRegisteInfo();
		info.mName = name;
		info.mScenePath = filePath;
		info.mSceneType = type;
		mSceneRegisteList.Add(name, info);
	}
	public string getScenePath(string name)
	{
		return mSceneRegisteList.ContainsKey(name) ? mSceneRegisteList[name].mScenePath : EMPTY_STRING;
	}
	public T getScene<T>(string name) where T : SceneInstance
	{
		return mSceneList.ContainsKey(name) ? mSceneList[name] as T : null;
	}
	public void setMainScene(string name)
	{
		if (!mSceneList.ContainsKey(name))
		{
			return;
		}
		SceneManager.SetActiveScene(mSceneList[name].mScene);
	}
	public void hideScene(string name)
	{
		if (!mSceneList.ContainsKey(name))
		{
			return;
		}
		mSceneList[name].setActive(false);
		mSceneList[name].onHide();
	}
	public void showScene(string name, bool hideOther = true, bool mainScene = true)
	{
		if (!mSceneList.ContainsKey(name))
		{
			return;
		}
		// 如果需要隐藏其他场景,则遍历所有场景设置可见性
		if(hideOther)
		{
			foreach(var item in mSceneList)
			{
				item.Value.setActive(name == item.Key);
				if(name == item.Key)
				{
					item.Value.onShow();
				}
				else
				{
					item.Value.onHide();
				}
			}
		}
		// 不隐藏其他场景则只是简单的将指定场景显示
		else
		{
			mSceneList[name].setActive(true);
			mSceneList[name].onShow();
		}
		if(mainScene)
		{
			setMainScene(name);
		}
	}
	// 目前只支持异步加载,因为SceneManager.LoadScene并不是真正地同步加载
	// 该方法只能保证在这一帧结束后场景能加载完毕,但是函数返回后场景并没有加载完毕
	public void loadSceneAsync(string sceneName, bool active, SceneLoadCallback callback)
	{
		// 如果场景已经加载,则直接返回
		if (mSceneList.ContainsKey(sceneName))
		{
			showScene(sceneName);
			if (callback != null)
			{
				mDirectLoadCallback.Add(callback);
			}
			return;
		}
		SceneInstance scene = createScene(sceneName);
		scene.mState = LOAD_STATE.LS_UNLOAD;
		scene.mActiveLoaded = active;
		scene.mLoadCallback = callback;
		mSceneList.Add(scene.mName, scene);
		// scenePath + sceneName表示场景文件AssetBundle的路径,包含文件名
		mResourceManager.loadAssetBundleAsync(getScenePath(sceneName) + sceneName, onSceneAssetBundleLoaded, scene);
	}
	// unloadPath表示是否将场景所属文件夹的所有资源卸载
	public void unloadScene(string name, bool unloadPath = true)
	{
		// 销毁场景,并且从列表中移除
		unloadSceneOnly(name);
		mSceneList.Remove(name);
		if(unloadPath)
		{
			mResourceManager.unloadPath(mSceneRegisteList[name].mScenePath);
		}
	}
	// 卸载除了dontUnloadSceneName以外的其他场景,初始默认场景除外
	public void unloadOtherScene(string dontUnloadSceneName)
	{
		var tempList = new Dictionary<string, SceneInstance>(mSceneList);
		foreach(var item in tempList)
		{
			if(item.Key != dontUnloadSceneName)
			{
				unloadScene(item.Key);
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onSceneAssetBundleLoaded(AssetBundleInfo bundle, object userData)
	{
		mLoadList.Add(userData as SceneInstance);
	}
	protected IEnumerator loadSceneCoroutine(SceneInstance scene)
	{
		scene.mState = LOAD_STATE.LS_LOADING;
		// 所有场景都只能使用叠加的方式来加载,方便场景管理器来管理所有场景的加载和卸载
		scene.mOperation = SceneManager.LoadSceneAsync(scene.mName, LoadSceneMode.Additive);
		// allowSceneActivation指定了加载场景时是否需要调用场景中所有脚本的Awake和Start,以及贴图材质的引用等等
		scene.mOperation.allowSceneActivation = true;
		WaitForEndOfFrame waitEndFrame = new WaitForEndOfFrame();
		while (true)
		{
			scene.mLoadCallback?.Invoke(scene.mOperation.progress, false);
			// 当allowSceneActivation为true时,加载到progress为1时停止,并且isDone为true,scene.isLoaded为true
			// 当allowSceneActivation为false时,加载到progress为0.9时就停止,并且isDone为false, scene.isLoaded为false
			// 当场景被激活时isDone变为true,progress也为1,scene.isLoaded为true
			if (scene.mOperation.isDone || scene.mOperation.progress >= 1.0f)
			{
				break;
			}
			yield return waitEndFrame;
		}
		// 首先获得场景
		scene.mScene = SceneManager.GetSceneByName(scene.mName);
		// 获得了场景根节点才能使场景显示或隐藏
		scene.mRoot = getGameObject(null, scene.mName + "_Root", true);
		// 加载完毕后就立即初始化
		scene.init();
		if(scene.mActiveLoaded)
		{
			showScene(scene.mName);
		}
		else
		{
			hideScene(scene.mName);
		}
		scene.mState = LOAD_STATE.LS_LOADED;
		scene.mLoadCallback?.Invoke(1.0f, true);
	}
	protected SceneInstance createScene(string sceneName)
	{
		if(!mSceneRegisteList.ContainsKey(sceneName))
		{
			logError("scene :" + sceneName + " is not registed!");
			return null;
		}
		return createInstance<SceneInstance>(mSceneRegisteList[sceneName].mSceneType, sceneName);
	}
	// 只销毁场景,不从列表移除
	protected void unloadSceneOnly(string name)
	{
		if (!mSceneList.ContainsKey(name))
		{
			return;
		}
		mSceneList[name].destroy();
		if (mSceneList[name].mRoot != null)
		{
#if UNITY_5_3_5
			SceneManager.UnloadScene(name);
#else
			SceneManager.UnloadSceneAsync(name);
#endif
		}
	}
}