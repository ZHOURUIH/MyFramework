using UnityEngine;

public struct CheckLayer
{
	public CHECK_DIRECTION mDirection;
	public Vector3 mDirectionVector;
	public float mCheckDistance;
	public float mMinDistance;
	public int mLayerIndex;
	public CheckLayer(int layerIndex, CHECK_DIRECTION direction, float checkDistance, float minDistance)
	{
		mLayerIndex = layerIndex;
		mDirection = direction;
		mCheckDistance = checkDistance;
		mMinDistance = minDistance;
		switch (direction)
		{
			case CHECK_DIRECTION.DOWN:		mDirectionVector = Vector3.down;	break;
			case CHECK_DIRECTION.UP:		mDirectionVector = Vector3.up;		break;
			case CHECK_DIRECTION.LEFT:		mDirectionVector = Vector3.left;	break;
			case CHECK_DIRECTION.RIGHT:		mDirectionVector = Vector3.right;	break;
			case CHECK_DIRECTION.FORWARD:	mDirectionVector = Vector3.forward; break;
			case CHECK_DIRECTION.BACK:		mDirectionVector = Vector3.back;	break;
			default:						mDirectionVector = Vector3.zero;	break;
		}
	}
}