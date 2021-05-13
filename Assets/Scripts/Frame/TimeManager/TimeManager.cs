using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TimeManager : FrameSystem
{
	//------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		// 这里只能使用未缩放的时间,否则会被自己的时间缩放所影响
		addComponent(typeof(COMTimeScale)).setIgnoreTimeScale(true);
	}
}