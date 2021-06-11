using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class myUGUIButton : myUGUIObject
{
	protected Button mButton;
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
	public void setUGUIClick(UnityAction callback)
	{
		mButton.onClick.AddListener(callback);
	}
}