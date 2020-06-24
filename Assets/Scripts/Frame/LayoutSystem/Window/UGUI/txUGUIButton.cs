using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class txUGUIButton : txUGUIObject
{
	protected Button mButton;
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		mButton = mObject.GetComponent<Button>();
		if (mButton == null)
		{
			mButton = mObject.AddComponent<Button>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
	}
}