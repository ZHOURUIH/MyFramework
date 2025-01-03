using UnityEngine;

public interface IMouseEventCollect
{
	string getName();
	string getDescription();
	bool isDestroy();
	bool isActive();
	bool isActiveInHierarchy();
	bool isHandleInput();
	void onMouseLeave(Vector3 mousePos, int touchID);
	void onMouseEnter(Vector3 mousePos, int touchID);
	void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID);
	void onMouseStay(Vector3 mousePos, int touchID);
	Collider getCollider(bool addIfNotExist = false);
	UIDepth getDepth();
	bool isReceiveScreenMouse();
	void onScreenMouseDown(Vector3 mousePos, int touchID);
	void onScreenMouseUp(Vector3 mousePos, int touchID);
	void onMouseDown(Vector3 mousePos, int touchID);
	void onMouseUp(Vector3 mousePos, int touchID);
	bool isPassRay();
	bool isPassDragEvent();
	void onReceiveDrag(IMouseEventCollect dragObj, Vector3 mousePos, ref bool continueEvent);
	bool isDragable();
	// 当前对象是否为parent的子节点
	bool isChildOf(IMouseEventCollect parent);
	int GetHashCode();
}