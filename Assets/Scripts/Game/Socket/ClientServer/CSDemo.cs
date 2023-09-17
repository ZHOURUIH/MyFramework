using System;

public class CSDemo : NetPacketFrame
{
	public LONGS mDemoArray = new LONGS();
	public USHORT mDemoUShort = new USHORT();
	public override void init()
	{
		base.init();
		addParam(mDemoArray, false);
		addParam(mDemoUShort, false);
	}
	public static void send()
	{
		;
	}
}