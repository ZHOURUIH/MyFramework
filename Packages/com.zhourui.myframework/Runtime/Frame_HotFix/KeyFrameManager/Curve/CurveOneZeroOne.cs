
// 从1到0再到1的折线
public class CurveOneZeroOne : MyCurve
{
	public override float evaluate(float time)
	{
		if(time <= 0.5f)
		{
			return 1.0f - time * 2.0f;
		}
		return 2.0f * time - 1.0f;
	}
}