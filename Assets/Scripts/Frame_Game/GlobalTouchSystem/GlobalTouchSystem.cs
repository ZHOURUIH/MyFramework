using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBase;
using static MathUtility;
using static FrameEditorUtility;

// 用来代替UGUI的EventSystem,统一多摄像机的鼠标事件通知
public class GlobalTouchSystem : FrameSystem
{
	protected Dictionary<int, TouchInfo> mTouchInfoList = new();        // 当前触点信息列表
	protected HashSet<IMouseEventCollect> mAllObjectSet = new();        // 所有参与鼠标或触摸事件的窗口和物体列表
	protected List<MouseCastWindowSet> mMouseCastWindowList = new();    // 所有窗口所对应的摄像机的列表,每个摄像机的窗口列表根据深度排序
	protected List<MouseCastObjectSet> mMouseCastObjectList = new();    // 所有场景中物体所对应的摄像机的列表,每个摄像机的物体列表根据深度排序
	protected bool mUseGlobalTouch = true;                              // 是否使用全局触摸检测来进行界面的输入检测
	public GlobalTouchSystem()
	{
		Input.multiTouchEnabled = true;
	}
	public override void destroy()
	{
		mAllObjectSet.Clear();
		mMouseCastWindowList.Clear();
		mMouseCastObjectList.Clear();
		base.destroy();
	}
	// 越顶层的物体越靠近列表前面
	public void getAllHoverObject(ICollection<IMouseEventCollect> hoverList, Vector3 pos, IMouseEventCollect ignoreWindow = null, bool ignorePassRay = false)
	{
		hoverList.Clear();
		using var a = new ListScope<IMouseEventCollect>(out var resultList);
		globalRaycast(resultList, pos, ignorePassRay);
		foreach (IMouseEventCollect window in resultList)
		{
			if (ignoreWindow != window)
			{
				hoverList.Add(window);
			}
		}
	}
	public override void update(float elapsedTime)
	{
		if (!mUseGlobalTouch)
		{
			return;
		}
		foreach (MouseCastWindowSet item in mMouseCastWindowList)
		{
			item.update();
		}

		using var a = new SafeDictionaryReader<int, TouchPoint>(mInputSystem.getTouchPointList());
		foreach (var item in a.mReadList)
		{
			if (item.Value.isCurrentDown())
			{
				notifyTouchPress(item.Value);
			}
			else if (item.Value.isCurrentUp())
			{
				notifyTouchRelease(item.Key);
			}
		}

		// 更新触点逻辑
		foreach (TouchInfo item in mTouchInfoList.Values)
		{
			item.update(elapsedTime);
		}

		// 检查摄像机是否被销毁
		if (isEditor())
		{
			foreach (MouseCastWindowSet item in mMouseCastWindowList)
			{
				if (item.getCamera() != null && item.getCamera().isDestroy())
				{
					logError("摄像机已销毁:" + item.getCamera().getName());
				}
			}
			foreach (MouseCastObjectSet item in mMouseCastObjectList)
			{
				if (item.mCamera != null && item.mCamera.isDestroy())
				{
					logError("摄像机已销毁:" + item.mCamera.getName());
				}
			}
		}
	}
	public bool isColliderRegisted(IMouseEventCollect obj) { return mAllObjectSet.Contains(obj); }
	// 注册碰撞器,只有注册了的碰撞器才会进行检测
	public void registeCollider(IMouseEventCollect obj, GameCamera camera = null)
	{
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

		if (obj is myUIObject uiObj)
		{
			// 寻找窗口对应的摄像机
			camera ??= mCameraManager.getUICamera();
			if (camera == null)
			{
				logError("can not find ui camera for raycast!");
			}
			// 将窗口加入到鼠标射线检测列表中
			MouseCastWindowSet mouseCastSet = null;
			foreach (MouseCastWindowSet item in mMouseCastWindowList)
			{
				if (item.getCamera() == camera)
				{
					mouseCastSet = item;
					break;
				}
			}
			if (mouseCastSet == null)
			{
				mouseCastSet = new();
				mouseCastSet.setCamera(camera);
				mMouseCastWindowList.Add(mouseCastSet);
			}
			mouseCastSet.addWindow(uiObj);
		}
		else if (obj is MovableObject)
		{
			MouseCastObjectSet mouseCastSet = null;
			foreach (MouseCastObjectSet item in mMouseCastObjectList)
			{
				if (item.mCamera == camera)
				{
					mouseCastSet = item;
					break;
				}
			}
			if (mouseCastSet == null)
			{
				mouseCastSet = new();
				mouseCastSet.setCamera(camera);
				mMouseCastObjectList.Add(mouseCastSet);
			}
			mouseCastSet.addObject(obj);
		}
		else
		{
			logError("不支持的注册类型:" + obj.GetType());
		}
		mAllObjectSet.Add(obj);
	}
	// 注销碰撞器
	public void unregisteCollider(IMouseEventCollect obj)
	{
		if (!mAllObjectSet.Remove(obj))
		{
			return;
		}

		foreach (TouchInfo item in mTouchInfoList.Values)
		{
			item.removeObject(obj);
		}

		if (obj is myUIObject window)
		{
			int count = mMouseCastWindowList.Count;
			for (int i = 0; i < count; ++i)
			{
				MouseCastWindowSet item = mMouseCastWindowList[i];
				if (item.removeWindow(window))
				{
					if (item.isEmpty())
					{
						mMouseCastWindowList.RemoveAt(i);
					}
					break;
				}
			}
		}
		else if (obj is MovableObject movable)
		{
			int count = mMouseCastObjectList.Count;
			for (int i = 0; i < count; ++i)
			{
				MouseCastObjectSet item = mMouseCastObjectList[i];
				if (item.removeObject(obj))
				{
					if (item.isEmpty())
					{
						mMouseCastObjectList.RemoveAt(i);
					}
					break;
				}
			}
		}
		else
		{
			logError("此对象无法注销:" + obj.ToString());
		}
	}
	public void notifyWindowActiveChanged()
	{
		foreach (MouseCastWindowSet item in mMouseCastWindowList)
		{
			item.notifyWindowActiveChanged();
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void notifyTouchPress(TouchPoint touch)
	{
		int touchID = touch.getTouchID();
		Vector3 pos = touch.getCurPosition();
		// 触点按下时记录触点的初始位置
		TouchInfo touchInfo = mTouchInfoList.getOrAddClass(touchID);
		touchInfo.init(touch);
		touchInfo.touchPress();

		// 通知全局屏幕触点事件
		foreach (IMouseEventCollect item in mAllObjectSet)
		{
			if (item.isReceiveScreenMouse())
			{
				item.onScreenMouseDown(pos, touchID);
			}
		}
		using var a = new SafeListReader<IMouseEventCollect>(touchInfo.getPressList());
		foreach (IMouseEventCollect obj in a.mReadList)
		{
			// 如果此时窗口已经被销毁了,则不再通知,因为可能在onScreenMouseDown中销毁了
			if (mAllObjectSet.Contains(obj))
			{
				obj.onMouseDown(pos, touchID);
			}
		}
	}
	protected void notifyTouchRelease(int touchID)
	{
		// 触点抬起时移除记录的触点位置
		if (!mTouchInfoList.TryGetValue(touchID, out TouchInfo touchInfo))
		{
			return;
		}

		Vector3 pos = touchInfo.getTouch().getCurPosition();
		foreach (IMouseEventCollect item in mAllObjectSet)
		{
			if (item.isReceiveScreenMouse())
			{
				item.onScreenMouseUp(pos, touchID);
			}
		}

		using var a = new SafeListReader<IMouseEventCollect>(touchInfo.getPressList());
		foreach (IMouseEventCollect obj in a.mReadList)
		{
			// 如果此时窗口已经被销毁了,则不再通知,因为可能在onScreenMouseUp中销毁了
			if (mAllObjectSet.Contains(obj))
			{
				obj.onMouseUp(pos, touchID);
			}
		}

		if (touchInfo.getTouch().isMouse())
		{
			touchInfo.clearPressList();
		}
		else
		{
			mTouchInfoList.Remove(touchID);
			UN_CLASS(ref touchInfo);
		}
	}
	// 全局射线检测
	protected void globalRaycast(List<IMouseEventCollect> resultList, Vector3 mousePos, bool ignorePassRay = false)
	{
		bool continueRay = true;
		// 每次检测UI时都需要对列表按摄像机深度进行降序排序
		quickSort(mMouseCastWindowList, MouseCastWindowSet.mComparisonDescend);
		foreach (MouseCastWindowSet item in mMouseCastWindowList)
		{
			if (!continueRay)
			{
				break;
			}
			// 检查摄像机是否被销毁
			GameCamera camera = item.getCamera();
			if (!camera.isValid())
			{
				logError("摄像机已销毁:" + camera.getName());
				continue;
			}
			Ray ray = getCameraRay(mousePos, camera.getCamera());
			// 没有指定的交互物体
			raycastLayout(ray, item.getWindowOrderList(), resultList, ref continueRay, false, ignorePassRay);
		}
		// UI层允许当前鼠标射线穿过时才检测场景物体
		if (continueRay)
		{
			quickSort(mMouseCastObjectList, MouseCastObjectSet.mCompareDescend);
			foreach (MouseCastObjectSet item in mMouseCastObjectList)
			{
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
				raycastMovableObject(getCameraRay(mousePos, camera), item.mObjectOrderList, resultList, ref continueRay, false);
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
		using var a = new ListScope<DistanceSortHelper>(out var sortList);
		foreach (IMouseEventCollect box in moveObjectList)
		{
			// 将所有射线碰到的物体都放到列表中
			if (box.isActiveInHierarchy() && 
				box.isHandleInput() && 
				box.getCollider() != null && 
				box.getCollider().Raycast(ray, out RaycastHit hit, 10000.0f))
			{
				sortList.Add(new(getSquaredLength(hit.point - ray.origin), box));
			}
		}
		// 根据相交点由近到远的顺序排序
		quickSort(sortList, DistanceSortHelper.mCompareAscend);
		foreach (DistanceSortHelper item in sortList)
		{
			retList.Add(item.mObject);
			if (!item.mObject.isPassRay())
			{
				continueRay = false;
				break;
			}
		}
	}
	// ignorePassRay表示是否忽略窗口的isPassRay属性,true表示认为所有的都允许射线穿透
	// 但是ignorePassRay不会影响到PassOnlyArea和ParentPassOnly
	protected void raycastLayout<T>(Ray ray,
									List<T> windowOrderList,
									List<IMouseEventCollect> retList,
									ref bool continueRay,
									bool clearList = true,
									bool ignorePassRay = false) where T : IMouseEventCollect
	{
		if (clearList)
		{
			retList.Clear();
		}
		// mParentPassOnlyList需要重新整理,排除未启用的布局的窗口
		// passParent,在只允许父节点穿透的列表中已成功穿透的父节点列表
		using var a = new HashSetScope2<IMouseEventCollect>(out var activeParentList, out var passParent);

		// 射线检测
		continueRay = true;
		foreach (IMouseEventCollect window in windowOrderList)
		{
			if (window.isDestroy())
			{
				logError("窗口已经被销毁,无法访问:" + window.getName());
				continue;
			}
			if (window.isActiveInHierarchy() && 
				window.isHandleInput() && 
				window.getCollider().Raycast(ray, out _, 10000.0f))
			{
				// 点击到了只允许父节点穿透的窗口,记录到列表中
				// 但是因为父节点一定是在子节点之后判断的,子节点可能已经拦截了射线,从而导致无法检测到父节点
				if (activeParentList.Contains(window))
				{
					passParent.Add(window);
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
	}
	// obj的所有父节点中是否允许射线选中obj
	// bindParentList是当前激活的已绑定的仅父节点区域穿透的列表
	// passedParentList是bindParentList中射线已经穿透的父节点
	protected bool isParentPassed(IMouseEventCollect obj, HashSet<IMouseEventCollect> bindParentList, HashSet<IMouseEventCollect> passedParentList)
	{
		foreach (IMouseEventCollect item in bindParentList)
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