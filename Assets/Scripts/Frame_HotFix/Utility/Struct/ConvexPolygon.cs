using System.Collections.Generic;
using UnityEngine;
using static MathUtility;

public class ConvexPolygon : ClassObject
{
	public List<Vector2> mPoints = new();
	public Color mColor = new(randomFloat(0.0f, 1.0f), randomFloat(0.0f, 1.0f), randomFloat(0.0f, 1.0f));
	public override void resetProperty()
	{
		base.resetProperty();
		mPoints.Clear();
		mColor = new(randomFloat(0.0f, 1.0f), randomFloat(0.0f, 1.0f), randomFloat(0.0f, 1.0f));
	}
	public void draw()
	{
		for (int i = 0; i < mPoints.count(); ++i)
		{
			Debug.DrawLine(mPoints[i], mPoints[(i + 1) % mPoints.count()], mColor);
		}
	}
}