
// 平方曲线
public class CurveQuadOut : MyCurve
{
	public override float evaluate(float time)
	{
		return -time * (time - 2.0f);
	}
}