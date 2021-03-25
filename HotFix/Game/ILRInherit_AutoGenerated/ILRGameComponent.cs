using System;
using System.Collections;
using System.Collections.Generic;
	
public class ILRGameComponent : GameComponent
{	
	public override void init(ComponentOwner owner) { base.init(owner); }
	public override void update(float elapsedTime) { base.update(elapsedTime); }
	public override void fixedUpdate(float elapsedTime) { base.fixedUpdate(elapsedTime); }
	public override void lateUpdate(float elapsedTime) { base.lateUpdate(elapsedTime); }
	public override void destroy() { base.destroy(); }
	public override void resetProperty() { base.resetProperty(); }
	public override void setActive(bool active) { base.setActive(active); }
	public override void setIgnoreTimeScale(bool ignore) { base.setIgnoreTimeScale(ignore); }
	public override void notifyOwnerActive(bool active) { base.notifyOwnerActive(active); }
	public override void setDestroy(bool isDestroy) { base.setDestroy(isDestroy); }
	public override bool isDestroy() { return base.isDestroy(); }
	public override void setAssignID(UInt64 assignID) { base.setAssignID(assignID); }
	public override UInt64 getAssignID() { return base.getAssignID(); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
