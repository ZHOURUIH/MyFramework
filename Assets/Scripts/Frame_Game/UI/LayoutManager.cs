using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBase;
using static StringUtility;
using static CSharpUtility;
using static MathUtility;
using static FrameDefine;
using static FileUtility;
using static FrameEditorUtility;

// 布局管理器
public class LayoutManager : FrameSystem
{
	protected Dictionary<Type, LayoutRegisteInfo> mLayoutRegisteList = new();	// 布局注册信息列表
	protected SafeDictionary<Type, GameLayout> mLayoutList = new();				// 所有布局的列表
	protected Dictionary<string, LayoutInfo> mLayoutAsyncList = new();			// 正在异步加载的布局列表
	protected Dictionary<Type, string> mLayoutTypeToPath = new();				// 根据布局ID查找布局路径
	protected Dictionary<string, Type> mLayoutPathToType = new();				// 根据布局路径查找布局ID
	protected AssetLoadDoneCallback mLayoutLoadCallback;						// 保存回调变量,避免GC
	protected myUGUICanvas mUGUIRoot;											// 所有UI的根节点
	protected bool mUseAnchor = true;											// 是否启用锚点来自动调节窗口的大小和位置
	public LayoutManager()
	{
		mLayoutLoadCallback = onLayoutPrefabLoaded;
		// 在构造中获取UI根节点,确保其他组件能在任意时刻正常访问
		mUGUIRoot = LayoutScript.newUIObject<myUGUICanvas>(null, null, getGameObject(UGUI_ROOT, null, true));
	}
	public myUGUICanvas getUIRoot() { return mUGUIRoot; }
	public GameObject getRootObject() { return mUGUIRoot?.getObject(); }
	public void notifyLayoutVisible(bool visible, GameLayout layout)
	{
		if (!visible)
		{
			// 布局在隐藏时都需要确认设置层为UI层
			setGameObjectLayer(layout.getRoot().getObject(), layout.getDefaultLayer());
		}
	}
	public bool isUseAnchor() { return mUseAnchor; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		using var a = new SafeDictionaryReader<Type, GameLayout>(mLayoutList);
		foreach (GameLayout item in a.mReadList.Values)
		{
			try
			{
				using var b = new ProfilerScope(item.getName());
				item.update(elapsedTime);
			}
			catch (Exception e)
			{
				logException(e,"界面:" + item.getName());
			}
		}
	}
	public override void onDrawGizmos()
	{
		using var a = new SafeDictionaryReader<Type, GameLayout>(mLayoutList);
		foreach (GameLayout item in a.mReadList.Values)
		{
			item.onDrawGizmos();
		}
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		using var a = new SafeDictionaryReader<Type, GameLayout>(mLayoutList);
		foreach (GameLayout item in a.mReadList.Values)
		{
			try
			{
				item.lateUpdate(elapsedTime);
			}
			catch(Exception e)
			{
				logException(e, "layout:" + item.getName());
			}
		}
	}
	public override void willDestroy()
	{
		using var a = new SafeDictionaryReader<Type, GameLayout>(mLayoutList);
		foreach (GameLayout item in a.mReadList.Values)
		{
			item.destroy();
		}
		mLayoutList.clear();
		mLayoutTypeToPath.Clear();
		mLayoutPathToType.Clear();
		mLayoutAsyncList.Clear();
		// 销毁UI摄像机
		mCameraManager.destroyCamera(mCameraManager.getUICamera(), false);
		myUIObject.destroyWindowSingle(mUGUIRoot, false);
		mUGUIRoot = null;
		Resources.UnloadUnusedAssets();
		base.willDestroy();
	}
	public string getLayoutPathByType(Type type) { return mLayoutTypeToPath.get(type); }
	public GameLayout getLayout(Type type) { return mLayoutList.get(type); }
	// 根据顺序类型,计算实际的渲染顺序
	public int generateRenderOrder(GameLayout exceptLayout, int renderOrder, LAYOUT_ORDER orderType)
	{
		if (orderType == LAYOUT_ORDER.ALWAYS_TOP)
		{
			if (renderOrder < ALWAYS_TOP_ORDER)
			{
				renderOrder += ALWAYS_TOP_ORDER;
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
	public GameLayout createLayout(LayoutInfo info)
	{
		if (mLayoutList.tryGetValue(info.mType, out GameLayout existLayout))
		{
			return existLayout;
		}
		if (isWebGL())
		{
			logError("webgl无法同步加载界面");
			return null;
		}
		string path = getLayoutPathByType(info.mType);
		info.mName = getFileNameNoSuffixNoDir(path);
		string pathUnderResource = R_UI_PREFAB_PATH + path + ".prefab";
		if (mLayoutRegisteList.get(info.mType).mInResource)
		{
			return newLayout(info, mResourceManager.loadInResource<GameObject>(pathUnderResource));
		}
		else
		{
			return newLayout(info, mResourceManager.loadGameResource<GameObject>(pathUnderResource));
		}
	}
	public void createLayoutAsync(LayoutInfo info, GameLayoutCallback callback)
	{
		if (mLayoutList.tryGetValue(info.mType, out GameLayout existLayout))
		{
			callback?.Invoke(existLayout);
			return;
		}
		string path = getLayoutPathByType(info.mType);
		info.mName = getFileNameNoSuffixNoDir(path);
		string pathUnderResource = R_UI_PREFAB_PATH + path + ".prefab";
		mLayoutAsyncList.Add(info.mName, info);
		if (mLayoutRegisteList.get(info.mType).mInResource)
		{
			mResourceManager.loadInResourceAsync(pathUnderResource, (GameObject asset) =>
			{
				if (mLayoutAsyncList.Remove(asset.name, out LayoutInfo info))
				{
					callback?.Invoke(newLayout(info, asset));
				}
			});
		}
		else
		{
			mResourceManager.loadGameResourceAsync(pathUnderResource, (GameObject asset) =>
			{
				if (mLayoutAsyncList.Remove(asset.name, out LayoutInfo info))
				{
					callback?.Invoke(newLayout(info, asset));
				}
			});
		}
	}
	public void destroyLayout(Type type)
	{
		GameLayout layout = getLayout(type);
		if (layout == null)
		{
			return;
		}
		mLayoutList.remove(type);
		layout.destroy();
	}
	public LayoutScript createScript(GameLayout layout)
	{
		LayoutRegisteInfo info = mLayoutRegisteList.get(layout.getType());
		var script = createInstance<LayoutScript>(info.mScriptType);
		if (script == null)
		{
			logError("界面脚本未注册, Type:" + layout.getType());
			return null;
		}
		script.setLayout(layout);
		return script;
	}
	public void registeLayout(Type classType, string name, bool inResource, LAYOUT_LIFE_CYCLE lifeCycle, LayoutScriptCallback callback)
	{
		// 编辑器下检查文件是否存在
		if (isEditor() && !isFileExist(inResource ? (P_RESOURCES_UI_PREFAB_PATH + name + ".prefab") : (P_UI_PREFAB_PATH + name + ".prefab")))
		{
			logError("界面文件不存在:" + (inResource ? (P_RESOURCES_UI_PREFAB_PATH + name + ".prefab") : (P_UI_PREFAB_PATH + name + ".prefab")));
			return;
		}
		LayoutRegisteInfo info = new();
		info.mScriptType = classType;
		info.mInResource = inResource;
		info.mLifeCycle = lifeCycle;
		info.mCallback = callback;
		mLayoutTypeToPath.Add(classType, name);
		mLayoutPathToType.Add(name, classType);
		mLayoutRegisteList.Add(classType, info);
	}
	// 获取当前已经显示的布局中最上层布局的渲染深度,但是不包括始终在最上层的布局
	public int getTopLayoutOrder(GameLayout exceptLayout, bool alwaysTop)
	{
		int maxOrder = 0;
		foreach (GameLayout layout in mLayoutList.getMainList().Values)
		{
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
			maxOrder += ALWAYS_TOP_ORDER;
		}
		return maxOrder;
	}
	// 卸载所有非常驻的布局
	public void unloadAllPartLayout()
	{
		using var a = new SafeDictionaryReader<Type, GameLayout>(mLayoutList);
		foreach (Type type in a.mReadList.Keys)
		{
			if (mLayoutRegisteList.get(type).mLifeCycle == LAYOUT_LIFE_CYCLE.PART_USE)
			{
				LT.UNLOAD(type);
			}
		}
	}
	public void notifyLayoutChanged(GameLayout layout)
	{
		mLayoutRegisteList.get(layout.getType()).mCallback?.Invoke(layout.getScript());
	}
	// 方便调用的布局注册函数
	public static void registeLayoutResPart<T>(Action<T> callback = null) where T : LayoutScript
	{
		registeLayout<T>(true, LAYOUT_LIFE_CYCLE.PART_USE, (script) => { callback?.Invoke(script as T); });
	}
	public static void registeLayoutResAlways<T>(Action<T> callback = null) where T : LayoutScript
	{
		registeLayout<T>(true, LAYOUT_LIFE_CYCLE.PERSIST, (script) => { callback?.Invoke(script as T); });
	}
	public static void registeLayout<T>(bool inResource, LAYOUT_LIFE_CYCLE lifeCycle, LayoutScriptCallback callback = null) where T : LayoutScript
	{
		mLayoutManager.registeLayout(typeof(T), typeof(T).ToString(), inResource, lifeCycle, callback);
	}
	public static void registeLayout<T>(Action<T> callback) where T : LayoutScript
	{
		registeLayout(typeof(T).ToString(), false, LAYOUT_LIFE_CYCLE.PART_USE, callback);
	}
	public static void registeLayoutPersist<T>(Action<T> callback) where T : LayoutScript
	{
		registeLayout(typeof(T).ToString(), false, LAYOUT_LIFE_CYCLE.PERSIST, callback);
	}
	public static void registeLayout<T>(string name, bool inResource, LAYOUT_LIFE_CYCLE lifeCycle, Action<T> callback) where T : LayoutScript
	{
		mLayoutManager.registeLayout(typeof(T), name, inResource, lifeCycle, (script) => { callback?.Invoke(script as T); });
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onLayoutPrefabLoaded(UnityEngine.Object asset, UnityEngine.Object[] subAssets, byte[] bytes, string loadPath)
	{
		if (mLayoutAsyncList.Remove(asset.name, out LayoutInfo info))
		{
			info.mCallback?.Invoke(newLayout(info, asset as GameObject));
		}
	}
	protected GameLayout newLayout(LayoutInfo info, GameObject prefab)
	{
		myUIObject layoutParent = info.mIsScene ? null : getUIRoot();
		GameObject layoutObj = instantiatePrefab(layoutParent?.getObject(), prefab, info.mName, true);
		GameLayout layout = new();
		layout.setPrefab(prefab);
		layout.setType(info.mType);
		layout.setName(info.mName);
		layout.setParent(layoutParent);
		layout.setOrderType(info.mOrderType);
		layout.setRenderOrder(generateRenderOrder(layout, info.mRenderOrder, info.mOrderType));
		layout.setInResources(mLayoutRegisteList.get(info.mType).mInResource);
		layout.init();
		if (layout.getRoot().getObject() != layoutObj)
		{
			logError("布局的根节点不是实例化出来的节点,请确保运行前UI根节点下没有与布局同名的节点");
		}
		mLayoutList.add(info.mType, layout);
		return layout;
	}
}