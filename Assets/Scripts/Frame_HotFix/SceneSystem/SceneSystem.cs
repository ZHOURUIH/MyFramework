using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using static UnityUtility;
using static FrameBaseHotFix;
using static FileUtility;
using static StringUtility;
using static CSharpUtility;
using static FrameUtility;

// 3D场景管理器,管理unity场景资源
public class SceneSystem : FrameSystem
{
	protected Dictionary<Type,List<SceneRegisteInfo>> mScriptMappingList = new();	// 场景脚本类型与场景注册信息的映射,允许多个相似的场景共用同一个场景脚本
	protected Dictionary<string, SceneRegisteInfo> mSceneRegisteList = new();		// 场景注册信息
	protected Dictionary<string, SceneInstance> mSceneList = new();					// 已经加载的所有场景
	public override void destroy()
	{
		base.destroy();
		foreach (string item in mSceneList.Keys)
		{
			unloadSceneOnly(item);
		}
		mSceneList.Clear();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (SceneInstance item in mSceneList.Values)
		{
			if (item.getActive())
			{
				item.update(elapsedTime);
			}
		}
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		foreach (SceneInstance item in mSceneList.Values)
		{
			if (item.getActive())
			{
				item.lateUpdate(elapsedTime);
			}
		}
	}
	// filePath是场景文件所在目录,不含场景名,最好该目录下包含所有只在这个场景使用的资源
	public void registeScene(Type type, string name, string filePath, SceneScriptCallback callback)
	{
		// 路径需要以/结尾
		validPath(ref filePath);
		SceneRegisteInfo info = mSceneRegisteList.add(name, new());
		info.mName = name;
		info.mScenePath = filePath;
		info.mSceneType = type;
		info.mCallback = callback;
		mScriptMappingList.getOrAddNew(type).Add(info);
	}
	public string getScenePath(string name) { return mSceneRegisteList.get(name)?.mScenePath ?? EMPTY; }
	public T getScene<T>(string name) where T : SceneInstance { return mSceneList.get(name) as T; }
	public int getScriptMappingCount(Type classType) { return mScriptMappingList.get(classType).count(); }
	public void setMainScene(string name)
	{
		if (!mSceneList.TryGetValue(name, out SceneInstance scene))
		{
			return;
		}
		SceneManager.SetActiveScene(scene.getScene());
	}
	public void hideScene(string name)
	{
		if (!mSceneList.TryGetValue(name, out SceneInstance scene))
		{
			return;
		}
		scene.setActive(false);
		scene.onHide();
	}
	public void showScene(string name, bool hideOther = true, bool mainScene = true)
	{
		if (!mSceneList.TryGetValue(name, out SceneInstance scene))
		{
			return;
		}
		// 如果需要隐藏其他场景,则遍历所有场景设置可见性
		if (hideOther)
		{
			foreach (var item in mSceneList)
			{
				SceneInstance curScene = item.Value;
				curScene.setActive(curScene == scene);
				if (curScene == scene)
				{
					curScene.onShow();
				}
				else
				{
					curScene.onHide();
				}
			}
		}
		// 不隐藏其他场景则只是简单的将指定场景显示
		else
		{
			scene.setActive(true);
			scene.onShow();
		}
		if (mainScene)
		{
			setMainScene(name);
		}
	}
	// 目前只支持异步加载,因为SceneManager.LoadScene并不是真正地同步加载
	// 该方法只能保证在这一帧结束后场景能加载完毕,但是函数返回后场景并没有加载完毕
	public CustomAsyncOperation loadSceneAsync(string sceneName, bool active, bool mainScene, Action loadedCallback, FloatCallback loadingCallback = null)
	{
		CustomAsyncOperation op = new();
		// 如果场景已经加载,则直接返回
		if (mSceneList.ContainsKey(sceneName))
		{
			showScene(sceneName, false, mainScene);
			if (loadingCallback != null || loadedCallback != null)
			{
				delayCall(() =>
				{
					loadingCallback?.Invoke(1.0f);
					loadedCallback?.Invoke();
					op.setFinish();
				});
			}
		}
		else
		{
			SceneInstance scene = mSceneList.add(sceneName, createScene(sceneName));
			scene.setState(LOAD_STATE.NONE);
			scene.setActiveLoaded(active);
			scene.setMainScene(mainScene);
			scene.setLoadingCallback(loadingCallback);
			scene.setLoadedCallback(loadedCallback);
			// scenePath + sceneName表示场景文件AssetBundle的路径,包含文件名
			mResourceManager.loadAssetBundleAsync(getScenePath(sceneName) + sceneName, (AssetBundleInfo bundle) =>
			{
				mGameFrameworkHotFix.StartCoroutine(loadSceneCoroutine(scene, op));
			});
		}
		return op;
	}
	// unloadPath表示是否将场景所属文件夹的所有资源卸载
	public void unloadScene(string name, bool unloadPath = true)
	{
		// 销毁场景,并且从列表中移除
		unloadSceneOnly(name);
		mSceneList.Remove(name);
		if (unloadPath)
		{
			mResourceManager?.unloadPath(mSceneRegisteList.get(name).mScenePath);
		}
	}
	// 卸载除了dontUnloadSceneName以外的其他场景,初始默认场景除外
	public void unloadOtherScene(string dontUnloadSceneName, bool unloadPath = true)
	{
		using var a = new ListScope<string>(out var tempList, mSceneList.Keys);
		foreach (string sceneName in tempList)
		{
			if (sceneName != dontUnloadSceneName)
			{
				unloadScene(sceneName, unloadPath);
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected IEnumerator loadSceneCoroutine(SceneInstance scene, CustomAsyncOperation op)
	{
		scene.setState(LOAD_STATE.LOADING);
		// 所有场景都只能使用叠加的方式来加载,方便场景管理器来管理所有场景的加载和卸载
		AsyncOperation operation = SceneManager.LoadSceneAsync(scene.getName(), LoadSceneMode.Additive);
		// allowSceneActivation指定了加载场景时是否需要调用场景中所有脚本的Awake和Start,以及贴图材质的引用等等
		operation.allowSceneActivation = true;
		while (true)
		{
			scene.callLoading(operation.progress);
			yield return null;
			// 当allowSceneActivation为true时,加载到progress为1时停止,并且isDone为true,scene.isLoaded为true
			// 当allowSceneActivation为false时,加载到progress为0.9时就停止,并且isDone为false, scene.isLoaded为false
			// 当场景被激活时isDone变为true,progress也为1,scene.isLoaded为true
			if (operation.isDone || operation.progress >= 1.0f)
			{
				break;
			}
		}
		// 首先获得场景
		scene.setScene(SceneManager.GetSceneByName(scene.getName()));
		// 获得了场景根节点才能使场景显示或隐藏,为了尽量避免此处查找节点错误,所以不能使用容易重名的名字
		scene.setRoot(getRootGameObject(scene.getName() + "_Root", true));
		// 加载完毕后就立即初始化
		scene.init();
		if (scene.isActiveLoaded())
		{
			showScene(scene.getName(), false, scene.isMainScene());
		}
		else
		{
			hideScene(scene.getName());
		}
		scene.setState(LOAD_STATE.LOADED);
		try
		{
			scene.callLoading(1.0f);
			scene.callLoaded();
		}
		catch (Exception e)
		{
			logException(e);
		}
		op.setFinish();
	}
	protected SceneInstance createScene(string sceneName)
	{
		if (!mSceneRegisteList.TryGetValue(sceneName, out SceneRegisteInfo info))
		{
			logError("scene :" + sceneName + " is not registed!");
			return null;
		}
		var scene = createInstance<SceneInstance>(info.mSceneType);
		scene.setName(sceneName);
		scene.setType(info.mSceneType);
		notifySceneChanged(scene, true);
		return scene;
	}
	// 只销毁场景,不从列表移除
	protected void unloadSceneOnly(string name)
	{
		if (!mSceneList.TryGetValue(name, out SceneInstance scene))
		{
			return;
		}
		notifySceneChanged(scene, false);
		scene.destroy();
		SceneManager.UnloadSceneAsync(name);
	}
	protected void notifySceneChanged(SceneInstance scene, bool isLoad)
	{
		mSceneRegisteList.get(scene.getName())?.mCallback?.Invoke(isLoad ? scene : null);
	}
}