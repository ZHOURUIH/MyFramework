using System;
using UnityEngine;

[Serializable]
public struct ComplexPoint
{
	public float mRelative;
	public int mAbsolute;
	public void setRelative(float relative) { mRelative = relative; }
	public void setAbsolute(float absolute) { mAbsolute = (int)(absolute + 0.5f * Mathf.Sign(absolute)); }
}