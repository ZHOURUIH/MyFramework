using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using TMPro;

public class txUGUIInputFieldTMP : txUGUIObject
{
	protected TMP_InputField mInputField;
	protected OnInputField mAction;
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		mInputField = mObject.GetComponent<TMP_InputField>();
		if (mInputField == null)
		{
			mInputField = mObject.AddComponent<TMP_InputField>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mInputField == null)
		{
			logError(GetType() + " can not find " + typeof(TMP_InputField) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		base.setAlpha(alpha, fadeChild);
		Color color = mInputField.textComponent.color;
		color.a = alpha;
		mInputField.textComponent.color = color;
	}
	public void setOnEndEdit(OnInputField action)
	{
		mAction = action;
		mInputField.onEndEdit.AddListener(OnEndEdit);
	}
	public void cleanUp() { setText(EMPTY_STRING); }
	public void setText(string value) { mInputField.text = value; }
	public void setText(float value) { setText(value.ToString()); }
	public string getText() { return mInputField.text; }
	public bool isFocused() { return mInputField.isFocused; }
	//------------------------------------------------------------------------------------------------
	protected void OnEndEdit(string value) { mAction?.Invoke(value); }
}