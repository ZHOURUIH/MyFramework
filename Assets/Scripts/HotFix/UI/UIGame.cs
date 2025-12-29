using Obfuz;
using UnityEngine;
using static StringUtility;

// auto generate member start
[ObfuzIgnore(ObfuzScope.TypeName)]
public class UIGame : LayoutScript
{
	protected myUGUIObject mAvatar;
	protected myUGUIText mSpeed;
	protected ScrollViewPanel mScrollViewPanel;
	// auto generate member end
	public UIGame()
	{
		// auto generate constructor start
		mScrollViewPanel = new(this);
		// auto generate constructor end
	}
	public override void assignWindow()
	{
		// auto generate assignWindow start
		newObject(out myUGUIObject background, "Background", false);
		newObject(out mAvatar, background, "Avatar");
		newObject(out mSpeed, background, "Speed");
		mScrollViewPanel.assignWindow(background, "ScrollViewPanel");
		// auto generate assignWindow end
	}
	public override void init()
	{
		base.init();
	}
	public override void onGameState()
	{
		base.onGameState();
	}
	public void setAvatarPosition(Vector3 pos)
	{
		mAvatar.setPosition(pos);
	}
	public void setSpeed(float speed)
	{
		mSpeed.setText("速度:" + FToS(speed, 0));
	}
}
