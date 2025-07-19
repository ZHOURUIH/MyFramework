using System;
using UnityEngine.UI;
using UnityEngine.Events;
using static UnityUtility;

// 对UGUI的ScrollBar的封装
public class myUGUIScrollBar : myUGUIObject
{
	protected Action<float, myUGUIScrollBar> mCallBack;	// 值改变的回调
	protected UnityAction<float> mThisValueCallback;	// 避免GC的委托
	protected Scrollbar mScrollBar;						// UGUI的ScrollBar组件
	public myUGUIScrollBar()
	{
		mThisValueCallback = onValueChangeCallBack;
	}
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mScrollBar))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个Scrollbar组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mScrollBar = mObject.AddComponent<Scrollbar>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
	}
	public void setValue(float value) { mScrollBar.value = value; }
	public float getValue() { return mScrollBar.value; }
	public void setCallBack(Action<float, myUGUIScrollBar> callBack)
	{
		mCallBack = callBack;
		mScrollBar.onValueChanged.AddListener(mThisValueCallback);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onValueChangeCallBack(float value)
	{
		mCallBack?.Invoke(value, this);
	}
}