void FloorBlend_float(float3 Normal, float Blend, out float Out)
{
    if (Normal.y < 0)
    {
        Out = 0;
        return;
    }
    
    float3 Node_Blend = pow(abs(Normal), Blend);
    Node_Blend /= dot(Node_Blend, 1.0);
    Out = Node_Blend.y;
}