
// 状态参数的基类
public class StateParam : ParamBase
{
	public float mBuffTime = -1.0f;					// 只用作参数存储,不会在buff中引用
	public Character mTarget;						// 附加的目标角色
	public Character mSource;						// 附加的源角色,也就是谁给附加的buff
	public override void resetProperty()
	{
		base.resetProperty();
		mBuffTime = -1.0f;
		mTarget = null;
		mSource = null;
	}
	// 通过拷贝已解析的参数来创建参数时需要调用
	public virtual void copy(StateParam other) { }
}