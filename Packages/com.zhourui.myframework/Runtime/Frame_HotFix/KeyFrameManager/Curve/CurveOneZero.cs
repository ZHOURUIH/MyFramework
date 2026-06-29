
// 从1到0的直线
public class CurveOneZero : MyCurve
{
	public override float evaluate(float time)
	{
		return 1.0f - time;
	}
}