// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

    Shader "Hidden/TerrainEngine/TerrainLayerUtils" {

    Properties { _MainTex ("Texture", any) = "" {} }

    SubShader {

        ZTest Always
        Cull Off
        ZWrite Off

        CGINCLUDE

            #include "UnityCG.cginc"

            float4 _LayerMask;
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;      // 1/width, 1/height, width, height

        ENDCG

        Pass    // Select one channel and copy it into R channel
        {
            Name "Get Terrain Layer Channel"

            BlendOp Max

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment GetLayer

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }
            float4 GetLayer(v2f i) : SV_Target
            {
                float4 layerWeights = tex2D(_MainTex, i.texcoord);
                return dot(layerWeights, _LayerMask);
            }
            ENDCG
        }

        Pass    // Copy the R channel of the input into a specific channel in the output
        {
            Name "Set Terrain Layer Channel"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment SetLayer

            sampler2D _AlphaMapTexture;
            sampler2D _OldAlphaMapTexture;

            sampler2D _OriginalTargetAlphaMap;
            float4 _OriginalTargetAlphaMask;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
            };


            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.texcoord2 = v.texcoord2;
                return o;
            }

            float4 SetLayer(v2f i) : SV_Target
            {
                // alpha map we are modifying -- _LayerMask tells us which channel is the target (set to 1.0), non-targets are 0.0
                // Note: all four channels can be non-targets, as the target may be in a different alpha map texture
                float4 alphaMap = tex2D(_AlphaMapTexture, i.texcoord2);

                // old alpha of the target channel (according to the current terrain tile)
                float4 origTargetAlphaMapSample = tex2D(_OriginalTargetAlphaMap, i.texcoord2);
                float origTargetAlpha = dot(origTargetAlphaMapSample, _OriginalTargetAlphaMask);

                // new alpha of the target channel (according to PaintContext destRenderTexture)
                float newAlpha = tex2D(_MainTex, i.texcoord).r;

                // not allowed to 'erase' a target channel (cannot reduce it's weight)
                // this is a requirement to work around edge sync bugs
                newAlpha = max(newAlpha, origTargetAlpha);

                float oldAlphaOthers = 1 - origTargetAlpha;
                if (oldAlphaOthers > 0.001f)
                {
                    float4 othersNormalized = alphaMap * (1 - _LayerMask) * (1.0f - newAlpha) / oldAlphaOthers;
                    return othersNormalized + _LayerMask * newAlpha;
                }
                return _LayerMask;
            }
            ENDCG
        }

    }
    Fallback Off
}
