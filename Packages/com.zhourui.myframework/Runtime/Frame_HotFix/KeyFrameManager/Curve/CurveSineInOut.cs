using static MathUtility;

// 正弦曲线
public class CurveSineInOut : MyCurve
{
	public override float evaluate(float time)
	{
		return -0.5f * ((PI_RADIAN * time).cos() - 1.0f);
	}
}