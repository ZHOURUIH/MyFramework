﻿#if USE_TMP
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static StringUtility;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseUtility;

// 对TextMeshPro的InputField的封装
public class myUGUIInputFieldTMP : myUGUIObject, IInputField
{
	protected UnityAction<string> mThisEditEnd;     // 避免GC的委托
	protected UnityAction<string> mThisEditting;	// 避免GC的委托
	protected TMP_InputField mInputField;			// TextMeshPro的InputField组件
	protected StringCallback mOnEndEdit;			// 输入结束时的回调
	protected StringCallback mOnEditting;           // 输入结束时的回调
	protected bool mEndNeedEnter;                   // 是否需要按下回车键才会认为是输入结束,false则是只要输入框失去焦点就认为输入结束而调用mEditorEndCallback
	public myUGUIInputFieldTMP()
	{
		mThisEditEnd = onEndEdit;
		mThisEditting = onEditting;
	}
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mInputField))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个TMP_InputField组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mInputField = mObject.AddComponent<TMP_InputField>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		base.setAlpha(alpha, fadeChild);
		Color color = mInputField.textComponent.color;
		color.a = alpha;
		mInputField.textComponent.color = color;
	}
	public void setOnEndEdit(StringCallback action, bool needEnter = true)
	{
		mOnEndEdit = action;
		mEndNeedEnter = needEnter;
		mInputField.onEndEdit.AddListener(mThisEditEnd);
	}
	public void setOnEditting(StringCallback action)
	{
		mOnEditting = action;
		mInputField.onValueChanged.AddListener(mThisEditting);
	}
	public void cleanUp() { setText(EMPTY); }
	public void setText(string value) { mInputField.text = value; }
	public void setText(int value) { setText(IToS(value)); }
	public void setText(float value) { setText(FToS(value, 2)); }
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
	public void clear(bool removeFocus = true)
	{
		setText(EMPTY);
		if (removeFocus)
		{
			focus(false);
		}
	}
	public void setCharacterLimit(int limit) { mInputField.characterLimit = limit; }
	public void setCaretPosition(int pos) { mInputField.caretPosition = pos; }
	public int getCaretPosition() { return mInputField.caretPosition; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onEndEdit(string value) 
	{
		// 只处理由回车触发的输入结束,移动端不处理
		if (!isRealMobile() && mEndNeedEnter && !isKeyDown(KeyCode.Return, FOCUS_MASK.UI) && !isKeyDown(KeyCode.KeypadEnter, FOCUS_MASK.UI))
		{
			return;
		}
		mOnEndEdit?.Invoke(value); 
	}
	protected void onEditting(string value) { mOnEditting?.Invoke(value); }
}
#endif