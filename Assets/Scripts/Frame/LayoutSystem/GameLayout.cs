using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class GameLayout : GameBase
{
	protected Dictionary<GameObject, txUIObject> mGameObjectSearchList; // 用于根据GameObject查找UI
	protected Dictionary<int, txUIObject> mObjectList;					// 布局中UI物体列表,用于保存所有已获取的UI
	protected HashSet<string> mAtlasName;   // 布局专用的图集,卸载布局时将会卸载图集,建议每个布局的图集都在单独的AssetBundle中,否则会出现无法卸载或者卸载错误的情况
#if USE_NGUI
	protected txNGUIPanel mRoot;			// NGUI的Panel
#endif
	protected txUGUICanvas mRoot;			// UGUI的Canvas
	protected LayoutScript mScript;			// 布局脚本
	protected GameObject mPrefab;			// 布局预设,布局从该预设实例化
	protected LAYOUT mType;					// 布局类型
	protected string mName;					// 布局名称
	protected bool mScriptInited;			// 脚本是否已经初始化
	protected bool mScriptControlHide;		// 是否由脚本来控制隐藏
	protected bool mIsNGUI;					// 是否为NGUI布局,true为NGUI,false为UGUI
	protected bool mIsScene;				// 是否为场景,如果是场景,就不将布局挂在NGUIRoot或者UGUIRoot下
	protected bool mCheckBoxAnchor;			// 是否检查布局中所有带碰撞盒的窗口是否自适应分辨率
	protected bool mIgnoreTimeScale;        // 更新布局时是否忽略时间缩放
	protected bool mBlurBack;               // 布局显示时是否需要使布局背后(比当前布局层级低)的所有布局模糊显示
	protected bool mAnchorApplied;			// 是否已经完成了自适应的调整
	protected int mRenderOrder;             // 渲染顺序,越大则渲染优先级越高
	protected int mDefaultLayer;			// 布局加载时所处的层
	public static LayoutScriptCallback mLayoutScriptCallback;
	public GameLayout()
	{
		mObjectList = new Dictionary<int, txUIObject>();
		mGameObjectSearchList = new Dictionary<GameObject, txUIObject>();
		mAtlasName = new HashSet<string>();
		mCheckBoxAnchor = true;
		mRenderOrder = -1;
	}
	public void setPrefab(GameObject prefab) { mPrefab = prefab; }
	public void addAtlas(string atlas) { mAtlasName.Add(atlas); }
	public void setRenderOrder(int renderOrder)
	{
		mRenderOrder = renderOrder;
		mRoot.setDepth(mRenderOrder);
		// 刷新所有窗口注册的深度
		refreshObjectDepth();
	}
	public int getRenderOrder(){return mRenderOrder;}
	public void setBlurBack(bool blurBack) { mBlurBack = blurBack; }
	public bool isBlurBack() { return mBlurBack; }
	public int getDefaultLayer() { return mDefaultLayer; }
	public bool isAnchorApplied() { return mAnchorApplied; }
	public txUIObject getUIObject(GameObject go)
	{
		if(mGameObjectSearchList.ContainsKey(go))
		{
			return mGameObjectSearchList[go];
		}
		return null;
	}
	public void init(LAYOUT type, string name, int renderOrder, bool isNGUI, bool isScene)
	{	
		mName = name;
		mType = type;
		mIsNGUI = isNGUI;
		mIsScene = isScene;
		mScript = mLayoutManager.createScript(mName, this);
		mLayoutScriptCallback?.Invoke(mScript, true);
		if (mScript == null)
		{
			logError("can not create layout script! type : " + mType);
		}
		// 初始化布局脚本
		if(!mIsScene)
		{
			mScript.newObject(out mRoot, mLayoutManager.getUIRoot(mIsNGUI), mName);
		}
		else
		{
			mScript.newObject(out mRoot, null, mName);
		}
		mRoot?.setDestroyImmediately(true);
		mDefaultLayer = mRoot.getObject().layer;
		setRenderOrder(renderOrder);
		mScript.setRoot(mRoot);
		mScript.assignWindow();
		// 查找完窗口后设置所有窗口的深度
		if(!mIsNGUI)
		{
			setUIChildDepth(mRoot, 0, false);
		}
		// 布局实例化完成,初始化之前,需要调用自适应组件的更新
		if (mLayoutManager.isUseAnchor())
		{
			applyAnchor(mRoot.getObject(), true, this);
		}
		mAnchorApplied = true;
		mScript.init();
		mScriptInited = true;
		// 加载完布局后强制隐藏
		setVisibleForce(false);
	}
	public void update(float elapsedTime)
	{
		if(mIgnoreTimeScale)
		{
			elapsedTime = Time.unscaledDeltaTime;
		}
		if (isVisible() && mScript != null && mScriptInited)
		{
			Profiler.BeginSample("UpdateLayout:" + getName());
			// 先更新所有的UI物体
			foreach (var obj in mObjectList)
			{
				if (obj.Value.canUpdate())
				{
					Profiler.BeginSample("UpdateWindow:" + obj.Value.getName());
					obj.Value.update(elapsedTime);
					Profiler.EndSample();
				}
			}
			Profiler.EndSample();
			Profiler.BeginSample("UpdateScript:" + getName());
			mScript.update(elapsedTime);
			Profiler.EndSample();
		}
	}
	public void onDrawGizmos()
	{
		if (isVisible() && mScript != null && mScriptInited)
		{
			mScript.onDrawGizmos();
		}
	}
	public void lateUpdate(float elapsedTime)
	{
		if(isVisible() && mScript != null && mScriptInited)
		{
			mScript.lateUpdate(elapsedTime);
		}
	}
	public void destroy()
	{
		if (mScript != null)
		{
			mLayoutScriptCallback?.Invoke(mScript, false);
			mScript.destroy();
			mScript = null;
		}
		txUIObject.destroyWindow(mRoot, true);
		mRoot = null;
		mResourceManager.unload(ref mPrefab);
		foreach (var item in mAtlasName)
		{
			mResourceManager.unloadPath(getFilePath(item));
		}
		mAtlasName.Clear();
	}
	public List<Collider> getAllCollider()
	{
		var boxList = new List<Collider>();
		foreach(var obj in mObjectList)
		{
			Collider collider = obj.Value.getCollider();
			if(collider != null)
			{
				boxList.Add(collider);
			}
		}
		return boxList;
	}
	// 设置是否会立即隐藏,应该由布局脚本调用
	public void setScriptControlHide(bool control) { mScriptControlHide = control; }
	public void setVisible(bool visible, bool immediately, string param)
	{
		if (mScript == null || !mScriptInited)
		{
			return;
		}
		// 通知布局管理器布局显示或隐藏
		mLayoutManager.notifyLayoutVisible(visible, this);
		// 设置布局显示或者隐藏时需要先通知脚本开始显示或隐藏
		mScript.notifyStartShowOrHide();
		// 显示布局时立即显示
		if (visible)
		{
			mRoot.setActive(visible);
			mScript.onReset();
			mScript.onGameState();
			mScript.onShow(immediately, param);
		}
		// 隐藏布局时需要判断
		else
		{
			if (!mScriptControlHide)
			{
				mRoot.setActive(visible);
			}
			mScript.onHide(immediately, param);
		}
	}
	public void setVisibleForce(bool visible)
	{
		if (mScript == null || !mScriptInited)
		{
			return;
		}
		mLayoutManager.notifyLayoutVisible(visible, this);
		// 直接设置布局显示或隐藏
		mRoot.setActive(visible);
	}
	public bool isVisible() { return mRoot.isActive(); }
	public txUIObject getRoot() { return mRoot; }
	public LayoutScript getScript() { return mScript; }
	public LAYOUT getType() { return mType; }
	public string getName() { return mName; }
	public bool isNGUI() { return mIsNGUI; }
	public void setCheckBoxAnchor(bool check) { mCheckBoxAnchor = check; }
	public bool isCheckBoxAnchor() { return mCheckBoxAnchor; }
	public void setIgnoreTimeScale(bool ignore) { mIgnoreTimeScale = ignore; }
	public bool isIgnoreTimeScale() { return mIgnoreTimeScale; }
	public void registerUIObject(txUIObject uiObj)
	{
		mObjectList.Add(uiObj.getID(), uiObj);
		mGameObjectSearchList.Add(uiObj.getObject(), uiObj);
	}
	public void unregisterUIObject(txUIObject uiObj)
	{
		if(uiObj == null)
		{
			return;
		}
		mObjectList.Remove(uiObj.getID());
		mGameObjectSearchList.Remove(uiObj.getObject());
	}
	public void setLayer(string layer)
	{
		setGameObjectLayer(mRoot.getObject(), layer);
	}
	// 刷新布局中所有带碰撞盒的物体的深度,在布局panel深度改变时调用
	public void refreshObjectDepth()
	{
		foreach(var item in mObjectList)
		{
			mGlobalTouchSystem.notifyWindowDepthChanged(item.Value);
		}
	}
	// 有节点删除或者增加
	public void notifyObjectChanged()
	{
		setUIChildDepth(mRoot, 0, false);
	}
	// 节点在当前父节点中的位置有改变
	public void notifyObjectOrderChanged(txUIObject obj)
	{
		txUIObject parentObj = obj.getParent();
		if(parentObj != null)
		{
			setUIChildDepth(parentObj, parentObj.getDepth(), parentObj != mRoot);
		}
	}
	//------------------------------------------------------------------------------------------------------------
	// 返回值是最后一个窗口的深度值
	protected int setUIChildDepth(txUIObject ui, int uiDepth, bool includeSelf = true)
	{
		// NGUI不需要重新设置所有节点的深度
		if(mIsNGUI)
		{
			return 0;
		}
		// 先设置当前窗口的深度
		if(includeSelf)
		{
			ui.setDepth(uiDepth);
		}
		// 再设置子窗口的深度
		int endDepth = (int)ui.getDepth();
		Transform transform = ui.getTransform();
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; ++i)
		{
			txUIObject uiObj = getUIObject(transform.GetChild(i).gameObject);
			if (uiObj != null)
			{
				endDepth = setUIChildDepth(uiObj, endDepth + 1);
			}
		}
		return endDepth;
	}
}