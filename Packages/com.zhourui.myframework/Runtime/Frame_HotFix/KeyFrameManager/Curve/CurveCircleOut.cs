
// 圆曲线
public class CurveCircleOut : MyCurve
{
	public override float evaluate(float time)
	{
		time -= 1.0f;
		return (1.0f - time * time).sqrt();
	}
}