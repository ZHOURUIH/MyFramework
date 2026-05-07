
// 自定义参数集合基类,带模板,子类可将自己的类型用模板传进来
public abstract class ParamCopyableT<T> : ParamCopyable where T : ParamCopyable
{
	public sealed override void initFromCopy(ParamCopyable other) 
	{
		initFromCopyInternal(other as T);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract void initFromCopyInternal(T other);
}