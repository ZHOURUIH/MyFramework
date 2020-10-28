using System;
using System.Collections;
using System.Collections.Generic;
	
public class ILRCommand : Command
{	
	public override void init() { base.init(); }
	public override void execute() { base.execute(); }
	public override string showDebugInfo() { return base.showDebugInfo(); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
