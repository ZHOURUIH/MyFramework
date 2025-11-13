using UnityEngine;

public interface IMouseEventCollect
{
	string getName();
	string getDescription();
	bool isDestroy();
	bool isActive();
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
	int GetHashCode();
}