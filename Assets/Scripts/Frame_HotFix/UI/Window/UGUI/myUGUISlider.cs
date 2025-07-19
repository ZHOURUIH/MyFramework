using UnityEngine.UI;
using UnityEngine.Events;
using static UnityUtility;

// 对UGUI的Slider组件的封装
public class myUGUISlider : myUGUIObject, ISlider
{
	protected Slider mSlider;	// UGUI的Slider组件
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mSlider))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个Slider组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
			mSlider = mObject.AddComponent<Slider>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
	}
	public Slider getSlider() { return mSlider; }
	public void setRange(int min, int max)
	{
		if (min > max)
		{
			logError("滑动条的范围下限不能高于范围上限");
		}
		mSlider.minValue = min;
		mSlider.maxValue = max;
	}
	public void setSliderCallback(UnityAction<float> callback) { mSlider.onValueChanged.AddListener(callback); }
	public void setValue(float value) { mSlider.value = value; }
	public float getValue() { return mSlider.value; }
	public void setFillRect(myUGUIObject obj) { mSlider.fillRect = obj.getRectTransform(); }
	public void setHandleRect(myUGUIObject obj) { mSlider.handleRect = obj.getRectTransform(); }
}