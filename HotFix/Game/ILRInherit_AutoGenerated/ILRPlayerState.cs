using System;
using System.Collections;
using System.Collections.Generic;
	
public class ILRPlayerState : PlayerState
{	
	public override void setPlayer(Character player) { base.setPlayer(player); }
	public override bool canEnter() { return base.canEnter(); }
	public override void enter() { base.enter(); }
	public override void update(float elapsedTime) { base.update(elapsedTime); }
	public override void fixedUpdate(float elapsedTime) { base.fixedUpdate(elapsedTime); }
	public override void leave(bool isBreak, string param) { base.leave(isBreak, param); }
	public override void keyProcess(float elapsedTime) { base.keyProcess(elapsedTime); }
	public override int getPriority() { return base.getPriority(); }
	public override void resetProperty() { base.resetProperty(); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
