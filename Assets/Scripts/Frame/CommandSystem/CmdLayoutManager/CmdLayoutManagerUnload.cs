using System;
using static FrameBase;

// 卸载一个布局
public class CmdLayoutManagerUnload
{
	// layoutID,布局ID
	public static void execute(int layoutID)
	{
		mLayoutManager.destroyLayout(layoutID);
	}
}