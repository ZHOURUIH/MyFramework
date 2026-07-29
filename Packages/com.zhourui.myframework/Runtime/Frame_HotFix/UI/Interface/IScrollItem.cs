
// 滚动项接口,滚动容器中的项需实现此接口
public interface IScrollItem
{
	void lerpItem(IScrollContainer curItem, IScrollContainer nextItem, float percent);
	myUGUIObject getItemRoot();
}