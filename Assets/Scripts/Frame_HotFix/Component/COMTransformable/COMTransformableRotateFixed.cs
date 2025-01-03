
// 锁定物体旋转的组件
public class COMTransformableRotateFixed : ComponentRotateFixed
{
	public override void update(float elapsedTime)
	{
		(mComponentOwner as Transformable).setWorldRotation(mFixedEuler);
		base.update(elapsedTime);
	}
}