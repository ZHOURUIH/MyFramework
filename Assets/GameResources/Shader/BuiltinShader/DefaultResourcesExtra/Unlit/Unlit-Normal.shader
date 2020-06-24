// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "Unlit/Texture" {
Properties
{
    _MainTex("Base (RGB)", 2D) = "white" {}
	_Grey("Grey", int) = 0
	_GreyValue("GreyValue", Range(0, 2)) = 1
	_LumOffset("LumOffset", Range(0, 20)) = 0
}

SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			int _Grey;
			float _GreyValue;
			float _LumOffset;

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord);
                UNITY_APPLY_FOG(i.fogCoord, col);
                UNITY_OPAQUE_ALPHA(col.a);
				
				float greyColor = (col.r + col.g + col.b) * 0.333;
				col.rgb = lerp(col.rgb, greyColor, _Grey) * _GreyValue;
				float lumOffset = _LumOffset + 1;
				if (_Grey > 0)
				{
					lumOffset = 1;
				}
				col.r = saturate(col.r * lumOffset);
				col.g = saturate(col.g * lumOffset);
				col.b = saturate(col.b * lumOffset);
                return col;
            }
        ENDCG
    }
}

}
