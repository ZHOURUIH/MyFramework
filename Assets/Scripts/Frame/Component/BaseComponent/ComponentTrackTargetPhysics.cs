using UnityEngine;

// 在物理更新中执行追踪
public class ComponentTrackTargetPhysics : ComponentTrackTarget
{
	public override void fixedUpdate(float elapsedTime)
	{
		if(mTarget != null)
		{
			Vector3 targetPos = getTargetPosition();
			Vector3 curPos = getPosition();
			Vector3 newPos = targetPos;
			bool done = true;
			float moveDelta = mSpeed * elapsedTime;
			if (lengthGreater(targetPos - curPos, moveDelta))
			{
				newPos = normalize(targetPos - curPos) * moveDelta + curPos;
				done = false;
			}
			setPosition(ref newPos);
			mTrackingCallback?.Invoke(this, false);
			if (done)
			{
				mDoneCallback?.Invoke(this, false);
			}
		}
		base.fixedUpdate(elapsedTime);
	}
}