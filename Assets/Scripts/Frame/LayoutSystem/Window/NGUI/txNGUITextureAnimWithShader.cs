using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class txNGUITextureAnimCriticalMask : txNGUITextureAnim
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderCriticalMask>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureAnimCriticalMaskFadeOutLinearDodge : txNGUITextureAnim
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderCriticalMaskFadeOutLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureAnimFeather : txNGUITextureAnim
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderFeather>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureAnimGrey : txNGUITextureAnim
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderGrey>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureAnimLumOffset : txNGUITextureAnim
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderLumOffset>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureAnimLumOffsetLinearDodge : txNGUITextureAnim
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderLumOffsetLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureAnimLinearDodge : txNGUITexture
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureAnimHSLOffset : txNGUITextureAnim
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderHSLOffset>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureAnimHSLOffsetLinearDodge : txNGUITextureAnim
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderHSLOffsetLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureAnimMaskCut : txNGUITextureAnim
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderMaskCut>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureAnimPixelMaskCut : txNGUITextureAnim
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderPixelMaskCut>();
	}
}

#endif