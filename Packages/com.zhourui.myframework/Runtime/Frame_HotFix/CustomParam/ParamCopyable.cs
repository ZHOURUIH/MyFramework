
// 自定义参数集合基类,可进行参数拷贝的
// 只用在必要的地方,比如有些时候有动态修改参数的需求,而且各个对象之间是独立修改的
// 这时候就需要使用ParamCopyable来代替ParamBase,在每次创建对象时都通过参数拷贝的形式去创建新的参数
public abstract class ParamCopyable : ParamBase
{
	public virtual void initFromCopy(ParamCopyable other) { }
}