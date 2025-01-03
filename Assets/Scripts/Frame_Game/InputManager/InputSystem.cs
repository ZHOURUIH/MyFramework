using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameEditorUtility;

// 输入系统,用于封装Input
public class InputSystem : FrameSystem
{
	protected SafeDictionary<KeyCode, SafeList<KeyInfo>> mKeyListenList = new();                // 按键按下的监听回调列表
	protected SafeDictionary<int, TouchPoint> mTouchPointList = new();  // 触点信息列表,可能与GlobalTouchSystem有一定的重合
	protected Dictionary<KeyCode, string> mKeyNameList = new();         // 快捷键名字列表
	protected SafeList<DeadClick> mLastClickList = new();               // 已经完成单击的行为,用于实现双击的功能
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
				if (!item.isMouse())
				{
					continue;
				}
				item.update(Input.mousePosition);
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

		// 检测这一帧按下过哪些按键
		mCurKeyDownList.Clear();
		mCurKeyUpList.Clear();
		foreach (KeyCode item in mKeyNameList.Keys)
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
		using var d = new SafeDictionaryReader<KeyCode, SafeList<KeyInfo>>(mKeyListenList);
		foreach (var item in d.mReadList)
		{
			if (!isKeyCurrentDown(item.Key))
			{
				continue;
			}
			using var e = new SafeListReader<KeyInfo>(item.Value);
			foreach (KeyInfo info in e.mReadList)
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
				if (touchPoint.isClick())
				{
					mLastClickList.add(new(touchPoint.getCurPosition()));
				}
				// 不会移除鼠标的触点,因为始终都会有一个鼠标触点
				if (!touchPoint.isMouse())
				{
					mTouchPointList.remove(item.Key);
					continue;
				}
			}
			touchPoint.lateUpdate();
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
	public TouchPoint getTouchPoint(int pointerID) 
	{
		mTouchPointList.tryGetValue(pointerID, out TouchPoint point);
		return point;
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
	public bool isAnyKeyDown(KeyCode key0, KeyCode key1) { return isKeyDown(key0) || isKeyDown(key1); }
	public bool isKeyCurrentDown(KeyCode key) { return mEnableKey && Input.GetKeyDown(key); }
	public bool isKeyDown(KeyCode key) { return mEnableKey && (Input.GetKeyDown(key) || Input.GetKey(key)); }
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
		TouchPoint point = new();
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