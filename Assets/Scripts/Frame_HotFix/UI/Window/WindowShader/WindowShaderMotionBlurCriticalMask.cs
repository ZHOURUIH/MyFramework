using UnityEngine;

// 带临界值遮罩的动态模糊shader
public class WindowShaderMotionBlurCriticalMask : WindowShaderCriticalMask
{
	protected Vector2 mSampleCenter;	// 采样中心
	protected float mMinRange;			// 最小范围,此范围内不进行模糊
	protected float mIncreaseSample;	// 采样增幅
	protected int mMaxSample;			// 最大采样
	protected int mSampleInterval;		// 采样间隔
	protected int mMinRangeID;			// 属性ID
	protected int mMaxSampleID;			// 属性ID
	protected int mIncreaseSampleID;	// 属性ID
	protected int mSampleIntervalID;	// 属性ID
	protected int mCenterXID;			// 属性ID
	protected int mCenterYID;			// 属性ID
	public WindowShaderMotionBlurCriticalMask()
	{
		mMinRange = 300.0f;
		mIncreaseSample = 0.2f;
		mMaxSample = 30;
		mSampleInterval = 3;
		mMinRangeID = Shader.PropertyToID("_MinRange");
		mMaxSampleID = Shader.PropertyToID("_MaxSample");
		mIncreaseSampleID = Shader.PropertyToID("_IncreaseSample");
		mSampleIntervalID = Shader.PropertyToID("_SampleInterval");
		mCenterXID = Shader.PropertyToID("_CenterX");
		mCenterYID = Shader.PropertyToID("_CenterY");
	}
	public void setMinRange(float minRange) { mMinRange = minRange; }
	public void setMaxSample(int maxSample) { mMaxSample = maxSample; }
	public void setIncreaseSample(float increaseValue) { mIncreaseSample = increaseValue; }
	public void setSampleInterval(int interval) { mSampleInterval = interval; }
	public void setSampleCenter(Vector2 center) { mSampleCenter = center; }
	public void setMotionBlur(bool motionBlur)
	{
		if (motionBlur)
		{
			mMaxSample = 30;
			mSampleInterval = 3;
		}
		else
		{
			mMaxSample = 1;
			mSampleInterval = 1;
		}
	}
	public override void applyShader(Material mat)
	{
		base.applyShader(mat);
		if (mat != null && mat.shader != null)
		{
			mat.SetFloat(mMinRangeID, mMinRange);
			mat.SetInt(mMaxSampleID, mMaxSample);
			mat.SetFloat(mIncreaseSampleID, mIncreaseSample);
			mat.SetInt(mSampleIntervalID, mSampleInterval);
			mat.SetFloat(mCenterXID, mSampleCenter.x);
			mat.SetFloat(mCenterYID, mSampleCenter.y);
		}
	}
}