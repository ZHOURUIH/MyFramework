using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
	
public class ILRCharacter : Character
{	
	protected override CharacterBaseData createCharacterData() { return base.createCharacterData(); }
	public override void init() { base.init(); }
	public override void resetProperty() { base.resetProperty(); }
	public override void destroyModel() { base.destroyModel(); }
	public override float getAnimationLength(string name) { return base.getAnimationLength(name); }
	public override void notifyComponentChanged(GameComponent component) { base.notifyComponentChanged(component); }
	protected override void initComponents() { base.initComponents(); }
	protected override void notifyModelLoaded(GameObject go) { base.notifyModelLoaded(go); }
	public override void destroy() { base.destroy(); }
	public override void setObject(GameObject obj, bool destroyOld = true) { base.setObject(obj, destroyOld); }
	public override void update(float elapsedTime) { base.update(elapsedTime); }
	public override void fixedUpdate(float elapsedTime) { base.fixedUpdate(elapsedTime); }
	public override Vector3 localToWorld(Vector3 point) { return base.localToWorld(point); }
	public override Vector3 worldToLocal(Vector3 point) { return base.worldToLocal(point); }
	public override Vector3 localToWorldDirection(Vector3 direction) { return base.localToWorldDirection(direction); }
	public override Vector3 worldToLocalDirection(Vector3 direction) { return base.worldToLocalDirection(direction); }
	public override GameObject getObject() { return base.getObject(); }
	public override Vector3 getPosition() { return base.getPosition(); }
	public override Vector3 getRotation() { return base.getRotation(); }
	public override Vector3 getScale() { return base.getScale(); }
	public override Vector3 getWorldPosition() { return base.getWorldPosition(); }
	public override Vector3 getWorldScale() { return base.getWorldScale(); }
	public override Vector3 getWorldRotation() { return base.getWorldRotation(); }
	public override bool isActive() { return base.isActive(); }
	public override bool isActiveInHierarchy() { return base.isActiveInHierarchy(); }
	public override bool isHandleInput() { return base.isHandleInput(); }
	public override Collider getCollider() { return base.getCollider(); }
	public override UIDepth getDepth() { return base.getDepth(); }
	public override bool isReceiveScreenMouse() { return base.isReceiveScreenMouse(); }
	public override bool isPassRay() { return base.isPassRay(); }
	public override bool isDragable() { return base.isDragable(); }
	public override bool isMouseHovered() { return base.isMouseHovered(); }
	public override bool isChildOf(IMouseEventCollect parent) { return base.isChildOf(parent); }
	public override void setPassRay(bool passRay) { base.setPassRay(passRay); }
	public override void setHandleInput(bool handleInput) { base.setHandleInput(handleInput); }
	public override void setName(string name) { base.setName(name); }
	public override void setActive(bool active) { base.setActive(active); }
	public override void setPosition(Vector3 pos) { base.setPosition(pos); }
	public override void setScale(Vector3 scale) { base.setScale(scale); }
	public override void setRotation(Vector3 rot) { base.setRotation(rot); }
	public override void setWorldPosition(Vector3 pos) { base.setWorldPosition(pos); }
	public override void setWorldRotation(Vector3 rot) { base.setWorldRotation(rot); }
	public override void setWorldScale(Vector3 scale) { base.setWorldScale(scale); }
	public override void move(Vector3 moveDelta, Space space = Space.Self) { base.move(moveDelta, space); }
	public override void setClickCallback(ObjectClickCallback callback) { base.setClickCallback(callback); }
	public override void setHoverCallback(ObjectHoverCallback callback) { base.setHoverCallback(callback); }
	public override void setPressCallback(ObjectPressCallback callback) { base.setPressCallback(callback); }
	public override void onMouseEnter() { base.onMouseEnter(); }
	public override void onMouseLeave() { base.onMouseLeave(); }
	public override void onMouseDown(Vector3 mousePos) { base.onMouseDown(mousePos); }
	public override void onMouseUp(Vector3 mousePos) { base.onMouseUp(mousePos); }
	public override void onMouseMove(ref Vector3 mousePos, ref Vector3 moveDelta, float moveTime) { base.onMouseMove(ref mousePos, ref moveDelta, moveTime); }
	public override void onMouseStay(Vector3 mousePos) { base.onMouseStay(mousePos); }
	public override void onScreenMouseDown(Vector3 mousePos) { base.onScreenMouseDown(mousePos); }
	public override void onScreenMouseUp(Vector3 mousePos) { base.onScreenMouseUp(mousePos); }
	public override void onReceiveDrag(IMouseEventCollect dragObj, ref bool continueEvent) { base.onReceiveDrag(dragObj, ref continueEvent); }
	public override void onDragHoverd(IMouseEventCollect dragObj, bool hover) { base.onDragHoverd(dragObj, hover); }
	public override void onMultiTouchStart(Vector2 touch0, Vector2 touch1) { base.onMultiTouchStart(touch0, touch1); }
	public override void onMultiTouchMove(Vector2 touch0, Vector2 lastTouch0, Vector2 touch1, Vector2 lastTouch1) { base.onMultiTouchMove(touch0, lastTouch0, touch1, lastTouch1); }
	public override void onMultiTouchEnd() { base.onMultiTouchEnd(); }
	public override void setAlpha(float alpha) { base.setAlpha(alpha); }
	public override float getAlpha() { return base.getAlpha(); }
	public override bool isEnable() { return base.isEnable(); }
	public override void setEnable(bool enable) { base.setEnable(enable); }
	public override void lateUpdate(float elapsedTime) { base.lateUpdate(elapsedTime); }
	public override void notifyAddComponent(GameComponent component) { base.notifyAddComponent(component); }
	public override void setIgnoreTimeScale(bool ignore, bool componentOnly = false) { base.setIgnoreTimeScale(ignore, componentOnly); }
	public override void receiveCommand(Command cmd) { base.receiveCommand(cmd); }
	public override string getName() { return base.getName(); }
	public override void setDestroy(bool isDestroy) { base.setDestroy(isDestroy); }
	public override bool isDestroy() { return base.isDestroy(); }
	public override void setAssignID(UInt64 assignID) { base.setAssignID(assignID); }
	public override UInt64 getAssignID() { return base.getAssignID(); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
