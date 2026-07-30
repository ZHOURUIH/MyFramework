
// 圆曲线
public class CurveCircleInOut : MyCurve
{
	public override float evaluate(float time)
	{
		time *= 2.0f;
		if (time < 1.0f)
		{
			return -0.5f * ((1.0f - time * time).sqrt() - 1.0f);
		}
		time -= 2.0f;
		return 0.5f * ((1.0f - time * time).sqrt() + 1.0f);
	}
}