// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/LumOffset"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LumOffset ("Lum Offset", float) = 0
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
			#pragma target 3.0
			
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
			float _LumOffset;

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
				// 颜色值限定在0-1之间
				_LumOffset += 1.0;
				col.r *= _LumOffset;
				col.g *= _LumOffset;
				col.b *= _LumOffset;
				col.r = clamp(col.r, 0.0, 1.0);
				col.g = clamp(col.g, 0.0, 1.0);
				col.b = clamp(col.b, 0.0, 1.0);
				col.a *= i.color.a;
				return col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
