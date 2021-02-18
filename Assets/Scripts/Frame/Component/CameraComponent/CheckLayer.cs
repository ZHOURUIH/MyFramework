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
		if (direction == CHECK_DIRECTION.DOWN)
		{
			mDirectionVector = Vector3.down;
		}
		else if (direction == CHECK_DIRECTION.UP)
		{
			mDirectionVector = Vector3.up;
		}
		else if (direction == CHECK_DIRECTION.LEFT)
		{
			mDirectionVector = Vector3.left;
		}
		else if (direction == CHECK_DIRECTION.RIGHT)
		{
			mDirectionVector = Vector3.right;
		}
		else if (direction == CHECK_DIRECTION.FORWARD)
		{
			mDirectionVector = Vector3.forward;
		}
		else if (direction == CHECK_DIRECTION.BACK)
		{
			mDirectionVector = Vector3.back;
		}
		else
		{
			mDirectionVector = Vector3.zero;
		}
	}
}