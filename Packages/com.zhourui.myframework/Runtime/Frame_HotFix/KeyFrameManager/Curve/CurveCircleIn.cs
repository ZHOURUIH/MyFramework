
// 圆曲线
public class CurveCircleIn : MyCurve
{
	public override float evaluate(float time)
	{
		return -((1.0f - time * time).sqrt() - 1.0f);
	}
}