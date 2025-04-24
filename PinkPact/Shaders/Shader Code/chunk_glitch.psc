sampler2D input : register(s0);

float intensity; // 0 to 1
float seed;

float rand(float2 co)
{
    return frac(sin(dot(co.xy, float2(12.9898,78.233)) + seed) * 43758.5453);
}

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float2 coord = uv;

    // Create grid coordinates for consistency
    float2 baseCoord = floor(uv * float2(5, 40)); // few strips vertically

    // Determine ultra-wide block size
    float blockWidth = lerp(0.8, 1.0, rand(baseCoord + 0.1)); // 60% – 100% of width
    float blockHeight = lerp(0.02, 0.25, rand(baseCoord + 0.2)); // still thin vertically

    float2 cellSize = float2(blockWidth, blockHeight);
    float2 blockCoord = floor(uv / cellSize);

    // Use seed for glitch chance
    float glitchChance = rand(blockCoord + seed * 0.3);

    if (glitchChance < intensity * 1.5)
    {
        float offset = (rand(blockCoord + 0.7) - 0.5) * 0.4; // ±40% horizontal shift
        coord.x += offset;
        coord.x = clamp(coord.x, 0.0, 1.0);
    }

    return tex2D(input, coord);
}