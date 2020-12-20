using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class GlobalTouchSystem : FrameSystem
{
	// 用于避免GC而使用的变量
	private Dictionary<IMouseEventCollect, Dictionary<int, Touch>> mTouchInWindow;  // 多点触控时所有拥有触点的窗口
	private Dictionary<IMouseEventCollect, List<Touch>> mTempMovingMultiWindow;
	private Dictionary<IMouseEventCollect, List<Touch>> mTempStartMultiWindow;
	private Dictionary<int, Touch> mTempMultiTouchPoint;
	private List<IMouseEventCollect> mTempList2;
	private List<IMouseEventCollect> mTempList;
	//------------------------------------------------------------------------------------------------------------------------
	protected Dictionary<IMouseEventCollect, MultiTouchInfo> mMultiTouchWindowList; // 当前已经被多点触控的窗口列表
	protected Dictionary<IMouseEventCollect, IMouseEventCollect> mPassOnlyArea;     // 点击穿透区域绑定列表,Key的区域中,只允许Value的区域穿透,Key无论是否设置了允许射线穿透,实际检测时都是不能够穿透的
	protected HashSet<IMouseEventCollect> mAllObjectSet;        // 所有参与鼠标或触摸事件的窗口和物体列表
	protected List<MouseCastWindowSet> mMouseCastWindowList;    // 所有窗口所对应的摄像机的列表,每个摄像机的窗口列表根据深度排序
	protected List<MouseCastObjectSet> mMouseCastObjectList;    // 所有场景中物体所对应的摄像机的列表,每个摄像机的物体列表根据深度排序
	protected List<IMouseEventCollect> mMouseDownWindowList;    // 鼠标按下时所选中的所有窗口和物体
	protected List<IMouseEventCollect> mParentPassOnlyList;		// 仅父节点区域可穿透的列表
	protected List<IMouseEventCollect> mMovableObjectList;      // MovableObject的列表与UI的列表分开存储,因为UI的深度一般固定不变,而MovableObject的深度只在检测时根据相交点判断
	protected IMouseEventCollect mHoverWindow;					// 鼠标当前悬停的窗口
	protected MyTimer mStayTimer;			// 鼠标停留的计时器
	protected Vector3 mLastMousePosition;	// 上一帧鼠标的位置
	protected Vector3 mCurTouchPosition;	// 在模拟触摸屏的条件下,当前触摸点
	protected bool mUseGlobalTouch;         // 是否使用全局触摸检测来进行界面的输入检测
	protected bool mSimulateTouch;          // 使用鼠标操作时是否模拟触摸屏
	protected bool mMousePressed;           // 在模拟触摸屏的条件下,屏幕是否被按下
	protected bool mUseHover;               // 是否判断鼠标悬停在某个窗口
	protected int mNGUICount;				// 注册的NGUI窗口数量,当数量不为0时,会启用NGUI摄像机的检测NGUI窗口
	protected int mUGUICount;               // 注册的UGUI窗口数量,当数量不为0时,会启用UGUI摄像机的检测UGUI窗口
	public GlobalTouchSystem()
	{
		mAllObjectSet = new HashSet<IMouseEventCollect>();
		mMouseCastWindowList = new List<MouseCastWindowSet>();
		mMouseCastObjectList = new List<MouseCastObjectSet>();
		mMovableObjectList = new List<IMouseEventCollect>();
		mMouseDownWindowList = new List<IMouseEventCollect>();
		mMultiTouchWindowList = new Dictionary<IMouseEventCollect, MultiTouchInfo>();
		mPassOnlyArea = new Dictionary<IMouseEventCollect, IMouseEventCollect>();
		mTempList = new List<IMouseEventCollect>();
		mTempList2 = new List<IMouseEventCollect>();
		mTouchInWindow = new Dictionary<IMouseEventCollect, Dictionary<int, Touch>>();
		mTempMultiTouchPoint = new Dictionary<int, Touch>();
		mTempStartMultiWindow = new Dictionary<IMouseEventCollect, List<Touch>>();
		mTempMovingMultiWindow = new Dictionary<IMouseEventCollect, List<Touch>>();
		mParentPassOnlyList = new List<IMouseEventCollect>();
		mStayTimer = new MyTimer();
		mSimulateTouch = true;
		mUseHover = true;
		mUseGlobalTouch = true;
		Input.multiTouchEnabled = true;
	}
	public override void init()
	{
		base.init();
		mStayTimer.init(-1.0f, 0.05f, false);
	}
	public override void destroy()
	{
		mAllObjectSet.Clear();
		mMouseCastWindowList.Clear();
		mMouseCastObjectList.Clear();
		base.destroy();
	}
	public void setUseGlobalTouch(bool use) { mUseGlobalTouch = use; }
	public void setSimulateTouch(bool simulate)
	{
		mSimulateTouch = simulate;
		if (mSimulateTouch)
		{
			Input.multiTouchEnabled = true;
		}
	}
	public Vector3 getCurMousePosition() { return mSimulateTouch ? mCurTouchPosition : Input.mousePosition; }
	public IMouseEventCollect getHoverWindow() { return mHoverWindow; }
	public IMouseEventCollect getHoverWindow(ref Vector3 pos, IMouseEventCollect ignoreWindow = null, bool ignorePassRay = false)
	{
		// 返回最上层的窗口
		var resultList = globalRaycast(ref pos, ignorePassRay);
		IMouseEventCollect forwardButton = null;
		int count = resultList.Count;
		for(int i = 0; i < count; ++i)
		{
			IMouseEventCollect window = resultList[i];
			if (ignoreWindow != window)
			{
				forwardButton = window;
				break;
			}
		}
		return forwardButton;
	}
	// 越顶层的窗口越靠近列表前面
	public List<IMouseEventCollect> getAllHoverWindow(ref Vector3 pos, IMouseEventCollect ignoreWindow = null, bool ignorePassRay = false)
	{
		var resultList = globalRaycast(ref pos, ignorePassRay);
		mTempList.Clear();
		int count = resultList.Count;
		for(int i = 0; i < count; ++i)
		{
			IMouseEventCollect window = resultList[i];
			if (ignoreWindow != window)
			{
				mTempList.Add(window);
			}
		}
		return mTempList;
	}
	public override void update(float elapsedTime)
	{
		if (!mUseHover || !mUseGlobalTouch)
		{
			return;
		}
		int touchCount = Input.touchCount;
		// 多点触控和单点触控需要分开判断
		if (touchCount >= 2)
		{
			// 此处未考虑到触点离开窗口时以及再次进入窗口所引起的结束多点触控和开始多点触控的判断
			List<IMouseEventCollect> endMultiWindow = mListPool.newList(out endMultiWindow);
			mTempMultiTouchPoint.Clear();
			mTempStartMultiWindow.Clear();
			mTempMovingMultiWindow.Clear();
			mTouchInWindow.Clear();
			// 查找触点所在窗口
			for (int i = 0; i < touchCount; ++i)
			{
				Touch touch = Input.GetTouch(i);
				mTempMultiTouchPoint.Add(touch.fingerId, touch);
				Vector3 touchPosition = new Vector3(touch.position.x, touch.position.y, 0.0f);
				IMouseEventCollect hoverWindow = getHoverWindow(ref touchPosition);
				if (hoverWindow != null)
				{
					if (!mTouchInWindow.TryGetValue(hoverWindow, out Dictionary<int, Touch> touchList))
					{
						touchList = new Dictionary<int, Touch>();
						mTouchInWindow.Add(hoverWindow, touchList);
					}
					// 每个窗口最多有两个触点
					if (touchList.Count < 2)
					{
						touchList.Add(touch.fingerId, touch);
					}
				}
			}
			// 判断哪些窗口上一帧还拥有两个触点,但是这一帧不足两个触点,则是
			foreach (var item in mTouchInWindow)
			{
				var touchesInWindow = item.Value;
				if (touchesInWindow.Count == 2)
				{
					// 结束多点触控
					bool isEnd = false;
					bool isStart = false;
					foreach (var touches in touchesInWindow)
					{
						// 有触点结束了,则认为多点触控已经结束
						if (touches.Value.phase == TouchPhase.Ended || touches.Value.phase == TouchPhase.Canceled)
						{
							isEnd = true;
						}
						// 有触点刚开始,则认为开始多点触控
						else if (touches.Value.phase == TouchPhase.Began)
						{
							isStart = true;
						}
					}
					if (isEnd)
					{
						endMultiWindow.Add(item.Key);
					}
					else if (isStart)
					{
						mTempStartMultiWindow.Add(item.Key, new List<Touch>(touchesInWindow.Values));
					}
					else
					{
						mTempMovingMultiWindow.Add(item.Key, new List<Touch>(touchesInWindow.Values));
					}
				}
			}
			// 新增的多点触控窗口
			foreach (var item in mTempStartMultiWindow)
			{
				if (!mMultiTouchWindowList.ContainsKey(item.Key))
				{
					var info = mClassPool.newClass(Typeof<MultiTouchInfo>()) as MultiTouchInfo;
					info.mWindow = item.Key;
					info.mPhase = TouchPhase.Began;
					info.mFinger0 = item.Value[0].fingerId;
					info.mFinger1 = item.Value[1].fingerId;
					info.mStartPosition0 = item.Value[0].position;
					info.mStartPosition1 = item.Value[1].position;
					info.mCurPosition0 = info.mStartPosition0;
					info.mCurPosition1 = info.mStartPosition1;
					mMultiTouchWindowList.Add(item.Key, info);
				}
				item.Key.onMultiTouchStart(item.Value[0].position, item.Value[1].position);
			}
			// 更新已存在的多点触控窗口
			foreach (var item in mMultiTouchWindowList)
			{
				IMouseEventCollect window = item.Key;
				if (mTempMovingMultiWindow.ContainsKey(window))
				{
					Touch touch0 = mTempMultiTouchPoint[item.Value.mFinger0];
					Touch touch1 = mTempMultiTouchPoint[item.Value.mFinger1];
					if (!isVectorEqual(touch0.position, item.Value.mCurPosition0) || !isVectorEqual(touch1.position, item.Value.mCurPosition1))
					{
						window.onMultiTouchMove(touch0.position, item.Value.mCurPosition0, touch1.position, item.Value.mCurPosition1);
						item.Value.mCurPosition0 = touch0.position;
						item.Value.mCurPosition1 = touch1.position;
						item.Value.mPhase = TouchPhase.Moved;
					}
					else
					{
						item.Value.mPhase = TouchPhase.Stationary;
					}
				}
			}
			// 结束多点触控的窗口
			int endCount = endMultiWindow.Count;
			for(int i = 0; i < endCount; ++i)
			{
				IMouseEventCollect item = endMultiWindow[i];
				item.onMultiTouchEnd();
				if (mMultiTouchWindowList.TryGetValue(item, out MultiTouchInfo info))
				{
					mClassPool.destroyClass(info);
					mMultiTouchWindowList.Remove(item);
				}
			}
			mListPool.destroyList(endMultiWindow);
		}
		else
		{
			// 手指操作触摸屏
			if (touchCount == 1)
			{
				Touch touch = Input.GetTouch(0);
				if (touch.phase == TouchPhase.Began)
				{
					notifyGlobalPress(true);
				}
				else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
				{
					notifyGlobalPress(false);
				}
			}
			// 鼠标点击屏幕
			else if (touchCount == 0)
			{
				if (mInputManager.getMouseCurrentDown(MOUSE_BUTTON.LEFT))
				{
					notifyGlobalPress(true);
				}
				else if (mInputManager.getMouseCurrentUp(MOUSE_BUTTON.LEFT))
				{
					notifyGlobalPress(false);
				}
			}
			// 模拟触摸时,更新触摸点
			if (mSimulateTouch && mMousePressed)
			{
				mCurTouchPosition = Input.mousePosition;
			}
			// 鼠标移动检测
			Vector3 curMousePosition = getCurMousePosition();
			if (!isVectorEqual(ref mLastMousePosition, ref curMousePosition))
			{
				// 检查当前悬停窗口
				checkHoverWindow(ref curMousePosition, true);
				// 鼠标移动事件
				Vector3 moveDelta = curMousePosition - mLastMousePosition;
				mHoverWindow?.onMouseMove(ref curMousePosition, ref moveDelta, elapsedTime);
				// 给鼠标按下时选中的所有窗口发送鼠标移动的消息
				int count = mMouseDownWindowList.Count;
				for(int i = 0; i < count; ++i)
				{
					IMouseEventCollect item = mMouseDownWindowList[i];
					if (item != mHoverWindow)
					{
						item.onMouseMove(ref curMousePosition, ref moveDelta, elapsedTime);
					}
				}
				mLastMousePosition = curMousePosition;
				// 如果在一个窗口上停留超过0.05秒没有移动,则触发停留在窗口上的事件
				mStayTimer.start();
			}
			else
			{
				if (mStayTimer.tickTimer(elapsedTime))
				{
					mHoverWindow?.onMouseStay(curMousePosition);
					// 给鼠标按下时选中的所有窗口发送鼠标移动的消息
					int count = mMouseDownWindowList.Count;
					for(int i = 0; i < count; ++i)
					{
						mMouseDownWindowList[i].onMouseStay(curMousePosition);
					}
				}
			}
		}
		// 检查摄像机是否被销毁
		int windowCount = mMouseCastWindowList.Count;
		for(int i = 0; i < windowCount; ++i)
		{
			MouseCastWindowSet item = mMouseCastWindowList[i];
			if (item.getCamera() != null && item.getCamera().isDestroy())
			{
				logError("摄像机已销毁:" + item.getCamera().getName());
			}
		}
		int objCount = mMouseCastObjectList.Count;
		for(int i = 0; i < objCount; ++i)
		{
			MouseCastObjectSet item = mMouseCastObjectList[i];
			if (item.mCamera != null && item.mCamera.isDestroy())
			{
				logError("摄像机已销毁:" + item.mCamera.getName());
			}
		}
	}
	public bool isColliderRegisted(IMouseEventCollect obj) { return mAllObjectSet.Contains(obj); }
	// 注册碰撞器,只有注册了的碰撞器才会进行检测
	public void registeCollider(IMouseEventCollect obj, GameCamera camera = null)
	{
		if(!mUseGlobalTouch)
		{
			logError("Not Active Global Touch!");
			return;
		}
		if (obj.getCollider() == null)
		{
			logError("window must has collider that can registeCollider! " + obj.getName());
			return;
		}
		if (mAllObjectSet.Contains(obj))
		{
			logError("不能重复注册窗口碰撞体: " + obj.getName());
			return;
		}
		if (obj is myUIObject)
		{
			// 寻找窗口对应的摄像机
			GUI_TYPE guiType = WidgetUtility.getGUIType(obj.getObject());
			if (guiType == GUI_TYPE.NGUI)
			{
				++mNGUICount;
				if (camera == null)
				{
					camera = mCameraManager.getUICamera(GUI_TYPE.NGUI);
				}
			}
			else if(guiType == GUI_TYPE.UGUI)
			{
				++mUGUICount;
				if (camera == null)
				{
					camera = mCameraManager.getUICamera(GUI_TYPE.UGUI);
				}
			}
			if (camera == null)
			{
				logError("can not find ui camera for raycast!");
			}
			// 将窗口加入到鼠标射线检测列表中
			MouseCastWindowSet mouseCastSet = null;
			int count = mMouseCastWindowList.Count;
			for(int i = 0; i < count; ++i)
			{
				MouseCastWindowSet item = mMouseCastWindowList[i];
				if (item.getCamera() == camera)
				{
					mouseCastSet = item;
					break;
				}
			}
			if (mouseCastSet == null)
			{
				mouseCastSet = new MouseCastWindowSet(camera);
				mMouseCastWindowList.Add(mouseCastSet);
			}
			mouseCastSet.addWindow(obj);
		}
		else if (obj is MovableObject)
		{
			MouseCastObjectSet mouseCastSet = null;
			int count = mMouseCastObjectList.Count;
			for(int i = 0; i < count; ++i)
			{
				MouseCastObjectSet item = mMouseCastObjectList[i];
				if (item.mCamera == camera)
				{
					mouseCastSet = item;
					break;
				}
			}
			if (mouseCastSet == null)
			{
				mouseCastSet = new MouseCastObjectSet(camera);
				mMouseCastObjectList.Add(mouseCastSet);
			}
			mouseCastSet.addObject(obj);
			mMovableObjectList.Add(obj);
		}
		mAllObjectSet.Add(obj);
	}
	// parent的区域中只有passOnlyArea的区域可以穿透
	public void bindPassOnlyArea(IMouseEventCollect parent, IMouseEventCollect passOnlyArea)
	{
		if (!mAllObjectSet.Contains(parent) || !mAllObjectSet.Contains(passOnlyArea))
		{
			logError("需要先注册窗口,才能绑定穿透区域");
			return;
		}
		mPassOnlyArea.Add(parent, passOnlyArea);
	}
	// parent的区域中才能允许parent的子节点接收射线检测
	public void bindPassOnlyParent(IMouseEventCollect parent)
	{
		if (!mAllObjectSet.Contains(parent))
		{
			logError("需要先注册窗口,才能绑定父节点穿透区域");
			return;
		}
		mParentPassOnlyList.Add(parent);
	}
	// 注销碰撞器
	public void unregisteCollider(IMouseEventCollect obj)
	{
		if (!mAllObjectSet.Contains(obj))
		{
			return;
		}
		if (mHoverWindow == obj)
		{
			mHoverWindow = null;
		}
		if (obj is myUIObject)
		{
			int count = mMouseCastWindowList.Count;
			for(int i = 0; i < count; ++i)
			{
				MouseCastWindowSet item = mMouseCastWindowList[i];
				if (item.hasWindow(obj))
				{
					item.removeWindow(obj);
					if (item.isEmpty())
					{
						mMouseCastWindowList.Remove(item);
					}
					break;
				}
			}
			GUI_TYPE guiType = WidgetUtility.getGUIType(obj.getObject());
			if (guiType == GUI_TYPE.NGUI)
			{
				--mNGUICount;
			}
			else if(guiType == GUI_TYPE.UGUI)
			{
				--mUGUICount;
			}
		}
		else if (obj is MovableObject)
		{
			int count = mMouseCastObjectList.Count;
			for(int i = 0; i < count; ++i)
			{
				MouseCastObjectSet item = mMouseCastObjectList[i];
				if (item.hasObject(obj))
				{
					item.removeObject(obj);
					if(item.isEmpty())
					{
						mMouseCastObjectList.Remove(item);
					}
					break;
				}
			}
			mMovableObjectList.Remove(obj);
		}
		mAllObjectSet.Remove(obj);
		mMouseDownWindowList.Remove(obj);
		mMultiTouchWindowList.Remove(obj);
	}
	public void notifyWindowDepthChanged(IMouseEventCollect button)
	{
		// 只判断UI的深度变化
		if (!(button is myUIObject))
		{
			return;
		}
		// 如果之前没有记录过,则不做判断
		if (!mAllObjectSet.Contains(button))
		{
			return;
		}
		int count = mMouseCastWindowList.Count;
		for (int i = 0; i < count; ++i)
		{
			MouseCastWindowSet item = mMouseCastWindowList[i];
			if (item.hasWindow(button))
			{
				item.windowDepthChanged();
				break;
			}
		}
	}
	//--------------------------------------------------------------------------------------------------------------------------
	protected void notifyGlobalPress(bool press)
	{
		// 开始触摸时记录触摸状态,同步上一次的触摸位置
		if (mSimulateTouch)
		{
			mMousePressed = press;
			if (mMousePressed)
			{
				mCurTouchPosition = Input.mousePosition;
				mLastMousePosition = mCurTouchPosition;
			}
		}
		Vector3 mousePosition = getCurMousePosition();
		// 检查当前悬停窗口
		checkHoverWindow(ref mousePosition, press);
		// 通知屏幕鼠标事件
		foreach (var item in mAllObjectSet)
		{
			if (!item.isReceiveScreenMouse())
			{
				continue;
			}
			if (press)
			{
				item.onScreenMouseDown(mousePosition);
			}
			else
			{
				item.onScreenMouseUp(mousePosition);
			}
		}
		var raycast = globalRaycast(ref mousePosition);
		int count = raycast.Count;
		for(int i = 0; i < count; ++i)
		{
			IMouseEventCollect obj = raycast[i];
			// 如果此时窗口已经被销毁了,则不再通知
			if(!mAllObjectSet.Contains(obj))
			{
				continue;
			}
			if (press)
			{
				obj.onMouseDown(mousePosition);
			}
			else
			{
				obj.onMouseUp(mousePosition);
			}
		}
		// 保存鼠标按下时所选中的所有窗口,需要给这些窗口发送鼠标移动的消息
		// 如果鼠标放开,则只是清空列表
		mMouseDownWindowList.Clear();
		if (press)
		{
			mMouseDownWindowList.AddRange(raycast);
		}
	}
	// 全局射线检测
	protected List<IMouseEventCollect> globalRaycast(ref Vector3 mousePos, bool ignorePassRay = false)
	{
		bool continueRay = true;
		mTempList2.Clear();
		// 每次检测UI时都需要对列表按摄像机深度进行降序排序
		quickSort(mMouseCastWindowList, MouseCastWindowSet.mComparisonDescend);
		int windowSetCount = mMouseCastWindowList.Count;
		for(int i = 0; i < windowSetCount; ++i)
		{
			var item = mMouseCastWindowList[i];
			if (!continueRay)
			{
				break;
			}
			// 检查摄像机是否被销毁
			if(item.getCamera() == null || item.getCamera().isDestroy())
			{
				logError("摄像机已销毁:" + item.getCamera().getName());
				continue;
			}
			getCameraRay(ref mousePos, out Ray ray, item.getCamera().getCamera());
			raycastLayout(ref ray, item.getWindowOrderList(), mTempList2, ref continueRay, false, ignorePassRay);
		}
		// UI层允许当前鼠标射线穿过时才检测场景物体
		if (continueRay)
		{
			quickSort(mMouseCastObjectList, MouseCastObjectSet.mCompareDescend);
			int objSetCount = mMouseCastObjectList.Count;
			for(int i = 0; i < objSetCount; ++i)
			{
				MouseCastObjectSet item = mMouseCastObjectList[i];
				if (!continueRay)
				{
					break;
				}
				// 检查摄像机是否被销毁
				if (item.mCamera != null && item.mCamera.isDestroy())
				{
					logError("摄像机已销毁:" + item.mCamera.getName());
					continue;
				}
				Camera camera;
				if(item.mCamera == null)
				{
					GameCamera mainCamera = mCameraManager.getMainCamera();
					if (mainCamera == null)
					{
						logError("找不到主摄像机,无法检测摄像机射线碰撞");
						continue;
					}
					camera = mainCamera.getCamera();
				}
				else
				{
					camera = item.mCamera.getCamera();
				}
				getCameraRay(ref mousePos, out Ray ray, camera);
				raycastMovableObject(ref ray, item.mObjectOrderList, mTempList2, ref continueRay, false);
			}
		}
		return mTempList2;
	}
	protected void raycastMovableObject(ref Ray ray, List<IMouseEventCollect> moveObjectList, List<IMouseEventCollect> retList, ref bool continueRay, bool clearList = true)
	{
		if(clearList)
		{
			retList.Clear();
		}
		continueRay = true;
		RaycastHit hit;
		List<DistanceSortHelper> sortList = mListPool.newList(out sortList);
		int objCount = moveObjectList.Count;
		for(int i = 0; i < objCount; ++i)
		{
			IMouseEventCollect box = moveObjectList[i];
			// 将所有射线碰到的物体都放到列表中
			if (box.isActive() && box.isHandleInput() && box.getCollider().Raycast(ray, out hit, 10000.0f))
			{
				sortList.Add(new DistanceSortHelper(getSquaredLength(hit.point - ray.origin), box));
			}
		}
		// 根据相交点由近到远的顺序排序
		quickSort(sortList, DistanceSortHelper.mCompareAscend);
		int sortCount = sortList.Count;
		for(int i = 0; i < sortCount; ++i)
		{
			DistanceSortHelper item = sortList[i];
			retList.Add(item.mObject);
			if (!item.mObject.isPassRay())
			{
				continueRay = false;
				break;
			}
		}
		mListPool.destroyList(sortList);
	}
	// ignorePassRay表示是否忽略窗口的isPassRay属性,true表示认为所有的都允许射线穿透
	// 但是ignorePassRay不会影响到PassOnlyArea和ParentPassOnly
	protected void raycastLayout(ref Ray ray, 
								 List<IMouseEventCollect> windowOrderList, 
								 List<IMouseEventCollect> retList, 
								 ref bool continueRay, 
								 bool clearList = true, 
								 bool ignorePassRay = false)
	{
		if(clearList)
		{
			retList.Clear();
		}
		// mParentPassOnlyList需要重新整理,排除未启用的布局的窗口
		List<IMouseEventCollect> activeParentList = mListPool.newList(out activeParentList);
		// 在只允许父节点穿透的列表中已成功穿透的父节点列表
		List<IMouseEventCollect> passParent = mListPool.newList(out passParent);

		// 筛选出已激活的父节点穿透窗口
		int allParentCount = mParentPassOnlyList.Count;
		for(int i = 0; i < allParentCount; ++i)
		{
			if (mParentPassOnlyList[i].isActiveInHierarchy())
			{
				activeParentList.Add(mParentPassOnlyList[i]);
			}
		}

		// 射线检测
		continueRay = true;
		int windowCount = windowOrderList.Count;
		for(int i = 0; i < windowCount; ++i)
		{
			IMouseEventCollect window = windowOrderList[i];
			if (window.isActiveInHierarchy() && window.isHandleInput() && window.getCollider().Raycast(ray, out _, 10000.0f))
			{
				// 点击到了只允许父节点穿透的窗口,记录到列表中
				// 但是因为父节点一定是在子节点之后判断的,子节点可能已经拦截了射线,从而导致无法检测到父节点
				if (activeParentList.Contains(window))
				{
					passParent.Add(window);
					// 特殊窗口暂时不能接收输入事件,所以不放入相交窗口列表中
					continue;
				}

				// 点击了只允许部分穿透的背景
				if (mPassOnlyArea.TryGetValue(window, out IMouseEventCollect passOnlyArea))
				{
					//判断是否点到了背景中允许穿透的部分,如果是允许穿透的部分,则射线可以继续判断下层的窗口，否则不允许再继续穿透
					continueRay = passOnlyArea.isActiveInHierarchy() && passOnlyArea.isHandleInput() && passOnlyArea.getCollider().Raycast(ray, out _, 10000.0f);
					if (!continueRay)
					{
						break;
					}
					// 特殊窗口暂时不能接收输入事件,所以不放入相交窗口列表中
					continue;
				}

				// 如果父节点不允许穿透
				if (!isParentPassed(window, activeParentList, passParent))
				{
					continue;
				}

				// 射线成功与窗口相交,放入列表
				retList.Add(window);
				// 如果射线不能穿透当前按钮,则不再继续
				continueRay = ignorePassRay || window.isPassRay();
			}
			if (!continueRay)
			{
				break;
			}
		}
		mListPool.destroyList(passParent);
		mListPool.destroyList(activeParentList);
	}
	// obj的所有父节点中是否允许射线选中obj
	// bindParentList是当前激活的已绑定的仅父节点区域穿透的列表
	// passedParentList是bindParentList中射线已经穿透的父节点
	protected bool isParentPassed(IMouseEventCollect obj, List<IMouseEventCollect> bindParentList, List<IMouseEventCollect> passedParentList)
	{
		int parentCount = bindParentList.Count;
		for(int i = 0; i < parentCount; ++i)
		{
			// 有父节点,并且父节点未成功穿透时,则认为当前窗口未相交
			if (obj.isChildOf(bindParentList[i]) && !passedParentList.Contains(bindParentList[i]))
			{
				return false;
			}
		}
		return true;
	}
	protected void checkHoverWindow(ref Vector3 mousePos, bool mouseDown)
	{
		IMouseEventCollect newWindow = null;
		// 模拟触摸状态下,如果鼠标未按下,则不会悬停在任何窗口上
		if (mouseDown || !mSimulateTouch)
		{
			// 计算鼠标当前所在最前端的窗口
			newWindow = getHoverWindow(ref mousePos);
		}
		// 判断鼠标是否还在当前窗口内
		if (mHoverWindow != null)
		{
			// 鼠标已经移动到了其他窗口中,发送鼠标离开的事件
			if (newWindow != mHoverWindow)
			{
				// 不过也许此时悬停窗口已经不接收输入事件了或者碰撞盒子被禁用了,需要判断一下
				if (mHoverWindow.isActive() && mHoverWindow.isHandleInput())
				{
					mHoverWindow.onMouseLeave();
				}
				// 找到鼠标所在的新的窗口,给该窗口发送鼠标进入的事件
				newWindow?.onMouseEnter();
			}
		}
		// 如果上一帧鼠标没有在任何窗口内,则计算这一帧鼠标所在的窗口
		else
		{
			// 发送鼠标进入的事件
			newWindow?.onMouseEnter();
		}
		mHoverWindow = newWindow;
	}
}
