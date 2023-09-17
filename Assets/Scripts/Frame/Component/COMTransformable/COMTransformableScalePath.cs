using UnityEngine;
using System;

// 使物体按指定的缩放关键帧列表进行缩放的组件
public class COMTransformableScalePath : ComponentPathNormal, IComponentModifyScale
{
	protected override void setValue(Vector3 value)
	{
		(mComponentOwner as Transformable).setScale(value);
	}
}