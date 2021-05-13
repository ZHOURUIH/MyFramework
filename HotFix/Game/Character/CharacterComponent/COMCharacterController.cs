using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class COMCharacterController : GameComponent
{
	protected CharacterGame mPlayer;
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mPlayer = owner as CharacterGame;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		Vector3 moveDir = Vector3.zero;
		if (getKeyDown(KeyCode.A))
		{
			moveDir += Vector3.left;
		}
		if (getKeyDown(KeyCode.D))
		{
			moveDir += Vector3.right;
		}
		if (getKeyDown(KeyCode.S))
		{
			moveDir += Vector3.down;
		}
		if (getKeyDown(KeyCode.W))
		{
			moveDir += Vector3.up;
		}
		if(getKeyCurrentDown(KeyCode.Q))
		{
			mPlayer.getData().mSpeed += 2.0f;
			GB.mScriptGaming.setSpeed(mPlayer.getData().mSpeed);
		}
		if (getKeyCurrentDown(KeyCode.E))
		{
			mPlayer.getData().mSpeed -= 2.0f;
			clampMin(ref mPlayer.getData().mSpeed, 0.0f);
			GB.mScriptGaming.setSpeed(mPlayer.getData().mSpeed);
		}
		if (!isVectorZero(moveDir))
		{
			moveDir = normalize(moveDir);
			mPlayer.setPosition(mPlayer.getPosition() + moveDir * mPlayer.getData().mSpeed);
			GB.mScriptGaming.setAvatarPosition(mPlayer.getPosition());
		}
	}
}