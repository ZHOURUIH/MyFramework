Shader "FastGUI/FastSpriteGPUDriven"
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
            sampler2D _FastSpriteTex0;
            sampler2D _FastSpriteTex1;
            sampler2D _FastSpriteTex2;
            sampler2D _FastSpriteTex3;
            sampler2D _FastSpriteTex4;
            sampler2D _FastSpriteTex5;
            sampler2D _FastSpriteTex6;
            sampler2D _FastSpriteTex7;
            fixed4 _Color;

            struct FastSpriteGPUDrivenVertex
            {
                float3 position;
                float2 uv;
            };

            struct FastSpriteGPUDrivenQuadAsset
            {
                float4 position01;
                float4 position23;
                float4 uv01;
                float4 uv23;
                uint packedIndices;
                uint padding0;
                uint padding1;
                uint padding2;
            };

            StructuredBuffer<FastSpriteGPUDrivenVertex> _FastSpriteGeometry;
            StructuredBuffer<FastSpriteGPUDrivenQuadAsset> _FastSpriteQuadAssets;
            StructuredBuffer<float4x4> _FastSpriteLocalMatrices;
            StructuredBuffer<float4> _FastSpriteColors;
            StructuredBuffer<int> _FastSpriteGeometryOffsets;
            StructuredBuffer<int> _FastSpriteRootIndices;
            StructuredBuffer<int> _FastSpriteFlags;
            StructuredBuffer<int> _FastSpriteDrawOrder;
            int _FastSpriteDrawOrderBase;
            StructuredBuffer<int> _FastSpriteIndirectOrderBases;
            int _FastSpriteIndirectCommandIndex;
            int _FastSpriteUseIndirectOrderBase;
            StructuredBuffer<float4x4> _FastSpriteRootMatrices;
            int _FastSpriteRootMatrixCount;

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float textureSlot : TEXCOORD1;
            };

            float2 selectPacked2(float4 pair01, float4 pair23, uint index)
            {
                if (index == 0u) return pair01.xy;
                if (index == 1u) return pair01.zw;
                if (index == 2u) return pair23.xy;
                return pair23.zw;
            }

            fixed4 sampleSpriteTexture(float2 uv, int textureSlot)
            {
                // Direct/fallback DrawProcedural keeps the original per-command _MainTex path.
                // Only the persistent indirect pool needs per-instance virtual texture selection,
                // so ordinary plans pay no eight-way sampler dispatch overhead.
                if (_FastSpriteUseIndirectOrderBase == 0) return tex2D(_MainTex, uv);
                if (textureSlot == 0) return tex2D(_FastSpriteTex0, uv);
                if (textureSlot == 1) return tex2D(_FastSpriteTex1, uv);
                if (textureSlot == 2) return tex2D(_FastSpriteTex2, uv);
                if (textureSlot == 3) return tex2D(_FastSpriteTex3, uv);
                if (textureSlot == 4) return tex2D(_FastSpriteTex4, uv);
                if (textureSlot == 5) return tex2D(_FastSpriteTex5, uv);
                if (textureSlot == 6) return tex2D(_FastSpriteTex6, uv);
                if (textureSlot == 7) return tex2D(_FastSpriteTex7, uv);
                return tex2D(_MainTex, uv);
            }

            v2f vert(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                v2f OUT;
                int orderBase = _FastSpriteUseIndirectOrderBase != 0
                    ? _FastSpriteIndirectOrderBases[_FastSpriteIndirectCommandIndex]
                    : _FastSpriteDrawOrderBase;
                int slot = _FastSpriteDrawOrder[orderBase + (int)instanceID];
                float4x4 localMatrix = _FastSpriteLocalMatrices[slot];
                float4 instanceColor = _FastSpriteColors[slot];
                int geometryOffset = _FastSpriteGeometryOffsets[slot];
                int rootIndex = _FastSpriteRootIndices[slot];
                int flags = _FastSpriteFlags[slot];
                // Bit 2 is the persistent-plan visibility flag.
                if ((flags & 4) == 0)
                {
                    OUT.vertex = float4(2.0, 2.0, 2.0, 1.0);
                    OUT.color = 0;
                    OUT.texcoord = 0;
                    OUT.textureSlot = 15.0;
                    return OUT;
                }
                float3 localPosition;
                float2 localUV;

                if (geometryOffset <= -2)
                {
                    int quadIndex = -geometryOffset - 2;
                    FastSpriteGPUDrivenQuadAsset quad = _FastSpriteQuadAssets[quadIndex];
                    uint lookupVertex = vertexID;
                    int flipMask = flags & 3;
                    bool reverseWinding = flipMask == 1 || flipMask == 2;
                    if (reverseWinding)
                    {
                        uint lane = vertexID % 3u;
                        uint triBase = vertexID - lane;
                        if (lane == 1u) lookupVertex = triBase + 2u;
                        else if (lane == 2u) lookupVertex = triBase + 1u;
                    }
                    uint corner = (quad.packedIndices >> (lookupVertex * 2u)) & 3u;
                    float2 p = selectPacked2(quad.position01, quad.position23, corner);
                    if ((flags & 1) != 0) p.x = -p.x;
                    if ((flags & 2) != 0) p.y = -p.y;
                    localPosition = float3(p, 0.0);
                    localUV = selectPacked2(quad.uv01, quad.uv23, corner);
                }
                else
                {
                    FastSpriteGPUDrivenVertex source = _FastSpriteGeometry[geometryOffset + (int)vertexID];
                    localPosition = source.position;
                    localUV = source.uv;
                }

                float4 worldPos = mul(localMatrix, float4(localPosition, 1.0));
                if (rootIndex >= 0 && rootIndex < _FastSpriteRootMatrixCount)
                {
                    worldPos = mul(_FastSpriteRootMatrices[rootIndex], worldPos);
                }
                OUT.vertex = mul(UNITY_MATRIX_VP, worldPos);
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                OUT.texcoord = localUV;
                OUT.textureSlot = (float)((flags >> 8) & 15);
                OUT.color = instanceColor * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                int textureSlot = (int)(IN.textureSlot + 0.5);
                fixed4 c = sampleSpriteTexture(IN.texcoord, textureSlot) * IN.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
