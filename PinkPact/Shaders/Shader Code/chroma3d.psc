sampler2D input : register(s0);

float intensity;      // user-provided intensity

float4 main(float2 uv : TEXCOORD) : COLOR
{
    // Distance to the closest edge of the element (in UV space)
    float2 toEdge = min(uv, 1.0 - uv);
    float edgeFactor = 1.0 - min(toEdge.x, toEdge.y) * 2.0; // 0 at center, 1 at edges

    // Displacement amount
    float2 offset = float2(1.0 / 512, 1.0 / 512) * intensity * edgeFactor;

    // Sample the color with red shifted left, blue shifted right
    float4 col;
    col.r = tex2D(input, uv - offset).r;
    col.g = tex2D(input, uv).g;
    col.b = tex2D(input, uv + offset).b;
    col.a = tex2D(input, uv).a;

    return col;
}