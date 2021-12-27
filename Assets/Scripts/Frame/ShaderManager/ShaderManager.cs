using System;
using System.Collections.Generic;
using UnityEngine;

// 几乎没有实质性作用,目前只用于判断一个shader是否为不需要复制就可通用的shader
public class ShaderManager : FrameSystem
{
	protected Dictionary<string, Shader> mShaderList;	// 已经获取的shader列表
	protected List<string> mSingleShaderList;			// 不可改变参数的shader列表,也就不需要复制多份材质
	public ShaderManager()
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
		registeSingleShader("UGUIOpaque");
	}
	public override void destroy()
	{
		// shader不主动卸载
		mShaderList.Clear();
		base.destroy();
	}
	public Shader getShader(string name)
	{
		if (mShaderList.TryGetValue(name, out Shader shader))
		{
			return shader;
		}
		shader = Shader.Find(name);
		if (shader == null)
		{
			logError("can not find shader : " + name);
		}
		mShaderList.Add(name, shader);
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