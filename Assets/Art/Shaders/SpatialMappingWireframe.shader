// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

//Shader "VR/SpatialMapping/Wireframe"
Shader "Custom/Wireframe"
{
    Properties
    {
        _WireThickness ("Wire Thickness", RANGE(0, 1800)) = 900
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _FadeDistanceStart ("Fade Distance Start", RANGE(0, 250)) = 10
        _FadeDistance ("Fade Distance", RANGE(0, 100)) = 25
        _AreaFadeStart ("AreaFadeStart", RANGE(0, 2)) = 0.0005
        _AreaFade ("AreaFade", RANGE(0,0.5)) = 0.1
    }

    SubShader
    {
        // Each color represents a meter.

        Tags { "RenderType"="Opaque"  "Queue" = "Transparent-1" }

        Pass
        {
            // Wireframe shader based on the the following
            // http://developer.download.nvidia.com/SDK/10/direct3d/Source/SolidWireframe/Doc/SolidWireframe.pdf

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _WireThickness;
            fixed4 _LineColor;
            float _FadeDistanceStart;
            float _FadeDistance;
            float _AreaFadeStart;
            float _AreaFade;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2g
            {
                float4 projectionSpaceVertex : SV_POSITION;
                float4 worldSpacePosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO_EYE_INDEX
            };

            struct g2f
            {
                float4 projectionSpaceVertex : SV_POSITION;
                float4 worldSpacePosition : TEXCOORD0;
                float4 dist : TEXCOORD1;
                float fade : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2g vert (appdata v)
            {
                v2g o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT_STEREO_EYE_INDEX(o);

                o.projectionSpaceVertex = UnityObjectToClipPos(v.vertex);
                o.worldSpacePosition = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i[0]);

                float2 p0 = i[0].projectionSpaceVertex.xy / i[0].projectionSpaceVertex.w;
                float2 p1 = i[1].projectionSpaceVertex.xy / i[1].projectionSpaceVertex.w;
                float2 p2 = i[2].projectionSpaceVertex.xy / i[2].projectionSpaceVertex.w;

                float2 edge0 = p2 - p1; 
                float2 edge1 = p2 - p0;
                float2 edge2 = p1 - p0;

                // To find the distance to the opposite edge, we take the
                // formula for finding the area of a triangle Area = Base/2 * Height,
                // and solve for the Height = (Area * 2)/Base.
                // We can get the area of a triangle by taking its cross product
                // divided by 2.  However we can avoid dividing our area/base by 2
                // since our cross product will already be double our area.
                float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);

                //float thicknessMult = saturate(area * 600.0);
                //float wireThickness = 1800 - (_WireThickness * thicknessMult);

                float wireThickness = 1800 - _WireThickness;

                // if (area < 0.0005)
                //    return;

                g2f o;
                o.fade = saturate((area - (_AreaFadeStart/1000.0)) / (_AreaFade/1000.0));

                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldSpacePosition = i[0].worldSpacePosition;
                o.projectionSpaceVertex = i[0].projectionSpaceVertex;
                o.dist.xyz = float3( (area / length(edge0)), 0.0, 0.0) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                triangleStream.Append(o);

                o.worldSpacePosition = i[1].worldSpacePosition;
                o.projectionSpaceVertex = i[1].projectionSpaceVertex;
                o.dist.xyz = float3(0.0, (area / length(edge1)), 0.0) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                triangleStream.Append(o);

                o.worldSpacePosition = i[2].worldSpacePosition;
                o.projectionSpaceVertex = i[2].projectionSpaceVertex;
                o.dist.xyz = float3(0.0, 0.0, (area / length(edge2))) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                triangleStream.Append(o);
            }

            fixed4 frag (g2f i) : SV_Target
            {
                float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];
                //minDistanceToEdge = i.dist[0];

                // Early out if we know we are not on a line segment.
                if(minDistanceToEdge > 0.9)
                {
                    return fixed4(0,0,0,0);
                }

                // Smooth our line out
                float t = exp2(-2 * minDistanceToEdge * minDistanceToEdge);
                //float t = minDistanceToEdge * 16;

                // const fixed4 colors[11] = {
                //         fixed4(1.0, 1.0, 1.0, 1.0),  // White
                //         fixed4(1.0, 0.0, 0.0, 1.0),  // Red
                //         fixed4(0.0, 1.0, 0.0, 1.0),  // Green
                //         fixed4(0.0, 0.0, 1.0, 1.0),  // Blue
                //         fixed4(1.0, 1.0, 0.0, 1.0),  // Yellow
                //         fixed4(0.0, 1.0, 1.0, 1.0),  // Cyan/Aqua
                //         fixed4(1.0, 0.0, 1.0, 1.0),  // Magenta
                //         fixed4(0.5, 0.0, 0.0, 1.0),  // Maroon
                //         fixed4(0.0, 0.5, 0.5, 1.0),  // Teal
                //         fixed4(1.0, 0.65, 0.0, 1.0), // Orange
                //         fixed4(1.0, 1.0, 1.0, 1.0)   // White
                //     };

                float cameraToVertexDistance = length(_WorldSpaceCameraPos - i.worldSpacePosition);
                float fade = (cameraToVertexDistance - _FadeDistanceStart) / _FadeDistance;
                fade = (1 - saturate(fade)) * (i.fade);

                //int index = clamp(floor(cameraToVertexDistance), 0, 10);
                //fixed4 wireColor = colors[0];
                //fixed4 wireColor = _LineColor;
                //fixed4 wireColor = lerp(_LineColor, float4(0,0,0,0), fade);
                fixed4 wireColor = lerp(float4(0,0,0,0), _LineColor, fade);

                fixed4 finalColor = lerp(float4(0,0,0,0), wireColor, t);
                finalColor.a = t;

                return finalColor;
            }
            ENDCG
        }
    }
}
