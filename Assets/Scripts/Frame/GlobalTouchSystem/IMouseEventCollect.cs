using UnityEngine;

public interface IMouseEventCollect
{
	GameObject getObject();
	string getName();
	string getDescription();
	void onMultiTouchStart(Vector3 touch0, Vector3 touch1);
	void onMultiTouchMove(Vector3 touch0, Vector3 lastTouch0, Vector3 touch1, Vector3 lastTouch1);
	void onMultiTouchEnd();
	bool isDestroy();
	bool isActive();
	bool isActiveInHierarchy();
	bool isHandleInput();
	void onMouseLeave(int touchID);
	void onMouseEnter(int touchID);
	void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID);
	void onMouseStay(Vector3 mousePos, int touchID);
	Collider getCollider();
	UIDepth getDepth();
	bool isReceiveScreenMouse();
	void onScreenMouseDown(Vector3 mousePos, int touchID);
	void onScreenMouseUp(Vector3 mousePos, int touchID);
	void onMouseDown(Vector3 mousePos, int touchID);
	void onMouseUp(Vector3 mousePos, int touchID);
	bool isPassRay();
	void setPassRay(bool passRay);
	void setClickCallback(ObjectClickCallback callback);
	void setHoverCallback(ObjectHoverCallback callback);
	void setPressCallback(ObjectPressCallback callback);
	void onReceiveDrag(IMouseEventCollect dragObj, BOOL continueEvent);
	void onDragHoverd(IMouseEventCollect dragObj, bool hover);
	bool isDragable();
	// 当前对象是否为parent的子节点
	bool isChildOf(IMouseEventCollect parent);
}