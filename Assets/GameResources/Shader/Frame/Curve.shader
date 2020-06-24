// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/Curve"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MinY ("MinY", float) = 0
		_MaxY ("MaxY", float) = 800
		_Alpha("Alpha", float) = 1
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
				half4 vertexcolor : COLOR1;
			};

			float HueToRGB(float v1, float v2, float vH)
			{
				if (vH < 0.0)
				{
					vH += 1.0;
				}
				if (vH > 1.0)
				{
					vH -= 1.0;
				}
				if (6.0 * vH < 1.0)
				{
					return v1 + (v2 - v1) * 6.0 * vH;
				}
				else if (2.0 * vH < 1.0)
				{
					return v2;
				}
				else if (3.0 * vH < 2.0)
				{
					return v1 + (v2 - v1) * (0.67 - vH) * 6.0;
				}
				else
				{
					return v1;
				}
			}
			
			// 色相(H),饱和度(S),亮度(L),转换为rgb
			// HSL和RGB的范围都是0-1
			float3 HSLToRGB(float3 hsl)
			{
				float3 rgb;
				float H = hsl.r;
				float S = hsl.g;
				float L = hsl.b;
				if (S == 0.0)						//HSL from 0 to 1
				{
					rgb.r = L;				//RGB results from 0 to 255
					rgb.g = L;
					rgb.b = L;
				}
				else
				{
					float var2;
					if (L < 0.5)
					{
						var2 = L * (1.0 + S);
					}
					else
					{
						var2 = L + S - (S * L);
					}
			
					float var1 = 2.0 * L - var2;
					rgb.r = HueToRGB(var1, var2, H + 0.33);
					rgb.g = HueToRGB(var1, var2, H);
					rgb.b = HueToRGB(var1, var2, H - 0.33);
				}
				return rgb;
			}

			float clampValue(float value, float min, float max)
			{
				if (value < min)
				{
					return min;
				}
				else if (value > max)
				{
					return max;
				}
				return value;
			}
			float inverseLerp(float value, float min, float max)
			{
				return (value - min) / (max - min);
			}
			
			float _MinY = 0.0;
			float _MaxY = 800.0;
			float _Alpha = 1.0;
			sampler2D _MainTex;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.color = v.color;
				float y = v.vertex.y;
				y = clampValue(y, _MinY, _MaxY);
				y = inverseLerp(y, _MinY, _MaxY);
				// 根据Y坐标计算颜色值
				y = clampValue(y, 0.0, 1.0);
				o.vertexcolor.rgb = HSLToRGB(float3(0.5 - y * 0.5, 1.0, 0.5));
				o.vertexcolor.a = _Alpha;
				return o;
			}
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = i.vertexcolor;
				return col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
