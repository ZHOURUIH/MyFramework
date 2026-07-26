using UnityEngine;

// 鼠标事件收集接口,所有可交互的UI窗口和3D物体都需要实现此接口
// 提供碰撞检测/深度/拖拽/射线穿透等功能的定义
public interface IMouseEventCollect
{
	string getName();
	string getDescription();
	bool isDestroy();
	bool isActiveInHierarchy();
	bool isHandleInput();
	void onTouchLeave(Vector3 touchPos, int touchID);
	void onTouchEnter(Vector3 touchPos, int touchID);
	void onTouchMove(Vector3 touchPos, Vector3 moveDelta, float moveTime, int touchID);
	void onTouchStay(Vector3 touchPos, int touchID);
	Collider getCollider(bool addIfNotExist = false);
	UIDepth getDepth();
	bool isReceiveScreenTouch();
	void onScreenTouchDown(Vector3 touchPos, int touchID);
	void onScreenTouchUp(Vector3 touchPos, int touchID);
	void onTouchDown(Vector3 touchPos, int touchID);
	void onTouchUp(Vector3 touchPos, int touchID);
	bool isPassRay();
	bool isPassDragEvent();
	void onReceiveDrag(IMouseEventCollect dragObj, Vector3 touchPos, ref bool continueEvent);
	bool isDraggable();
	// 当前对象是否为parent的子节点
	bool isChildOf(IMouseEventCollect parent);
}