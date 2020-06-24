using System;
using System.Collections.Generic;
using UnityEngine;

public class ShaderManager : FrameComponent
{
	protected Dictionary<string, Shader> mShaderList;
	protected List<string> mSingleShaderList;
	public ShaderManager(string name)
		:base(name)
	{
		mShaderList = new Dictionary<string, Shader>();
		mSingleShaderList = new List<string>();
	}
	public override void init()
	{
		base.init();
		mShaderManager.registeSingleShader("NGUIDefault");
		mShaderManager.registeSingleShader("UGUIDefault");
		mShaderManager.registeSingleShader("UGUIVideo");
		mShaderManager.registeSingleShader("BlurMaskDownSample");
		mShaderManager.registeSingleShader("EdgeAlpha");
		mShaderManager.registeSingleShader("Feather");
		mShaderManager.registeSingleShader("LinearDodge");
		mShaderManager.registeSingleShader("Multiple");
	}
	public override void destroy()
	{
		// shader不主动卸载
		mShaderList.Clear();
		base.destroy();
	}
	public Shader getShader(string name)
	{
		if(mShaderList.ContainsKey(name))
		{
			return mShaderList[name];
		}
		Shader shader = Shader.Find(name);
		if(shader != null)
		{
			mShaderList.Add(name, shader);
		}
		else
		{
			logError("can not find shader : " + name);
		}
		return shader;
	}
	public bool isSingleShader(string shaderName)
	{
		return mSingleShaderList.Contains(shaderName);
	}
	public void registeSingleShader(string shaderName)
	{
		if(!mSingleShaderList.Contains(shaderName))
		{
			mSingleShaderList.Add(shaderName);
		}
	}
}