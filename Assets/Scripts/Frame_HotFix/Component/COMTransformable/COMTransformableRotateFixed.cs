
// 锁定物体旋转的组件
public class COMTransformableRotateFixed : ComponentRotateFixed
{
	public override void update(float elapsedTime)
	{
		(mComponentOwner as ITransformable).setWorldRotation(mFixedEuler);
		base.update(elapsedTime);
	}
}