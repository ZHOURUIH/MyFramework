using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;
using static FrameUtility;

// 输入系统,用于封装Input
public class InputSystem : FrameSystem
{
	protected Dictionary<IEventListener, Dictionary<Action, KeyListenInfo>> mListenerList = new();		// 以监听者为索引的快捷键监听列表
	protected SafeDictionary<KeyCode, SafeList<KeyListenInfo>> mKeyListenList = new();					// 按键按下的监听回调列表
	protected SafeDictionary<int, TouchPoint> mTouchPointList = new();  // 触点信息列表,可能与GlobalTouchSystem有一定的重合
	protected HashSet<IInputField> mInputFieldList = new();				// 输入框列表,用于判断当前是否正在输入
	protected SafeList<DeadClick> mLastClickList = new();               // 已经完成单击的行为,用于实现双击的功能
	protected List<KeyCode> mAllKeys = new();                           // 所有支持的按键列表
	protected List<KeyCode> mCurKeyDownList = new();					// 这一帧按下的按键列表,每帧都会清空一次,用于在其他地方获取这一帧按下了哪些按键
	protected List<KeyCode> mCurKeyUpList = new();						// 这一帧抬起的按键列表,每帧都会清空一次,用于在其他地方获取这一帧按下了哪些按键
	protected int mFocusMask;											// 当前的输入掩码,是输入框的输入还是快捷键输入
	protected bool mEnableKey = true;									// 是否启用按键的响应
	protected bool mActiveInput = true;									// 是否检测输入
	public override void init()
	{
		base.init();
		initKey();
		// 编辑器或者桌面端,默认会有鼠标三个键的触点
		if (isEditor() || isStandalone())
		{
			addTouch((int)MOUSE_BUTTON.LEFT, true);
			addTouch((int)MOUSE_BUTTON.RIGHT, true);
			addTouch((int)MOUSE_BUTTON.MIDDLE, true);
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if(!mActiveInput)
		{
			return;
		}
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
			if (isEditor() || isStandalone())
			{
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
			}
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
			using var a = new SafeDictionaryReader<int, TouchPoint>(mTouchPointList);
			foreach (TouchPoint item in a.mReadList.Values)
			{
				// 此处只更新鼠标的位置,因为touchCount为0时,mTouchPointList种也可能存在这一帧抬起的还未来得及移除的触摸屏的触点
				// webgl上也可能在鼠标按下时读取不到触点数量,所以触点数量为0而且又检测到了鼠标按下而添加信息到mTouchPointList,也使用鼠标位置来更新触点
				if (item.isMouse() || isWebGL())
				{
					item.update(Input.mousePosition);
				}
			}
		}
		else
		{
			// 先将触点信息放入字典中,方便查询,虽然一般情况下触点都不会超过2个
			using var a = new DicScope<int, Touch>(out var touchInfoList);
			for (int i = 0; i < Input.touchCount; ++i)
			{
				Touch touch = Input.GetTouch(i);
				touchInfoList.Add(touch.fingerId, touch);
			}
			// 找到指定ID的触点信息,获取触点的位置
			using var b = new SafeDictionaryReader<int, TouchPoint>(mTouchPointList);
			foreach (var item in b.mReadList)
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
		}

		// 判断是否有触点已经过期
		DateTime now = DateTime.Now;
		using var c = new SafeListReader<DeadClick>(mLastClickList);
		int deadCount = c.mReadList.Count;
		for (int i = 0; i < deadCount; ++i)
		{
			if ((now - c.mReadList[i].mClickTime).TotalSeconds > 1.0f)
			{
				mLastClickList.removeAt(i);
			}
		}

		bool inputting = false;
		foreach (IInputField item in mInputFieldList)
		{
			if (item.isVisible() && item.isFocused())
			{
				inputting = true;
				break;
			}
		}
		setMask(inputting ? FOCUS_MASK.UI : FOCUS_MASK.SCENE);

		// 检测这一帧按下过哪些按键
		mCurKeyDownList.Clear();
		mCurKeyUpList.Clear();
		foreach (KeyCode item in mAllKeys)
		{
			if (Input.GetKeyDown(item))
			{
				mCurKeyDownList.Add(item);
			}
			else if (Input.GetKeyUp(item))
			{
				mCurKeyUpList.Add(item);
			}
		}
		
		// 遍历监听列表,发送监听事件
		COMBINATION_KEY curCombination = COMBINATION_KEY.NONE;
		curCombination |= isAnyKeyDown(KeyCode.LeftControl, KeyCode.RightControl) ? COMBINATION_KEY.CTRL : COMBINATION_KEY.NONE;
		curCombination |= isAnyKeyDown(KeyCode.LeftShift, KeyCode.RightShift) ? COMBINATION_KEY.SHIFT : COMBINATION_KEY.NONE;
		curCombination |= isAnyKeyDown(KeyCode.LeftAlt, KeyCode.RightAlt) ? COMBINATION_KEY.ALT : COMBINATION_KEY.NONE;
		using var d = new SafeDictionaryReader<KeyCode, SafeList<KeyListenInfo>>(mKeyListenList);
		foreach (var item in d.mReadList)
		{
			if (!isKeyCurrentDown(item.Key))
			{
				continue;
			}
			using var e = new SafeListReader<KeyListenInfo>(item.Value);
			foreach (KeyListenInfo info in e.mReadList)
			{
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
		using var b = new SafeDictionaryReader<int, TouchPoint>(mTouchPointList);
		foreach (var item in b.mReadList)
		{
			TouchPoint touchPoint = item.Value;
			if (touchPoint.isCurrentUp())
			{
				// 只放入单击的行为,双击行为不再放入,否则会导致双击检测错误
				if (touchPoint.isClick() && !touchPoint.isDoubleClick())
				{
					mLastClickList.add(new(touchPoint.getCurPosition()));
				}
				// 不会移除鼠标的触点,因为始终都会有一个鼠标触点
				if (!touchPoint.isMouse())
				{
					mTouchPointList.remove(item.Key);
					UN_CLASS(ref touchPoint);
					continue;
				}
			}
			touchPoint.lateUpdate();
		}
	}
	public void registeInputField(IInputField inputField) { mInputFieldList.Add(inputField); }
	public void unregisteInputField(IInputField inputField) { mInputFieldList.Remove(inputField); }
	public void setMask(FOCUS_MASK mask) { mFocusMask = (int)mask; }
	public bool hasMask(FOCUS_MASK mask) { return mask == FOCUS_MASK.NONE || mFocusMask == 0 || (mFocusMask & (int)mask) != 0; }
	// 添加对于指定按键的当前按下事件监听,一般用于一些不重要的临时逻辑,如果是游戏允许自定义的快捷键,需要使用KeyMappingSystem来映射
	public void listenKeyCurrentDown(KeyCode key, Action callback, IEventListener listener, COMBINATION_KEY combination = COMBINATION_KEY.NONE)
	{
		CLASS(out KeyListenInfo info);
		info.mCallback = callback;
		info.mListener = listener;
		info.mKey = key;
		info.mCombinationKey = combination;
		if (!mKeyListenList.tryGetValue(key, out var list))
		{
			list = new();
			mKeyListenList.add(key, list);
		}
		list.add(info);
		mListenerList.getOrAddNew(listener).TryAdd(callback, info);
	}
	// 移除监听者的所有按键监听
	public void unlistenKey(IEventListener listener)
	{
		if (!mListenerList.Remove(listener, out var list))
		{
			return;
		}
		foreach (var item in list)
		{
			mKeyListenList.tryGetValue(item.Value.mKey, out var callbackList);
			if (callbackList == null)
			{
				continue;
			}
			var callbacks = callbackList.getMainList();
			for (int i = 0; i < callbacks.Count; ++i)
			{
				if (item.Key == callbacks[i].mCallback)
				{
					UN_CLASS(callbackList.get(i));
					callbackList.removeAt(i--);
				}
			}
		}
	}
	// 是否有任意触点在这一帧按下,如果有,则返回第一个在这一帧按下的触点
	public bool getTouchDown(out TouchPoint touchPoint)
	{
		touchPoint = null;
		foreach (TouchPoint item in mTouchPointList.getMainList().Values)
		{
			if (item.isCurrentDown())
			{
				touchPoint = item;
				return true;
			}
		}
		return false;
	}
	// 是否有任意触点在这一帧完成一次点击操作,如果有,则返回第一个在这一帧完成点击的触点
	public TouchPoint getTouchClick()
	{
		foreach (TouchPoint item in mTouchPointList.getMainList().Values)
		{
			if (item.isClick())
			{
				return item;
			}
		}
		return null;
	}
	// 是否有任意触点在这一帧完成一次双击操作,如果有,则返回第一个在这一帧完成双击的触点
	public TouchPoint isTouchDoubleClick()
	{
		foreach (TouchPoint item in mTouchPointList.getMainList().Values)
		{
			if (item.isDoubleClick())
			{
				return item;
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
		return mTouchPointList.tryGetValue(pointerID, out TouchPoint point) && point.isCurrentUp();
	}
	public TouchPoint getTouchPoint(int pointerID) { return mTouchPointList.get(pointerID); }
	public Vector3 getMouseLeftPosition() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("getMouseLeftPosition仅限编辑器或者桌面端使用");
			return Vector3.zero;
		}
		return getTouchPoint((int)MOUSE_BUTTON.LEFT).getCurPosition();
	}
	public Vector3 getMouseMiddlePosition()
	{
		if (!isEditor() && !isStandalone())
		{
			logError("getMouseLeftPosition仅限编辑器或者桌面端使用");
			return Vector3.zero;
		}
		return getTouchPoint((int)MOUSE_BUTTON.MIDDLE).getCurPosition();
	}
	public Vector3 getMouseRightPosition()
	{
		if (!isEditor() && !isStandalone())
		{
			logError("getMouseLeftPosition仅限编辑器或者桌面端使用");
			return Vector3.zero;
		}
		return getTouchPoint((int)MOUSE_BUTTON.RIGHT).getCurPosition();
	}
	// 外部可以通过获取点击操作列表,获取到这一帧的所有点击操作信息,且不分平台,统一移动端触屏和桌面端鼠标操作(未考虑桌面端的触屏)
	public SafeDictionary<int, TouchPoint> getTouchPointList() { return mTouchPointList; }
	public int getTouchPointDownCount()
	{
		int count = 0;
		foreach (TouchPoint item in mTouchPointList.getMainList().Values)
		{
			if (item.isDown())
			{
				++count;
			}
		}
		return count;
	}
	// 以下鼠标相关函数只能在windows或者编辑器中使用
	public void setMouseVisible(bool visible) 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return;
		}
		Cursor.visible = visible; 
	}
	public float getMouseWheelDelta() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return 0.0f;
		}
		return Input.mouseScrollDelta.y; 
	}
	public float getMouseMoveX() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return 0.0f;
		}
		return Input.GetAxis("Mouse X"); 
	}
	public float getMouseMoveY() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return 0.0f;
		}
		return Input.GetAxis("Mouse Y"); 
	}
	public bool isMouseLeftDown() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return isMouseLeftKeepDown() || isMouseLeftCurrentDown(); 
	}
	public bool isMouseRightDown() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return isMouseRightKeepDown() || isMouseRightCurrentDown(); 
	}
	public bool isMouseMiddleDown() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return isMouseMiddleKeepDown() || isMouseMiddleCurrentDown(); 
	}
	public bool isMouseLeftKeepDown() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return Input.GetMouseButton((int)MOUSE_BUTTON.LEFT); 
	}
	public bool isMouseRightKeepDown() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return Input.GetMouseButton((int)MOUSE_BUTTON.RIGHT); 
	}
	public bool isMouseMiddleKeepDown() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return Input.GetMouseButton((int)MOUSE_BUTTON.MIDDLE); 
	}
	public bool isMouseLeftCurrentDown() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return Input.GetMouseButtonDown((int)MOUSE_BUTTON.LEFT); 
	}
	public bool isMouseRightCurrentDown() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return Input.GetMouseButtonDown((int)MOUSE_BUTTON.RIGHT); 
	}
	public bool isMouseMiddleCurrentDown() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return Input.GetMouseButtonDown((int)MOUSE_BUTTON.MIDDLE); 
	}
	public bool isMouseLeftCurrentUp() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return Input.GetMouseButtonUp((int)MOUSE_BUTTON.LEFT); 
	}
	public bool isMouseRightCurrentUp() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return Input.GetMouseButtonUp((int)MOUSE_BUTTON.RIGHT); 
	}
	public bool isMouseMiddleCurrentUp() 
	{
		if (!isEditor() && !isStandalone())
		{
			logError("只能在编辑器或者桌面平台调用");
			return false;
		}
		return Input.GetMouseButtonUp((int)MOUSE_BUTTON.MIDDLE); 
	}
	// mask表示要检测的输入类型,是UI输入框输入,还是场景全局输入,或者是其他情况下的输入
	public bool isAnyKeyDown(KeyCode key0, KeyCode key1, FOCUS_MASK mask = FOCUS_MASK.SCENE) { return isKeyDown(key0, mask) || isKeyDown(key1, mask); }
	public bool isKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE) { return mEnableKey && Input.GetKeyDown(key) && hasMask(mask); }
	public bool isKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE) { return mEnableKey && Input.GetKeyUp(key) && hasMask(mask); }
	public bool isKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE) { return mEnableKey && (Input.GetKeyDown(key) || Input.GetKey(key)) && hasMask(mask); }
	public bool isKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE) { return mEnableKey && (Input.GetKeyUp(key) || !Input.GetKey(key)) && hasMask(mask); }
	public void setEnableKey(bool enable) { mEnableKey = enable; }
	public bool isEnableKey() { return mEnableKey; }
	public List<KeyCode> getCurKeyDownList() { return mCurKeyDownList; }
	public List<KeyCode> getCurKeyUpList() { return mCurKeyUpList; }
	public bool getActiveInput() { return mActiveInput; }
	public void setActiveInput(bool value)
	{
		mActiveInput = value;
		foreach (var each in mTouchPointList.getMainList().safe())
		{
			each.Value.resetState();
		}
	}
	public bool isSupportKey(KeyCode key) { return mAllKeys.Contains(key); }
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
		CLASS(out TouchPoint point);
		point.setMouse(isMouse);
		point.setTouchID(pointerID);
		mTouchPointList.add(point.getTouchID(), point);
		return point;
	}
	protected void initKey()
	{
		mAllKeys.Add(KeyCode.A);
		mAllKeys.Add(KeyCode.B);
		mAllKeys.Add(KeyCode.C);
		mAllKeys.Add(KeyCode.D);
		mAllKeys.Add(KeyCode.E);
		mAllKeys.Add(KeyCode.F);
		mAllKeys.Add(KeyCode.G);
		mAllKeys.Add(KeyCode.H);
		mAllKeys.Add(KeyCode.I);
		mAllKeys.Add(KeyCode.J);
		mAllKeys.Add(KeyCode.K);
		mAllKeys.Add(KeyCode.L);
		mAllKeys.Add(KeyCode.M);
		mAllKeys.Add(KeyCode.N);
		mAllKeys.Add(KeyCode.O);
		mAllKeys.Add(KeyCode.P);
		mAllKeys.Add(KeyCode.Q);
		mAllKeys.Add(KeyCode.R);
		mAllKeys.Add(KeyCode.S);
		mAllKeys.Add(KeyCode.T);
		mAllKeys.Add(KeyCode.U);
		mAllKeys.Add(KeyCode.V);
		mAllKeys.Add(KeyCode.W);
		mAllKeys.Add(KeyCode.X);
		mAllKeys.Add(KeyCode.Y);
		mAllKeys.Add(KeyCode.Z);
		mAllKeys.Add(KeyCode.Keypad0);
		mAllKeys.Add(KeyCode.Keypad1);
		mAllKeys.Add(KeyCode.Keypad2);
		mAllKeys.Add(KeyCode.Keypad3);
		mAllKeys.Add(KeyCode.Keypad4);
		mAllKeys.Add(KeyCode.Keypad5);
		mAllKeys.Add(KeyCode.Keypad6);
		mAllKeys.Add(KeyCode.Keypad7);
		mAllKeys.Add(KeyCode.Keypad8);
		mAllKeys.Add(KeyCode.Keypad9);
		mAllKeys.Add(KeyCode.KeypadPeriod);
		mAllKeys.Add(KeyCode.KeypadDivide);
		mAllKeys.Add(KeyCode.KeypadMultiply);
		mAllKeys.Add(KeyCode.KeypadMinus);
		mAllKeys.Add(KeyCode.KeypadPlus);
		mAllKeys.Add(KeyCode.Alpha0);
		mAllKeys.Add(KeyCode.Alpha1);
		mAllKeys.Add(KeyCode.Alpha2);
		mAllKeys.Add(KeyCode.Alpha3);
		mAllKeys.Add(KeyCode.Alpha4);
		mAllKeys.Add(KeyCode.Alpha5);
		mAllKeys.Add(KeyCode.Alpha6);
		mAllKeys.Add(KeyCode.Alpha7);
		mAllKeys.Add(KeyCode.Alpha8);
		mAllKeys.Add(KeyCode.Alpha9);
		mAllKeys.Add(KeyCode.F1);
		mAllKeys.Add(KeyCode.F2);
		mAllKeys.Add(KeyCode.F3);
		mAllKeys.Add(KeyCode.F4);
		mAllKeys.Add(KeyCode.F5);
		mAllKeys.Add(KeyCode.F6);
		mAllKeys.Add(KeyCode.F7);
		mAllKeys.Add(KeyCode.F8);
		mAllKeys.Add(KeyCode.F9);
		mAllKeys.Add(KeyCode.F10);
		mAllKeys.Add(KeyCode.F11);
		mAllKeys.Add(KeyCode.F12);
		mAllKeys.Add(KeyCode.Equals);
		mAllKeys.Add(KeyCode.Minus);
		mAllKeys.Add(KeyCode.LeftBracket);
		mAllKeys.Add(KeyCode.RightBracket);
		mAllKeys.Add(KeyCode.Backslash);
		mAllKeys.Add(KeyCode.Semicolon);
		mAllKeys.Add(KeyCode.Quote);
		mAllKeys.Add(KeyCode.Comma);
		mAllKeys.Add(KeyCode.Period);
		mAllKeys.Add(KeyCode.Slash);
		mAllKeys.Add(KeyCode.BackQuote);
		mAllKeys.Add(KeyCode.Backspace);
		mAllKeys.Add(KeyCode.Insert);
		mAllKeys.Add(KeyCode.Delete);
		mAllKeys.Add(KeyCode.Home);
		mAllKeys.Add(KeyCode.End);
		mAllKeys.Add(KeyCode.PageUp);
		mAllKeys.Add(KeyCode.PageDown);
		mAllKeys.Add(KeyCode.Tab);
		mAllKeys.Add(KeyCode.UpArrow);
		mAllKeys.Add(KeyCode.DownArrow);
		mAllKeys.Add(KeyCode.LeftArrow);
		mAllKeys.Add(KeyCode.RightArrow);
		mAllKeys.Add(KeyCode.Space);
		mAllKeys.Add(KeyCode.LeftShift);
		mAllKeys.Add(KeyCode.RightShift);
		mAllKeys.Add(KeyCode.LeftControl);
		mAllKeys.Add(KeyCode.RightControl);
	}
}