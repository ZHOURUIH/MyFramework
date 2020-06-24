using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class GameKeyframe : MonoBehaviour
{
	[HideInInspector]
	public AnimationCurve mCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
}