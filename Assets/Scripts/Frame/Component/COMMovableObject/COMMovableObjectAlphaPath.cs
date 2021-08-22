using System;

public class COMMovableObjectAlphaPath : ComponentPathAlphaNormal, IComponentModifyAlpha
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(float value)
	{
		(mComponentOwner as MovableObject).setAlpha(value);
	}
}