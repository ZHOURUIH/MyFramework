using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class txUGUIScrollBar : txUGUIObject
{
	protected Scrollbar mScrollBar;
	protected Action<float, txUGUIScrollBar> mCallBack;
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
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
			logError(GetType() + " can not find " + typeof(Scrollbar) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
	}
	public void setValue(float value) { mScrollBar.value = value; }
	public float getValue() { return mScrollBar.value; }
	public void setCallBack(Action<float, txUGUIScrollBar> callBack)
	{
		mCallBack = callBack;
		mScrollBar.onValueChanged.AddListener(onValueChangeCallBack);
	}
	protected void onValueChangeCallBack(float value)
	{
		mCallBack?.Invoke(value, this);
	}
}