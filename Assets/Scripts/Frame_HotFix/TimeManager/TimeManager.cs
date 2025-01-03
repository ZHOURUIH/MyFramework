
// 时间管理器,用于对时间进行缩放
public class TimeManager : FrameSystem
{
	protected COMTimeScale mCOMTimeScale;	// 时间缩放组件
	public override void init()
	{
		base.init();
		// 这里只能使用未缩放的时间,否则会被自己的时间缩放所影响
		mCOMTimeScale.setIgnoreTimeScale(true);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addInitComponent(out mCOMTimeScale, false);
	}
}