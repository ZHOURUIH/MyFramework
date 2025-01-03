using UnityEngine;

public class CustomAsyncOperation : CustomYieldInstruction
{
	public bool mFinish;
	public override bool keepWaiting { get { return !mFinish; } }
	public override void Reset()
	{
		base.Reset();
		mFinish = false;
	}
}