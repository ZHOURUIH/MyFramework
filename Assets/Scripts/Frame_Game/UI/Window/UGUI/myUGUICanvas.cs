using UnityEngine;
using UnityEngine.UI;

// 对UGUICanvas的封装
public class myUGUICanvas : myUGUIObject
{
	protected Canvas mCanvas;		// UGUI的Canvas组件
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mCanvas))
		{
			mCanvas = mObject.AddComponent<Canvas>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
		mCanvas.overrideSorting = true;
		// 添加GraphicRaycaster
		getOrAddUnityComponent<GraphicRaycaster>();
	}
	public void setSortingOrder(int order) { mCanvas.sortingOrder = order; }
	public Canvas getCanvas() { return mCanvas; }
}