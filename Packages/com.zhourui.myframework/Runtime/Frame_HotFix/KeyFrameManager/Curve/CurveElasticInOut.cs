using static MathUtility;

// 振荡曲线
public class CurveElasticInOut : MyCurve
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
		float period = 0.45f;
		float s = period / TWO_PI_RADIAN * 1.0f.divide(mOvershootOrAmplitude).asin();
		if (time < 1.0f)
		{
			time -= 1.0f;
			return -0.5f * (mOvershootOrAmplitude * 2.0f.pow(10.0f * time) * ((time - s) * TWO_PI_RADIAN / period)).sin();
		}
		else
		{
			time -= 1.0f;
			return mOvershootOrAmplitude * 2.0f.pow(-10.0f * time) * ((time - s) * TWO_PI_RADIAN / period).sin() * 0.5f + 1.0f;
		}
	}
}