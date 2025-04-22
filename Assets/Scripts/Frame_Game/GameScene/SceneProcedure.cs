
// 场景流程
public abstract class SceneProcedure
{
	// 由GameScene调用
	// 进入流程
	public virtual void init(){}
	// 更新流程
	public virtual void update(float elapsedTime){}
	// 退出流程
	public virtual void exit(){}
	public virtual void willDestroy(){}
}