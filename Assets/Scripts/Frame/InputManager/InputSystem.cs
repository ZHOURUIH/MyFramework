using UnityEngine;
using System.Collections.Generic;
using System;
using static UnityUtility;
using static StringUtility;

public struct KeyMapping
{
	public int mCustomKey;
	public KeyCode mKey;
	public string mCustomKeyName;
	public KeyMapping(int customKey, KeyCode key, string customKeyName)
	{
		mCustomKey = customKey;
		mKey = key;
		mCustomKeyName = customKeyName;
	}
}

// 输入系统,用于封装Input
public class InputSystem : FrameSystem
{
	protected Dictionary<object, Dictionary<OnKeyCurrentDown, KeyInfo>> mListenerList;      // 以监听者为索引的快捷键监听列表
	protected SafeDictionary<KeyCode, SafeList<KeyInfo>> mKeyListenList;                    // 按键按下的监听回调列表
	protected SafeDictionary<int, TouchPoint> mTouchPointList;  // 触点信息列表,可能与GlobalTouchSystem有一定的重合
	protected Dictionary<KeyCode, string> mKeyNameList;         // 快捷键名字列表
	protected Dictionary<int, KeyMapping> mDefaultKeyMapping;	// 默认的按键映射表,用于游戏恢复默认快捷键设置时使用,需要手动备份默认映射表
	protected Dictionary<int, KeyMapping> mKeyMapping;			// 按键映射表,key是自定义的一个整数,用于当作具体功能的枚举,value是对应的按键
	protected HashSet<IInputField> mInputFieldList;				// 输入框列表,用于判断当前是否正在输入
	protected SafeList<DeadClick> mLastClickList;               // 已经完成单击的行为,用于实现双击的功能
	protected List<KeyCode> mCurKeyDownList;					// 这一帧按下的按键列表,每帧都会清空一次,用于在其他地方获取这一帧按下了哪些按键
	protected List<KeyCode> mCurKeyUpList;						// 这一帧抬起的按键列表,每帧都会清空一次,用于在其他地方获取这一帧按下了哪些按键
	protected int mFocusMask;                                   // 当前的输入掩码,是输入框的输入还是快捷键输入
	protected bool mEnableKey;									// 是否启用按键的响应
	public InputSystem()
	{
		mListenerList = new Dictionary<object, Dictionary<OnKeyCurrentDown, KeyInfo>>();
		mKeyListenList = new SafeDictionary<KeyCode, SafeList<KeyInfo>>();
		mTouchPointList = new SafeDictionary<int, TouchPoint>();
		mKeyNameList = new Dictionary<KeyCode, string>();
		mDefaultKeyMapping = new Dictionary<int, KeyMapping>();
		mKeyMapping = new Dictionary<int, KeyMapping>();
		mInputFieldList = new HashSet<IInputField>();
		mLastClickList = new SafeList<DeadClick>();
		mCurKeyDownList = new List<KeyCode>();
		mCurKeyUpList = new List<KeyCode>();
		mEnableKey = true;
	}
	public override void init()
	{
		base.init();
		initKey();
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
			foreach (var item in mTouchPointList.startForeach("InputSystem:107"))
			{
				// 此处只更新鼠标的位置,因为touchCount为0时,mTouchPointList种也可能存在这一帧抬起的还未来得及移除的触摸屏的触点
				if (!item.Value.isMouse())
				{
					continue;
				}
				item.Value.update(Input.mousePosition);
			}
			mTouchPointList.endForeach();
		}
		else
		{
			// 先将触点信息放入字典中,方便查询,虽然一般情况下触点都不会超过2个
			using (new DicScope<int, Touch>(out var touchInfoList))
			{
				for (int i = 0; i < Input.touchCount; ++i)
				{
					Touch touch = Input.GetTouch(i);
					touchInfoList.Add(touch.fingerId, touch);
				}
				// 找到指定ID的触点信息,获取触点的位置
				foreach (var item in mTouchPointList.startForeach("InputSystem:129"))
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
				mTouchPointList.endForeach();
			}
		}

		// 判断是否有触点已经过期
		DateTime now = DateTime.Now;
		var deadList = mLastClickList.startForeach("InputSystem:147");
		int deadCount = deadList.Count;
		for (int i = 0; i < deadCount; ++i)
		{
			if ((now - deadList[i].mClickTime).TotalSeconds > 1.0f)
			{
				mLastClickList.removeAt(i);
			}
		}
		mLastClickList.endForeach();

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

		// 检测这一帧按下过哪些按键
		mCurKeyDownList.Clear();
		mCurKeyUpList.Clear();
		foreach (var item in mKeyNameList)
		{
			if (Input.GetKeyDown(item.Key))
			{
				mCurKeyDownList.Add(item.Key);
			}
			else if (Input.GetKeyUp(item.Key))
			{
				mCurKeyUpList.Add(item.Key);
			}
		}

		// 遍历监听列表,发送监听事件
		COMBINATION_KEY curCombination = COMBINATION_KEY.NONE;
		curCombination |= isAnyKeyDown(KeyCode.LeftControl, KeyCode.RightControl) ? COMBINATION_KEY.CTRL : COMBINATION_KEY.NONE;
		curCombination |= isAnyKeyDown(KeyCode.LeftShift, KeyCode.RightShift) ? COMBINATION_KEY.SHIFT : COMBINATION_KEY.NONE;
		curCombination |= isAnyKeyDown(KeyCode.LeftAlt, KeyCode.RightAlt) ? COMBINATION_KEY.ALT : COMBINATION_KEY.NONE;
		foreach (var item in mKeyListenList.startForeach("InputSystem:189"))
		{
			try
			{
				if (!isKeyCurrentDown(item.Key))
				{
					continue;
				}
				var callbackList = item.Value.startForeach("InputSystem:195");
				int callbackCount = callbackList.Count;
				for (int i = 0; i < callbackCount; ++i)
				{
					KeyInfo info = callbackList[i];
					if (info.mCombinationKey == curCombination)
					{
						info.mCallback?.Invoke();
					}
				}
				item.Value.endForeach();
			}
			catch(Exception e)
			{
				logException(e);
			}
		}
		mKeyListenList.endForeach();
	}
	public override void lateUpdate(float elapsedTime)
	{
		base.lateUpdate(elapsedTime);
		// 销毁已经不存在的触点
		foreach (var item in mTouchPointList.startForeach("InputSystem:213"))
		{
			try
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
			catch(Exception e)
			{
				logException(e);
			}
		}
		mTouchPointList.endForeach();
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
			var callbacks = callbackList.getMainList();
			for (int i = 0; i < callbacks.Count; ++i)
			{
				if (item.Key == callbacks[i].mCallback)
				{
					callbackList.removeAt(i--);
				}
			}
		}
		mListenerList.Remove(listener);
	}
	// 是否有任意触点在这一帧按下,如果有,则返回第一个在这一帧按下的触点
	public bool getTouchDown(out TouchPoint touchPoint)
	{
		touchPoint = null;
		foreach (var item in mTouchPointList.getMainList())
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
		foreach (var item in mTouchPointList.getMainList())
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
		foreach (var item in mTouchPointList.getMainList())
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
	public bool isMouseRightDown() { return isMouseRightKeepDown() || isMouseRightCurrentDown(); }
	public bool isMouseMiddleDown() { return isMouseMiddleKeepDown() || isMouseMiddleCurrentDown(); }
	public bool isMouseRightKeepDown() { return Input.GetMouseButton((int)MOUSE_BUTTON.RIGHT); }
	public bool isMouseMiddleKeepDown() { return Input.GetMouseButton((int)MOUSE_BUTTON.MIDDLE); }
	public bool isMouseRightCurrentDown() { return Input.GetMouseButtonDown((int)MOUSE_BUTTON.RIGHT); }
	public bool isMouseMiddleCurrentDown() { return Input.GetMouseButtonDown((int)MOUSE_BUTTON.MIDDLE); }
	public bool isMouseRightCurrentUp() { return Input.GetMouseButtonUp((int)MOUSE_BUTTON.RIGHT); }
	public bool isMouseMiddleCurrentUp() { return Input.GetMouseButtonUp((int)MOUSE_BUTTON.MIDDLE); }
#endif
	// mask表示要检测的输入类型,是UI输入框输入,还是场景全局输入,或者是其他情况下的输入
	public bool isAnyKeyDown(KeyCode key0, KeyCode key1, FOCUS_MASK mask = FOCUS_MASK.NONE) { return isKeyDown(key0, mask) || isKeyDown(key1, mask); }
	public bool isKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mEnableKey && Input.GetKeyDown(key) && hasMask(mask); }
	public bool isKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mEnableKey && Input.GetKeyUp(key) && hasMask(mask); }
	public bool isKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mEnableKey && (Input.GetKeyDown(key) || Input.GetKey(key)) && hasMask(mask); }
	public bool isKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mEnableKey && (Input.GetKeyUp(key) || !Input.GetKey(key)) && hasMask(mask); }
	public void setEnableKey(bool enable) { mEnableKey = enable; }
	public bool isEnableKey() { return mEnableKey; }
	// 备份默认的映射表
	public void makeDefaultKeyMappingList()
	{
		mDefaultKeyMapping.Clear();
		foreach (var item in mKeyMapping)
		{
			mDefaultKeyMapping.Add(item.Key, item.Value);
		}
	}
	// 恢复到默认的映射表
	public void restoreDefaultKeyMapping()
	{
		mKeyMapping.Clear();
		foreach (var item in mDefaultKeyMapping)
		{
			mKeyMapping.Add(item.Key, item.Value);
		}
	}
	public List<KeyCode> getCurKeyDownList() { return mCurKeyDownList; }
	public List<KeyCode> getCurKeyUpList() { return mCurKeyUpList; }
	public Dictionary<int, KeyMapping> getKeyMappingList() { return mKeyMapping; }
	public string getKeyMappingName(int keyMapping) { return getKeyName(getKeyMapping(keyMapping)); }
	public string getKeyName(KeyCode key)
	{
		if (mKeyNameList.TryGetValue(key, out string name))
		{
			return name;
		}
		return EMPTY;
	}
	public void setKeyMapping(int mapping, KeyCode key, string actionName = null)
	{
		KeyMapping mappingInfo = new KeyMapping();
		mappingInfo.mCustomKey = mapping;
		mappingInfo.mKey = key;
		mappingInfo.mCustomKeyName = actionName;
		mKeyMapping[mapping] = mappingInfo;
	}
	public string getKeyMappingActionName(int mapping)
	{
		if (mKeyMapping.TryGetValue(mapping, out KeyMapping info))
		{
			return info.mCustomKeyName;
		}
		return EMPTY;
	}
	public KeyCode getKeyMapping(int mapping)
	{
		if (mKeyMapping.TryGetValue(mapping, out KeyMapping info))
		{
			return info.mKey;
		}
		return KeyCode.None;
	}
	public bool isKeyCurrentDown(int mapping)
	{
		KeyCode key = getKeyMapping(mapping);
		if (key == KeyCode.None)
		{
			return false;
		}
		return isKeyCurrentDown(key, FOCUS_MASK.SCENE);
	}
	public bool isKeyDown(int mapping)
	{
		KeyCode key = getKeyMapping(mapping);
		if (key == KeyCode.None)
		{
			return false;
		}
		return isKeyDown(key, FOCUS_MASK.SCENE);
	}
	public KeyCode getDefaultMappingKey(int mapping)
	{
		mDefaultKeyMapping.TryGetValue(mapping, out KeyMapping key);
		return key.mKey;
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
	protected void initKey()
	{
		initSingleKey(KeyCode.A, "A");
		initSingleKey(KeyCode.B, "B");
		initSingleKey(KeyCode.C, "C");
		initSingleKey(KeyCode.D, "D");
		initSingleKey(KeyCode.E, "E");
		initSingleKey(KeyCode.F, "F");
		initSingleKey(KeyCode.G, "G");
		initSingleKey(KeyCode.H, "H");
		initSingleKey(KeyCode.I, "I");
		initSingleKey(KeyCode.J, "J");
		initSingleKey(KeyCode.K, "K");
		initSingleKey(KeyCode.L, "L");
		initSingleKey(KeyCode.M, "M");
		initSingleKey(KeyCode.N, "N");
		initSingleKey(KeyCode.O, "O");
		initSingleKey(KeyCode.P, "P");
		initSingleKey(KeyCode.Q, "Q");
		initSingleKey(KeyCode.R, "R");
		initSingleKey(KeyCode.S, "S");
		initSingleKey(KeyCode.T, "T");
		initSingleKey(KeyCode.U, "U");
		initSingleKey(KeyCode.V, "V");
		initSingleKey(KeyCode.W, "W");
		initSingleKey(KeyCode.X, "X");
		initSingleKey(KeyCode.Y, "Y");
		initSingleKey(KeyCode.Z, "Z");
		initSingleKey(KeyCode.Keypad0, "Num0");
		initSingleKey(KeyCode.Keypad1, "Num1");
		initSingleKey(KeyCode.Keypad2, "Num2");
		initSingleKey(KeyCode.Keypad3, "Num3");
		initSingleKey(KeyCode.Keypad4, "Num4");
		initSingleKey(KeyCode.Keypad5, "Num5");
		initSingleKey(KeyCode.Keypad6, "Num6");
		initSingleKey(KeyCode.Keypad7, "Num7");
		initSingleKey(KeyCode.Keypad8, "Num8");
		initSingleKey(KeyCode.Keypad9, "Num9");
		initSingleKey(KeyCode.KeypadPeriod, "Num.");
		initSingleKey(KeyCode.KeypadDivide, "Num/");
		initSingleKey(KeyCode.KeypadMultiply, "Num*");
		initSingleKey(KeyCode.KeypadMinus, "Num-");
		initSingleKey(KeyCode.KeypadPlus, "Num+");
		initSingleKey(KeyCode.Alpha0, "0");
		initSingleKey(KeyCode.Alpha1, "1");
		initSingleKey(KeyCode.Alpha2, "2");
		initSingleKey(KeyCode.Alpha3, "3");
		initSingleKey(KeyCode.Alpha4, "4");
		initSingleKey(KeyCode.Alpha5, "5");
		initSingleKey(KeyCode.Alpha6, "6");
		initSingleKey(KeyCode.Alpha7, "7");
		initSingleKey(KeyCode.Alpha8, "8");
		initSingleKey(KeyCode.Alpha9, "9");
		initSingleKey(KeyCode.F1, "F1");
		initSingleKey(KeyCode.F2, "F2");
		initSingleKey(KeyCode.F3, "F3");
		initSingleKey(KeyCode.F4, "F4");
		initSingleKey(KeyCode.F5, "F5");
		initSingleKey(KeyCode.F6, "F6");
		initSingleKey(KeyCode.F7, "F7");
		initSingleKey(KeyCode.F8, "F8");
		initSingleKey(KeyCode.F9, "F9");
		initSingleKey(KeyCode.F10, "F10");
		initSingleKey(KeyCode.F11, "F11");
		initSingleKey(KeyCode.F12, "F12");
		initSingleKey(KeyCode.Equals, "=");
		initSingleKey(KeyCode.Minus, "-");
		initSingleKey(KeyCode.LeftBracket, "[");
		initSingleKey(KeyCode.RightBracket, "]");
		initSingleKey(KeyCode.Backslash, "\\");
		initSingleKey(KeyCode.Semicolon, ";");
		initSingleKey(KeyCode.Quote, "'");
		initSingleKey(KeyCode.Comma, ",");
		initSingleKey(KeyCode.Period, ".");
		initSingleKey(KeyCode.Slash, "/");
		initSingleKey(KeyCode.BackQuote, "`");
		initSingleKey(KeyCode.Backspace, "Back");
		initSingleKey(KeyCode.Insert, "Insert");
		initSingleKey(KeyCode.Delete, "Del");
		initSingleKey(KeyCode.Home, "Home");
		initSingleKey(KeyCode.End, "End");
		initSingleKey(KeyCode.PageUp, "PgUp");
		initSingleKey(KeyCode.PageDown, "PgDn");
		initSingleKey(KeyCode.Tab, "Tab");
		initSingleKey(KeyCode.UpArrow, "↑");
		initSingleKey(KeyCode.DownArrow, "↓");
		initSingleKey(KeyCode.LeftArrow, "←");
		initSingleKey(KeyCode.RightArrow, "→");
	}
	protected void initSingleKey(KeyCode key, string name)
	{
		mKeyNameList.Add(key, name);
	}
}