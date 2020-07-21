using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// 按照深度降序排序
public struct DepthDescend : IComparer<UIDepth>
{
	// a小于b返回1, a等于b返回0, a大于b返回-1
	public int Compare(UIDepth a, UIDepth b)
	{
		// 优先比较布局的深度
		if (a.mPanelDepth != b.mPanelDepth)
		{
			return MathUtility.sign(b.mPanelDepth - a.mPanelDepth);
		}
		// 如果布局深度相同,再比较窗口深度
		else
		{
			return (int)MathUtility.sign(b.mWindowDepth - a.mWindowDepth);
		}
	}
}