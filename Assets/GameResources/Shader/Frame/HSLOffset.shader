// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/HSLOffset"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_HSLTex("HSL Texture", 2D) = "white" {}
		_HSLOffset("HSL Offset", Color) = (0, 0, 0, 0)
		_HasHSLTex("Has HSL Texture", int) = 0
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
					return v1 + (v2 - v1) * (0.67 - vH ) * 6.0;
				}
				else
				{
					return v1;
				}
			}

			// rgb转换为色相(H),饱和度(S),亮度(L)
			// HSL和RGB的范围都是0-1
			float3 RGBtoHSL(float3 rgb)
			{
				float minRGB = min(min(rgb.r, rgb.g), rgb.b);
				float maxRGB = max(max(rgb.r, rgb.g), rgb.b);
				float delta = maxRGB - minRGB;

				float H = 0.0;
				float S = 0.0;
				float L = ( maxRGB + minRGB ) * 0.5;
				// 如果三个分量的最大和最小相等,则说明该颜色是灰色的,灰色的色相和饱和度都为0
				if(delta > 0.0)								//Chromatic data...
				{
					if (L < 0.5) 
					{
						S = delta / (maxRGB + minRGB);
					}
					else           
					{
						S = delta / (2.0 - maxRGB - minRGB);
					}

					float inverseDelta = 1.0 / delta;
					float halfDelta = delta * 0.5;
					float delR = ((maxRGB - rgb.r) * 0.167 + halfDelta) * inverseDelta;
					float delG = ((maxRGB - rgb.g) * 0.167 + halfDelta) * inverseDelta;
					float delB = ((maxRGB - rgb.b) * 0.167 + halfDelta) * inverseDelta;

					if (rgb.r == maxRGB) 
					{
						H = delB - delG;
					}
					else if (rgb.g == maxRGB) 
					{
						H = 0.33 + delR - delB;
					}
					else if (rgb.b == maxRGB) 
					{
						H = 0.67 + delG - delR;
					}

					if (H < 0.0)
					{
						H += 1.0;
					}
					else if (H > 1.0) 
					{
						H -= 1.0;
					}
				}
				return float3(H, S, L);
			}

			// 色相(H),饱和度(S),亮度(L),转换为rgb
			// HSL和RGB的范围都是0-1
			float3 HSLtoRGB(float3 hsl)
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

			sampler2D _MainTex;
			sampler2D _HSLTex;
			float4 _HSLOffset;
			int _HasHSLTex;

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
				float3 hsl;
				if (_HasHSLTex == 0)
				{
					// 转换到HSL颜色空间,再做HSL计算偏移
					hsl = RGBtoHSL(float3(col.r, col.g, col.b));
				}
				else
				{
					// 从HSL纹理中采样对应像素的HSL值
					hsl = tex2D(_HSLTex, i.texcoord).rgb;
				}
				hsl += float3(_HSLOffset.r, _HSLOffset.g, _HSLOffset.b);
				// 转回RGB空间,并转换到0-1之间
				float3 rgb = HSLtoRGB(hsl);

				// 颜色值限定在0-1之间
				col.r = clamp(rgb.r, 0.0, 1.0);
				col.g = clamp(rgb.g, 0.0, 1.0);
				col.b = clamp(rgb.b, 0.0, 1.0);
				col.a *= i.color.a;
				return col;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
