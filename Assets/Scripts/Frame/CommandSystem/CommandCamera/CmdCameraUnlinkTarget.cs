using System;
using UnityEngine;

public class CmdCameraUnlinkTarget : Command
{
	public override void resetProperty()
	{
		base.resetProperty();
	}
	public override void execute()
	{
		var camera = mReceiver as GameCamera;
		camera.unlinkTarget();
	}
}