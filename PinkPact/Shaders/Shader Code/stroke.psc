sampler2D input : register(s0);
sampler2D brushTexture : register(s1);

// Effect parameters
float OutlineThickness : register(c0);
float OpacityThreshold : register(c1);
float2 TextureDimensions : register(c2);

// Main shader function
float4 main(float2 uv : TEXCOORD) : COLOR
{
    float AlphaThreshold = OpacityThreshold >= 1.0 ? 0.99999 : OpacityThreshold;
    float4 originalColor = tex2D(input, uv);

    // If pixel is already opaque enough, return it as-is
    if (originalColor.a >= AlphaThreshold)
    {
        return originalColor;
    }

    float2 pixelSize = OutlineThickness / TextureDimensions;
    int steps = 128;
    float angleStep = 6.28318530718 / steps;

    float strokeInfluence = 0.0;

    for (int i = 0; i < steps; i++)
    {
        float angle = angleStep * i;
        float2 direction = float2(cos(angle), sin(angle));
        float2 sampleUV = uv + direction * pixelSize;
        
        // Clamp UV to avoid out-of-bounds
        sampleUV = clamp(sampleUV, 0.0, 1.0);
        
        float neighborAlpha = tex2D(input, sampleUV).a;

        // Linearly scale contribution based on opacity
        strokeInfluence += saturate((neighborAlpha - AlphaThreshold) * 50); // boost influence
    }

    // Normalize influence [0..1]
    float strokeAlpha = saturate(strokeInfluence / steps);

    // If there's enough influence, apply stroke
    if (strokeAlpha > 0.0)
    {
        float4 brushColor = tex2D(brushTexture, uv);
        
        // Fallback if brush has no opacity
        if (brushColor.a < 0.01)
            brushColor = float4(1, 1, 1, 1);

        return float4(brushColor.rgb, strokeAlpha);
    }

    return originalColor;
}