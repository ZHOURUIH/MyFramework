using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class GlobalTouchSystem : FrameComponent
{
	// 用于避免GC而使用的变量
	private List<IMouseEventCollect> mTempList;
	private List<IMouseEventCollect> mTempList2;
	private Dictionary<int, Touch> mTempMultiTouchPoint;
	private Dictionary<IMouseEventCollect, List<Touch>> mTempStartMultiWindow;
	private Dictionary<IMouseEventCollect, List<Touch>> mTempMovingMultiWindow;
	private Dictionary<IMouseEventCollect, Dictionary<int, Touch>> mTouchInWindow;	// 多点触控时所有拥有触点的窗口
	//------------------------------------------------------------------------------------------------------------------------
	protected Dictionary<IMouseEventCollect, MultiTouchInfo> mMultiTouchWindowList;	// 当前已经被多点触控的窗口列表
	protected Dictionary<IMouseEventCollect, UIDepth> mButtonDepthList;             // 窗口的深度查找列表,用于保存窗口的当前深度
	protected Dictionary<IMouseEventCollect, IMouseEventCollect> mPassOnlyArea;     // 点击穿透区域绑定列表,当点击到Key的区域时,只允许Value的区域穿透
	protected List<MouseCastWindowSet> mMouseCastWindowList;    // 所有窗口所对应的摄像机的列表,每个摄像机的窗口列表根据深度排序
	protected List<MouseCastObjectSet> mMouseCastObjectList;    // 所有场景中物体所对应的摄像机的列表,每个摄像机的物体列表根据深度排序
	protected List<IMouseEventCollect> mAllButtonList;          // 所有参与鼠标或触摸事件的窗口和物体列表
	protected HashSet<IMouseEventCollect> mAllButtonSet;        // 辅助mAllButtonList查找的列表
	protected List<IMouseEventCollect> mMovableObjectList;		// MovableObject的列表与UI的列表分开存储,因为UI的深度一般固定不变,而MovableObject的深度只在检测时根据相交点判断
	protected List<IMouseEventCollect> mMouseDownWindowList;	// 鼠标按下时所选中的所有窗口和物体
	protected IMouseEventCollect mHoverWindow;
	protected CustomTimer mStayTimer;
	protected Vector3 mLastMousePosition;
	protected Vector3 mCurTouchPosition;	// 在模拟触摸屏的条件下,当前触摸点
	protected bool mSimulateTouch;			// 使用鼠标操作时是否模拟触摸屏
	protected bool mUseHover;				// 是否判断鼠标悬停在某个窗口
	protected bool mUseGlobalTouch;			// 是否使用全局触摸检测来进行界面的输入检测
	protected bool mMousePressed;			// 在模拟触摸屏的条件下,屏幕是否被按下
	protected int mNGUICount;				// 注册的NGUI窗口数量,当数量不为0时,会启用NGUI摄像机的检测NGUI窗口
	protected int mUGUICount;				// 注册的UGUI窗口数量,当数量不为0时,会启用UGUI摄像机的检测UGUI窗口
	public GlobalTouchSystem(string name)
		:base(name)
	{
		mAllButtonList = new List<IMouseEventCollect>();
		mAllButtonSet = new HashSet<IMouseEventCollect>();
		mButtonDepthList = new Dictionary<IMouseEventCollect, UIDepth>();
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
		mStayTimer = new CustomTimer();
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
		mAllButtonList.Clear();
		mAllButtonSet.Clear();
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
		foreach (var button in resultList)
		{
			if (ignoreWindow != button)
			{
				forwardButton = button;
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
		foreach (var button in resultList)
		{
			if (ignoreWindow != button)
			{
				mTempList.Add(button);
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
					if (!mTouchInWindow.ContainsKey(hoverWindow))
					{
						mTouchInWindow.Add(hoverWindow, new Dictionary<int, Touch>());
					}
					// 每个窗口最多有两个触点
					if (mTouchInWindow[hoverWindow].Count < 2)
					{
						mTouchInWindow[hoverWindow].Add(touch.fingerId, touch);
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
					MultiTouchInfo info;
					mClassPool.newClass(out info);
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
			foreach (var item in endMultiWindow)
			{
				item.onMultiTouchEnd();
				if(mMultiTouchWindowList.ContainsKey(item))
				{
					mClassPool.destroyClass(mMultiTouchWindowList[item]);
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
				if (mInputManager.getMouseCurrentDown(MOUSE_BUTTON.MB_LEFT))
				{
					notifyGlobalPress(true);
				}
				else if (mInputManager.getMouseCurrentUp(MOUSE_BUTTON.MB_LEFT))
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
				foreach (var item in mMouseDownWindowList)
				{
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
				if (mStayTimer.checkTimeCount(elapsedTime))
				{
					mHoverWindow?.onMouseStay(curMousePosition);
					// 给鼠标按下时选中的所有窗口发送鼠标移动的消息
					foreach (var item in mMouseDownWindowList)
					{
						item.onMouseStay(curMousePosition);
					}
				}
			}
		}
		// 检查摄像机是否被销毁
		foreach (var item in mMouseCastWindowList)
		{
			if (item.mCamera != null && item.mCamera.isDestroied())
			{
				logError("摄像机已销毁:" + item.mCamera.getName());
			}
		}
		foreach (var item in mMouseCastObjectList)
		{
			if (item.mCamera != null && item.mCamera.isDestroied())
			{
				logError("摄像机已销毁:" + item.mCamera.getName());
			}
		}
		if (getKeyCurrentDown(KeyCode.F2))
		{
			Vector3 mousePos = getMousePosition();
			var resultList = getAllHoverWindow(ref mousePos, null, true);
			int resultCount = resultList.Count;
			for(int i = 0; i < resultCount; ++i)
			{
				UIDepth depth = resultList[i].getUIDepth();
				logInfo("窗口:" + resultList[i].getName() + "深度:layout:" + depth.mPanelDepth + ", window:" + depth.mWindowDepth, LOG_LEVEL.LL_FORCE);
			}
		}
	}
	public bool isColliderRegisted(IMouseEventCollect obj) { return mAllButtonSet.Contains(obj); }
	// 注册碰撞器,只有注册了的碰撞器才会进行检测
	public void registeBoxCollider(IMouseEventCollect button, 
									ObjectClickCallback clickCallback = null, 
									ObjectPressCallback pressCallback = null, 
									ObjectHoverCallback hoverCallback = null,
									GameCamera camera = null)
	{
		if(!mUseGlobalTouch)
		{
			logError("Not Active Global Touch!");
			return;
		}
		if (button.getCollider() == null)
		{
			logError("window must has collider that can registeBoxCollider! " + button.getName());
			return;
		}
		button.setClickCallback(clickCallback);
		button.setPressCallback(pressCallback);
		button.setHoverCallback(hoverCallback);
		if (!mAllButtonSet.Contains(button))
		{
			if (button is txUIObject)
			{
				// 寻找窗口对应的摄像机
				if (WidgetUtility.isNGUI((button as txUIObject).getObject()))
				{
					++mNGUICount;
					if (camera == null)
					{
						camera = mCameraManager.getUICamera(true);
					}
				}
				else
				{
					++mUGUICount;
					if (camera == null)
					{
						camera = mCameraManager.getUICamera(false);
					}
				}
				if (camera == null)
				{
					logError("can not find ui camera for raycast!");
				}
				// 将窗口加入到鼠标射线检测列表中
				UIDepth depth = button.getUIDepth();
				MouseCastWindowSet mouseCastSet = null;
				foreach (var item in mMouseCastWindowList)
				{
					if (item.mCamera == camera)
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
				mouseCastSet.addWindow(depth, button);
				mButtonDepthList.Add(button, depth);
			}
			else if (button is MovableObject)
			{
				MouseCastObjectSet mouseCastSet = null;
				foreach (var item in mMouseCastObjectList)
				{
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
				mouseCastSet.addObject(button);
				mMovableObjectList.Add(button);
			}
			mAllButtonList.Add(button);
			mAllButtonSet.Add(button);
		}
	}
	public void bindPassOnlyArea(IMouseEventCollect parent, IMouseEventCollect passOnlyArea)
	{
		mPassOnlyArea.Add(parent, passOnlyArea);
	}
	// 注销碰撞器
	public void unregisteBoxCollider(IMouseEventCollect obj)
	{
		if (!mAllButtonSet.Contains(obj))
		{
			return;
		}
		if (mHoverWindow == obj)
		{
			mHoverWindow = null;
		}
		if (obj is txUIObject)
		{
			// 从深度列表中移除
			UIDepth depth = mButtonDepthList[obj];
			mButtonDepthList.Remove(obj);
			UIDepth curDepth = obj.getUIDepth();
			if (depth.mPanelDepth != curDepth.mPanelDepth || !isFloatEqual(depth.mWindowDepth, curDepth.mWindowDepth))
			{
				logError("depth error");
			}
			foreach (var item in mMouseCastWindowList)
			{
				if (item.hasWindow(depth, obj))
				{
					item.removeWindow(obj);
					if (item.isEmpty())
					{
						mMouseCastWindowList.Remove(item);
					}
					break;
				}
			}
			if (WidgetUtility.isNGUI((obj as txUIObject).getObject()))
			{
				--mNGUICount;
			}
			else
			{
				--mUGUICount;
			}
		}
		else if (obj is MovableObject)
		{
			foreach(var item in mMouseCastObjectList)
			{
				if(item.hasObject(obj))
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
		mAllButtonList.Remove(obj);
		mAllButtonSet.Remove(obj);
		mMouseDownWindowList.Remove(obj);
		mMultiTouchWindowList.Remove(obj);
	}
	public void notifyWindowDepthChanged(IMouseEventCollect button)
	{
		// 只判断UI的深度变化
		if(!(button is txUIObject))
		{
			return;
		}
		// 如果之前没有记录过,则不做判断
		if(!mAllButtonSet.Contains(button))
		{
			return;
		}
		UIDepth lastDepth = mButtonDepthList[button];
		foreach(var item in mMouseCastWindowList)
		{
			if(item.hasWindow(lastDepth, button))
			{
				item.windowDepthChanged(lastDepth, button);
				break;
			}
		}
		mButtonDepthList[button] = button.getUIDepth();
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
		foreach (var item in mAllButtonList)
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
		foreach (var button in raycast)
		{
			// 如果此时窗口已经被销毁了,则不再通知
			if(!mAllButtonSet.Contains(button))
			{
				continue;
			}
			if (press)
			{
				button.onMouseDown(mousePosition);
			}
			else
			{
				button.onMouseUp(mousePosition);
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
		mMouseCastWindowList.Sort(MouseCastWindowSet.depthDescend);
		foreach(var item in mMouseCastWindowList)
		{
			if(!continueRay)
			{
				break;
			}
			// 检查摄像机是否被销毁
			if(item.mCamera == null || item.mCamera.isDestroied())
			{
				logError("摄像机已销毁:" + item.mCamera.getName());
				continue;
			}
			getCameraRay(ref mousePos, out Ray ray, item.mCamera.getCamera());
			raycastLayout(ref ray, item.mWindowOrderList, mTempList2, ref continueRay, false, 0, ignorePassRay);
		}
		// UI层允许当前鼠标射线穿过时才检测场景物体
		if(continueRay)
		{
			mMouseCastObjectList.Sort(MouseCastObjectSet.depthDescend);
			foreach (var item in mMouseCastObjectList)
			{
				if (!continueRay)
				{
					break;
				}
				// 检查摄像机是否被销毁
				if (item.mCamera != null && item.mCamera.isDestroied())
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
		foreach (var box in moveObjectList)
		{
			// 将所有射线碰到的物体都放到列表中
			if (box.isActive() && box.isHandleInput() && box.getCollider().Raycast(ray, out hit, 10000.0f))
			{
				sortList.Add(new DistanceSortHelper(getSquaredLength(hit.point - ray.origin), box));
			}
		}
		// 根据相交点由由近到远的顺序排序
		sortList.Sort(DistanceSortHelper.distanceAscend);
		foreach (var item in sortList)
		{
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
	protected void raycastLayout(ref Ray ray, SortedDictionary<UIDepth, List<IMouseEventCollect>> windowOrderList, 
								List<IMouseEventCollect> retList, ref bool continueRay, bool clearList = true, 
								int maxCount = 0, bool ignorePassRay = false)
	{
		if(clearList)
		{
			retList.Clear();
		}
		continueRay = true;
		RaycastHit hit;
		foreach (var box in windowOrderList)
		{
			int count = box.Value.Count;
			for (int i = 0; i < count; ++i)
			{
				IMouseEventCollect window = box.Value[i];
				if (window.isActive() && window.isHandleInput() && window.getCollider().Raycast(ray, out hit, 10000.0f))
				{
					// 点击了只允许部分穿透的背景
					if (mPassOnlyArea.ContainsKey(window))
					{
						IMouseEventCollect child = mPassOnlyArea[window];
						//判断是否点到了背景中允许穿透的部分,如果是允许穿透的部分,则射线可以继续判断下层的窗口，否则不允许再继续穿透
						continueRay = child.isActive() && child.isHandleInput() && child.getCollider().Raycast(ray, out hit, 10000.0f);
						if (!continueRay)
						{
							break;
						}
					}
					else
					{
						retList.Add(window);
						// 如果射线不能穿透当前按钮,或者已经达到最大数量,则不再继续
						bool passRay = ignorePassRay || window.isPassRay();
						bool countNotFull = maxCount <= 0 || retList.Count < maxCount;
						continueRay = passRay && countNotFull;
						if (!continueRay)
						{
							break;
						}
					}
				}
			}
			if (!continueRay)
			{
				break;
			}
		}
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
