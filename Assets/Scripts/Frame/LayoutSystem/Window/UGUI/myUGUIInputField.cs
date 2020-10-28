using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class myUGUIInputField : myUGUIObject, IInputField
{
	protected InputField mInputField;
	protected OnInputField mEditEndCallback;
	protected OnInputField mEdittingCallback;
	public override void init()
	{
		base.init();
		mInputField = mObject.GetComponent<InputField>();
		if (mInputField == null)
		{
			mInputField = mObject.AddComponent<InputField>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mInputField == null)
		{
			logError(Typeof(this) + " can not find " + Typeof<InputField>() + ", window:" + mName + ", layout:" + mLayout.getName());
		}
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		base.setAlpha(alpha, fadeChild);
		Color color = mInputField.textComponent.color;
		color.a = alpha;
		mInputField.textComponent.color = color;
	}
	public void setOnEditEnd(OnInputField action)
	{
		mEditEndCallback = action;
		mInputField.onEndEdit.AddListener(onEditEnd);
	}
	public void setOnEditting(OnInputField action)
	{
		mEdittingCallback = action;
		mInputField.onValueChanged.AddListener(onEditting);
	}
	public void cleanUp() { setText(EMPTY_STRING); }
	public void setText(string value) { mInputField.text = value; }
	public void setText(float value) { setText(value.ToString()); }
	public string getText() { return mInputField.text; }
	public bool isFocused() { return mInputField.isFocused; }
	public bool isVisible() { return isActive(); }
	public void focus(bool active = true)
	{
		if (active)
		{
			mInputField.ActivateInputField();
		}
		else
		{
			mInputField.DeactivateInputField();
		}
	}
	public void setCharacterLimit(int limit) { mInputField.characterLimit = limit; }
	public void setCaretPosition(int pos) { mInputField.caretPosition = pos; }
	public int getCaretPosition() { return mInputField.caretPosition; }
	//------------------------------------------------------------------------------------------------
	protected void onEditEnd(string value) 
	{
		// 只处理由回车触发的输入结束
		if (!getKeyDown(KeyCode.Return) && !getKeyDown(KeyCode.KeypadEnter))
		{
			return;
		}
		mEditEndCallback?.Invoke(value); 
	}
	protected void onEditting(string value) { mEdittingCallback?.Invoke(value); }
}