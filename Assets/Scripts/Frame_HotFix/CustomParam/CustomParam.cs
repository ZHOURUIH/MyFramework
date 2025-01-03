
// 自定义参数集合基类,不可拷贝,只是解析和存储参数
public abstract class CustomParam : ClassObject
{
	public ParamSet mParamSet = new();
	public abstract void registeParams();
	public override void resetProperty()
	{
		base.resetProperty();
		mParamSet?.resetProperty();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void registeParam(StringCallback callback)
	{
		mParamSet ??= new();
		mParamSet.registeParam(callback);
	}
}