
// 游戏设置
public class GameSetting : FrameSystem
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addInitComponent<COMGameSettingAudio>(false);
	}
}