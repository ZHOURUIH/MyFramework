using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using static UnityUtility;
using static CSharpUtility;
using static StringUtility;
using static FrameUtility;

// 对UGUI的InputField的封装
public class myUGUIInputField : myUGUIImage, IInputField
{
	protected UnityAction<string> mThisEditEnd;		// 避免GC的委托
	protected UnityAction<string> mThisEditing;		// 避免GC的委托
	protected OnInputField mEdittingCallback;		// 正在输入的回调
	protected OnInputField mEditEndCallback;		// 输入结束的回调
	protected InputField mInputField;				// UGUI的InputField组件
	protected bool mEndNeedEnter;					// 是否需要按下回车键才会认为是输入结束,false则是只要输入框失去焦点就认为输入结束而调用mEditorEndCallback
	public myUGUIInputField()
	{
		mThisEditEnd = onEditEnd;
		mThisEditing = onEditting;
	}
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
			logError(Typeof(this) + " can not find " + typeof(InputField) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
		if (!mImage.raycastTarget)
		{
			logError("输入框需要开启图片的raycastTarget");
		}
	}
	public override void setAlpha(float alpha, bool fadeChild)
	{
		base.setAlpha(alpha, fadeChild);
		Color color = mInputField.textComponent.color;
		color.a = alpha;
		mInputField.textComponent.color = color;
	}
	// needEnter表示是否需要按下回车键才会认为是输入结束,false则是只要输入框失去焦点就认为输入结束
	public void setOnEditEnd(OnInputField action, bool needEnter = true)
	{
		mEditEndCallback = action;
		mEndNeedEnter = needEnter;
		mInputField.onEndEdit.AddListener(mThisEditEnd);
	}
	public void setOnEditting(OnInputField action)
	{
		mEdittingCallback = action;
		mInputField.onValueChanged.AddListener(mThisEditing);
	}
	public void clear(bool removeFocus = true) 
	{
		setText(EMPTY); 
		if (removeFocus)
		{
			focus(false);
		}
	}
	public void setText(string value) { mInputField.text = value; }
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
	public void setCharacterLimit(int limit) { mInputField.characterLimit = limit; }
	public void setCaretPosition(int pos) { mInputField.caretPosition = pos; }
	public int getCaretPosition() { return mInputField.caretPosition; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onEditEnd(string value) 
	{
		// 只处理由回车触发的输入结束
		if (mEndNeedEnter && !isKeyDown(KeyCode.Return) && !isKeyDown(KeyCode.KeypadEnter))
		{
			return;
		}
		mEditEndCallback?.Invoke(value); 
	}
	protected void onEditting(string value) { mEdittingCallback?.Invoke(value); }
}