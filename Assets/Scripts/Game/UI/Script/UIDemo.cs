using UnityEngine;
using static UnityUtility;

public class UIDemo : LayoutScript
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
		mBackground.registeCollider(onBackgroundClick);
		mLabel.registeCollider(onTextClick);
	}
	public override void onGameState()
	{
		Vector3 curPos = mLabel.getPosition();
		Vector3 targetPos = curPos + new Vector3(100.0f, 0.0f, 0.0f);
		FT.MOVE_EX(mLabel, KEY_CURVE.ZERO_ONE_ZERO, curPos, targetPos, 1.0f, true, 0.0f, null, null);
	}
	public void setText(string text)
	{
		mLabel.setText(text);
	}
	//--------------------------------------------------------------------------------------------------------------------------
	protected void onBackgroundClick()
	{
		log("点击背景");
	}
	protected void onTextClick()
	{
		log("点击文字");
	}
}