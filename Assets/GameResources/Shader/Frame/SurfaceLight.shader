// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Frame/SurfaceLight"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white"{}
		_RimColor("Rim Color",Color) = (1,1,1,1)
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
		Offset -1, -1
		Cull Off
		Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "unitylightingcommon.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
			};
			struct v2f
			{
				float4 vertex : SV_POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float3 normalDir : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};
			
			sampler2D _MainTex;
			fixed4 _RimColor;
			v2f vert(appdata i)
			{
				v2f o;
				// 转化顶点位置
				o.vertex = UnityObjectToClipPos(i.vertex);
				// 获取世界空间法线向量
				o.normalDir = mul(float4(i.normal, 0), unity_WorldToObject).xyz;
				// 获取世界坐标系顶点位置
				o.worldPos = mul(unity_ObjectToWorld, i.vertex);
				o.texcoord = i.texcoord;
				o.color = i.color;
				return o;
			}
			fixed4 frag(v2f v) : SV_TARGET
			{
				fixed4 texColor = tex2D(_MainTex, v.texcoord);
				// 法线标准化
				float3 normal = normalize(v.normalDir);
				// 视角方向标准化
				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - v.worldPos.xyz);
				// 点积,夹角越小,点积越大,范围0~1
				float NdotV = saturate(dot(normal, viewDir));
				// 漫反射加边缘色加环境光
				fixed3 diffuse = NdotV * texColor.rgb + _RimColor * (1 - NdotV) + UNITY_LIGHTMODEL_AMBIENT.rgb * 0.05;
				// 混合输出
				fixed4 final = fixed4(diffuse, texColor.a);
				return final;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
