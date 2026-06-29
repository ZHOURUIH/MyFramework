
// 弹跳曲线
public class CurveBounceInOut : MyCurve
{
	public override float evaluate(float time)
	{
		return bounceEaseInOut(time);
	}
}