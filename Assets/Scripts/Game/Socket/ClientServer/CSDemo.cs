using System;
using System.Collections;
using System.Collections.Generic;

public class CSDemo : SocketPacket
{
	public UINTS mDemoArray = new UINTS(10);
	public USHORT mDemoShort = new USHORT();
	protected override void fillParams()
	{
		pushParam(mDemoArray, true);
		pushParam(mDemoShort);
	}
}