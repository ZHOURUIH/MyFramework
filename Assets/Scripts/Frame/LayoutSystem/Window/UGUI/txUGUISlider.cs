using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class txUGUISlider : txUGUIObject, ISlider
{
	protected Slider mSlider;
	protected Action<float> onValueChangeCallBack;
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
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
			logError(GetType() + " can not find " + typeof(Slider) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
	}
	public Slider getSlider() { return mSlider; }
	public void setLimitValue(int max, int min)
	{
		mSlider.maxValue = max;
		mSlider.minValue = min;
	}
	public void setValue(float value) { mSlider.value = value; }
	public float getValue() { return mSlider.value; }
	public void setFillRect(txUIObject obj) { mSlider.fillRect = obj.getRectTransform(); }
}