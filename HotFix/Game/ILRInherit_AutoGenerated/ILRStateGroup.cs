using System;
using System.Collections;
using System.Collections.Generic;
	
public class ILRStateGroup : StateGroup
{	
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
