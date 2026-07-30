
// 指数曲线
public class CurveExpoInOut : MyCurve
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
		if (time < 2.0f)
		{
			return 0.5f * 2.0f.pow(10.0f * (time - 1.0f));
		}
		else
		{
			--time;
			return 0.5f * (-2.0f.pow(-10.0f * time) + 2.0f);
		}
	}
}