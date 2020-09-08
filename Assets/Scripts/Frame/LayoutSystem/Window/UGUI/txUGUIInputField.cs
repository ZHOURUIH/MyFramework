using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class txUGUIInputField : txUGUIObject, IInputField
{
	protected InputField mInputField;
	protected OnInputField mEditEndCallback;
	protected OnInputField mEdittingCallback;
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
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
			logError(GetType() + " can not find " + typeof(InputField) + ", window:" + mName + ", layout:" + mLayout.getName());
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
	public void focus() { mInputField.ActivateInputField(); }
	public void setCharacterLimit(int limit) { mInputField.characterLimit = limit; }
	public void setCaretPosition(int pos) { mInputField.caretPosition = pos; }
	public int getCaretPosition() { return mInputField.caretPosition; }
	//------------------------------------------------------------------------------------------------
	protected void onEditEnd(string value) { mEditEndCallback?.Invoke(value); }
	protected void onEditting(string value) { mEdittingCallback?.Invoke(value); }
}