
// 四次方曲线
public class CurveQuartInOut : MyCurve
{
	public override float evaluate(float time)
	{
		if (time * 0.5f < 1.0f)
		{
			return 0.5f * time * time * time * time;
		}
		time -= 2.0f;
		return -0.5f * (time * time * time * time - 2.0f);
	}
}