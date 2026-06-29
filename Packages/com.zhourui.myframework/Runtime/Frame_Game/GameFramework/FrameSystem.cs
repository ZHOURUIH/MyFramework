
// 系统组件基类,一般都是管理器
public class FrameSystem
{
	public virtual void init() { }
	public virtual void update(float elapsedTime) { }
	// 即将销毁时调用,退出程序时会先调用一次全部系统的即将销毁,再全部调用一次销毁
	public virtual void willDestroy() { }
	public virtual void destroy(){}
	public string getName() { return GetType().ToString(); }
}