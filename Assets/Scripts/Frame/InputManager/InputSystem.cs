using UnityEngine;
using System.Collections.Generic;
using System;

// 输入系统,用于封装Input
public class InputSystem : FrameSystem
{
	protected Dictionary<object, Dictionary<OnKeyCurrentDown, KeyInfo>> mListenerList;      // 以监听者为索引的快捷键监听列表
	protected SafeDictionary<KeyCode, SafeList<KeyInfo>> mKeyListenList;                    // 按键按下的监听回调列表
	protected SafeDictionary<int, TouchPoint> mTouchPointList;  // 触点信息列表,可能与GlobalTouchSystem有一定的重合
	protected HashSet<IInputField> mInputFieldList;				// 输入框列表,用于判断当前是否正在输入
	protected SafeList<DeadClick> mLastClickList;				// 已经完成单击的行为,用于实现双击的功能
	protected int mFocusMask;                                   // 当前的输入掩码,是输入框的输入还是快捷键输入
	public InputSystem()
	{
		mListenerList = new Dictionary<object, Dictionary<OnKeyCurrentDown, KeyInfo>>();
		mKeyListenList = new SafeDictionary<KeyCode, SafeList<KeyInfo>>();
		mTouchPointList = new SafeDictionary<int, TouchPoint>();
		mInputFieldList = new HashSet<IInputField>();
		mLastClickList = new SafeList<DeadClick>();
	}
	public override void init()
	{
		base.init();
		// 编辑器或者桌面端,默认会有鼠标三个键的触点
#if UNITY_EDITOR || UNITY_STANDALONE
		addTouch((int)MOUSE_BUTTON.LEFT, true);
		addTouch((int)MOUSE_BUTTON.RIGHT, true);
		addTouch((int)MOUSE_BUTTON.MIDDLE, true);
#endif
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 点击操作会判断触点和鼠标,因为都会产生点击操作
		// 在触屏上,会将第一个触点也当作鼠标,也就是触屏上使用isMouseCurrentDown实际上是获得的第一个触点的信息
		// 所以先判断有没有触点,如果没有再判断鼠标
		if (Input.touchCount == 0)
		{
			// 左键
			if (Input.GetMouseButtonDown((int)MOUSE_BUTTON.LEFT))
			{
				pointDown((int)MOUSE_BUTTON.LEFT, Input.mousePosition);
			}
			else if (Input.GetMouseButtonUp((int)MOUSE_BUTTON.LEFT))
			{
				pointUp((int)MOUSE_BUTTON.LEFT, Input.mousePosition);
			}
			// 仅在桌面端才会有右键和中键
#if UNITY_EDITOR || UNITY_STANDALONE
			// 右键
			if (isMouseRightCurrentDown())
			{
				pointDown((int)MOUSE_BUTTON.RIGHT, Input.mousePosition);
			}
			else if (isMouseRightCurrentUp())
			{
				pointUp((int)MOUSE_BUTTON.RIGHT, Input.mousePosition);
			}
			// 中键
			if (isMouseMiddleCurrentDown())
			{
				pointDown((int)MOUSE_BUTTON.MIDDLE, Input.mousePosition);
			}
			else if (isMouseMiddleCurrentUp())
			{
				pointUp((int)MOUSE_BUTTON.MIDDLE, Input.mousePosition);
			}
#endif
		}
		else
		{
			for (int i = 0; i < Input.touchCount; ++i)
			{
				Touch touch = Input.GetTouch(i);
				if (touch.phase == TouchPhase.Began)
				{
					pointDown(touch.fingerId, touch.position);
				}
				else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
				{
					pointUp(touch.fingerId, touch.position);
				}
			}
		}

		// 更新触点位置
		if (Input.touchCount == 0)
		{
			foreach (var item in mTouchPointList.startForeach())
			{
				// 此处只更新鼠标的位置,因为touchCount为0时,mTouchPointList种也可能存在这一帧抬起的还未来得及移除的触摸屏的触点
				if (!item.Value.isMouse())
				{
					continue;
				}
				item.Value.update(Input.mousePosition);
			}
		}
		else
		{
			// 先将触点信息放入字典中,方便查询,虽然一般情况下触点都不会超过2个
			LIST(out Dictionary<int, Touch> touchInfoList);
			for (int i = 0; i < Input.touchCount; ++i)
			{
				Touch touch = Input.GetTouch(i);
				touchInfoList.Add(touch.fingerId, touch);
			}
			// 找到指定ID的触点信息,获取触点的位置
			foreach (var item in mTouchPointList.startForeach())
			{
				if (touchInfoList.TryGetValue(item.Key, out Touch touch))
				{
					item.Value.update(touch.position);
				}
				// 触点已经不存在了,应该不会出现这种情况,只是为了保险
				else
				{
					item.Value.update(item.Value.getCurPosition());
				}
			}
			UN_LIST(touchInfoList);
		}

		// 判断是否有触点已经过期
		DateTime now = DateTime.Now;
		var deadList = mLastClickList.startForeach();
		int deadCount = deadList.Count;
		for (int i = 0; i < deadCount; ++i)
		{
			if ((now - deadList[i].mClickTime).TotalSeconds > 1.0f)
			{
				mLastClickList.removeAt(i);
			}
		}

		bool inputting = false;
		foreach (var item in mInputFieldList)
		{
			if (item.isVisible() && item.isFocused())
			{
				inputting = true;
				break;
			}
		}
		setMask(inputting ? FOCUS_MASK.UI : FOCUS_MASK.SCENE);

		COMBINATION_KEY curCombination = COMBINATION_KEY.NONE;
		curCombination |= isAnyKeyDown(KeyCode.LeftControl, KeyCode.RightControl) ? COMBINATION_KEY.CTRL : COMBINATION_KEY.NONE;
		curCombination |= isAnyKeyDown(KeyCode.LeftShift, KeyCode.RightShift) ? COMBINATION_KEY.SHIFT : COMBINATION_KEY.NONE;
		curCombination |= isAnyKeyDown(KeyCode.LeftAlt, KeyCode.RightAlt) ? COMBINATION_KEY.ALT : COMBINATION_KEY.NONE;
		// 遍历监听列表,发送监听事件
		foreach (var item in mKeyListenList.startForeach())
		{
			if (!isKeyCurrentDown(item.Key))
			{
				continue;
			}
			var callbackList = item.Value.startForeach();
			int callbackCount = callbackList.Count;
			for (int i = 0; i < callbackCount; ++i)
			{
				KeyInfo info = callbackList[i];
				if (info.mCombinationKey == curCombination)
				{
					info.mCallback?.Invoke();
				}
			}
		}
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		// 销毁已经不存在的触点
		foreach (var item in mTouchPointList.startForeach())
		{
			if (item.Value.isCurrentUp())
			{
				// 只放入单击的行为,双击行为不再放入,否则会导致双击检测错误
				if (item.Value.isClick() && !item.Value.isDoubleClick())
				{
					mLastClickList.add(new DeadClick(item.Value.getCurPosition()));
				}
				// 不会移除鼠标的触点,因为始终都会有一个鼠标触点
				if (!item.Value.isMouse())
				{
					mTouchPointList.remove(item.Key);
					continue;
				}
			}
			item.Value.lateUpdate();
		}
	}
	public void registeInputField(IInputField inputField) { mInputFieldList.Add(inputField); }
	public void unregisteInputField(IInputField inputField) { mInputFieldList.Remove(inputField); }
	public void setMask(FOCUS_MASK mask) { mFocusMask = (int)mask; }
	public bool hasMask(FOCUS_MASK mask) { return mask == FOCUS_MASK.NONE || mFocusMask == 0 || (mFocusMask & (int)mask) != 0; }
	// 添加对于指定按键的当前按下事件监听
	public void listenKeyCurrentDown(KeyCode key, OnKeyCurrentDown callback, object listener, COMBINATION_KEY combination = COMBINATION_KEY.NONE)
	{
		var info = new KeyInfo();
		info.mCallback = callback;
		info.mListener = listener;
		info.mKey = key;
		info.mCombinationKey = combination;
		if (!mKeyListenList.tryGetValue(key, out SafeList<KeyInfo> list))
		{
			list = new SafeList<KeyInfo>();
			mKeyListenList.add(key, list);
		}
		list.add(info);
		if (!mListenerList.TryGetValue(listener, out Dictionary<OnKeyCurrentDown, KeyInfo> callbcakList))
		{
			callbcakList = new Dictionary<OnKeyCurrentDown, KeyInfo>();
			mListenerList.Add(listener, callbcakList);
		}
		if (!callbcakList.ContainsKey(callback))
		{
			callbcakList.Add(callback, info);
		}
	}
	// 移除监听者的所有按键监听
	public void unlistenKey(object listener)
	{
		if (!mListenerList.TryGetValue(listener, out Dictionary<OnKeyCurrentDown, KeyInfo> list))
		{
			return;
		}
		foreach (var item in list)
		{
			mKeyListenList.tryGetValue(item.Value.mKey, out SafeList<KeyInfo> callbackList);
			if (callbackList == null)
			{
				continue;
			}
			var callbacks = callbackList.startForeach();
			int count = callbacks.Count;
			for (int i = 0; i < count; ++i)
			{
				if (item.Key == callbacks[i].mCallback)
				{
					callbackList.removeAt(i);
				}
			}
		}
		mListenerList.Remove(listener);
	}
	// 是否有任意触点在这一帧按下,如果有,则返回第一个在这一帧按下的触点
	public bool getTouchDown(out TouchPoint touchPoint)
	{
		touchPoint = null;
		foreach (var item in mTouchPointList.startForeach())
		{
			if (item.Value.isCurrentDown())
			{
				touchPoint = item.Value;
				return true;
			}
		}
		return false;
	}
	// 是否有任意触点在这一帧完成一次点击操作,如果有,则返回第一个在这一帧完成点击的触点
	public TouchPoint getTouchClick()
	{
		foreach (var item in mTouchPointList.startForeach())
		{
			if (item.Value.isClick())
			{
				return item.Value;
			}
		}
		return null;
	}
	// 是否有任意触点在这一帧完成一次双击操作,如果有,则返回第一个在这一帧完成双击的触点
	public TouchPoint isTouchDoubleClick()
	{
		foreach (var item in mTouchPointList.startForeach())
		{
			if (item.Value.isDoubleClick())
			{
				return item.Value;
			}
		}
		return null;
	}
	// 指定触点是否处于持续按下状态
	public bool isTouchKeepDown(int pointerID)
	{
		if (!mTouchPointList.tryGetValue(pointerID, out TouchPoint point))
		{
			return false;
		}
		// 只要不是这一帧抬起和按下的,都是处于持续按下状态
		return !point.isCurrentUp() && !point.isCurrentDown();
	}
	// 指定触点是否在这一帧抬起
	public bool isTouchUp(int pointerID)
	{
		if (!mTouchPointList.tryGetValue(pointerID, out TouchPoint point))
		{
			return false;
		}
		return point.isCurrentUp();
	}
	public TouchPoint getTouchPoint(int pointerID) 
	{
		mTouchPointList.tryGetValue(pointerID, out TouchPoint point);
		return point;
	}
	// 外部可以通过获取点击操作列表,获取到这一帧的所有点击操作信息,且不分平台,统一移动端触屏和桌面端鼠标操作(未考虑桌面端的触屏)
	public SafeDictionary<int, TouchPoint> getTouchPointList() { return mTouchPointList; }
	public void setMouseVisible(bool visible) { Cursor.visible = visible; }
#if UNITY_EDITOR || UNITY_STANDALONE
	public float getMouseWheelDelta() { return Input.mouseScrollDelta.y; }
	public bool isMouseRightDown()
	{
		return isMouseRightKeepDown() || isMouseRightCurrentDown();
	}
	public bool isMouseMiddleDown()
	{
		return isMouseMiddleKeepDown() || isMouseMiddleCurrentDown();
	}
	public bool isMouseRightKeepDown()
	{
		return Input.GetMouseButton((int)MOUSE_BUTTON.RIGHT);
	}
	public bool isMouseMiddleKeepDown()
	{
		return Input.GetMouseButton((int)MOUSE_BUTTON.MIDDLE);
	}
	public bool isMouseRightCurrentDown()
	{
		return Input.GetMouseButtonDown((int)MOUSE_BUTTON.RIGHT);
	}
	public bool isMouseMiddleCurrentDown()
	{
		return Input.GetMouseButtonDown((int)MOUSE_BUTTON.MIDDLE);
	}
	public bool isMouseRightCurrentUp()
	{
		return Input.GetMouseButtonUp((int)MOUSE_BUTTON.RIGHT);
	}
	public bool isMouseMiddleCurrentUp()
	{
		return Input.GetMouseButtonUp((int)MOUSE_BUTTON.MIDDLE);
	}
#endif
	// mask表示要检测的输入类型,是UI输入框输入,还是场景全局输入,或者是其他情况下的输入
	public bool isAnyKeyDown(KeyCode key0, KeyCode key1, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return isKeyDown(key0, mask) || isKeyDown(key1, mask);
	}
	public new bool isKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return Input.GetKeyDown(key) && hasMask(mask);
	}
	public new bool isKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return Input.GetKeyUp(key) && hasMask(mask);
	}
	public new bool isKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return (Input.GetKeyDown(key) || Input.GetKey(key)) && hasMask(mask);
	}
	public new bool isKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return (Input.GetKeyUp(key) || !Input.GetKey(key)) && hasMask(mask);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void pointDown(int pointerID, Vector3 position)
	{
		if (!mTouchPointList.tryGetValue(pointerID, out TouchPoint point))
		{
			point = addTouch(pointerID, false);
		}
		point.pointDown(position);
	}
	protected void pointUp(int pointerID, Vector3 position)
	{
		if (!mTouchPointList.tryGetValue(pointerID, out TouchPoint point))
		{
			return;
		}
		point.pointUp(position, mLastClickList.getMainList());
	}
	protected TouchPoint addTouch(int pointerID, bool isMouse)
	{
		var point = new TouchPoint();
		point.setMouse(isMouse);
		point.setTouchID(pointerID);
		mTouchPointList.add(point.getTouchID(), point);
		return point;
	}
}