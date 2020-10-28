using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptDemo : LayoutScript
{
	protected myUGUIImage mBackground;
	protected myUGUIText mLabel;
	public override void assignWindow()
	{
		newObject(out mBackground, "Background");
		newObject(out mLabel, "Label");
	}
	public override void init()
	{
		registeCollider(mBackground, onBackgroundClick);
		registeCollider(mLabel, onTextClick);
	}
	public override void onGameState()
	{
		Vector3 curPos = mLabel.getPosition();
		Vector3 targetPos = curPos + new Vector3(100.0f, 0.0f, 0.0f);
		LT.MOVE(mLabel, FrameDefine.ZERO_ONE_ZERO, curPos, targetPos, 1.0f, true);
	}
	public void setText(string text)
	{
		mLabel.setText(text);
	}
	//--------------------------------------------------------------------------------------------------------------------------
	protected void onBackgroundClick(IMouseEventCollect go)
	{
		logInfo("点击背景");
	}
	protected void onTextClick(IMouseEventCollect go)
	{
		logInfo("点击文字");
	}
}