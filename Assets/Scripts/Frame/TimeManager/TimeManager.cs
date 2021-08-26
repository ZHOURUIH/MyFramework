using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TimeManager : FrameSystem
{
	protected COMTimeScale mCOMTimeScale;
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
		addComponent(out mCOMTimeScale);
	}
}