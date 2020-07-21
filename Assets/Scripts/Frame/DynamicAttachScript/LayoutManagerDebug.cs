using System;
using System.Collections.Generic;
using UnityEngine;

public class LayoutManagerDebug : MonoBehaviour
{
	public bool UseAnchor;
	public void Update()
	{
		GameLayoutManager layoutManager = FrameBase.mLayoutManager;
		UseAnchor = layoutManager.isUseAnchor();
	}
}