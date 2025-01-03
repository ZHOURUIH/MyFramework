
// 五次方曲线
public class CurveQuintIn : MyCurve
{
	public override float evaluate(float time)
	{
		return time * time * time * time * time;
	}
}