using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBase;
using static FrameDefine;
using static FrameBaseDefine;
using static FileUtility;
using static FrameBaseUtility;

// 布局管理器
public class LayoutManager : FrameSystem
{
	protected Dictionary<Type, LayoutRegisteInfo> mLayoutRegisteList = new();	// 布局注册信息列表
	protected Dictionary<Type, GameLayout> mLayoutList = new();					// 所有布局的列表
	protected Dictionary<string, LayoutInfo> mLayoutAsyncList = new();			// 正在异步加载的布局列表
	protected Canvas mUGUIRoot;													// 所有UI的根节点
	public LayoutManager()
	{
		// 在构造中获取UI根节点,确保其他组件能在任意时刻正常访问
		mUGUIRoot = getRootGameObject(UGUI_ROOT, true).GetComponent<Canvas>();
	}
	public Vector2 getRootSize() { return (mUGUIRoot.transform as RectTransform).rect.size; }
	public Canvas getUIRoot() { return mUGUIRoot; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (GameLayout item in mLayoutList.Values)
		{
			try
			{
				item.updateLayout(elapsedTime);
			}
			catch (Exception e)
			{
				logExceptionBase(e,"界面:" + item.getName());
			}
		}
	}
	public override void willDestroy()
	{
		foreach (GameLayout item in mLayoutList.Values)
		{
			item.destroy();
		}
		mLayoutList.Clear();
		mLayoutAsyncList.Clear();
		mUGUIRoot = null;
		Resources.UnloadUnusedAssets();
		base.willDestroy();
	}
	public string getLayoutPathByType(Type type) { return mLayoutRegisteList.get(type).mFileNameNoSuffix; }
	public GameLayout getLayout(Type type) { return mLayoutList.get(type); }
	public GameLayout createLayout(LayoutInfo info)
	{
		if (mLayoutList.TryGetValue(info.mType, out GameLayout existLayout))
		{
			return existLayout;
		}
		if (isWebGL())
		{
			logErrorBase("webgl无法同步加载界面");
			return null;
		}
		return newLayout(info, mResourceManager.loadInResource<GameObject>(R_UI_PREFAB_PATH + getLayoutPathByType(info.mType) + ".prefab"));
	}
	public void createLayoutAsync(LayoutInfo info, GameLayoutCallback callback)
	{
		if (mLayoutList.TryGetValue(info.mType, out GameLayout existLayout))
		{
			callback?.Invoke(existLayout);
			return;
		}
		mLayoutAsyncList.Add(info.mType.ToString(), info);
		mResourceManager.loadInResourceAsync(R_UI_PREFAB_PATH + getLayoutPathByType(info.mType) + ".prefab", (GameObject asset) =>
		{
			if (mLayoutAsyncList.Remove(asset.name, out LayoutInfo info))
			{
				callback?.Invoke(newLayout(info, asset));
			}
		});
	}
	public void registeLayout(Type classType, string fileName, GameLayoutCallback callback)
	{
		// 编辑器下检查文件是否存在
		if (isEditor() && !isFileExist(P_RESOURCES_UI_PREFAB_PATH + fileName + ".prefab"))
		{
			logErrorBase("界面文件不存在:" + P_RESOURCES_UI_PREFAB_PATH + fileName + ".prefab");
			return;
		}
		LayoutRegisteInfo info = new();
		info.mScriptType = classType;
		info.mCallback = callback;
		info.mFileNameNoSuffix = fileName;
		mLayoutRegisteList.Add(classType, info);
	}	
	// 卸载所有非常驻的布局
	public void unloadAllLayout()
	{
		foreach (GameLayout pair in mLayoutList.Values)
		{
			pair.setVisible(false);
			pair.destroy();
		}
		mLayoutList.Clear();
	}
	public void notifyLayoutChanged(GameLayout layout, bool createOrDestroy)
	{
		mLayoutRegisteList.get(layout.getType()).mCallback?.Invoke(createOrDestroy ? layout : null);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected GameLayout newLayout(LayoutInfo info, GameObject prefab)
	{
		GameObject layoutObj = instantiatePrefab(mUGUIRoot.gameObject, prefab, info.mType.ToString(), true);
		var layout = Activator.CreateInstance(info.mType) as GameLayout;
		layout.setPrefab(prefab);
		layout.setType(info.mType);
		layout.setRenderOrder(info.mRenderOrder);
		layout.initLayout();
		if (layout.getRoot().gameObject != layoutObj)
		{
			logErrorBase("布局的根节点不是实例化出来的节点,请确保运行前UI根节点下没有与布局同名的节点");
		}
		return mLayoutList.add(info.mType, layout);
	}
}