using System;

public class SCDemo : NetPacketFrame
{
	public UINT mDemoParam = new UINT();
	public override void init()
	{
		base.init();
		addParam(mDemoParam, false);
	}
	public override void execute()
	{
		;
	}
}