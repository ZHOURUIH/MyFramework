using System.Collections.Generic;

// 游戏设置
public class GameSetting : FrameSystem
{
	public GameSetting() { }
	public override void resetProperty()
	{
		base.resetProperty();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addInitComponent<COMGameSettingAudio>();
	}
}