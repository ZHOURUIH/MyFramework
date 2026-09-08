Shader "FastUI/TMP SDF"
{
	Properties
	{
		_MainTex("Font Atlas", 2D) = "white" {}
		[HDR]_FaceColor("Face Color", Color) = (1,1,1,1)
		_FaceDilate("Face Dilate", Range(-1,1)) = 0
		_WeightNormal("Weight Normal", Float) = 0
		[HDR]_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OutlineWidth("Outline Width", Range(0,1)) = 0
		_OutlineSoftness("Outline Softness", Range(0,1)) = 0
		_ScaleRatioA("Scale Ratio A", Float) = 1
		_GradientScale("Gradient Scale", Float) = 5

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
		_UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}
	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				fixed4 vertexColor : COLOR;
				float2 uv : TEXCOORD0;
				float sdfScale : TEXCOORD1;
			};

			sampler2D _MainTex;
			fixed4 _FaceColor;
			float _FaceDilate;
			float _WeightNormal;
			fixed4 _OutlineColor;
			float _OutlineWidth;
			float _OutlineSoftness;
			float _ScaleRatioA;
			float _GradientScale;
			float _UseUIAlphaClip;

			v2f vert(appdata input)
			{
				v2f output;
				output.vertex = UnityObjectToClipPos(input.vertex);
				output.vertexColor = input.color;
				output.uv = input.uv.xy;
				output.sdfScale = max(abs(input.uv.w), 0.000001);
				return output;
			}

			fixed4 frag(v2f input) : SV_Target
			{
				float distanceValue = tex2D(_MainTex, input.uv).a;

				// TMP's face weight / dilate / outline values are expressed around
				// the 0.5 SDF contour. ScaleRatioA is part of TMP's material model.
				float ratioA = max(_ScaleRatioA, 0.000001);
				float faceWeight = (_WeightNormal * 0.25 + _FaceDilate) * ratioA * 0.5;
				float outlineWidth = max(_OutlineWidth, 0.0) * ratioA * 0.5;
				float outlineSoftness = max(_OutlineSoftness, 0.0) * ratioA * 0.5;

				float faceThreshold = 0.5 - faceWeight;
				float outlineThreshold = faceThreshold - outlineWidth;

				// fwidth keeps anti-aliasing screen-space stable. FastText also supplies
				// its local SDF scale in uv.w; use it to keep material softness stable
				// under RectTransform scaling without relying on TMP's CanvasRenderer UV1 packing.
				float aa = max(fwidth(distanceValue), 0.0001);
				float scaleCompensation = max(input.sdfScale, 0.000001);
				float soft = outlineSoftness / scaleCompensation;
				float faceAlpha = smoothstep(faceThreshold - aa, faceThreshold + aa, distanceValue);

				fixed4 faceColor = input.vertexColor * _FaceColor;
				fixed4 result = faceColor;
				result.a *= faceAlpha;

				if (outlineWidth > 0.000001 || outlineSoftness > 0.000001)
				{
					float outlineAlpha = smoothstep(
						outlineThreshold - aa - soft,
						outlineThreshold + aa + soft,
						distanceValue);

					fixed4 outlineColor = _OutlineColor;
					outlineColor.a *= input.vertexColor.a;

					float onlyOutline = saturate(outlineAlpha - faceAlpha);
					float combinedAlpha = result.a + onlyOutline * outlineColor.a;

					if (combinedAlpha > 0.000001)
					{
						float3 combinedRGB =
							result.rgb * result.a +
							outlineColor.rgb * (onlyOutline * outlineColor.a);
						result.rgb = combinedRGB / combinedAlpha;
					}
					result.a = combinedAlpha;
				}

				if (_UseUIAlphaClip > 0.5)
				{
					clip(result.a - 0.001);
				}
				return result;
			}
			ENDCG
		}
	}
}
