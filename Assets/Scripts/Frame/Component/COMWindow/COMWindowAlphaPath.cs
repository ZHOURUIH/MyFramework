using System;

public class COMWindowAlphaPath : ComponentPathAlphaNormal, IComponentModifyAlpha
{
	//------------------------------------------------------------------------------------------------------------
	protected override void setValue(float value)
	{
		var obj = mComponentOwner as myUIObject;
		obj.setAlpha(value, false);
	}
}
