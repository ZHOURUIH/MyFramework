using UnityEngine;
using static GBH;
using static FrameUtility;
using static MathUtility;

public class COMCharacterController : GameComponent
{
	protected CharacterGame mPlayer;
	protected CharacterGameData mPlayerData;
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mPlayer = owner as CharacterGame;
		mPlayerData = mPlayer.getData();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		Vector3 moveDir = Vector3.zero;
		if (isKeyDown(KeyCode.A))
		{
			moveDir += Vector3.left;
		}
		if (isKeyDown(KeyCode.D))
		{
			moveDir += Vector3.right;
		}
		if (isKeyDown(KeyCode.S))
		{
			moveDir += Vector3.down;
		}
		if (isKeyDown(KeyCode.W))
		{
			moveDir += Vector3.up;
		}
		if(isKeyCurrentDown(KeyCode.Q))
		{
			mPlayerData.mSpeed += 2.0f;
			mUIGame.setSpeed(mPlayerData.mSpeed);
		}
		if (isKeyCurrentDown(KeyCode.E))
		{
			mPlayerData.mSpeed = clampMin(mPlayerData.mSpeed - 2.0f, 0.0f);
			mUIGame.setSpeed(mPlayerData.mSpeed);
		}
		if (!isVectorZero(moveDir))
		{
			mPlayer.setPosition(mPlayer.getPosition() + normalize(moveDir) * mPlayerData.mSpeed);
			mUIGame.setAvatarPosition(mPlayer.getPosition());
		}
	}
}