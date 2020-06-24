using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_NGUI

public class txNGUITextureCriticalMask : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderCriticalMask>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureCriticalMaskFadeOutLinearDodge : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderCriticalMaskFadeOutLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureFeather : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderFeather>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureGrey : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderGrey>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureLumOffset : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderLumOffset>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureLumOffsetLinearDodge : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderLumOffsetLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureLinearDodge : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureHSLOffset : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderHSLOffset>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureHSLOffsetLinearDodge : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderHSLOffsetLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITextureMaskCut : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderMaskCut>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txNGUITexturePixelMaskCut : txNGUITexture
{
	public override void init(GameLayout layout, GameObject go, txUIObject parent)
	{
		base.init(layout, go, parent);
		setWindowShader<WindowShaderPixelMaskCut>();
	}
}

#endif