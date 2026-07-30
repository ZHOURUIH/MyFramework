using static MathUtility;

// 正弦曲线
public class CurveSineOut : MyCurve
{
	public override float evaluate(float time)
	{
		return (time * HALF_PI_RADIAN).sin();
	}
}