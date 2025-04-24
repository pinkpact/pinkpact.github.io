sampler2D input : register(s0);

float intensity;    // pixel size multiplier

float4 main(float2 uv : TEXCOORD) : COLOR
{
    float i = intensity * 10,
          single_size = (1.0 / 512) * max(i, 0.000001);

    float2 blockSize = float2(single_size / 2, single_size),
           blockCoord = floor(uv / blockSize) * blockSize; 

    return tex2D(input, blockCoord);
}