using UnityEngine;
using System.Collections;

public class txUGUIRawImageCriticalMask : txUGUIRawImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderCriticalMask>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIRawImageCriticalMaskFadeOutLinearDodge : txUGUIRawImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderCriticalMaskFadeOutLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIRawImageFeather : txUGUIRawImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderFeather>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIRawImageGrey : txUGUIRawImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderGrey>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIRawImageLumOffset : txUGUIRawImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderLumOffset>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIRawImageLumOffsetLinearDodge : txUGUIRawImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderLumOffsetLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIRawImageHSLOffset : txUGUIRawImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderHSLOffset>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIRawImageHSLOffsetLinearDodge : txUGUIRawImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderHSLOffsetLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIRawImageMaskCut : txUGUIRawImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderMaskCut>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIRawImagePixelMaskCut : txUGUIRawImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderPixelMaskCut>();
	}
}