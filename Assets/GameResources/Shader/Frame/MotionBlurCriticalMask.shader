// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/MotionBlurCriticalMask"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MinRange("Min Range", float) = 300.0				// 距离中心多少像素才开始模糊
		_MaxSample("Max Sample", int) = 30					// 一个模糊像素的处理最大采样次数
		_IncreaseSample("Increase Sample", float) = 0.2		// 每增加一个像素单位需要增加多少次采样次数
		_SampleInterval("Sample Interval", int) = 1			// 每次采样之间的像素间隔
		_CenterX("Center X", float) = 0.5					// 动态模糊的中心点X
		_CenterY("Center Y", float) = 0.5					// 动态模糊的中心点Y
		_MaskTex("Texture", 2D) = "white" {}
		_CriticalValue("Critical Value", float) = 1
		_InverseVertical("Inverse Vertical", int) = 0
	}
	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"LightMode"="ForwardBase"
		}

		// No culling or depth
		Cull Off
		Lighting Off
		ZWrite Off
		Offset -1, -1
		Fog { Mode Off }
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			
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
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _MinRange;
			int _MaxSample;
			float _IncreaseSample;
			int _SampleInterval;
			uniform half4 _MainTex_TexelSize;
			float _CenterX;
			float _CenterY;
			sampler2D _MaskTex;
			float _CriticalValue;
			int _InverseVertical;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				o.color = v.color;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// 动态模糊处理
				// 转换为以像素为单位的坐标
				float2 pixelPos = float2(_MainTex_TexelSize.z * i.uv.x, _MainTex_TexelSize.w * i.uv.y);
				float2 pixelCenter = float2(_MainTex_TexelSize.z * _CenterX, _MainTex_TexelSize.w * _CenterY);
				float2 dir = pixelPos - pixelCenter;
				float pixelLen = length(dir);
				dir = normalize(dir);
				int sampleCount = (int)((pixelLen - _MinRange) * _IncreaseSample);
				sampleCount = clamp(sampleCount, _SampleInterval, _MaxSample) / _SampleInterval;
				fixed4 finalColor = float4(0.0f, 0.0f, 0.0f, 0.0f);
				for (int k = 0; k < 100; ++k)
				{
					if (k >= sampleCount)
					{
						break;
					}
					float2 samplePos = pixelPos + dir * k * _SampleInterval;
					samplePos.x = clamp(samplePos.x * _MainTex_TexelSize.x, 0.0f, 1.0f);
					samplePos.y = clamp(samplePos.y * _MainTex_TexelSize.y, 0.0f, 1.0f);
					fixed4 curColor = tex2D(_MainTex, float2(samplePos.x, samplePos.y)).rgba;
					finalColor += curColor;
				}
				finalColor /= sampleCount;
				// 裁剪处理
				fixed2 maskUV = i.uv;
				maskUV.y = abs(_InverseVertical - maskUV.y);
				fixed4 maskColor = tex2D(_MaskTex, maskUV).rgba;
				finalColor.a = finalColor.a * ceil(maskColor.r - _CriticalValue);
				return finalColor;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
