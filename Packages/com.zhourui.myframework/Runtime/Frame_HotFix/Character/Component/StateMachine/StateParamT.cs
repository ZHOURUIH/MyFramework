
// buff状态参数的基类,主要功能是存储buff参数
public abstract class BuffParamT<T> : StateParam where T : StateParam
{
	public sealed override void copy(StateParam other)
	{
		copyInternal(other as T);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract void copyInternal(T other);
}