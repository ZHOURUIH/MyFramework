using UnityEngine;
using System.Collections;

#if USE_NGUI

public class txNGUIObject : txUIObject
{
	protected UIWidget mWidget;
	public override bool selfAlphaChild() { return true; }
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		mWidget = mObject.GetComponent<UIWidget>();
		string layoutName = mLayout != null ? mLayout.getName() : EMPTY_STRING;
		// BoxCollider必须与UIWidget(或者UIWidget的派生类)一起使用,否则在自适应屏幕时BoxCollider会出现错误
		if (mWidget == null)
		{
			logWarning("BoxCollider must used with UIWidget! Otherwise can not adapt to the screen sometimes! name : " + mName + ", layout : " + layoutName);
		}
		else if (!mWidget.autoResizeBoxCollider)
		{
			logWarning("UIWidget's autoResizeBoxCollider must be true! Otherwise can not adapt to the screen sometimes! name : " + mName + ", layout : " + layoutName);
		}
	}
	public override int getDepth()
	{
		logError("there is no depth in txNGUIObject, use other window type instead!");
		return 0;
	}
	// 由NGUI调用的callback
	public void setClickCallback(UIEventListener.VoidDelegate callback) { UIEventListener.Get(mObject).onClick = callback; }
	public void setHoverCallback(UIEventListener.BoolDelegate callback) { UIEventListener.Get(mObject).onHover = callback; }
	public void setPressCallback(UIEventListener.BoolDelegate callback) { UIEventListener.Get(mObject).onPress = callback; }
	public void setDoubleClickCallback(UIEventListener.VoidDelegate callback) { UIEventListener.Get(mObject).onDoubleClick = callback; }
}

#endif