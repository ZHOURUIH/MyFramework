// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/CriticalMask"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			sampler2D _MaskTex;
			float4 _MainTex_ST;
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
				fixed4 srcColor = tex2D(_MainTex, i.uv).rgba;
				fixed2 maskUV = i.uv;
				maskUV.y = abs(_InverseVertical - maskUV.y);
				fixed4 maskColor = tex2D(_MaskTex, maskUV).rgba;
				srcColor.a = srcColor.a * ceil(maskColor.r - _CriticalValue);
				return srcColor;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
