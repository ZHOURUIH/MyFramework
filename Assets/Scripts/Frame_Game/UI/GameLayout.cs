using System;
using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;
using static WidgetUtility;
using static FrameBase;
using static FrameBaseDefine;

// 用于表示一个布局
public class GameLayout
{
	protected Canvas mCanvas;					// 布局Canvas
	protected Transform mTransform;				// 布局根节点
	protected GameObject mPrefab;               // 布局预设,布局从该预设实例化
	protected Type mType;						// 布局的脚本类型
	protected int mRenderOrder;					// 渲染顺序,越大则渲染优先级越高,不能小于0
	protected bool mAnchorApplied;              // 是否已经完成了自适应的调整
	public virtual void assignWindow() { }
	public virtual void update(float elapsedTime) { }
	// 重置布局状态后,再根据当前游戏状态设置布局显示前的状态
	public virtual void onGameState() { }
	public virtual void onHide() { }
	public void close()
	{
		setVisible(false);
	}
	public void getUIComponent<T>(out T com, string name) where T : Component
	{
		com = getGameObject(name, mCanvas.gameObject).GetComponent<T>();
	}
	public static void getUIComponent<T>(out T com, Component parent, string name) where T : Component
	{
		com = getGameObject(name, parent.gameObject).GetComponent<T>();
	}
	public virtual void init() { }
	public void initLayout()
	{
		mLayoutManager.notifyLayoutChanged(this, true);

		// 初始化布局脚本
		mCanvas = getGameObject(mType.ToString(), mLayoutManager.getUIRoot().gameObject).GetComponent<Canvas>();
		
		// 去除自带的锚点
		// 在unity2020中,不知道为什么实例化以后的RectTransform的大小会自动变为视图窗口大小,为了适配计算正确,这里需要重置一次
		RectTransform rectTransform = mCanvas.GetComponent<RectTransform>();
		rectTransform.anchorMin = Vector2.one * 0.5f;
		rectTransform.anchorMax = Vector2.one * 0.5f;
		setRectSize(rectTransform, new(STANDARD_WIDTH, STANDARD_HEIGHT));

		mTransform = mCanvas.gameObject.transform;
		assignWindow();
		// 布局实例化完成,初始化之前,需要调用自适应组件的更新
		applyAnchor(mCanvas.gameObject, true, this);
		mAnchorApplied = true;
		init();
		// init后再次设置布局的渲染顺序,这样可以在此处刷新所有窗口的深度,因为是否刷新跟是否注册了碰撞体有关
		// 所以在assignWindow和init中不需要在创建窗口对象时刷新深度,这样会造成很大的性能浪费
		setRenderOrder(mRenderOrder);
		// 加载完布局后强制隐藏
		mCanvas.gameObject.SetActive(false);
	}
	public void updateLayout(float elapsedTime)
	{
		if (!isVisible())
		{
			return;
		}
		// 更新脚本逻辑
		update(elapsedTime);
	}
	public void destroy()
	{
		mLayoutManager.notifyLayoutChanged(this, false);
		destroyUnityObject(mCanvas.gameObject);
		mCanvas = null;
		mTransform = null;
		mResourceManager.unloadInResources(ref mPrefab);
	}
	public void setRenderOrder(int renderOrder)
	{
		mRenderOrder = renderOrder;
		if (mRenderOrder < 0)
		{
			logErrorBase("布局深度不能小于0,否则无法正确计算窗口深度");
			return;
		}
		if (mCanvas == null)
		{
			return;
		}
		mCanvas.sortingOrder = mRenderOrder;
	}
	public void setVisible(bool visible)
	{
		if (mTransform == null)
		{
			return;
		}
		if (visible == isVisible())
		{
			return;
		}
		// 显示布局时立即显示
		if (visible)
		{
			mCanvas.gameObject.SetActive(visible);
			onGameState();
		}
		// 隐藏布局时需要判断
		else
		{
			mCanvas.gameObject.SetActive(visible);
			onHide();
		}
	}
	public Canvas getRoot()									{ return mCanvas; }
	public string getName()									{ return mType.ToString(); }
	public Type getType()									{ return mType; }
	public bool isVisible()									{ return mCanvas != null && mCanvas.gameObject.activeInHierarchy; }
	public void setPrefab(GameObject prefab)				{ mPrefab = prefab; }
	public void setType(Type type)							{ mType = type; }
}