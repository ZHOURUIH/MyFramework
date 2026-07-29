using UnityEngine;

// 拖拽视图项接口,可被拖拽的视图项需实现此接口
public interface IDragViewItem
{
	void setPosition(Vector3 pos);
	Vector3 getPosition();
}