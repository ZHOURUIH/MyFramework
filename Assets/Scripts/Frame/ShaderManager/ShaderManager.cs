using System;
using System.Collections.Generic;
using UnityEngine;

// 几乎没有实质性作用,目前只用于判断一个shader是否为不需要复制就可通用的shader
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
		registeSingleShader("NGUIDefault");
		registeSingleShader("UGUIDefault");
		registeSingleShader("UGUIVideo");
		registeSingleShader("BlurMaskDownSample");
		registeSingleShader("EdgeAlpha");
		registeSingleShader("Feather");
		registeSingleShader("LinearDodge");
		registeSingleShader("Multiple");
		registeSingleShader("SnapPixel");
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