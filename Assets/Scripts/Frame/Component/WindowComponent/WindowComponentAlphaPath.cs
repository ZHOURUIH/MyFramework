using System;

public class WindowComponentAlphaPath : ComponentPathAlphaNormal, IComponentModifyAlpha
{
	//------------------------------------------------------------------------------------------------------------
	protected override void setValue(float value)
	{
		var obj = mComponentOwner as myUIObject;
		obj.setAlpha(value, false);
	}
}
