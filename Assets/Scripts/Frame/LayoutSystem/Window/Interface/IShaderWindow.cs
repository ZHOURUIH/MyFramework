using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IShaderWindow
{
	void setWindowShader(WindowShader shader);
	WindowShader getWindowShader();
}