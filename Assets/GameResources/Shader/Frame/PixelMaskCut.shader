// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/PixelMaskCut"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MaskTex ("Texture", 2D) = "white" {}
		_SizeX("Size X", float) = 0.0
		_SizeY("Size Y", float) = 0.0
		_PosX("Pos X", float) = 0.0
		_PosY("Pos Y", float) = 0.0
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
			uniform half4 _MainTex_TexelSize;	// 主纹理的像素尺寸
			uniform half4 _MaskTex_TexelSize;	// 遮挡纹理的像素尺寸
			float _SizeX;
			float _SizeY;
			float _PosX;
			float _PosY;

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
				// _PosX是MaskTex中心点相对于MainTex中心的像素坐标
				fixed4 srcColor = tex2D(_MainTex, i.uv);
				// 计算对应的遮挡纹理坐标
				float2 curPixelPos = i.uv * _MainTex_TexelSize.zw;
				float2 relative = _MainTex_TexelSize.zw / 2 - float2(_SizeX, _SizeY) / 2;
				float2 maskPixelPos = curPixelPos - (relative + float2(_PosX, _PosY));
				float2 newUV = maskPixelPos * _MaskTex_TexelSize.xy;
				// 当遮挡纹理坐标在0~1之间时才会取红色分量作为透明度,否则透明度为0
				float alpha = 0;
				if (newUV.x >= 0 && newUV.x <= 1.0 && newUV.y >= 0 && newUV.y <= 1.0)
				{
					alpha = tex2D(_MaskTex, newUV).r;
				}
				return fixed4(srcColor.rgb, alpha * i.color.a * srcColor.a);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
