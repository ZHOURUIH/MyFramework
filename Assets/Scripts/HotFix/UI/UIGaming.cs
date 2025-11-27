using UnityEngine;
using static StringUtility;
using static FrameBaseHotFix;

public class UIGaming : LayoutScript
{
	// auto generate member start
	protected myUGUIObject mAvatar;
	protected myUGUIText mSpeed;
	// auto generate member end
	public UIGaming()
	{
		mNeedUpdate = false;
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out myUGUIObject background, "Background");
		newObject(out mAvatar, background, "Avatar");
		newObject(out mSpeed, background, "Speed");
		// auto generate assignWindow end
	}
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