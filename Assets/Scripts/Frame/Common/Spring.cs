using System;
using static MathUtility;

// 用于模拟弹簧的类
public class Spring : ClassObject
{
	protected float mNormalLength;	// 未施加力时弹簧的长度
	protected float mObjectSpeed;	// 弹簧未固定的一端的移动速度
	protected float mObjectMass;	// 弹簧未固定的一段的质量
	protected float mCurLength;		// 当前弹簧的长度
	protected float mMinLength;		// 弹簧压缩的最小长度
	protected float mSpringK;		// 弹力系数
	protected float mPreAcce;		// 用于保存上一帧的加速度
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