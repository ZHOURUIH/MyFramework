using static MathUtility;

// 振荡曲线
public class CurveElasticOut : MyCurve
{
	public override float evaluate(float time)
	{
		if (time.isZero())
		{
			return 0.0f;
		}
		if (time.isEqual(1.0f))
		{
			return 1.0f;
		}
		float period = 0.3f;
		float s1 = period / TWO_PI_RADIAN * mOvershootOrAmplitude.inverse().asin();
		return mOvershootOrAmplitude * 2.0f.pow(-10.0f * time) * ((time - s1) * TWO_PI_RADIAN / period).sin() + 1.0f;
	}
}