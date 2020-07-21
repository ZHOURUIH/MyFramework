using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public struct UIDepth
{
	public int mPanelDepth;
	public float mWindowDepth;
	public UIDepth(int panelDepth, float windowDepth)
	{
		mPanelDepth = panelDepth;
		mWindowDepth = windowDepth;
	}
}