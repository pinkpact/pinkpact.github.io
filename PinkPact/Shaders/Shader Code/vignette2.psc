sampler2D input : register(s0);

float4 vignetteColor;   // Vignette brush color (can be anything)
float intensity;        // Intensity of the vignette (0 = none, higher = stronger)

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float2 iResolution = float2(1920, 1080);

    float2 fragCoord = uv * iResolution.xy;
    float2 coord = fragCoord / iResolution.xy;

    // Base vignette factor
    coord *= 1.0 - coord.yx;
    float vig = coord.x * coord.y * 15.0;
    vig = pow(abs(vig), 0.25);

    // Adjust vignette with intensity
    vig = lerp(0.0, 1.0, pow(vig, intensity));

    // Sample input texture
    float4 texColor = tex2D(input, uv);

    // Blend vignette color with texture color based on the adjusted vig
    float3 blendedColor = lerp(vignetteColor.rgb, texColor.rgb, vig);

    return float4(blendedColor, texColor.a);
}