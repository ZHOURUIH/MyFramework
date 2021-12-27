using UnityEngine;
using System.Collections;

// 设置在编辑模式下也执行该脚本
// 用于实现模糊的后处理脚本,挂在摄像机上的
[ExecuteInEditMode]
public class GaussianBlur : MonoBehaviour
{
	private string ShaderName = "Frame/GaussianBlur";	// 指定Shader名称
	public Shader CurShader;							// 着色器实例
	private Material CurMaterial;						// 材质实例
	[Range(0, 6), Tooltip("[降采样次数]向下采样的次数。此值越大,则采样间隔越大,需要处理的像素点越少,运行速度越快。")]
	public int DownSampleNum = 2;						// 降采样次数
	[Range(0.0f, 20.0f), Tooltip("[模糊扩散度]进行高斯模糊时，相邻像素点的间隔。此值越大相邻像素间隔越远，图像越模糊。但过大的值会导致失真。")]
	public float BlurSpreadSize = 3.0f;					// 模糊扩散度
	[Range(0, 8), Tooltip("[迭代次数]此值越大,则模糊操作的迭代次数越多，模糊效果越好，但消耗越大。")]
	public int BlurIterations = 3;						// 迭代次数
	public Color mMaskColor = Color.white;				// 颜色
	Material material
	{
		get
		{
			if (CurMaterial == null)
			{
				CurMaterial = new Material(CurShader);
				CurMaterial.hideFlags = HideFlags.HideAndDontSave;
			}
			return CurMaterial;
		}
	}
	void Start()
	{
		// 找到当前的Shader文件  
		if (CurShader == null)
		{
			CurShader = Shader.Find(ShaderName);
		}
	}
	void OnRenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		// 着色器实例不为空，就进行参数设置  
		if (CurShader != null)
		{
			// 【0】参数准备  
			// 根据向下采样的次数确定宽度系数。用于控制降采样后相邻像素的间隔  
			float widthMod = 1.0f / (1.0f * (1 << DownSampleNum));
			// Shader的降采样参数赋值  
			material.SetFloat("_DownSampleValue", BlurSpreadSize * widthMod);
			material.SetColor("_MaskColor", mMaskColor);
			// 设置渲染模式：双线性  
			sourceTexture.filterMode = FilterMode.Bilinear;
			// 通过右移，准备长、宽参数值  
			int renderWidth = sourceTexture.width >> DownSampleNum;
			int renderHeight = sourceTexture.height >> DownSampleNum;

			// 【1】处理Shader的通道0，用于降采样 ||Pass 0,for down sample  
			// 准备一个缓存renderBuffer，用于准备存放最终数据  
			RenderTexture renderBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, sourceTexture.format);
			// 设置渲染模式：双线性  
			renderBuffer.filterMode = FilterMode.Bilinear;
			// 拷贝sourceTexture中的渲染数据到renderBuffer,并仅绘制指定的pass0的纹理数据  
			Graphics.Blit(sourceTexture, renderBuffer, material, 0);

			// 【2】根据BlurIterations（迭代次数），来进行指定次数的迭代操作  
			for (int i = 0; i < BlurIterations; i++)
			{
				// 【2.1】Shader参数赋值  
				// 迭代偏移量参数  
				float iterationOffs = i * 1.0f;
				// Shader的降采样参数赋值  
				material.SetFloat("_DownSampleValue", BlurSpreadSize * widthMod + iterationOffs);

				// 【2.2】处理Shader的通道1，垂直方向模糊处理 || Pass1,for vertical blur  
				// 定义一个临时渲染的缓存tempBuffer  
				RenderTexture tempBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, sourceTexture.format);
				// 拷贝renderBuffer中的渲染数据到tempBuffer,并仅绘制指定的pass1的纹理数据  
				Graphics.Blit(renderBuffer, tempBuffer, material, 1);
				//  清空renderBuffer  
				RenderTexture.ReleaseTemporary(renderBuffer);
				// 将tempBuffer赋给renderBuffer，此时renderBuffer里面pass0和pass1的数据已经准备好  
				renderBuffer = tempBuffer;

				// 【2.3】处理Shader的通道2，竖直方向模糊处理 || Pass2,for horizontal blur  
				// 获取临时渲染纹理  
				tempBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, sourceTexture.format);
				// 拷贝renderBuffer中的渲染数据到tempBuffer,并仅绘制指定的pass2的纹理数据  
				Graphics.Blit(renderBuffer, tempBuffer, CurMaterial, 2);

				// 【2.4】得到pass0、pass1和pass2的数据都已经准备好的renderBuffer  
				// 再次清空renderBuffer  
				RenderTexture.ReleaseTemporary(renderBuffer);
				// 再次将tempBuffer赋给renderBuffer，此时renderBuffer里面pass0、pass1和pass2的数据都已经准备好  
				renderBuffer = tempBuffer;
			}

			// 拷贝最终的renderBuffer到目标纹理，并绘制所有通道的纹理到屏幕  
			Graphics.Blit(renderBuffer, destTexture);
			// 清空renderBuffer  
			RenderTexture.ReleaseTemporary(renderBuffer);
		}
		// 着色器实例为空，直接拷贝屏幕上的效果。此情况下是没有实现屏幕特效的  
		else
		{
			// 直接拷贝源纹理到目标渲染纹理  
			Graphics.Blit(sourceTexture, destTexture);
		}
	}
	void Update()
	{
		// 若程序没有在运行，去寻找对应的Shader文件  
#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			CurShader = Shader.Find(ShaderName);
		}
#endif
	}
	void OnDisable()
	{
		if (CurMaterial)
		{
			// 立即销毁材质实例
#if UNITY_EDITOR
			DestroyImmediate(CurMaterial);
#else
			Destroy(CurMaterial);
#endif
		}
	}
}