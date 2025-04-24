sampler2D input : register(s0);

float intensity; // 0.0 = full color, 1.0 = full grayscale

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(input, uv);

    // Compute grayscale using luminance approximation
    float gray = dot(color.rgb, float3(0.299, 0.587, 0.114));

    // Linearly interpolate between original and grayscale
    float3 result = lerp(color.rgb, float3(gray, gray, gray), intensity);

    return float4(result, color.a);
}