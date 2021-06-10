using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class LayoutManager : FrameSystem
{
	protected SafeDictionary<int, GameLayout> mLayoutList;
	protected Dictionary<string, LayoutInfo> mLayoutAsyncList;
	protected Dictionary<Type, List<int>> mScriptMappingList;
	protected Dictionary<int, string> mLayoutTypeToPath;
	protected Dictionary<string, int> mLayoutPathToType;
	protected Dictionary<int, Type> mScriptRegisteList;
	protected List<GameLayout> mBackBlurLayoutList;             // 需要背景模糊的布局的列表
	protected COMLayoutManagerEscHide mCOMEscHide;				// Esc按键事件传递逻辑的组件
	protected AssetLoadDoneCallback mLayoutLoadCallback;		// 保存回调变量,避免GC
	protected myUGUICanvas mUGUIRoot;							// 所有UI的根节点
	protected bool mUseAnchor;									// 是否启用锚点来自动调节窗口的大小和位置
	public LayoutManager()
	{
		mUseAnchor = true;
		mScriptMappingList = new Dictionary<Type, List<int>>();
		mScriptRegisteList = new Dictionary<int, Type>();
		mLayoutTypeToPath = new Dictionary<int, string>();
		mLayoutPathToType = new Dictionary<string, int>();
		mLayoutList = new SafeDictionary<int, GameLayout>();
		mLayoutAsyncList = new Dictionary<string, LayoutInfo>();
		mBackBlurLayoutList = new List<GameLayout>();
		mLayoutLoadCallback = onLayoutPrefabLoaded;
		// 在构造中获取UI根节点,确保其他组件能在任意时刻正常访问
		mUGUIRoot = LayoutScript.newUIObject<myUGUICanvas>(null, null, getGameObject(FrameDefine.UGUI_ROOT, true));
	}
	public new Canvas getUGUIRootComponent() { return mUGUIRoot.getCanvas(); }
	public myUIObject getUIRoot() { return mUGUIRoot; }
	public GameObject getRootObject() { return mUGUIRoot?.getObject(); }
	public void notifyLayoutRenderOrder(GameLayout layout)
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
				CMD_MAIN(out CmdLayoutManagerBackBlur cmd, false);
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
			CMD_MAIN(out CmdLayoutManagerBackBlur cmd, false);
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
	public GameLayout getGameLayout(int id)
	{
		mLayoutList.tryGetValue(id, out GameLayout layout);
		return layout;
	}
	public SafeDictionary<int, GameLayout> getLayoutList() { return mLayoutList; }
	public LayoutScript getScript(int id)
	{
		GameLayout layout = getGameLayout(id);
		return layout != null ? layout.getScript() : null;
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
			if (!mResourceManager.loadResourceAsync<GameObject>(fullPath, mLayoutLoadCallback))
			{
				logError("can not find layout : " + path);
			}
			return null;
		}
		else
		{
			return newLayout(info, mResourceManager.loadResource<GameObject>(fullPath));
		}
	}
	public void destroyLayout(int id)
	{
		GameLayout layout = getGameLayout(id);
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
		Type type = mScriptRegisteList[layout.getID()];
		LayoutScript script = createInstance<LayoutScript>(type);
		if (script == null)
		{
			logError("界面脚本未注册, ID:" + layout.getID());
			return null;
		}
		script.setType(type);
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
	public void registeLayout(Type classType, int type, string name)
	{
		mLayoutTypeToPath.Add(type, name);
		mLayoutPathToType.Add(name, type);
		mScriptRegisteList.Add(type, classType);
		if (!mScriptMappingList.TryGetValue(classType, out List<int> list))
		{
			list = new List<int>();
			mScriptMappingList.Add(classType, list);
		}
		list.Add(type);
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
	//-----------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		mCOMEscHide = addComponent(typeof(COMLayoutManagerEscHide)) as COMLayoutManagerEscHide;
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
		GameObject layoutParent = info.mIsScene ? null : getRootObject();
		instantiatePrefab(layoutParent, prefab, info.mName, true);
		GameLayout layout = new GameLayout();
		layout.setPrefab(prefab);
		layout.setID(info.mID);
		layout.setName(info.mName);
		layout.setIsScene(info.mIsScene);
		layout.setOrderType(info.mOrderType);
		layout.init(generateRenderOrder(layout, info.mRenderOrder, info.mOrderType));
		mLayoutList.add(info.mID, layout);
		return layout;
	}
}