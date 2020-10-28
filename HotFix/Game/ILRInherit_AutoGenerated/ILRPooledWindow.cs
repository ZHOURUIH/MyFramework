using System;
using System.Collections;
using System.Collections.Generic;
	
public abstract class ILRPooledWindow : PooledWindow
{	
	public override void setScript(LayoutScript script) { base.setScript(script); }
	public override void init() { base.init(); }
	public override void destroy() { base.destroy(); }
	public override void reset() { base.reset(); }
	public override void recycle() { base.recycle(); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
