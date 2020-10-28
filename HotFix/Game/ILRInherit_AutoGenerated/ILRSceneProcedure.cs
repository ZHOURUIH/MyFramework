using System;
using System.Collections;
using System.Collections.Generic;
	
public abstract class ILRSceneProcedure : SceneProcedure
{	
	public override void destroy() { base.destroy(); }
	protected override void onInitFromChild(SceneProcedure lastProcedure, string intent) { base.onInitFromChild(lastProcedure, intent); }
	protected override void onUpdate(float elapsedTime) { base.onUpdate(elapsedTime); }
	protected override void onLateUpdate(float elapsedTime) { base.onLateUpdate(elapsedTime); }
	protected override void onKeyProcess(float elapsedTime) { base.onKeyProcess(elapsedTime); }
	protected override void onExitToChild(SceneProcedure nextProcedure) { base.onExitToChild(nextProcedure); }
	protected override void onExitSelf() { base.onExitSelf(); }
	public override void onNextProcedurePrepared(SceneProcedure nextPreocedure) { base.onNextProcedurePrepared(nextPreocedure); }
	protected override void onPrepareExit(SceneProcedure nextPreocedure) { base.onPrepareExit(nextPreocedure); }
	public override void notifyConstructDone() { base.notifyConstructDone(); }
	public override bool Equals(object obj) { return base.Equals(obj); }
	public override int GetHashCode() { return base.GetHashCode(); }
	public override string ToString() { return base.ToString(); }
}	
