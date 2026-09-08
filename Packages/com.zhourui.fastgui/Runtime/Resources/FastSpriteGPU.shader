Shader "FastGUI/FastSpriteGPU"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            StructuredBuffer<float4x4> _FastSpriteRootMatrices;
            int _FastSpriteRootMatrixCount;
            struct appdata_t
            {
                float3 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float rootIndex : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                int rootIndex = (int)floor(IN.rootIndex + 0.5);
                bool gpuRoot = IN.rootIndex >= 0.0 && rootIndex >= 0 && rootIndex < _FastSpriteRootMatrixCount;

                if (gpuRoot)
                {
                    float4 worldPos = mul(_FastSpriteRootMatrices[rootIndex], float4(IN.vertex, 1.0));
                    OUT.vertex = mul(UNITY_MATRIX_VP, worldPos);

                    OUT.texcoord = IN.texcoord;
                    OUT.color = IN.color * _Color;
                }
                else
                {
                    OUT.vertex = UnityObjectToClipPos(float4(IN.vertex, 1.0));
                    OUT.texcoord = IN.texcoord;
                    OUT.color = IN.color * _Color;
                }

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
