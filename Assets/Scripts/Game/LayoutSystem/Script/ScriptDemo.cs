using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptDemo : LayoutScript
{
	protected txUGUIImage mBackground;
	protected txUGUIText mLabel;
	public ScriptDemo(string name)
		:base(name){}
	public override void assignWindow()
	{
		newObject(out mBackground, "Background");
		newObject(out mLabel, "Label");
	}
	public override void init()
	{
		registeBoxCollider(mBackground, onBackgroundClick);
		registeBoxCollider(mLabel, onTextClick);
	}
	public override void onReset(){}
	public override void onGameState()
	{
		Vector3 curPos = mLabel.getPosition();
		Vector3 targetPos = curPos + new Vector3(100.0f, 0.0f, 0.0f);
		LT.MOVE(mLabel, CommonDefine.ZERO_ONE_ZERO, curPos, targetPos, 1.0f, true);
	}
	public override void onShow(bool immediately, string param){}
	public override void onHide(bool immediately, string param){}
	public override void update(float elapsedTime){}
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