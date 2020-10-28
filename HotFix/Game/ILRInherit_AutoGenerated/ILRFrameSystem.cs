using System;
using System.Collections;
using System.Collections.Generic;
	
public class ILRFrameSystem : FrameSystem
{	
	public override void init() { base.init(); }
	public override void destroy() { base.destroy(); }
	public override void onDrawGizmos() { base.onDrawGizmos(); }
	public override void setActive(bool active) { base.setActive(active); }
	public override void update(float elapsedTime) { base.update(elapsedTime); }
	public override void lateUpdate(float elapsedTime) { base.lateUpdate(elapsedTime); }
	public override void fixedUpdate(float elapsedTime) { base.fixedUpdate(elapsedTime); }
	public override void notifyAddComponent(GameComponent component) { base.notifyAddComponent(component); }
	public override void notifyComponentDetached(GameComponent component) { base.notifyComponentDetached(component); }
	public override void notifyComponentAttached(GameComponent component) { base.notifyComponentAttached(component); }
	public override void notifyComponentDestroied(GameComponent component) { base.notifyComponentDestroied(component); }
	public override void setIgnoreTimeScale(bool ignore, bool componentOnly = false) { base.setIgnoreTimeScale(ignore, componentOnly); }
	public override void resetProperty() { base.resetProperty(); }
	protected override void initComponents() { base.initComponents(); }
	public override void receiveCommand(Command cmd) { base.receiveCommand(cmd); }
	public override string getName() { return base.getName(); }
	public override void setName(string name) { base.setName(name); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
