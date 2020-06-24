using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CommandLayoutManagerBackBlur : Command
{
	public List<GameLayout> mExcludeLayout;
	public bool mBlur;
	public bool mIsNGUI;
	public override void init()
	{
		base.init();
		mExcludeLayout = null;
		mBlur = true;
		mIsNGUI = false;
	}
	public override void execute()
	{
		// 找到mExcludeLayout中层级最高的,低于该层的都设置到模糊层
		var layoutList = mLayoutManager.getLayoutList();
		int maxOrder = -999;
		foreach (var item in mExcludeLayout)
		{
			maxOrder = getMax(item.getRenderOrder(), maxOrder);
		}
		foreach (var item in layoutList)
		{
			if(!item.Value.isVisible())
			{
				continue;
			}	
			if(item.Value.getRenderOrder() < maxOrder)
			{
				setGameObjectLayer(item.Value.getRoot().getObject(), CommonDefine.LAYER_UI_BLUR);
			}
			else
			{
				setGameObjectLayer(item.Value.getRoot().getObject(), item.Value.getDefaultLayer());
			}
		}
		// 开启模糊摄像机
		mCameraManager.activeBlurCamera(mIsNGUI, mBlur);
	}
}