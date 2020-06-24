using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IDragViewItem
{
	void setPosition(Vector3 pos);
	Vector3 getPosition();
}