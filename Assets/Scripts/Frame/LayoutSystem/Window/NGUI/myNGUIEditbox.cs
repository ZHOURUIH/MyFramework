using UnityEngine;
using System.Collections;

#if USE_NGUI

public class myNGUIEditbox : myNGUISprite
{
	protected UIInput mInput;
	public override void init()
	{
		base.init();
		mInput = getUnityComponent<UIInput>();
	}
	public void setText(string text){mInput.value = text;}
	public string getText(){return mInput.value;}
	public void cleanUp()
	{
		mInput.RemoveFocus();
		setText(EMPTY_STRING);
	}
	public void startInput(){mInput.isSelected = true;}
	public void stopInput(){mInput.isSelected = false;}
	public bool isFocused() { return mInputField.isSelected; }
	public bool isVisible() { return isActive(); }
	public void setInputSubmitCallback(EventDelegate.Callback callback)
	{
		EventDelegate.Add(mInput.onSubmit, callback);
	}
	public void setInpuntOnChangeCallback(EventDelegate.Callback callback)
	{
		EventDelegate.Add(mInput.onChange, callback);
	}
	public void setOnValidateCallback(UIInput.OnValidate validate)
	{
		mInput.onValidate = validate;
	}
	public void setOnKeyUpCallback(UIInput.OnKeyDelegate onKeyDelegate)
	{
		mInput.onKeyUpCallback = onKeyDelegate;
	}
	public void setOnKeyDownUpCallback(UIInput.OnKeyDelegate onKeyDelegate)
	{
		mInput.onKeyDownCallback = onKeyDelegate;
	}
	public void setOnKeyKeepDownCallback(UIInput.OnKeyDelegate onKeyDelegate)
	{
		mInput.onKeyKeepDownCallback = onKeyDelegate;
	}
	public void setOnInputEventCallback(UIInput.OnInputEvent onInputEvent)
	{
		mInput.onInputEvent = onInputEvent;
	}
}

#endif