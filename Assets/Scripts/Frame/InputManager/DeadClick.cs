using UnityEngine;
using System;

// 已经完成过的单击行为,用于双击检测
public struct DeadClick
{
	public DateTime mClickTime;
	public Vector3 mClickPosition;
	public DeadClick(Vector3 clickPos)
	{
		mClickTime = DateTime.Now;
		mClickPosition = clickPos;
	}
}