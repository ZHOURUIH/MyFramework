using UnityEngine;
using System;
using System.Collections;

public class MovableObjectComponentAlphaPath : ComponentPathAlphaNormal, IComponentModifyAlpha
{
	//------------------------------------------------------------------------------------------------------------
	protected override void setValue(float value)
	{
		(mComponentOwner as MovableObject).setAlpha(value);
	}
}
