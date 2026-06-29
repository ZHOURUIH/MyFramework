using UnityEngine;

public class CustomAsyncOperation : CustomYieldInstruction
{
	protected bool mFinish;
	public override bool keepWaiting { get { return !mFinish; } }
	public override void Reset()
	{
		base.Reset();
		mFinish = false;
	}
	public CustomAsyncOperation setFinish() 
	{
		mFinish = true;
		return this; 
	}
}