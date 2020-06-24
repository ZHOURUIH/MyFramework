// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/Grey"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Grey("grey", int) = 0
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
			int _Grey;
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
				float grayValue = (col.r + col.g + col.b) * 0.33;
				col.r = col.r * (1 - _Grey) + grayValue * _Grey;
				col.g = col.g * (1 - _Grey) + grayValue * _Grey;
				col.b = col.b * (1 - _Grey) + grayValue * _Grey;
				col.a *= i.color.a;
				return col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
