using System;
using System.Collections;
using System.Collections.Generic;
	
public class ILRCommand : Command
{	
	public override void resetProperty() { base.resetProperty(); }
	public override void execute() { base.execute(); }
	public override void showDebugInfo(MyStringBuilder builder) { base.showDebugInfo(builder); }
	public override void setDestroy(bool isDestroy) { base.setDestroy(isDestroy); }
	public override bool isDestroy() { return base.isDestroy(); }
	public override void setAssignID(UInt64 assignID) { base.setAssignID(assignID); }
	public override UInt64 getAssignID() { return base.getAssignID(); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
