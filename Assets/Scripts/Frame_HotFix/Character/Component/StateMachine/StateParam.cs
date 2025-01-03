
// 状态参数的基类
public class StateParam : ClassObject
{
	protected ParamSet mParamSet;					// buff参数注册的函数列表,用于解析表格的buff参数
	public float mBuffTime;							// 只用作参数存储,不会在buff中引用
	public Character mTarget;						// 附加的目标角色
	public Character mSource;						// 附加的源角色,也就是谁给附加的buff
	public override void resetProperty()
	{
		base.resetProperty();
		mParamSet?.resetProperty();
		mBuffTime = 0.0f;
		mTarget = null;
		mSource = null;
	}
	public void registeParam(StringCallback callback)
	{
		mParamSet ??= new();
		mParamSet.registeParam(callback);
	}
	public void registeParam(FloatCallback callback)
	{
		mParamSet ??= new();
		mParamSet.registeParam(callback);
	}
	// 通过拷贝已解析的参数来创建参数时需要调用
	public virtual void copy(BuffParam other) { }
	// 通过解析表格数据来创建参数时需要调用,因为只需要模板对象注册解析函数,所以不在构造中注册解析函数
	public virtual void registeAllParam() { }
	public virtual void check() { }
	public ParamSet getParamSet() { return mParamSet; }
	public bool setParam(int index, string value) { return mParamSet?.setParam(index, value) ?? false; }
	public int getParamCount() { return mParamSet?.getParamCount() ?? 0; }
}