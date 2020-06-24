using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderGrey : WindowShader
{
	protected bool mIsGrey = false;
	protected string mGrey = "Grey";
	public void setGrey(bool grey){ mIsGrey = grey;}
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			if (mat.shader.name == mGrey)
			{
				mat.SetInt("_Grey", mIsGrey ? 1 : 0);
			}
		}
	}
}