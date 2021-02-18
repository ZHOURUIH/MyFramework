using UnityEngine;
using UnityEngine.UI;

public class myUGUICanvas : myUGUIObject
{
	protected Canvas mCanvas;
	public override void init()
	{
		base.init();
		mCanvas = mObject.GetComponent<Canvas>();
		if (mCanvas == null)
		{
			mCanvas = mObject.AddComponent<Canvas>();
			// 添加UGUI组件后需要重新获取RectTransform
			mRectTransform = mObject.GetComponent<RectTransform>();
			mTransform = mRectTransform;
		}
		if (mCanvas == null)
		{
			logError(Typeof(this) + " can not find " + Typeof<Canvas>() + ", window:" + mName + ", layout:" + mLayout.getName());
		}
		mCanvas.overrideSorting = true;
		// 添加GraphicRaycaster
		getUnityComponent<GraphicRaycaster>();
	}
	public void setSortingOrder(int order) { mCanvas.sortingOrder = order; }
	public Canvas getCanvas() { return mCanvas; }
}