
// 弹跳曲线
public class CurveBounceOut : MyCurve
{
	public override float evaluate(float time)
	{
		return bounceEaseOut(time);
	}
}