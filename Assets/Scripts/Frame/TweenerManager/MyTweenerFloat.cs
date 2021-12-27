using System;

// 用于实现对浮点数的渐变,外部可通过OT.TWEEN_FLOAT进行访问
public class MyTweenerFloat : MyTweener
{
	protected COMMyTweenerFloat mComponentFloat;	// 浮点数渐变组件
	public override bool isDoing() { return mComponentFloat.getState() == PLAY_STATE.PLAY; }
	public override void resetProperty()
	{
		base.resetProperty();
		mComponentFloat = null;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		addComponent(out mComponentFloat);
	}
}