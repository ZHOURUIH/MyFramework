using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptGaming : ILRLayoutScript
{
	protected myUGUIObject mBackground;
	protected myUGUIObject mAvatar;
	protected myUGUIText mSpeed;
	public override void assignWindow()
	{
		newObject(out mBackground, "Background");
		newObject(out mAvatar, mBackground, "Avatar");
		newObject(out mSpeed, mBackground, "Speed");
	}
	public override void init(){}
	public void setAvatarPosition(Vector3 pos)
	{
		FT.MOVE(mAvatar, pos);
	}
	public void setSpeed(float speed)
	{
		mSpeed.setText("速度:" + FToS(speed, 0));
	}
	//------------------------------------------------------------------------------------------------
}