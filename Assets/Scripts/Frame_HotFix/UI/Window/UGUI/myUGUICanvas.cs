using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;

// 对UGUI Canvas的封装,实际上基类已经记录了一个Canvas变量
public class myUGUICanvas : myUGUIObject
{
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mCanvas))
		{
			if (!mIsNewObject)
			{
				logError("需要添加一个Canvas组件,name:" + getName() + ", layout:" + getLayout().getName());
			}
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
	public void setSortingLayer(string layerName) { mCanvas.sortingLayerName = layerName; }
}