using System;

public class MyTweenerFloat : MyTweener
{
	protected COMMyTweenerFloat mComponentFloat;
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