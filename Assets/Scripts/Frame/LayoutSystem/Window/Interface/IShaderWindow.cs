using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IShaderWindow
{
	void setWindowShader<T>() where T : WindowShader, new();
	T getWindowShader<T>() where T : WindowShader;
}