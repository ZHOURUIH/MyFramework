
// 倒退曲线
public class CurveBackOut : MyCurve
{
	public override float evaluate(float time)
	{
		time -= 1.0f;
		return time * time * ((mOvershootOrAmplitude + 1.0f) * time + mOvershootOrAmplitude) + 1.0f;
	}
}