using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using RenderHeads.Media.AVProVideo;

#if USE_NGUI

public class txNGUIVideoCriticalMask : txNGUIVideo
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderCriticalMask>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUIVideoMotionBlurCriticalMask : txNGUIVideo
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderMotionBlurCriticalMask>();
	}
}

#endif