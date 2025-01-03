
// 0到1再到0的折线
public class CurveZeroOneZero : MyCurve
{
	public override float evaluate(float time)
	{
		if(time <= 0.5f)
		{
			return time * 2.0f;
		}
		return 2.0f - time * 2.0f;
	}
}