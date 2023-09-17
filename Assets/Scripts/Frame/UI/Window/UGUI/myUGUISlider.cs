using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using static UnityUtility;
using static CSharpUtility;

// 对UGUI的Slider组件的封装
public class myUGUISlider : myUGUIObject, ISlider
{
	protected Slider mSlider;	// UGUI的Slider组件
	public override void init()
	{
		base.init();
		mSlider = mObject.GetComponent<Slider>();
		if (mSlider == null)
		{
			mSlider = mObject.AddComponent<Slider>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mSlider == null)
		{
			logError(Typeof(this) + " can not find " + typeof(Slider) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
	}
	public Slider getSlider() { return mSlider; }
	public void setRange(int max, int min)
	{
		mSlider.maxValue = max;
		mSlider.minValue = min;
	}
	public void setSliderCallback(UnityAction<float> callback) { mSlider.onValueChanged.AddListener(callback); }
	public void setValue(float value) { mSlider.value = value; }
	public float getValue() { return mSlider.value; }
	public void setFillRect(myUGUIObject obj) { mSlider.fillRect = obj.getRectTransform(); }
	public void setHandleRect(myUGUIObject obj) { mSlider.handleRect = obj.getRectTransform(); }
}