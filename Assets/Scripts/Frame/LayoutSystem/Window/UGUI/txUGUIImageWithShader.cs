using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class txUGUIImageCriticalMask : txUGUIImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderCriticalMask>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIImageCriticalMaskFadeOutLinearDodge : txUGUIImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderCriticalMaskFadeOutLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIImageFeather : txUGUIImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderFeather>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIImageGrey : txUGUIImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderGrey>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIImageLumOffset : txUGUIImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderLumOffset>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIImageLumOffsetLinearDodge : txUGUIImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderLumOffsetLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIImageHSLOffset : txUGUIImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderHSLOffset>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIImageHSLOffsetLinearDodge : txUGUIImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderHSLOffsetLinearDodge>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIImageMaskCut : txUGUIImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderMaskCut>();
	}
}
//---------------------------------------------------------------------------------------------------------------------------
public class txUGUIImagePixelMaskCut : txUGUIImage
{
	public override void init(GameObject go, txUIObject parent)
	{
		base.init(go, parent);
		setWindowShader<WindowShaderPixelMaskCut>();
	}
}