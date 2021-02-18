using System;
using UnityEngine;

public class WindowShaderGrey : WindowShader
{
	protected bool mIsGrey = false;
	protected int mGreyID;
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