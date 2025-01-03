
// 自定义参数集合基类,带模板,子类可将自己的类型用模板传进来
public abstract class CustomParamCopyableT<T> : CustomParamCopyable where T : CustomParamCopyable
{
	public sealed override void initFromCopy(CustomParamCopyable other) 
	{
		initFromCopyInternal(other as T);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract void initFromCopyInternal(T other);
}