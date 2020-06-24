// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/CriticalMaskFadeOutLinearDodge"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MaskTex("Texture", 2D) = "white" {}
		_FadeOutTex("Texture", 2D) = "black" {}
		_CriticalValue("Critical Value", float) = 1
		_FadeOutCriticalValue("Fade Out Critical Value", float) = 0
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
		Blend SrcAlpha DstAlpha

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
			sampler2D _MaskTex;
			sampler2D _FadeOutTex;
			float4 _MainTex_ST;
			float _CriticalValue;
			float _FadeOutCriticalValue;
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
				fixed4 srcColor = tex2D(_MainTex, i.uv).rgba;
				fixed2 maskUV = i.uv;
				maskUV.y = abs(_InverseVertical - maskUV.y);
				fixed4 maskColor = tex2D(_MaskTex, maskUV).rgba;
				srcColor.a = srcColor.a * ceil(maskColor.r - _CriticalValue);
				// 当前绿色分量减去临界值表示距离,距离小于0时不显示,大于0且小于1时,距离越远透明度越低
				fixed4 fadeOutColor = tex2D(_FadeOutTex, i.uv).rgba;
				float distance = 1 - fadeOutColor.g - _FadeOutCriticalValue;
				distance = distance * ceil(distance) * 3;
				distance = clamp(distance, 0, 1);
				srcColor.a = srcColor.a * pow(distance, 2);
				return srcColor;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
