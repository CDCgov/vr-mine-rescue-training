#include "TriplanarUtil.cginc"

void TriplanarMaskMap_float(
    UnityTexture2D WallDiffuse, UnityTexture2D WallNormal, UnityTexture2D WallMaskMap,
    /*UnityTexture2D RoofDiffuse, UnityTexture2D RoofNormal,*/
    UnityTexture2D FloorDiffuse, UnityTexture2D FloorNormal, UnityTexture2D FloorMaskMap,
    UnitySamplerState Sampler, 
    float3 Position, float3 Normal, 
    float3 Tangent, float3 BiTangent, 
    float WallTile, float FloorTile,
    float Blend, 
    float NormalWallScale, float NormalFloorScale,
    float4 MaskWallScale, float4 MaskFloorScale,
    out float3 DiffuseOut, out float3 NormalOut, out float4 MaskOut)
{
    float3 uvWall = ComputeTriplanarUV(Position, WallTile);       

    float3 blend = max(pow(abs(Normal), Blend), 0);
    blend /= (blend.x + blend.y + blend.z).xxx;

    //float roofOrFloor = dot(Normal, float3(0, 1, 0));

    float3 normX = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D(WallNormal, Sampler, uvWall.zy));    
    float3 normZ = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D(WallNormal, Sampler, uvWall.xy));

    float3 diffX = SAMPLE_TEXTURE2D(WallDiffuse, Sampler, uvWall.zy);    
    float3 diffZ = SAMPLE_TEXTURE2D(WallDiffuse, Sampler, uvWall.xy);
    
    float4 maskX = SAMPLE_TEXTURE2D(WallMaskMap, Sampler, uvWall.zy);
    float4 maskZ = SAMPLE_TEXTURE2D(WallMaskMap, Sampler, uvWall.xy);

    float3 normY;
    float3 diffY;

    //if (roofOrFloor < 0)
    //{
    //    float3 uvRoof = ComputeTriplanarUV(Position, RoofTile);
    //    normY = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D(RoofNormal, Sampler, uvRoof.xz));
    //    normY = NormalStrength(normY, NormalRoofScale);
    //    diffY = SAMPLE_TEXTURE2D(RoofDiffuse, Sampler, uvRoof.xz);
    //}
    //else
    //{
    //    float3 uvFloor = ComputeTriplanarUV(Position, FloorTile);
    //    normY = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D(FloorNormal, Sampler, uvFloor.xz));
    //    normY = NormalStrength(normY, NormalFloorScale);
    //    diffY = SAMPLE_TEXTURE2D(FloorDiffuse, Sampler, uvFloor.xz);
    //}
    
    //sample floor textures into Y
    float3 uvFloor = ComputeTriplanarUV(Position, FloorTile);
    normY = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D(FloorNormal, Sampler, uvFloor.xz));    
    diffY = SAMPLE_TEXTURE2D(FloorDiffuse, Sampler, uvFloor.xz);
    float4 maskY = SAMPLE_TEXTURE2D(FloorMaskMap, Sampler, uvFloor.xz);
     
    //Scale normal strength
    normX = NormalStrength(normX, NormalWallScale);    
    normY = NormalStrength(normY, NormalFloorScale);
    normZ = NormalStrength(normZ, NormalWallScale);
    
    ///Scale mask map
    maskX *= MaskWallScale;
    maskY *= MaskFloorScale;
    maskZ *= MaskWallScale;

    normX = float3(normX.xy + Normal.zy, abs(normX.z) * Normal.x);
    normY = float3(normY.xy + Normal.xz, abs(normY.z) * Normal.y);
    normZ = float3(normZ.xy + Normal.xy, abs(normZ.z) * Normal.z);

    NormalOut = float3(normalize(normX.zyx * blend.x + normY.xzy * blend.y + normZ.xyz * blend.z));
    float3x3 normalTransform = float3x3(Tangent, BiTangent, Normal);
    NormalOut = TransformWorldToTangent(NormalOut, normalTransform);
     
    DiffuseOut = diffX * blend.x + diffY * blend.y + diffZ * blend.z;
     
    MaskOut = maskX * blend.x + maskY * blend.y + maskZ * blend.z;

    /*float3 Node_UV = Position * Tile;
    float3 Node_Blend = pow(abs(Normal), Blend);
    Node_Blend /= dot(Node_Blend, 1.0);
    float4 Node_X = SAMPLE_TEXTURE2D(TextureA, Sampler, Node_UV.zy);
    float4 Node_Y = SAMPLE_TEXTURE2D(TextureB, Sampler, Node_UV.xz);
    float4 Node_Z = SAMPLE_TEXTURE2D(TextureC, Sampler, Node_UV.xy);
    Out = Node_X * Node_Blend.x + Node_Y * Node_Blend.y + Node_Z * Node_Blend.z;*/
}