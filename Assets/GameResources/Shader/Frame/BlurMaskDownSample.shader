// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/BlurMaskDownSample"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		// No culling or depth
		Cull Off
		Lighting On
		ZWrite Off
		Offset -1, -1
		Fog{ Mode Off }
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha
		GrabPass{}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct VertexInput
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
			};

			struct VertexOutput_DownSmpl
			{
				float4 vertex : SV_POSITION;
				half2 uv20 : TEXCOORD0;//一级纹理坐标（右上）
				half2 uv21 : TEXCOORD1;//二级纹理坐标（左下）
				half2 uv22 : TEXCOORD2;//三级纹理坐标（右下）
				half2 uv23 : TEXCOORD3;//四级纹理坐标（左上）
			};

			sampler2D _MainTex;
			uniform sampler2D _GrabTexture;
			float4 _MainTex_ST;
			uniform half4 _GrabTexture_TexelSize;
			VertexOutput_DownSmpl vert(VertexInput v)
			{
				VertexOutput_DownSmpl o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float4 uvgrab = ComputeGrabScreenPos(o.vertex);
				float2 uv = float2(uvgrab.x / uvgrab.w, uvgrab.y / uvgrab.w);
				//对图像的降采样：取像素上下左右周围的点，分别存于四级纹理坐标中  
				o.uv20 = uv + _GrabTexture_TexelSize.xy * half2(1.0h, 1.0h);
				o.uv21 = uv + _GrabTexture_TexelSize.xy * half2(-1.0h, -1.0h);
				o.uv22 = uv + _GrabTexture_TexelSize.xy * half2(1.0h, -1.0h);
				o.uv23 = uv + _GrabTexture_TexelSize.xy * half2(-1.0h, 1.0h);
				return o;
			}
			fixed4 frag(VertexOutput_DownSmpl i) : SV_Target
			{ 
				fixed4 color = fixed4(0, 0, 0, 0);
				color += tex2D(_GrabTexture, i.uv20);
				color += tex2D(_GrabTexture, i.uv21);
				color += tex2D(_GrabTexture, i.uv22);
				color += tex2D(_GrabTexture, i.uv23);
				return color / 4;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
