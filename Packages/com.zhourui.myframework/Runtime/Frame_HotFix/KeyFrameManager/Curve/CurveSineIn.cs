using static MathUtility;

// 正弦曲线
public class CurveSineIn : MyCurve
{
	public override float evaluate(float time)
	{
		return -(time * HALF_PI_RADIAN).cos() + 1.0f;
	}
}