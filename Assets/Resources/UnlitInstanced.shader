// Upgrade NOTE: upgraded instancing buffer 'MyProperties' to new syntax.

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Instanced/UnlitInstanced"
{
	// Unlit shader. Simplest possible colored shader.
	// - no lighting
	// - no lightmap support
	// - no texture
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_Scale("Scale", Float) = 1.0
	}

	SubShader{
		//Tags{ "RenderType" = "Opaque" }
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		ZWrite On
		ZTest Less
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0


			// Enable instancing for this shader
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				UNITY_FOG_COORDS(0)
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			fixed4 _Color;
			float4x4 _TransformMat;
			float _Scale;

			UNITY_INSTANCING_BUFFER_START(MyProperties)
			//UNITY_DEFINE_INSTANCED_PROP(float4x4, _LocalTransform)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Position)
#define _Position_arr MyProperties
			UNITY_INSTANCING_BUFFER_END(MyProperties)

			v2f vert(appdata_t v)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float4x4 mvp;

				//mvp = mul(unity_ObjectToWorld, _TransformMat);
				//mvp = mul(_TransformMat, unity_ObjectToWorld);
				
				//mvp = mul(UNITY_MATRIX_VP, mvp);
				//mvp = mul(UNITY_MATRIX_VP, _TransformMat);
				//o.vertex = mul(mvp, v.vertex);

				////float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				//float4 worldPos = mul(_TransformMat, v.vertex);
				//o.vertex = mul(UNITY_MATRIX_VP, worldPos);

				//float4 localPos = mul(_TransformMat, v.vertex);
				//float4 worldPos = mul(unity_ObjectToWorld, localPos);
				//o.vertex = mul(UNITY_MATRIX_VP, worldPos);
				
				//float4x4 mat = UNITY_ACCESS_INSTANCED_PROP(_LocalTransform);
				//float4 localPos = mul(mat, v.vertex);

				float4 pos = UNITY_ACCESS_INSTANCED_PROP(_Position_arr, _Position);
				float4 vpos = v.vertex * _Scale;
				vpos.w = 1;
				float4 localPos = vpos + float4(pos.xyz, 0);

				mvp = mul(UNITY_MATRIX_VP, _TransformMat);
				o.vertex = mul(mvp, localPos);

				//o.vertex = UnityObjectToClipPos(localPos);
				
				//o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				UNITY_SETUP_INSTANCE_ID(i);

				fixed4 col = _Color;
				//UNITY_APPLY_FOG(i.fogCoord, col);
				//UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}
			ENDCG
		}
	}

}