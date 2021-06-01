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
			mPlayer.getData().mSpeed += 2.0f;
			GB.mScriptGaming.setSpeed(mPlayer.getData().mSpeed);
		}
		if (isKeyCurrentDown(KeyCode.E))
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