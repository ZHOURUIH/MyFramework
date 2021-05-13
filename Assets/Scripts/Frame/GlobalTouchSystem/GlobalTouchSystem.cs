using UnityEngine;
using System.Collections.Generic;

public class GlobalTouchSystem : FrameSystem
{
	protected Dictionary<IMouseEventCollect, MultiTouchInfo> mMultiTouchWindowList;         // 当前已经被多点触控的窗口列表
	protected SafeDictionary<IMouseEventCollect, IMouseEventCollect> mPassOnlyArea;			// 点击穿透区域绑定列表,Key的区域中,只允许Value的区域穿透,Key无论是否设置了允许射线穿透,实际检测时都是不能够穿透的
	protected HashSet<IMouseEventCollect> mMouseDownWindowList; // 鼠标按下时所选中的所有窗口和物体
	protected HashSet<IMouseEventCollect> mParentPassOnlyList;	// 仅父节点区域可穿透的列表
	protected HashSet<IMouseEventCollect> mAllObjectSet;        // 所有参与鼠标或触摸事件的窗口和物体列表
	protected List<MouseCastWindowSet> mMouseCastWindowList;    // 所有窗口所对应的摄像机的列表,每个摄像机的窗口列表根据深度排序
	protected List<MouseCastObjectSet> mMouseCastObjectList;    // 所有场景中物体所对应的摄像机的列表,每个摄像机的物体列表根据深度排序
	protected IMouseEventCollect mHoverWindow;					// 鼠标当前悬停的窗口
	protected MyTimer mStayTimer;								// 鼠标停留的计时器
	protected Vector3 mLastMousePosition;						// 上一帧鼠标的位置
	protected Vector3 mCurTouchPosition;                        // 在模拟触摸屏的条件下,当前触摸点
	protected int mCurTouchID;									// 单点模式下,当前的触点ID
	protected bool mUseGlobalTouch;								// 是否使用全局触摸检测来进行界面的输入检测
	protected bool mSimulateTouch;								// 使用鼠标操作时是否模拟触摸屏
	protected bool mMousePressed;								// 在模拟触摸屏的条件下,屏幕是否被按下
	protected bool mUseHover;									// 是否判断鼠标悬停在某个窗口
	public GlobalTouchSystem()
	{
		mAllObjectSet = new HashSet<IMouseEventCollect>();
		mMouseCastWindowList = new List<MouseCastWindowSet>();
		mMouseCastObjectList = new List<MouseCastObjectSet>();
		mMouseDownWindowList = new HashSet<IMouseEventCollect>();
		mMultiTouchWindowList = new Dictionary<IMouseEventCollect, MultiTouchInfo>();
		mPassOnlyArea = new SafeDictionary<IMouseEventCollect, IMouseEventCollect>();
		mParentPassOnlyList = new HashSet<IMouseEventCollect>();
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
		LIST_MAIN(out List<IMouseEventCollect> resultList);
		globalRaycast(resultList, ref pos, ignorePassRay);
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
		UN_LIST_MAIN(resultList);
		return forwardButton;
	}
	// 越顶层的窗口越靠近列表前面
	public void getAllHoverWindow(List<IMouseEventCollect> hoverList, ref Vector3 pos, IMouseEventCollect ignoreWindow = null, bool ignorePassRay = false)
	{
		LIST_MAIN(out List<IMouseEventCollect> resultList);
		globalRaycast(resultList, ref pos, ignorePassRay);
		int count = resultList.Count;
		for(int i = 0; i < count; ++i)
		{
			IMouseEventCollect window = resultList[i];
			if (ignoreWindow != window)
			{
				hoverList.Add(window);
			}
		}
		UN_LIST_MAIN(resultList);
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
			// 多点触控时将单点触控ID清除
			mCurTouchID = -1;

			// 此处未考虑到触点离开窗口时以及再次进入窗口所引起的结束多点触控和开始多点触控的判断
			LIST_MAIN(out List<IMouseEventCollect>  endMultiWindow);
			LIST_MAIN(out Dictionary<int, Touch> tempMultiTouchPoint);
			LIST_MAIN(out Dictionary<IMouseEventCollect, List<Touch>> tempStartMultiWindow);
			LIST_MAIN(out Dictionary<IMouseEventCollect, List<Touch>> tempMovingMultiWindow);
			// 多点触控时所有拥有触点的窗口
			LIST_MAIN(out Dictionary<IMouseEventCollect, Dictionary<int, Touch>> touchInWindow);
			// 查找触点所在窗口
			for (int i = 0; i < touchCount; ++i)
			{
				Touch touch = Input.GetTouch(i);
				tempMultiTouchPoint.Add(touch.fingerId, touch);
				Vector3 touchPosition = new Vector3(touch.position.x, touch.position.y, 0.0f);
				IMouseEventCollect hoverWindow = getHoverWindow(ref touchPosition);
				if (hoverWindow != null)
				{
					if (!touchInWindow.TryGetValue(hoverWindow, out Dictionary<int, Touch> touchList))
					{
						LIST_MAIN(out touchList);
						touchInWindow.Add(hoverWindow, touchList);
					}
					// 每个窗口最多有两个触点
					if (touchList.Count < 2)
					{
						touchList.Add(touch.fingerId, touch);
					}
				}
			}
			// 判断哪些窗口上一帧还拥有两个触点,但是这一帧不足两个触点,则是
			foreach (var item in touchInWindow)
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
						LIST_MAIN(out List<Touch> list);
						list.AddRange(touchesInWindow.Values);
						tempStartMultiWindow.Add(item.Key, list);
					}
					else
					{
						LIST_MAIN(out List<Touch> list);
						list.AddRange(touchesInWindow.Values);
						tempMovingMultiWindow.Add(item.Key, list);
					}
				}
			}

			foreach(var item in touchInWindow)
			{
				UN_LIST_MAIN(item.Value);
			}
			UN_LIST_MAIN(touchInWindow);

			// 新增的多点触控窗口
			foreach (var item in tempStartMultiWindow)
			{
				if (!mMultiTouchWindowList.ContainsKey(item.Key))
				{
					CLASS_MAIN(out MultiTouchInfo info);
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

			foreach(var item in tempStartMultiWindow)
			{
				UN_LIST_MAIN(item.Value);
			}
			UN_LIST_MAIN(tempStartMultiWindow);

			// 更新已存在的多点触控窗口
			foreach (var item in mMultiTouchWindowList)
			{
				IMouseEventCollect window = item.Key;
				if (tempMovingMultiWindow.ContainsKey(window))
				{
					Touch touch0 = tempMultiTouchPoint[item.Value.mFinger0];
					Touch touch1 = tempMultiTouchPoint[item.Value.mFinger1];
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

			foreach (var item in tempMovingMultiWindow)
			{
				UN_LIST_MAIN(item.Value);
			}
			UN_LIST_MAIN(tempMovingMultiWindow);

			// 结束多点触控的窗口
			int endCount = endMultiWindow.Count;
			for(int i = 0; i < endCount; ++i)
			{
				IMouseEventCollect item = endMultiWindow[i];
				item.onMultiTouchEnd();
				if (mMultiTouchWindowList.TryGetValue(item, out MultiTouchInfo info))
				{
					UN_CLASS(info);
					mMultiTouchWindowList.Remove(item);
				}
			}
			UN_LIST_MAIN(endMultiWindow);
			UN_LIST_MAIN(tempMultiTouchPoint);
		}
		else
		{
			// 由于这一帧可能会清除触点ID,所以先备份一个ID
			int curTouchID = mCurTouchID;
			// 手指操作触摸屏
			if (touchCount == 1)
			{
				Touch touch = Input.GetTouch(0);
				mCurTouchID = touch.fingerId;
				curTouchID = mCurTouchID;
				if (touch.phase == TouchPhase.Began)
				{
					notifyGlobalPress(true, curTouchID);
				}
				else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
				{
					notifyGlobalPress(false, curTouchID);
					mCurTouchID = -1;
				}
			}
			// 鼠标点击屏幕
			else if (touchCount == 0)
			{
				if (mInputManager.getMouseCurrentDown(MOUSE_BUTTON.LEFT))
				{
					mCurTouchID = (int)MOUSE_BUTTON.LEFT;
					curTouchID = mCurTouchID;
					notifyGlobalPress(true, curTouchID);
				}
				else if (mInputManager.getMouseCurrentUp(MOUSE_BUTTON.LEFT))
				{
					curTouchID = (int)MOUSE_BUTTON.LEFT;
					notifyGlobalPress(false, curTouchID);
					mCurTouchID = -1;
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
				checkHoverWindow(ref curMousePosition, true, curTouchID);
				// 鼠标移动事件
				Vector3 moveDelta = curMousePosition - mLastMousePosition;
				mHoverWindow?.onMouseMove(curMousePosition, moveDelta, elapsedTime, curTouchID);
				// 给鼠标按下时选中的所有窗口发送鼠标移动的消息
				foreach(var item in mMouseDownWindowList)
				{
					if (item != mHoverWindow)
					{
						item.onMouseMove(curMousePosition, moveDelta, elapsedTime, curTouchID);
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
					mHoverWindow?.onMouseStay(curMousePosition, curTouchID);
					// 给鼠标按下时选中的所有窗口发送鼠标移动的消息
					foreach (var item in mMouseDownWindowList)
					{
						item.onMouseStay(curMousePosition, curTouchID);
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
			if (camera == null)
			{
				camera = mCameraManager.getUICamera();
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
		mPassOnlyArea.add(parent, passOnlyArea);
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
				if (!item.hasWindow(obj))
				{
					continue;
				}
				item.removeWindow(obj);
				if (item.isEmpty())
				{
					mMouseCastWindowList.Remove(item);
				}
				break;
			}
		}
		else if (obj is MovableObject)
		{
			int count = mMouseCastObjectList.Count;
			for(int i = 0; i < count; ++i)
			{
				MouseCastObjectSet item = mMouseCastObjectList[i];
				if (!item.hasObject(obj))
				{
					continue;
				}
				item.removeObject(obj);
				if (item.isEmpty())
				{
					mMouseCastObjectList.Remove(item);
				}
				break;
			}
		}
		mAllObjectSet.Remove(obj);
		mMouseDownWindowList.Remove(obj);
		mMultiTouchWindowList.Remove(obj);
		mParentPassOnlyList.Remove(obj);
		// key或者value中任意一个注销了,都要从列表中移除
		if(!mPassOnlyArea.remove(obj))
		{
			foreach(var item in mPassOnlyArea.startForeach())
			{
				if(item.Value == obj)
				{
					mPassOnlyArea.remove(item.Key);
				}
			}
		}
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
	protected void notifyGlobalPress(bool press, int touchID)
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
		checkHoverWindow(ref mousePosition, press, touchID);
		// 通知屏幕鼠标事件
		foreach (var item in mAllObjectSet)
		{
			if (!item.isReceiveScreenMouse())
			{
				continue;
			}
			if (press)
			{
				item.onScreenMouseDown(mousePosition, touchID);
			}
			else
			{
				item.onScreenMouseUp(mousePosition, touchID);
			}
		}
		LIST_MAIN(out List<IMouseEventCollect> raycast);
		globalRaycast(raycast, ref mousePosition);
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
				obj.onMouseDown(mousePosition, touchID);
			}
			else
			{
				obj.onMouseUp(mousePosition, touchID);
			}
		}
		// 保存鼠标按下时所选中的所有窗口,需要给这些窗口发送鼠标移动的消息
		// 如果鼠标放开,则只是清空列表
		mMouseDownWindowList.Clear();
		if (press)
		{
			int castCount = raycast.Count;
			for(int i = 0; i < castCount; ++i)
			{
				mMouseDownWindowList.Add(raycast[i]);
			}
		}
		UN_LIST_MAIN(raycast);
	}
	// 全局射线检测
	protected void globalRaycast(List<IMouseEventCollect> resultList, ref Vector3 mousePos, bool ignorePassRay = false)
	{
		bool continueRay = true;
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
			raycastLayout(ref ray, item.getWindowOrderList(), resultList, ref continueRay, false, ignorePassRay);
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
				raycastMovableObject(ref ray, item.mObjectOrderList, resultList, ref continueRay, false);
			}
		}
	}
	protected void raycastMovableObject(ref Ray ray, List<IMouseEventCollect> moveObjectList, List<IMouseEventCollect> retList, ref bool continueRay, bool clearList = true)
	{
		if(clearList)
		{
			retList.Clear();
		}
		continueRay = true;
		RaycastHit hit;
		LIST_MAIN(out List<DistanceSortHelper> sortList);
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
		UN_LIST_MAIN(sortList);
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
		LIST_MAIN(out HashSet<IMouseEventCollect> activeParentList);
		// 在只允许父节点穿透的列表中已成功穿透的父节点列表
		LIST_MAIN(out HashSet<IMouseEventCollect> passParent);

		// 筛选出已激活的父节点穿透窗口
		foreach(var item in mParentPassOnlyList)
		{
			if (item.isDestroy())
			{
				logError("窗口已经被销毁,无法访问:" + item.getName());
				continue;
			}
			if (item.isActiveInHierarchy())
			{
				activeParentList.Add(item);
			}
		}

		// 射线检测
		continueRay = true;
		int windowCount = windowOrderList.Count;
		for(int i = 0; i < windowCount; ++i)
		{
			IMouseEventCollect window = windowOrderList[i];
			if (window.isDestroy())
			{
				logError("窗口已经被销毁,无法访问:" + window.getName());
				continue;
			}
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
				if (mPassOnlyArea.tryGetValue(window, out IMouseEventCollect passOnlyArea))
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
		UN_LIST_MAIN(passParent);
		UN_LIST_MAIN(activeParentList);
	}
	// obj的所有父节点中是否允许射线选中obj
	// bindParentList是当前激活的已绑定的仅父节点区域穿透的列表
	// passedParentList是bindParentList中射线已经穿透的父节点
	protected bool isParentPassed(IMouseEventCollect obj, HashSet<IMouseEventCollect> bindParentList, HashSet<IMouseEventCollect> passedParentList)
	{
		foreach(var item in bindParentList)
		{
			// 有父节点,并且父节点未成功穿透时,则认为当前窗口未相交
			if (obj.isChildOf(item) && !passedParentList.Contains(item))
			{
				return false;
			}
		}
		return true;
	}
	protected void checkHoverWindow(ref Vector3 mousePos, bool mouseDown, int touchID)
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
					mHoverWindow.onMouseLeave(touchID);
				}
				// 找到鼠标所在的新的窗口,给该窗口发送鼠标进入的事件
				newWindow?.onMouseEnter(touchID);
			}
		}
		// 如果上一帧鼠标没有在任何窗口内,则计算这一帧鼠标所在的窗口
		else
		{
			// 发送鼠标进入的事件
			newWindow?.onMouseEnter(touchID);
		}
		mHoverWindow = newWindow;
	}
}
