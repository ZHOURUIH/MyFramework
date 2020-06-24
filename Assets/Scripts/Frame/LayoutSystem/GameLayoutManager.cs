using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

public struct LayoutAsyncInfo
{
	public string mName;
	public int mRenderOrder;
	public bool mIsNGUI;
	public bool mIsScene;
	public LAYOUT mType;
	public GameLayout mLayout;
	public GameObject mLayoutObject;
	public LayoutAsyncDone mCallback;
}

public class GameLayoutManager : FrameComponent
{
	protected Dictionary<Type, List<LAYOUT>> mScriptMappingList;
	protected Dictionary<LAYOUT, Type> mScriptRegisteList;
	protected Dictionary<LAYOUT, string> mLayoutTypeToName;
	protected Dictionary<string, LAYOUT> mLayoutNameToType;
	protected Dictionary<LAYOUT, GameLayout> mLayoutList;
	protected Dictionary<string, LayoutAsyncInfo> mLayoutAsyncList;
	protected List<GameLayout> mBackBlurLayoutList;					// 需要背景模糊的布局的列表
#if USE_NGUI
	protected txNGUIObject mNGUIRoot;
	protected UIRoot mNGUIRootComponent;
#endif
	protected txUGUICanvas mUGUIRoot;
	protected bool mUseAnchor;           // 是否启用锚点来自动调节窗口的大小和位置
	public GameLayoutManager(string name)
		: base(name)
	{
		mUseAnchor = true;
		mScriptMappingList = new Dictionary<Type, List<LAYOUT>>();
		mScriptRegisteList = new Dictionary<LAYOUT, Type>();
		mLayoutTypeToName = new Dictionary<LAYOUT, string>();
		mLayoutNameToType = new Dictionary<string, LAYOUT>();
		mLayoutList = new Dictionary<LAYOUT, GameLayout>();
		mLayoutAsyncList = new Dictionary<string, LayoutAsyncInfo>();
		mBackBlurLayoutList = new List<GameLayout>();
		// 在构造中获取UI根节点,确保其他组件能在任意时刻正常访问
		mUGUIRoot = LayoutScript.newUIObject<txUGUICanvas>(null, null, getGameObject(null, CommonDefine.UGUI_ROOT, true));
#if USE_NGUI
		mNGUIRoot = LayoutScript.newUIObject<txNGUIObject>(null, null, getGameObject(null, CommonDefine.NGUI_ROOT, true));
		mNGUIRootComponent = mNGUIRoot.getUnityComponent<UIRoot>(false);
#endif
	}
#if USE_NGUI
	public new UIRoot getNGUIRootComponent() { return mNGUIRootComponent; }
#endif
	public new Canvas getUGUIRootComponent() { return mUGUIRoot.getCanvas(); }
	public txUIObject getUIRoot(bool ngui)
	{
		if (ngui)
		{
#if USE_NGUI
			return mNGUIRoot;
#else
			return null;
#endif
		}
		else
		{
			return mUGUIRoot;
		}
	}
	public GameObject getRootObject(bool ngui)
	{
		txUIObject root = getUIRoot(ngui);
		return root != null ? root.getObject() : null;
	}
	public void notifyLayoutVisible(bool visible, GameLayout layout) 
	{
		if(visible)
		{
			if (layout.isBlurBack())
			{
				mBackBlurLayoutList.Add(layout);
			}
			// 显示布局时,如果当前正在显示有背景模糊的布局,则需要判断当前布局是否需要模糊
			if (mBackBlurLayoutList.Count > 0)
			{
				CommandLayoutManagerBackBlur cmd = newCmd(out cmd);
				cmd.mExcludeLayout = mBackBlurLayoutList;
				cmd.mBlur = mBackBlurLayoutList.Count > 0;
				pushCommand(cmd, this);
			}
		}
		else
		{
			if (layout.isBlurBack())
			{
				mBackBlurLayoutList.Remove(layout);
			}
			CommandLayoutManagerBackBlur cmd = newCmd(out cmd);
			cmd.mExcludeLayout = mBackBlurLayoutList;
			cmd.mBlur = mBackBlurLayoutList.Count > 0;
			pushCommand(cmd, this);
			// 布局在隐藏时都需要确认设置层为UI层
			setGameObjectLayer(layout.getRoot().getObject(), layout.getDefaultLayer());
		}
	}
	public void setUseAnchor(bool useAnchor) { mUseAnchor = useAnchor; }
	public bool isUseAnchor() { return mUseAnchor; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		foreach (var layout in mLayoutList)
		{
			Profiler.BeginSample(layout.Value.getName());
			layout.Value.update(elapsedTime);
			Profiler.EndSample();
		}
	}
	public override void onDrawGizmos()
	{
		foreach (var layout in mLayoutList)
		{
			layout.Value.onDrawGizmos();
		}
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		foreach (var item in mLayoutList)
		{
			item.Value.lateUpdate(elapsedTime);
		}
	}
	public override void destroy()
	{
		foreach (var item in mLayoutList)
		{
			item.Value.destroy();
		}
		mLayoutList.Clear();
		mLayoutTypeToName.Clear();
		mLayoutNameToType.Clear();
		mLayoutAsyncList.Clear();
		// 销毁UI摄像机
		mCameraManager.destroyCamera(mCameraManager.getUICamera(true));
		mCameraManager.destroyCamera(mCameraManager.getUICamera(false));
#if USE_NGUI
		txUIObject.destroyWindowSingle(mNGUIRoot, false);
		mNGUIRoot = null;
#endif
		txUIObject.destroyWindowSingle(mUGUIRoot, false);
		mUGUIRoot = null;
		base.destroy();
	}
	public string getLayoutNameByType(LAYOUT type)
	{
		if (mLayoutTypeToName.ContainsKey(type))
		{
			return mLayoutTypeToName[type];
		}
		else
		{
			logError("can not find LayoutType: " + type);
		}
		return EMPTY_STRING;
	}
	public LAYOUT getLayoutTypeByName(string name)
	{
		if (mLayoutNameToType.ContainsKey(name))
		{
			return mLayoutNameToType[name];
		}
		else
		{
			logError("can not  find LayoutName:" + name);
		}
		return LAYOUT.L_MAX;
	}
	public GameLayout getGameLayout(LAYOUT type)
	{
		return mLayoutList.ContainsKey(type) ? mLayoutList[type] : null;
	}
	public Dictionary<LAYOUT, GameLayout> getLayoutList() { return mLayoutList; }
	public T getScript<T>(LAYOUT type) where T : LayoutScript
	{
		GameLayout layout = getGameLayout(type);
		return layout != null ? layout.getScript() as T : null;
	}
	public int getScriptMappingCount(Type classType)
	{
		return mScriptMappingList[classType].Count;
	}
	public GameLayout createLayout(LAYOUT type, int renderOrder, bool async, LayoutAsyncDone callback, bool isNGUI, bool isScene)
	{
		if (mLayoutList.ContainsKey(type))
		{
			if (async && callback != null)
			{
				callback(mLayoutList[type]);
				return null;
			}
			else
			{
				return mLayoutList[type];
			}
		}
		string name = getLayoutNameByType(type);
		string path = isNGUI ? CommonDefine.R_NGUI_PREFAB_PATH : CommonDefine.R_UGUI_PREFAB_PATH;
		GameObject layoutParent = getRootObject(isNGUI);
		if (isScene)
		{
			layoutParent = null;
		}
		// 如果是异步加载则,则先加入列表中
		if (async)
		{
			LayoutAsyncInfo info = new LayoutAsyncInfo();
			info.mName = name;
			info.mType = type;
			info.mRenderOrder = renderOrder;
			info.mLayout = null;
			info.mLayoutObject = null;
			info.mIsNGUI = isNGUI;
			info.mIsScene = isScene;
			info.mCallback = callback;
			mLayoutAsyncList.Add(info.mName, info);
			bool ret = mResourceManager.loadResourceAsync<GameObject>(path + name + "/" + name, onLayoutPrefabAsyncDone, null, true);
			if (!ret)
			{
				logError("can not find layout : " + name);
			}
			return null;
		}
		else
		{
			GameObject prefab = mResourceManager.loadResource<GameObject>(path + name + "/" + name, true);
			instantiatePrefab(layoutParent, prefab, name, true);
			GameLayout layout = new GameLayout();
			layout.setPrefab(prefab);
			addLayoutToList(layout, type);
			layout.init(type, name, renderOrder, isNGUI, isScene);
			return layout;
		}
	}
	public void destroyLayout(LAYOUT type)
	{
		GameLayout layout = getGameLayout(type);
		if (layout == null)
		{
			return;
		}
		removeLayoutFromList(layout);
		layout.destroy();
	}
	public LayoutScript createScript(string name, GameLayout layout)
	{
		LayoutScript script = createInstance<LayoutScript>(mScriptRegisteList[layout.getType()], name);
		script.setLayout(layout);
		return script;
	}
	public List<Collider> getAllLayoutBoxCollider()
	{
		var allBoxList = new List<Collider>();
		foreach (var layout in mLayoutList)
		{
			allBoxList.AddRange(layout.Value.getAllCollider());
		}
		return allBoxList;
	}
	public void registeLayout(Type classType, LAYOUT type, string name)
	{
		mLayoutTypeToName.Add(type, name);
		mLayoutNameToType.Add(name, type);
		mScriptRegisteList.Add(type, classType);
		if (!mScriptMappingList.ContainsKey(classType))
		{
			mScriptMappingList.Add(classType, new List<LAYOUT>());
		}
		mScriptMappingList[classType].Add(type);
	}
	// 获取已注册的布局数量,而不是已加载的布局数量
	public int getLayoutCount() { return mLayoutTypeToName.Count; }
	//----------------------------------------------------------------------------------------------------------------------------------------------------
	protected void addLayoutToList(GameLayout layout, LAYOUT type)
	{
		mLayoutList.Add(type, layout);
	}
	protected void removeLayoutFromList(GameLayout layout)
	{
		if (layout != null)
		{
			mLayoutList.Remove(layout.getType());
		}
	}
	protected void onLayoutPrefabAsyncDone(UnityEngine.Object asset, UnityEngine.Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		LayoutAsyncInfo info = mLayoutAsyncList[asset.name];
		mLayoutAsyncList.Remove(asset.name);
		info.mLayoutObject = instantiatePrefab(null, (GameObject)asset, true);
		info.mLayout = new GameLayout();
		addLayoutToList(info.mLayout, info.mType);
		info.mLayout.setPrefab(asset as GameObject);
		GameObject layoutParent = getRootObject(info.mIsNGUI);
		if (info.mIsScene)
		{
			layoutParent = null;
		}
		setNormalProperty(info.mLayoutObject, layoutParent, info.mName);
		info.mLayout.init(info.mType, info.mName, info.mRenderOrder, info.mIsNGUI, info.mIsScene);
		info.mCallback?.Invoke(info.mLayout);
	}
}