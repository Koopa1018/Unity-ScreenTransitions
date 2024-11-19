Shader "Transition/Transition Transparent"
{
	Properties
	{
		_Color ("Color", Color) = (0,0,0)
		_Alpha ("Visibility", Range(0,1) ) = 1
	}
	// SubShader
	// {
	// 	Tags { "RenderType"="Opaque" "Queue" = "Overlay+1000"}
	// 	LOD 100

	// 	Cull Off
	// 	Lighting Off
	// 	ZWrite On
	// 	ZTest Always
	// 	Blend SrcAlpha OneMinusSrcAlpha

	// 	Pass
	// 	{
	// 		CGPROGRAM
	// 		#pragma vertex vert
	// 		#pragma fragment frag
	// 		#pragma target 2.0
			
	// 		#include "UnityCG.cginc"

	// 		struct appdata
	// 		{
	// 			float4 vertex : POSITION;
	// 			//float2 uv : TEXCOORD0;
	// 		};

	// 		struct v2f
	// 		{
	// 			//float2 uv : TEXCOORD0;
	// 			float4 vertex : SV_POSITION;
	// 		};

	// 		fixed4 _Color;
	// 		fixed _Alpha;
			
	// 		v2f vert (appdata v)
	// 		{
	// 			v2f o;
	// 			o.vertex = UnityObjectToClipPos(v.vertex);
	// 			//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
	// 			//UNITY_TRANSFER_FOG(o,o.vertex);
	// 			return o;
	// 		}
			
	// 		fixed4 frag (v2f i) : SV_Target
	// 		{
	// 			fixed4 c = _Color;
	// 			c.a = _Alpha;
	// 			return c;
	// 		}
	// 		ENDCG
	// 	}
	// }
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Overlay+1000"}

		Cull Off
		Lighting Off
		ZWrite On
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				//float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				//float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			fixed4 _Color;
			fixed _Alpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(_Color.r, _Color.g, _Color.b, _Alpha);
			}
			ENDCG
		}
	}
}
