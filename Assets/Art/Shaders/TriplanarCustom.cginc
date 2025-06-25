void MultitextureTriplanar_float(UnityTexture2D TextureA, UnityTexture2D TextureB, UnityTexture2D TextureC, UnitySamplerState Sampler, float3 Position, float3 Normal, float Tile, float Blend, out float4 Out){
    float3 Node_UV = Position * Tile;
    float3 Node_Blend = pow(abs(Normal), Blend);
    Node_Blend /= dot(Node_Blend, 1.0);
    float4 Node_X = SAMPLE_TEXTURE2D(TextureA, Sampler, Node_UV.zy);
    float4 Node_Y = SAMPLE_TEXTURE2D(TextureB, Sampler, Node_UV.xz);
    float4 Node_Z = SAMPLE_TEXTURE2D(TextureC, Sampler, Node_UV.xy);
    Out = Node_X * Node_Blend.x + Node_Y * Node_Blend.y + Node_Z * Node_Blend.z;
}