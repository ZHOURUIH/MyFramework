using System;
using UnityEngine;

// 断开摄像机与当前对象的连接
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