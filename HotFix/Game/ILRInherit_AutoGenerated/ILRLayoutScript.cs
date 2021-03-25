using System;
using System.Collections;
using System.Collections.Generic;
	
public abstract class ILRLayoutScript : LayoutScript
{	
	public override void destroy() { base.destroy(); }
	public override void init() { base.init(); }
	public override void update(float elapsedTime) { base.update(elapsedTime); }
	public override void lateUpdate(float elapsedTime) { base.lateUpdate(elapsedTime); }
	public override void onReset() { base.onReset(); }
	public override void onGameState() { base.onGameState(); }
	public override void onDrawGizmos() { base.onDrawGizmos(); }
	public override void onShow(bool immediately, string param) { base.onShow(immediately, param); }
	public override void onHide(bool immediately, string param) { base.onHide(immediately, param); }
	public override void addDelayCmd(Command cmd) { base.addDelayCmd(cmd); }
	public override void interruptCommand(UInt64 assignID, bool showError = true) { base.interruptCommand(assignID, showError); }
	public override void onCmdStarted(Command cmd) { base.onCmdStarted(cmd); }
	public override void interruptAllCommand() { base.interruptAllCommand(); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
