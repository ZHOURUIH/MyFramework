// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/Feather"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
				// 当前采样点到中心的距离
                float dstSq = distance(i.uv, float2(0.5, 0.5));
				// 最远距离
				float max = sqrt(2.0) / 2.0;
				// 根据指数衰减计算出的透明度,为了保证中心大部分区域透明度都为1,所以扩大了范围,然后再限定
				float a = clamp(pow((max - dstSq) * 1.8, 3.0) * 1.8, 0.0, 1.0);
				return fixed4(srcColor.rgb, a * i.color.a * srcColor.a);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
