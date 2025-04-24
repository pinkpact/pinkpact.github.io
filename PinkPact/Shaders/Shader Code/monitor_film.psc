sampler2D input : register(s0);

float2 resolution;
float intensity;    // Controls strength of the pixel grid + brightness (0–1)
float distortionStrength;

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float distortion = distortionStrength * 0.01;

    // Step 1: Center coordinates for distortion
    float2 centeredUV = uv - 0.5;
    float radius = length(centeredUV);

    // Step 2: Apply barrel distortion near edges
    float2 distortedUV = centeredUV * (1.0 + distortion * radius * radius);
    distortedUV += 0.5;

    // Step 3: Clamp to keep UVs in bounds
    distortedUV = clamp(distortedUV, 0.0, 1.0);

    // Step 4: Generate a pixel grid pattern like viewing a monitor through a phone
    float2 pixelGrid = sin(distortedUV * float2(resolution.x, resolution.y) * 3.14159);

    // Step 5: Sharpen and scale brightness effect for visibility on small visuals
    float brightnessMod = (pixelGrid.x + pixelGrid.y) * intensity;

    // Step 6: Sample color and apply brightness mod
    float4 color = tex2D(input, distortedUV);
    color.rgb *= (1.0 + brightnessMod); // brighten pixels on "grid lines"

    return color;
}