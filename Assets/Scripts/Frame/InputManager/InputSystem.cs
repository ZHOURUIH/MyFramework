using UnityEngine;
using System.Collections.Generic;
using System;

// 输入系统,用于封装Input
public class InputSystem : FrameSystem
{
	protected Dictionary<object, Dictionary<OnKeyCurrentDown, KeyInfo>> mListenerList;		// 以监听者为索引的快捷键监听列表
	protected SafeDictionary<KeyCode, SafeList<KeyInfo>> mKeyListenList;					// 按键按下的监听回调列表
	protected SafeDictionary<int, ClickPoint> mClickPointList;	// 这一帧点击的坐标的列表,因为触点数量不一定,所以只是记录这一帧有哪些点击操作,可能与GlobalTouchSystem有一定的重合
	protected HashSet<IInputField> mInputFieldList;				// 输入框列表,用于判断当前是否正在输入
	protected Vector3 mLastMousePosition;						// 上一帧的鼠标坐标
	protected Vector3 mCurMousePosition;						// 当前鼠标坐标,以左下角为原点的坐标
	protected Vector3 mMouseDelta;								// 这一帧的鼠标移动量
	protected int mFocusMask;									// 当前的输入掩码,是输入框的输入还是快捷键输入
	public InputSystem()
	{
		mListenerList = new Dictionary<object, Dictionary<OnKeyCurrentDown, KeyInfo>>();
		mKeyListenList = new SafeDictionary<KeyCode, SafeList<KeyInfo>>();
		mInputFieldList = new HashSet<IInputField>();
		mClickPointList = new SafeDictionary<int, ClickPoint>();
	}
	// 添加对于指定按键的当前按下事件监听
	public void listenKeyCurrentDown(KeyCode key, OnKeyCurrentDown callback, object listener, bool ctrlDown = false, bool shiftDown = false, bool altDown = false)
	{
		CLASS(out KeyInfo info);
		info.mCallback = callback;
		info.mListener = listener;
		info.mKey = key;
		info.mCtrlDown = ctrlDown;
		info.mShiftDown = shiftDown;
		info.mAltDown = altDown;
		if (!mKeyListenList.tryGetValue(key, out SafeList<KeyInfo> list))
		{
			list = new SafeList<KeyInfo>();
			mKeyListenList.add(key, list);
		}
		list.add(info);
		if (!mListenerList.TryGetValue(listener, out Dictionary<OnKeyCurrentDown, KeyInfo> callbcakList))
		{
			LIST_PERSIST(out callbcakList);
			mListenerList.Add(listener, callbcakList);
		}
		callbcakList.Add(callback, info);
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
			int count = callbackList.count();
			for (int i = 0; i < count;)
			{
				if (item.Key == callbackList.get(i).mCallback)
				{
					UN_CLASS(callbackList.get(i));
					callbackList.removeAt(i);
					--count;
				}
				else
				{
					++i;
				}
			}
		}
		UN_LIST(list);
		mListenerList.Remove(listener);
	}
	public void registeInputField(IInputField inputField) { mInputFieldList.Add(inputField); }
	public void unregisteInputField(IInputField inputField) { mInputFieldList.Remove(inputField); }
	public void setMask(FOCUS_MASK mask) { mFocusMask = (int)mask; }
	public bool hasMask(FOCUS_MASK mask) { return mask == FOCUS_MASK.NONE || mFocusMask == 0 || (mFocusMask & (int)mask) != 0; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 点击操作会判断触点和鼠标,因为都会产生点击操作
		// 在触屏上,会将第一个触点也当作鼠标,也就是触屏上使用isMouseCurrentDown实际上是获得的第一个触点的信息
		// 所以先判断有没有触点,如果没有再判断鼠标
		if (Input.touchCount == 0)
		{
			// 左键
			if (isMouseCurrentDown(MOUSE_BUTTON.LEFT))
			{
				pointDown((int)MOUSE_BUTTON.LEFT, Input.mousePosition);
			}
			else if (isMouseCurrentUp(MOUSE_BUTTON.LEFT))
			{
				pointUp((int)MOUSE_BUTTON.LEFT, Input.mousePosition);
			}
			// 右键
			if (isMouseCurrentDown(MOUSE_BUTTON.RIGHT))
			{
				pointDown((int)MOUSE_BUTTON.RIGHT, Input.mousePosition);
			}
			else if (isMouseCurrentUp(MOUSE_BUTTON.RIGHT))
			{
				pointUp((int)MOUSE_BUTTON.RIGHT, Input.mousePosition);
			}
			// 中键
			if (isMouseCurrentDown(MOUSE_BUTTON.MIDDLE))
			{
				pointDown((int)MOUSE_BUTTON.MIDDLE, Input.mousePosition);
			}
			else if (isMouseCurrentUp(MOUSE_BUTTON.MIDDLE))
			{
				pointUp((int)MOUSE_BUTTON.MIDDLE, Input.mousePosition);
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
				else if (touch.phase == TouchPhase.Ended)
				{
					pointUp(touch.fingerId, touch.position);
				}
			}
		}

		mCurMousePosition = Input.mousePosition;
		// 鼠标任意键按下时,确保这一帧没有鼠标移动量
		if (isMouseCurrentDown(MOUSE_BUTTON.LEFT) ||
			isMouseCurrentDown(MOUSE_BUTTON.RIGHT) ||
			isMouseCurrentDown(MOUSE_BUTTON.MIDDLE))
		{
			mMouseDelta = Vector3.zero;
		}
		else
		{
			mMouseDelta = mCurMousePosition - mLastMousePosition;
		}
		mLastMousePosition = mCurMousePosition;
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

		bool isCtrlDown = isKeyDown(KeyCode.LeftControl) || isKeyDown(KeyCode.RightControl);
		bool isShiftDown = isKeyDown(KeyCode.LeftShift) || isKeyDown(KeyCode.RightShift);
		bool isAltDown = isKeyDown(KeyCode.LeftAlt) || isKeyDown(KeyCode.RightAlt);
		int helperKeyMask = generateHelperKeyMask(isCtrlDown, isShiftDown, isAltDown);
		// 遍历监听列表,发送监听事件
		foreach (var item in mKeyListenList.startForeach())
		{
			var callbackList = item.Value.startForeach();
			int callbackCount = callbackList.Count;
			if (callbackCount == 0)
			{
				continue;
			}
			if (!isKeyCurrentDown(item.Key, FOCUS_MASK.SCENE))
			{
				continue;
			}
			for (int i = 0; i < callbackCount; ++i)
			{
				KeyInfo info = callbackList[i];
				if (generateHelperKeyMask(info.mCtrlDown, info.mShiftDown, info.mAltDown) == helperKeyMask)
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
		var clickList = mClickPointList.startForeach();
		foreach (var item in clickList)
		{
			if (!item.Value.isFinish())
			{
				continue;
			}
			int pointID = item.Key;
			UN_CLASS(clickList[pointID]);
			mClickPointList.remove(pointID);
		}
	}
	// 外部可以通过获取点击操作列表,获取到这一帧的所有点击操作信息,且不分平台,统一触屏和鼠标操作
	public SafeDictionary<int, ClickPoint> getClickPointList() { return mClickPointList; }
	public void setMouseVisible(bool visible) { Cursor.visible = visible; }
	public Vector3 getMousePosition() { return mCurMousePosition; }
	public Vector3 getMouseDelta() { return mMouseDelta; }
	public float getMouseWheelDelta() { return Input.mouseScrollDelta.y; }
	public bool isMouseDown(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return isMouseKeepDown(mouse, mask) || isMouseCurrentDown(mouse, mask);
	}
	public bool isMouseKeepDown(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return Input.GetMouseButton((int)mouse) && hasMask(mask);
	}
	public bool isMouseCurrentDown(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return Input.GetMouseButtonDown((int)mouse) && hasMask(mask);
	}
	public bool isMouseCurrentUp(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return Input.GetMouseButtonUp((int)mouse) && hasMask(mask);
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
	protected int generateHelperKeyMask(bool ctrlDown, bool shiftDown, bool altDown)
	{
		int curHelperKeyMask = 0;
		if (ctrlDown)
		{
			curHelperKeyMask |= 1 << 0;
		}
		if (shiftDown)
		{
			curHelperKeyMask |= 1 << 1;
		}
		if (altDown)
		{
			curHelperKeyMask |= 1 << 2;
		}
		return curHelperKeyMask;
	}
	protected void pointDown(int pointerID, Vector3 position)
	{
		if (mClickPointList.containsKey(pointerID))
		{
			logWarning("已包含相同的触点ID, ID:" + IToS(pointerID));
			mClickPointList.clear();
			return;
		}
		CLASS(out ClickPoint point);
		point.pointDown(pointerID, position);
		mClickPointList.add(point.getPointerID(), point);
	}
	protected void pointUp(int pointerID, Vector3 position)
	{
		if (!mClickPointList.tryGetValue(pointerID, out ClickPoint point))
		{
			logWarning("找不到触点信息, ID:" + pointerID);
			return;
		}
		point.pointUp(position);
	}
}