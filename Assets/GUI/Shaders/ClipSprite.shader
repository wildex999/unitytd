Shader "Custom/ClipSprite" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_clipRect ("Clipping Rectangle", Vector) = (0, 0, 1, 1)
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader {
		Pass {
			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" } 
			Lighting Off Cull Off ZWrite Off Fog { Mode Off } 
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members worldPos)
#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag


			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _clipRect;
			float4 _Color;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.texcoord;
				o.worldPos = mul(_Object2World, v.vertex).xyz;
				return o;
			}

			float4 frag(v2f i) : COLOR 
			{
				if(i.worldPos.x > _clipRect.x && i.worldPos.y > _clipRect.y && i.worldPos.x < _clipRect.x + _clipRect.z && i.worldPos.y < _clipRect.y + _clipRect.w)
				{
					float4 newOut = tex2D(_MainTex, i.uv) * _Color;
					return newOut;
				}
				else
				{
					return float4(0, 0, 0, 0);
				}
			}

			ENDCG
		} 
	}
}
