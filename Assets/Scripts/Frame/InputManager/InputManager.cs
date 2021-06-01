using UnityEngine;
using System.Collections.Generic;

public class KeyInfo : FrameBase
{
	public object mListener;
	public OnKeyCurrentDown mCallback;
	public KeyCode mKey;
	public bool mCtrlDown;
	public bool mShiftDown;
	public bool mAltDown;
}

public class InputManager : FrameSystem
{
	protected Dictionary<object, Dictionary<OnKeyCurrentDown, KeyInfo>> mListenerList;
	protected SafeDictionary<KeyCode, SafeList<KeyInfo>> mKeyListenList;	// 按键按下的监听回调列表
	protected HashSet<IInputField> mInputFieldList;			// 输入框列表,用于判断当前是否正在输入
	protected Vector3 mLastMousePosition;					// 上一帧的鼠标坐标
	protected Vector3 mCurMousePosition;					// 当前鼠标坐标,以左下角为原点的坐标
	protected Vector3 mMouseDelta;							// 这一帧的鼠标移动量
	protected int mFocusMask;								// 当前的输入掩码,是输入框的输入还是快捷键输入
	public InputManager()
	{
		mListenerList = new Dictionary<object, Dictionary<OnKeyCurrentDown, KeyInfo>>();
		mKeyListenList = new SafeDictionary<KeyCode, SafeList<KeyInfo>>();
		mInputFieldList = new HashSet<IInputField>();
	}
	// 添加对于指定按键的当前按下事件监听
	public void listenKeyCurrentDown(KeyCode key, OnKeyCurrentDown callback, object listener, bool ctrlDown = false, bool shiftDown = false, bool altDown = false)
	{
		CLASS_MAIN(out KeyInfo info);
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
		if(!mListenerList.TryGetValue(listener, out Dictionary<OnKeyCurrentDown, KeyInfo> callbcakList))
		{
			LIST_MAIN_PERSIST(out callbcakList);
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
			for(int i = 0; i < count;)
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
		UN_LIST_MAIN(list);
		mListenerList.Remove(listener);
	}
	public void registeInputField(IInputField inputField) { mInputFieldList.Add(inputField); }
	public void unregisteInputField(IInputField inputField) { mInputFieldList.Remove(inputField); }
	public void setMask(FOCUS_MASK mask) { mFocusMask = (int)mask; }
	public bool hasMask(FOCUS_MASK mask) { return mask == FOCUS_MASK.NONE || mFocusMask == 0 || (mFocusMask & (int)mask) != 0; }
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mCurMousePosition = Input.mousePosition;
		mMouseDelta = mCurMousePosition - mLastMousePosition;
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

		bool isCtrlDown = getKeyDown(KeyCode.LeftControl) || getKeyDown(KeyCode.RightControl);
		bool isShiftDown = getKeyDown(KeyCode.LeftShift) || getKeyDown(KeyCode.RightShift);
		bool isAltDown = getKeyDown(KeyCode.LeftAlt) || getKeyDown(KeyCode.RightAlt);
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
			if(!getKeyCurrentDown(item.Key, FOCUS_MASK.SCENE))
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
	public void setMouseVisible(bool visible) { Cursor.visible = visible; }
	public new Vector3 getMousePosition() { return mCurMousePosition; }
	public Vector3 getMouseDelta() { return mMouseDelta; }
	public float getMouseWheelDelta() { return Input.mouseScrollDelta.y; }
	public bool getMouseDown(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return getMouseKeepDown(mouse, mask) || getMouseCurrentDown(mouse, mask);
	}
	public bool getMouseKeepDown(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return Input.GetMouseButton((int)mouse) && hasMask(mask);
	}
	public bool getMouseCurrentDown(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return Input.GetMouseButtonDown((int)mouse) && hasMask(mask);
	}
	public bool getMouseCurrentUp(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return Input.GetMouseButtonUp((int)mouse) && hasMask(mask);
	}
	public new virtual bool getKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return Input.GetKeyDown(key) && hasMask(mask);
	}
	public new virtual bool getKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return Input.GetKeyUp(key) && hasMask(mask);
	}
	public new virtual bool getKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return (Input.GetKeyDown(key) || Input.GetKey(key)) && hasMask(mask);
	}
	public new virtual bool getKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return (Input.GetKeyUp(key) || !Input.GetKey(key)) && hasMask(mask);
	}
}