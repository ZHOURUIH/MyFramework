using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;

public interface ISocketConnect
{
	string getName();
	void update(float elapsedTime);
	void destroy();
}