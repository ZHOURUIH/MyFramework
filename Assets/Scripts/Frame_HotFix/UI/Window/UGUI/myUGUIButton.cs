using UnityEngine.Events;
using UnityEngine.UI;

// 因为button组件一般都是跟Image组件一起的,所以继承myUGUIImage
public class myUGUIButton : myUGUIImageSimple
{
	protected Button mButton;		// UGUI的Button组件
	public override void init()
	{
		base.init();
		if (!mObject.TryGetComponent(out mButton))
		{
			mButton = mObject.AddComponent<Button>();
			// 添加UGUI组件后需要重新获取RectTransform
			mObject.TryGetComponent(out mRectTransform);
			mTransform = mRectTransform;
		}
	}
	public void setUGUIButtonClick(UnityAction callback)
	{
		mButton.onClick.AddListener(callback);
	}
}