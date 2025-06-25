inline float3 NormalStrength(float3 In, float Strength)
{
    //return (In.rg * Strength, lerp(1, In.b, saturate(Strength)));
    //return float3(In.rg * Strength, 1);
    //return In * Strength;
    return float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
}

inline float3 ComputeTriplanarUV(float3 Position, float Tile)
{
    return Position * Tile;
}

/*
void MultitextureTriplanarNormal_float(UnityTexture2D TextureA, UnityTexture2D TextureB, UnityTexture2D TextureC, 
    UnitySamplerState Sampler, 
    float3 Position, float3 Normal, float3 Tangent, float3 BiTangent, float Tile, float Blend, float NormalWallScale, 
    out float4 Out)
{
    float3 Node_UV = Position * Tile; 
    float3 Node_Blend = max(pow(abs(Normal), Blend), 0);
    Node_Blend /= (Node_Blend.x + Node_Blend.y + Node_Blend.z).xxx;
    float3 Node_X = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D(TextureA, Sampler, Node_UV.zy));
    float3 Node_Y = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D(TextureB, Sampler, Node_UV.xz));
    float3 Node_Z = UnpackNormalmapRGorAG(SAMPLE_TEXTURE2D(TextureC, Sampler, Node_UV.xy));

    //Node_X = Node_X * NormalWallScale;
    //Node_Z = Node_Z * NormalWallScale;

    Node_X = NormalStrength(Node_X, NormalWallScale);
    Node_Z = NormalStrength(Node_Z, NormalWallScale);

    Node_X = float3(Node_X.xy + Normal.zy, abs(Node_X.z) * Normal.x);
    Node_Y = float3(Node_Y.xy + Normal.xz, abs(Node_Y.z) * Normal.y);
    Node_Z = float3(Node_Z.xy + Normal.xy, abs(Node_Z.z) * Normal.z);
    Out = float4(normalize(Node_X.zyx * Node_Blend.x + Node_Y.xzy * Node_Blend.y + Node_Z.xyz * Node_Blend.z), 1);
    //float3x3 Node_Transform = float3x3(IN.WorldSpaceTangent, IN.WorldSpaceBiTangent, IN.WorldSpaceNormal);
    float3x3 Node_Transform = float3x3(Tangent, BiTangent, Normal);
    Out.rgb = TransformWorldToTangent(Out.rgb, Node_Transform);

    /*float3 Node_UV = Position * Tile;
    float3 Node_Blend = pow(abs(Normal), Blend);
    Node_Blend /= dot(Node_Blend, 1.0);
    float4 Node_X = SAMPLE_TEXTURE2D(TextureA, Sampler, Node_UV.zy);
    float4 Node_Y = SAMPLE_TEXTURE2D(TextureB, Sampler, Node_UV.xz);
    float4 Node_Z = SAMPLE_TEXTURE2D(TextureC, Sampler, Node_UV.xy);
    Out = Node_X * Node_Blend.x + Node_Y * Node_Blend.y + Node_Z * Node_Blend.z;
}
*/