
// 立方体曲线
public class CurveCubicIn : MyCurve
{
	public override float evaluate(float time)
	{
		return time * time * time;
	}
}