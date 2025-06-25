Shader "Custom/TwoSidedTransparent" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	
	SubShader {
		Tags { "RenderType"="Opaque" "Queue" = "AlphaTest"}
		LOD 200
		
		
		Cull Off		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf TwoSidedLambert fullforwardshadows alpha:premul keepalpha
		//#pragma vertex vert //Add this back in to have double sided light again, but then other stuff won't work

		half4 LightingTwoSidedLambert(SurfaceOutput s, half3 lightDir, half atten)
		{
			half4 c;
			half NdotL = dot(s.Normal, lightDir);
			half NdotLBack = dot(s.Normal * -1, lightDir);
			NdotL = max(NdotL, NdotLBack);

			c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);
			c.a = s.Alpha;
			return c;
		}
		
		
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		//#pragma vertex vert

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		/*void vert(inout appdata_full v) {
			v.normal *= -1;
		}*/

		void surf(Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			// Metallic and smoothness come from slider variables
			//o.Metallic = _Metallic;
			//o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG

		//Cull Back			
		//CGPROGRAM
		//	// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard fullforwardshadows alpha:premul keepalpha	

		//// Use shader model 3.0 target, to get nicer looking lighting
		//#pragma target 3.0

		//sampler2D _MainTex;

		//struct Input {
		//	float2 uv_MainTex;
		//};

		//half _Glossiness;
		//half _Metallic;
		//fixed4 _Color;
		//

		//void surf(Input IN, inout SurfaceOutputStandard o) {
		//	// Albedo comes from a texture tinted by color
		//	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
		//	o.Albedo = c.rgb;
		//	// Metallic and smoothness come from slider variables
		//	o.Metallic = _Metallic;
		//	o.Smoothness = _Glossiness;
		//	o.Alpha = c.a;
		//}
		//ENDCG
		
	}
	FallBack "Diffuse"
}
