using System;
using UnityEngine;

public class WindowShaderBlurMaskHorizontal : WindowShader
{
	protected float mSampleInterval = 1.5f;
	protected int mSampleIntervalID;
	public WindowShaderBlurMaskHorizontal()
	{
		mSampleIntervalID = Shader.PropertyToID("_SampleInterval");
	}
	public void setSampleInterval(float sampleInterval) { mSampleInterval = sampleInterval; }
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetFloat(mSampleIntervalID, mSampleInterval);
		}
	}
}