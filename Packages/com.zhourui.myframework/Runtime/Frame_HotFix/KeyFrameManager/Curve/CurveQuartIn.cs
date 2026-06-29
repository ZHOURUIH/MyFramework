
// 四次方曲线
public class CurveQuartIn : MyCurve
{
	public override float evaluate(float time)
	{
		return time * time * time * time;
	}
}