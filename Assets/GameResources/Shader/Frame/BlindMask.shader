// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/BlindMask"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_RangeWidth("RangeWidth", float) = 100
		_RangeHeight("RangeHeight", float) = 100
		_CenterX("CenterX", float) = 0
		_CenterY("CenterY", float) = 0
		_ScaleX("ScaleX", Range(0, 10)) = 1
		_ScaleY("ScaleY", Range(0, 10)) = 1
		_InnerRadius("InnerRadius", float) = 1000
		_OuterRadius("OuterRadius", float) = 1200
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
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			struct v2f
			{
				float4 vertex : SV_POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			sampler2D _MainTex;
			float _RangeWidth;
			float _RangeHeight;
			float _CenterX;
			float _CenterY;
			float _InnerRadius;
			float _OuterRadius;
			float _ScaleX;
			float _ScaleY;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;
				return o;
			}
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.texcoord);
				// 当前像素坐标
				float2 pixelPos = float2(i.texcoord.x * _RangeWidth, i.texcoord.y * _RangeHeight);
				float2 pixelCenter = float2(_RangeWidth * 0.5 + _CenterX, _RangeHeight * 0.5 + _CenterY);
				float2 disDelta = pixelPos - pixelCenter;
				disDelta.x /= _ScaleX;
				disDelta.y /= _ScaleY;
				float distance = sqrt(disDelta.x * disDelta.x + disDelta.y * disDelta.y);
				// 确保外圈不能小于内圈
				float a = step(0, _OuterRadius - _InnerRadius - 1);
				_OuterRadius = lerp(_InnerRadius + 1, _OuterRadius, a);
				float alpha = (distance - _InnerRadius) / (_OuterRadius - _InnerRadius);
				alpha = (saturate(alpha) + 0.1) * (saturate(alpha) + 0.1) * step(0.0, alpha);
				col.a = i.color.a * saturate(alpha);
				col.rgb *= i.color.rgb;
				return col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
