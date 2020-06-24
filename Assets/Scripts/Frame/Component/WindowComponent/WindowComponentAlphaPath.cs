using UnityEngine;
using System;
using System.Collections;

public class WindowComponentAlphaPath : ComponentPathAlphaNormal, IComponentModifyAlpha
{
	//------------------------------------------------------------------------------------------------------------
	protected override void setValue(float value)
	{
		txUIObject obj = mComponentOwner as txUIObject;
		// 因为NGUI中透明度小于0.001时认为是将窗口隐藏,会重新构建网格顶点,所以此处最低为0.002
		if (WidgetUtility.isNGUI(obj.getObject()))
		{
			clampMin(ref value, 0.002f);
		}
		obj.setAlpha(value, false);
	}
}
