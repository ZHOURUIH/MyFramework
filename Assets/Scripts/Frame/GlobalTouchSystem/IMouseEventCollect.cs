using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface IMouseEventCollect
{
	string getName();
	void onMultiTouchStart(Vector2 touch0, Vector2 touch1);
	void onMultiTouchMove(Vector2 touch0, Vector2 lastTouch0, Vector2 touch1, Vector2 lastTouch1);
	void onMultiTouchEnd();
	bool isActive();
	bool isHandleInput();
	void onMouseLeave();
	void onMouseEnter();
	void onMouseMove(ref Vector3 mousePos, ref Vector3 moveDelta, float moveTime);
	void onMouseStay(Vector3 mousePos);
	Collider getCollider(bool addIfNull = false);
	UIDepth getUIDepth();
	bool isReceiveScreenMouse();
	void onScreenMouseDown(Vector3 mousePos);
	void onScreenMouseUp(Vector3 mousePos);
	void onMouseDown(Vector3 mousePos);
	void onMouseUp(Vector3 mousePos);
	bool isPassRay();
	void setPassRay(bool passRay);
	void setClickCallback(ObjectClickCallback callback);
	void setHoverCallback(ObjectHoverCallback callback);
	void setPressCallback(ObjectPressCallback callback);
	void onReceiveDrag(IMouseEventCollect dragObj, ref bool continueEvent);
	void onDragHoverd(IMouseEventCollect dragObj, bool hover);
	bool isDragable();
}