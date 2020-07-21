using System;
using System.Collections.Generic;
using UnityEngine;

public class WindowShaderBlurMaskHorizontal : WindowShader
{
	protected float mSampleInterval = 1.5f;
	protected string mBlurMaskHorizontal = "BlurMaskHorizontal";
	public void setSampleInterval(float sampleInterval) { mSampleInterval = sampleInterval; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			if (getFileName(mat.shader.name) == mBlurMaskHorizontal)
			{
				mat.SetFloat("_SampleInterval", mSampleInterval);
			}
		}
	}
}