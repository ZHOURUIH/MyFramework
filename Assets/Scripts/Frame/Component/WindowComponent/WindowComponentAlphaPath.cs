using System;

public class WindowComponentAlphaPath : ComponentPathAlphaNormal, IComponentModifyAlpha
{
	//------------------------------------------------------------------------------------------------------------
	protected override void setValue(float value)
	{
		myUIObject obj = mComponentOwner as myUIObject;
		obj.setAlpha(value, false);
	}
}
