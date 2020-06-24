// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Legacy Shaders/Diffuse" {
Properties
{
    _Color("Main Color", Color) = (1,1,1,1)
    _MainTex("Base (RGB)", 2D) = "white" {}
	_Grey("Grey", int) = 0
	_GreyValue("GreyValue", Range(0, 2)) = 1
	_LumOffset("LumOffset", Range(0, 20)) = 0
}
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 200

CGPROGRAM
#pragma surface surf Custom

sampler2D _MainTex;
fixed4 _Color;
int _Grey;
float _GreyValue;
float _LumOffset;

struct Input {
    float2 uv_MainTex;
};

inline void LightingCustom_GI(
			SurfaceOutput s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			gi = UnityGlobalIllumination(data, 1.0, s.Normal);
		}
		inline fixed4 UnityCustomLight(SurfaceOutput s, UnityLight light)
		{
			fixed diff = max(0, dot(s.Normal, light.dir));
			fixed4 c;
			c.rgb = s.Albedo * light.color * diff;
			c.a = s.Alpha;
			return c;
		}
		inline fixed4 LightingCustom(SurfaceOutput s, UnityGI gi)
		{
			fixed4 col = UnityCustomLight(s, gi.light);
#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
			col.rgb += s.Albedo * gi.indirect.diffuse;
#endif
			float greyColor = (col.r + col.g + col.b) * 0.333;
			col.rgb = lerp(col.rgb, greyColor, _Grey) * _GreyValue;
			float lumOffset = _LumOffset + 1;
			if (_Grey == 0)
			{
				col.r = saturate(col.r * lumOffset);
				col.g = saturate(col.g * lumOffset);
				col.b = saturate(col.b * lumOffset);
			}
			return col;
		}

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 col = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = col;
    o.Alpha = col.a;
}
ENDCG
}

Fallback "Legacy Shaders/VertexLit"
}
