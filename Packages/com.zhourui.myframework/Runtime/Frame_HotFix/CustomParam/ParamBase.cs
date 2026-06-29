
// 存储参数的解析委托
public class ParamBase : ClassObject
{
	public ParamSet mParamSet;                   // 参数注册的函数列表,用于解析表格的参数
	public override void resetProperty()
	{
		base.resetProperty();
		mParamSet?.resetProperty();
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
	// 通过解析表格数据来创建参数时需要调用,因为只需要模板对象注册解析函数,所以不在构造中注册解析函数
	// registeAllParam中需要去调用registeParam来注册所需要的参数
	public virtual void registeAllParam() { }
	public virtual void check() { }
	public ParamSet getParamSet() { return mParamSet; }
	public bool setParam(int index, string value) { return mParamSet?.setParam(index, value) ?? false; }
	public int getParamCount() { return mParamSet?.getParamCount() ?? 0; }
}