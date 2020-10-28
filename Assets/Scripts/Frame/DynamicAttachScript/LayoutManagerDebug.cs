using System;
using System.Collections.Generic;
using UnityEngine;

public class LayoutManagerDebug : MonoBehaviour
{
	public bool UseAnchor;
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
		{
			return;
		}
		LayoutManager layoutManager = FrameBase.mLayoutManager;
		UseAnchor = layoutManager.isUseAnchor();
	}
}