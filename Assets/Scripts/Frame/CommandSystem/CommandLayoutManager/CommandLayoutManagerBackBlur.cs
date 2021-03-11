using System.Collections.Generic;
using UnityEngine;

public class CommandLayoutManagerBackBlur : Command
{
	public List<GameLayout> mExcludeLayout;
	public bool mBlur;
	public override void resetProperty()
	{
		base.resetProperty();
		mExcludeLayout = null;
		mBlur = true;
	}
	public override void execute()
	{
		// 找到mExcludeLayout中层级最高的,低于该层的都设置到模糊层
		var layoutList = mLayoutManager.getLayoutList();
		int maxOrder = -999;
		int excludeCount = mExcludeLayout.Count;
		for(int i = 0; i < excludeCount; ++i)
		{
			maxOrder = getMax(mExcludeLayout[i].getRenderOrder(), maxOrder);
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
				setGameObjectLayer(rootObj, FrameDefine.LAYER_UI_BLUR);
			}
			else
			{
				setGameObjectLayer(rootObj, layout.getDefaultLayer());
			}
		}
		// 开启模糊摄像机
		mCameraManager.activeBlurCamera(mBlur);
	}
}