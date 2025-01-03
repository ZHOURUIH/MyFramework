
// 自定义参数集合基类,可进行参数拷贝的
public abstract class CustomParamCopyable : CustomParam
{
	public virtual void initFromCopy(CustomParamCopyable other) { }
}