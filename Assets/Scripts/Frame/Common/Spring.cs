using System;

public class Spring : FrameBase
{
	protected float mNormalLength;
	protected float mObjectSpeed;
	protected float mObjectMass;
	protected float mCurLength;
	protected float mMinLength;
	protected float mSpringK;
	protected float mPreAcce;
	protected float mForce;			// 力和速度 只有正负没有方向,正的是沿着拉伸弹簧的方向,负值压缩弹簧的方向
	public Spring()
	{
		mObjectMass = 1.0f;
		mMinLength = 0.5f;
		mSpringK = 1.0f;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mNormalLength = 0.0f;
		mObjectSpeed = 0.0f;
		mObjectMass = 1.0f;
		mCurLength = 0.0f;
		mMinLength = 0.5f;
		mSpringK = 1.0f;
		mPreAcce = 0.0f;
		mForce = 0.0f;
	}
	public void update(float fElaspedTime)
	{
		// 计算拉力
		float elasticForce = calculateElasticForce() * -1.0f;
		// 加速度
		float acceleration = (mForce + elasticForce) / mObjectMass;
		if (isFloatZero(acceleration) || (acceleration < 0.0f && mPreAcce > 0.0f) || (acceleration > 0.0f && mPreAcce < 0.0f))
		{
			mObjectSpeed = 0.0f;
			acceleration = 0.0f;
		}
		else
		{
			// 速度
			mObjectSpeed += acceleration * fElaspedTime;
		}
		// 长度
		mCurLength += mObjectSpeed * fElaspedTime;
		if (mCurLength <= mMinLength)
		{
			mCurLength = mMinLength;
			mObjectSpeed = 0.0f;
			acceleration = 0.0f;
		}
		mPreAcce = acceleration;
	}
	public void setNormaLength(float length)	{ mNormalLength = length; }
	public void setMass(float mass)				{ mObjectMass = mass; }
	public void setSpringk(float k)				{ mSpringK = k; }
	public void setSpeed(float speed)			{ mObjectSpeed = speed; }
	public void setForce(float force)			{ mForce = force; }
	public void setCurLength(float length)		{ mCurLength = length; }
	// 计算拉力 如果为正则是压缩弹簧的方向,为负拉伸弹簧的方向
	public float calculateElasticForce()		{ return (mCurLength - mNormalLength) * mSpringK; }
	public float getSpeed()						{ return mObjectSpeed; }
	public float getLength()					{ return mCurLength; }
	public float getNomalLength()				{ return mNormalLength; }
};