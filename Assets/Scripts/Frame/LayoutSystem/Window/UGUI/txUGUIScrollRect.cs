using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class txUGUIScrollRect : txUGUIObject
{
	protected ScrollRect mScrollRect;
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		mScrollRect = mObject.GetComponent<ScrollRect>();
		if (mScrollRect == null)
		{
			mScrollRect = mObject.AddComponent<ScrollRect>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mScrollRect == null)
		{
			logError(GetType() + " can not find " + typeof(ScrollRect) + ", window:" + mName + ", layout:" + mLayout.getName());
		}
	}
	public ScrollRect getScrollRect() { return mScrollRect; }
	public Vector2 getNormalizedPosition() { return mScrollRect.normalizedPosition; }
	public void setNormalizedPosition(Vector2 pos) { mScrollRect.normalizedPosition = pos; }
}