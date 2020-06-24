// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/MaskCut"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MaskTex ("Texture", 2D) = "white" {}
		_SizeX("Size X", float) = 1.0
		_SizeY("Size Y", float) = 1.0
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
			float _SizeX;	// 对遮挡纹理的缩放
			float _SizeY;	// 对遮挡纹理的缩放

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
				fixed4 srcColor = tex2D(_MainTex, i.uv);
				float2 vDir = i.uv - float2(0.5, 0.5);
				float2 newUV = float2(vDir.x / _SizeX, vDir.y / _SizeY) + float2(0.5, 0.5);
				// 取红色分量作为主纹理采样的透明度
				fixed3 maskColor = tex2D(_MaskTex, newUV).rgb;
				return fixed4(srcColor.rgb, maskColor.r * i.color.a * srcColor.a);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
