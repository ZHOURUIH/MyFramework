using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;

// 几乎没有实质性作用,目前只用于判断一个shader是否为不需要复制就可通用的shader
public class ShaderManager : FrameSystem
{
	protected Dictionary<string, Shader> mShaderList = new();	// 已经获取的shader列表
	protected List<string> mSingleShaderList = new();			// 不可改变参数的shader列表,也就不需要复制多份材质
	public override void init()
	{
		base.init();
		registeSingleShader("Frame/NGUIDefault");
		registeSingleShader("Frame/UGUIDefault");
		registeSingleShader("Frame/UGUIVideo");
		registeSingleShader("Frame/BlurMaskDownSample");
		registeSingleShader("Frame/EdgeAlpha");
		registeSingleShader("Frame/Feather");
		registeSingleShader("Frame/LinearDodge");
		registeSingleShader("Frame/Multiple");
		registeSingleShader("Frame/SnapPixel");
		registeSingleShader("Frame/UGUIOpaque");
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
		return mShaderList.add(name, shader);
	}
	public bool isSingleShader(string shaderName) { return mSingleShaderList.Contains(shaderName); }
	public void registeSingleShader(string shaderName) { mSingleShaderList.addUnique(shaderName); }
}