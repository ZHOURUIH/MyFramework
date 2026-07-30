
// 指数曲线
public class CurveExpoOut : MyCurve
{
	public override float evaluate(float time)
	{
		if (time.isFloatEqual(1.0f))
		{
			return 1.0f;
		}
		return -2.0f.pow(-10.0f * time) + 1.0f;
	}
}