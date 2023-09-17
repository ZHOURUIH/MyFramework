using System;

// 物体的以指定透明度列表变化的组件
public class COMMovableObjectAlphaPath : ComponentPathAlphaNormal, IComponentModifyAlpha
{
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void setValue(float value)
	{
		(mComponentOwner as MovableObject).setAlpha(value);
	}
}