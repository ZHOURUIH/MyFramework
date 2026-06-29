using static MathUtility;

// 指数曲线
public class CurveExpoOut : MyCurve
{
	public override float evaluate(float time)
	{
		if (isFloatEqual(time, 1.0f))
		{
			return 1.0f;
		}
		return -pow(2.0f, -10.0f * time) + 1.0f;
	}
}