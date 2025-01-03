
// 角色状态基类,到参数的模板类型,如果CharacterState的子类还有派生,则需要再添加派生类的模板参数类,而不是直接继承此类
public class CharacterStateT<T> : CharacterState where T: StateParam
{
	protected T mCustomParam;                   // 此参数只能在enter中使用,执行完enter后就会回收销毁
	public override void setParam(StateParam param)
	{
		base.setParam(param);
		mCustomParam = param as T;
	}
	public T getCustomParam() { return mCustomParam; }
	public override void resetProperty()
	{
		base.resetProperty();
		mCustomParam = null;
	}
}