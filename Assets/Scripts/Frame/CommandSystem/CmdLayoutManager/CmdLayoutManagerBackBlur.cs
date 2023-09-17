using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static FrameBase;
using static UnityUtility;
using static FrameDefine;

// 设置背景模糊的布局,一般由LayoutManager调用
public class CmdLayoutManagerBackBlur
{
	// excludeLayout,不模糊的布局列表
	// blur,是否开启模糊
	public static void execute(List<GameLayout> excludeLayout, bool blur)
	{
		// 找到mExcludeLayout中层级最高的,低于该层的都设置到模糊层
		var layoutList = mLayoutManager.getLayoutList();
		int maxOrder = -999;
		int excludeCount = excludeLayout.Count;
		for(int i = 0; i < excludeCount; ++i)
		{
			maxOrder = getMax(excludeLayout[i].getRenderOrder(), maxOrder);
		}
		var mainList = layoutList.getMainList();
		foreach (var item in mainList)
		{
			GameLayout layout = item.Value;
			if(!layout.isVisible())
			{
				continue;
			}
			GameObject rootObj = layout.getRoot().getObject();
			if (layout.getRenderOrder() < maxOrder)
			{
				setGameObjectLayer(rootObj, LAYER_UI_BLUR);
			}
			else
			{
				setGameObjectLayer(rootObj, layout.getDefaultLayer());
			}
		}
		// 开启模糊摄像机
		mCameraManager.activeBlurCamera(blur);
	}
}