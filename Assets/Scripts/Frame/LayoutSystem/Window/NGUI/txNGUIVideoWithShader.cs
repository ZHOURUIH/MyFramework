using UnityEngine;
using System.Collections;

#if USE_NGUI

public class txNGUIVideoCriticalMask : txNGUIVideo
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderCriticalMask>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUIVideoMotionBlurCriticalMask : txNGUIVideo
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderMotionBlurCriticalMask>();
	}
}

#endif