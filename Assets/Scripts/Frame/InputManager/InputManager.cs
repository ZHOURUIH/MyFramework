using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager : FrameComponent
{
	protected HashSet<IInputField> mInputFieldList;
	protected Vector3 mLastMousePosition;
	protected Vector3 mCurMousePosition;
	protected Vector3 mMouseDelta;
	protected int mFocusMask;
	public InputManager(string name)
		: base(name)
	{
		mInputFieldList = new HashSet<IInputField>();
	}
	public void registeInputField(IInputField inputField) { mInputFieldList.Add(inputField); }
	public void unregisteInputField(IInputField inputField) { mInputFieldList.Remove(inputField); }
	public void setMask(FOCUS_MASK mask) { mFocusMask = (int)mask; }
	public bool hasMask(FOCUS_MASK mask) { return mask == FOCUS_MASK.FM_NONE || mFocusMask == 0 || (mFocusMask & (int)mask) != 0; }
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
		setMask(inputting ? FOCUS_MASK.FM_UI : FOCUS_MASK.FM_SCENE);
	}
	public void setMouseVisible(bool visible) { Cursor.visible = visible; }
	public new Vector3 getMousePosition() { return mCurMousePosition; }
	public Vector3 getMouseDelta() { return mMouseDelta; }
	public float getMouseWheelDelta() { return Input.mouseScrollDelta.y; }
	public bool getMouseDown(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return getMouseKeepDown(mouse, mask) || getMouseCurrentDown(mouse, mask);
	}
	public bool getMouseKeepDown(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return Input.GetMouseButton((int)mouse) && hasMask(mask);
	}
	public bool getMouseCurrentDown(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return Input.GetMouseButtonDown((int)mouse) && hasMask(mask);
	}
	public bool getMouseCurrentUp(MOUSE_BUTTON mouse, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return Input.GetMouseButtonUp((int)mouse) && hasMask(mask);
	}
	public new virtual bool getKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return Input.GetKeyDown(key) && hasMask(mask);
	}
	public new virtual bool getKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return Input.GetKeyUp(key) && hasMask(mask);
	}
	public new virtual bool getKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return (Input.GetKeyDown(key) || Input.GetKey(key)) && hasMask(mask);
	}
	public new virtual bool getKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return (Input.GetKeyUp(key) || !Input.GetKey(key)) && hasMask(mask);
	}
}