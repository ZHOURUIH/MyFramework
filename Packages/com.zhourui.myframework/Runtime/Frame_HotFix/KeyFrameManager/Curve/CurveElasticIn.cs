using static MathUtility;

// 振荡曲线
public class CurveElasticIn : MyCurve
{
	public override float evaluate(float time)
	{
		if (time.isFloatZero())
		{
			return 0.0f;
		}
		if (time.isFloatEqual(1.0f))
		{
			return 1.0f;
		}
		float period = 0.3f;
		float s0 = period / TWO_PI_RADIAN * 1.divide(mOvershootOrAmplitude).asin();
		time -= 1.0f;
		return -(mOvershootOrAmplitude * 2.0f.pow(10.0f * time) * ((time - s0) * TWO_PI_RADIAN / period).sin());
	}
}