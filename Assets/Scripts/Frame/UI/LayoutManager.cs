using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

// 布局管理器
public class LayoutManager : FrameSystem
{
	protected Dictionary<int, LayoutRegisteInfo> mLayoutRegisteList;	// 布局注册信息列表
	protected SafeDictionary<int, GameLayout> mLayoutList;				// 所有布局的列表
	protected Dictionary<string, LayoutInfo> mLayoutAsyncList;			// 正在异步加载的布局列表
	protected Dictionary<Type, List<int>> mScriptMappingList;			// 布局脚本类型与布局ID的映射,允许多个不同的布局共用同一个布局脚本
	protected Dictionary<int, string> mLayoutTypeToPath;				// 根据布局ID查找布局路径
	protected Dictionary<string, int> mLayoutPathToType;				// 根据布局路径查找布局ID
	protected HashSet<LayoutScriptCallback> mLayoutScriptCallback;		// 脚本的回调列表
	protected List<GameLayout> mBackBlurLayoutList;						// 需要背景模糊的布局的列表
	protected COMLayoutManagerEscHide mCOMEscHide;						// Esc按键事件传递逻辑的组件
	protected AssetLoadDoneCallback mLayoutLoadCallback;				// 保存回调变量,避免GC
	protected myUGUICanvas mUGUIRoot;									// 所有UI的根节点
	protected bool mUseAnchor;											// 是否启用锚点来自动调节窗口的大小和位置
	public LayoutManager()
	{
		mUseAnchor = true;
		mLayoutRegisteList = new Dictionary<int, LayoutRegisteInfo>();
		mScriptMappingList = new Dictionary<Type, List<int>>();
		mLayoutTypeToPath = new Dictionary<int, string>();
		mLayoutPathToType = new Dictionary<string, int>();
		mLayoutList = new SafeDictionary<int, GameLayout>();
		mLayoutAsyncList = new Dictionary<string, LayoutInfo>();
		mLayoutScriptCallback = new HashSet<LayoutScriptCallback>();
		mBackBlurLayoutList = new List<GameLayout>();
		mLayoutLoadCallback = onLayoutPrefabLoaded;
		// 在构造中获取UI根节点,确保其他组件能在任意时刻正常访问
		mUGUIRoot = LayoutScript.newUIObject<myUGUICanvas>(null, null, getGameObject(FrameDefine.UGUI_ROOT, true));
	}
	public new Canvas getUGUIRootComponent() { return mUGUIRoot.getCanvas(); }
	public myUGUICanvas getUIRoot() { return mUGUIRoot; }
	public GameObject getRootObject() { return mUGUIRoot?.getObject(); }
	public void notifyLayoutRenderOrder()
	{
		mCOMEscHide.notifyLayoutRenderOrder();
	}
	public void notifyLayoutVisible(bool visible, GameLayout layout)
	{
		mCOMEscHide.notifyLayoutVisible(visible, layout);
		if (visible)
		{
			if (layout.isBlurBack())
			{
				mBackBlurLayoutList.Add(layout);
			}
			// 显示布局时,如果当前正在显示有背景模糊的布局,则需要判断当前布局是否需要模糊
			if (mBackBlurLayoutList.Count > 0)
			{
				CMD(out CmdLayoutManagerBackBlur cmd, LOG_LEVEL.LOW);
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
			CMD(out CmdLayoutManagerBackBlur cmd, LOG_LEVEL.LOW);
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
		var updateList = mLayoutList.startForeach();
		foreach (var item in updateList)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Profiler.BeginSample(item.Value.getName());
#endif
			item.Value.update(elapsedTime);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Profiler.EndSample();
#endif
		}
	}
	public override void onDrawGizmos()
	{
		var updateList = mLayoutList.startForeach();
		foreach (var item in updateList)
		{
			item.Value.onDrawGizmos();
		}
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		var updateList = mLayoutList.startForeach();
		foreach (var item in updateList)
		{
			item.Value.lateUpdate(elapsedTime);
		}
	}
	public override void destroy()
	{
		mInputSystem.unlistenKey(this);
		var updateList = mLayoutList.startForeach();
		foreach (var item in updateList)
		{
			item.Value.destroy();
		}
		mLayoutList.clear();
		mLayoutTypeToPath.Clear();
		mLayoutPathToType.Clear();
		mLayoutAsyncList.Clear();
		// 销毁UI摄像机
		mCameraManager.destroyCamera(mCameraManager.getUICamera());
		myUIObject.destroyWindowSingle(mUGUIRoot, false);
		mUGUIRoot = null;
		base.destroy();
	}
	public string getLayoutPathByType(int type)
	{
		if (!mLayoutTypeToPath.TryGetValue(type, out string name))
		{
			logError("can not find LayoutType: " + type);
			return null;
		}
		return name;
	}
	public int getLayoutTypeByPath(string path)
	{
		if (!mLayoutPathToType.TryGetValue(path, out int layoutID))
		{
			logError("can not  find LayoutName:" + path);
			return LAYOUT.NONE;
		}
		return layoutID;
	}
	public GameLayout getLayout(int id)
	{
		mLayoutList.tryGetValue(id, out GameLayout layout);
		return layout;
	}
	public SafeDictionary<int, GameLayout> getLayoutList() { return mLayoutList; }
	public LayoutScript getScript(int id)
	{
		return getLayout(id)?.getScript();
	}
	public int getScriptMappingCount(Type classType)
	{
		return mScriptMappingList[classType].Count;
	}
	// 根据顺序类型,计算实际的渲染顺序
	public int generateRenderOrder(GameLayout exceptLayout, int renderOrder, LAYOUT_ORDER orderType)
	{
		if (orderType == LAYOUT_ORDER.ALWAYS_TOP)
		{
			if (renderOrder < FrameDefine.ALWAYS_TOP_ORDER)
			{
				renderOrder += FrameDefine.ALWAYS_TOP_ORDER;
			}
		}
		else if (orderType == LAYOUT_ORDER.ALWAYS_TOP_AUTO)
		{
			renderOrder = getTopLayoutOrder(exceptLayout, true) + 1;
		}
		else if (orderType == LAYOUT_ORDER.AUTO)
		{
			renderOrder = getTopLayoutOrder(exceptLayout, false) + 1;
		}
		return renderOrder;
	}
	public GameLayout createLayout(LayoutInfo info, bool async)
	{
		if (mLayoutList.tryGetValue(info.mID, out GameLayout existLayout))
		{
			if (async)
			{
				info.mCallback?.Invoke(existLayout);
			}
			return existLayout;
		}
		string path = getLayoutPathByType(info.mID);
		info.mName = getFileNameNoSuffix(path, true);
		string fullPath = FrameDefine.R_UGUI_PREFAB_PATH + path;
		// 如果是异步加载则,则先加入列表中
		if (async)
		{
			mLayoutAsyncList.Add(info.mName, info);
			bool result = false;
			if (mLayoutRegisteList[info.mID].mInResource)
			{
				result = mResourceManager.loadInResourceAsync<GameObject>(fullPath, mLayoutLoadCallback);
			}
			else
			{
				result = mResourceManager.loadResourceAsync<GameObject>(fullPath, mLayoutLoadCallback);
			}
			if (!result)
			{
				logError("can not find layout : " + path);
			}
			return null;
		}
		else
		{
			if (mLayoutRegisteList[info.mID].mInResource)
			{
				return newLayout(info, mResourceManager.loadInResource<GameObject>(fullPath));
			}
			else
			{
				return newLayout(info, mResourceManager.loadResource<GameObject>(fullPath));
			}
		}
	}
	public void destroyLayout(int id)
	{
		GameLayout layout = getLayout(id);
		if (layout == null)
		{
			return;
		}
		mCOMEscHide.notifyLayoutDestroy(layout);
		mLayoutList.remove(id);
		layout.destroy();
	}
	public LayoutScript createScript(GameLayout layout)
	{
		LayoutRegisteInfo info = mLayoutRegisteList[layout.getID()];
		var script = createInstance<LayoutScript>(info.mScriptType);
		if (script == null)
		{
			logError("界面脚本未注册, ID:" + layout.getID());
			return null;
		}
		script.setType(info.mScriptType);
		script.setLayout(layout);
		return script;
	}
	public void getAllLayoutBoxCollider(List<Collider> colliders)
	{
		colliders.Clear();
		var mainList = mLayoutList.getMainList();
		foreach (var item in mainList)
		{
			item.Value.getAllCollider(colliders, true);
		}
	}
	public void registeLayout(Type classType, int id, string name, bool inResource, LAYOUT_LIFE_CYCLE lifeCycle)
	{
		var info = new LayoutRegisteInfo();
		info.mScriptType = classType;
		info.mName = name;
		info.mID = id;
		info.mInResource = inResource;
		info.mLifeCycle = lifeCycle;
		mLayoutTypeToPath.Add(id, name);
		mLayoutPathToType.Add(name, id);
		mLayoutRegisteList.Add(id, info);
		if (!mScriptMappingList.TryGetValue(classType, out List<int> list))
		{
			list = new List<int>();
			mScriptMappingList.Add(classType, list);
		}
		list.Add(id);
	}
	// 获取已注册的布局数量,而不是已加载的布局数量
	public int getLayoutCount() { return mLayoutTypeToPath.Count; }
	// 获取当前已经显示的布局中最上层布局的渲染深度,但是不包括始终在最上层的布局
	public int getTopLayoutOrder(GameLayout exceptLayout, bool alwaysTop)
	{
		int maxOrder = 0;
		var mainList = mLayoutList.getMainList();
		foreach (var item in mainList)
		{
			GameLayout layout = item.Value;
			if (exceptLayout == layout)
			{
				continue;
			}
			bool curIsAlwaysTop = layout.getRenderOrderType() == LAYOUT_ORDER.ALWAYS_TOP ||
								  layout.getRenderOrderType() == LAYOUT_ORDER.ALWAYS_TOP_AUTO;
			if (!layout.isVisible() || alwaysTop != curIsAlwaysTop)
			{
				continue;
			}
			maxOrder = getMax(maxOrder, layout.getRenderOrder());
		}
		// 如果没有始终在最上层的布局,则需要确保渲染顺序最低不能小于指定值
		if (alwaysTop && maxOrder == 0)
		{
			maxOrder += FrameDefine.ALWAYS_TOP_ORDER;
		}
		return maxOrder;
	}
	// 卸载所有非常驻的布局
	public void unloadAllPartLayout()
	{
		var mainList = mLayoutList.startForeach();
		foreach (var item in mainList)
		{
			if (mLayoutRegisteList[item.Key].mLifeCycle == LAYOUT_LIFE_CYCLE.PART_USE)
			{
				LT.UNLOAD_LAYOUT(item.Key);
			}
		}
	}
	public void addScriptCallback(LayoutScriptCallback callback)
	{
		mLayoutScriptCallback.Add(callback);
	}
	public void removeScriptCallback(LayoutScriptCallback callback)
	{
		mLayoutScriptCallback.Remove(callback);
	}
	public void notifyLayoutChanged(GameLayout layout, bool isLoad)
	{
		foreach(var item in mLayoutScriptCallback)
		{
			item.Invoke(layout.getScript(), isLoad);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addComponent(out mCOMEscHide, true);
	}
	protected void onLayoutPrefabLoaded(UnityEngine.Object asset, UnityEngine.Object[] subAssets, byte[] bytes, object userData, string loadPath)
	{
		LayoutInfo info = mLayoutAsyncList[asset.name];
		mLayoutAsyncList.Remove(asset.name);
		GameLayout layout = newLayout(info, asset as GameObject);
		info.mCallback?.Invoke(layout);
	}
	protected GameLayout newLayout(LayoutInfo info, GameObject prefab)
	{
		myUIObject layoutParent = info.mIsScene ? null : getUIRoot();
		GameObject layoutObj = instantiatePrefab(layoutParent?.getObject(), prefab, info.mName, true);
		GameLayout layout = new GameLayout();
		layout.setPrefab(prefab);
		layout.setID(info.mID);
		layout.setName(info.mName);
		layout.setParent(layoutParent);
		layout.setOrderType(info.mOrderType);
		layout.setRenderOrder(generateRenderOrder(layout, info.mRenderOrder, info.mOrderType));
		layout.setInResources(mLayoutRegisteList[info.mID].mInResource);
		layout.init();
		if (layout.getRoot().getObject() != layoutObj)
		{
			logError("布局的根节点不是实例化出来的节点,请确保运行前UI根节点下没有与布局同名的节点");
		}
		mLayoutList.add(info.mID, layout);
		return layout;
	}
}