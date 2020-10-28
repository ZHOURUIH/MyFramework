using System;
using System.Collections;
using System.Collections.Generic;
	
public class ILRSceneInstance : SceneInstance
{	
	public override void init() { base.init(); }
	public override void destroy() { base.destroy(); }
	public override void update(float elapsedTime) { base.update(elapsedTime); }
	public override void onShow() { base.onShow(); }
	public override void onHide() { base.onHide(); }
	protected override void findGameObject() { base.findGameObject(); }
	protected override void initGameObject() { base.initGameObject(); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
