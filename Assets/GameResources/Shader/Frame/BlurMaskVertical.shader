// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/BlurMaskVertical"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_SampleInterval("Sample Interval", float) = 1.5
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

			struct appdata
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 uv : TEXCOORD0;
			};
			struct v2f
			{
				float4 vertex : SV_POSITION;
				half4 color : COLOR;
				float2 uv : TEXCOORD0;
				float4 uvgrab : TEXCOORD1;
			};

			// rho为标准差
			float GetGaussianDistribution(float x, float y, float rho)
			{
				float g = 1.0f / sqrt(2.0f * 3.141592654f * rho * rho);
				return g * exp(-(x * x + y * y) / (2 * rho * rho));
			}

			sampler2D _MainTex;
			uniform sampler2D _GrabTexture;
			float4 _MainTex_ST;
			uniform half4 _GrabTexture_TexelSize;
			float _SampleInterval;
			const static int BlurRadius = 3;
			//【5】准备高斯模糊权重矩阵参数7x4的矩阵 ||  Gauss Weight  
			static const half4 GaussWeight[2 * BlurRadius + 1] =
			{
				half4(0.0205,0.0205,0.0205,1),
				half4(0.0855,0.0855,0.0855,1),
				half4(0.232,0.232,0.232,1),
				half4(0.324,0.324,0.324,1),
				half4(0.232,0.232,0.232,1),
				half4(0.0855,0.0855,0.0855,1),
				half4(0.0205,0.0205,0.0205,1)
			};
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				o.color = v.color;
				o.uvgrab = ComputeGrabScreenPos(o.vertex);
				return o;
			}
			fixed4 frag(v2f i) : SV_Target
			{
				float2 grabUV = float2(i.uvgrab.x / i.uvgrab.w, i.uvgrab.y / i.uvgrab.w);
				fixed4 totalColor = fixed4(0, 0, 0, 0);
				int sampleCount = BlurRadius * 2 + 1;
				for (int ii = 0; ii < sampleCount; ++ii)
				{
					float2 uvOffset = float2(_GrabTexture_TexelSize.x, (ii - BlurRadius) * _GrabTexture_TexelSize.y * _SampleInterval);
					totalColor += tex2D(_GrabTexture, grabUV + uvOffset).rgba * GaussWeight[ii];
				}
				totalColor.a *= i.color.a;
				totalColor.rgb *= i.color.rgb;
				return totalColor;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
