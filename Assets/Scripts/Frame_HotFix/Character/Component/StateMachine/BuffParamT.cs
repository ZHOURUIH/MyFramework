
// buff状态参数的基类,主要功能是存储buff参数
public abstract class BuffParamT<T> : BuffParam where T : BuffParam
{
	public sealed override void copy(BuffParam other)
	{
		copyInternal(other as T);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract void copyInternal(T other);
}