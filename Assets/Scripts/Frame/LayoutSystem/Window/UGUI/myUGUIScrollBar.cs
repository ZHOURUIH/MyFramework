using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Events;

public class myUGUIScrollBar : myUGUIObject
{
	protected Scrollbar mScrollBar;
	protected Action<float, myUGUIScrollBar> mCallBack;
	protected UnityAction<float> mThisValueCallback;
	public myUGUIScrollBar()
	{
		mThisValueCallback = onValueChangeCallBack;
	}
	public override void init()
	{
		base.init();
		mScrollBar = mObject.GetComponent<Scrollbar>();
		if (mScrollBar == null)
		{
			mScrollBar = mObject.AddComponent<Scrollbar>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mScrollBar == null)
		{
			logError(Typeof(this) + " can not find " + Typeof<Scrollbar>() + ", window:" + mName + ", layout:" + mLayout.getName());
		}
	}
	public void setValue(float value) { mScrollBar.value = value; }
	public float getValue() { return mScrollBar.value; }
	public void setCallBack(Action<float, myUGUIScrollBar> callBack)
	{
		mCallBack = callBack;
		mScrollBar.onValueChanged.AddListener(mThisValueCallback);
	}
	//------------------------------------------------------------------------------------------------------------------------
	protected void onValueChangeCallBack(float value)
	{
		mCallBack?.Invoke(value, this);
	}
}