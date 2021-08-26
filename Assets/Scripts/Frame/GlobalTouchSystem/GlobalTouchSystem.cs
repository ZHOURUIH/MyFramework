using UnityEngine;
using System.Collections.Generic;

// 用来代替UGUI的EventSystem,统一多摄像机的鼠标事件通知
public class GlobalTouchSystem : FrameSystem
{
	protected SafeDictionary<IMouseEventCollect, IMouseEventCollect> mPassOnlyArea;         // 点击穿透区域绑定列表,Key的区域中,只允许Value的区域穿透,Key无论是否设置了允许射线穿透,实际检测时都是不能够穿透的
	protected Dictionary<int, TouchInfo> mTouchInfoList;        // 当前触点信息列表
	protected HashSet<IMouseEventCollect> mParentPassOnlyList;  // 仅父节点区域可穿透的列表
	protected HashSet<IMouseEventCollect> mAllObjectSet;        // 所有参与鼠标或触摸事件的窗口和物体列表
	protected List<MouseCastWindowSet> mMouseCastWindowList;    // 所有窗口所对应的摄像机的列表,每个摄像机的窗口列表根据深度排序
	protected List<MouseCastObjectSet> mMouseCastObjectList;    // 所有场景中物体所对应的摄像机的列表,每个摄像机的物体列表根据深度排序
	protected bool mUseGlobalTouch;                             // 是否使用全局触摸检测来进行界面的输入检测
	public GlobalTouchSystem()
	{
		mPassOnlyArea = new SafeDictionary<IMouseEventCollect, IMouseEventCollect>();
		mTouchInfoList = new Dictionary<int, TouchInfo>();
		mParentPassOnlyList = new HashSet<IMouseEventCollect>();
		mAllObjectSet = new HashSet<IMouseEventCollect>();
		mMouseCastWindowList = new List<MouseCastWindowSet>();
		mMouseCastObjectList = new List<MouseCastObjectSet>();
		mUseGlobalTouch = true;
		Input.multiTouchEnabled = true;
	}
	public override void init()
	{
		base.init();
	}
	public override void destroy()
	{
		mAllObjectSet.Clear();
		mMouseCastWindowList.Clear();
		mMouseCastObjectList.Clear();
		base.destroy();
	}
	public void setUseGlobalTouch(bool use) { mUseGlobalTouch = use; }
	public IMouseEventCollect getHoverWindow(Vector3 pos, IMouseEventCollect ignoreWindow = null, bool ignorePassRay = false)
	{
		// 返回最上层的窗口
		LIST(out List<IMouseEventCollect> resultList);
		globalRaycast(resultList, pos, ignorePassRay);
		IMouseEventCollect forwardButton = null;
		int count = resultList.Count;
		for (int i = 0; i < count; ++i)
		{
			IMouseEventCollect window = resultList[i];
			if (ignoreWindow != window)
			{
				forwardButton = window;
				break;
			}
		}
		UN_LIST(resultList);
		return forwardButton;
	}
	// 越顶层的窗口越靠近列表前面
	public void getAllHoverWindow(ICollection<IMouseEventCollect> hoverList, Vector3 pos, IMouseEventCollect ignoreWindow = null, bool ignorePassRay = false)
	{
		LIST(out List<IMouseEventCollect> resultList);
		globalRaycast(resultList, pos, ignorePassRay);
		int count = resultList.Count;
		for (int i = 0; i < count; ++i)
		{
			IMouseEventCollect window = resultList[i];
			if (ignoreWindow != window)
			{
				hoverList.Add(window);
			}
		}
		UN_LIST(resultList);
	}
	public override void update(float elapsedTime)
	{
		if (!mUseGlobalTouch)
		{
			return;
		}

		// 点击操作会判断触点和鼠标,因为都会产生点击操作
		// 在触屏上,会将第一个触点也当作鼠标,也就是触屏上使用InputManager.isMouseCurrentDown实际上是获得的第一个触点的信息
		// 所以先判断有没有触点,如果没有再判断鼠标
		if (Input.touchCount == 0)
		{
			if (mInputSystem.isMouseCurrentDown(MOUSE_BUTTON.LEFT))
			{
				notifyTouchPress(mInputSystem.getMousePosition(), (int)MOUSE_BUTTON.LEFT);
			}
			else if (mInputSystem.isMouseCurrentUp(MOUSE_BUTTON.LEFT))
			{
				// 使用鼠标时始终都会有一个触点信息,即使抬起也不会移除
				notifyTouchRelease(mInputSystem.getMousePosition(), (int)MOUSE_BUTTON.LEFT, false);
			}
			else
			{
				if (mTouchInfoList.TryGetValue((int)MOUSE_BUTTON.LEFT, out TouchInfo info))
				{
					info.setCurPosition(mInputSystem.getMousePosition());
				}
			}
		}
		else
		{
			int count = Input.touchCount;
			for (int i = 0; i < count; ++i)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.phase == TouchPhase.Began)
				{
					notifyTouchPress(touch.position, touch.fingerId);
				}
				else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
				{
					notifyTouchRelease(touch.position, touch.fingerId);
				}
				else if (touch.phase == TouchPhase.Moved)
				{
					if (mTouchInfoList.TryGetValue(touch.fingerId, out TouchInfo info))
					{
						info.setCurPosition(touch.position);
					}
				}
			}
		}

		// 更新触点逻辑
		foreach (var item in mTouchInfoList)
		{
			item.Value.update(elapsedTime);
		}

		// 检查摄像机是否被销毁
#if UNITY_EDITOR
		int windowCount = mMouseCastWindowList.Count;
		for (int i = 0; i < windowCount; ++i)
		{
			MouseCastWindowSet item = mMouseCastWindowList[i];
			if (item.getCamera() != null && item.getCamera().isDestroy())
			{
				logError("摄像机已销毁:" + item.getCamera().getName());
			}
		}
		int objCount = mMouseCastObjectList.Count;
		for (int i = 0; i < objCount; ++i)
		{
			MouseCastObjectSet item = mMouseCastObjectList[i];
			if (item.mCamera != null && item.mCamera.isDestroy())
			{
				logError("摄像机已销毁:" + item.mCamera.getName());
			}
		}
#endif
	}
	public bool isColliderRegisted(IMouseEventCollect obj) { return mAllObjectSet.Contains(obj); }
	// 注册碰撞器,只有注册了的碰撞器才会进行检测
	public void registeCollider(IMouseEventCollect obj, GameCamera camera = null)
	{
		if (!mUseGlobalTouch)
		{
			logWarning("全局输入检测未启用,将无法注册碰撞体!");
			return;
		}
		if (obj.getCollider() == null)
		{
			logError("注册碰撞体的物体上找不到碰撞体组件! name:" + obj.getName() + ", " + obj.getDescription());
			return;
		}
		if (mAllObjectSet.Contains(obj))
		{
			logError("不能重复注册碰撞体: " + obj.getName() + ", " + obj.getDescription());
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
			for (int i = 0; i < count; ++i)
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
				mouseCastSet = new MouseCastWindowSet();
				mouseCastSet.setCamera(camera);
				mMouseCastWindowList.Add(mouseCastSet);
			}
			mouseCastSet.addWindow(obj);
		}
		else if (obj is MovableObject)
		{
			MouseCastObjectSet mouseCastSet = null;
			int count = mMouseCastObjectList.Count;
			for (int i = 0; i < count; ++i)
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
				mouseCastSet = new MouseCastObjectSet();
				mouseCastSet.setCamera(camera);
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
			logError("需要先注册碰撞体,才能绑定穿透区域, name" + passOnlyArea.getName() + ", " + passOnlyArea.getDescription());
			return;
		}
		mPassOnlyArea.add(parent, passOnlyArea);
	}
	// parent的区域中才能允许parent的子节点接收射线检测
	public void bindPassOnlyParent(IMouseEventCollect parent)
	{
		if (!mAllObjectSet.Contains(parent))
		{
			logError("需要先注册碰撞体,才能绑定父节点穿透区域, name:" + parent.getName() + ", " + parent.getDescription());
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

		foreach (var item in mTouchInfoList)
		{
			item.Value.removeObject(obj);
		}

		if (obj is myUIObject)
		{
			int count = mMouseCastWindowList.Count;
			for (int i = 0; i < count; ++i)
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
			for (int i = 0; i < count; ++i)
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
		mParentPassOnlyList.Remove(obj);
		// key或者value中任意一个注销了,都要从列表中移除
		if (!mPassOnlyArea.remove(obj))
		{
			foreach (var item in mPassOnlyArea.startForeach())
			{
				if (item.Value == obj)
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
	//------------------------------------------------------------------------------------------------------------------------------
	protected void notifyTouchPress(Vector3 pos, int touchID)
	{
		// 触点按下时记录触点的初始位置
		if (!mTouchInfoList.TryGetValue(touchID, out TouchInfo touchInfo))
		{
			CLASS(out touchInfo);
			mTouchInfoList.Add(touchID, touchInfo);
		}
		touchInfo.init(touchID, pos);
		// 通知全局屏幕触点事件
		foreach (var item in mAllObjectSet)
		{
			if (!item.isReceiveScreenMouse())
			{
				continue;
			}
			item.onScreenMouseDown(pos, touchID);
		}
		var pressList = touchInfo.getPressList();
		int count = pressList.Count;
		for (int i = 0; i < count; ++i)
		{
			IMouseEventCollect obj = pressList[i];
			// 如果此时窗口已经被销毁了,则不再通知,因为可能在onScreenMouseDown中销毁了
			if (!mAllObjectSet.Contains(obj))
			{
				continue;
			}
			obj.onMouseDown(pos, touchID);
		}
	}
	protected void notifyTouchRelease(Vector3 pos, int touchID, bool removeTouch = true)
	{
		// 触点抬起时移除记录的触点位置
		mTouchInfoList.TryGetValue(touchID, out TouchInfo touchInfo);

		// 通知全局屏幕触点事件
		foreach (var item in mAllObjectSet)
		{
			if (!item.isReceiveScreenMouse())
			{
				continue;
			}
			item.onScreenMouseUp(pos, touchID);
		}

		var pressList = touchInfo.getPressList();
		int count = pressList.Count;
		for (int i = 0; i < count; ++i)
		{
			IMouseEventCollect obj = pressList[i];
			// 如果此时窗口已经被销毁了,则不再通知,因为可能在onScreenMouseUp中销毁了
			if (!mAllObjectSet.Contains(obj))
			{
				continue;
			}
			obj.onMouseUp(pos, touchID);
		}

		if (removeTouch)
		{
			mTouchInfoList.Remove(touchID);
			UN_CLASS(touchInfo);
		}
		else
		{
			touchInfo.clearPressList();
		}
	}
	// 全局射线检测
	protected void globalRaycast(List<IMouseEventCollect> resultList, Vector3 mousePos, bool ignorePassRay = false)
	{
		bool continueRay = true;
		// 每次检测UI时都需要对列表按摄像机深度进行降序排序
		quickSort(mMouseCastWindowList, MouseCastWindowSet.mComparisonDescend);
		int windowSetCount = mMouseCastWindowList.Count;
		for (int i = 0; i < windowSetCount; ++i)
		{
			var item = mMouseCastWindowList[i];
			if (!continueRay)
			{
				break;
			}
			// 检查摄像机是否被销毁
			GameCamera camera = item.getCamera();
			if (camera == null || camera.isDestroy())
			{
				logError("摄像机已销毁:" + camera.getName());
				continue;
			}
			Ray ray = getCameraRay(mousePos, camera.getCamera());
			raycastLayout(ray, item.getWindowOrderList(), resultList, ref continueRay, false, ignorePassRay);
		}
		// UI层允许当前鼠标射线穿过时才检测场景物体
		if (continueRay)
		{
			quickSort(mMouseCastObjectList, MouseCastObjectSet.mCompareDescend);
			int objSetCount = mMouseCastObjectList.Count;
			for (int i = 0; i < objSetCount; ++i)
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
				if (item.mCamera == null)
				{
					GameCamera mainCamera = getMainCamera();
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
				Ray ray = getCameraRay(mousePos, camera);
				raycastMovableObject(ray, item.mObjectOrderList, resultList, ref continueRay, false);
			}
		}
	}
	protected void raycastMovableObject(Ray ray, List<IMouseEventCollect> moveObjectList, List<IMouseEventCollect> retList, ref bool continueRay, bool clearList = true)
	{
		if (clearList)
		{
			retList.Clear();
		}
		continueRay = true;
		RaycastHit hit;
		LIST(out List<DistanceSortHelper> sortList);
		int objCount = moveObjectList.Count;
		for (int i = 0; i < objCount; ++i)
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
		for (int i = 0; i < sortCount; ++i)
		{
			DistanceSortHelper item = sortList[i];
			retList.Add(item.mObject);
			if (!item.mObject.isPassRay())
			{
				continueRay = false;
				break;
			}
		}
		UN_LIST(sortList);
	}
	// ignorePassRay表示是否忽略窗口的isPassRay属性,true表示认为所有的都允许射线穿透
	// 但是ignorePassRay不会影响到PassOnlyArea和ParentPassOnly
	protected void raycastLayout(Ray ray,
								 List<IMouseEventCollect> windowOrderList,
								 List<IMouseEventCollect> retList,
								 ref bool continueRay,
								 bool clearList = true,
								 bool ignorePassRay = false)
	{
		if (clearList)
		{
			retList.Clear();
		}
		// mParentPassOnlyList需要重新整理,排除未启用的布局的窗口
		LIST(out HashSet<IMouseEventCollect> activeParentList);
		// 在只允许父节点穿透的列表中已成功穿透的父节点列表
		LIST(out HashSet<IMouseEventCollect> passParent);

		// 筛选出已激活的父节点穿透窗口
		foreach (var item in mParentPassOnlyList)
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
		for (int i = 0; i < windowCount; ++i)
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
					// 判断是否点到了背景中允许穿透的部分,如果是允许穿透的部分,则射线可以继续判断下层的窗口，否则不允许再继续穿透
					continueRay = passOnlyArea.isActiveInHierarchy() &&
									passOnlyArea.isHandleInput() &&
									passOnlyArea.getCollider().Raycast(ray, out _, 10000.0f);
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
		UN_LIST(passParent);
		UN_LIST(activeParentList);
	}
	// obj的所有父节点中是否允许射线选中obj
	// bindParentList是当前激活的已绑定的仅父节点区域穿透的列表
	// passedParentList是bindParentList中射线已经穿透的父节点
	protected bool isParentPassed(IMouseEventCollect obj, HashSet<IMouseEventCollect> bindParentList, HashSet<IMouseEventCollect> passedParentList)
	{
		foreach (var item in bindParentList)
		{
			// 有父节点,并且父节点未成功穿透时,则认为当前窗口未相交
			if (obj.isChildOf(item) && !passedParentList.Contains(item))
			{
				return false;
			}
		}
		return true;
	}
}