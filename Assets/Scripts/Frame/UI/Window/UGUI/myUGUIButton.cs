using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// 因为button组件一般都是跟Image组件一起的,所以继承myUGUIImage
public class myUGUIButton : myUGUIImage
{
	protected Button mButton;		// UGUI的Button组件
	public override void init()
	{
		base.init();
		mButton = mObject.GetComponent<Button>();
		if (mButton == null)
		{
			mButton = mObject.AddComponent<Button>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
	}
	public void setUGUIButtonClick(UnityAction callback)
	{
		mButton.onClick.AddListener(callback);
	}
}