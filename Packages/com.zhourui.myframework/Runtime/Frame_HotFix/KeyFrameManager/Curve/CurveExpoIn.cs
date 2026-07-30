
// 指数曲线
public class CurveExpoIn : MyCurve
{
	public override float evaluate(float time)
	{
		if (time.isFloatZero())
		{
			return 0.0f;
		}
		return 2.0f.pow(10.0f * (time - 1.0f));
	}
}