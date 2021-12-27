using System;
using UnityEngine;

// 变灰shader
public class WindowShaderGrey : WindowShader
{
	protected bool mIsGrey;		// 是否变灰
	protected int mGreyID;		// 属性ID
	public WindowShaderGrey()
	{
		mGreyID = Shader.PropertyToID("_Grey");
	}
	public void setGrey(bool grey){ mIsGrey = grey;}
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetInt(mGreyID, mIsGrey ? 1 : 0);
		}
	}
}