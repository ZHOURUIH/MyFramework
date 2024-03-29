﻿using System;

// 显示或隐藏一个物体
public class CmdMovableObjectActive : Command
{
	public bool mActive;	// 显示或隐藏
	public override void resetProperty()
	{
		base.resetProperty();
		mActive = true;
	}
	public override void execute()
	{
		var obj = mReceiver as MovableObject;
		obj.setActive(mActive);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mActive:", mActive);
	}
}