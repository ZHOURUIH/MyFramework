
// 拖拽视图循环接口,支持无限循环的拖拽视图需实现此接口
public interface IDragViewLoop
{
	public void updateDragView();
	public bool isActive();
}