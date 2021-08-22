using System;
using UnityEngine;

public class WindowShaderBlurMaskVertical : WindowShader
{
	protected float mSampleInterval;
	protected int mSampleIntervalID;
	public WindowShaderBlurMaskVertical()
	{
		mSampleInterval = 1.5f;
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