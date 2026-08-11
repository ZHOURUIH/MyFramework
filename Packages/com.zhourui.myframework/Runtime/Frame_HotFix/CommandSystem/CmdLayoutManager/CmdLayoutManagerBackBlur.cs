using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static FrameBaseHotFix;
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
		int maxOrder = -999;
		foreach (GameLayout layout in excludeLayout)
		{
			maxOrder = getMax(layout.getRenderOrder(), maxOrder);
		}
		if (mLayoutManager.getLayoutList().isForeaching())
		{
			using var a = new DicScope<Type, GameLayout>(out var tempList);
			tempList.addRange(mLayoutManager.getLayoutList().getMainList());
			foreach (var item in tempList)
			{
				setLayoutBlur(item.Value, maxOrder);
			}
		}
		else
		{
			foreach (var item in mLayoutManager.getLayoutList())
			{
				setLayoutBlur(item.Value, maxOrder);
			}
		}
		// 开启模糊摄像机
		mCameraManager.activeBlurCamera(blur);
	}
	protected static void setLayoutBlur(GameLayout layout, int maxOrder)
	{
		if (!layout.isVisible())
		{
			return;
		}
		GameObject rootObj = layout.getRoot().getGameObject();
		if (layout.getRenderOrder() < maxOrder)
		{
			setGameObjectLayer(rootObj, LAYER_INT_UI_BLUR);
		}
		else
		{
			setGameObjectLayer(rootObj, layout.getDefaultLayer());
		}
	}
}