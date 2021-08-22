using System;
using UnityEngine;

public class CmdGameCameraUnlinkTarget : Command
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