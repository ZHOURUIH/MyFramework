using UnityEngine;
using static StringUtility;
using static FrameBase;

public class UIGaming : LayoutScript
{
	protected myUGUIObject mBackground;
	protected myUGUIObject mAvatar;
	protected myUGUIText mSpeed;
	public UIGaming()
	{
		mNeedUpdate = false;
	}
	public override void assignWindow()
	{
		newObject(out mBackground, "Background");
		newObject(out mAvatar, mBackground, "Avatar");
		newObject(out mSpeed, mBackground, "Speed");
	}
	public override void init(){}
	public override void onGameState()
	{
		base.onGameState();
		if (mCharacterManager.getMyself() != null)
		{
			setSpeed((mCharacterManager.getMyself() as CharacterGame).getData().mSpeed);
		}
	}
	public void setAvatarPosition(Vector3 pos)
	{
		mAvatar.setPosition(pos);
	}
	public void setSpeed(float speed)
	{
		mSpeed.setText("速度:" + FToS(speed, 0));
	}
	//------------------------------------------------------------------------------------------------
}