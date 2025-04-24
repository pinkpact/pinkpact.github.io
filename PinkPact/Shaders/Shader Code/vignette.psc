sampler2D input : register(s0);
sampler2D vignetteBrush : register(s1);

float Intensity : register(c0); // Controls vignette spread (not brightness)

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float2 center = float2(0.5, 0.5);
    float2 offset = uv - center;

    // --------------------------
    // 1. Rounded (corner) vignette
    // --------------------------
    float cornerDist = length(offset);
    float cornerFadeStart = 0.42 - Intensity * 0.13;
    float cornerFadeEnd = 0.75;
    float cornerVignette = smoothstep(cornerFadeStart, cornerFadeEnd, cornerDist);

    // De-emphasize corner effect slightly
    cornerVignette *= 0.3;

    // --------------------------
    // 2. Edge-based vignette (horizontal & vertical)
    // --------------------------
    float horizontalDist = abs(offset.x);
    float verticalDist = abs(offset.y);

    float edgeFadeStart = 0.5 - Intensity * 0.3;
    float edgeFadeEnd = 0.7;

    float horizontalVignette = smoothstep(edgeFadeStart, edgeFadeEnd, horizontalDist);
    float verticalVignette = smoothstep(edgeFadeStart, edgeFadeEnd, verticalDist);

    float edgeVignette = max(horizontalVignette, verticalVignette);

    // Slightly boost edge impact
    edgeVignette *= 1.25;

    // --------------------------
    // 3. Dampening the edge/corner intersection
    // --------------------------
    // Calculate the intersection zone where both edge and corner meet
    float intersectionDamp = smoothstep(0.3, 0.4, cornerDist);
    edgeVignette *= 1.0 - intersectionDamp * 0.5; // reduce edge vignette in intersection zone

    // --------------------------
    // 4. Adjust edge vignette based on intensity
    // --------------------------
    // Decrease the edge effect as intensity grows past 0.9
    float edgeDampFactor = saturate(1.0 - (Intensity - 0.89) * 10.0); // increases dampening as intensity > 0.9
    edgeVignette *= edgeDampFactor;

    // --------------------------
    // 5. Combine both
    // --------------------------
    float combinedVignette = max(cornerVignette, edgeVignette * (1.0 - cornerVignette * 0.8));

    float blendAmount = combinedVignette * (0.3 + Intensity * 0.7);

    // --------------------------
    // 6. Sample and blend
    // --------------------------
    float4 baseColor = tex2D(input, uv);
    float4 glowColor = tex2D(vignetteBrush, uv);

    if (glowColor.a < 0.01)
        glowColor = float4(1, 1, 1, 1); // fallback white

    float4 result = lerp(baseColor, glowColor, blendAmount);
    result.a = baseColor.a;

    return result;
}